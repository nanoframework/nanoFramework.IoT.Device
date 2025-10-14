// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.DnsServer;
using System.Diagnostics;
using System.Net;
using System.Threading;

namespace DnsSample
{
    public class Program
    {
        public static void Main()
        {
            Debug.WriteLine("Hello from nanoFramework DNS Server sample!");

            IPAddress address = IPAddress.Parse("192.168.1.1");

            // wildcard so all DNS requests are answered and redirected to the server address
            DnsEntry[] dnsEntries = new DnsEntry[] { new("*", address) };

            DnsServer dnsServer = new DnsServer(
                address,
                dnsEntries);

            // start DNS server
            dnsServer.Start();

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
