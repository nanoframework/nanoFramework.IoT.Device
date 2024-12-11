// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using Iot.Device.MulticastDNS;
using Iot.Device.MulticastDNS.Entities;
using Iot.Device.MulticastDNS.Enum;
using Iot.Device.MulticastDNS.EventArgs;
using nanoFramework.Networking;
using nanoFramework.WebServer;

namespace MulticastDNS.Samples
{
    internal class Program
    {
        // Replace with your wifi ssid/pwd
        const string Ssid = "...";
        private const string Pwd = "...";

        // The following string contains the domain we will query through a browser.
        private const string DeviceDomain = "nanodevice.local";

        private static string s_ipAddress;

        public static void Main()
        {
            // Connect to the WiFi.
            WifiNetworkHelper.ConnectDhcp(Ssid, Pwd);

            // Instantiate the MulticastDNSService
            using MulticastDNSService multicastDNSService = new();

            // After resolving the domain, the IP address of the device is sent back to the browser
            // We'll serve some text back to show this is actually working
            using WebServer webServer = new(80, HttpProtocol.Http);

            // Register the event handler that will receive mDNS messages
            multicastDNSService.MessageReceived += MulticastDNSService_MessageReceived;

            // Register the event handler that will treat the HTTP requests
            webServer.CommandReceived += WebServer_CommandReceived;

            // Find the IP address of the device
            s_ipAddress = FindMyIp();

            // Start the MulticastDNSService
            multicastDNSService.Start();
            // Start the webserver
            webServer.Start();

            Debug.WriteLine("All ready! Feel free to surf to http://nanodevice.local in your favorite browser...");

            Thread.Sleep(Timeout.Infinite);
        }

        private static string FindMyIp()
        {
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
            // Interface 0 is the wifi interface in ESP32. Adjust for other platforms or if you are using Ethernet
            return interfaces[0].IPv4Address;
        }

        private static void WebServer_CommandReceived(object obj, WebServerEventArgs e)
        {
            byte[] bytes = Encoding.UTF8.GetBytes("Hello there!");
            e.Context.Response.OutputStream.Write(bytes, 0, bytes.Length);
        }

        private static void MulticastDNSService_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (e.Message != null)
                foreach (Question question in e.Message.GetQuestions())
                {
                    if (question.QueryType == DnsResourceType.A && question.Domain == DeviceDomain)
                    {
                        var response = new Response();
                        response.AddAnswer(new ARecord(question.Domain, IPAddress.Parse(s_ipAddress)));
                        e.Response = response;
                    }
                }
        }
    }
}
