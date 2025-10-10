// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.DhcpServer.Enums;
using Iot.Device.DhcpServer.Options;
using System.Net;

namespace Iot.Device.DhcpServer
{
    internal class MessageBuilder
    {
        public static DhcpMessage CreateAck(
            DhcpMessage request,
            IPAddress serverIdentifier,
            IPAddress yourIPAddress,
            IPAddress subnetMask,
            OptionCollection? options = null)
        {
            return CreateResponse(
                request,
                DhcpMessageType.Ack,
                serverIdentifier,
                yourIPAddress,
                subnetMask,
                options);
        }

        public static DhcpMessage CreateNak(
            DhcpMessage request,
            IPAddress serverIdentifier)
        {
            return CreateResponse(
                request,
                DhcpMessageType.Nak,
                serverIdentifier,
                IPAddress.Any);
        }

        public static DhcpMessage CreateOffer(
            DhcpMessage request,
            IPAddress serverIdentifier,
            IPAddress yourIPAddress,
            IPAddress subnetMask,
            OptionCollection? options = null)
        {
            return CreateResponse(
                request,
                DhcpMessageType.Offer,
                serverIdentifier,
                yourIPAddress,
                subnetMask,
                options);
        }

        /// <summary>
        /// Create a response <see cref="DhcpMessage"/> from a request. Only <see cref="DhcpOptionCode.DhcpMessageType"/> and <see cref="DhcpOptionCode.ServerIdentifier"/> options are set.
        /// </summary>
        /// <param name="request">The request we are responding to.</param>
        /// <param name="responseType">The response <see cref="DhcpMessageType"/>.</param>
        /// <param name="serverIdentifier">The <see cref="IPAddress"/> of the server.</param>
        /// <param name="yourIPAddress">The <see cref="IPAddress"/> of the client.</param>
        /// <remarks>If the <paramref name="responseType"/> is <see cref="DhcpMessageType.Nak"/> then <paramref name="yourIPAddress"/> is ignored and will be set it <see cref="IPAddress.Any"/>.</remarks>
        private static DhcpMessage CreateResponse(
            DhcpMessage request,
            DhcpMessageType responseType,
            IPAddress serverIdentifier,
            IPAddress yourIPAddress)
        {
            var message = new DhcpMessage
            {
                OperationCode = DhcpOperation.BootReply,
                HardwareType = request.HardwareType,
                HardwareAddressLength = request.HardwareAddressLength,
                Hops = 0,
                TransactionId = request.TransactionId,
                SecondsElapsed = 0,
                Flags = request.Flags,
                ClientIPAddress = IPAddress.Any,
                YourIPAddress = DhcpMessageType.Nak == responseType ? IPAddress.Any : yourIPAddress,
                ServerIPAddress = IPAddress.Any,
                GatewayIPAddress = request.GatewayIPAddress,
                ClientHardwareAddress = request.ClientHardwareAddress,
                MagicCookie = request.MagicCookie,
            };

            message.Options.Add(new MessageTypeOption(responseType));
            message.Options.Add(new IPAddressOption(DhcpOptionCode.ServerIdentifier, serverIdentifier));

            return message;
        }

        private static DhcpMessage CreateResponse(
            DhcpMessage request,
            DhcpMessageType responseType,
            IPAddress serverIdentifier,
            IPAddress yourIPAddress,
            IPAddress subnetMask,
            OptionCollection? options = null)
        {
            var message = CreateResponse(request, responseType, serverIdentifier, yourIPAddress);
            var requestType = request.DhcpMessageType;

            message.Options.Add(new IPAddressOption(DhcpOptionCode.SubnetMask, subnetMask));

            if (options is not null)
            {
                foreach (var optionObject in options)
                {
                    if (optionObject is not IOption option)
                    {
                        continue;
                    }

                    if (!message.Options.Contains(option.Code) && IsOptionAllowedInResponse(requestType, responseType, option))
                    {
                        message.Options.Add(option);
                    }
                }
            }

            return message;
        }

        /// <summary>
        /// Checks if a given <see cref="IOption"/> is allowed in a response based on <paramref name="requestType"/> and/or <paramref name="responseType"/>.
        /// </summary>
        /// <param name="requestType">The request message type.</param>
        /// <param name="responseType">The response message type.</param>
        /// <param name="option">The option to check.</param>
        /// <returns><see langword="true"/> if the option is allowed; otherwise, <see langword="false"/>.</returns>
        private static bool IsOptionAllowedInResponse(
            DhcpMessageType requestType,
            DhcpMessageType responseType,
            IOption option)
        {
            return option.Code switch
            {
                DhcpOptionCode.ClassId => true,
                DhcpOptionCode.ClientId => DhcpMessageType.Nak == responseType,
                DhcpOptionCode.DhcpMessageType => true,
                DhcpOptionCode.LeaseTime => DhcpMessageType.Nak != responseType && DhcpMessageType.Inform != requestType,
                DhcpOptionCode.DhcpMaxMessageSize => false,
                DhcpOptionCode.ParameterList => false,
                DhcpOptionCode.ServerIdentifier => true,
                DhcpOptionCode.RequestedIpAddress => false,
                _ => DhcpMessageType.Nak != responseType
            };
        }

        public static DhcpMessage Parse(byte[] data)
        {
            var message = new DhcpMessage
            {
                OperationCode = (DhcpOperation)data[MessageIndex.Operation],
                HardwareType = (HardwareType)data[MessageIndex.HardwareAddressType],
                HardwareAddressLength = data[MessageIndex.HardwareAddressLength],
                Hops = data[MessageIndex.Hops],
                TransactionId = Converter.GetUInt32(data, MessageIndex.TransactionId),
                SecondsElapsed = Converter.GetUInt16(data, MessageIndex.SecondsElapsed),
                Flags = Converter.GetUInt16(data, MessageIndex.Flags),
                ClientIPAddress = Converter.GetIPAddress(data, MessageIndex.ClientIPAddress),
                YourIPAddress = Converter.GetIPAddress(data, MessageIndex.YourIPAddress),
                ServerIPAddress = Converter.GetIPAddress(data, MessageIndex.ServerIPAddress),
                GatewayIPAddress = Converter.GetIPAddress(data, MessageIndex.GatewayIPAddress),
                ClientHardwareAddress = new byte[data[MessageIndex.HardwareAddressLength]],
                Options = OptionCollection.Parse(data)
            };

            Converter.CopyTo(data, MessageIndex.HardwareAddress, message.ClientHardwareAddress, 0, message.ClientHardwareAddress.Length);
            Converter.CopyTo(data, MessageIndex.MagicCookie, message.MagicCookie, 0, message.MagicCookie.Length);

            return message;
        }
    }
}
