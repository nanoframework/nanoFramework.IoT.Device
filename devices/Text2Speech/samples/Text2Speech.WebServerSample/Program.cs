// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Threading;
using Iot.Device.Text2Speech.Samples;
using nanoFramework.Networking;
using nanoFramework.WebServer;

namespace Iot.Device.Text2Speech.WebServerSample
{
    /// <summary>
    /// Hosts the Text2Speech browser interface on an ESP32-S3-BOX-Lite.
    /// </summary>
    public static class Program
    {
        private const string WifiSsid = "YourSSID";
        private const string WifiPassword = "YourPassword";
        private const int WifiTimeoutMilliseconds = 60000;

        /// <summary>
        /// Connects Wi-Fi, initializes audio, and starts the HTTP server.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Wi-Fi cannot connect or the Text2Speech web server cannot start.
        /// </exception>
        public static void Main()
        {
            Esp32S3BoxLiteWavPlayer player = null;
            try
            {
                CancellationTokenSource timeout =
                    new CancellationTokenSource(WifiTimeoutMilliseconds);
                bool connected = WifiNetworkHelper.ConnectDhcp(
                    WifiSsid,
                    WifiPassword,
                    requiresDateTime: false,
                    token: timeout.Token);
                if (!connected)
                {
                    string message = "Unable to connect Wi-Fi. Status: "
                        + WifiNetworkHelper.Status.ToString() + ".";
                    if (WifiNetworkHelper.HelperException != null)
                    {
                        message += " " + WifiNetworkHelper.HelperException.Message;
                    }

                    Debug.WriteLine(message);
                    throw new InvalidOperationException();
                }

                string address = NetworkInterface.GetAllNetworkInterfaces()[0].IPv4Address;
                Debug.WriteLine("Wi-Fi connected. Open http://" + address + "/");

                player = new Esp32S3BoxLiteWavPlayer(I2sPlaybackMode.Fast8Khz);
                TtsController.Initialize(player);

                using (WebServer server = new WebServer(
                    80,
                    HttpProtocol.Http,
                    new Type[] { typeof(TtsController) }))
                {
                    if (!server.Start())
                    {
                        throw new InvalidOperationException();
                    }

                    Thread.Sleep(Timeout.Infinite);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Text2Speech web sample failed: " + ex.ToString());
                throw;
            }
            finally
            {
                if (player != null)
                {
                    player.Dispose();
                }
            }
        }
    }
}
