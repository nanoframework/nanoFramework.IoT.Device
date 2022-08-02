// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using Iot.Device.DhcpServer;
using nanoFramework.Networking;
using nanoFramework.Runtime.Native;

namespace WiFiAP
{
    internal class Program
    {
        // GPIO pin used to put device into AP set-up mode
        // Adjust to use another one on your board
        private const int SetupPin = 5;

        // for regular ESP32 boards
        // Adjust accordingly for example:
        // const int LED_PIN = 16; // for board with battery
        private const int LedPin = 2;

        // Start Simple WebServer
        private static WebServer _server = new WebServer();

        // Prepare Simple DHCPServer
        private static DhcpServer _dhcpserver = new DhcpServer();

        // Connected Station count
        private static int _connectedCount = 0;
        private static GpioPin _led;

        public static void Main()
        {
            Debug.WriteLine("Welcome to nF...");
            Debug.WriteLine($"Main FW: {SystemInfo.Version}");

            GpioPin setupButton = new GpioController().OpenPin(SetupPin, PinMode.InputPullUp);

            _led = new GpioController().OpenPin(LedPin, PinMode.Output);
            _led.Write(PinValue.High);
            Timer aliveLed = new Timer(CheckStatusTimerCallback, null, 500, 1000);

            // If Wireless station is not enabled then start Soft AP to allow Wireless configuration
            // or Button pressed
            if (!Wireless80211.IsEnabled() || (setupButton.Read() == PinValue.Low))
            {
                Wireless80211.Disable();
                if (WirelessAP.Setup() == false)
                {
                    // Reboot device to Activate Access Point on restart
                    Debug.WriteLine($"Setup Soft AP, Rebooting device");
                    Power.RebootDevice();
                }

                Debug.WriteLine($"Running Soft AP, waiting for client to connect");
                Debug.WriteLine($"Soft AP IP address :{WirelessAP.GetIP()}");

                _dhcpserver.CaptivePortalUrl = "http://192.168.4.1";
                _dhcpserver.Start(IPAddress.Parse(WirelessAP.GetIP()), new IPAddress(new byte[] { 255, 255, 255, 0 }));
                _server.Start();

                // Link up Network event to show Stations connecting/disconnecting to Access point.
                // Uncomment to have it:
                // NetworkChange.NetworkAPStationChanged += NetworkChange_NetworkAPStationChanged;
                //// Now that the normal Wifi is deactivated, that we have setup a static IP
                //// We can start the Web server
            }
            else
            {
                Debug.WriteLine($"Running in normal mode, connecting to Access point");
                var conf = Wireless80211.GetConfiguration();
                bool success;

                // For devices like STM32, the password can't be read
                if (string.IsNullOrEmpty(conf.Password))
                {
                    // In this case, we will let the automatic connection happen
                    success = WifiNetworkHelper.Reconnect(requiresDateTime: true, token: new CancellationTokenSource(60000).Token);
                }
                else
                {
                    // If we have access to the password, we will force the reconnection
                    // This is mainly for ESP32 which will connect normaly like that.
                    success = WifiNetworkHelper.ConnectDhcp(conf.Ssid, conf.Password, requiresDateTime: true, token: new CancellationTokenSource(60000).Token);
                }

                Debug.WriteLine($"Connection is {success}");
                if (success)
                {
                    string ipAdr = Wireless80211.WaitIP();
                    Debug.WriteLine($"Connected as {ipAdr}");

                    // We can even wait for a DateTime now
                    Thread.Sleep(100);
                    success = WifiNetworkHelper.Status == NetworkHelperStatus.NetworkIsReady;
                    if (success)
                    {
                        if (DateTime.UtcNow.Year > DateTime.MinValue.Year)
                        {
                            Debug.WriteLine($"We have a valid date: {DateTime.UtcNow}");
                            Debug.WriteLine($"We have a valid date: +8 {DateTime.UtcNow.AddHours(8)}");
                        }
                        else
                        {
                            Debug.WriteLine($"We have a invalid date!!! ( {DateTime.UtcNow} )");
                        }

                        Debug.WriteLine($"Starting http server...");
                        _server.Start();
                    }
                }
                else
                {
                    Debug.WriteLine($"Something wrong happened, can't connect at all");
                }
            }

            // Just wait for now
            // Here you would have the reset of your program using the client WiFI link
            Thread.Sleep(Timeout.Infinite);
        }

        private static void CheckStatusTimerCallback(object state)
        {
            _led.Toggle();
        }

        /// <summary>
        /// Event handler for Stations connecting or Disconnecting.
        /// </summary>
        /// <param name="networkIndex">The index of Network Interface raising event.</param>
        /// <param name="e">Event argument.</param>
        private static void NetworkChange_NetworkAPStationChanged(int networkIndex, NetworkAPStationEventArgs e)
        {
            Debug.WriteLine($"NetworkAPStationChanged event Index:{networkIndex} Connected:{e.IsConnected} Station:{e.StationIndex} ");

            // if connected then get information on the connecting station 
            if (e.IsConnected)
            {
                WirelessAPConfiguration wapconf = WirelessAPConfiguration.GetAllWirelessAPConfigurations()[0];
                WirelessAPStation station = wapconf.GetConnectedStations(e.StationIndex);

                string macString = BitConverter.ToString(station.MacAddress);
                Debug.WriteLine($"Station mac {macString} Rssi:{station.Rssi} PhyMode:{station.PhyModes} ");

                _connectedCount++;

                // Start web server when it connects otherwise the bind to network will fail as 
                // no connected network. Start web server when first station connects 
                if (_connectedCount == 1)
                {
                    // Wait for Station to be fully connected before starting web server
                    // other you will get a Network error
                    Thread.Sleep(2000);                    
                    _server.Start();
                }
            }
            else
            {
                // Station disconnected. When no more station connected then stop web server
                if (_connectedCount > 0)
                {
                    _connectedCount--;
                    if (_connectedCount == 0)
                    {
                        // _dhcpserver.Stop();
                        _server.Stop();
                    }
                }
            }
        }
    }
}
