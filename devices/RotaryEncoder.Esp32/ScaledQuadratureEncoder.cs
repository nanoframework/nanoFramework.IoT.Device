// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.RotaryEncoder.Esp32
{
    /// <summary>
    /// Scaled Quadrature Rotary Controller binding.
    /// </summary>
    public class ScaledQuadratureEncoder : QuadratureRotaryEncoder
    {
        private double _rangeMax;
        private double _rangeMin;
        private double _pulseIncrement;
        private long _previousPulse;

        /// <summary>Gets or sets current value associated with the RotaryEncoder.</summary>
        public double Value { get; set; }

        /// <summary>
        /// Gets or sets the AccelerationSlope property along with the AccelerationOffset property represents how the
        /// increase or decrease in value should grow as the incremental encoder is turned faster.
        /// </summary>
        public float AccelerationSlope { get; set; } = -0.05F;

        /// <summary>
        /// Gets or sets the AccelerationOffset property along with the AccelerationSlope property represents how the
        /// increase or decrease in value should grow as the incremental encoder is turned faster.
        /// </summary>
        public float AccelerationOffset { get; set; } = 6.0F;

        /// <summary>
        /// EventHandler to allow the notification of value changes.
        /// </summary>
        public event RotaryEncoderEventHandler? ValueChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScaledQuadratureEncoder" /> class.
        /// </summary>
        /// <param name="pinA">Pin A that is connected to the rotary encoder. Sometimes called clk.</param>
        /// <param name="pinB">Pin B that is connected to the rotary encoder. Sometimes called data.</param>
        /// <param name="pulsesPerRotation">The number of pulses to be received for every full rotation of the encoder.</param>
        /// <param name="pulseIncrement">The amount that the value increases or decreases on each pulse from the rotary encoder.</param>
        /// <param name="rangeMin">Minimum value permitted. The value is clamped to this.</param>
        /// <param name="rangeMax">Maximum value permitted. The value is clamped to this.</param>
        /// <param name="refreshRate">Refresh rate to check the encoder.</param>
        public ScaledQuadratureEncoder(int pinA, int pinB, int pulsesPerRotation, double pulseIncrement, double rangeMin, double rangeMax, TimeSpan refreshRate)
            : base(pinA, pinB, pulsesPerRotation, refreshRate)
        {
            _pulseIncrement = pulseIncrement;
            _rangeMin = rangeMin;
            _rangeMax = rangeMax;

            Value = _rangeMin;
            PulseCountChanged += OnPulse;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScaledQuadratureEncoder" /> class.
        /// </summary>
        /// <param name="pinA">Pin A that is connected to the rotary encoder. Sometimes called clk.</param>
        /// <param name="pinB">Pin B that is connected to the rotary encoder. Sometimes called data.</param>
        /// <param name="pulsesPerRotation">The number of pulses to be received for every full rotation of the encoder.</param>
        /// <param name="refreshRate">Refresh rate to check the encoder.</param>
        public ScaledQuadratureEncoder(int pinA, int pinB, int pulsesPerRotation, TimeSpan refreshRate)
            : this(pinA, pinB, 1, 0, 100, pulsesPerRotation, refreshRate)
        { 
        }

        /// <summary>
        /// Read the current Value.
        /// </summary>
        /// <returns>The value associated with the rotary encoder.</returns>
        public double ReadValue() => Value;

        /// <summary>
        /// Calculate the amount of acceleration to be applied to the increment of the encoder.
        /// </summary>
        /// <remarks>
        /// This uses a straight line function output = input * AccelerationSlope + Acceleration offset but can be overridden
        /// to perform different algorithms.
        /// </remarks>
        /// <param name="milliSecondsSinceLastPulse">The amount of time elapsed since the last data pulse from the encoder in milliseconds.</param>
        /// <returns>A value that can be used to apply acceleration to the rotary encoder.</returns>
        protected virtual int Acceleration(int milliSecondsSinceLastPulse)
        {
            // apply a straight line line function to the pulseWidth to determine the acceleration but clamp the lower value to 1
            return Math.Max(1, (int)((milliSecondsSinceLastPulse * AccelerationSlope) + AccelerationOffset));
        }

        private void OnPulse(object sender, RotaryEncoderEventArgs e)
        {            
            // calculate how much to change the value by
            double valueChange = (e.Value - _previousPulse) * _pulseIncrement * Acceleration(e.MillisecLastPulse);

            // We know we're getting long here
            _previousPulse = (long)e.Value;

            // set the value to the new value clamped by the maximum and minumum of the range.
            Value = Math.Max(Math.Min(Value + valueChange, _rangeMax), _rangeMin);

            // fire an event if an event handler has been attached
            ValueChanged?.Invoke(this, new RotaryEncoderEventArgs(Value, e.MillisecLastPulse));
        }
    }
}
