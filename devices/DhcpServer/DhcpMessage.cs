// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.DhcpServer.Enums;
using Iot.Device.DhcpServer.Options;
using System;
using System.Collections;
using System.Net;
using System.Text;

namespace Iot.Device.DhcpServer
{
    /// <summary>
    /// DHCP Message class.
    /// </summary>
    public class DhcpMessage
    {
        /// <summary>
        /// Gets or sets the operation Code.
        /// </summary>
        public DhcpOperation OperationCode { get; set; }

        /// <summary>
        /// Gets or setsthe hardware type.
        /// </summary>
        public HardwareType HardwareType { get; set; }

        /// <summary>
        /// Gets or sets the hardware address lenght.
        /// </summary>
        public byte HardwareAddressLength { get; set; }

        /// <summary>
        /// Gets or sets the hops.
        /// </summary>
        public byte Hops { get; set; }

        /// <summary>
        /// Gets or sets the transaction ID.
        /// </summary>
        public uint TransactionId { get; set; }

        /// <summary>
        /// Gets or sets the seconds elapsed.
        /// </summary>
        public ushort SecondsElapsed { get; set; }

        /// <summary>
        /// Gets or sets the Flags.
        /// </summary>
        public ushort Flags { get; set; }

        /// <summary>
        /// Gets or sets the client IP addres.
        /// </summary>
        public IPAddress ClientIPAddress { get; set; } = new IPAddress(0);

        /// <summary>
        /// Gets or sets your server IP address.
        /// </summary>
        public IPAddress YourIPAddress { get; set; } = new IPAddress(0);

        /// <summary>
        /// Gets or sets the server IP address.
        /// </summary>
        public IPAddress ServerIPAddress { get; set; } = new IPAddress(0);

        /// <summary>
        /// Gets or sets the gateway IP address.
        /// </summary>
        public IPAddress GatewayIPAddress { get; set; } = new IPAddress(0);

        /// <summary>
        /// Gets or sets the client hardware address.
        /// </summary>
        public byte[] ClientHardwareAddress { get; set; }

        /// <summary>
        /// Gets or sets the magic cookie.
        /// </summary>
        /// <remarks>
        /// The first four octets of the 'options' field of the DHCP message
        /// contain the (decimal) values 99, 130, 83 and 99, respectively (this
        /// is the same magic cookie as is defined in RFC 1497 [17]).
        /// </remarks>
        public byte[] MagicCookie { get; init; } = new byte[4];

        /// <summary>
        /// Gets or sets the client hardware address.
        /// </summary>
        public string ClientHardwareAddressAsString => BitConverter.ToString(ClientHardwareAddress);

        /// <summary>
        /// Gets or sets the magic cookie.
        /// </summary>
        public byte[] Cookie { get; set; }

        /// <summary>
        /// Gets or sets the options.
        /// </summary>
        public OptionCollection Options { get; set; } = new();

        /// <summary>
        /// Gets the message type.
        /// </summary>
        public DhcpMessageType DhcpMessageType => ((MessageTypeOption)Options.Get(DhcpOptionCode.DhcpMessageType)!).Deserialize();

        /// <summary>
        /// Gets the host name.
        /// </summary>
        public string HostName => Options.GetOrDefault(DhcpOptionCode.HostName, string.Empty);

        /// <summary>
        /// Gets the request IP aAddress.
        /// </summary>
        public IPAddress RequestedIpAddress => Options.GetOrDefault(DhcpOptionCode.RequestedIpAddress, IPAddress.Any);

        /// <summary>
        /// Gets the DHCP server addres.
        /// </summary>
        public IPAddress ServerIdentifier => Options.GetOrDefault(DhcpOptionCode.ServerIdentifier, IPAddress.Any);

        /// <summary>
        /// Converts this <see cref="DhcpMessage"/> to a <see cref="T:byte[]"/>.
        /// </summary>
        public byte[] GetBytes()
        {
            var data = new byte[MessageIndex.Options + Options.Length];

            Converter.CopyTo((byte)OperationCode, data, MessageIndex.Operation);
            Converter.CopyTo((byte)HardwareType, data, MessageIndex.HardwareAddressType);
            Converter.CopyTo(HardwareAddressLength, data, MessageIndex.HardwareAddressLength);
            Converter.CopyTo(Hops, data, MessageIndex.Hops);
            Converter.CopyTo(TransactionId, data, MessageIndex.TransactionId);
            Converter.CopyTo(SecondsElapsed, data, MessageIndex.SecondsElapsed);
            Converter.CopyTo(Flags, data, MessageIndex.Flags);
            Converter.CopyTo(ClientIPAddress, data, MessageIndex.ClientIPAddress);
            Converter.CopyTo(YourIPAddress, data, MessageIndex.YourIPAddress);
            Converter.CopyTo(ServerIPAddress, data, MessageIndex.ServerIPAddress);
            Converter.CopyTo(GatewayIPAddress, data, MessageIndex.GatewayIPAddress);
            Converter.CopyTo(ClientHardwareAddress, data, MessageIndex.HardwareAddress);
            Converter.CopyTo(MagicCookie, data, MessageIndex.MagicCookie);
            Converter.CopyTo(Options.GetBytes(), data, MessageIndex.Options);

            return data;
        }

#if DEBUG

        /// <inheritdoc/>
        public override string ToString()
        {
            // output the message in a readable format
            StringBuilder messageOutput = new StringBuilder();
            messageOutput.AppendLine($"DHCP Message {DhcpMessageType.AsString()}");
            messageOutput.AppendLine($"HwType: {HardwareType}, HwLen: {HardwareAddressLength}, Hops: {Hops}, XID: {TransactionId}, SECS: {SecondsElapsed}, FLAGS: {Flags}");
            messageOutput.AppendLine($"CIADDR: {ClientIPAddress}, YIADDR: {YourIPAddress}, SIADDR: {ServerIPAddress}, GIADDR: {GatewayIPAddress}");
            messageOutput.AppendLine($"CHADDR: {ClientHardwareAddressAsString}");
            messageOutput.AppendLine("Options:");

            if (Options.Length == 0)
            {
                messageOutput.AppendLine(" No options");
            }
            else
            {
                // List all options using the enumerator
                IEnumerator optionEnum = Options.GetEnumerator();
                while (optionEnum.MoveNext())
                {
                    if (optionEnum.Current is IOption option)
                    {
                        ComposeOptionOutput(option, messageOutput);
                    }
                }
            }

            return messageOutput.ToString();
        }

        private static void ComposeOptionOutput(
            IOption option,
            StringBuilder stringBuilder)
        {
            var optionCode = option.Code;
            var length = option.Length;
            var optionValue = option.Data;

            if (optionCode == DhcpOptionCode.HostName)
            {
                stringBuilder.AppendLine($"  Host Name: {Encoding.UTF8.GetString(optionValue, 0, length)}");
            }
            else if (optionCode == DhcpOptionCode.RequestedIpAddress && length == 4)
            {
                stringBuilder.AppendLine($"  Requested IP Address: {new IPAddress(optionValue)}");
            }
            else if (optionCode == DhcpOptionCode.ServerIdentifier && length == 4)
            {
                stringBuilder.AppendLine($"  DHCP Server IP Address: {new IPAddress(optionValue)}");
            }
            else if (optionCode == DhcpOptionCode.DhcpMaxMessageSize && length == 2)
            {
                ushort maxSize = BitConverter.ToUInt16(optionValue, 0);
                stringBuilder.AppendLine($"  DHCP Max Message Size: {maxSize} bytes");
            }
            else if (optionCode == DhcpOptionCode.ClassId && length > 0)
            {
                stringBuilder.AppendLine($"  Class ID: {Encoding.UTF8.GetString(optionValue, 0, length)}");
            }
            else if (optionCode == DhcpOptionCode.ClientId && length > 0)
            {
                stringBuilder.Append("  Client ID: ");
                stringBuilder.AppendLine(BitConverter.ToString(optionValue));
            }
            else if (optionCode == DhcpOptionCode.ParameterList && length > 0)
            {
                stringBuilder.Append("  Parameter List: ");
                for (int i = 0; i < length; i++)
                {
                    if (i > 0)
                    {
                        stringBuilder.Append(", ");
                    }

                    DhcpOptionCode paramCode = (DhcpOptionCode)optionValue[i];
                    stringBuilder.Append(paramCode.AsString());
                }

                stringBuilder.AppendLine();
            }
            else if (optionCode == DhcpOptionCode.CaptivePortal && length > 0)
            {
                stringBuilder.AppendLine($"  Captive Portal URL: {Encoding.UTF8.GetString(optionValue, 0, length)}");
            }
            else if (optionCode == DhcpOptionCode.NamingAuthority)
            {
                stringBuilder.AppendLine($"  Naming Authority: {Encoding.UTF8.GetString(optionValue, 0, length)}");
            }
            else if (optionCode == DhcpOptionCode.DhcpMessageType)
            {
                // these options won't be added to the output as they are already shown in the header
            }
            else
            {
                // For other options, just show the raw value
                stringBuilder.AppendLine($"  Option ({optionCode}) Value: {BitConverter.ToString(optionValue)}");
            }
        }

#endif

    }
}
