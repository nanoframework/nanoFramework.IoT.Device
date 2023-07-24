// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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

        /// <inheritdoc/>
        public bool DeleteFile(string fileName)
        {
            // Allocate buffer
            _modem.Channel.SendCommand("AT+CFSINIT");

            var response = _modem.Channel.SendCommand($"AT+CFSDFILE=0,\"{fileName}\"");

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

            var response = _modem.Channel.SendSingleLineCommandAsync("AT+CFSGFRS?", "+CFSGFRS:");
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

            var response = _modem.Channel.SendCommand($"AT+CFSREN=0,\"{oldFileName}\",\"{newFileName}\"");

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

            var response = _modem.Channel.SendSingleLineCommandAsync("AT+CFSGFRS?", "+CFSGFRS:");
            if (response.Success)
            {
                size = response.Intermediates.Count > 0 ? int.Parse(((string)response.Intermediates[0]).Substring(10)) : -1;
            }

            // Free data buffer
            _modem.Channel.SendCommand("AT+CFSTERM");
            return size;
        }

        /// <inheritdoc/>
        public string ReadFile(string fileName)
        {
            string result = null;

            // Allocate buffer
            _modem.Channel.SendCommand("AT+CFSINIT");

            // Get the file size
            var response = _modem.Channel.SendSingleLineCommandAsync($"AT+CFSGFIS=0,\"{fileName}\"", "+CFSGFIS:");
            if (response.Success)
            {
                var size = response.Intermediates.Count > 0 ? int.Parse(((string)response.Intermediates[0]).Substring(10)) : -1;
                if (size > 0)
                {
                    // Read the file
                    var fileresp = _modem.Channel.SendMultilineCommand($"AT+CFSRFILE=0,\"{fileName}\",0,{size},0", string.Empty);
                    if (fileresp.Success)
                    {
                        result = fileresp.Intermediates.Count > 1 ? (string)fileresp.Intermediates[1] : null;
                    }
                }
            }

            // Free data buffer
            _modem.Channel.SendCommand("AT+CFSTERM");
            return result;
        }

        /// <inheritdoc/>
        public bool WriteFile(string fileName, string content)
        {
            // Allocate buffer
            _modem.Channel.SendCommand("AT+CFSINIT");

            // 10 seconds timeout
            _modem.Channel.SendCommand($"AT+CFSWFILE=0,\"{fileName}\",0,{content.Length},10000");

            // Send the content
            var response = _modem.Channel.SendCommand(content);

            // Free data buffer
            _modem.Channel.SendCommand("AT+CFSTERM");

            return response.Success;
        }
    }
}
