// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using IoT.Device.AtModem.Modem;

namespace IoT.Device.AtModem.FileStorage
{
    /// <summary>
    /// Represents a <see cref="Sim7080"/> file storage.
    /// </summary>
    public class Sim7080FileStorage : IFileStorage
    {
        private readonly ModemBase _modem;

        internal Sim7080FileStorage(ModemBase modem)
        {
            _modem = modem;
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

            var response = _modem.Channel.SendSingleLineCommand("AT+CFSGFRS?", "+CFSGFRS:");
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
            int size = -1;

            // Allocate buffer
            _modem.Channel.SendCommand("AT+CFSINIT");

            var response = _modem.Channel.SendSingleLineCommand($"AT+AT+CFSDFILE={(int)Storage},\"{fileName}\"", string.Empty);
            if (response.Success)
            {
                size = response.Intermediates.Count > 0 ? int.Parse(((string)response.Intermediates[0]).Substring(10)) : -1;
            }

            // Free data buffer
            _modem.Channel.SendCommand("AT+CFSTERM");
            return size;
        }

        /// <inheritdoc/>
        public string ReadFile(string fileName, int position = 0)
        {
            string result = null;

            // Allocate buffer
            _modem.Channel.SendCommand("AT+CFSINIT");

            // Get the file size
            var response = _modem.Channel.SendSingleLineCommand($"AT+CFSGFIS={(int)Storage},\"{fileName}\"", "+CFSGFIS:");
            if (response.Success)
            {
                var size = response.Intermediates.Count > 0 ? int.Parse(((string)response.Intermediates[0]).Substring(10)) : -1;
                if (size > 0)
                {
                    // Read the file
                    var fileresp = _modem.Channel.SendMultilineCommand($"AT+CFSRFILE={(int)Storage},\"{fileName}\",{(position > 0 ? 1 : 0)},{size - position},{position}", string.Empty);
                    if (fileresp.Success)
                    {
                        foreach (var item in fileresp.Intermediates)
                        {
                            result += (string)item + "\r\n";
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
        public bool WriteFile(string fileName, string content, CreateMode createMode = CreateMode.Override)
        {
            // Allocate buffer
            _modem.Channel.SendCommand("AT+CFSINIT");

            // 10 seconds timeout
            _modem.Channel.SendCommand($"AT+CFSWFILE={(int)Storage},\"{fileName}\",{(int)createMode},{content.Length},10000");

            // Send the content
            var response = _modem.Channel.SendCommand(content);

            // Free data buffer
            _modem.Channel.SendCommand("AT+CFSTERM");

            return response.Success;
        }

        /// <inheritdoc/>
        public bool WriteFile(string fileName, byte[] content, CreateMode createMode = CreateMode.Override)
        {
            // Allocate buffer
            _modem.Channel.SendCommand("AT+CFSINIT");

            // 10 seconds timeout
            _modem.Channel.SendCommand($"AT+CFSWFILE={(int)Storage},\"{fileName}\",{(int)createMode},{content.Length},10000");

            // Send the content
            _modem.Channel.SendBytesWithoutAck(content);

            // Free data buffer
            var response = _modem.Channel.SendCommand("AT+CFSTERM");

            return response.Success;
        }

        /// <inheritdoc/>
        public bool ReadFile(string fileName, ref byte[] content, int position = 0)
        {
            throw new NotImplementedException();
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
        public bool HasDirectorySupport => false;
    }
}
