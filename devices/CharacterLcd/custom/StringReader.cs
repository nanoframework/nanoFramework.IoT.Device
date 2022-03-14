// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.CharacterLcd
{
    /// <summary>
    /// A simple String Reader class
    /// </summary>
    internal class StringReader:IDisposable
    {
        private string _str;
        private int _pos;

        /// <summary>
        /// Gets or sets the new line
        /// </summary>
        public string NewLine { get; set; } = "\r\n";

        /// <summary>
        /// Creates a string reader
        /// </summary>
        /// <param name="toRead"></param>
        public StringReader(string toRead)
        {
            _str = toRead;
            _pos = 0;
        }

        /// <summary>
        /// Read a line
        /// </summary>
        /// <returns>The line of an empty string</returns>
        public string ReadLine()
        {
            var nl = _str.IndexOf(NewLine, _pos);
            if (nl == -1)
            {
                return string.Empty;
            }

            var toReturn = _str.Substring(_pos, nl - _pos);
            _pos = nl + NewLine.Length;

            return toReturn;
        }

        public void Dispose()
        {
            // Nothing to do
        }
    }
}
