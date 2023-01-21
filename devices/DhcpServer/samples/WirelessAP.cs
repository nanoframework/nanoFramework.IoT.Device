// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Net.NetworkInformation;

namespace WiFiAP
{
    internal static class WirelessAP
    {
        /// <summary>
        /// Disable the Soft AP for next restart.
        /// </summary>
        public static void Disable()
        {
            WirelessAPConfiguration wapconf = GetConfiguration();
            wapconf.Options = WirelessAPConfiguration.ConfigurationOptions.None;
            wapconf.SaveConfiguration();
        }

        /// <summary>
        /// Set-up the Wireless AP settings, enable and save.
        /// </summary>
        /// <returns>True if already set-up.</returns>
        public static bool Setup()
        {
            string softApIP = "192.168.4.1";

            NetworkInterface ni = GetInterface();
            WirelessAPConfiguration wapconf = GetConfiguration();

            // Check if already Enabled and return true
            if (wapconf.Options == (WirelessAPConfiguration.ConfigurationOptions.Enable |
                                    WirelessAPConfiguration.ConfigurationOptions.AutoStart) &&
                ni.IPv4Address == softApIP)
            {
                // attempt to enable DHCP on AP
                // ni.EnableDhcp();
                // wapconf.Options = (WirelessAPConfiguration.ConfigurationOptions)10;
                return true;
            }

            // Set up IP address for Soft AP
            ni.EnableStaticIPv4(softApIP, "255.255.255.0", softApIP);

            // Set Options for Network Interface
            // Enable    - Enable the Soft AP ( Disable to reduce power )
            // AutoStart - Start Soft AP when system boots.
            // HiddenSSID- Hide the SSID
            wapconf.Options = WirelessAPConfiguration.ConfigurationOptions.AutoStart |
                            WirelessAPConfiguration.ConfigurationOptions.Enable;

            // Set the SSID for Access Point. If not set will use default  "nano_xxxxxx"
            // wapconf.Ssid = "MySsid";

            // Maximum number of simultaneous connections, reserves memory for connections
            wapconf.MaxConnections = 3;

            // To set-up Access point with no Authentication
            wapconf.Authentication = AuthenticationType.Open;
            wapconf.Password = string.Empty;

            // To set up Access point with no Authentication. Password minimum 8 chars.
            // wapconf.Authentication = AuthenticationType.WPA2;
            // wapconf.Password = "password";

            // Save the configuration so on restart Access point will be running.
            wapconf.SaveConfiguration();

            return false;
        }

        /// <summary>
        /// Find the Wireless AP configuration.
        /// </summary>
        /// <returns>Wireless AP configuration or NUll if not available.</returns>
        public static WirelessAPConfiguration GetConfiguration()
        {
            NetworkInterface ni = GetInterface();
            return WirelessAPConfiguration.GetAllWirelessAPConfigurations()[ni.SpecificConfigId];
        }

        public static NetworkInterface GetInterface()
        {
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

            // Find WirelessAP interface
            foreach (NetworkInterface ni in interfaces)
            {
                if (ni.NetworkInterfaceType == NetworkInterfaceType.WirelessAP)
                {
                    return ni;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the IP address of the Soft AP.
        /// </summary>
        /// <returns>IP address.</returns>
        public static string GetIP()
        {
            NetworkInterface ni = GetInterface();
            return ni.IPv4Address;
        }
    }
}
