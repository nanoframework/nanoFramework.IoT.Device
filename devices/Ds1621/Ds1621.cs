// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.I2c;
using Iot.Device.Common;
using UnitsNet;
using UnitsNet.Units;

namespace Iot.Device.Ds1621
{
    /// <summary>
    /// I2C digital thermometer with temperature range of -55°C to 125°C and 0.5°C resolution.
    /// </summary>
    public class Ds1621 : IDisposable
    {
        /// <summary>
        /// Default I2C address for Ds1621.
        /// </summary>
        public const byte DefaultI2cAddress = 0x48;

        /// <summary>
        /// The underlying I2C device used for communication.
        /// </summary>
        private readonly I2cDevice _i2cDevice;

        /// <summary>
        /// Converts the devices interal two-byte temperature format into a floating point representation.
        /// </summary>
        /// <param name="temperatureMSB">The most significant bit of the temperature register.</param>
        /// <param name="temperatureLSB">The least significant bit of the temperature register.</param>
        /// <returns>The temperature in degrees Celsius.</returns>
        internal static Temperature UnpackTemperature(byte temperatureMSB, byte temperatureLSB)
        {
            double unpackedTemperature = (sbyte)temperatureMSB;

            // Check if 0.5°C bit is set.
            if (temperatureLSB == 0x80)
            {
                unpackedTemperature += 0.5f;
            }

            return Temperature.FromDegreesCelsius(unpackedTemperature);
        }

        /// <summary>
        /// Converts a temperature into the devices interal two-byte temperature format.
        /// </summary>
        /// <param name="temperature">The temperature to convert.</param>
        /// <returns>
        /// A byte array holding the temperature.
        /// </returns>
        /// <remarks>
        /// The temperature will be clamped to the the range -55°C through 125°C and rounded to the nearest 0.5°C.
        /// </remarks>
        internal static byte[] PackTemperature(Temperature temperature)
        {
            byte[] packedTemperature = new byte[2];

            // Clamp temperature to the valid range for a Ds1621.
            double clampedTemperature = Math.Clamp(temperature.DegreesCelsius, -55, 125);

            // Get temperature fraction for use in rounding temperature to nearest 0.5°C.
            double fractionalPortion = clampedTemperature - (int)clampedTemperature;

            if (clampedTemperature >= 0)
            {
                if (fractionalPortion >= 0.75)
                {
                    // Round temperature up and store in two's complement.
                    packedTemperature[0] = (byte)(sbyte)((int)clampedTemperature + 1);

                    // No half degree flag.
                    packedTemperature[1] = 0;
                }
                else
                {
                    // Truncate temperature and store in two's complement.
                    packedTemperature[0] = (byte)(sbyte)clampedTemperature;

                    if (fractionalPortion <= 0.25)
                    {
                        // No half degree flag.
                        packedTemperature[1] = 0;
                    }
                    else
                    {
                        // Round to 0.5°C (Set half degree flag).
                        packedTemperature[1] = 0x80;
                    }
                }
            }
            else
            {
                if (fractionalPortion >= -0.25)
                {
                    // Round temperature down and store in two's complement.
                    packedTemperature[0] = (byte)~(-(sbyte)clampedTemperature - 1);

                    // No half degree flag.
                    packedTemperature[1] = 0;
                }
                else
                {
                    // Truncate temperature and store in two's complement.
                    packedTemperature[0] = (byte)~(-(sbyte)clampedTemperature);

                    if (fractionalPortion <= -0.75)
                    {
                        // No half degree flag.
                        packedTemperature[1] = 0;
                    }
                    else
                    {
                        // Round to 0.5°C (Set half degree flag).
                        packedTemperature[1] = 0x80;
                    }
                }
            }

            return packedTemperature;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Ds1621" /> class.
        /// </summary>
        /// <param name="i2cDevice">The I2C device to use for communication.</param>
        /// <param name="mode">The measurement mode to use when the device receives a temperature measurement command.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="i2cDevice"/> is <c>null</c>.</exception>
        public Ds1621(I2cDevice i2cDevice, MeasurementMode mode = MeasurementMode.Single)
        {
            if (i2cDevice == null)
            {
                throw new ArgumentNullException();
            }

            _i2cDevice = i2cDevice;
            MeasurementMode = mode;
        }

        #region Temperature

        /// <summary>
        /// Requests the device perform a temperature measurement.
        /// </summary>
        /// <remarks>
        /// A temperature measurement can take up to 750ms.
        /// </remarks>
        public void MeasureTemperature()
        {
            _i2cDevice.WriteByte((byte)Command.StartTemperatureConversion);
        }

        /// <summary>
        /// Requests the device stop performing temperature measurement.
        /// </summary>
        public void StopTemperatureMeasurement()
        {
            _i2cDevice.WriteByte((byte)Command.StopTemperatureConversion);
        }

        /// <summary>
        /// Gets a value indicating whether the device is currently performing a temperature measurement.
        /// </summary>
        public bool IsMeasuringTemperature
        {
            get
            {
                return RegisterHelper.RegisterBitIsSet(_i2cDevice, (byte)Register.Configuration, (byte)RegisterMask.TemperatureConversionDoneMask);
            }
        }

        /// <summary>
        /// Returns the last temperature measurement taken by the device in the range -55°C through 125°C and rounded to the nearest 0.5°C.
        /// </summary>
        /// <returns>
        /// A <see cref = "Temperature" /> object whose value is a 9-bit representation of the last temperature measurement taken by the device.
        /// </returns>
        public Temperature GetTemperature()
        {
            byte[] temperature = RegisterHelper.ReadRegisterBlock(_i2cDevice, (byte)Register.Temperature, 2);
            return UnpackTemperature(temperature[0], temperature[1]);
        }

        /// <summary>
        /// Returns the last temperature measurement taken by the device in the range -55°C through 125°C.
        /// </summary>
        /// <returns>
        /// A <see cref = "Temperature" /> object whose value is a 12-bit representation of the last temperature measurement taken by the device.
        /// </returns>
        public Temperature GetHighResolutionTemperature()
        {
            // Register reads must be done in the following order.
            byte[] temperature = RegisterHelper.ReadRegisterBlock(_i2cDevice, (byte)Register.Temperature, 2);
            byte countsRemaining = RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.CountsRemaining);
            byte countsPerDegree = RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.CountsPerDegree);

            double highResolutionTemperature = temperature[0] - 0.25d + ((countsPerDegree - countsRemaining) / (double)countsPerDegree);

            return Temperature.FromDegreesCelsius(highResolutionTemperature);
        }

        #endregion

        #region High Temperature Alarm

        /// <summary>
        /// Gets or sets the current high temperature alarm threshold.
        /// </summary>
        /// <remarks>
        /// The temperature will be clamped to the the range -55°C through 125°C and rounded to the nearest 0.5°C.
        /// </remarks>
        public Temperature HighTemperatureAlarm
        {
            get
            {
                byte[] highTemperature = RegisterHelper.ReadRegisterBlock(_i2cDevice, (byte)Register.HighTemperature, 2);
                return UnpackTemperature(highTemperature[0], highTemperature[1]);
            }

            set
            {
                byte[] highTemperature = PackTemperature(value);
                RegisterHelper.WriteRegisterBlock(_i2cDevice, (byte)Register.HighTemperature, highTemperature);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the device has triggered a high temperature alarm.
        /// </summary>
        public bool HasHighTemperatureAlarm
        {
            get
            {
                return RegisterHelper.RegisterBitIsSet(_i2cDevice, (byte)Register.Configuration, (byte)RegisterMask.HighTemperatureAlarmMask);
            }
        }

        /// <summary>
        /// Clears the high temperature alarm flag.
        /// </summary>
        public void ResetHighTemperatureAlarm()
        {
            RegisterHelper.ClearRegisterBit(_i2cDevice, (byte)Register.Configuration, (byte)RegisterMask.HighTemperatureAlarmMask);
        }

        #endregion

        #region Low Temperature Alarm

        /// <summary>
        /// Gets or sets the current low temperature alarm threshold.
        /// </summary>
        /// <remarks>
        /// The temperature will be clamped to the the range -55°C through 125°C and rounded to the nearest 0.5°C.
        /// </remarks>
        public Temperature LowTemperatureAlarm
        {
            get
            {
                byte[] lowTemperature = RegisterHelper.ReadRegisterBlock(_i2cDevice, (byte)Register.LowTemperature, 2);
                return UnpackTemperature(lowTemperature[0], lowTemperature[1]);
            }

            set
            {
                byte[] lowTemperature = PackTemperature(value);
                RegisterHelper.WriteRegisterBlock(_i2cDevice, (byte)Register.LowTemperature, lowTemperature);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the device has triggered a low temperature alarm.
        /// </summary>
        public bool HasLowTemperatureAlarm
        {
            get
            {
                return RegisterHelper.RegisterBitIsSet(_i2cDevice, (byte)Register.Configuration, (byte)RegisterMask.LowTemperatureAlarmMask);
            }
        }

        /// <summary>
        /// Clears the low temperature alarm flag.
        /// </summary>
        public void ResetLowTemperatureAlarm()
        {
            RegisterHelper.ClearRegisterBit(_i2cDevice, (byte)Register.Configuration, (byte)RegisterMask.LowTemperatureAlarmMask);
        }

        #endregion

        /// <summary>
        /// Gets or sets the polarity of the thermostat output pin when a temperature alarm is asserted.
        /// </summary>
        public PinValue OutputPolarity
        {
            get
            {
                if (RegisterHelper.RegisterBitIsSet(_i2cDevice, (byte)Register.Configuration, (byte)RegisterMask.OutputPolarityMask))
                {
                    return PinValue.High;
                }

                return PinValue.Low;
            }

            set
            {
                if (value == PinValue.High)
                {
                    RegisterHelper.SetRegisterBit(_i2cDevice, (byte)Register.Configuration, (byte)RegisterMask.OutputPolarityMask);
                }
                else
                {
                    RegisterHelper.ClearRegisterBit(_i2cDevice, (byte)Register.Configuration, (byte)RegisterMask.OutputPolarityMask);
                }
            }
        }

        /// <summary>
        /// Gets or sets the mode used when the device receives a temperature measurement command.
        /// </summary>
        public MeasurementMode MeasurementMode
        {
            get
            {
                if (RegisterHelper.RegisterBitIsSet(_i2cDevice, (byte)Register.Configuration, (byte)RegisterMask.OneShotConversionModeMask))
                {
                    return MeasurementMode.Single;
                }

                return MeasurementMode.Continuous;
            }

            set
            {
                if (value == MeasurementMode.Single)
                {
                    RegisterHelper.SetRegisterBit(_i2cDevice, (byte)Register.Configuration, (byte)RegisterMask.OneShotConversionModeMask);
                }
                else
                {
                    RegisterHelper.ClearRegisterBit(_i2cDevice, (byte)Register.Configuration, (byte)RegisterMask.OneShotConversionModeMask);
                }
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
        }
    }
}
