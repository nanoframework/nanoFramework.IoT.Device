﻿// Copyright (c) 2017 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.

using System;
using System.Buffers.Binary;
using System.Device.I2c;
using System.Threading;
using Iot.Device.Sen5x.Entities;
using Iot.Device.Sen5x.Extensions;

namespace Iot.Device.Sen5x
{
    /// <summary>
    /// Documentation for this public class.
    /// </summary>
    public class Sen5xSensor
    {
        /// <summary>
        /// The I2C address as specified in the datasheet.
        /// </summary>
        public const byte DefaultI2cAddress = 0x69;

        /// <summary>
        /// The I2C bus speed as specified in the datasheet.
        /// </summary>
        public const I2cBusSpeed DefaultI2cBusSpeed = I2cBusSpeed.StandardMode;

        private I2cDevice _i2c;

        /// <summary>
        /// Initializes a new instance of the <see cref="Sen5xSensor"/> class.
        /// </summary>
        /// <param name="i2cDevice">The I2cDevice to which this sensor is connected.</param>
        public Sen5xSensor(I2cDevice i2cDevice)
        {
            _i2c = i2cDevice;
        }

        /// <summary>
        /// Starts the measurement. After power up, the module is in Idle-Mode. Before any measurement values can be read, the Measurement-Mode needs to be started using this command.
        /// </summary>
        public void StartMeasurement()
        {
            Read(0x0021, TimeSpan.FromMilliseconds(50));
        }

        /// <summary>
        /// Starts a continuous measurement without PM. Only humidity, temperature, VOC, and NOx are available in this mode. Laser and fan are switched off to keep power consumption low.
        /// </summary>
        public void StartMeasurementWithoutPM()
        {
            Read(0x0037, TimeSpan.FromMilliseconds(50));
        }

        /// <summary>
        /// Stops the measurement. Use this command to return to the initial state (Idle-Mode).
        /// </summary>
        public void StopMeasurement()
        {
            Read(0x0104, TimeSpan.FromMilliseconds(200));
        }

        /// <summary>
        /// This command can be used for polling to find out when new measurements are available.
        /// </summary>
        /// <returns>True when data is ready to be read, false otherwise.</returns>
        public bool ReadDataReadyFlag()
        {
            var data = Read(0x0202, TimeSpan.FromMilliseconds(20), new byte[3]);
            return data[1] > 0;
        }

        /// <summary>
        /// Reads the measured values from the sensor module and resets the "Data-Ready Flag". If the sensor module is in Measurement-Mode, an updated measurement value is provided
        /// every second and the "Data-Ready Flag" is set. If no synchronized readout is desired, the "Data-Ready Flag" can be ignored. The command "Read Measured Values" always returns
        /// the latest measured values. In RHT/Gas-Only Measurement Mode, the PM output is 0xFFFF. If any value is unknown, 0xFFFF is returned.
        /// </summary>
        /// <returns>The measured values.</returns>
        public Measurement ReadMeasurement()
        {
            return (Measurement)Read(0x03c4, TimeSpan.FromMilliseconds(20), new Measurement());
        }

        /// <summary>
        /// These commands allow to compensate temperature effects of the design-in at customer side by applying a custom temperature offset to the ambient temperature.
        /// </summary>
        /// <returns>The current temperature compensation parameters.</returns>
        public TemperatureCompensationParameters ReadTemperatureCompensationParameters()
        {
            return (TemperatureCompensationParameters)Read(0x60b2, TimeSpan.FromMilliseconds(20), new TemperatureCompensationParameters());
        }

        /// <summary>
        /// These commands allow to compensate temperature effects of the design-in at customer side by applying a custom temperature offset to the ambient temperature.
        /// </summary>
        /// <param name="value">The new temperature compensation parameters to be set.</param>
        public void WriteTemperatureCompensationParameters(TemperatureCompensationParameters value)
        {
            Write(0x60b2, TimeSpan.FromMilliseconds(20), value);
        }

        /// <summary>
        /// The temperature compensation algorithm is optimized for a cold start by default, i.e., it is assumed that the "Start Measurement" commands are called on a device not yet warmed up by previous measurements.
        /// If the measurement is started on a device that is already warmed up, this parameter can be used to improve the initial accuracy of the ambient temperature output. This parameter can be gotten and
        /// set in any state of the device, but it is applied only the next time starting a measurement, i.e., when sending a "Start Measurement" command.So, the parameter needs to be written before a warm-start
        /// measurement is started.
        /// </summary>
        /// <returns>The warm start parameter.</returns>
        public WarmStartParameters ReadWarmStartParameter()
        {
            return (WarmStartParameters)Read(0x60c6, TimeSpan.FromMilliseconds(20), new WarmStartParameters());
        }

        /// <summary>
        /// The temperature compensation algorithm is optimized for a cold start by default, i.e., it is assumed that the "Start Measurement" commands are called on a device not yet warmed up by previous measurements.
        /// If the measurement is started on a device that is already warmed up, this parameter can be used to improve the initial accuracy of the ambient temperature output. This parameter can be gotten and
        /// set in any state of the device, but it is applied only the next time starting a measurement, i.e., when sending a "Start Measurement" command.So, the parameter needs to be written before a warm-start
        /// measurement is started.
        /// </summary>
        /// <param name="value">The new warm start parameter to be set.</param>
        public void WriteWarmStartParameter(WarmStartParameters value)
        {
            Write(0x60c6, TimeSpan.FromMilliseconds(20), value);
        }

        /// <summary>
        /// The VOC algorithm can be customized by tuning 6 different parameters. Note that this command is available only in idle mode. In measure mode, this command has no effect. In addition, it has no effect if at least one parameter is outside the specified range.
        /// </summary>
        /// <returns>The algorithm tuning parametes for VOC.</returns>
        public AlgorithmTuningParameters ReadVocAlgorithmTuningParameters()
        {
            return (AlgorithmTuningParameters)Read(0x60d0, TimeSpan.FromMilliseconds(20), new AlgorithmTuningParameters());
        }

        /// <summary>
        /// The VOC algorithm can be customized by tuning 6 different parameters. Note that this command is available only in idle mode. In measure mode, this command has no effect. In addition, it has no effect if at least one parameter is outside the specified range.
        /// </summary>
        /// <param name="value">The new algorithm tuning parameters for VOC.</param>
        public void WriteVocAlgorithmTuningParameters(AlgorithmTuningParameters value)
        {
            Write(0x60d0, TimeSpan.FromMilliseconds(20), value);
        }

        /// <summary>
        /// The NOx algorithm can be customized by tuning 6 different parameters. Note that this command is available only in idle mode. In measure mode, this command has no effect. In addition, it has no effect if at least one parameter is outside the specified range.
        /// </summary>
        /// <returns>The algorithm tuning parametes for NOx.</returns>
        public AlgorithmTuningParameters ReadNoxAlgorithmTuningParameters()
        {
            return (AlgorithmTuningParameters)Read(0x60e1, TimeSpan.FromMilliseconds(20), new AlgorithmTuningParameters());
        }

        /// <summary>
        /// The NOx algorithm can be customized by tuning 6 different parameters. Note that this command is available only in idle mode. In measure mode, this command has no effect. In addition, it has no effect if at least one parameter is outside the specified range.
        /// </summary>
        /// <param name="value">The new algorithm tuning parameters for NOx.</param>
        public void WriteNoxAlgorithmTuningParameters(AlgorithmTuningParameters value)
        {
            Write(0x60e1, TimeSpan.FromMilliseconds(20), value);
        }

        /// <summary>
        /// By default, the RH/T acceleration algorithm is optimized for a sensor which is positioned in free air. If the sensor is integrated into another
        /// device, the ambient RH/T output values might not be optimal due to different thermal behavior. This parameter can be used to read the RH/T
        /// acceleration behavior for the actual use-case, leading in an improvement of the ambient RH/T output accuracy.
        /// </summary>
        /// <returns>The parameters for the RH/T acceleration mode.</returns>
        public RhtAccelerationModeParameters ReadRhtAccelerationMode()
        {
            return (RhtAccelerationModeParameters)Read(0x60f7, TimeSpan.FromMilliseconds(20), new RhtAccelerationModeParameters());
        }

        /// <summary>
        /// By default, the RH/T acceleration algorithm is optimized for a sensor which is positioned in free air. If the sensor is integrated into another
        /// device, the ambient RH/T output values might not be optimal due to different thermal behavior. This parameter can be used to adapt the RH/T
        /// acceleration behavior for the actual use-case, leading in an improvement of the ambient RH/T output accuracy.
        /// </summary>
        /// <param name="value">The new RH/T acceleration mode parameters.</param>
        public void WriteRhtAccelerationMode(RhtAccelerationModeParameters value)
        {
            Write(0x60f7, TimeSpan.FromMilliseconds(20), value);
        }
        
        /// <summary>
        /// Allows to backup and restore the VOC algorithm state to resume operation after a short interruption, skipping initial learning phase. By default,
        /// the VOC algorithm resets its state to initial values each time a measurement is started, even if the measurement was stopped only for a short time.
        /// So, the VOC index output value needs a long time until it is stable again. This can be avoided by restoring the previously memorized algorithm
        /// state before starting the measure mode.
        /// </summary>
        /// <returns>The current VOC algorithm state.</returns>
        public VocAlgorithmState ReadVocAlgorithmState()
        {
            return (VocAlgorithmState)Read(0x6181, TimeSpan.FromMilliseconds(20), new VocAlgorithmState());
        }

        /// <summary>
        /// Allows to backup and restore the VOC algorithm state to resume operation after a short interruption, skipping initial learning phase. By default,
        /// the VOC algorithm resets its state to initial values each time a measurement is started, even if the measurement was stopped only for a short time.
        /// So, the VOC index output value needs a long time until it is stable again. This can be avoided by restoring the previously memorized algorithm
        /// state before starting the measure mode.
        /// </summary>
        /// <param name="value">The new VOC algorithm state to write.</param>
        public void WriteVocAlgorithmState(VocAlgorithmState value)
        {
            Write(0x6181, TimeSpan.FromMilliseconds(20), value);
        }

        /// <summary>
        /// Starts the fan-cleaning manually. This command can only be executed in Measurement-Mode.
        /// </summary>
        public void StartFanCleaning()
        {
            Read(0x5607, TimeSpan.FromMilliseconds(20));
        }

        /// <summary>
        /// Reads the interval [s] of the periodic fan-cleaning.
        /// </summary>
        /// <returns>The current auto cleaning interval parameters.</returns>
        public AutoCleaningIntervalParameters ReadAutoCleaningInterval()
        {
            return (AutoCleaningIntervalParameters)Read(0x8004, TimeSpan.FromMilliseconds(20), new AutoCleaningIntervalParameters());
        }

        /// <summary>
        /// Writes the interval [s] of the periodic fan-cleaning. Please note that since this configuration is volatile, it will be reverted to the default value after a device reset.
        /// </summary>
        /// <param name="value">The new auto cleaning interval to be set.</param>
        public void WriteAutoCleaningInterval(AutoCleaningIntervalParameters value)
        {
            Write(0x8004, TimeSpan.FromMilliseconds(20), value);
        }

        /// <summary>
        /// This command returns the product name SEN5x (SEN50, SEN54 or SEN55).
        /// </summary>
        /// <returns>The product name SEN5x (SEN50, SEN54 or SEN55).</returns>
        public string ReadProductName()
        {
            return ((StringValue)Read(0xd014, TimeSpan.FromMilliseconds(20), new StringValue(32))).Text;
        }

        /// <summary>
        /// This command returns the requested serial number.
        /// </summary>
        /// <returns>The serial number.</returns>
        public string ReadSerialNumber()
        {
            return ((StringValue)Read(0xd033, TimeSpan.FromMilliseconds(20), new StringValue(32))).Text;
        }

        /// <summary>
        /// Get firmware version. There is no Major/Minor, only a single value.
        /// </summary>
        /// <returns>The firmware version.</returns>
        public byte ReadFirmwareVersion()
        {
            var data = Read(0xd100, TimeSpan.FromMilliseconds(20), new byte[3]);
            return data[0];
        }

        /// <summary>
        /// Use this command to read the Device Status Register.
        /// </summary>
        /// <returns>The device status.</returns>
        public DeviceStatus ReadDeviceStatus()
        {
            return (DeviceStatus)Read(0xd206, TimeSpan.FromMilliseconds(20), new DeviceStatus());
        }

        /// <summary>
        /// Clears all flags in device status register.
        /// </summary>
        public void ClearDeviceStatus()
        {
            Read(0xd210, TimeSpan.FromMilliseconds(20));
        }

        /// <summary>
        /// Device software reset command. After calling this command, the module is in the same state as after a power reset.
        /// </summary>
        public void DeviceReset()
        {
            Read(0xd304, TimeSpan.FromMilliseconds(100));
        }

        private AbstractReadEntity Read(ushort cmd, TimeSpan commandExecutionTime, AbstractReadEntity entity = null)
        {
            SpanByte data = new byte[2];
            BinaryPrimitives.WriteUInt16BigEndian(data, cmd);
            var result = _i2c.Write(data);
            if (result.Status != I2cTransferStatus.FullTransfer)
            {
                throw new InvalidOperationException($"Unexpected status ({result.Status}) on I2C write");
            }

            Thread.Sleep(commandExecutionTime);
            if (entity != null)
            {
                SpanByte response = new byte[entity.ByteCount];
                result = _i2c.Read(response);
                if (result.Status != I2cTransferStatus.FullTransfer)
                {
                    throw new InvalidOperationException($"Unexpected status ({result.Status}) on I2C read");
                }

                response.VerifyCrc();
                entity.FromSpanByte(response);
            }

            return entity;
        }

        private SpanByte Read(ushort cmd, TimeSpan commandExecutionTime, SpanByte response)
        {
            SpanByte data = new byte[2];
            BinaryPrimitives.WriteUInt16BigEndian(data, cmd);
            var result = _i2c.Write(data);
            if (result.Status != I2cTransferStatus.FullTransfer)
            {
                throw new InvalidOperationException($"Unexpected status ({result.Status}) on I2C write");
            }

            Thread.Sleep(commandExecutionTime);
            if (response.Length > 0)
            {
                result = _i2c.Read(response);
                if (result.Status != I2cTransferStatus.FullTransfer)
                {
                    throw new InvalidOperationException($"Unexpected status ({result.Status}) on I2C read");
                }

                response.VerifyCrc();
            }

            return response;
        }

        private void Write(ushort cmd, TimeSpan commandExecutionTime, AbstractReadWriteEntity entity)
        {
            SpanByte data = new byte[2 + entity.ByteCount];
            BinaryPrimitives.WriteUInt16BigEndian(data, cmd);
            SpanByte entityPart = data.Slice(2);
            entity.ToSpanByte(entityPart);
            entityPart.UpdateCrc();
            var result = _i2c.Write(data);
            if (result.Status != I2cTransferStatus.FullTransfer)
            {
                throw new InvalidOperationException($"Unexpected status ({result.Status}) on I2C write");
            }

            Thread.Sleep(commandExecutionTime);
        }
    }
}
