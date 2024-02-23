// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading;
using Iot.Device.AtModem.Modem;

namespace Iot.Device.AtModem.FileStorage
{
    /// <summary>
    /// Represents the file storage of SIM767XX.
    /// </summary>
    public class Sim7672FileStorage : IFileStorage
    {
        private readonly ModemBase _modem;
        private StorageDirectory _storage = StorageDirectory.Internal;
        private string _currentDrive = string.Empty;

        internal Sim7672FileStorage(ModemBase modem)
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

        /// <summary>
        /// Gets or sets the storage directory.
        /// </summary>
        public StorageDirectory Storage
        {
            get => _storage;
            set
            {
                string drive = string.Empty;
                if (value == StorageDirectory.Internal)
                {
                    drive = "C:";
                }
                else
                {
                    drive = "D:";
                }

                var response = _modem.Channel.SendCommandReadSingleLine($"AT+FSCD={drive}", "+FSCD");
                if (response.Success)
                {
                    // +FSCD: C:/
                    _currentDrive = ((string)response.Intermediates[0]).Substring(7);
                    _storage = value;
                }
            }
        }

        /// <inheritdoc/>
        public bool HasDirectorySupport => true;

        /// <inheritdoc/>
        public bool CreateDirectory(string directoryName)
        {
            // Works only in the current drive
            var response = _modem.Channel.SendCommand($"AT+FSMKDIR={directoryName}");
            return response.Success;
        }

        /// <inheritdoc/>
        public bool DeleteDirectory(string directoryName)
        {
            // Works only in the current drive
            var response = _modem.Channel.SendCommand($"AT+FSRMDIR={directoryName}");
            return response.Success;
        }

        /// <inheritdoc/>
        public bool DeleteFile(string fileName)
        {
            fileName = Replace(fileName, '\\', '/');

            // Del is only on the current drive
            var response = _modem.Channel.SendCommand($"AT+FSDEL={fileName}");
            return response.Success;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // Nothing to do here.
        }

        /// <inheritdoc/>
        public int GetAvailableStorage()
        {
            var response = _modem.Channel.SendCommandReadSingleLine("AT+FSMEM", "+FSMEM");
            if (response.Success)
            {
                try
                {
                    // +FSMEM: C:(11348480, 2201600)
                    var line = ((string)response.Intermediates[0]).Substring(11).TrimEnd(')');
                    var parts = line.Split(',');
                    var total = int.Parse(parts[0]);
                    var used = int.Parse(parts[1]);
                    return total - used;
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
            fileName = Replace(fileName, '\\', '/');
            var response = _modem.Channel.SendCommandReadSingleLine($"AT+FSATTRI={_currentDrive}{fileName}", "+FSATTRI");
            if (response.Success)
            {
                try
                {
                    // +FSATTRI: 12345
                    return int.Parse(((string)response.Intermediates[0]).Substring(10));
                }
                catch
                {
                    // Nothing on purpose
                }
            }

            return -1;
        }

        /// <inheritdoc/>
        public string[] ListDirectory(string directoryName)
        {
            // You can only read the current drive
            var response = _modem.Channel.SendCommandReadMultiline($"AT+FSLS", string.Empty);
            if (response.Success)
            {
                try
                {
                    var result = new string[response.Intermediates.Count];
                    for (int i = 0; i < response.Intermediates.Count; i++)
                    {
                        // Result looks like this:
                        // AT+FSLS
                        // +FSLS: SUBDIRECTORIES:
                        // FirstDir
                        // SecondDir
                        //
                        // +FSLS: FILES:
                        // image_0.jpg
                        // image_1.jpg
                        //
                        // OK
                        var res = (string)response.Intermediates[i];
                        if (res.StartsWith("+FSL") || string.IsNullOrEmpty(res))
                        {
                            continue;
                        }

                        result[i] = res;
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
        public string ReadFile(string fileName, int position = 0)
        {
            string result = null;
            fileName = Replace(fileName, '\\', '/');

            int size = GetFileSize(fileName);
            var fileresp = _modem.Channel.SendCommandReadMultiline($"AT+CFTRANTX={_currentDrive}{fileName},{position},{size - position}", string.Empty);
            if (fileresp.Success)
            {
                foreach (var item in fileresp.Intermediates)
                {
                    if (((string)item).StartsWith("+CFTRANTX"))
                    {
                        continue;
                    }

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
            try
            {
                var res = ReadFile(fileName, position);
                content = System.Text.Encoding.UTF8.GetBytes(res);
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public bool RenameFile(string oldFileName, string newFileName)
        {
            oldFileName = Replace(oldFileName, '\\', '/');
            newFileName = Replace(newFileName, '\\', '/');

            // This does work only in the current directory
            var response = _modem.Channel.SendCommand($"AT+FSRENAME={oldFileName},{newFileName}");
            return response.Success;
        }

        /// <inheritdoc/>
        public bool WriteFile(string fileName, string content, CreateMode createMode = CreateMode.Override) => WriteFile(fileName, System.Text.Encoding.UTF8.GetBytes(content), createMode);

        /// <inheritdoc/>
        public bool WriteFile(string fileName, byte[] content, CreateMode createMode = CreateMode.Override)
        {
            AtResponse response;
            fileName = Replace(fileName, '\\', '/');

            // First check if the file exists already
            var size = GetFileSize(fileName);
            if ((size > 0) && (createMode == CreateMode.Override))
            {
                // We need to create the file
                DeleteFile(fileName);
            }

            // Then we can write the data, here, it'sz like for SMS, we are getting a > prompt
            if (createMode == CreateMode.Override)
            {
                response = _modem.Channel.SendCommandReadSingleLine($"AT+CFTRANRX={_currentDrive}{fileName},{content.Length}", string.Empty, TimeSpan.FromMilliseconds(200));
            }
            else
            {
                size = size > 0 ? size : 0;
                response = _modem.Channel.SendCommandReadSingleLine($"AT+CFTRANRX={_currentDrive}{fileName},{size},{content.Length}", string.Empty, TimeSpan.FromMilliseconds(200));
            }

            // Wait for the > prompt
            Thread.Sleep(100);
            _modem.Channel.SendBytesWithoutAck(content);

            // We need to wait a bit
            Thread.Sleep(1000);

            // Making sure all is ok
            var result = _modem.Channel.SendCommand("AT");

            // And removing the result of the AT command
            _modem.Channel.Clear();
            return result.Success;
        }

        private string Replace(string str, char origin, char replacement)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }

            string ret = string.Empty;
            for (int i = 0; i < str.Length; i++)
            {
                ret += str[i] == origin ? replacement : str[i];
            }

            return ret;
        }
    }
}
