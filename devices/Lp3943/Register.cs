// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Lp3943
{
    internal enum Register : byte
    {
        Input1 = 0x00, // LED 0-7 input register
        Input2 = 0x01, // LED 8-15 input register
        Psc0 = 0x02, // Frequency Prescaler 0
        Pwm0 = 0x03, // PWM register 0
        Psc1 = 0x04, // Frequency Prescaler 1
        Pwm1 = 0x05, // PWM register 1
        Ls0 = 0x06, // LED 0-3 Selector
        Ls1 = 0x07, // LED 4-7 Selector
        Ls2 = 0x08, // LED 8-11 Selector
        Ls3 = 0x09 // LED 12-15 Selector
    }
}
