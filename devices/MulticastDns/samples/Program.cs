// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using Iot.Device.MulticastDns;
using Iot.Device.MulticastDns.Entities;
using Iot.Device.MulticastDns.Enum;
using Iot.Device.MulticastDns.EventArgs;
using nanoFramework.Networking;
using nanoFramework.WebServer;

namespace MulticastDns.Samples
{
    internal class Program
    {
        // Replace with your wifi ssid/pwd
        const string Ssid = "...";
        const string Pwd = "...";

        // The following string contains the domain we will query through a browser.
        const string DeviceDomain = "nanodevice.local";

        private static string _ipAddress;

        public static void Main()
        {
            // Connect to the WiFi.
            bool result = WifiNetworkHelper.ConnectDhcp(Ssid, Pwd);
            Debug.Assert(result, "Looks like connecting to the WiFi didn't quite work out...");

            // Instantiate the MulticastDnsService
            using MulticastDnsService multicastDnsService = new();

            // After resolving the domain, the IP address of the device is sent back to the browser
            // We'll serve some text back to show this is actually working
            using WebServer webServer = new(80, HttpProtocol.Http);

            // Register the event handler that will receive mDNS messages
            multicastDnsService.MessageReceived += MulticastDnsService_MessageReceived;

            // Register the event handler that will treat the HTTP requests
            webServer.CommandReceived += WebServer_CommandReceived;

            // Find the IP address of the device
            _ipAddress = FindMyIp();

            // Start the MulticastDnsService
            multicastDnsService.Start();
            // Start the webserver
            webServer.Start();

            Debug.WriteLine("All ready! Feel free to surf to http://nanodevice.local in your favorite browser...");

            Thread.Sleep(Timeout.Infinite);
        }

        private static string FindMyIp()
        {
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
            return interfaces[0].IPv4Address;
        }

        private static void WebServer_CommandReceived(object obj, WebServerEventArgs e)
        {
            byte[] bytes = Encoding.UTF8.GetBytes("Hello there!");
            e.Context.Response.OutputStream.Write(bytes, 0, bytes.Length);
        }

        private static void MulticastDnsService_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (e.Message != null)
                foreach (Question question in e.Message.GetQuestions())
                {
                    if (question.QueryType == DnsResourceType.A && question.Domain == DeviceDomain)
                    {
                        var response = new Response();
                        response.AddAnswer(new ARecord(question.Domain, IPAddress.Parse(_ipAddress)));
                        e.Response = response;
                    }
                }
        }
    }
}
