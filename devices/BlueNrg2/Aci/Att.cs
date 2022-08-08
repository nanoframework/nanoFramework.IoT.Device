// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Iot.Device.BlueNrg2.Aci.Events;

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

        /// <summary>
        /// Send a Find Information Request. This command is used to obtain the mapping of attribute
        /// handles with their associated types. The responses of the procedure are given through the
        /// <see cref="EventProcessor.AttFindInfoResponseEvent"/> event. The end of the procedure is
        /// indicated by a <see cref="EventProcessor.GattProcessCompleteEvent"/> event.
        /// </summary>
        /// <param name="connectionHandle">Connection handle that identifies the connection.</param>
        /// <param name="startHandle">First requested handle number.</param>
        /// <param name="endHandle">Last requested handle number.</param>
        /// <returns>Value indicating success or error code.</returns>
        public BleStatus FindInformationRequest(ushort connectionHandle, ushort startHandle, ushort endHandle)
        {
            var ptr = 0;
            var command = new byte[258];
            BitConverter.GetBytes(connectionHandle).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(startHandle).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(endHandle).CopyTo(command, ptr);
            ptr += 2;

            const uint responseLength = 1;
            var response = new byte[responseLength];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x10c,
                Event = 0x0F,
                CommandParameter = command,
                CommandLength = (uint)ptr,
                ResponseParameter = response,
                ResponseLength = responseLength
            };

            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus)response[0];
            return BleStatus.Success;
        }

        /// <summary>
        /// Send a Find By Type Value Request The Find By Type Value Request is
        /// used to obtain the handles of attributes that have a given 16-bit UUID
        /// attribute type and a given attribute value. The responses of the
        /// procedure are given through the <see cref="EventProcessor.AttFindByTypeValueResponseEvent"/>
        /// event. The end of the procedure is indicated by a <see cref="EventProcessor.GattProcessCompleteEvent"/> event.
        /// </summary>
        /// <param name="connectionHandle">Connection handle that identifies the connection.</param>
        /// <param name="startHandle">First requested handle number.</param>
        /// <param name="endHandle">Last requested handle number.</param>
        /// <param name="uuid">2 octet UUID to find (little-endian).</param>
        /// <param name="attributeValueLength">Length of attribute value (maximum value is ATT_MTU - 7).</param>
        /// <param name="attributeValue">Attribute value to find.</param>
        /// <returns>Value indicating success or error code.</returns>
        /// <exception cref="ArgumentException"><see cref="attributeValue"/> should have the length indicated by <see cref="attributeValueLength"/></exception>
        public BleStatus FindByTypeValueRequest(
            ushort connectionHandle,
            ushort startHandle,
            ushort endHandle,
            ushort uuid,
            byte attributeValueLength,
            byte[] attributeValue)
        {
            if (attributeValue is null || attributeValue.Length != attributeValueLength)
                throw new ArgumentException();

            var ptr = 0;
            var command = new byte[258];
            BitConverter.GetBytes(connectionHandle).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(startHandle).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(endHandle).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(uuid).CopyTo(command, ptr);
            ptr += 2;
            command[ptr] = attributeValueLength;
            ptr += 1;
            attributeValue.CopyTo(command, ptr);
            ptr += attributeValueLength;

            const uint responseLength = 1;
            var response = new byte[responseLength];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x10d,
                Event = 0x0F,
                CommandParameter = command,
                CommandLength = (uint)ptr,
                ResponseParameter = response,
                ResponseLength = responseLength
            };

            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus)response[0];
            return BleStatus.Success;
        }

        /// <summary>
        /// Send a Read By Type Request. The Read By Type Request is used to
        /// obtain the values of attributes where the attribute type is known but
        /// the handle is not known. The responses of the procedure are given
        /// through the <see cref="EventProcessor.AttReadByTypeResponseEvent"/> event. The end of the
        /// procedure is indicated by a <see cref="EventProcessor.GattProcessCompleteEvent"/> event.
        /// </summary>
        /// <param name="connectionHandle">Connection handle that identifies the connection.</param>
        /// <param name="startHandle">First requested handle number.</param>
        /// <param name="endHandle">Last requested handle number.</param>
        /// <param name="uuidType">UUID type.</param>
        /// <param name="uuid">The UUID to read from.</param>
        /// <returns>Value indicating success or error code.</returns>
        /// <exception cref="ArgumentException"><see cref="uuid"/> has to be of length as determined by <see cref="UuidType"/></exception>
        public BleStatus ReadByTypeRequest(ushort connectionHandle, ushort startHandle, ushort endHandle, UuidType uuidType, byte[] uuid)
        {
            var uuidLength = uuidType == UuidType.Uuid16 ? 2 : 16;

            if (uuid is null || uuid.Length != uuidLength)
                throw new ArgumentException();

            var ptr = 0;
            var command = new byte[258];
            BitConverter.GetBytes(connectionHandle).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(startHandle).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(endHandle).CopyTo(command, ptr);
            ptr += 2;
            command[ptr] = (byte)uuidType;
            ptr += 1;
            uuid.CopyTo(command, ptr);
            ptr += uuidLength;

            const uint responseLength = 1;
            var response = new byte[responseLength];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x10e,
                Event = 0x0F,
                CommandParameter = command,
                CommandLength = (uint)ptr,
                ResponseParameter = response,
                ResponseLength = responseLength
            };

            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus)response[0];
            return BleStatus.Success;
        }

        /// <summary>
        /// Send a Read By Group Type Request.  The Read By Group Type Request is
        /// used to obtain the values of grouping attributes where the attribute
        /// type is known but the handle is not known. Grouping attributes are
        /// defined  at GATT layer. The grouping attribute types are: "Primary
        /// Service", "Secondary Service"  and "Characteristic".  The responses of
        /// the procedure are given through the <see cref="EventProcessor.AttReadByGroupTypeResponseEvent"/> event.
        /// The end of the procedure is indicated by a <see cref="EventProcessor.GattProcessCompleteEvent"/> event.
        /// </summary>
        /// <param name="connectionHandle">Connection handle that identifies the connection.</param>
        /// <param name="startHandle">First requested handle number.</param>
        /// <param name="endHandle">Last requested handle number.</param>
        /// <param name="uuidType">UUID type.</param>
        /// <param name="uuid">The UUID to read from.</param>
        /// <returns>Value indicating success or error code.</returns>
        /// <exception cref="ArgumentException"><see cref="uuid"/> has to be of length as determined by <see cref="UuidType"/></exception>
        public BleStatus ReadByGroupTypeRequest(ushort connectionHandle, ushort startHandle, ushort endHandle, UuidType uuidType, byte[] uuid)
        {
            var uuidLength = uuidType == UuidType.Uuid16 ? 2 : 16;

            if (uuid is null || uuid.Length != uuidLength)
                throw new ArgumentException();

            var ptr = 0;
            var command = new byte[258];
            BitConverter.GetBytes(connectionHandle).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(startHandle).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(endHandle).CopyTo(command, ptr);
            ptr += 2;
            command[ptr] = (byte)uuidType;
            ptr += 1;
            uuid.CopyTo(command, ptr);
            ptr += uuidLength;

            const uint responseLength = 1;
            var response = new byte[responseLength];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x10f,
                Event = 0x0F,
                CommandParameter = command,
                CommandLength = (uint)ptr,
                ResponseParameter = response,
                ResponseLength = responseLength
            };

            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus)response[0];
            return BleStatus.Success;
        }

        /// <summary>
        /// Send a Prepare Write Request. The Prepare Write Request is used to
        /// request the server to prepare to write the value of an attribute.  The
        /// responses of the procedure are given through the <see cref="EventProcessor.AttPrepareWriteResponseEvent"/>
        /// event. The end of the procedure is indicated by a <see cref="EventProcessor.GattProcessCompleteEvent"/>.
        /// </summary>
        /// <param name="connectionHandle">Connection handle that identifies the connection.</param>
        /// <param name="attributeHandle">Handle of the attribute to be written.</param>
        /// <param name="valueOffset">The offset of the first octet to be written.</param>
        /// <param name="attributeValueLength">Length of attribute value (maximum value is ATT_MTU - 5).</param>
        /// <param name="attributeValue">The value of the attribute to be written.</param>
        /// <returns>Value indicating success or error code.</returns>
        /// <exception cref="ArgumentException"><see cref="attributeValue"/> should have the length indicated by <see cref="attributeValueLength"/></exception>
        public BleStatus PrepareWriteRequest(
            ushort connectionHandle,
            ushort attributeHandle,
            ushort valueOffset,
            byte attributeValueLength,
            byte[] attributeValue)
        {
            if (attributeValue is null || attributeValue.Length != attributeValueLength)
                throw new ArgumentException();

            var ptr = 0;
            var command = new byte[258];
            BitConverter.GetBytes(connectionHandle).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(attributeHandle).CopyTo(command, ptr);
            ptr += 2;
            BitConverter.GetBytes(valueOffset).CopyTo(command, ptr);
            ptr += 2;
            command[ptr] = attributeValueLength;
            ptr += 1;
            attributeValue.CopyTo(command, ptr);
            ptr += attributeValueLength;

            const uint responseLength = 1;
            var response = new byte[responseLength];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x110,
                Event = 0x0F,
                CommandParameter = command,
                CommandLength = (uint)ptr,
                ResponseParameter = response,
                ResponseLength = responseLength
            };

            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus)response[0];
            return BleStatus.Success;
        }

        /// <summary>
        /// Send an Execute Write Request. The Execute Write Request is used to
        /// request the server to write or cancel the write  of all the prepared
        /// values currently held in the prepare queue from this client.  The
        /// result of the procedure is given through the <see cref="EventProcessor.AttExecuteWriteResponseEvent"/>
        /// event.  The end of the procedure is indicated by a <see cref="EventProcessor.GattProcessCompleteEvent"/> event.
        /// </summary>
        /// <param name="connectionHandle">identifies the connection written to</param>
        /// <param name="executeAllWrites">If set to true, all pending prepared values are written immediately,
        /// if set to false all prepared writes will be cancelled.
        /// </param>
        /// <returns>Value indicating success or error code.</returns>
        public BleStatus ExecuteWriteRequest(ushort connectionHandle, bool executeAllWrites)
        {
            var ptr = 0;
            var command = new byte[258];
            BitConverter.GetBytes(connectionHandle).CopyTo(command, ptr);
            ptr += 2;
            command[ptr] = (byte)(executeAllWrites ? 0x01 : 0x00);

            const uint responseLength = 1;
            var response = new byte[responseLength];
            var rq = new Request
            {
                OpCodeGroup = 0x3f,
                OpCodeCommand = 0x111,
                Event = 0x0F,
                CommandParameter = command,
                CommandLength = (uint)ptr,
                ResponseParameter = response,
                ResponseLength = responseLength
            };

            if (_transportLayer.SendRequest(ref rq, false) < 0)
                return BleStatus.Timeout;
            if (response[0] != 0)
                return (BleStatus)response[0];
            return BleStatus.Success;
        }
    }
}
