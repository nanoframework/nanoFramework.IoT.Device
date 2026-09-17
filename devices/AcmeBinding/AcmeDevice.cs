// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Model;
using System.Drawing;
using System.Numerics;
using UnitsNet;

namespace Iot.Device.AcmeBinding
{
    /// <summary>
    /// Synthetic, hardware-free device binding used to exercise every <c>System.Device.Model</c>
    /// attribute case (Telemetry, Property, Command, Component) so the AI-Native IoT reflection
    /// engine and its renderers can be built and tested without real hardware.
    /// </summary>
    [Interface("Acme synthetic sensor/actuator")]
    public class AcmeDevice : IDisposable
    {
        private double _temperatureCelsius = 21.0;
        private int _samplingRateHz = 10;
        private double _threshold = 50.0;
        private AcmeMode _mode = AcmeMode.Idle;
        private Color _statusLedColor = Color.Green;
        private int _uptimeSeconds;
        private double _lastCalibrationOffset;
        private double _lastCalibrationScale;
        private int _lastCalibrationIterations;

        /// <summary>
        /// Initializes a new instance of the <see cref="AcmeDevice" /> class.
        /// </summary>
        public AcmeDevice()
        {
            Beeper = new AcmeBeeperModule();
        }

        /// <summary>
        /// Gets the simulated ambient temperature.
        /// </summary>
        /// <remarks>Telemetry case: no-arg property returning a UnitsNet-typed value.</remarks>
        [Telemetry]
        public Temperature Temperature => Temperature.FromDegreesCelsius(_temperatureCelsius);

        /// <summary>
        /// Gets the number of seconds the device has been running, incrementing on every read.
        /// </summary>
        /// <remarks>Telemetry case: no-arg method returning a value.</remarks>
        /// <returns>Simulated uptime in seconds.</returns>
        [Telemetry("Uptime")]
        public int GetUptimeSeconds() => (int)(Environment.TickCount64 / 1000);

        /// <summary>
        /// Reads the simulated device orientation.
        /// </summary>
        /// <remarks>Telemetry case: method returning bool with one out argument.</remarks>
        /// <param name="orientation">The simulated orientation vector.</param>
        /// <returns><see langword="true" /> if the orientation was read successfully.</returns>
        [Telemetry("Orientation", "The simulated orientation vector.")]
        public bool TryReadOrientation(out Vector3 orientation)
        {
            orientation = new Vector3(0.0, 0.0, 1.0);
            return true;
        }

        /// <summary>
        /// Gets the read-only synthetic firmware version.
        /// </summary>
        /// <remarks>Property case: read-only property.</remarks>
        public string FirmwareVersion
        {
            [Property]
            get => "1.0.0-synthetic";
        }

        /// <summary>
        /// Gets or sets the sampling rate, in hertz.
        /// </summary>
        /// <remarks>Property case: writable property via a single get/set accessor pair.</remarks>
        public int SamplingRateHz
        {
            [Property]
            get => _samplingRateHz;
            [Property]
            set => _samplingRateHz = value;
        }

        /// <summary>
        /// Gets the current operating mode.
        /// </summary>
        /// <remarks>Property case: read-only property of a non-primitive (enum) type.</remarks>
        [Property]
        public AcmeMode CurrentMode => _mode;

        /// <summary>
        /// Gets the current alert threshold.
        /// </summary>
        /// <remarks>Property case: writable property via separately-named getter/setter methods merged by name.</remarks>
        /// <returns>The current threshold value.</returns>
        [Property("Threshold")]
        public double GetThreshold() => _threshold;

        /// <summary>
        /// Sets the current alert threshold.
        /// </summary>
        /// <remarks>Property case: writable property via separately-named getter/setter methods merged by name.</remarks>
        /// <param name="value">The new threshold value.</param>
        [Property("Threshold")]
        public void SetThreshold(double value) => _threshold = value;

        /// <summary>
        /// Gets the color the last <see cref="SetStatusLedColor(Color)" /> call set, for test observability.
        /// </summary>
        public Color StatusLedColor => _statusLedColor;

        /// <summary>
        /// Gets the offset from the last <see cref="Calibrate(double, double, int)" /> call, for test observability.
        /// </summary>
        public double LastCalibrationOffset => _lastCalibrationOffset;

        /// <summary>
        /// Gets the scale from the last <see cref="Calibrate(double, double, int)" /> call, for test observability.
        /// </summary>
        public double LastCalibrationScale => _lastCalibrationScale;

        /// <summary>
        /// Gets the iteration count from the last <see cref="Calibrate(double, double, int)" /> call, for test observability.
        /// </summary>
        public int LastCalibrationIterations => _lastCalibrationIterations;

        /// <summary>
        /// Resets the device to its idle state.
        /// </summary>
        /// <remarks>Command case: zero parameters.</remarks>
        [Command]
        public void Reset()
        {
            _mode = AcmeMode.Idle;
            _uptimeSeconds = 0;
        }

        /// <summary>
        /// Switches the device to the given operating mode.
        /// </summary>
        /// <remarks>Command case: one parameter, of a non-primitive (enum) type.</remarks>
        /// <param name="mode">The mode to switch to.</param>
        [Command]
        public void SetMode(AcmeMode mode)
        {
            _mode = mode;
        }

        /// <summary>
        /// Sets the color of the status LED.
        /// </summary>
        /// <remarks>Command case: one parameter, of a non-primitive (<see cref="System.Drawing.Color" />) type.</remarks>
        /// <param name="color">The color to set the status LED to.</param>
        [Command]
        public void SetStatusLedColor(Color color)
        {
            _statusLedColor = color;
        }

        /// <summary>
        /// Runs the (simulated) calibration routine.
        /// </summary>
        /// <remarks>Command case: multiple parameters, exercising the "pack into a single object" convention
        /// required by MCP tools that only accept 0 or 1 parameter.</remarks>
        /// <param name="offset">Calibration offset to apply.</param>
        /// <param name="scale">Calibration scale factor to apply.</param>
        /// <param name="iterations">Number of calibration iterations to simulate.</param>
        [Command]
        public void Calibrate(double offset, double scale, int iterations)
        {
            _mode = AcmeMode.Calibrating;
            _lastCalibrationOffset = offset;
            _lastCalibrationScale = scale;
            _lastCalibrationIterations = iterations;
            _mode = AcmeMode.Idle;
        }

        /// <summary>
        /// Gets the synthetic beeper sub-device, nested under this device's MCP surface.
        /// </summary>
        /// <remarks>Component case: recursively flattened into the parent's capability model.</remarks>
        [Component]
        public AcmeBeeperModule Beeper { get; }

        /// <inheritdoc />
        public void Dispose()
        {
        }
    }
}
