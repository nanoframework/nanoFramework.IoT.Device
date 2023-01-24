// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;

namespace Iot.Device.QtrSensors
{
    /// <summary>
    /// Qtr Sensors Base class.
    /// </summary>
    public abstract class QtrBase
    {
        /// <summary>
        /// Gets or sets the Emitter selection.
        /// </summary>
        public abstract EmitterSelection EmitterSelection { get; set; }

        /// <summary>
        /// Gets or sets the EmmiterValue..
        /// </summary>
        public abstract PinValue EmitterValue { get; set; }

        /// <summary>
        /// Gets or sets the samples per sensor at each read.
        /// </summary>
        public byte SamplesPerSensor { get; set; } = 1;

        /// <summary>
        /// Calibrates the sensors.
        /// </summary>
        /// <param name="itteration">Number of itterations.</param>
        /// <param name="emitterOn">True to set the emmitters on.</param>
        /// <returns>The calibration data.</returns>
        public abstract CalibrationData[] Calibrate(int itteration, bool emitterOn);

        /// <summary>
        /// Resets the calibration.
        /// </summary>
        /// <param name="emitterOn">True to reset the emitters on.</param>
        public abstract void ResetCalibration(bool emitterOn);

        /// <summary>
        /// Returns the values in a ration from 0.0 to 1.0.
        /// </summary>
        /// <returns>An array of raw values.</returns>
        public abstract double[] ReadRatio();

        /// <summary>
        /// Reads the values as Raw values.
        /// </summary>
        /// <returns>An array of raw values.</returns>
        public abstract double[] ReadRaw();

        /// <summary>
        /// Reads the position of the line from -1.0 to 1.0, 0 is centered. -1 means full left, +1 means full right, 0 means centered.
        /// By convention, full left is the sensor 0 and full right is the last sensor.
        /// If no position is found, the last position will be returned.
        /// </summary>
        /// <param name="blackLine">True for a black line, false for a white line.</param>
        /// <returns>The position between -1.0 and 1.0.</returns>
        public abstract double ReadPosition(bool blackLine = true);
    }
}
