// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;

namespace Iot.Device.Max1704x
{
    /// <summary>
    /// Base class for controlling MAX1704x via I2C interface.
    /// </summary>
    public abstract class Max1704X
    {
        /// <summary>
        /// The default I2C address.
        /// </summary>
        public const byte DefaultAddress = 0x36;
        
        private const byte ThresholdMinValue = 32;
        
        private readonly I2cDevice _i2CDevice;

        /// <summary>
        /// Gets or sets the compensation value for the model gauge algorithm.
        /// The upper 8 bits of the CONFIG register control the compensation.
        /// </summary>
        /// <value>
        /// The compensation value as a byte.
        /// </value>
        public byte Compensation
        {
            get
            {
                var configReg = GetConfigRegister();
                var compensation = (configReg & 0xFF00) >> 8;
                return (byte)compensation;
            }

            set
            {
                var configReg = GetConfigRegister();
                configReg &= 0x00FF;
                configReg |= value;
                Write16(configReg, Registers.Max17043Config);
            }
        }

        /// <summary>
        /// Gets the voltage of the battery.
        /// </summary>
        /// <returns>The voltage of the battery in volts.</returns>
        public float BatteryVoltage
        {
            get
            {
                var vCell = Read16(Registers.Max17043Vcell);
                return vCell / Divider * FullScale;    
            }
        }

        /// <summary>
        /// Gets the battery percentage.
        /// </summary>
        /// <returns>The battery percentage as a float value.</returns>
        public float BatteryPercent
        {
            get
            {
                var soc = Read16(Registers.Max17043Soc);
                var percent = (float)((soc & 0xFF00) >> 8);
                percent += (soc & 0x00FF) / 256.0f;
                return percent;    
            }
        }

        /// <summary>
        /// Gets the version of the device.
        /// </summary>
        /// <value>The version of the device.</value>
        public ushort Version => Read16(Registers.Max17043Version);

        /// <summary>
        /// Gets or sets the threshold of the property.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the value is out of range.</exception>
        public byte Threshold
        {
            get
            {
                var configReg = GetConfigRegister();
                var threshold = configReg & 0x001F;
                threshold = 32 - threshold;
                return (byte)threshold;    
            }

            set
            {
                if (value > 100 || value < ThresholdMinValue)
                {
                    throw new ArgumentOutOfRangeException();
                }
                
                var percent = ThresholdMinValue - value;
                var configReg = GetConfigRegister();
                configReg &= 0xFFE0;
                configReg |= (ushort)percent;
                Write16(configReg, Registers.Max17043Config);
            }
        }

        /// <summary>
        /// Gets a value indicating whether an alert is active.
        /// </summary>
        /// <value>
        /// <c>true</c> if an alert is active; otherwise, <c>false</c>.
        /// </value>
        public bool Alert
        {
            get
            {
                var configReg = GetConfigRegister();
                if ((configReg & Registers.Max17043ConfigAlert) != 0)
                {
                    return true;
                }

                return false;    
            }
        }

        /// <summary>
        /// Gets the maximum value that the property can have or represent.
        /// </summary>
        protected abstract float FullScale { get; }

        /// <summary>
        /// Gets the value used to divide two numbers in the implementation of a division operation.
        /// </summary>
        protected abstract float Divider { get; }
        
        /// <summary>
        /// Initializes a new instance of the Max1704X class.
        /// </summary>
        /// <param name="i2CDevice">The I2C device used for communication with device.</param>
        protected Max1704X(I2cDevice i2CDevice)
        {
            _i2CDevice = i2CDevice;
        }

        /// <summary>
        /// Clears the alert state of the MAX17043 device.
        /// </summary>
        public void ClearAlert()
        {
            var configReg = GetConfigRegister();
            configReg &= Negate(Registers.Max17043ConfigAlert);
            Write16(configReg, Registers.Max17043Config);
        }

        /// <summary>
        /// Puts the device into sleep mode.
        /// In sleep mode, the IC halts all operations, reducing current
        /// consumption to below 1μA. After exiting sleep mode,
        /// the IC continues normal operation. In sleep mode, the
        /// IC does not detect self-discharge. If the battery changes
        /// state while the IC sleeps, the IC cannot detect it, causing
        /// SOC error. Wake up the IC before charging or discharging.
        /// </summary>
        public virtual void Sleep()
        {
            var configReg = GetConfigRegister();
            if ((configReg & Registers.Max17043ConfigSleep) != 0)
            {
                return;
            }

            configReg |= Registers.Max17043ConfigSleep;
            Write16(configReg, Registers.Max17043Config);
        }

        /// <summary>
        /// Wakes up the device from sleep mode.
        /// </summary>
        public virtual void Wake()
        {
            var configReg = GetConfigRegister();
            if ((configReg & Registers.Max17043ConfigSleep) == 0)
            {
                return;
            }

            configReg &= Negate(Registers.Max17043ConfigSleep);
            Write16(configReg, Registers.Max17043Config);
        }
        
        /// <summary>
        /// A quick-start allows the MAX17043 to restart fuel-gauge calculations in the
        /// same manner as initial power-up of the IC. If an application’s power-up
        /// sequence is exceedingly noisy such that excess error is introduced into the
        /// IC’s “first guess” of SOC, the host can issue a quick-start to reduce the
        /// error. A quick-start is initiated by a rising edge on the QSTRT pin, or
        /// through software by writing 4000h to MODE register.
        /// Note: on the MAX17048/49 this will also clear / disable EnSleep.
        /// </summary>
        public void QuickStart()
        {
            Write16(Registers.Max17043ModeQuickstart, Registers.Max17043Mode);
        }

        /// <summary>
        /// Reset the device to its initial state by writing a specific value to the CMD Register.
        /// This causes the device to reset as if power had been removed, and the reset occurs when
        /// the last bit of the value has been clocked in.
        /// </summary>
        public void Reset()
        {
            Write16(Registers.Max17043CommandPor, Registers.Max17043Command);
        }

        /// <summary>
        /// Negates a byte value by performing a bitwise complement operation.
        /// </summary>
        /// <param name="value">The byte value to negate.</param>
        /// <returns>The negation of the byte value.</returns>
        protected static byte Negate(byte value)
        {
            return (byte)~value;
        }

        /// <summary>
        /// Negates the given unsigned short value by performing a bitwise NOT operation.
        /// </summary>
        /// <param name="value">The unsigned short value to be negated.</param>
        /// <returns>The result of the bitwise NOT operation on the value.</returns>
        protected static ushort Negate(ushort value)
        {
            return (ushort)~value;
        }

        /// <summary>
        /// Negates the given integer value.
        /// </summary>
        /// <param name="value">The value to be negated.</param>
        /// <returns>The negated value as an unsigned 16-bit integer.</returns>
        protected static ushort Negate(int value)
        {
            return (ushort)~value;
        }
        
        /// <summary>
        /// Retrieves the value of the config register from the MAX17043 register.
        /// </summary>
        /// <returns>The config register value as an unsigned short.</returns>
        protected ushort GetConfigRegister()
        {
            return Read16(Registers.Max17043Config);
        }

        /// <summary>
        /// Writes a 16-bit value to the specified I2C address.
        /// </summary>
        /// <param name="data">The 16-bit value to write.</param>
        /// <param name="address">The register address.</param>
        /// <exception cref="Exception">Thrown when the write operation fails to complete.</exception>
        protected void Write16(ushort data, byte address)
        {
            var result = _i2CDevice.Write(new[] { address, ToMsbByte(data), ToLsbByte(data) });
            if (result.Status != I2cTransferStatus.FullTransfer)
            {
                throw new Exception();
            }
        }

        /// <summary>
        /// Reads a 16-bit value from the specified address.
        /// </summary>
        /// <param name="address">The address from which to read the value.</param>
        /// <returns>The 16-bit value read from the specified address.</returns>
        /// <exception cref="Exception">Thrown if the read transfer is not successful.</exception>
        protected ushort Read16(byte address)
        {
            var readBuffer = new byte[2];
            var result = _i2CDevice.WriteRead(new[] { address }, readBuffer);
            if (result.Status != I2cTransferStatus.FullTransfer)
            {
                throw new Exception();
            }

            return FromByteArray(readBuffer);
        }
        
        private static ushort FromByteArray(byte[] array)
        {
            return (ushort)((array[0] << 8) | array[1]);
        }

        private static byte ToMsbByte(int input)
        {
            return (byte)((input & 0xFF00) >> 8);
        }

        private static byte ToLsbByte(int input)
        {
            return (byte)(input & 0x00FF);
        }
    }
}