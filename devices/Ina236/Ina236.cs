// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using System.Device.I2c;
using System.Device.Model;
using UnitsNet;

namespace Iot.Device.Adc
{
    /// <summary>
    /// INA236 bidirectional current and power monitor.
    /// </summary>
    [Interface("INA236 Bidirectional Current/Power monitor")]
    public class Ina236 : IDisposable
    {
        /// <summary>
        /// The default I2C address for this device.
        /// </summary>
        public const byte DefaultI2cAddress = 0x40;

        private IIna236I2cDevice _i2cDevice;
        private readonly ElectricResistance _shuntResistance;
        private ElectricCurrent _currentLsb;
        private readonly ElectricCurrent _maxCurrent;
        private ElectricPotential _voltageLsb;

        /// <summary>
        /// Initializes an INA236 device.
        /// </summary>
        /// <param name="i2cDevice">The I2C device used to communicate with the INA236.</param>
        /// <param name="shuntResistance">The resistance of the measurement shunt.</param>
        /// <param name="maxCurrent">The maximum expected current.</param>
        public Ina236(I2cDevice i2cDevice, ElectricResistance shuntResistance, ElectricCurrent maxCurrent)
            : this(new Ina236I2cDevice(i2cDevice), shuntResistance, maxCurrent)
        {
        }

        internal Ina236(IIna236I2cDevice i2cDevice, ElectricResistance shuntResistance, ElectricCurrent maxCurrent)
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));
            _shuntResistance = shuntResistance;
            if (_shuntResistance.Ohms <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(shuntResistance), "The shunt resistance must be greater than zero.");
            }

            _maxCurrent = maxCurrent;
            Reset();
        }

        /// <summary>
        /// Resets the INA236 and configures its calibration register.
        /// </summary>
        [Command]
        public void Reset()
        {
            EnsureNotDisposed();
            WriteRegister(Ina236Register.Configuration, 0x8000);

            ushort deviceId = ReadRegisterUnsigned(Ina236Register.DeviceId);
            if ((deviceId & 0xFFF0) != 0xA080)
            {
                throw new InvalidOperationException("The device does not identify as an INA236.");
            }

            ElectricCurrent currentLsbMinimum = ElectricCurrent.FromAmperes(_maxCurrent.Amperes / 32768 * 2);
            int calibrationValue = (int)(0.00512 / (currentLsbMinimum.Amperes * _shuntResistance.Ohms));
            if (calibrationValue <= 0 || calibrationValue > ushort.MaxValue)
            {
                throw new InvalidOperationException("The calibration value is outside the supported range.");
            }

            _currentLsb = ElectricCurrent.FromAmperes(0.00512 / _shuntResistance.Ohms / calibrationValue);
            _voltageLsb = ElectricPotential.FromMicrovolts(2.5);
            WriteRegister(Ina236Register.Calibration, (ushort)calibrationValue);
        }

        /// <summary>
        /// Gets or sets the operating mode.
        /// </summary>
        [Property]
        public Ina236OperatingMode OperatingMode
        {
            get => (Ina236OperatingMode)(ReadRegisterUnsigned(Ina236Register.Configuration) & (ushort)Ina236OperatingMode.ModeMask);
            set
            {
                int registerValue = ReadRegisterUnsigned(Ina236Register.Configuration) & ~0b111;
                WriteRegister(Ina236Register.Configuration, (ushort)(registerValue | (int)value));
            }
        }

        /// <summary>
        /// Gets or sets the number of samples averaged into one result.
        /// </summary>
        [Property]
        public uint AverageOverNoSamples
        {
            get
            {
                int value = (ReadRegisterUnsigned(Ina236Register.Configuration) >> 9) & 0x7;
                uint[] samples = new uint[] { 1, 4, 16, 64, 128, 256, 512, 1024 };
                return samples[value];
            }

            set
            {
                int encodedValue;
                if (value <= 1) encodedValue = 0;
                else if (value <= 4) encodedValue = 1;
                else if (value <= 16) encodedValue = 2;
                else if (value <= 64) encodedValue = 3;
                else if (value <= 128) encodedValue = 4;
                else if (value <= 256) encodedValue = 5;
                else if (value <= 512) encodedValue = 6;
                else encodedValue = 7;

                int registerValue = ReadRegisterUnsigned(Ina236Register.Configuration) & 0xF1FF;
                WriteRegister(Ina236Register.Configuration, (ushort)(registerValue | (encodedValue << 9)));
            }
        }

        /// <summary>
        /// Gets or sets the bus-voltage conversion time in microseconds.
        /// </summary>
        public int BusConversionTime
        {
            get => ConversionPeriodFromValue((ReadRegisterUnsigned(Ina236Register.Configuration) >> 6) & 0x7);
            set
            {
                int registerValue = ReadRegisterUnsigned(Ina236Register.Configuration) & 0xFE3F;
                WriteRegister(Ina236Register.Configuration, (ushort)(registerValue | (ValueFromConversionPeriod(value) << 6)));
            }
        }

        /// <summary>
        /// Gets or sets the shunt-voltage conversion time in microseconds.
        /// </summary>
        public int ShuntConversionTime
        {
            get => ConversionPeriodFromValue((ReadRegisterUnsigned(Ina236Register.Configuration) >> 3) & 0x7);
            set
            {
                int registerValue = ReadRegisterUnsigned(Ina236Register.Configuration) & 0xFFC7;
                WriteRegister(Ina236Register.Configuration, (ushort)(registerValue | (ValueFromConversionPeriod(value) << 3)));
            }
        }

        /// <summary>
        /// Releases the underlying I2C device.
        /// </summary>
        public void Dispose()
        {
            _i2cDevice?.Dispose();
            _i2cDevice = null;
        }

        /// <summary>
        /// Reads the measured shunt voltage.
        /// </summary>
        /// <returns>The shunt potential difference.</returns>
        [Telemetry("ShuntVoltage")]
        public ElectricPotential ReadShuntVoltage() => ElectricPotential.FromVolts(ReadRegisterSigned(Ina236Register.ShuntVoltage) * _voltageLsb.Volts);

        /// <summary>
        /// Reads the measured bus voltage.
        /// </summary>
        /// <returns>The bus potential.</returns>
        [Telemetry("BusVoltage")]
        public ElectricPotential ReadBusVoltage() => ElectricPotential.FromMillivolts(ReadRegisterUnsigned(Ina236Register.BusVoltage) * 1.6);

        /// <summary>
        /// Reads the calculated current.
        /// </summary>
        /// <returns>The current through the shunt.</returns>
        [Telemetry("Current")]
        public ElectricCurrent ReadCurrent() => ElectricCurrent.FromAmperes(ReadRegisterSigned(Ina236Register.Current) * _currentLsb.Amperes);

        /// <summary>
        /// Reads the calculated power.
        /// </summary>
        /// <returns>The power consumed by the attached device.</returns>
        [Telemetry("Power")]
        public Power ReadPower() => Power.FromWatts(ReadRegisterUnsigned(Ina236Register.Power) * _currentLsb.Amperes * 32);

        private static int ValueFromConversionPeriod(int period)
        {
            if (period <= 140) return 0;
            if (period <= 204) return 1;
            if (period <= 332) return 2;
            if (period <= 588) return 3;
            if (period <= 1100) return 4;
            if (period <= 2116) return 5;
            if (period <= 4156) return 6;
            return 7;
        }

        private static int ConversionPeriodFromValue(int value)
        {
            int[] periods = new int[] { 140, 204, 332, 588, 1100, 2116, 4156, 8244 };
            return periods[value];
        }

        private ushort ReadRegisterUnsigned(Ina236Register register) => (ushort)ReadRegister(register);

        private short ReadRegisterSigned(Ina236Register register) => ReadRegister(register);

        private short ReadRegister(Ina236Register register)
        {
            EnsureNotDisposed();
            SpanByte buffer = new byte[2];
            buffer[0] = (byte)register;
            _i2cDevice.Write(buffer.Slice(0, 1));
            _i2cDevice.Read(buffer);
            return BinaryPrimitives.ReadInt16BigEndian(buffer);
        }

        private void WriteRegister(Ina236Register register, ushort value)
        {
            EnsureNotDisposed();
            SpanByte buffer = new byte[3];
            buffer[0] = (byte)register;
            BinaryPrimitives.WriteUInt16BigEndian(buffer.Slice(1, 2), value);
            _i2cDevice.Write(buffer);
        }

        private void EnsureNotDisposed()
        {
            if (_i2cDevice == null)
            {
                throw new ObjectDisposedException(nameof(Ina236));
            }
        }
    }

    internal interface IIna236I2cDevice : IDisposable
    {
        void Read(SpanByte buffer);

        void Write(SpanByte buffer);
    }

    internal sealed class Ina236I2cDevice : IIna236I2cDevice
    {
        private readonly I2cDevice _i2cDevice;

        public Ina236I2cDevice(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));
        }

        public void Read(SpanByte buffer) => _i2cDevice.Read(buffer);

        public void Write(SpanByte buffer) => _i2cDevice.Write(buffer);

        public void Dispose() => _i2cDevice.Dispose();
    }
}