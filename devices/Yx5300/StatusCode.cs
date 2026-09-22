// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Yx5300
{
    /// <summary>
    /// Yx5300 MP3 player.
    /// </summary>
    public partial class Yx5300
    {
        /// <summary>
        /// Status code.
        /// </summary>
        public enum StatusCode
        {
            /// <summary>No error (library generated status).</summary>
            NoError = 0x00,

            /// <summary>Timeout on response message (library generated status).</summary>
            Timeout = 0x01,

            /// <summary>Wrong version number in return message (library generated status).</summary>
            Version = 0x02,

            /// <summary>Device checksum invalid (library generated status).</summary>
            Checksum = 0x03,

            /// <summary>TF card was inserted (unsolicited).</summary>
            CardInserted = 0x3a,

            /// <summary>TF card was removed (unsolicited).</summary>
            CardRemoved = 0x3b,

            /// <summary>Track/file has ended (unsolicited).</summary>
            EndOfFile = 0x3d,

            /// <summary>Initialization complete (unsolicited).</summary>
            InitializationComplete = 0x3f,

            /// <summary>Error file not found.</summary>
            FileNotFound = 0x40,

            /// <summary>Message acknowledged ok.</summary>
            AcknoledgeOk = 0x41,

            /// <summary>Current status.</summary>
            Status = 0x42,

            /// <summary>Current volume level.</summary>
            Volume = 0x43,

            /// <summary>Equalizer status.</summary>
            Equalizer = 0x44,

            /// <summary>TF total file count.</summary>
            TotalFileCount = 0x48,

            /// <summary>Current file playing.</summary>
            Playing = 0x4c,

            /// <summary>Total number of files in the folder.</summary>
            NumberOfFilesInFolder = 0x4e,

            /// <summary>Total number of folders.</summary>
            TotalNumberOfFiles = 0x4f
        }
    }
}