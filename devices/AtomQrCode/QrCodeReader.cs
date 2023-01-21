// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace Iot.Device.AtomQrCode
{
    /// <summary>
    /// Class to control an M5Stack ATOM QR Code reader module.
    /// </summary>
    public class QrCodeReader : IDisposable
    {
        // NOTE: the command sequences in the document are coming from the device datasheet:
        // https://m5stack.oss-cn-shenzhen.aliyuncs.com/resource/docs/datasheet/atombase/AtomicQR/ATOM_QRCODE_CMD_EN.pdf

        /// <summary>
        /// Prefix for commands starting with 0x07.
        /// </summary>
        private const byte CommandPrefix07 = 0x07;

        /// <summary>
        /// Prefix for commands starting with 0x08.
        /// </summary>
        private const byte CommandPrefix08 = 0x08;

        /// <summary>
        /// Index of 1st parameter in suffix buffers.
        /// </summary>
        private const byte Param1Index = 1;

        /// <summary>
        /// Index of 2nd parameter in suffix buffers.
        /// </summary>
        private const byte Param2Index = 3;

        /// <summary>
        /// Length of ACK message when decoding in host mode.
        /// Actual data will start after this position.
        /// </summary>
        private const int HostModeAckLenght = 6;

        private const int CommandBufferLastPos = 10;

        /// <summary>
        /// Sequence common to most commands.
        /// </summary>
        private static readonly byte[] CommandCommon = new byte[] { 0xC6, 0x04, 0x08, 0x00, 0xF2 };

        private static readonly byte[] DecodingCommandBase = new byte[] { 0x04, 0xE4, 0x04, 0x00, 0xFF, 0x14 };

        private static AutoResetEvent newDataAvailable = new AutoResetEvent(false);

        // temp buffer to hold command data
        // created here to speed up transmission
        private readonly byte[] _commandBuffer;

        // temp buffer to hold received data
        // created here to speed up receiving
        private readonly byte[] _readBuffer;

        // backing fields for properties
        private readonly string _portName;
        private readonly SerialPort _readerSerialPort;

        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private BarcodeDataAvailableEventHandler _callbacksBarcodeDataAvailableEvent = null;

        private int _readBufferIndex;
        private bool _disposed;
        private ScanTimeout _lastTimeout;
        private bool _isReallyStopped = true;
        private bool _waitingForData = false;
        private TriggerMode _triggerMode = TriggerMode.None;

        /// <summary>
        /// Gets or sets the trigger mode for scanning.
        /// </summary>
        public TriggerMode TriggerMode
        {
            get => _triggerMode;
            set
            {
                byte[] suffix = new byte[] { 0x8A, 0x08, 0xFE, 0x95 };

                switch (value)
                {
                    case TriggerMode.Host:
                        // this is the default on the suffix
                        break;

                    case TriggerMode.KeyHold:
                        suffix[Param1Index] = 0x00;
                        suffix[Param2Index] = 0x9D;
                        break;

                    case TriggerMode.KeyPress:
                        suffix[Param1Index] = 0x02;
                        suffix[Param2Index] = 0x9B;
                        break;

                    case TriggerMode.Continuous:
                        suffix[Param1Index] = 0x04;
                        suffix[Param2Index] = 0x99;
                        break;

                    case TriggerMode.Autosensing:
                        suffix[Param1Index] = 0x09;
                        suffix[Param2Index] = 0x94;
                        break;
                }

                SendCommandType07(
                    suffix,
                    true);

                // store mode
                _triggerMode = value;
            }
        }

        /// <summary>
        /// Sets the buzzer volume intensity.
        /// </summary>
        public BuzzerVolume BuzzerVolume
        {
            set
            {
                byte[] suffix = new byte[] { 0x8C, 0x00, 0xFE, 0x9B };

                switch (value)
                {
                    case BuzzerVolume.High:
                        // this is the default on the suffix
                        break;

                    case BuzzerVolume.Medium:
                        suffix[Param1Index] = 0x01;
                        suffix[Param2Index] = 0x9A;
                        break;

                    case BuzzerVolume.Low:
                        suffix[Param1Index] = 0x02;
                        suffix[Param2Index] = 0x99;
                        break;
                }

                SendCommandType07(suffix);
            }
        }

        /// <summary>
        /// Sets a value indicating whether to enable all beeps. Sets to <see langword="false"/> to disable all beeps.
        /// </summary>
        public bool EnableBeep
        {
            set
            {
                // default for disabling beeps
                byte[] suffix = new byte[] { 0x0C, 0x00, 0xFE, 0x28 };

                if (value)
                {
                    suffix[Param1Index] = 0x01;
                    suffix[Param2Index] = 0x28;
                }

                SendCommandType08(suffix);
            }
        }

        /// <summary>
        /// Sets the positioning light mode.
        /// </summary>
        public Positioninglight Positioninglight
        {
            set
            {
                byte[] suffix = new byte[] { 0x03, 0x00, 0xFE, 0x31 };

                switch (value)
                {
                    case Positioninglight.OnWhenReading:
                        // this is the default on the suffix
                        break;

                    case Positioninglight.AlwaysOn:
                        suffix[Param1Index] = 0x01;
                        suffix[Param2Index] = 0x30;
                        break;

                    case Positioninglight.AlwaysOff:
                        suffix[Param1Index] = 0x02;
                        suffix[Param2Index] = 0x2F;
                        break;
                }

                SendCommandType08(suffix);
            }
        }

        /// <summary>
        /// Sets the flood light mode.
        /// </summary>
        public FloodLight FloodLight
        {
            set
            {
                byte[] suffix = new byte[] { 0x02, 0x00, 0xFE, 0x32 };

                switch (value)
                {
                    case FloodLight.OnWhenReading:
                        // this is the default on the suffix
                        break;

                    case FloodLight.AlwaysOn:
                        suffix[Param1Index] = 0x01;
                        suffix[Param2Index] = 0x31;
                        break;

                    case FloodLight.AlwaysOff:
                        suffix[Param1Index] = 0x02;
                        suffix[Param2Index] = 0x30;
                        break;
                }

                SendCommandType08(suffix);
            }
        }

        /// <summary>
        /// Gets or sets the terminator to be appended to the code data.
        /// </summary>
        public Terminator Terminator { private get; set; } = Terminator.None;

        /// <summary>
        /// Initializes a new instance of the <see cref="QrCodeReader" /> class.
        /// </summary>
        /// <param name="portName">The port to use (for example, COM1).</param>
        public QrCodeReader(string portName)
        {
            _portName = portName;

            _disposed = false;

            // instantiate serial port with reader default baud rate
            _readerSerialPort = new SerialPort(portName, baudRate: 9600);

            // setup event handler
            _readerSerialPort.DataReceived += Reader_DataReceived;

            // set terminator and "new line" as TAB
            _readerSerialPort.WatchChar = '\t';
            _readerSerialPort.NewLine = "\t";

            // open serial port
            _readerSerialPort.Open();

            // setup buffers
            _commandBuffer = new byte[11];
            _readBuffer = new byte[256];

            PerformInitialConfig();
        }

        /// <summary>
        /// Starts scanning for barcodes in automatic mode.
        /// Requires to set the scan mode to <see cref="TriggerMode.Autosensing"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">If <see cref="TriggerMode"/> hasn't been set to <see cref="TriggerMode.Continuous"/> previously.</exception>
        public void StartScanning()
        {
            if (TriggerMode != TriggerMode.Autosensing)
            {
                throw new InvalidOperationException();
            }

            // setup thread to process barcode reading
            new Thread(() =>
            {
                // sanity check for previous execution exited
                if (!_isReallyStopped)
                {
                    throw new InvalidOperationException();
                }

                // update flag
                _isReallyStopped = false;

                // hang in here to let the command flow to make sure the thread doesn't exit immediately
                Thread.Sleep(100);

                do
                {
                    // need to convert from enum value to milliseconds
                    if (newDataAvailable.WaitOne((int)_lastTimeout * 1000, true))
                    {
                        OnBarcodeDataAvailableInternal(ProcessNewData());
                    }
                }
                while (_waitingForData);

                // update flag
                _isReallyStopped = true;
            }).Start();

            // clear SerialPort
            _ = _readerSerialPort.ReadExisting();

            // reset read buffer index
            _readBufferIndex = 0;

            // send command to start scanning
            ExcuteStartScanning();
        }

        /// <summary>
        /// Stop scanning.
        /// Requires a previous call to <see cref="StartScanning"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">If <see cref="StartScanning"/> hasn't been set called before to start scanning.</exception>
        public void StopScanning()
        {
            if (!_waitingForData)
            {
                throw new InvalidOperationException();
            }

            // store reader state
            _waitingForData = false;

            // send command to stop decoding
            Array.Copy(
                DecodingCommandBase,
                _commandBuffer,
                DecodingCommandBase.Length);

            _commandBuffer[Param1Index] = 0xE5;
            _commandBuffer[5] = 0x13;

            SendCommand(
                _commandBuffer,
                0,
                DecodingCommandBase.Length);

            // set back host mode
            TriggerMode = TriggerMode.Host;
        }

        /// <summary>
        /// Tries to read a barcode within the provided timeout.
        /// Requires previously setting the scan mode to <see cref="TriggerMode.Host"/>.
        /// </summary>
        /// <param name="timeout">Timeout waiting for a barcode to be read.</param>
        /// <param name="data">A string with the data read from the barcode.</param>
        /// <returns><see langword="true"/> if a barcode was was read; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// This call will block until a code is read by the scanner or a timeout occurs waiting to scan a barcode.
        /// </remarks>
        /// <exception cref="ArgumentException">If <paramref name="timeout"/> can't be rounded to a valid timeout value.</exception>
        /// <exception cref="InvalidOperationException">
        /// <para>
        /// If <see cref="TriggerMode"/> hasn't been set to <see cref="TriggerMode.Host"/> previously.
        /// </para>
        /// <para>
        /// -or-
        /// </para>
        /// <para>
        /// A previous call has been made to <see cref="StartScanning"/> and continuous reading it's active.
        /// </para>
        /// /// <para>
        /// -or-
        /// </para>
        /// If <see cref="TriggerMode"/> has been set to any mode other than <see cref="TriggerMode.Host"/>.
        /// </exception>
        public bool TryReadBarcode(ScanTimeout timeout, out string data)
        {
            if (timeout != _lastTimeout)
            {
                // configure new timeout
                SetScanTimeout(timeout);
            }

            return TryReadBarcode(out data);
        }

        /// <summary>
        /// Tries to read a bar code.
        /// Requires previously setting the scan mode to <see cref="TriggerMode.Host"/>.
        /// </summary>
        /// <param name="data">A string with the data read from the barcode.</param>
        /// <returns><see langword="true"/> if a barcode was was read; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// This call will block until a code is read by the scanner or a timeout occurs waiting to scan a barcode.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// <para>
        /// If <see cref="TriggerMode"/> hasn't been set to <see cref="TriggerMode.Host"/> previously.
        /// </para>
        /// <para>
        /// -or-
        /// </para>
        /// <para>
        /// A previous call has been made to <see cref="StartScanning"/> and continuous reading it's active.
        /// </para>
        /// /// <para>
        /// -or-
        /// </para>
        /// If <see cref="TriggerMode"/> has been set to any mode other than <see cref="TriggerMode.Host"/>.
        /// </exception>
        public bool TryReadBarcode(out string data)
        {
            if (TriggerMode != TriggerMode.Host || _waitingForData)
            {
                throw new InvalidOperationException();
            }

            // clear SerialPort
            _ = _readerSerialPort.ReadExisting();

            // reset read buffer index
            _readBufferIndex = 0;

            // send command to start reading
            ExcuteStartScanning();

            if (newDataAvailable.WaitOne((int)_lastTimeout * 1000, true))
            {
                // reset flag
                _waitingForData = false;

                data = ProcessNewData();

                return true;
            }
            else
            {
                // reset flag
                _waitingForData = false;

                data = string.Empty;

                return false;
            }
        }

        private string ProcessNewData()
        {
            int tabIndex = 0;
            var codeData = string.Empty;

            // look for TAB, if anything was read
            if (_readBufferIndex > HostModeAckLenght)
            {
                // start searching after ACK segment
                tabIndex = HostModeAckLenght;

                do
                {
                    if (_readBuffer[tabIndex] == '\t')
                    {
                        break;
                    }

                    tabIndex++;
                }
                while (tabIndex < _readBuffer.Length);
            }

            if (tabIndex < _readBuffer.Length)
            {
                // read data from buffer
                // start after host ACK segment
                codeData = Encoding.UTF8.GetString(
                    _readBuffer,
                    HostModeAckLenght,
                    tabIndex - HostModeAckLenght);

                codeData = AddTerminator(codeData);
            }
            else
            {
                // nothing was read
            }

            return codeData;
        }

        private string AddTerminator(string data)
        {
            // add the requested terminator
            switch (Terminator)
            {
                case Terminator.None:
                    // nothing to do
                    break;

                case Terminator.Cr:
                    data += "\r";
                    break;

                case Terminator.Tab:
                    data += "\t";
                    break;

                case Terminator.CrLf:
                    data += "\r\n";
                    break;

                case Terminator.DoubleCr:
                    data += "\r\r";
                    break;

                case Terminator.DoubleClLf:
                    data += "\r\n\r\n";
                    break;
            }

            return data;
        }

        /// <summary>
        /// Restore all parameters and configurations to factory defaults.
        /// </summary>
        public void RestoreToFactoryDefaults()
        {
            SendCommandType08(new byte[] { 0xFF, 0x00, 0xFD, 0x35 });
        }

        /// <summary>
        /// Indicates that barcode data is available from the <see cref="QrCodeReader"/> object.
        /// </summary>
        public event BarcodeDataAvailableEventHandler BarcodeDataAvailable
        {
            add
            {
                if (_disposed)
                {
#pragma warning disable S3877 // OK to throw this here
                    throw new ObjectDisposedException();
#pragma warning restore S3877 // Exceptions should not be thrown from unexpected methods
                }

                BarcodeDataAvailableEventHandler callbacksOld = _callbacksBarcodeDataAvailableEvent;
                BarcodeDataAvailableEventHandler callbacksNew = (BarcodeDataAvailableEventHandler)Delegate.Combine(callbacksOld, value);

                try
                {
                    _callbacksBarcodeDataAvailableEvent = callbacksNew;
                }
                catch
                {
                    _callbacksBarcodeDataAvailableEvent = callbacksOld;

                    throw;
                }
            }

            remove
            {
                if (_disposed)
                {
#pragma warning disable S3877 // OK to throw this here
                    throw new ObjectDisposedException();
#pragma warning restore S3877 // Exceptions should not be thrown from unexpected methods
                }

                BarcodeDataAvailableEventHandler callbacksOld = _callbacksBarcodeDataAvailableEvent;
                BarcodeDataAvailableEventHandler callbacksNew = (BarcodeDataAvailableEventHandler)Delegate.Remove(callbacksOld, value);

                try
                {
                    _callbacksBarcodeDataAvailableEvent = callbacksNew;
                }
                catch
                {
                    _callbacksBarcodeDataAvailableEvent = callbacksOld;

                    throw;
                }
            }
        }

        /// <summary>
        /// Handles internal events and re-dispatches them to the publicly subscribed delegates.
        /// </summary>
        /// <param name="barcodeData">The barcode data.</param>
        internal void OnBarcodeDataAvailableInternal(string barcodeData)
        {
            BarcodeDataAvailableEventHandler callbacks = null;

            if (!_disposed && !string.IsNullOrEmpty(barcodeData))
            {
                callbacks = _callbacksBarcodeDataAvailableEvent;
            }

            callbacks?.Invoke(this, new BarcodeDataAvailableEventArgs(barcodeData));
        }

        #region Dispose implementation

        /// <inheritdoc/>
        public void Dispose()
        {
            if (!_disposed)
            {
                _readerSerialPort?.Close();

                _disposed = true;
            }
        }

        #endregion

        #region Helper methods

        private void PerformInitialConfig()
        {
            // send command to disable beep on parameter set
            // add wake-up, just in case
            SendCommandType08(new byte[] { 0x0E, 0x00, 0xFE, 0x26 }, true);

            // set trigger to Host mode as default
            TriggerMode = TriggerMode.Host;

            // send command to set terminator to TAB
            SendCommandType08(new byte[] { 0x05, 0x03, 0xFE, 0x2C });

            // set scan timeout to reader default: 3 seconds
            SetScanTimeout(ScanTimeout.Timeout3sec);
        }

        private void ExcuteStartScanning()
        {
            // need to set this upfront, otherwise we could missing data
            _waitingForData = true;

            SendCommand(
                DecodingCommandBase,
                0,
                DecodingCommandBase.Length,
                true);
        }

        private void SetScanTimeout(ScanTimeout timeout)
        {
            // valid timeout values are: 1 - 3 - 5 - 10 - 15 - 20 seconds.

            // value for 1000ms
            byte[] suffix = new byte[] { 0x88, 0x0A, 0xFE, 0x95 };

            switch (timeout)
            {
                case ScanTimeout.Timeout1sec:
                    // this is the default on the suffix
                    break;

                case ScanTimeout.Timeout3sec:
                    suffix[Param1Index] = 0x1E;
                    suffix[Param2Index] = 0x81;
                    break;

                case ScanTimeout.Timeout5sec:
                    suffix[Param1Index] = 0x32;
                    suffix[Param2Index] = 0x6D;
                    break;

                case ScanTimeout.Timeout10sec:
                    suffix[Param1Index] = 0x64;
                    suffix[Param2Index] = 0x3B;
                    break;

                case ScanTimeout.Timeout15sec:
                    suffix[Param1Index] = 0x96;
                    suffix[Param2Index] = 0x09;
                    break;

                case ScanTimeout.Timeout20sec:
                    suffix[Param1Index] = 0xC8;
                    suffix[Param2Index] = 0xD7;
                    break;
            }

            SendCommandType07(suffix);

            // store new timeout value
            _lastTimeout = timeout;
        }

        private void SendCommandType07(
            byte[] suffix,
            bool sendWakeUp = false)
        {
            // buffer content length
            int commandLength = 1;

            // set command prefix
            _commandBuffer[0] = CommandPrefix07;

            // set command common sequence
            // this type has different ending, so removing the last one 
            Array.Copy(
                CommandCommon,
                0,
                _commandBuffer,
                commandLength,
                CommandCommon.Length - 1);

            // update counter
            commandLength += CommandCommon.Length - 1;

            // set command suffix
            Array.Copy(
                suffix,
                0,
                _commandBuffer,
                commandLength,
                suffix.Length);

            // update counter
            commandLength += suffix.Length;

            SendCommand(
                _commandBuffer,
                0,
                commandLength,
                sendWakeUp);
        }

        private void SendCommandType08(
            byte[] suffix,
            bool sendWakeUp = false)
        {
            // buffer content length
            int commandLength = 1;

            // set command prefix
            _commandBuffer[0] = CommandPrefix08;

            // set command common sequence 
            Array.Copy(
                CommandCommon,
                0,
                _commandBuffer,
                commandLength,
                CommandCommon.Length);

            // update counter
            commandLength += CommandCommon.Length;

            // set command suffix
            Array.Copy(
                suffix,
                0,
                _commandBuffer,
                commandLength,
                suffix.Length);

            // update counter
            commandLength += suffix.Length;

            SendCommand(
                _commandBuffer,
                0,
                commandLength,
                sendWakeUp);
        }

        private void SendCommand(
            byte[] buffer,
            int offset,
            int count,
            bool sendWakeUp = false)
        {
            if (!_readerSerialPort.IsOpen)
            {
                throw new InvalidOperationException();
            }

            if (sendWakeUp)
            {
                // send wake-up 
                _commandBuffer[CommandBufferLastPos] = 0x00;

                _readerSerialPort.Write(
                    _commandBuffer,
                    CommandBufferLastPos,
                    1);

                // need to wait
                Thread.Sleep(50);
            }

            _readerSerialPort.Write(buffer, offset, count);

            // wait to process command and receive reply
            Thread.Sleep(50);
        }

        private void Reader_DataReceived(
            object sender,
            SerialDataReceivedEventArgs e)
        {
            if (_waitingForData)
            {
                _readBufferIndex += _readerSerialPort.Read(
                    _readBuffer,
                    _readBufferIndex,
                    _readerSerialPort.BytesToRead);

                // peek last received byte for TAB
                if (_readBufferIndex > 0)
                {
                    if (_readBuffer[_readBufferIndex - 1] == '\t')
                    {
                        // signal event
                        newDataAvailable.Set();
                    }
                }
            }
            else
            {
                // just throw away the data
                _ = _readerSerialPort.ReadExisting();
            }
        }

        #endregion
    }
}
