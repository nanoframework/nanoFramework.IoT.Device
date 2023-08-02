// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using IoT.Device.AtModem.DTOs;
using IoT.Device.AtModem.Modem;

namespace IoT.Device.AtModem.Network
{
    /// <summary>
    /// A network interface for SIM800.
    /// </summary>
    public class Sim800Network : INetwork
    {
        private readonly ModemBase _modem;

        internal Sim800Network(ModemBase modem)
        {
            _modem = modem;
        }

        /// <inheritdoc/>
        public NetworkInformation NetworkInformation => throw new NotImplementedException();

        /// <inheritdoc/>
        public bool IsConnected => throw new NotImplementedException();

        /// <inheritdoc/>
        public bool Connect(PersonalIdentificationNumber pin = null, AccessPointConfiguration apn = null, int maxRetry = 10)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public bool Disconnect()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Operator[] GetOperators()
        {
            throw new NotImplementedException();
        }
    }
}
