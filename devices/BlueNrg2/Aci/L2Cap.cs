// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.BlueNrg2.Aci
{
	public class L2Cap
	{
		private readonly TransportLayer _transportLayer;

		internal L2Cap(TransportLayer transportLayer)
		{
			_transportLayer = transportLayer;
		}

		public BleStatus ConnectionParameterUpdateRequest(
			ushort connectionHandle,
			ushort minimumConnectionInterval,
			ushort maximumConnectionInterval,
			ushort slaveLatency,
			ushort timeoutMultiplier)
		{
			var command = new byte[10];
			BitConverter.GetBytes(connectionHandle).CopyTo(command, 0);
			BitConverter.GetBytes(minimumConnectionInterval).CopyTo(command, 2);
			BitConverter.GetBytes(maximumConnectionInterval).CopyTo(command, 4);
			BitConverter.GetBytes(slaveLatency).CopyTo(command, 6);
			BitConverter.GetBytes(timeoutMultiplier).CopyTo(command, 8);

			var status = new byte[1];
			var rq = new Request
			{
				OpCodeGroup = 0x3f,
				OpCodeCommand = 0x181,
				Event = 0x0F,
				CommandParameter = command,
				CommandLength = 10,
				ResponseParameter = status,
				ResponseLength = 1
			};
			if (_transportLayer.SendRequest(ref rq, false) < 0)
				return BleStatus.Timeout;
			return status[0] != 0 ? (BleStatus)status[0] : BleStatus.Success;
		}

		public BleStatus ConnectionParameterUpdateResponse(
			ushort connectionHandle,
			ushort minimumConnectionInterval,
			ushort maximumConnectionInterval,
			ushort slaveLatency,
			ushort timeoutMultiplier,
			ushort minimumConnectionLength,
			ushort maximumConnectionLength,
			byte identifier,
			bool accept
		)
		{
			var command = new byte[16];
			BitConverter.GetBytes(connectionHandle).CopyTo(command, 0);
			BitConverter.GetBytes(minimumConnectionInterval).CopyTo(command, 2);
			BitConverter.GetBytes(maximumConnectionInterval).CopyTo(command, 4);
			BitConverter.GetBytes(slaveLatency).CopyTo(command, 6);
			BitConverter.GetBytes(timeoutMultiplier).CopyTo(command, 8);
			BitConverter.GetBytes(minimumConnectionLength).CopyTo(command, 10);
			BitConverter.GetBytes(maximumConnectionLength).CopyTo(command, 12);
			command[14] = identifier;
			command[15] = (byte)(accept ? 0x01 : 0x00);

			var status = new byte[1];
			var rq = new Request
			{
				OpCodeGroup = 0x3f,
				OpCodeCommand = 0x182,
				CommandParameter = command,
				CommandLength = 16,
				ResponseParameter = status,
				ResponseLength = 1
			};
			if (_transportLayer.SendRequest(ref rq, false) < 0)
				return BleStatus.Timeout;
			return status[0] != 0 ? (BleStatus)status[0] : BleStatus.Success;
		}
	}
}
