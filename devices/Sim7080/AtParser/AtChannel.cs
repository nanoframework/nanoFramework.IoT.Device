// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Threading;

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

        // TODO: Add support for unsolicited events
        ////public event EventHandler<UnsolicitedEventArgs> UnsolicitedEvent;

        private bool _debugEnabled;
        ////private Action<string> _debugAction;

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
        public static AtChannel Create(Stream stream)
        {
            return new AtChannel(new AtReader(stream), new AtWriter(stream));
        }

        /// <summary>
        /// Creates an instance of AtChannel using the specified input and output streams.
        /// </summary>
        /// <param name="inputStream">The stream used for input.</param>
        /// <param name="outputStream">The stream used for output.</param>
        /// <returns>An instance of AtChannel.</returns>
        public static AtChannel Create(Stream inputStream, Stream outputStream)
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
        /// Gets or sets the default timeout duration for AT commands.
        /// </summary>
        public TimeSpan DefaultCommandTimeout { get; set; } = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Opens the AT channel.
        /// </summary>
        public void Open()
        {
            _atReader.Open();
            _thread = new Thread(ReaderLoopAsync);
            _thread.Start();
        }

        /// <summary>
        /// Closes the AT channel.
        /// </summary>
        public void Close()
        {
            Dispose();
        }

        /// <summary>
        /// Checks if the debug mode is enabled.
        /// </summary>
        /// <returns><see langword="true"/> if the debug mode is enabled; otherwise, <see langword="false"/>.</returns>
        public bool IsDebugEnabled()
        {
            return _debugEnabled;
        }

        /////// <summary>
        /////// Enables the debug mode with the specified debug action.
        /////// </summary>
        /////// <param name="debugAction">The action to perform for debug output.</param>
        ////public void EnableDebug(Action<string> debugAction)
        ////{
        ////    this.debugAction = debugAction ?? throw new ArgumentNullException(nameof(debugAction));
        ////    _debugEnabled = true;
        ////}

        /////// <summary>
        /////// Disables the debug mode.
        /////// </summary>
        ////public void DisableDebug()
        ////{
        ////    _debugEnabled = false;
        ////    debugAction = default;
        ////}

        /// <summary>
        /// Clears all available items from the AT reader.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        public void ClearAsync(CancellationToken cancellationToken = default)
        {
            for (int i = 0; i < _atReader.AvailableItems(); i++)
            {
                _atReader.ReadAsync(cancellationToken);
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
            return SendFullCommandAsync(new AtCommand(AtCommandType.NoResult, command, null, null, timeout == default ? DefaultCommandTimeout : timeout));
        }

        /// <summary>
        /// Sends a single-line command and retrieves the command response.
        /// </summary>
        /// <param name="command">The AT command string to send.</param>
        /// <param name="responsePrefix">The expected response prefix.</param>
        /// <param name="timeout">The optional timeout duration for the command.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the command response.</returns>
        public virtual AtResponse SendSingleLineCommandAsync(string command, string responsePrefix, TimeSpan timeout = default)
        {
            AtResponse response = SendFullCommandAsync(new AtCommand(AtCommandType.SingleLine, command, responsePrefix, null, timeout == default ? DefaultCommandTimeout : timeout));

            if (response != null && response.Success && !(response.Intermediates.Count > 0))
            {
                // Successful command must have an intermediate response
                throw new IOException("Did not get an intermediate response");
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
        public virtual AtResponse SendMultilineCommand(string command, string responsePrefix, TimeSpan timeout = default)
        {
            AtCommandType commandType = responsePrefix == null ? AtCommandType.MultiLineNoPreeffiixx : AtCommandType.MultiLine;
            return SendFullCommandAsync(new AtCommand(commandType, command, responsePrefix, null, timeout == default ? DefaultCommandTimeout : timeout));
        }

        /// <summary>
        /// Sends an SMS command and retrieves the command response.
        /// </summary>
        /// <param name="command">The AT command string to send.</param>
        /// <param name="pdu">The SMS Protocol Data Unit (PDU).</param>
        /// <param name="responsePrefix">The expected response prefix.</param>
        /// <param name="timeout">The optional timeout duration for the command.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the command response.</returns>
        public virtual AtResponse SendSmsAsync(string command, string pdu, string responsePrefix, TimeSpan timeout = default)
        {
            AtResponse response = SendFullCommandAsync(new AtCommand(AtCommandType.SingleLine, command, responsePrefix, pdu, timeout == default ? DefaultCommandTimeout : timeout));

            if (response != null && response.Success && !(response.Intermediates.Count > 0))
            {
                // Successful command must have an intermediate response
                throw new IOException("Did not get an intermediate response");
            }

            return response;
        }

        private AtResponse SendFullCommandAsync(AtCommand command, CancellationToken cancellationToken = default)
        {
            try
            {
                _currentCommand = command;
                _currentResponse = new AtResponse();

                if (_debugEnabled)
                {
                    ////_debugAction($"Out: {command.Command}");
                }

                _atWriter.WriteLineAsync(command.Command);

                if ((!_waitingForCommandResponse.WaitOne((int)command.Timeout.TotalMilliseconds, true)) || cancellationToken.IsCancellationRequested)
                {
                    throw new TimeoutException("Timed out while waiting for command response");
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
                    line1 = _atReader.ReadAsync(_cancellationTokenSource.Token);
                    if (_debugEnabled)
                    {
                        ////_debugAction($"In: {line1}");
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
                        line2 = _atReader.ReadAsync(_cancellationTokenSource.Token);
                        if (_debugEnabled)
                        {
                            ////_debugAction($"In: {line2}");
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
            else if (_currentCommand.SmsPdu != null && line == "> ")
            {
                // See eg. TS 27.005 4.3
                // Commands like AT+CMGS have a "> " prompt
                if (_debugEnabled)
                {
                    ////_debugAction($"Out: {_currentCommand.SmsPdu}");
                }

                _atWriter.WriteSmsPduAndCtrlZAsync(_currentCommand.SmsPdu);
                _currentCommand.SmsPdu = null;
            }
            else
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
            // TODO: Handle unsolicited messages
            ////UnsolicitedEvent?.Invoke(this, new UnsolicitedEventArgs(line1, line2));
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

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~AtChannel()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

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
