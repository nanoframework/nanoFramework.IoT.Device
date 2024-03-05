// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Text;
using System.Threading;
using Iot.Device.AtModem.Events;
using Iot.Device.AtModem.Modem;

namespace Iot.Device.AtModem.FileStorage
{
    /// <summary>
    /// Represents a <see cref="Sim7080"/> file storage.
    /// </summary>
    public class Sim7080FileStorage : IFileStorage
    {
        private readonly ModemBase _modem;
        private readonly AutoResetEvent _fileStorageEvent = new AutoResetEvent(false);
        private int _fileStorageResult = -1;

        internal Sim7080FileStorage(ModemBase modem)
        {
            _modem = modem;
            _modem.GenericEvent += ModemGenericEvent;
        }

        private void ModemGenericEvent(object sender, GenericEventArgs e)
        {
            if (e.Message.StartsWith("+CFSGFIS:"))
            {
                _fileStorageResult = int.Parse(((string)e.Message).Substring(10));
                _fileStorageEvent.Set();
            }
        }

        /// <summary>
        /// Specifies the storage directories for various purposes.
        /// </summary>
        public enum StorageDirectory
        {
            /// <summary>
            /// The custom application storage directory.
            /// </summary>
            CustApp = 0,

            /// <summary>
            /// The storage directory for firmware over-the-air updates (FOTA).
            /// </summary>
            Fota,

            /// <summary>
            /// The data transmission storage directory.
            /// </summary>
            DataTx,

            /// <summary>
            /// The customer-specific storage directory.
            /// </summary>
            Customer,
        }

        /// <inheritdoc/>
        public bool HasDirectorySupport => false;

        /// <summary>
        /// Gets or sets the storage directory.
        /// </summary>
        public StorageDirectory Storage { get; set; } = StorageDirectory.CustApp;

        /// <inheritdoc/>
        public bool DeleteFile(string fileName)
        {
            // Allocate buffer
            _modem.Channel.SendCommand("AT+CFSINIT");

            var response = _modem.Channel.SendCommand($"AT+CFSDFILE={(int)Storage},\"{fileName}\"");

            // Free data buffer
            _modem.Channel.SendCommand("AT+CFSTERM");

            return response.Success;
        }

        /// <inheritdoc/>
        public int GetAvailableStorage()
        {
            int size = -1;

            // Allocate buffer
            _modem.Channel.SendCommand("AT+CFSINIT");

            var response = _modem.Channel.SendCommandReadSingleLine("AT+CFSGFRS?", "+CFSGFRS:");
            if (response.Success)
            {
                size = response.Intermediates.Count > 0 ? int.Parse(((string)response.Intermediates[0]).Substring(10)) : -1;
            }

            // Free data buffer
            _modem.Channel.SendCommand("AT+CFSTERM");
            return size;
        }

        /// <inheritdoc/>
        public bool RenameFile(string oldFileName, string newFileName)
        {
            // Allocate buffer
            _modem.Channel.SendCommand("AT+CFSINIT");

            var response = _modem.Channel.SendCommand($"AT+CFSREN={(int)Storage},\"{oldFileName}\",\"{newFileName}\"");

            // Free data buffer
            _modem.Channel.SendCommand("AT+CFSTERM");

            return response.Success;
        }

        /// <inheritdoc/>
        public int GetFileSize(string fileName)
        {
            // Allocate buffer
            _modem.Channel.SendCommand("AT+CFSINIT");

            _fileStorageEvent.Reset();
            _fileStorageResult = -1;

            var response = _modem.Channel.SendCommand($"AT+CFSGFIS={(int)Storage},\"{fileName}\"");
            if (response.Success)
            {
                // Wait for the answer
                _fileStorageEvent.WaitOne(2000, true);
            }

            // Free data buffer
            _modem.Channel.SendCommand("AT+CFSTERM");
            return _fileStorageResult;
        }

        /// <inheritdoc/>
        public string ReadFile(string fileName, int position = 0)
        {
            string result = null;

            // Allocate buffer
            _modem.Channel.SendCommand("AT+CFSINIT");

            _fileStorageEvent.Reset();
            _fileStorageResult = -1;

            // Get the file size
            var response = _modem.Channel.SendCommand($"AT+CFSGFIS={(int)Storage},\"{fileName}\"");
            if (response.Success)
            {
                // Wait for the answer
                _fileStorageEvent.WaitOne(2000, true);
                if (_fileStorageResult > 0)
                {
                    _fileStorageEvent.Reset();

                    // Read the file
                    var fileresp = _modem.Channel.SendCommandReadMultiline($"AT+CFSRFILE={(int)Storage},\"{fileName}\",{(position > 0 ? 1 : 0)},{_fileStorageResult - position},{position}", string.Empty);
                    if (fileresp.Success)
                    {
                        for (int i = 1; i < fileresp.Intermediates.Count; i++)
                        {
                            result += (string)fileresp.Intermediates[i] + "\r\n";
                        }

                        if (result != null)
                        {
                            result = result.Substring(0, result.Length - 2);
                        }
                    }
                }
            }

            // Free data buffer
            _modem.Channel.SendCommand("AT+CFSTERM");
            return result;
        }

        /// <inheritdoc/>
        public bool WriteFile(string fileName, string content, CreateMode createMode = CreateMode.Override) => WriteFile(fileName, Encoding.UTF8.GetBytes(content), createMode);

        /// <inheritdoc/>
        public bool WriteFile(string fileName, byte[] content, CreateMode createMode = CreateMode.Override)
        {
            // Allocate buffer
            _modem.Channel.SendCommand("AT+CFSINIT");

            // Old way, keeping up to being deeply tested
            ////_modem.Channel.Stop();

            ////// 10 seconds timeout
            ////_modem.Channel.SendBytesWithoutAck(Encoding.UTF8.GetBytes($"AT+CFSWFILE={(int)Storage},\"{fileName}\",{(int)createMode},{content.Length},10000\r\n"));

            ////// Waiting for the DOWNLOAD prompt to arrive
            ////_modem.Channel.ReadLine();
            ////var download = _modem.Channel.ReadLine();
            ////if (!download.StartsWith("DOWNLOAD"))
            ////{
            ////    _modem.Channel.Start();
            ////    return false;
            ////}

            ////// Send the content
            ////_modem.Channel.SendBytesWithoutAck(content);
            ////Thread.Sleep(200);
            ////_modem.Channel.Clear();
            ////_modem.Channel.Start();

            bool success = false;
            AtCommand command = new AtCommand(AtCommandType.CustomEndOfLine, $"AT+CFSWFILE={(int)Storage},\"{fileName}\",{(int)createMode},{content.Length},10000", "DOWNLOAD", null, TimeSpan.FromSeconds(1));
            var response = _modem.Channel.SendFullCommand(command, new CancellationTokenSource(1000).Token);
            if (response.Success)
            {
                _modem.Channel.SendBytesWithoutAck(content);
                _modem.Channel.Stop();

                // Waiting for the OK prompt to arrive
                var tocken = new CancellationTokenSource(5000).Token;
                string line;
                do
                {
                    line = _modem.Channel.ReadLine();
                }
                while (((line == "OK") || (line == "ERROR")) && (!tocken.IsCancellationRequested));

                if (line == "OK")
                {
                    success = true;
                }

                _modem.Channel.Start();
            }

            // Free data buffer
            _modem.Channel.SendCommand("AT+CFSTERM");

            return success;
        }

        /// <inheritdoc/>
        public bool ReadFile(string fileName, ref byte[] content, int position = 0)
        {
            bool result = false;

            // Allocate buffer
            _modem.Channel.SendCommand("AT+CFSINIT");

            _fileStorageEvent.Reset();
            _fileStorageResult = -1;

            // Get the file size
            var response = _modem.Channel.SendCommand($"AT+CFSGFIS={(int)Storage},\"{fileName}\"");
            if (response.Success)
            {
                // Wait for the answer
                _fileStorageEvent.WaitOne(2000, true);
                if (_fileStorageResult > 0)
                {
                    _fileStorageEvent.Reset();

                    // Stop the reading thread and go for manual mode
                    _modem.Channel.Stop();

                    // Read the file
                    _modem.Channel.SendBytesWithoutAck(Encoding.UTF8.GetBytes($"AT+CFSRFILE={(int)Storage},\"{fileName}\",{(position > 0 ? 1 : 0)},{_fileStorageResult - position},{position}\r\n"));
                    string line;
                    do
                    {
                        line = _modem.Channel.ReadLine();
                    }
                    while (!line.StartsWith("+CFSRFILE: "));

                    int size = int.Parse(line.Substring(11));
                    if (content.Length < size)
                    {
                        content = new byte[size];
                    }

                    Thread.Sleep(20);

                    // Read by chunk of 64 bytes
                    int index = 0;
                    while (index < size)
                    {
                        var chunk = _modem.Channel.ReadRawBytes(Math.Min(64, size - index));
                        chunk.CopyTo(content, index);
                        index += chunk.Length;
                    }

                    Thread.Sleep(100);
                    _modem.Channel.Clear();
                    _modem.Channel.Start();
                    result = true;
                }
            }

            // Free data buffer
            _modem.Channel.SendCommand("AT+CFSTERM");
            return result;
        }

        /// <inheritdoc/>
        public bool DeleteDirectory(string directoryName)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public bool CreateDirectory(string directoryName)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public string[] ListDirectory(string directoryName)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // Nothing to to
        }
    }
}
