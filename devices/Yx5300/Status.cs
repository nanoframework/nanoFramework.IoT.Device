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
        /// Class containing status data.
        /// </summary>
        public class Status
        {
            /// <summary>
            /// Gets or sets the status code.
            /// </summary>
            public StatusCode Code { get; set; }

            /// <summary>
            /// Gets or sets the associated data.
            /// </summary>
            public ushort Data { get; set; }
        }
    }
}