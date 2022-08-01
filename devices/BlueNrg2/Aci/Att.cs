// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.BlueNrg2.Aci
{
    /// <summary>
    /// class containing all the Att commands.
    /// </summary>
    public class Att
    {
        private readonly TransportLayer _transportLayer;

        internal Att(TransportLayer transportLayer)
        {
            _transportLayer = transportLayer;
        }

        public BleStatus FindInformationRequest(ushort connectionHandle, ushort startHandle, ushort endHandle)
        {
            throw new NotImplementedException();
        }

        public BleStatus FindByTypeValueRequest(
            ushort connectionHandle,
            ushort startHandle,
            ushort endHandle,
            ushort uuid,
            byte attributeValueLength,
            byte[] attributeValue)
        {
            throw new NotImplementedException();
        }

        public BleStatus ReadByTypeRequest(ushort connectionHandle, ushort startHandle, ushort endHandle, UuidType uuidType, ref byte[] uuid)
        {
            throw new NotImplementedException();
        }

        public BleStatus ReadByGroupTypeRequest(ushort connectionHandle, ushort startHandle, ushort endHandle, UuidType uuidType, ref byte[] uuid)
        {
            throw new NotImplementedException();
        }

        public BleStatus PrepareWriteRequest(
            ushort connectionHandle,
            ushort attributeHandle,
            ushort valueOffset,
            byte attributeValueLength,
            byte[] attributeValue)
        {
            throw new NotImplementedException();
        }

        /// <param name="connectionHandle">identifies the connection written to</param>
        /// <param name="executeAllWrites">
        ///     If set to true, all pending prepared values are written immediately, if set to false all
        ///     prepared writes will be cancelled.
        /// </param>
        public BleStatus ExecuteWriteRequest(ushort connectionHandle, bool executeAllWrites)
        {
            throw new NotImplementedException();
        }
    }
}
