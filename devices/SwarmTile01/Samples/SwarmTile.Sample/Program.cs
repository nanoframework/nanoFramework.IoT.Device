//
// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Eclo Solutions
// See LICENSE file in the project root for full license information.
//

using Iot.Device.Swarm;
using System.Diagnostics;
using System.Threading;

namespace NFApp28
{
    public class Program
    {
        public static void Main()
        {
            Debug.WriteLine("Starting SWARM Tile demo");

            SwarmTile swarmTile = new SwarmTile("COM1");

            // set this here just for debugging
            swarmTile.TimeoutForCommandExecution = 50_000;

            // wait 5 seconds for the Tile to become operational
            if (!swarmTile.DeviceReady.WaitOne(5_000, false))
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
                Debug.WriteLine($"DeviceID: {swarmTile.DeviceID}");
                Debug.WriteLine($"DeviceName: {swarmTile.DeviceName}");

                // setup event to handle power state change notifications
                swarmTile.PowerStateChanged += SwarmTile_PowerStateChanged;

                // setup handler for message events
                swarmTile.MessageEvent += SwarmTile_MessageEvent;

                // set GPS info rate to each 5 minutes
                swarmTile.GeospatialInformationRate = 5 * 60;

                // set date time information to each minute
                swarmTile.DateTimeStatusRate = 60;

                // get count on how many received messages are waiting to be read
                var unreadCount = swarmTile.MessagesReceived.UnreadCount;

                Debug.WriteLine($"There are {unreadCount} unread messages.");

                if (unreadCount > 0)
                {
                    bool keepReading = true;

                    while (keepReading)
                    {
                        // read newest message
                        var newestMessage = swarmTile.MessagesReceived.ReadNewestMessage();

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
                var toTxCount = swarmTile.MessagesToTransmit.Count;

                Debug.WriteLine($"There are {toTxCount} messages queued for transmission.");

                // compose message to transmit
                MessageToTransmit message = new MessageToTransmit("Hello from .NET nanoFramework!");
                message.ApplicationID = 12345;

                // send message
                string msgId;

                if (swarmTile.TryToSendMessage(message, out msgId))
                {
                    Debug.WriteLine($"Message {msgId} waiting to be transmitted!");
                }
                else
                {
                    Debug.WriteLine($"Failed to send message. Error: {swarmTile.LastErrorMessage}.");
                }
            }

            Thread.Sleep(Timeout.Infinite);
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
    }
}
