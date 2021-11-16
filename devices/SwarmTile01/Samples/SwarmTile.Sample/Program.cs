//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using Iot.Device.Swarm;
using System;
using System.Diagnostics;
using System.Threading;

Debug.WriteLine("Starting SWARM Tile demo");

var swarmTile = new SwarmTile("COM2");

// set this here just for debugging
swarmTile.TimeoutForCommandExecution = 50_000;

Thread.Sleep(10_000);

//swarmTile.PowerOff();

var rt = swarmTile.GetReceiveTestRate();

swarmTile.SetReceiveTestRate(15);

//swarmTile.SendToSleep(20);

//Message message = new Message("Hello Space! 1805", 12345);
//message.HoldDuration = 60;

//var msgId = swarmTile.TransmitData(message);

Thread.Sleep(Timeout.Infinite);

// Browse our samples repository: https://github.com/nanoframework/samples
// Check our documentation online: https://docs.nanoframework.net/
// Join our lively Discord community: https://discord.gg/gCyBu8T
