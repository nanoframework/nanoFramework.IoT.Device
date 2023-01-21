// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Threading;
using nanoFramework.Networking;

namespace WiFiAP
{
    internal class Wireless80211
    {
        public static bool IsEnabled()
        {
            Wireless80211Configuration wconf = GetConfiguration();
            return !string.IsNullOrEmpty(wconf.Ssid);
        }

        /// <summary>
        /// Disable the Wireless station interface.
        /// </summary>
        public static void Disable()
        {
            Wireless80211Configuration wconf = GetConfiguration();
            wconf.Options = Wireless80211Configuration.ConfigurationOptions.None;
            wconf.SaveConfiguration();
        }

        /// <summary>
        /// Configure and enable the Wireless station interface.
        /// </summary>
        /// <param name="ssid">The SSID.</param>
        /// <param name="password">The password.</param>
        public static void Configure(string ssid, string password)
        {
            // And we have to force connect once here even for a short time
            var success = WifiNetworkHelper.ConnectDhcp(ssid, password, token: new CancellationTokenSource(10000).Token);
            Debug.WriteLine($"Connection is {success}");
            Wireless80211Configuration wconf = GetConfiguration();
            wconf.Options = Wireless80211Configuration.ConfigurationOptions.AutoConnect | Wireless80211Configuration.ConfigurationOptions.Enable;
            wconf.SaveConfiguration();
        }

        /// <summary>
        /// Get the Wireless station configuration.
        /// </summary>
        /// <returns>Wireless80211Configuration object.</returns>
        public static Wireless80211Configuration GetConfiguration()
        {
            NetworkInterface ni = GetInterface();
            return Wireless80211Configuration.GetAllWireless80211Configurations()[ni.SpecificConfigId];
        }

        public static NetworkInterface GetInterface()
        {
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

            // Find WirelessAP interface
            foreach (NetworkInterface ni in interfaces)
            {
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                {
                    return ni;
                }
            }

            return null;
        }

        public static string WaitIP()
        {
            while (true)
            {
                NetworkInterface ni = GetInterface();
                if (ni.IPv4Address != null && ni.IPv4Address.Length > 0)
                {
                    if (ni.IPv4Address[0] != '0')
                    {
                        return ni.IPv4Address;
                    }
                }

                Thread.Sleep(500);
            }
        }
    }
}
