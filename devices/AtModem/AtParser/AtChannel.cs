// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;
using IoT.Device.AtModem.Events;

namespace IoT.Device.AtModem
{
    /// <summary>
    /// Represents an AT channel for communication with a SIM7080 module.
    /// </summary>
    public class AtChannel : IDisposable
    {
        private static readonly string[] FinalResponseErrors = new string[]
        {
            "ERROR",
            "+CMS ERROR:",
            "+CME ERROR:",
            "NO CARRIER",
            "NO ANSWER",
            "NO DIALTONE"
        };

        private static readonly string[] FinalResponseSuccesses = new string[]
        {
            "OK",
            "CONNECT"
        };

        private static readonly string[] SmsUnsoliciteds = new string[]
        {
            "+CMT:",
            "+CDS:",
            "+CBM:"
        };

        /// <summary>
        /// Represents the method that will handle unsolicited events.
        /// </summary>
        /// <param name="sender">The source of the unsolicited event.</param>
        /// <param name="e">An instance of UnsolicitedEventArgs containing event data.</param>
        public delegate void UnsolicitedEventHandler(object sender, UnsolicitedEventArgs e);

        /// <summary>
        /// Occurs when an unsolicited event is raised.
        /// </summary>
        public event UnsolicitedEventHandler UnsolicitedEvent;

        private bool _debugEnabled;

        private bool _isDisposed;
        private IAtReader _atReader;
        private IAtWriter _atWriter;
        private CancellationTokenSource _cancellationTokenSource;
        private ManualResetEvent _waitingForCommandResponse = new ManualResetEvent(false);
        private Thread _thread;

        private AtCommand _currentCommand;
        private AtResponse _currentResponse;

        private static bool IsFinalResponseSuccess(string line)
        {
            foreach (string resp in FinalResponseSuccesses)
            {
                if (line.StartsWith(resp))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsFinalResponseError(string line)
        {
            foreach (string resp in FinalResponseErrors)
            {
                if (line.StartsWith(resp))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsSMSUnsolicited(string line)
        {
            foreach (string resp in SmsUnsoliciteds)
            {
                if (line.StartsWith(resp))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Creates an instance of AtChannel using the specified input and output streams.
        /// </summary>
        /// <param name="stream">The stream used for both input and output.</param>
        /// <returns>An instance of AtChannel.</returns>
        public static AtChannel Create(SerialPort stream)
        {
            return new AtChannel(new AtReader(stream), new AtWriter(stream));
        }

        /// <summary>
        /// Creates an instance of AtChannel using the specified input and output streams.
        /// </summary>
        /// <param name="inputStream">The stream used for input.</param>
        /// <param name="outputStream">The stream used for output.</param>
        /// <returns>An instance of AtChannel.</returns>
        public static AtChannel Create(SerialPort inputStream, SerialPort outputStream)
        {
            return new AtChannel(new AtReader(inputStream), new AtWriter(outputStream));
        }

        /// <summary>
        /// Initializes a new instance of the AtChannel class with the specified AT reader and writer.
        /// </summary>
        /// <param name="atReader">The AT reader for reading responses.</param>
        /// <param name="atWriter">The AT writer for sending commands.</param>
        public AtChannel(IAtReader atReader, IAtWriter atWriter)
        {
            _atReader = atReader;
            _atWriter = atWriter;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        /// <summary>
        /// Gets a value indicating whether the AT channel is open.
        /// </summary>
        public bool IsOpen { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the AT channel is running.
        /// </summary>
        public bool IsRunning { get; internal set; }

        /// <summary>
        /// Gets or sets the default timeout duration for AT commands.
        /// </summary>
        public TimeSpan DefaultCommandTimeout { get; set; } = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Opens the AT channel.
        /// </summary>
        public void Open()
        {
            IsOpen = true;
            _atReader.Open();
            Clear();
            Start();
        }

        /// <summary>
        /// Closes the AT channel.
        /// </summary>
        public void Close()
        {
            Dispose();
            IsOpen = false;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the debug mode is enabled.
        /// </summary>
        /// <returns><see langword="true"/> if the debug mode is enabled; otherwise, <see langword="false"/>.</returns>
        public bool DebugEnabled
        {
            get => _debugEnabled;
            set => _debugEnabled = value;
        }

        /// <summary>
        /// Clears all available items from the AT reader.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        public void Clear(CancellationToken cancellationToken = default)
        {
            for (int i = 0; i < _atReader.AvailableItems(); i++)
            {
                _atReader.Read(cancellationToken);
            }
        }

        /// <summary>
        /// Sends a command and retrieves the command response.
        /// </summary>
        /// <param name="command">The AT command string to send.</param>
        /// <param name="timeout">The optional timeout duration for the command.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the command response.</returns>
        public virtual AtResponse SendCommand(string command, TimeSpan timeout = default)
        {
            return SendFullCommand(new AtCommand(AtCommandType.NoResult, command, null, null, timeout == default ? DefaultCommandTimeout : timeout));
        }

        /// <summary>
        /// Send bytes in a raw format without waiting for an acknowledgement.
        /// </summary>
        /// <param name="content">The byte array to send.</param>
        public virtual void SendBytesWithoutAck(byte[] content)
        {
            _atWriter.Write(content);
        }

        /// <summary>
        /// Reads bytes in a raw format.
        /// </summary>
        /// <param name="length">The number of bytes to read.</param>
        /// <returns>A buffer with the bytes read of the desired size even if there has been less data read.</returns>
        public virtual byte[] ReadRawBytes(int length)
        {
            return _atReader.ReadBytes(length);
        }

        /// <summary>
        /// Stops the reading thread.
        /// </summary>
        public void Stop()
        {
            // We have to pause the thread
            _cancellationTokenSource.Cancel();
            _thread.Join();
            IsRunning = false;
        }

        /// <summary>
        /// Starts the reading thread.
        /// </summary>
        public void Start()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _thread = new Thread(ReaderLoopAsync);
            _thread.Start();
            IsRunning = true;
        }

        /// <summary>
        /// Reads a single line from the AT reader.
        /// </summary>
        /// <param name="timeout">The optional timeout duration for the command.</param>
        /// <returns>A string containing a line.</returns>
        public virtual string ReadLine(TimeSpan timeout = default)
        {
            return _atReader.ReadSingleLine(new CancellationTokenSource(timeout == default ? DefaultCommandTimeout : timeout).Token);
        }

        /// <summary>
        /// Sends a single-line command and retrieves the command response.
        /// </summary>
        /// <param name="command">The AT command string to send.</param>
        /// <param name="responsePrefix">The expected response prefix.</param>
        /// <param name="timeout">The optional timeout duration for the command.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the command response.</returns>
        public virtual AtResponse SendCommandReadSingleLine(string command, string responsePrefix, TimeSpan timeout = default)
        {
            AtResponse response = SendFullCommand(new AtCommand(AtCommandType.SingleLine, command, responsePrefix, null, timeout == default ? DefaultCommandTimeout : timeout));

            if (response != null && response.Success && !(response.Intermediates.Count > 0))
            {
                // Successful command must have an intermediate response
                response.Success = false;
            }

            return response;
        }

        /// <summary>
        /// Sends a multi-line command and retrieves the command response.
        /// </summary>
        /// <param name="command">The AT command string to send.</param>
        /// <param name="responsePrefix">The expected response prefix.</param>
        /// <param name="timeout">The optional timeout duration for the command.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the command response.</returns>
        public virtual AtResponse SendCommandReadMultiline(string command, string responsePrefix, TimeSpan timeout = default)
        {
            AtCommandType commandType = responsePrefix == null ? AtCommandType.MultiLineNoPreeffiixx : AtCommandType.MultiLine;
            return SendFullCommand(new AtCommand(commandType, command, responsePrefix, null, timeout == default ? DefaultCommandTimeout : timeout));
        }

        /// <summary>
        /// Sends an SMS command and retrieves the command response.
        /// </summary>
        /// <param name="command">The AT command string to send.</param>
        /// <param name="pdu">The SMS Protocol Data Unit (PDU).</param>
        /// <param name="responsePrefix">The expected response prefix.</param>
        /// <param name="timeout">The optional timeout duration for the command.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the command response.</returns>
        public virtual AtResponse SendSms(string command, string pdu, string responsePrefix, TimeSpan timeout = default)
        {
            AtResponse response = SendFullCommand(new AtCommand(AtCommandType.SingleLine, command, responsePrefix, pdu, timeout == default ? DefaultCommandTimeout : timeout));

            if (response != null && response.Success && !(response.Intermediates.Count > 0))
            {
                // Successful command must have an intermediate response
                response.Success = false;
            }

            return response;
        }

        private AtResponse SendFullCommand(AtCommand command, CancellationToken cancellationToken = default)
        {
            try
            {
                _currentCommand = command;
                _currentResponse = new AtResponse();
                _waitingForCommandResponse = new ManualResetEvent(false);

                if (_debugEnabled)
                {
                    Debug.WriteLine($"Out: {command.Command}");
                }

                _atWriter.WriteLineAsync(command.Command);

                if ((!_waitingForCommandResponse.WaitOne((int)command.Timeout.TotalMilliseconds, true)) || cancellationToken.IsCancellationRequested)
                {
                    return new AtResponse() { Success = false, FinalResponse = "Command timed out" };
                }

                return _currentResponse;
            }
            finally
            {
                _currentCommand = default;
                _currentResponse = default;
            }
        }

        private void ReaderLoopAsync()
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                string line1;
                try
                {
                    line1 = _atReader.Read(_cancellationTokenSource.Token);
                    if (_debugEnabled)
                    {
                        Debug.WriteLine($"In: {line1}");
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                if (line1 == null)
                {
                    break;
                }

                if (line1 == string.Empty)
                {
                    continue;
                }

                if (IsSMSUnsolicited(line1))
                {
                    string line2;
                    try
                    {
                        line2 = _atReader.Read(_cancellationTokenSource.Token);
                        if (_debugEnabled)
                        {
                            Debug.WriteLine($"In: {line2}");
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }

                    if (line2 == null)
                    {
                        break;
                    }

                    HandleUnsolicited(line1, line2);
                }
                else
                {
                    ProcessMessage(line1);
                }
            }
        }

        private void ProcessMessage(string line)
        {
            if (_currentResponse == null)
            {
                HandleUnsolicited(line);
            }
            else if (IsFinalResponseSuccess(line))
            {
                _currentResponse.Success = true;
                HandleFinalResponse(line);
            }
            else if (IsFinalResponseError(line))
            {
                _currentResponse.Success = false;
                HandleFinalResponse(line);
            }
            else if (_currentCommand?.SmsPdu != null && line == "> ")
            {
                // See eg. TS 27.005 4.3
                // Commands like AT+CMGS have a "> " prompt
                if (_debugEnabled)
                {
                    Debug.WriteLine($"Out: {_currentCommand.SmsPdu}");
                }

                _atWriter.WriteSmsPduAndCtrlZAsync(_currentCommand.SmsPdu);
                _currentCommand.SmsPdu = null;
            }
            else
            {
                if (_currentCommand != null)
                {
                    switch (_currentCommand.CommandType)
                    {
                        case AtCommandType.NoResult:
                            HandleUnsolicited(line);
                            break;
                        case AtCommandType.Numeric:
                            if (!(_currentResponse.Intermediates.Count > 0) && IsDigit(line[0]))
                            {
                                AddIntermediate(line);
                            }
                            else
                            {
                                // Either we already have an intermediate response or the line doesn't begin with a digit
                                HandleUnsolicited(line);
                            }

                            break;
                        case AtCommandType.SingleLine:
                            if (!(_currentResponse.Intermediates.Count > 0) && line.StartsWith(_currentCommand.ResponsePrefix))
                            {
                                AddIntermediate(line);
                            }
                            else
                            {
                                // We already have an intermediate response
                                HandleUnsolicited(line);
                            }

                            break;
                        case AtCommandType.MultiLine:
                            if (line.StartsWith(_currentCommand.ResponsePrefix))
                            {
                                AddIntermediate(line);
                            }
                            else
                            {
                                HandleUnsolicited(line);
                            }

                            break;
                        case AtCommandType.MultiLineNoPreeffiixx:
                            AddIntermediate(line);
                            break;
                        default:
                            // This should never be reached
                            // TODO: Log error or something
                            HandleUnsolicited(line);
                            break;
                    }
                }
                else
                {
                    HandleUnsolicited(line);
                }
            }
        }

        private bool IsDigit(char c) => c >= '0' && c <= '9';

        private void AddIntermediate(string line)
        {
            _currentResponse.Intermediates.Add(line);
        }

        private void HandleFinalResponse(string line)
        {
            _currentResponse.FinalResponse = line;
            _waitingForCommandResponse.Set();
        }

        private void HandleUnsolicited(string line1, string line2 = null)
        {
            UnsolicitedEvent?.Invoke(this, new UnsolicitedEventArgs(line1, line2));
        }

        #region Dispose

        /// <inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    _cancellationTokenSource.Cancel();
                    _thread?.Join();
                    _thread = null;
                    _atReader.Close();
                    _atWriter.Close();
                    _waitingForCommandResponse.Set();
                    _waitingForCommandResponse = null;
                }

                _isDisposed = true;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
