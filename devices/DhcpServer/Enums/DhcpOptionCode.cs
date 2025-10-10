// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.DhcpServer.Enums
{
    /// <summary>
    /// DHCP option code. See https://www.ibm.com/docs/en/i/7.2?topic=concepts-dhcp-options-lookup for a comprehensive definition of each of them.
    /// Also present in https://en.wikipedia.org/wiki/Dynamic_Host_Configuration_Protocol.
    /// This enumeration is not complete and more options are available.
    /// </summary>
    public enum DhcpOptionCode : byte
    {
        /// <summary>Pad.</summary>
        Pad = 0,

        /// <summary>Subnet mask.</summary>
        SubnetMask = 1,

        /// <summary>Time offset.</summary>
        TimeOffset = 2,

        /// <summary>Router.</summary>
        Router = 3,

        /// <summary>Time server.</summary>
        TimeServer = 4,

        /// <summary>Name server.</summary>
        NameServer = 5,

        /// <summary>Domain name server.</summary>
        DomainNameServer = 6,

        /// <summary>Log server.</summary>
        LogServer = 7,

        /// <summary>Cookie server.</summary>
        CookieServer = 8,

        /// <summary>Line printer server.</summary>
        LinePrinterServer = 9,

        /// <summary>Impress server.</summary>
        ImpressServer = 10,

        /// <summary>Resource location server.</summary>
        ResourceLocationServer = 11,

        /// <summary>Hostname.</summary>
        Hostname = 12,

        /// <summary>Boot file size.</summary>
        BootFileSize = 13,

        /// <summary>Merit dump file.</summary>
        MeritDumpFile = 14,

        /// <summary>Domain name suffix.</summary>
        DomainNameSuffix = 15,

        /// <summary>Swap server.</summary>
        WrapServer = 16,

        /// <summary>Root path.</summary>
        RootPath = 17,

        /// <summary>Extensions path.</summary>
        ExtensionsPath = 18,

        /// <summary>IP forwarding.</summary>
        IpForwarding = 19,

        /// <summary>Non-Local source routing.</summary>
        NonLocalSourceRouting = 20,

        /// <summary>Policy filter.</summary>
        PolicyFilter = 21,

        /// <summary>Maximum datagram reassembly size.</summary>
        MaximumDatagramReassemblySize = 22,

        /// <summary>Default IP time to live.</summary>
        DefaultIpTimeToLive = 23,

        /// <summary>Path MTU aging timeout.</summary>
        PathMtuAgingTimeout = 24,

        /// <summary>Path MTU plateau table.</summary>
        PathMtuPlateauTable = 25,

        /// <summary>Interface MTU.</summary>
        InterfaceMtu = 26,

        /// <summary>All subnets are local.</summary>
        AllSubnetsAreLocal = 27,

        /// <summary>Broadcast address.</summary>
        BroadcastAddress = 28,

        /// <summary>Perform mask discovery.</summary>
        PerformMaskDiscovery = 29,

        /// <summary>Mask supplier.</summary>
        MaskSupplier = 30,

        /// <summary>Perform router discovery.</summary>
        PerformRouterDiscovery = 31,

        /// <summary>Router solicitation address option.</summary>
        RouteurSolicitationAddressOption = 32,

        /// <summary>Static route.</summary>
        StaticRoute = 33,

        /// <summary>Trailer encapsulation.</summary>
        TailerEncapsulation = 34,

        /// <summary>ARP cache timeout.</summary>
        ArpCacheTimeout = 35,

        /// <summary>Ethernet encapsulation.</summary>
        EthernetEncapsulation = 36,

        /// <summary>TCP default TTL.</summary>
        TcpDefaultTtl = 37,

        /// <summary>TCP keep-alive interval.</summary>
        TcpKeepAliveInterval = 38,

        /// <summary>TCP keep-alive garbage.</summary>
        TcpKeppAliveGarbage = 39,

        /// <summary>Network information service domain.</summary>
        NetworkInformationServiceDomain = 40,

        /// <summary>Network information servers.</summary>
        NetworkInformationServers = 41,

        /// <summary>Network time protocol servers option.</summary>
        NetworkTimeProtocolServersOption = 42,

        /// <summary>NetBIOS over TCP/IP name server.</summary>
        NetBiosTcpNameServer = 44,

        /// <summary>NetBIOS over TCP/IP datagram distribution server.</summary>
        NetBiosTcpDatagramDistributionServer = 45,

        /// <summary>NetBIOS over TCP/IP node type.</summary>
        NetBiosTcpNodeType = 46,

        /// <summary>NetBIOS over TCP/IP scope.</summary>
        NetBiosTcpScope = 47,

        /// <summary>X Window System Font server.</summary>
        XWindowSystemFontServer = 48,

        /// <summary>X Window System display manager.</summary>
        XWindowSystemDisplayManager = 49,

        /// <summary>Requested IP Address.</summary>
        RequestedIpAddress = 50,

        /// <summary>IP address lease time.</summary>
        AddressTime = 51,

        /// <summary>DHCP message type.</summary>
        DhcpMessageType = 53,

        /// <summary>DHCP address.</summary>
        DhcpAddress = 54,

        /// <summary>Parameter list.</summary>
        ParameterList = 55,

        /// <summary>DHCP Message.</summary>
        DhcpMessage = 56,

        /// <summary>DHCP maximum message size.</summary>
        DhcpMaxMessageSize = 57,

        /// <summary>Renewal (T1) time value.</summary>
        RenewalT1 = 58,

        /// <summary>Rebinding (T2) time value.</summary>
        RebindingT2 = 59,

        /// <summary>Class ID.</summary>
        ClassId = 60,

        /// <summary>Client ID.</summary>
        ClientId = 61,

        /// <summary>NetWare/IP domain name.</summary>
        NetWareIpDomainName = 62,

        /// <summary>NetWare/IP.</summary>
        NetWareIp = 63,

        /// <summary>NIS domain name.</summary>
        NisDomainName = 64,

        /// <summary>NIS servers.</summary>
        NisServer = 65,

        /// <summary>Server name.</summary>
        ServerName = 66,

        /// <summary>Boot file name.</summary>
        BootFileName = 67,

        /// <summary>Home address.</summary>
        HomeAddress = 68,

        /// <summary>SMTP servers.</summary>
        SmtpServers = 69,

        /// <summary>POP3 server.</summary>
        Pop3Server = 70,

        /// <summary>NNTP server.</summary>
        NntpServer = 71,

        /// <summary>WWW Server.</summary>
        WwwServer = 72,

        /// <summary>Finger server.</summary>
        FingerServer = 73,

        /// <summary>IRC server.</summary>
        IrcServer = 74,

        /// <summary>StreetTalk server.</summary>
        StreetTalkServer = 75,

        /// <summary>STDA server.</summary>
        StdaServer = 76,

        /// <summary>User class.</summary>
        UserClass = 77,

        /// <summary>Directory agent.</summary>
        DirectoryAgent = 78,

        /// <summary>Service scope.</summary>
        ServiceScope = 79,

        /// <summary>Naming authority.</summary>
        NamingAuthority = 80,

        /// <summary>Auto config.</summary>
        AutoConfig = 0x74,

        /// <summary>Captive portal. See RFC8910.</summary>
        CaptivePortal = 114,

        /// <summary>End.</summary>
        End = 0xFF
    }

#if DEBUG

    /// <summary>
    /// Extensions for the <see cref="DhcpOptionCode"/> enumeration.
    /// </summary>
    public static class DhcpOptionCodeExtensions
    {
        /// <summary>
        /// Converts the DHCP option code to a string.
        /// </summary>
        /// <param name="optionCode">The DHCP option code.</param>
        /// <returns>The string representation of the DHCP option code.</returns>
        public static string AsString(this DhcpOptionCode optionCode)
        {
            switch (optionCode)
            {
                case DhcpOptionCode.Pad:
                    return "Pad";
                case DhcpOptionCode.SubnetMask:
                    return "Subnet Mask";
                case DhcpOptionCode.TimeOffset:
                    return "Time Offset";
                case DhcpOptionCode.Router:
                    return "Router";
                case DhcpOptionCode.TimeServer:
                    return "Time Server";
                case DhcpOptionCode.NameServer:
                    return "Name Server";
                case DhcpOptionCode.DomainNameServer:
                    return "Domain Name Server";
                case DhcpOptionCode.LogServer:
                    return "Log Server";
                case DhcpOptionCode.CookieServer:
                    return "Cookie Server";
                case DhcpOptionCode.LinePrinterServer:
                    return "Line Printer Server";
                case DhcpOptionCode.ImpressServer:
                    return "Impress Server";
                case DhcpOptionCode.ResourceLocationServer:
                    return "Resource Location Server";
                case DhcpOptionCode.Hostname:
                    return "Host Name";
                case DhcpOptionCode.BootFileSize:
                    return "Boot File Size";
                case DhcpOptionCode.MeritDumpFile:
                    return "Merit Dump File";
                case DhcpOptionCode.DomainNameSuffix:
                    return "Domain Name";
                case DhcpOptionCode.WrapServer:
                    return "Swap Server";
                case DhcpOptionCode.RootPath:
                    return "Root Path";
                case DhcpOptionCode.ExtensionsPath:
                    return "Extensions Path";
                case DhcpOptionCode.IpForwarding:
                    return "IP Forwarding";
                case DhcpOptionCode.NonLocalSourceRouting:
                    return "Non-Local Source Routing";
                case DhcpOptionCode.PolicyFilter:
                    return "Policy Filter";
                case DhcpOptionCode.MaximumDatagramReassemblySize:
                    return "Maximum Datagram Reassembly Size";
                case DhcpOptionCode.DefaultIpTimeToLive:
                    return "Default IP TTL";
                case DhcpOptionCode.PathMtuAgingTimeout:
                    return "Path MTU Aging Timeout";
                case DhcpOptionCode.PathMtuPlateauTable:
                    return "Path MTU Plateau Table";
                case DhcpOptionCode.InterfaceMtu:
                    return "Interface MTU";
                case DhcpOptionCode.AllSubnetsAreLocal:
                    return "All Subnets Are Local";
                case DhcpOptionCode.BroadcastAddress:
                    return "Broadcast Address";
                case DhcpOptionCode.PerformMaskDiscovery:
                    return "Perform Mask Discovery";
                case DhcpOptionCode.MaskSupplier:
                    return "Mask Supplier";
                case DhcpOptionCode.PerformRouterDiscovery:
                    return "Perform Router Discovery";
                case DhcpOptionCode.RouteurSolicitationAddressOption:
                    return "Router Solicitation Address";
                case DhcpOptionCode.StaticRoute:
                    return "Static Route";
                case DhcpOptionCode.TailerEncapsulation:
                    return "Trailer Encapsulation";
                case DhcpOptionCode.ArpCacheTimeout:
                    return "ARP Cache Timeout";
                case DhcpOptionCode.EthernetEncapsulation:
                    return "Ethernet Encapsulation";
                case DhcpOptionCode.TcpDefaultTtl:
                    return "TCP Default TTL";
                case DhcpOptionCode.TcpKeepAliveInterval:
                    return "TCP Keep-Alive Interval";
                case DhcpOptionCode.TcpKeppAliveGarbage:
                    return "TCP Keep-Alive Garbage";
                case DhcpOptionCode.NetworkInformationServiceDomain:
                    return "NIS Domain";
                case DhcpOptionCode.NetworkInformationServers:
                    return "NIS Servers";
                case DhcpOptionCode.NetworkTimeProtocolServersOption:
                    return "NTP Servers";
                case DhcpOptionCode.NetBiosTcpNameServer:
                    return "NetBIOS Name Server";
                case DhcpOptionCode.NetBiosTcpDatagramDistributionServer:
                    return "NetBIOS Datagram Distribution Server";
                case DhcpOptionCode.NetBiosTcpNodeType:
                    return "NetBIOS Node Type";
                case DhcpOptionCode.NetBiosTcpScope:
                    return "NetBIOS Scope";
                case DhcpOptionCode.XWindowSystemFontServer:
                    return "X Window System Font Server";
                case DhcpOptionCode.XWindowSystemDisplayManager:
                    return "X Window System Display Manager";
                case DhcpOptionCode.RequestedIpAddress:
                    return "Requested IP Address";
                case DhcpOptionCode.AddressTime:
                    return "IP Address Lease Time";
                case DhcpOptionCode.DhcpMessageType:
                    return "DHCP Message Type";
                case DhcpOptionCode.DhcpAddress:
                    return "DHCP Server Identifier";
                case DhcpOptionCode.ParameterList:
                    return "Parameter Request List";
                case DhcpOptionCode.DhcpMessage:
                    return "DHCP Message";
                case DhcpOptionCode.DhcpMaxMessageSize:
                    return "DHCP Maximum Message Size";
                case DhcpOptionCode.RenewalT1:
                    return "Renewal Time Value";
                case DhcpOptionCode.RebindingT2:
                    return "Rebinding Time Value";
                case DhcpOptionCode.ClassId:
                    return "Class Identifier";
                case DhcpOptionCode.ClientId:
                    return "Client Identifier";
                case DhcpOptionCode.NetWareIpDomainName:
                    return "NetWare/IP Domain Name";
                case DhcpOptionCode.NetWareIp:
                    return "NetWare/IP Information";
                case DhcpOptionCode.NisDomainName:
                    return "NIS+ Domain Name";
                case DhcpOptionCode.NisServer:
                    return "NIS+ Servers";
                case DhcpOptionCode.ServerName:
                    return "Server Name";
                case DhcpOptionCode.BootFileName:
                    return "Boot File Name";
                case DhcpOptionCode.HomeAddress:
                    return "Home Agent Addresses";
                case DhcpOptionCode.SmtpServers:
                    return "SMTP Server";
                case DhcpOptionCode.Pop3Server:
                    return "POP3 Server";
                case DhcpOptionCode.NntpServer:
                    return "NNTP Server";
                case DhcpOptionCode.WwwServer:
                    return "WWW Server";
                case DhcpOptionCode.FingerServer:
                    return "Finger Server";
                case DhcpOptionCode.IrcServer:
                    return "IRC Server";
                case DhcpOptionCode.StreetTalkServer:
                    return "StreetTalk Server";
                case DhcpOptionCode.StdaServer:
                    return "StreetTalk Directory Assistance Server";
                case DhcpOptionCode.UserClass:
                    return "User Class";
                case DhcpOptionCode.DirectoryAgent:
                    return "Directory Agent";
                case DhcpOptionCode.ServiceScope:
                    return "Service Scope";
                case DhcpOptionCode.NamingAuthority:
                    return "Naming Authority";
                case DhcpOptionCode.AutoConfig:
                    return "Auto Configuration";
                case DhcpOptionCode.CaptivePortal:
                    return "Captive Portal";

                default:
                    return ((byte)optionCode).ToString();
            }
        }
    }

#endif
}
