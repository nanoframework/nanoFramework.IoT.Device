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
        /// <summary>
        /// Prefix for commands starting with 0x07.
        /// </summary>
        private static byte _commandPrefix07 = 0x07;

        /// <summary>
        /// Prefix for commands starting with 0x08.
        /// </summary>
        private static byte _commandPrefix08 = 0x08;

        /// <summary>
        /// Index of 1st parameter in suffix buffers.
        /// </summary>
        private static byte _param1Index = 1;

        /// <summary>
        /// Index of 2nd parameter in suffix buffers.
        /// </summary>
        private static byte _param2Index = 3;

        /// <summary>
        /// Length of ACK message when decoding in host mode.
        /// Actual data will start after this position.
        /// </summary>
        private static int _hostModeAckLenght = 6;

        /// <summary>
        /// Sequence common to most commands.
        /// </summary>
        private static byte[] _commandCommon = new byte[] { 0xC6, 0x04, 0x08, 0x00, 0xF2 };

        private static byte[] _decodingCommandBase = new byte[] { 0x04, 0xE4, 0x04, 0x00, 0xFF, 0x14 };

        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private BarcodeDataAvailableEventHandler _callbacksBarcodeDataAvailableEvent = null;

        private static AutoResetEvent _newDataAvailable = new(false);

        // temp buffer to hold command data
        // created here to speed up transmission
        private byte[] _commandBuffer;
        private int _commandBufferLastPos = 10;

        // temp buffer to hold received data
        // created here to speed up receiving
        private byte[] _readBuffer;
        private int _readBufferIndex;

        private bool _disposed;

        private TimeSpan _lastTimeout;

        // backing fields for properties
        private readonly string _portName;
        private readonly SerialPort _readerSerialPort;
        private bool _waitingForData = false;
        private TriggerMode _triggerMode = TriggerMode.None;

        /// <summary>
        /// Get and set the trigger mode for scanning.
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
                        suffix[_param1Index] = 0x00;
                        suffix[_param2Index] = 0x9D;
                        break;

                    case TriggerMode.KeyPress:
                        suffix[_param1Index] = 0x02;
                        suffix[_param2Index] = 0x9B;
                        break;

                    case TriggerMode.Continuous:
                        suffix[_param1Index] = 0x04;
                        suffix[_param2Index] = 0x99;
                        break;

                    case TriggerMode.Autosensing:
                        suffix[_param1Index] = 0x09;
                        suffix[_param2Index] = 0x94;
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
        /// Set the buzzer volume intensity.
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
                        suffix[_param1Index] = 0x01;
                        suffix[_param2Index] = 0x9A;
                        break;

                    case BuzzerVolume.Low:
                        suffix[_param1Index] = 0x02;
                        suffix[_param2Index] = 0x99;
                        break;
                }

                SendCommandType07(suffix);
            }
        }

        /// <summary>
        /// <see langword="true"/> to enable all beeps. <see langword="false"/> to disable all beeps.
        /// </summary>
        public bool EnableBeep
        {
            set
            {
                // default for disabling beeps
                byte[] suffix = new byte[] { 0x0C, 0x00, 0xFE, 0x28 };

                if (value)
                {
                    suffix[_param1Index] = 0x01;
                    suffix[_param2Index] = 0x28;
                }

                SendCommandType08(suffix);
            }
        }

        /// <summary>
        /// Set the positioning light mode.
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
                        suffix[_param1Index] = 0x01;
                        suffix[_param2Index] = 0x30;
                        break;

                    case Positioninglight.AlwaysOff:
                        suffix[_param1Index] = 0x02;
                        suffix[_param2Index] = 0x2F;
                        break;
                }

                SendCommandType08(suffix);
            }
        }

        /// <summary>
        /// Set the flood light mode.
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
                        suffix[_param1Index] = 0x01;
                        suffix[_param2Index] = 0x31;
                        break;

                    case FloodLight.AlwaysOff:
                        suffix[_param1Index] = 0x02;
                        suffix[_param2Index] = 0x30;
                        break;
                }

                SendCommandType08(suffix);
            }
        }

        /// <summary>
        /// Set the terminator to be appended to the code data.
        /// </summary>
        public Terminator Terminator { set; private get; } = Terminator.None;

        /// <summary>
        /// Creates a QR Code Reader.
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
        /// Start scanning for barcodes in automatic mode.
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
                // hang in here to let the command flow to make sure the thread doesn't exit immediately
                Thread.Sleep(100);

                do
                {
                    if (_newDataAvailable.WaitOne((int)_lastTimeout.TotalMilliseconds, true))
                    {
                        OnBarcodeDataAvailableInternal(ProcessNewData());
                    }
                }
                while (_waitingForData);

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
                _decodingCommandBase,
                _commandBuffer,
                _decodingCommandBase.Length);

            _commandBuffer[_param1Index] = 0xE5;
            _commandBuffer[5] = 0x13;

            SendCommand(
                _commandBuffer,
                0,
                _decodingCommandBase.Length);

            // set back host mode
            TriggerMode = TriggerMode.Host;
        }

        /// <summary>
        /// Tries to read a barcode within the provided timeout.
        /// Requires previously setting the scan mode to <see cref="TriggerMode.Host"/>.
        /// </summary>
        /// <param name="timeout">Timeout waiting for a barcode to be read. See below for valid timeout values.</param>
        /// <returns>A string with the data read from the barcode. An <see cref="string.Empty"/> will be returned if no data was read.</returns>
        /// <remarks>
        /// <para>
        /// This call will block until a code is read by the scanner or a timeout occurs waiting to scan a barcode.
        /// </para>
        /// <para>
        /// The timeout will be rounded to a valid timeout value.
        /// </para>
        /// <para>
        /// Valid timeout values are: 1-3-5-10-15-20 seconds. 
        /// </para>
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
        public string TryReadBarcode(TimeSpan timeout)
        {
            if (timeout.Ticks != _lastTimeout.Ticks)
            {
                // configure new timeout
                SetScanTimeout(timeout);
            }

            return TryReadBarcode();
        }

        /// <summary>
        /// Tries to read a bar code.
        /// Requires previously setting the scan mode to <see cref="TriggerMode.Host"/>.
        /// </summary>
        /// <returns>A string with the data read from the barcode. An <see cref="string.Empty"/> will be returned if no data was read.</returns>
        /// <remarks>
        /// <para>
        /// This call will block until a code is read by the scanner or a timeout occurs waiting to scan a barcode.
        /// </para>
        /// <para>
        /// The timeout to scan a barcode is the default one of the reader (3 seconds) or can be set by calling <see cref="TryReadBarcode(TimeSpan)"/>.
        /// </para>
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
        public string TryReadBarcode()
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

            if (_newDataAvailable.WaitOne((int)_lastTimeout.TotalMilliseconds, true))
            {
                return ProcessNewData();
            }
            else
            {
                return string.Empty;
            }
        }

        private string ProcessNewData()
        {
            int tabIndex = 0;
            var codeData = string.Empty;


            // look for TAB, if anything was read
            if (_readBufferIndex > _hostModeAckLenght)
            {
                // start searching after ACK segment
                tabIndex = _hostModeAckLenght;

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
                    _hostModeAckLenght,
                    tabIndex - _hostModeAckLenght);

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

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_readerSerialPort != null)
                    {
                        _readerSerialPort.Close();
                        _readerSerialPort.Dispose();
                    }
                }

                _readBuffer = null;
                _disposed = true;
            }
        }

        /// <inheritdoc/>
        ~QrCodeReader()
        {
            Dispose(disposing: false);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
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
            SetScanTimeout(TimeSpan.FromSeconds(3));
        }

        private void ExcuteStartScanning()
        {
            // need to set this upfront, otherwise we could missing data
            _waitingForData = true;

            SendCommand(
                _decodingCommandBase,
                0,
                _decodingCommandBase.Length,
                true);
        }

        private void SetScanTimeout(TimeSpan timeout)
        {
            // valid timeout values are: 1 - 3 - 5 - 10 - 15 - 20 seconds.

            // value for 1000ms
            byte[] suffix = new byte[] { 0x88, 0x0A, 0xFE, 0x95 };

            if (timeout.TotalMilliseconds <= 1000)
            {
                // this is the default on the suffix
            }
            else if (timeout.TotalMilliseconds <= 3000)
            {
                suffix[_param1Index] = 0x1E;
                suffix[_param2Index] = 0x81;
            }
            else if (timeout.TotalMilliseconds <= 5000)
            {
                suffix[_param1Index] = 0x32;
                suffix[_param2Index] = 0x6D;
            }
            else if (timeout.TotalMilliseconds <= 10000)
            {
                suffix[_param1Index] = 0x64;
                suffix[_param2Index] = 0x3B;
            }
            else if (timeout.TotalMilliseconds <= 15000)
            {
                suffix[_param1Index] = 0x96;
                suffix[_param2Index] = 0x09;
            }
            else if (timeout.TotalMilliseconds <= 20000)
            {
                suffix[_param1Index] = 0xC8;
                suffix[_param2Index] = 0xD7;
            }
            else
            {
                throw new ArgumentException();
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
            _commandBuffer[0] = _commandPrefix07;

            // set command common sequence
            // this type has different ending, so removing the last one 
            Array.Copy(
                _commandCommon,
                0,
                _commandBuffer,
                commandLength,
                _commandCommon.Length - 1);

            // update counter
            commandLength += _commandCommon.Length - 1;

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
            _commandBuffer[0] = _commandPrefix08;

            // set command common sequence 
            Array.Copy(
                _commandCommon,
                0,
                _commandBuffer,
                commandLength,
                _commandCommon.Length);

            // update counter
            commandLength += _commandCommon.Length;

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
                _commandBuffer[_commandBufferLastPos] = 0x00;

                _readerSerialPort.Write(
                    _commandBuffer,
                    _commandBufferLastPos,
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
                        _newDataAvailable.Set();
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
