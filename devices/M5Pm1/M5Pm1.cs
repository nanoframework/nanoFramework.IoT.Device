// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.I2c;
using System.IO;
using System.Threading;
using UnitsNet;

namespace Iot.Device.M5Pm1
{
    /// <summary>
    /// Driver for the M5Stack M5PM1 power-management IC used on ESP32-S3 boards such as the M5StickS3.
    /// </summary>
    /// <remarks>
    /// The M5PM1 handles the battery and USB power paths, charging control and status, board power gates
    /// and external 5V switching. This binding exposes the telemetry and control features that are most
    /// commonly used (battery/VBUS/5V-out voltage, external 5V enable, charge enable, charging status and
    /// the power source). The register behaviour is ported from the M5Stack <c>M5Unified</c> power
    /// implementation, which is the most authoritative reference for the M5PM1.
    /// </remarks>
    public class M5Pm1 : IDisposable
    {
        /// <summary>
        /// M5PM1 default I2C address.
        /// </summary>
        public const int I2cDefaultAddress = 0x6E;

        /// <summary>
        /// M5PM1 device identifier (value of registers 0x00-0x01).
        /// </summary>
        public const int DeviceId = 0x2050;

        // Number of attempts used to wake the PMIC when it sleeps on an idle I2C bus.
        private const int WakeUpAttempts = 100;

        // Register map (ported from the M5Stack M5Unified power implementation).
        private const byte RegDeviceId = 0x00;
        private const byte RegPowerSource = 0x04;
        private const byte RegPowerControl = 0x06;
        private const byte RegI2cConfig = 0x09;
        private const byte RegWatchdog = 0x0A;
        private const byte RegGpioMode = 0x10;
        private const byte RegGpioOutput = 0x11;
        private const byte RegGpioInput = 0x12;
        private const byte RegGpioDrive = 0x13;
        private const byte RegGpioFunction0 = 0x16;
        private const byte RegGpioFunction1 = 0x17;
        private const byte RegBatteryVoltage = 0x22;
        private const byte RegVbusVoltage = 0x24;
        private const byte RegOutputVoltage = 0x26;

        // Bit masks inside the power-control register (0x06).
        private const byte ChargeEnableBit = 0x01;
        private const byte ExternalOutputBit = 0x08;

        // Charging status bit inside the GPIO input register (0x12); the pin is low while charging.
        private const byte ChargingStatusBit = 0x01;

        // Power-source field (bits 2:0) of the power-source register (0x04).
        private const byte PowerSourceMask = 0x07;

        private readonly bool _shouldDispose;
        private I2cDevice _i2cDevice;

        /// <summary>
        /// Initializes a new instance of the <see cref="M5Pm1" /> class.
        /// </summary>
        /// <param name="i2cDevice">The I2C device used for communication with the M5PM1.</param>
        /// <param name="shouldDispose"><see langword="true" /> to dispose the <paramref name="i2cDevice"/> when this instance is disposed; otherwise, <see langword="false" />.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="i2cDevice"/> is <see langword="null" />.</exception>
        /// <exception cref="IOException">Thrown when the M5PM1 does not acknowledge on the I2C bus.</exception>
        public M5Pm1(I2cDevice i2cDevice, bool shouldDispose = true)
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentNullException();
            _shouldDispose = shouldDispose;

            if (!WakeUp())
            {
                throw new IOException();
            }
        }

        /// <summary>
        /// Gets the device identifier reported by the M5PM1.
        /// </summary>
        /// <returns>The device identifier, which should equal <see cref="DeviceId" /> (0x2050).</returns>
        public int GetDeviceId() => ReadRegister16(RegDeviceId);

        /// <summary>
        /// Gets a value indicating whether the battery is currently charging.
        /// </summary>
        public bool IsCharging => (ReadRegister(RegGpioInput) & ChargingStatusBit) == 0;

        /// <summary>
        /// Gets or sets a value indicating whether battery charging is enabled.
        /// </summary>
        public bool BatteryChargeEnabled
        {
            get => (ReadRegister(RegPowerControl) & ChargeEnableBit) != 0;
            set => UpdateRegister(RegPowerControl, ChargeEnableBit, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the external 5V output is enabled.
        /// </summary>
        public bool ExternalOutputEnabled
        {
            get => (ReadRegister(RegPowerControl) & ExternalOutputBit) != 0;
            set => UpdateRegister(RegPowerControl, ExternalOutputBit, value);
        }

        /// <summary>
        /// Gets the battery voltage.
        /// </summary>
        /// <returns>The battery voltage.</returns>
        public ElectricPotential GetBatteryVoltage()
        {
            return ElectricPotential.FromMillivolts(ReadRegister16(RegBatteryVoltage));
        }

        /// <summary>
        /// Gets the VBUS (USB input) voltage.
        /// </summary>
        /// <returns>The VBUS voltage.</returns>
        public ElectricPotential GetVbusVoltage()
        {
            return ElectricPotential.FromMillivolts(ReadRegister16(RegVbusVoltage));
        }

        /// <summary>
        /// Gets the external 5V output voltage.
        /// </summary>
        /// <returns>The 5V output voltage.</returns>
        public ElectricPotential GetOutputVoltage()
        {
            return ElectricPotential.FromMillivolts(ReadRegister16(RegOutputVoltage));
        }

        /// <summary>
        /// Gets the active power source (5V input, 5V input/output, or battery).
        /// </summary>
        /// <returns>The current <see cref="Iot.Device.M5Pm1.PowerSource" />, or <see cref="PowerSource.Unknown" /> if the register reports a value outside the documented range.</returns>
        public PowerSource GetPowerSource()
        {
            int source = ReadRegister(RegPowerSource) & PowerSourceMask;
            return source <= (int)PowerSource.Battery ? (PowerSource)source : PowerSource.Unknown;
        }

        /// <summary>
        /// Sets the direction (input or output) of a GPIO pin.
        /// </summary>
        /// <param name="pin">The GPIO pin to configure.</param>
        /// <param name="mode">The direction to apply. Only <see cref="PinMode.Input" /> and <see cref="PinMode.Output" /> are supported; the M5PM1 has no internal pull resistors.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="mode"/> is not <see cref="PinMode.Input" /> or <see cref="PinMode.Output" />.</exception>
        public void SetGpioMode(Pin pin, PinMode mode)
        {
            if (mode != PinMode.Input && mode != PinMode.Output)
            {
                throw new ArgumentException();
            }

            UpdateRegister(RegGpioMode, (byte)(1 << (int)pin), mode == PinMode.Output);
        }

        /// <summary>
        /// Sets the output driver type (push-pull or open-drain) of a GPIO pin.
        /// </summary>
        /// <param name="pin">The GPIO pin to configure.</param>
        /// <param name="drive">The driver type to apply.</param>
        public void SetGpioDrive(Pin pin, GpioDrive drive)
            => UpdateRegister(RegGpioDrive, (byte)(1 << (int)pin), drive == GpioDrive.OpenDrain);

        /// <summary>
        /// Sets the multiplexed function of a GPIO pin.
        /// </summary>
        /// <param name="pin">The GPIO pin to configure.</param>
        /// <param name="function">The function to apply.</param>
        public void SetGpioFunction(Pin pin, GpioFunction function)
        {
            // GPIO0-3 use FUNC0 (0x16); GPIO4 uses FUNC1 (0x17). Each pin occupies a 2-bit field.
            byte register = (int)pin < 4 ? RegGpioFunction0 : RegGpioFunction1;
            int shift = ((int)pin < 4 ? (int)pin : (int)pin - 4) * 2;
            byte mask = (byte)(0x03 << shift);
            byte current = ReadRegister(register);
            byte updated = (byte)((current & (0xFF - mask)) | ((byte)((byte)function << shift) & mask));
            WriteRegister(register, updated);
        }

        /// <summary>
        /// Sets the output level of a GPIO pin (when it is configured as an output).
        /// </summary>
        /// <param name="pin">The GPIO pin to drive.</param>
        /// <param name="value">The level to drive the pin to.</param>
        public void WriteGpio(Pin pin, PinValue value)
            => UpdateRegister(RegGpioOutput, (byte)(1 << (int)pin), value == PinValue.High);

        /// <summary>
        /// Reads the input level of a GPIO pin.
        /// </summary>
        /// <param name="pin">The GPIO pin to read.</param>
        /// <returns>The input level of the pin.</returns>
        public PinValue ReadGpio(Pin pin)
            => (ReadRegister(RegGpioInput) & (1 << (int)pin)) != 0 ? PinValue.High : PinValue.Low;

        /// <summary>
        /// Reads the output latch level of a GPIO pin (the last value written, not the physical input level).
        /// </summary>
        /// <param name="pin">The GPIO pin to read.</param>
        /// <returns>The output latch level of the pin.</returns>
        public PinValue ReadGpioOutputLatch(Pin pin)
            => (ReadRegister(RegGpioOutput) & (1 << (int)pin)) != 0 ? PinValue.High : PinValue.Low;

        // The M5PM1 sleeps when the I2C bus is idle and NAKs the first transaction, so a cold read comes
        // back as zeros. Poll the device-ID register until the PMIC acknowledges, then apply the M5Stack
        // reliability init (disable the I2C idle-sleep and the watchdog) so it stays responsive.
        private bool WakeUp()
        {
            SpanByte writeBuffer = new byte[] { RegDeviceId };
            SpanByte readBuffer = new byte[2];
            for (int attempt = 0; attempt < WakeUpAttempts; attempt++)
            {
                I2cTransferResult result = _i2cDevice.WriteRead(writeBuffer, readBuffer);
                if (result.Status == I2cTransferStatus.FullTransfer)
                {
                    WriteRegister(RegI2cConfig, 0x00);
                    WriteRegister(RegWatchdog, 0x00);
                    return true;
                }

                Thread.Sleep(2);
            }

            return false;
        }

        private void UpdateRegister(byte register, byte mask, bool set)
        {
            byte current = ReadRegister(register);
            byte updated = set ? (byte)(current | mask) : (byte)(current & (0xFF - mask));
            WriteRegister(register, updated);
        }

        private void WriteRegister(byte register, byte value)
        {
            SpanByte writeBuffer = new byte[2];
            writeBuffer[0] = register;
            writeBuffer[1] = value;
            _i2cDevice.Write(writeBuffer);
        }

        private byte ReadRegister(byte register)
        {
            SpanByte writeBuffer = new byte[1];
            writeBuffer[0] = register;
            SpanByte readBuffer = new byte[1];
            _i2cDevice.WriteRead(writeBuffer, readBuffer);
            return readBuffer[0];
        }

        // Reads a little-endian 16-bit value (low byte at 'register', high byte at 'register' + 1).
        private int ReadRegister16(byte register)
        {
            SpanByte writeBuffer = new byte[1];
            writeBuffer[0] = register;
            SpanByte readBuffer = new byte[2];
            _i2cDevice.WriteRead(writeBuffer, readBuffer);
            return (readBuffer[1] << 8) | readBuffer[0];
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_shouldDispose)
            {
                _i2cDevice?.Dispose();
            }

            _i2cDevice = null;
        }
    }
}
