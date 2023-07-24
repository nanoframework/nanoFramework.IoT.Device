// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace IoT.Device.AtModem.DTOs
{
    /// <summary>
    /// Configuration of GPRS access point (APN).
    /// </summary>
    public class AccessPointConfiguration
    {
        /// <summary>
        /// Gets the Access Point name.
        /// </summary>
        public string AccessPointName { get; }

        /// <summary>
        /// Gets the User name. Null if not used.
        /// </summary>
        public string UserName { get; }

        /// <summary>
        /// Gets the Password. Null if not used.
        /// </summary>
        public string Password { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AccessPointConfiguration"/> class.
        /// </summary>
        /// <param name="apn">Access Point name.</param>
        public AccessPointConfiguration(string apn)
        {
            AccessPointName = apn;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AccessPointConfiguration"/> class.
        /// </summary>
        /// <param name="apn">Access Point name.</param>
        /// <param name="userName">User name.</param>
        /// <param name="password">Password.</param>
        public AccessPointConfiguration(string apn, string userName, string password)
        {
            AccessPointName = apn;
            UserName = userName;
            Password = password;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AccessPointConfiguration"/> class.
        /// </summary>
        public AccessPointConfiguration()
        { 
        }
    }
}