// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Multiplexing
{
    /// <summary>
    /// Represents pin mappings for the ShiftRegister binding.
    /// Requires specifying 3 pins (serial data in, data clock, and latch).
    /// Can specify output enable pin (otherwise, wire to ground).
    /// </summary>
    public struct ShiftRegisterPinMapping
    {
        // Datasheet: https://www.ti.com/lit/ds/symlink/sn74hc595.pdf
        // Datasheet: http://archive.fairchip.com/pdf/MACROBLOCK/MBI5168.pdf
        // Datasheet: http://archive.fairchip.com/pdf/MACROBLOCK/MBI5027.pdf

        /// <summary>
        /// Initializes a new instance of the <see cref="ShiftRegisterPinMapping" /> struct.
        /// </summary>
        /// <param name="serialData">Serial data in pin.</param>
        /// <param name="clock">Shift register clock pin.</param>
        /// <param name="latchEnable">Register clock pin (latch).</param>
        /// <param name="outputEnable">Output enable pin.</param>
        public ShiftRegisterPinMapping(int serialData, int clock, int latchEnable, int outputEnable = -1)
        {
            SerialDataInput = serialData;
            Clock = clock;
            LatchEnable = latchEnable;
            OutputEnable = outputEnable;
        }
        
        /// <summary>
        /// Minimal pin bindings for ShiftRegister.
        /// Output enable should be wired to ground when using Minimal mapping.
        /// </summary>
        public static ShiftRegisterPinMapping Minimal => new ShiftRegisterPinMapping(16, 20, 21);
        /*
            SerialDataInput = 16    -- serial data in
            Clock           = 20    -- storage register clock
            LatchEnable     = 21    -- enable latch to publish storage register
        */

        /// <summary>
        /// Complete pin bindings for ShiftRegister.
        /// </summary>
        public static ShiftRegisterPinMapping Complete => new ShiftRegisterPinMapping(16, 20, 21, 12);
        /*
            SerialDataInput = 16    -- serial data in
            Clock           = 20    -- storage register clock
            LatchEnable     = 21    -- enable latch to publish storage register
            OutputEnable    = 12    -- output enable or disable
        */

        /// <summary>
        /// Gets or sets serial data in pin.
        /// </summary>
        public int SerialDataInput { get; set; }

        /// <summary>
        /// Gets or sets storage register clock pin.
        /// </summary>
        public int Clock { get; set; }

        /// <summary>
        /// Gets or sets shift register clock pin.
        /// </summary>
        public int LatchEnable { get; set; }

        /// <summary>
        /// Gets or sets output enable pin.
        /// </summary>
        public int OutputEnable { get; set; }
    }
}
