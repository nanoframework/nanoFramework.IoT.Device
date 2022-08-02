﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.Net;
using System.Text;
using Iot.Device.DhcpServer.Enums;

namespace Iot.Device.DhcpServer
{
    /// <summary>
    /// DHCP Message class.
    /// </summary>
    public class DhcpMessage
    {
        private const int DhcppacketSize = 300;
        private const int IndexToOptions = 240;

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
        public byte[] Cookie { get; set; }

        /// <summary>
        /// Gets or sets the options.
        /// </summary>
        public byte[] Options { get; set; }

        /// <summary>
        /// Gets the message type.
        /// </summary>
        public DhcpMessageType DhcpMessageType
        {
            get
            {
                if (IsOptionsInvalid())
                {
                    return DhcpMessageType.Unknown;
                }

                if (OptionsContainsKey(DhcpOptionCode.DhcpMessageType))
                {
                    var data = GetOption(DhcpOptionCode.DhcpMessageType)[0];
                    return (DhcpMessageType)data;
                }

                return DhcpMessageType.Unknown;
            }
        }

        /// <summary>
        /// Gets the host name.
        /// </summary>
        public string HostName
        {
            get
            {
                if (IsOptionsInvalid())
                {
                    return string.Empty;
                }

                if (OptionsContainsKey(DhcpOptionCode.Hostname))
                {
                    var data = GetOption(DhcpOptionCode.Hostname);
                    return Encoding.UTF8.GetString(data, 0, data.Length);
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// Gets the request IP aAddress.
        /// </summary>
        public IPAddress RequestedIpAddress
        {
            get
            {
                if (IsOptionsInvalid())
                {
                    return new IPAddress(0);
                }

                if (OptionsContainsKey(DhcpOptionCode.RequestedIpAddress))
                {
                    var data = GetOption(DhcpOptionCode.RequestedIpAddress);
                    return new IPAddress(data);
                }

                return new IPAddress(0);
            }
        }

        /// <summary>
        /// Gets the DHCP addres.
        /// </summary>
        public IPAddress DhcpAddress
        {
            get
            {
                if (IsOptionsInvalid())
                {
                    return new IPAddress(0);
                }

                if (OptionsContainsKey(DhcpOptionCode.DhcpAddress))
                {
                    var data = GetOption(DhcpOptionCode.DhcpAddress);
                    return new IPAddress(data);
                }

                return new IPAddress(0);
            }
        }

        /// <summary>
        /// Parses the message.
        /// </summary>
        /// <param name="dhcppacket">The byte array message.</param>
        public void Parse(ref byte[] dhcppacket)
        {
            // See the build function for details on a message.
            const int LongSize = 4;
            int inc = 0;
            OperationCode = (DhcpOperation)dhcppacket[0];
            HardwareType = (HardwareType)dhcppacket[1];
            HardwareAddressLength = dhcppacket[2];
            Hops = dhcppacket[3];
            inc += LongSize;
            TransactionId = BitConverter.ToUInt32(dhcppacket, inc);
            inc += LongSize;
            SecondsElapsed = BitConverter.ToUInt16(dhcppacket, inc);
            inc += 2;
            Flags = BitConverter.ToUInt16(dhcppacket, inc);
            inc += 2;
            ClientIPAddress = new IPAddress(BitConverter.GetBytes(BitConverter.ToUInt32(dhcppacket, inc)));
            inc += LongSize;
            YourIPAddress = new IPAddress(BitConverter.GetBytes(BitConverter.ToUInt32(dhcppacket, inc)));
            inc += LongSize;
            ServerIPAddress = new IPAddress(BitConverter.GetBytes(BitConverter.ToUInt32(dhcppacket, inc)));
            inc += LongSize;
            GatewayIPAddress = new IPAddress(BitConverter.GetBytes(BitConverter.ToUInt32(dhcppacket, inc)));
            inc += LongSize;
            ClientHardwareAddress = new byte[HardwareAddressLength];
            Array.Copy(dhcppacket, inc, ClientHardwareAddress, 0, HardwareAddressLength);
            Cookie = new byte[4];

            // We directly go to the magic cookie.
            inc = 236;
            Array.Copy(dhcppacket, inc, Cookie, 0, 4);

            // check copy options array
            inc = IndexToOptions;
            int offset = inc;
            if (dhcppacket[offset] != 0)
            {
                while (dhcppacket[offset] != 0xff)
                {
                    byte optcode = dhcppacket[offset++];
                    int optlen = dhcppacket[offset++];
                    offset += optlen;
                }

                Options = new byte[offset - inc + 1];
                Array.Copy(dhcppacket, inc, Options, 0, Options.Length);
            }
        }

        /// <summary>
        /// Build a message.
        /// </summary>
        /// <returns>The message as a byte array.</returns>
        public byte[] Build()
        {
            // Example of a discovery message
            // byte 0  byte 1  byte 2  byte 3
            // OP      HTYPE   HLEN    HOPS
            // 0x01    0x01    0x06    0x00
            // XID
            // 0x3903F326
            // SECS            FLAGS
            // 0x0000          0x0000
            // CIADDR(Client IP address)
            // 0x00000000
            // YIADDR(Your IP address)
            // 0x00000000
            // SIADDR(Server IP address)
            // 0x00000000
            // GIADDR(Gateway IP address)
            // 0x00000000
            // CHADDR(Client hardware address)
            // 0x00053C04
            // 0x8D590000
            // 0x00000000
            // 0x00000000
            // 192 octets of 0s, or overflow space for additional options; BOOTP legacy.
            // Magic cookie
            // 0x63825363
            // DHCP options
            // 0x350101 53: 1(DHCP Discover)
            // 0x3204c0a80164 50: 192.168.1.100 requested
            // 0x370401030f06 55(Parameter Request List):
            // - 1 (Request Subnet Mask),
            // - 3 (Router),
            // - 15 (Domain Name),
            // - 6 (Domain Name Server)
            // 0xff 255(Endmark)
            const int LongSize = 4;
            int inc = 0;
            byte[] dhcpPacket = new byte[DhcppacketSize];
            dhcpPacket[0] = (byte)OperationCode;
            dhcpPacket[1] = (byte)HardwareType;
            dhcpPacket[2] = HardwareAddressLength;
            dhcpPacket[3] = Hops;
            inc += LongSize;
            BitConverter.GetBytes(TransactionId).CopyTo(dhcpPacket, inc);
            inc += LongSize;
            BitConverter.GetBytes(SecondsElapsed).CopyTo(dhcpPacket, inc);

            // Only 2 bytes for the previous one
            inc += 2;
            BitConverter.GetBytes(Flags).CopyTo(dhcpPacket, inc);
            inc += 2;
            ClientIPAddress.GetAddressBytes().CopyTo(dhcpPacket, inc);
            inc += LongSize;
            YourIPAddress.GetAddressBytes().CopyTo(dhcpPacket, inc);
            inc += LongSize;
            ServerIPAddress.GetAddressBytes().CopyTo(dhcpPacket, inc);
            inc += LongSize;
            GatewayIPAddress.GetAddressBytes().CopyTo(dhcpPacket, inc);
            inc += LongSize;
            ClientHardwareAddress.CopyTo(dhcpPacket, inc);

            // We directly jump to the Magic cookie
            inc = 236;
            Cookie.CopyTo(dhcpPacket, inc);
            inc += LongSize;
            Options.CopyTo(dhcpPacket, inc);
            return dhcpPacket;
        }

        private byte[] BuildType(DhcpMessageType acktype, IPAddress cip, IPAddress mask, IPAddress sip, byte[] additionalOptions = null)
        {
            OperationCode = DhcpOperation.BootReply;
            YourIPAddress = cip;
            ResetOptions();
            AddOption(DhcpOptionCode.DhcpMessageType, new byte[] { (byte)acktype });
            if (acktype != DhcpMessageType.Nak)
            {
                AddOption(DhcpOptionCode.SubnetMask, mask.GetAddressBytes());
                AddOption(DhcpOptionCode.DhcpAddress, sip.GetAddressBytes());
            }

            if (additionalOptions != null)
            {
                AddOptionRaw(ref additionalOptions);
            }

            return Build();
        }

        /// <summary>
        /// Offer message.
        /// </summary>
        /// <param name="cip">Client IP addres..</param>
        /// <param name="mask">Network mask.</param>
        /// <param name="sip">Server IP address.</param>
        /// <param name="additionalOptions">Additional options to send.</param>
        /// <returns>A byte arry with the message.</returns>
        public byte[] Offer(IPAddress cip, IPAddress mask, IPAddress sip, byte[] additionalOptions = null) => BuildType(DhcpMessageType.Offer, cip, mask, sip, additionalOptions);

        /// <summary>
        /// Ackanoledge message.
        /// </summary>
        /// <param name="cip">Client IP addres..</param>
        /// <param name="mask">Network mask.</param>
        /// <param name="sip">Server IP address.</param>
        /// <param name="additionalOptions">Additional options to send.</param>
        /// <returns>A byte arry with the message.</returns>
        public byte[] Acknoledge(IPAddress cip, IPAddress mask, IPAddress sip, byte[] additionalOptions = null) => BuildType(DhcpMessageType.Ack, cip, mask, sip, additionalOptions);

        /// <summary>
        /// Not Ackanoledge message.
        /// </summary>
        /// <returns>A byte arry with the message.</returns>
        public byte[] NotAcknoledge()
        {
            YourIPAddress = new IPAddress(0);
            return BuildType(DhcpMessageType.Nak, new IPAddress(0), new IPAddress(0), new IPAddress(0));
        }

        /// <summary>
        /// Not Ackanoledge message.
        /// </summary>
        /// <param name="cip">Client IP addres..</param>
        /// <param name="mask">Network mask.</param>
        /// <param name="sip">Server IP address.</param>
        /// <returns>A byte arry with the message.</returns>
        public byte[] Decline(IPAddress cip, IPAddress mask, IPAddress sip) => BuildType(DhcpMessageType.Decline, cip, mask, sip);

        private int OptionsFindKey(DhcpOptionCode lookOpt)
        {
            int offset = 0;
            if (Options[offset] != (byte)DhcpOptionCode.Pad)
            {
                while (Options[offset] != (byte)DhcpOptionCode.End)
                {
                    byte optcode = Options[offset++];
                    int optlen = Options[offset++];
                    if ((DhcpOptionCode)optcode == lookOpt)
                    {
                        return offset - 2;
                    }

                    offset += optlen;
                }
            }

            return -1;
        }

        /// <summary>
        /// Resets the options.
        /// </summary>
        public void ResetOptions()
        {
            // 240 is where the options are starting, right after the magic cookie
            Options = new byte[DhcppacketSize - IndexToOptions];
            Options[0] = 0xff;
        }

        /// <summary>
        /// Add an option. This will just add the option to the option list, you are responsible to use the proper code and encoding.
        /// </summary>
        /// <param name="optdata">The options to add.</param>
        private void AddOptionRaw(ref byte[] optdata)
        {
            int offset = 0;
            while (Options[offset] != 0xff)
            {
                byte optcode = Options[offset++];
                int optlen = Options[offset++];
                offset += optlen;
            }

            optdata.CopyTo(Options, offset);
            Options[offset + optdata.Length] = (byte)DhcpOptionCode.End; // set end of options
        }

        /// <summary>
        /// Add an option to the options.
        /// </summary>
        /// <param name="optType">The option code.</param>
        /// <param name="optData">The option data.</param>
        public void AddOption(DhcpOptionCode optType, byte[] optData)
        {
            byte[] optTyData = new byte[2 + optData.Length];
            optTyData[0] = (byte)optType;
            optTyData[1] = (byte)optData.Length;
            optData.CopyTo(optTyData, 2);
            AddOptionRaw(ref optTyData);
        }

        /// <summary>
        /// Checks if the option contains a specific key.
        /// </summary>
        /// <param name="lookOpt">The option to check.</param>
        /// <returns>True if found.</returns>
        public bool OptionsContainsKey(DhcpOptionCode lookOpt) => OptionsFindKey(lookOpt) == -1 ? false : true;

        /// <summary>
        /// Gets the option contained in a key.
        /// </summary>
        /// <param name="lookOpt">The option to check.</param>
        /// <returns>The byte array with the raw option value.</returns>
        public byte[] GetOption(DhcpOptionCode lookOpt)
        {
            int optofs = OptionsFindKey(lookOpt);
            if (optofs == -1)
            {
                return null;
            }

            byte[] optVal = new byte[Options[optofs + 1]];
            Array.Copy(Options, optofs + 2, optVal, 0, optVal.Length);
            return optVal;
        }

        private bool IsOptionsInvalid() => !((Options != null) && (Options.Length > 0));
    }
}
