// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Relay;
using System.Device.I2c;
using System.Threading;

//////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the I2C GPIOs
// used for the bus
//Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
//Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);

Unit4Relay unit4Relay = new(new I2cDevice(new I2cConnectionSettings(1, Base4Relay.DefaultI2cAddress)));
// This will synchronize the led with the relay
unit4Relay.SynchronizedMode = true;
// Set relay 2, the led 2 should be on
unit4Relay.SetRelay(2, State.On);
// Set back the asyn modo
unit4Relay.SynchronizedMode = false;
// Set relay 1, the led 1 should be off while the relay on
unit4Relay.SetRelay(1, State.On);
// Set led 0 to on, the relay should be off
unit4Relay.SetLed(0, State.On);

Thread.Sleep(Timeout.Infinite);