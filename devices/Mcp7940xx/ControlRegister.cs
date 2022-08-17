// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Mcp7940xx
{
    /// <summary>
    /// Mcp7940xx control register.
    /// </summary>
    internal enum ControlRegister : byte
    {
        /// <summary>
        /// Determines polarity of the MFP pin when in General Purpose Output mode.
        /// </summary>
        /// <remarks>
        /// <list type="table">
        /// <item>If <see cref="ControlRegister.SquareWaveOutput" /> = 1, this bit is ignored.</item>
        /// <item>If <see cref="ControlRegister.Alarm1InterruptEnabled" /> = 1 or <see cref="ControlRegister.Alarm2InterruptEnabled" /> = 1, this bit is ignored.</item>
        /// <item>Otherwise :</item>
        /// <item>1 = MFP is a logic high.</item>
        /// <item>0 = MFP is a logic low.</item>
        /// </list>
        /// </remarks>
        GeneralPurposeOutput = 0b1000_0000,

        /// <summary>
        /// Determines if the MFP pin is in Square Wave Output mode.
        /// </summary>
        /// <remarks>
        /// <list type="table">
        /// <item>1 = Enable square wave output on MFP pin.</item>
        /// <item>0 = Disable square wave output on MFP pin.</item>
        /// </list>
        /// </remarks>
        SquareWaveOutput = 0b0100_0000,

        /// <summary>
        /// Determines if Alarm 2 interrupt mode is active.
        /// </summary>
        /// <remarks>
        /// <list type="table">
        /// <item>1 = Enable alarm interrupt output on MFP pin for Alarm 2.</item>
        /// <item>0 = Disable alarm interrupt output on MFP pin for Alarm 2.</item>
        /// </list>
        /// </remarks>
        Alarm2InterruptEnabled = 0b0010_0000,

        /// <summary>
        /// Determines if Alarm 1 interrupt mode is active.
        /// </summary>
        /// <remarks>
        /// <list type="table">
        /// <item>1 = Enable alarm interrupt output on MFP pin for Alarm 1.</item>
        /// <item>0 = Disable alarm interrupt output on MFP pin for Alarm 1.</item>
        /// </list>
        /// </remarks>
        Alarm1InterruptEnabled = 0b0001_0000,

        /// <summary>
        /// Determines if the clock is being driven by an external clock source.
        /// </summary>
        /// <remarks>
        /// <list type="table">
        /// <item>1 = X1 pin is being driven by a 32.768 kHz external clock source.</item>
        /// <item>0 = No external clock source is present on the X1 pin.</item>
        /// </list>
        /// </remarks>
        ExternalClockInput = 0b000_01000,

        /// <summary>
        /// Determines if the clock is in Coarse Trim mode.
        /// </summary>
        /// <remarks>
        /// <list type="table">
        /// <item>Coarse Trim mode results in the MCP7940 applying digital trimming 128 times per second.</item>
        /// <item>If <see cref="ControlRegister.SquareWaveOutput" /> = 1, the MFP pin will output a trimmed signal.</item>
        /// <item>1 = Enable Coarse Trim mode.</item>
        /// <item>0 = Disable Coarse Trim mode.</item>
        /// </list>
        /// </remarks>
        CoarseTrimMode = 0b0000_0100,

        /// <summary>
        /// Determines the frequency of the clock output on MFP the pin if <see cref="ControlRegister.SquareWaveOutput" /> = 1.
        /// </summary>
        /// <remarks>
        /// <list type="table">
        /// <item>If <see cref="ControlRegister.CoarseTrimMode" /> = 1, the frequency selected will be subject to coarse trimming.</item>
        /// <item>Must contain a value defined in <see cref="SquareWaveFrequency" />.</item>
        /// </list>
        /// </remarks>
        SquareWaveFrequencyMask = 0b0000_0011
    }
}
