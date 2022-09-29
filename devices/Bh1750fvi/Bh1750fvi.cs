// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using System.Device.I2c;
using System.Device.Model;
using UnitsNet;

namespace Iot.Device.Bh1750fvi
{
    /// <summary>
    /// Ambient Light Sensor BH1750FVI.
    /// </summary>
    [Interface("Ambient Light Sensor BH1750FVI")]
    public class Bh1750fvi : IDisposable
    {
        private const byte DefaultLightTransmittance = 0b01000101;

        private I2cDevice _i2cDevice;

        private double _lightTransmittance;

        /// <summary>
        /// Gets or sets light transmittance, from 27.20% to 222.50%.
        /// </summary>
        [Property]
        public double LightTransmittance
        {
            get => _lightTransmittance;
            set
            {
                SetLightTransmittance(value);
                _lightTransmittance = value;
            }
        }

        /// <summary>
        /// Gets or sets measuring mode.
        /// </summary>
        [Property]
        public MeasuringMode MeasuringMode { get; set; }

        /// <summary>
        /// BH1750FVI Illuminance (Lux).
        /// </summary>
        [Telemetry]
        public Illuminance Illuminance => GetIlluminance();

        /// <summary>
        /// Initializes a new instance of the <see cref="Bh1750fvi" /> class.
        /// </summary>
        /// <param name="i2cDevice">The I2C device used for communication.</param>
        /// <param name="measuringMode">The measuring mode of BH1750FVI.</param>
        /// <param name="lightTransmittance">BH1750FVI Light Transmittance, from 27.20% to 222.50%.</param>
        public Bh1750fvi(I2cDevice i2cDevice, MeasuringMode measuringMode = MeasuringMode.ContinuouslyHighResolutionMode, double lightTransmittance = 1)
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));

            _i2cDevice.WriteByte((byte)Command.PowerOn);
            _i2cDevice.WriteByte((byte)Command.Reset);

            LightTransmittance = lightTransmittance;
            MeasuringMode = measuringMode;
        }

        /// <summary>
        /// Set BH1750FVI Light Transmittance.
        /// </summary>
        /// <param name="transmittance">Light Transmittance, from 27.20% to 222.50%.</param>
        private void SetLightTransmittance(double transmittance)
        {
            if (transmittance > 2.225 || transmittance < 0.272)
            {
                throw new ArgumentOutOfRangeException(nameof(transmittance), $"{nameof(transmittance)} needs to be in the range of 27.20% to 222.50%.");
            }

            byte val = (byte)(DefaultLightTransmittance / transmittance);

            _i2cDevice.WriteByte((byte)((byte)Command.MeasurementTimeHigh | (val >> 5)));
            _i2cDevice.WriteByte((byte)((byte)Command.MeasurementTimeLow | (val & 0b00011111)));
        }

        /// <summary>
        /// Get BH1750FVI Illuminance.
        /// </summary>
        /// <returns>Illuminance (Default unit: Lux).</returns>
        private Illuminance GetIlluminance()
        {
            if (MeasuringMode == MeasuringMode.OneTimeHighResolutionMode || MeasuringMode == MeasuringMode.OneTimeHighResolutionMode2 || MeasuringMode == MeasuringMode.OneTimeLowResolutionMode)
            {
                _i2cDevice.WriteByte((byte)Command.PowerOn);
            }

            SpanByte readBuff = new byte[2];

            _i2cDevice.WriteByte((byte)MeasuringMode);
            _i2cDevice.Read(readBuff);

            ushort raw = BinaryPrimitives.ReadUInt16BigEndian(readBuff);

            double result = raw / (1.2 * _lightTransmittance);

            if (MeasuringMode == MeasuringMode.ContinuouslyHighResolutionMode2 || MeasuringMode == MeasuringMode.OneTimeHighResolutionMode2)
            {
                result *= 2;
            }

            return Illuminance.FromLux(result);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void Dispose()
        {
            _i2cDevice?.Dispose();
            _i2cDevice = null;
        }
    }
}
