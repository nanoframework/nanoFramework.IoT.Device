// Copyright (c) 2017 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.

using System;
using System.Buffers.Binary;
using System.IO.Ports;
using Iot.Device.Scd30.Entities;
using UnitsNet;

namespace Iot.Device.Scd30
{
    /// <summary>
    /// Sensirion SCD30 Sensor Module (CO2, humidity, and temperature sensor).
    /// </summary>
    /// <remarks>
    /// Most of the SCD30 documentation below is derived or copied from the original documentation
    /// at https://sensirion.com/media/documents/D7CEEF4A/6165372F/Sensirion_CO2_Sensors_SCD30_Interface_Description.pdf .
    /// </remarks>
    public class Scd30Sensor
    {
        private const byte Scd30ModbusAddress = 0x61;

        private readonly SerialPort _serial;

        /// <summary>
        /// Initializes a new instance of the <see cref="Scd30Sensor" /> class to control the SCD30 via <see cref="SerialPort"/>. The BaudRate, DataBits, Parity, StopBits and timeouts will be configured automatically to interact with the SCD30.
        /// </summary>
        /// <param name="serial">The serial port to use to communicate with the SCD30 sensor.</param>
        public Scd30Sensor(SerialPort serial)
        {
            _serial = serial;
            ConfigureSerialPortForScd30();
        }

        private void ConfigureSerialPortForScd30()
        {
            _serial.BaudRate = 19200;
            _serial.DataBits = 8;
            _serial.Parity = Parity.None;
            _serial.StopBits = StopBits.One;
            _serial.ReadTimeout = 1000;
            _serial.WriteTimeout = 1000;

            if (!_serial.IsOpen)
            {
                _serial.Open();
            }
        }

        /// <summary>
        /// <para>
        ///     Starts continuous measurement of the SCD30 to measure CO2 concentration, humidity and temperature. Measurement data
        ///     which is not read from the sensor will be overwritten. The measurement interval is adjustable via
        ///     <see cref="SetMeasurementInterval(TimeSpan)"/>. Initial measurement rate is 2s.
        /// </para>
        /// <para>
        ///     Continuous measurement status is saved in non-volatile memory. When the sensor is powered down while continuous
        ///     measurement mode is active SCD30 will measure continuously after repowering without sending the measurement command.
        ///     The CO2 measurement value can be compensated for ambient pressure by feeding the pressure value in mBar to the sensor.
        /// </para>
        /// <para>
        ///     Setting the ambient pressure will overwrite previous settings of altitude compensation. Setting the argument to zero will
        ///     deactivate the ambient pressure compensation (default ambient pressure = 1013.25 mBar).
        /// </para>
        /// </summary>
        public void StartContinuousMeasurement()
        {
            StartContinuousMeasurement(Pressure.FromMillibars(0));
        }

        /// <summary>
        /// <para>
        ///     Starts continuous measurement of the SCD30 to measure CO2 concentration, humidity and temperature. Measurement data
        ///     which is not read from the sensor will be overwritten. The measurement interval is adjustable via
        ///     <see cref="SetMeasurementInterval(TimeSpan)"/>. Initial measurement rate is 2s.
        /// </para>
        /// <para>
        ///     Continuous measurement status is saved in non-volatile memory. When the sensor is powered down while continuous
        ///     measurement mode is active SCD30 will measure continuously after repowering without sending the measurement command.
        /// </para>
        /// <para>
        ///     The CO2 measurement value can be compensated for ambient pressure by feeding the pressure value in mBar to the sensor.
        ///     Setting the ambient pressure will overwrite previous settings of altitude compensation. Setting the argument to zero will
        ///     deactivate the ambient pressure compensation (default ambient pressure = 1013.25 mBar).
        /// </para>
        /// </summary>
        /// <param name="ambientPressureCompentation">Set 0 millibars to use default ambient pressure (1013.25mBar), or pass the pressure to use instead (range 700-1400 millibars).</param>
        /// <exception cref="ArgumentOutOfRangeException">When the pressure is not within the acceptable range.</exception>
        public void StartContinuousMeasurement(Pressure ambientPressureCompentation)
        {
            if (ambientPressureCompentation.Millibars != 0 && (ambientPressureCompentation.Millibars < 700 || ambientPressureCompentation.Millibars > 1400))
            {
                throw new ArgumentOutOfRangeException();
            }

            ModbusWriteSingleHoldingRegister(ModbusRegister.START_MEASUREMENT_ADDR, (ushort)ambientPressureCompentation.Millibars);
        }

        /// <summary>
        /// Stops the continuous measurement of the SCD30.
        /// </summary>
        public void StopContinuousMeasurement()
        {
            ModbusWriteSingleHoldingRegister(ModbusRegister.STOP_MEASUREMENT_ADDR, 1);
        }

        /// <summary>
        /// Sets the interval used by the SCD30 sensor to measure in continuous measurement mode (see chapter 1.4.1). Initial value is 2 s. The chosen measurement interval is saved in non-volatile memory and thus is not reset to its initial value after power up.
        /// </summary>
        /// <param name="measurementInterval">The new measurement interval. Must be within 2 seconds and 1800 seconds, and will be floored to full seconds if a smaller unit is used.</param>
        /// <exception cref="ArgumentOutOfRangeException">When the given interval is not within the acceptable range.</exception>
        public void SetMeasurementInterval(TimeSpan measurementInterval)
        {
            if (measurementInterval.TotalSeconds < 2 || measurementInterval.TotalSeconds > 1800)
            {
                throw new ArgumentOutOfRangeException();
            }

            ushort seconds = (ushort)measurementInterval.TotalSeconds;
            ModbusWriteSingleHoldingRegister(ModbusRegister.MEASUREMENT_INTERVAL_ADDR, seconds);
        }

        /// <summary>
        /// Reads the currently set measurement interval on the SCD30 sensor.
        /// </summary>
        /// <returns><see cref="TimeSpan"/> containing the currently set measurement interval.</returns>
        public TimeSpan GetMeasurementInterval()
        {
            var data = ModbusReadHoldingRegisters(ModbusRegister.MEASUREMENT_INTERVAL_ADDR, 1);
            return TimeSpan.FromSeconds(BinaryPrimitives.ReadUInt16BigEndian(data));
        }

        /// <summary>
        /// <para>
        ///     Data ready command is used to determine if a measurement can be read from the sensor’s buffer. Whenever there is a
        ///     measurement available from the internal buffer this command returns 1 and 0 otherwise. As soon as the measurement has been
        ///     read by the return value changes to 0. Note that the read header should be send with a delay of > 3ms following the write
        ///     sequence.
        /// </para>
        /// <para>
        ///     It is recommended to use data ready status byte before readout of the measurement values.
        /// </para>
        /// </summary>
        /// <returns>True when there's a measurement available to be read out, otherwise false.</returns>
        public bool GetDataReadyStatus()
        {
            var data = ModbusReadHoldingRegisters(ModbusRegister.DATA_READY_STATUS_ADDR, 1);
            return BinaryPrimitives.ReadUInt16BigEndian(data) == 1;
        }

        /// <summary>
        /// <para>
        ///     Data ready command is used to determine if a measurement can be read from the sensor’s buffer. Whenever there is a
        ///     measurement available from the internal buffer this command returns 1 and 0 otherwise. As soon as the measurement has been
        ///     read by the return value changes to 0.
        /// </para>
        /// <para>
        ///     It is recommended to use data ready status byte before readout of the measurement values.
        /// </para>
        /// </summary>
        /// <returns>The measured values.</returns>
        public Measurement ReadMeasurement()
        {
            var data = ModbusReadHoldingRegisters(ModbusRegister.READ_MEASUREMENT_ADDR, 6);
            return new Measurement(data);
        }

        /// <summary>
        /// <para>
        ///     Continuous automatic self-calibration can be (de-)activated with this command. When activated for the first time a
        ///     period of minimum 7 days is needed so that the algorithm can find its initial parameter set for ASC. The sensor has to be exposed
        ///     to fresh air for at least 1 hour every day. Also during that period, the sensor may not be disconnected from the power supply,
        ///     otherwise the procedure to find calibration parameters is aborted and has to be restarted from the beginning. The successfully
        ///     calculated parameters are stored in non-volatile memory of the SCD30 having the effect that after a restart the previously found
        ///     parameters for ASC are still present. Note that the most recently found self-calibration parameters will be actively used for
        ///     self-calibration disregarding the status of this feature. Finding a new parameter set by the here described method will always
        ///     overwrite the settings from external recalibration (see chapter 0) and vice-versa. The feature is switched off by default.
        /// </para>
        /// <para>
        ///     To work properly SCD30 has to see fresh air on a regular basis. Optimal working conditions are given when the sensor sees
        ///     fresh air for one hour every day so that ASC can constantly re-calibrate. ASC only works in continuous measurement mode.
        /// </para>
        /// <para>
        ///     ASC status is saved in non-volatile memory. When the sensor is powered down while ASC is activated SCD30 will continue with
        ///     automatic self-calibration after repowering without sending the command.
        /// </para>
        /// </summary>
        /// <param name="activate">True to activate automatic self-calibration, false to deactivate.</param>
        public void SetAutomaticSelfCalibration(bool activate)
        {
            ModbusWriteSingleHoldingRegister(ModbusRegister.AUTOMATIC_SELF_CALIBRATION_ADDR, (ushort)(activate ? 1 : 0));
        }

        /// <summary>
        /// Get the current setting for the ASC (Automatic Self Calibration). See <see cref="SetAutomaticSelfCalibration(bool)"/> for more information.
        /// </summary>
        /// <returns>True when Automatic Self Calibration is enabled, false otherwise.</returns>
        public bool GetAutomaticSelfCalibration()
        {
            var data = ModbusReadHoldingRegisters(ModbusRegister.AUTOMATIC_SELF_CALIBRATION_ADDR, 1);
            return BinaryPrimitives.ReadUInt16BigEndian(data) == 1;
        }

        /// <summary>
        /// <para>
        ///     Forced recalibration (FRC) is used to compensate for sensor drifts when a reference value of the CO2 concentration in close
        ///     proximity to the SCD30 is available. For best results, the sensor has to be run in a stable environment in continuous mode at a
        ///     measurement rate of 2s for at least two minutes before applying the FRC command and sending the reference value. Setting a
        ///     reference CO2 concentration by the method described here will always supersede corrections from the ASC (see chapter 1.4.6)
        ///     and vice-versa. The reference CO2 concentration has to be within the range 400 ppm ≤ cref(CO2) ≤ 2000 ppm.
        /// </para>
        /// <para>
        ///     The FRC method imposes a permanent update of the CO2 calibration curve which persists after repowering the sensor. The
        ///     most recently used reference value is retained in volatile memory and can be read out with the command sequence given below.
        /// </para>
        /// <para>
        ///     After repowering the sensor, the command will return the standard reference value of 400 ppm.
        /// </para>
        /// </summary>
        /// <param name="referenceCo2Concentration">The reference CO2 concentration in the range 400-2000 ppm.</param>
        public void SetForcedRecalibrationValue(VolumeConcentration referenceCo2Concentration)
        {
            if (referenceCo2Concentration.PartsPerMillion < 400 || referenceCo2Concentration.PartsPerMillion > 2000)
            {
                throw new ArgumentOutOfRangeException();
            }

            ModbusWriteSingleHoldingRegister(ModbusRegister.FORCED_RECALIBRATION_ADDR, (ushort)referenceCo2Concentration.PartsPerMillion);
        }

        /// <summary>
        /// Get the current value for the Forced Recalibration (FRC) setting. See <see cref="SetForcedRecalibrationValue(VolumeConcentration)"/> for more information.
        /// </summary>
        /// <returns>The current Forced Recalibration (FRC) value.</returns>
        public VolumeConcentration GetForcedRecalibrationValue()
        {
            var data = ModbusReadHoldingRegisters(ModbusRegister.FORCED_RECALIBRATION_ADDR, 1);
            return VolumeConcentration.FromPartsPerMillion(BinaryPrimitives.ReadUInt16BigEndian(data));
        }

        /// <summary>
        /// <para>
        ///     The on-board RH/T sensor is influenced by thermal self-heating of SCD30 and other electrical components. Design-in alters the
        ///     thermal properties of SCD30 such that temperature and humidity offsets may occur when operating the sensor in end-customer
        ///     devices. Compensation of those effects is achievable by writing the temperature offset found in continuous operation of the
        ///     device into the sensor.
        /// </para>
        /// <para>
        ///     Temperature offset value is saved in non-volatile memory. The last set value will be used for temperature offset compensation
        ///     after repowering.
        /// </para>
        /// </summary>
        /// <param name="temperatureOffset">The temperature offset to be used.</param>
        public void SetTemperatureOffset(Temperature temperatureOffset)
        {
            ushort offset = (ushort)(temperatureOffset.DegreesCelsius * 100);
            ModbusWriteSingleHoldingRegister(ModbusRegister.TEMPERATURE_OFFSET_ADDR, offset);
        }

        /// <summary>
        /// Get the currently configured temperature offset. See <see cref="SetTemperatureOffset(Temperature)"/> for more information.
        /// </summary>
        /// <returns>The currently configured temperature offset.</returns>
        public Temperature GetTemperatureOffset()
        {
            var data = ModbusReadHoldingRegisters(ModbusRegister.TEMPERATURE_OFFSET_ADDR, 1);
            return Temperature.FromDegreesCelsius(BinaryPrimitives.ReadUInt16BigEndian(data) / 100d);
        }

        /// <summary>
        /// <para>
        ///     Measurements of CO2 concentration based on the NDIR principle are influenced by altitude. SCD30 offers to compensate
        ///     deviations due to altitude by using this command. Setting altitude is disregarded when an ambient pressure is given to
        ///     the sensor, please see section 1.4.1.
        /// </para>
        /// <para>
        ///     Altitude value is saved in non-volatile memory. The last set value will be used for altitude compensation after repowering.
        /// </para>
        /// </summary>
        /// <param name="altitudeAboveSeaLevel">Height over sea level above 0.</param>
        public void SetAltitudeCompensation(Length altitudeAboveSeaLevel)
        {
            ModbusWriteSingleHoldingRegister(ModbusRegister.ALTITUDE_COMPENSATION_ADDR, (ushort)altitudeAboveSeaLevel.Meters);
        }

        /// <summary>
        /// Get the currently configured altitude compensation. See <see cref="SetAltitudeCompensation(Length)"/> for more information.
        /// </summary>
        /// <returns>The currently configured altitude compensation.</returns>
        public Length GetAltitudeCompensation()
        {
            var data = ModbusReadHoldingRegisters(ModbusRegister.ALTITUDE_COMPENSATION_ADDR, 1);
            return Length.FromMeters(BinaryPrimitives.ReadUInt16BigEndian(data));
        }

        /// <summary>
        /// This command can be used to read out the firmware version of SCD30 module.
        /// </summary>
        /// <returns>The firmware version of the SCD30 sensor.</returns>
        public Version ReadFirmwareVersion()
        {
            var data = ModbusReadHoldingRegisters(ModbusRegister.FIRMWARE_VERSION_ADDR, 1);
            return new Version(data[0], data[1]);
        }

        /// <summary>
        /// <para>
        ///     The SCD30 provides a soft reset mechanism that forces the sensor into the same state as after powering up without the need
        ///     for removing the power-supply. It does so by restarting its system controller. After soft reset the sensor will reload all calibrated
        ///     data. However, it is worth noting that the sensor reloads calibration data prior to every measurement by default. This includes
        ///     previously set reference values from ASC or FRC as well as temperature offset values last setting.
        /// </para>
        /// <para>
        ///     The sensor is able to receive the command at any time, regardless of its internal state.
        /// </para>
        /// </summary>
        public void SoftReset()
        {
            ModbusWriteSingleHoldingRegister(ModbusRegister.SOFT_RESET_ADDR, 1);
        }

        private void ModbusWriteSingleHoldingRegister(ModbusRegister register, ushort value)
        {
            // Function 0x6 request is always 8 bytes
            var request = new byte[8];
            var requestSpan = new Span<byte>(request);
            request[0] = Scd30ModbusAddress;
            request[1] = 0x6;
            BinaryPrimitives.WriteUInt16BigEndian(requestSpan.Slice(2, 2), (ushort)register);
            BinaryPrimitives.WriteUInt16BigEndian(requestSpan.Slice(4, 2), value);
            BinaryPrimitives.WriteUInt16BigEndian(requestSpan.Slice(6, 2), CalculateModbusRtuCrc16(requestSpan.Slice(0, 6)));

            // Function 0x6 response is always 8 bytes
            var response = new byte[8];

            // Execute
            ModbusWriteAndRead(request, response);
        }

        private Span<byte> ModbusReadHoldingRegisters(ModbusRegister register, byte number)
        {
            // Function 0x3 request is always 8 bytes
            var request = new byte[8];
            var requestSpan = new Span<byte>(request);
            request[0] = Scd30ModbusAddress;
            request[1] = 0x3;
            BinaryPrimitives.WriteUInt16BigEndian(requestSpan.Slice(2, 2), (ushort)register);
            BinaryPrimitives.WriteUInt16BigEndian(requestSpan.Slice(4, 2), number);
            BinaryPrimitives.WriteUInt16BigEndian(requestSpan.Slice(6, 2), CalculateModbusRtuCrc16(requestSpan.Slice(0, 6)));

            // Response size depends on the number of registers requested
            var response = new byte[5 + (2 * number)];

            // Execute
            ModbusWriteAndRead(request, response);

            // Return data from response
            return new Span<byte>(response, 3, number * 2);
        }

        private void ModbusWriteAndRead(byte[] request, byte[] response)
        {
            // Clear the serial buffer if there's anything
            while (_serial.BytesToRead > 0)
            {
                _serial.ReadByte();
            }

            // Write the request
            _serial.Write(request, 0, request.Length);

            // Wait for the expected amount of data (to fill the response array)
            _serial.WaitForData(response.Length);
            _serial.Read(response, 0, response.Length);

            // Verify the received message
            var responseSpan = new Span<byte>(response);
            var calculatedCrc = CalculateModbusRtuCrc16(responseSpan.Slice(0, response.Length - 2));
            var receivedCrc = BinaryPrimitives.ReadUInt16BigEndian(responseSpan.Slice(response.Length - 2));
            if (calculatedCrc != receivedCrc)
            {
                throw new ApplicationException($"Invalid checksum (calculated {calculatedCrc}, got {receivedCrc})");
            }

            // When the highest bit is set (function code | highest bit=0x80), then this indicates an exception on modbus level
            if ((request[1] | 0x80) == response[1])
            {
                throw new ApplicationException($"Response indicates an exception occurred (code 0x{response[2]:X2})");
            }
        }

        private ushort CalculateModbusRtuCrc16(Span<byte> data)
        {
            // Described in:
            //   https://sensirion.com/media/documents/D7CEEF4A/6165372F/Sensirion_CO2_Sensors_SCD30_Interface_Description.pdf
            // Example code from:
            //   https://ctlsys.com/support/how_to_compute_the_modbus_rtu_message_crc/
            ushort crc = 0xFFFF;

            for (int pos = 0; pos < data.Length; pos++)
            {
                crc ^= data[pos];

                for (int i = 8; i != 0; i--)
                {
                    if ((crc & 0x0001) != 0)
                    {
                        crc >>= 1;
                        crc ^= 0xA001;
                    }
                    else
                    {
                        crc >>= 1;
                    }
                }
            }

            // Note, this number has low and high bytes swapped, swap bytes
            return (ushort)((crc >> 8) | (crc << 8));
        }
    }
}
