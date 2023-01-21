// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using nanoFramework.Runtime.Events;

namespace Iot.Device.RotaryEncoder
{
    /// <summary>
    /// EventArgs used with the RotaryEncode binding to pass event information when the Value changes.
    /// </summary>
    public class RotaryEncoderEventArgs : BaseEvent
    {
        /// <summary>Gets current value associated with the RotaryEncoder.</summary>
        public double Value { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RotaryEncoderEventArgs" /> class.
        /// </summary>
        /// <param name="value">Current value associated with the rotary encoder.</param>
        public RotaryEncoderEventArgs(double value)
        {
            Value = value;
        }
    }
}
