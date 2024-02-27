// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading;
using Iot.Device.AtModem.Modem;

namespace Iot.Device.AtModem.FileStorage
{
    /// <summary>
    /// Represents the file storage of SIM800.
    /// </summary>
    public class Sim800FileStorage : IFileStorage
    {
        private readonly ModemBase _modem;
        private StorageDirectory _storage = StorageDirectory.Internal;
        private string _currentDrive = string.Empty;

        internal Sim800FileStorage(ModemBase modem)
        {
            _modem = modem;
            Storage = StorageDirectory.Internal;
        }

        /// <summary>
        /// Specifies the storage directories for various purposes.
        /// </summary>
        public enum StorageDirectory
        {
            /// <summary>
            /// The internal storage..
            /// </summary>
            Internal = 0,

            /// <summary>
            /// The SD Card. If you are using this option, make sure you have an SD Card reader connected to the modem and a card inserted.
            /// </summary>
            SdCard,
        }

        /// <inheritdoc/>
        public bool HasDirectorySupport => true;

        /// <summary>
        /// Gets or sets the storage directory.
        /// </summary>
        public StorageDirectory Storage
        {
            get => _storage;
            set
            {
                var response = _modem.Channel.SendCommandReadSingleLine($"AT+FSDRIVE={(int)value}", "+FSDRIVE");
                if (response.Success)
                {
                    _currentDrive = ((string)response.Intermediates[0]).Substring(10) + @":\";
                    _storage = value;
                }
            }
        }

        /// <inheritdoc/>
        public bool DeleteFile(string fileName)
        {
            var response = _modem.Channel.SendCommand($"AT+FSDEL={_currentDrive}{fileName}");
            return response.Success;
        }

        /// <inheritdoc/>
        public int GetAvailableStorage()
        {
            var response = _modem.Channel.SendCommandReadSingleLine("AT+FSMEM", "+FSMEM");
            if (response.Success)
            {
                try
                {
                    // +FSMEM: C:186368bytes,D:123345bytes
                    var line = ((string)response.Intermediates[0]).Substring(8);
                    var parts = line.Split(',');
                    foreach (var part in parts)
                    {
                        if (part.StartsWith(_currentDrive.Substring(0, 1)))
                        {
                            var free = part.Split(':')[1];
                            return int.Parse(free.Substring(0, free.Length - 5));
                        }
                    }
                }
                catch
                {
                    // Nothing on purpose
                }
            }

            return -1;
        }

        /// <inheritdoc/>
        public int GetFileSize(string fileName)
        {
            var response = _modem.Channel.SendCommandReadSingleLine($"AT+FSFLSIZE={_currentDrive}{fileName}", "+FSFLSIZE");
            if (response.Success)
            {
                try
                {
                    // +FSFLSIZE: 12345
                    return int.Parse(((string)response.Intermediates[0]).Substring(11));
                }
                catch
                {
                    // Nothing on purpose
                }
            }

            return -1;
        }

        /// <inheritdoc/>
        public string ReadFile(string fileName, int position = 0)
        {
            string result = null;

            int size = GetFileSize(fileName);
            var fileresp = _modem.Channel.SendCommandReadSingleLine($"AT+FSREAD={_currentDrive}{fileName},{(position > 0 ? 1 : 0)},{size - position},{position}", string.Empty);
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

            return result;
        }

        /// <inheritdoc/>
        public bool ReadFile(string fileName, ref byte[] content, int position = 0)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc/>
        public bool RenameFile(string oldFileName, string newFileName)
        {
            var response = _modem.Channel.SendCommand($"AT+FSRENAME={_currentDrive}{oldFileName},{_currentDrive}{newFileName}");
            return response.Success;
        }

        /// <inheritdoc/>
        public bool WriteFile(string fileName, string content, CreateMode createMode = CreateMode.Override) => WriteFile(fileName, System.Text.Encoding.UTF8.GetBytes(content), createMode);

        /// <inheritdoc/>
        public bool WriteFile(string fileName, byte[] content, CreateMode createMode = CreateMode.Override)
        {
            AtResponse response;

            // First check if the file exists already
            if (GetFileSize(fileName) < 0)
            {
                // We need to create the file
                response = _modem.Channel.SendCommand($"AT+FSCREATE={_currentDrive}{fileName}");
                if (!response.Success)
                {
                    return false;
                }
            }

            // Then we can write the data, here, it'sz like for SMS, we are getting a > prompt
            response = _modem.Channel.SendCommandReadSingleLine($"AT+FSWRITE={_currentDrive}{fileName},{(int)createMode},{content.Length},10", string.Empty, TimeSpan.FromMilliseconds(200));
            _modem.Channel.SendBytesWithoutAck(content);

            // We need to wait a bit
            Thread.Sleep(1000);

            // Making sure all is ok
            var result = _modem.Channel.SendCommand("AT");

            // And removing the result of the AT command
            _modem.Channel.Clear();
            return result.Success;
        }

        /// <inheritdoc/>
        public bool DeleteDirectory(string directoryName)
        {
            var response = _modem.Channel.SendCommand($"AT+FSRMDIR={_currentDrive}{directoryName}");
            return response.Success;
        }

        /// <inheritdoc/>
        public bool CreateDirectory(string directoryName)
        {
            var response = _modem.Channel.SendCommand($"AT+FSMKDIR={_currentDrive}{directoryName}");
            return response.Success;
        }

        /// <inheritdoc/>
        public string[] ListDirectory(string directoryName)
        {
            var response = _modem.Channel.SendCommandReadMultiline($"AT+FSLS={_currentDrive}{(directoryName.EndsWith("\\") ? directoryName : directoryName + "\\")}", string.Empty);
            if (response.Success)
            {
                try
                {
                    var result = new string[response.Intermediates.Count];
                    for (int i = 0; i < response.Intermediates.Count; i++)
                    {
                        result[i] = (string)response.Intermediates[i];
                    }

                    return result;
                }
                catch
                {
                    // Nothing on purpose
                }
            }

            return null;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // Nothing to to
        }
    }
}
