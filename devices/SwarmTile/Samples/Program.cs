// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Eclo Solutions
// See LICENSE file in the project root for full license information.

using Iot.Device.Ws28xx;
using Iot.Device.Swarm;
using nanoFramework.Hardware.Esp32;
using System.Device.Spi;
using System.Diagnostics;
using System.Drawing;
using System.Threading;

namespace Swarm.Sample
{
    public class Program
    {
        private static Ws28xx _neoPixel;
        private static SwarmTile _swarmTile;

        public static void Main()
        {
            Debug.WriteLine("Starting SWARM Tile demo");

            ///////////////////////////////////////////////////
            // setup SPI GPIOs to connecting to NeoPixel LED with SPI
            Configuration.SetPinFunction(38, DeviceFunction.SPI1_MOSI);
            Configuration.SetPinFunction(40, DeviceFunction.SPI1_MISO);
            Configuration.SetPinFunction(45, DeviceFunction.SPI1_CLOCK);

            // Make sure as well you are using the right chip select
            using SpiDevice spiDevice = SpiDevice.Create(new SpiConnectionSettings(1, 37)
            {
                ClockFrequency = 20_000_000,
                Mode = SpiMode.Mode0,
                DataBitLength = 8
            });

            _neoPixel = new Ws2808(spiDevice, 2);

            // clear LED
            ColorWipe(_neoPixel, Color.Transparent);

            _swarmTile = new SwarmTile("COM1");

            // set this here just for debugging
            _swarmTile.TimeoutForCommandExecution = 50_000;

            // wait 5 seconds for the Tile to become operational
            if (!_swarmTile.DeviceReady.WaitOne(5_000, false))
            {
                ////////////////////////////
                // Tile is not responsive //
                ////////////////////////////

                Debug.WriteLine("****************************************************************");
                Debug.WriteLine("*** TILE IS NOT RESPONSIVE, POSSIBLY POWERED OFF OR SLEEPING ***");
                Debug.WriteLine("****************************************************************");
            }
            else
            {
                // Tile is operational

                // output device IDs
                Debug.WriteLine($"DeviceID: {_swarmTile.DeviceID}");
                Debug.WriteLine($"DeviceName: {_swarmTile.DeviceName}");

                // setup event to handle power state change notifications
                _swarmTile.PowerStateChanged += SwarmTile_PowerStateChanged;

                // setup handler for message events
                _swarmTile.MessageEvent += SwarmTile_MessageEvent;

                // setup hander to process RSSI updates
                _swarmTile.BackgroundNoiseInfoAvailable += SwarmTile_BackgroundNoiseInfoAvailable;   

                // set GPS info rate to each 5 minutes
                _swarmTile.GeospatialInformationRate = 5 * 60;

                // set date time information to each minute
                _swarmTile.DateTimeStatusRate = 60;

                // set noise indicator update rate to 5 seconds
                _swarmTile.ReceiveTestRate = 5;

                // get count on how many received messages are waiting to be read
                var unreadCount = _swarmTile.MessagesReceived.UnreadCount;

                Debug.WriteLine($"There are {unreadCount} unread messages.");

                if (unreadCount > 0)
                {
                    bool keepReading = true;

                    while (keepReading)
                    {
                        // read newest message
                        var newestMessage = _swarmTile.MessagesReceived.ReadNewestMessage();

                        if (newestMessage == null)
                        {
                            // no more messages to read
                            keepReading = false;
                        }
                        else
                        {
                            Debug.WriteLine($"+++ New message +++");
                            Debug.WriteLine($"{newestMessage}");
                            Debug.WriteLine("");
                        }
                    }
                }

                // get count on how many messages are queued for transmission
                var toTxCount = _swarmTile.MessagesToTransmit.Count;

                Debug.WriteLine($"There are {toTxCount} messages queued for transmission.");

                //// compose message to transmit
                //MessageToTransmit message = new MessageToTransmit("Hello from .NET nanoFramework!");
                //message.ApplicationID = 12345;

                //// send message
                //string msgId;

                //if (_swarmTile.TryToSendMessage(message, out msgId))
                //{
                //    Debug.WriteLine($"Message {msgId} waiting to be transmitted!");
                //}
                //else
                //{
                //    Debug.WriteLine($"Failed to send message. Error: {_swarmTile.LastErrorMessage}.");
                //}
            }

            Thread.Sleep(Timeout.Infinite);
        }

        private static void SwarmTile_BackgroundNoiseInfoAvailable(int rssi)
        {
            // process value and

            if (_swarmTile.OperationalQuality > OperationalQuality.Bad)
            {
                // set RED
                ColorWipe(_neoPixel, Color.FromArgb(50, 255, 0, 0));
            }
            else if (_swarmTile.OperationalQuality < OperationalQuality.Marginal)
            {
                // set GREEN
                ColorWipe(_neoPixel, Color.FromArgb(50, 0, 250, 0));
            }
            else
            {
                // set ORANGE
                ColorWipe(_neoPixel, Color.FromArgb(50, 255, 255, 0));
            }

            _neoPixel.Update();
        }

        private static void SwarmTile_MessageEvent(MessageEvent messageEvent, string messageId)
        {
            switch (messageEvent)
            {
                case MessageEvent.Expired:
                    Debug.WriteLine($"Message {messageId} expired without being transmitted.");
                    break;

                case MessageEvent.Sent:
                    Debug.WriteLine($"Message {messageId} has been successfully transmitted.");
                    break;

                case MessageEvent.Received:
                    Debug.WriteLine($"Just received message {messageId}.");
                    break;

            }
        }

        private static void SwarmTile_PowerStateChanged(PowerState powerState)
        {
            switch (powerState)
            {
                case PowerState.On:
                    Debug.WriteLine($"Tile is ON.");
                    break;

                case PowerState.Off:
                    Debug.WriteLine($"Tile is OFF.");
                    break;

                case PowerState.Sleep:
                    Debug.WriteLine($"Tile was entered SLEEP.");
                    break;

                case PowerState.Unknown:
                    Debug.WriteLine($"Tile power mode is unknown.");
                    break;
            }
        }
        private static void ColorWipe(Ws28xx neo, Color color)
        {
            BitmapImage img = neo.Image;

            img.SetPixel(0, 0, color);
            img.SetPixel(1, 0, color);
            neo.Update();
        }
    }
}
