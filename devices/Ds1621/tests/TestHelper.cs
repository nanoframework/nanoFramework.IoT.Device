// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Common;
using Iot.Device.Ds1621;
using nanoFramework.TestFramework;
using System;
using System.Device.I2c;

namespace Iot.Device.NFUnitTest
{
    internal static class TestHelper
    {
        static internal string GetRegisterName(Register register)
        {
            switch (register)
            {
                case Register.Temperature:
                    return "Temperature";

                case Register.HighTemperature:
                    return "HighTemperature";

                case Register.LowTemperature:
                    return "LowTemperature";

                case Register.Configuration:
                    return "Configuration";

                case Register.CountsRemaining:
                    return "CountsRemaining";

                case Register.CountsPerDegree:
                    return "CountsPerDegree";

                default:
                    return "Unrecognized Register";
            }
        }

        static internal void AssertRegistersEqual(SpanByte registerBankA, SpanByte registerB, Register register)
        {
            Assert.Equal(registerBankA[(byte)register], registerB[(byte)register], message: GetRegisterName(register));
        }

        static internal void AssertRegistersNotEqual(SpanByte registerA, SpanByte registerB, Register register)
        {
            Assert.NotEqual(registerA[(byte)register], registerB[(byte)register], message: GetRegisterName(register));
        }

        static internal void AssertMaskedRegistersEqual(byte registerA, byte registerB, byte bitMask)
        {
            Assert.Equal(registerA & bitMask, registerB & bitMask);
        }

        static internal void AssertMaskedRegistersEqual(SpanByte registerA, SpanByte registerB, Register register, byte bitMask)
        {
            Assert.Equal(registerA[(byte)register] & bitMask, registerB[(byte)register] & bitMask, message: GetRegisterName(register));
        }

        static internal void AssertMaskedRegistersNotEqual(byte registerA, byte registerB, byte bitMask)
        {
            Assert.NotEqual(registerA & bitMask, registerB & bitMask);
        }

        static internal void AssertMaskedRegistersNotEqual(SpanByte registerA, SpanByte registerB, Register register, byte bitMask)
        {
            Assert.NotEqual(registerA[(byte)register] & bitMask, registerB[(byte)register] & bitMask, message: GetRegisterName(register));
        }

    }
}
