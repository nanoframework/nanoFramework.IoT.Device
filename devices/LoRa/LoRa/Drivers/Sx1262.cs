// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using System.Device.Gpio;
using System.Device.Spi;
using System.Diagnostics;
using System.Threading;

using Iot.Device.LoRa;

namespace Iot.Device.LoRa.Drivers.Sx1262
{
    /// <summary>
    /// <para>Low-level SX1262 LoRa radio driver.</para>
    /// <para>Heltec Vision Master E213 (HT-VME213) default pin mapping: NSS = 8, SCK = 9, MOSI = 10, MISO = 11, RST = 12, BUSY = 13, DIO1 = 14.</para>
    /// <para>Supports reset, initialization, TX, RX polling, and buffer access.</para>
    /// <datasheet>https://semtech.my.salesforce.com/sfc/p/#E0000000JelG/a/RQ000008n3pp/qXjWn19TZmb.1MgqPZ8Vrc5V7U.M_lOAIoTZHcEAeTI or see datasheet folder.</datasheet>
    /// </summary>
    public class Sx1262 : ILoRaDevice
    {
        /// <summary>
        /// Maximum TX/RX payload length supported by this driver (matches the SX1262 length field in this configuration).
        /// </summary>
        public const int MaxPayloadLength = 255;

        // ---------------------------------------------------------------
        // Op-codes (datasheet section 11.1)
        // ---------------------------------------------------------------
        private const byte OpGetStatus = 0xC0;
        private const byte OpSetStandby = 0x80;
        private const byte OpSetSleep = 0x84;
        private const byte OpSetPacketType = 0x8A;
        private const byte OpSetRfFrequency = 0x86;
        private const byte OpSetTxParams = 0x8E;
        private const byte OpSetPaConfig = 0x95;
        private const byte OpSetModulationParams = 0x8B;
        private const byte OpSetPacketParams = 0x8C;
        private const byte OpSetBufferBaseAddr = 0x98;
        private const byte OpSetDioIrqParams = 0x08;
        private const byte OpGetIrqStatus = 0x12;
        private const byte OpClearIrqStatus = 0x02;
        private const byte OpSetRx = 0x82;
        private const byte OpSetTx = 0x83;
        private const byte OpWriteBuffer = 0x0E;
        private const byte OpReadBuffer = 0x1E;
        private const byte OpGetRxBufferStatus = 0x13;
        private const byte OpGetPacketStatus = 0x14;
        private const byte OpSetDio3AsTcxoCtrl = 0x97;
        private const byte OpSetDio2AsRfSwCtrl = 0x9D;
        private const byte OpSetRegulatorMode = 0x96;
        private const byte OpCalibrate = 0x89;

        // ---------------------------------------------------------------
        // IRQ bit masks (datasheet section 13.3.2)
        // ---------------------------------------------------------------
        private const ushort IrqTxDone = 0x0001;
        private const ushort IrqRxDone = 0x0002;
        private const ushort IrqCrcErr = 0x0040;
        private const ushort IrqTimeout = 0x0200;

        // ---------------------------------------------------------------
        // Hardware
        // ---------------------------------------------------------------
        private readonly SpiDevice _spi;
        private readonly GpioController _gpio;
        private readonly bool _shouldDispose;
        private readonly bool _disposeSpi;
        private readonly object _sendLock = new object();
        private readonly object _pollLock = new object();

        private GpioPin _resetPin;
        private GpioPin _busyPin;
        private GpioPin _dio1Pin;

        private bool _disposed;

        // ---------------------------------------------------------------
        // RX poll thread
        // ---------------------------------------------------------------
        private Thread _pollThread;

        // 0 = poll loop runs; 1 = stop requested. Use Interlocked for cross-thread visibility (documented on nanoFramework).
        private int _stopPolling;

        // ---------------------------------------------------------------
        // Static helpers (SA1204: public static before non-static public)
        //// ---------------------------------------------------------------

        /// <summary>
        /// Decodes chip mode bits [6:4] from a raw status byte.
        /// </summary>
        /// <param name="status">The status byte returned by the chip.</param>
        /// <returns>A short label describing the chip mode.</returns>
        public static string DecodeChipMode(byte status)
        {
            byte mode = (byte)((status >> 4) & 0x07);
            switch (mode)
            {
                case 0x02: return "STDBY_RC";
                case 0x03: return "STDBY_XOSC";
                case 0x04: return "FS";
                case 0x05: return "RX";
                case 0x06: return "TX";
                default: return "UNKNOWN";
            }
        }

        // ---------------------------------------------------------------
        // Construction
        //// ---------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="Sx1262" /> class.
        /// </summary>
        /// <param name="spiDevice">The SPI device for the radio.</param>
        /// <param name="resetPin">GPIO pin number for reset (active low).</param>
        /// <param name="busyPin">GPIO pin number for the BUSY line.</param>
        /// <param name="dio1Pin">GPIO pin number for DIO1 (IRQ).</param>
        /// <param name="gpioController">Optional shared GPIO controller; a new instance is created when null.</param>
        /// <param name="shouldDispose">True to dispose the GPIO controller when this instance is disposed.</param>
        /// <param name="disposeSpi">True to dispose <paramref name="spiDevice" /> when this instance is disposed (use false when the bus is shared).</param>
        /// <remarks>
        /// GPIO pins are opened incrementally. Temporary <see cref="GpioPin" /> references hold partially opened pins; after all opens succeed, fields are assigned and temporaries are nulled so failure cleanup only disposes pins that were actually opened.
        /// </remarks>
        public Sx1262(
            SpiDevice spiDevice,
            int resetPin,
            int busyPin,
            int dio1Pin,
            GpioController gpioController = null,
            bool shouldDispose = true,
            bool disposeSpi = false)
        {
            if (spiDevice == null)
            {
                throw new ArgumentNullException(nameof(spiDevice));
            }

            if (resetPin < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(resetPin));
            }

            if (busyPin < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(busyPin));
            }

            if (dio1Pin < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(dio1Pin));
            }

            _spi = spiDevice;
            _gpio = gpioController == null ? new GpioController() : gpioController;
            _shouldDispose = shouldDispose || gpioController == null;
            _disposeSpi = disposeSpi;

            GpioPin resetPinObj = null;
            GpioPin busyPinObj = null;
            GpioPin dio1PinObj = null;
            try
            {
                resetPinObj = _gpio.OpenPin(resetPin, PinMode.Output);
                busyPinObj = _gpio.OpenPin(busyPin, PinMode.Input);
                dio1PinObj = _gpio.OpenPin(dio1Pin, PinMode.Input);
                resetPinObj.Write(PinValue.High);
                _resetPin = resetPinObj;
                _busyPin = busyPinObj;
                _dio1Pin = dio1PinObj;
                resetPinObj = null;
                busyPinObj = null;
                dio1PinObj = null;
            }
            catch
            {
                if (dio1PinObj != null)
                {
                    dio1PinObj.Dispose();
                }

                if (busyPinObj != null)
                {
                    busyPinObj.Dispose();
                }

                if (resetPinObj != null)
                {
                    resetPinObj.Dispose();
                }

                if (_shouldDispose && _gpio != null)
                {
                    _gpio.Dispose();
                }

                throw;
            }
        }

        // ---------------------------------------------------------------
        // Events
        //// ---------------------------------------------------------------

        /// <inheritdoc/>
        public event PacketReceivedHandler PacketReceived;

        // ---------------------------------------------------------------
        // Step 1 — Reset + BUSY + GetStatus
        //// ---------------------------------------------------------------

        /// <inheritdoc/>
        public void Reset()
        {
            _resetPin.Write(PinValue.High);
            Thread.Sleep(10);
            _resetPin.Write(PinValue.Low);
            Thread.Sleep(100);
            _resetPin.Write(PinValue.High);
            Thread.Sleep(10);
            WaitBusy(5000);
        }

        /// <summary>
        /// Blocks until BUSY goes low or the timeout expires.
        /// </summary>
        /// <param name="timeoutMs">Maximum time to wait, in milliseconds.</param>
        /// <exception cref="TimeoutException">BUSY did not go low within <paramref name="timeoutMs" />.</exception>
        public void WaitBusy(int timeoutMs)
        {
            int elapsed = 0;
            while (_busyPin.Read() == PinValue.High)
            {
                Thread.Sleep(1);
                if (++elapsed >= timeoutMs)
                {
                    throw new TimeoutException();
                }
            }
        }

        /// <summary>
        /// Reads the chip status byte.
        /// </summary>
        /// <returns>The second byte of the status SPI transaction.</returns>
        public byte GetStatus()
        {
            byte[] tx = new byte[] { OpGetStatus, 0x00 };
            byte[] rx = new byte[2];
            WaitBusy(5000);
            _spi.TransferFullDuplex(tx, rx);
            return rx[1];
        }

        // ---------------------------------------------------------------
        // Step 2 — Full init sequence
        //// ---------------------------------------------------------------

        /// <inheritdoc/>
        /// <remarks>
        /// <para>Command opcodes are listed in the Semtech SX1261/2 datasheet §11.1.</para>
        /// <para>Literal parameter bytes below follow §13.4 (configuration): TCXO on DIO3, calibration, regulator, standby, packet type, PA, modulation, IRQ mapping, etc. Adjust values for your board and RF plan; this sequence matches a typical 868 MHz LoRa setup.</para>
        /// </remarks>
        public void Initialize()
        {
            WriteCommand(OpSetDio3AsTcxoCtrl, new byte[] { 0x02, 0x00, 0x01, 0x40 });
            WriteCommand(OpCalibrate, new byte[] { 0x7F });
            WaitBusy(3000);
            WriteCommand(OpSetDio2AsRfSwCtrl, new byte[] { 0x01 });
            WriteCommand(OpSetRegulatorMode, new byte[] { 0x01 });
            WriteCommand(OpSetStandby, new byte[] { 0x01 });
            WriteCommand(OpSetPacketType, new byte[] { 0x01 });
            SetRfFrequency(868000000);
            WriteCommand(OpSetPaConfig, new byte[] { 0x04, 0x07, 0x00, 0x01 });
            WriteCommand(OpSetTxParams, new byte[] { 0x0E, 0x04 });
            WriteCommand(OpSetModulationParams, new byte[] { 0x07, 0x04, 0x01, 0x00 });
            WriteCommand(OpSetPacketParams, new byte[] { 0x00, 0x08, 0x00, 0xFF, 0x01, 0x00 });
            WriteCommand(OpSetBufferBaseAddr, new byte[] { 0x00, 0x00 });
            byte[] irqParams = new byte[] { 0x02, 0x03, 0x02, 0x03, 0x00, 0x00, 0x00, 0x00 };
            WriteCommand(OpSetDioIrqParams, irqParams);
        }

        /// <inheritdoc/>
        public void SetRfFrequency(uint frequencyHz)
        {
            ulong frf = ((ulong)frequencyHz << 25) / 32000000UL;
            byte[] rfFreqBytes = new byte[4];
            BinaryPrimitives.WriteUInt32BigEndian(rfFreqBytes, (uint)frf);
            WriteCommand(OpSetRfFrequency, rfFreqBytes);
        }

        /// <summary>
        /// Puts the chip into continuous RX mode (timeout = 0xFFFFFF).
        /// </summary>
        /// <remarks>Called automatically by <see cref="Sx1262.StartPolling" /> and after transmit.</remarks>
        public void StartReceiving()
        {
            WriteCommand(OpSetRx, new byte[] { 0xFF, 0xFF, 0xFF });
        }

        // ---------------------------------------------------------------
        // Step 3 — TX
        //// ---------------------------------------------------------------

        /// <inheritdoc/>
        /// <exception cref="InvalidOperationException">This method was called from the RX polling thread (e.g. inside <see cref="PacketReceived" />). Post work to another thread instead.</exception>
        /// <exception cref="TimeoutException">DIO1 did not indicate TX completion within <paramref name="timeoutMs" />, or the chip reported a TX timeout IRQ.</exception>
        public void Send(byte[] payload, int timeoutMs)
        {
            // Poll-thread check must run before _sendLock: the poll thread must never block on _sendLock
            // (e.g. if a PacketReceived handler calls Send), or it can deadlock with the thread holding the lock.
            bool wasPolling;
            lock (_pollLock)
            {
                if (_pollThread != null && Thread.CurrentThread == _pollThread)
                {
                    throw new InvalidOperationException();
                }

                wasPolling = _pollThread != null;
            }

            // Never call StartPolling while holding _sendLock: the poll thread may invoke PacketReceived
            // and a handler could call Send again, which would deadlock. Restart RX only after releasing the lock.
            bool restoreRxAfterSend = false;
            lock (_sendLock)
            {
                SendCore(payload, timeoutMs, wasPolling, out restoreRxAfterSend);
            }

            if (restoreRxAfterSend)
            {
                StartPolling();
            }
        }

        /// <summary>TX path shared by <see cref="Send" />.</summary>
        /// <param name="payload">The payload to send.</param>
        /// <param name="timeoutMs">The timeout in milliseconds.</param>
        /// <param name="wasPolling">Indicates whether polling was active before sending.</param>
        /// <param name="restoreRxAfterSend">Set to true to restore RX polling after sending.</param>
        /// <exception cref="ArgumentNullException"><paramref name="payload" /> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="payload" /> is empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="payload" /> is longer than <see cref="MaxPayloadLength" />.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="timeoutMs" /> is not positive.</exception>
        /// <exception cref="TimeoutException">TX did not complete in time or the chip signaled a TX timeout.</exception>
        /// <exception cref="InvalidOperationException">The chip did not report <c>TxDone</c> after TX.</exception>
        private void SendCore(byte[] payload, int timeoutMs, bool wasPolling, out bool restoreRxAfterSend)
        {
            restoreRxAfterSend = false;

            if (payload == null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            if (payload.Length == 0)
            {
                throw new ArgumentException(string.Empty, nameof(payload));
            }

            if (payload.Length > MaxPayloadLength)
            {
                throw new ArgumentOutOfRangeException(nameof(payload));
            }

            if (timeoutMs <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(timeoutMs));
            }

            if (wasPolling)
            {
                StopPolling();
                restoreRxAfterSend = true;
            }

            ClearIrqStatus(0xFFFF);

            byte[] packetParams = new byte[]
            {
                0x00, 0x08, 0x00,
                (byte)payload.Length,
                0x01, 0x00
            };
            WriteCommand(OpSetPacketParams, packetParams);

            WriteBuffer(0x00, payload);
            WriteCommand(OpSetTx, new byte[] { 0x00, 0x00, 0x00 });

            int elapsed = 0;
            while (!IsDio1High)
            {
                Thread.Sleep(1);
                if (++elapsed >= timeoutMs)
                {
                    throw new TimeoutException();
                }
            }

            ushort irq = GetIrqStatus();
            ClearIrqStatus(0xFFFF);

            if ((irq & IrqTimeout) != 0)
            {
                throw new TimeoutException();
            }

            if ((irq & IrqTxDone) == 0)
            {
                throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Writes bytes into the SX1262 data buffer at the given offset.
        /// </summary>
        /// <param name="offset">Start offset in the chip buffer.</param>
        /// <param name="data">Payload bytes to write.</param>
        public void WriteBuffer(byte offset, byte[] data)
        {
            byte[] tx = new byte[2 + data.Length];
            tx[0] = OpWriteBuffer;
            tx[1] = offset;
            Array.Copy(data, 0, tx, 2, data.Length);
            WaitBusy(5000);
            _spi.Write(tx);
        }

        // ---------------------------------------------------------------
        // Step 4 — RX
        //// ---------------------------------------------------------------

        /// <summary>
        /// Gets the RX buffer status after RxDone fires on DIO1.
        /// </summary>
        /// <param name="payloadLength">Receives the length of the received payload.</param>
        /// <param name="bufferOffset">Receives the buffer offset of the payload.</param>
        public void GetRxBufferStatus(out byte payloadLength, out byte bufferOffset)
        {
            byte[] r = ReadCommand(OpGetRxBufferStatus, 2);
            payloadLength = r[0];
            bufferOffset = r[1];
        }

        /// <summary>
        /// Reads <paramref name="length" /> bytes from the chip RX buffer starting at <paramref name="offset" />.
        /// </summary>
        /// <param name="offset">Offset in the RX buffer.</param>
        /// <param name="length">Number of bytes to read.</param>
        /// <returns>A copy of the received bytes.</returns>
        public byte[] ReadBuffer(byte offset, byte length)
        {
            byte[] tx = new byte[3 + length];
            byte[] rx = new byte[3 + length];
            tx[0] = OpReadBuffer;
            tx[1] = offset;
            WaitBusy(5000);
            _spi.TransferFullDuplex(tx, rx);
            byte[] result = new byte[length];
            Array.Copy(rx, 3, result, 0, length);
            return result;
        }

        /// <summary>
        /// Gets the signal quality for the last received packet.
        /// </summary>
        /// <param name="rssi">Receives RSSI in dBm.</param>
        /// <param name="snr">Receives SNR in dB.</param>
        public void GetPacketStatus(out int rssi, out float snr)
        {
            byte[] r = ReadCommand(OpGetPacketStatus, 3);
            rssi = -(r[0] / 2);
            snr = ((sbyte)r[1]) / 4.0f;
        }

        /// <summary>
        /// Reads IRQ flags, pulls the packet from the buffer, raises <see cref="Sx1262.PacketReceived" />, then returns to RX mode.
        /// </summary>
        /// <returns>A <see cref="LoRaMessage" /> on success; null on CRC error or timeout.</returns>
        public LoRaMessage HandleRxDone()
        {
            ushort irq = GetIrqStatus();
            ClearIrqStatus(0xFFFF);

            LoRaMessage msg = null;
            try
            {
                if ((irq & IrqTimeout) != 0)
                {
                    return null;
                }

                if ((irq & IrqCrcErr) != 0)
                {
                    return null;
                }

                if ((irq & IrqRxDone) == 0)
                {
                    return null;
                }

                GetRxBufferStatus(out byte length, out byte offset);
                byte[] payload = ReadBuffer(offset, length);

                GetPacketStatus(out int rssi, out float snr);
                msg = new LoRaMessage(payload, rssi, snr);
            }
            finally
            {
                StartReceiving();
            }

            if (msg != null)
            {
                PacketReceivedHandler handler = PacketReceived;
                if (handler != null)
                {
                    try
                    {
                        handler(this, msg);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("LoRa PacketReceived handler failed: " + ex.Message);
                    }
                }
            }

            return msg;
        }

        // ---------------------------------------------------------------
        // RX poll thread (nanoFramework safe)
        //// ---------------------------------------------------------------

        /// <inheritdoc/>
        public void StartPolling()
        {
            // Keep new Thread(), Start(), and _pollThread assignment under the same lock as StopPolling uses
            // so another thread never Join()s a worker that is published but not yet started.
            // Callers must not invoke Send from PacketReceived (poll thread); doing so would deadlock while this lock is held during Start().
            lock (_pollLock)
            {
                if (_pollThread != null)
                {
                    return;
                }

                Interlocked.Exchange(ref _stopPolling, 0);
                StartReceiving();
                Thread worker = new Thread(PollLoop);
                worker.Start();
                _pollThread = worker;
            }
        }

        /// <inheritdoc/>
        public void StopPolling()
        {
            Thread worker;
            lock (_pollLock)
            {
                Interlocked.Exchange(ref _stopPolling, 1);
                worker = _pollThread;
            }

            if (worker == null)
            {
                return;
            }

            if (Thread.CurrentThread != worker)
            {
                if (worker.IsAlive)
                {
                    worker.Join();
                }
            }

            lock (_pollLock)
            {
                if (Thread.CurrentThread != worker && _pollThread == worker)
                {
                    _pollThread = null;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether DIO1 is high (an IRQ is pending).
        /// </summary>
        public bool IsDio1High => _dio1Pin.Read() == PinValue.High;

        /// <summary>
        /// Gets the current IRQ status flags from the chip.
        /// </summary>
        /// <returns>The 16-bit IRQ status flags.</returns>
        public ushort GetIrqStatus()
        {
            byte[] r = ReadCommand(OpGetIrqStatus, 2);
            return BinaryPrimitives.ReadUInt16BigEndian(r);
        }

        /// <summary>
        /// Clears IRQ flags after handling.
        /// </summary>
        /// <param name="mask">Bits to clear in the IRQ status register.</param>
        public void ClearIrqStatus(ushort mask)
        {
            byte[] clearBytes = new byte[2];
            BinaryPrimitives.WriteUInt16BigEndian(clearBytes, mask);
            WriteCommand(OpClearIrqStatus, clearBytes);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            StopPolling();

            if (_resetPin != null)
            {
                _resetPin.Dispose();
                _resetPin = null;
            }

            if (_busyPin != null)
            {
                _busyPin.Dispose();
                _busyPin = null;
            }

            if (_dio1Pin != null)
            {
                _dio1Pin.Dispose();
                _dio1Pin = null;
            }

            if (_disposeSpi && _spi != null)
            {
                _spi.Dispose();
            }

            if (_shouldDispose && _gpio != null)
            {
                _gpio.Dispose();
            }

            _disposed = true;
        }

        private void PollLoop()
        {
            try
            {
                while (Interlocked.CompareExchange(ref _stopPolling, 0, 0) == 0)
                {
                    if (IsDio1High)
                    {
                        try
                        {
                            HandleRxDone();
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("Sx1262 RX handling error: " + ex.Message);
                        }
                    }
                    else
                    {
                        Thread.Sleep(5);
                    }
                }
            }
            finally
            {
                lock (_pollLock)
                {
                    if (_pollThread == Thread.CurrentThread)
                    {
                        _pollThread = null;
                    }
                }
            }
        }

        internal void WriteCommand(byte opCode, byte[] data)
        {
            byte[] tx = new byte[1 + data.Length];
            tx[0] = opCode;
            Array.Copy(data, 0, tx, 1, data.Length);
            WaitBusy(5000);
            _spi.Write(tx);
        }

        internal byte[] ReadCommand(byte opCode, int responseLen)
        {
            byte[] tx = new byte[2 + responseLen];
            byte[] rx = new byte[2 + responseLen];
            tx[0] = opCode;
            WaitBusy(5000);
            _spi.TransferFullDuplex(tx, rx);
            byte[] result = new byte[responseLen];
            Array.Copy(rx, 2, result, 0, responseLen);
            return result;
        }
    }
}
