// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace nanoFramework.M2Mqtt
{
    /// <summary>
    /// Supported SSL/TLS protocol versions.
    /// </summary>
    public enum MqttSslProtocols
    {
        /// <summary>
        /// None.
        /// </summary>
        None,

        /// <summary>
        /// SSL version 3.
        /// </summary>
        SSLv3,

        /// <summary>
        /// TLS version 1.0.
        /// </summary>
        TLSv1_0,

        /// <summary>
        /// TLS version 1.1.
        /// </summary>
        TLSv1_1,

        /// <summary>
        /// TLS version 1.2.
        /// </summary>
        TLSv1_2,

        /// <summary>
        /// TLS version 1.3.
        /// </summary>
        TLSv1_3
    }
}