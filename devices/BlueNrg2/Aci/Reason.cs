// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.BlueNrg2.Aci
{
    /// <summary>
    /// Reason for behaviour.
    /// </summary>
    public enum Reason : byte
    {
        /// <summary>
        /// Authentication failure.
        /// </summary>
        AuthenticationFailure = 0x05,

        /// <summary>
        /// Remote user terminated the connection.
        /// </summary>
        RemoteUserTerminatedConnection = 0x13,

        /// <summary>
        /// Remote device terminated the connection due to low resources. (eg. battery life etc.)
        /// </summary>
        RemoteDeviceLowResources = 0x14,

        /// <summary>
        /// Remote device terminated the connection due to it powering off.
        /// </summary>
        RemoteDevicePowerOff = 0x15,

        /// <summary>
        /// Unsupported remote feature.
        /// </summary>
        UnsupportedRemoteFeature = 0x1A,

        /// <summary>
        /// Unacceptable connection parameters.
        /// </summary>
        UnacceptableConnectionParameters = 0x3B
    }
}
