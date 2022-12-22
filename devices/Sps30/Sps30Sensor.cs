// Copyright (c) 2017 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.

using System.Buffers.Binary;
using System.Text;
using Iot.Device.Sps30.Entities;
using Iot.Device.Sps30.Shdlc;

namespace Iot.Device.Sps30
{
    /// <summary>
    /// Allows for interaction with the SPS30 particulate matter sensor. Uses the SHDLC protocol as specified by Sensirion.
    /// </summary>
    /// <remarks>
    /// Datasheet can be found at https://sensirion.com/media/documents/8600FF88/616542B5/Sensirion_PM_Sensors_Datasheet_SPS30.pdf.
    /// </remarks>
    public class Sps30Sensor
    {
        private readonly ShdlcProtocol _shdlc;

        /// <summary>
        /// Initializes a new instance of the <see cref="Sps30Sensor" /> class.
        /// </summary>
        /// <param name="shdlc">An initialized <see cref="ShdlcProtocol"/> instance.</param>
        public Sps30Sensor(ShdlcProtocol shdlc)
        {
            _shdlc = shdlc;
        }

        /// <summary>
        /// Starts the measurement. After power up, the module is in Idle-Mode. Before any measurement values can be read, the Measurement-Mode needs to be started using this command.
        /// </summary>
        /// <param name="format">The format for the measurement, see <see cref="MeasurementOutputFormat"/>.</param>
        public void StartMeasurement(MeasurementOutputFormat format)
        {
            _shdlc.Execute(0, 0x00, new byte[] { 0x01, (byte)format }, 20);
        }

        /// <summary>
        /// Stops the measurement. Use this command to return to the initial state (Idle-Mode).
        /// </summary>
        public void StopMeasurement()
        {
            _shdlc.Execute(0, 0x01, new byte[0], 20);
        }

        /// <summary>
        /// Reads the measured values from the module. This command can be used to poll for new measurement values. The measurement interval is 1 second.
        /// </summary>
        /// <returns>The parsed measurement, either Float or UInt16, depending on <see cref="StartMeasurement(MeasurementOutputFormat)"/>.</returns>
        public Measurement ReadMeasuredValues()
        {
            var data = _shdlc.Execute(0, 0x03, new byte[0], 20);
            if (data.Length == 0)
            {
                return null;
            }

            return new Measurement(data);
        }

        /// <summary>
        /// Enters the Sleep-Mode with minimum power consumption. This will also deactivate the UART interface, note the wake-up sequence described at the Wake-up command.
        /// </summary>
        public void Sleep()
        {
            _shdlc.Execute(0, 0x10, new byte[0], 5);
        }

        /// <summary>
        /// Use this command to switch from Sleep-Mode to Idle-Mode.
        /// </summary>
        public void WakeUp()
        {
            _shdlc.Serial.Write(new byte[] { 0xFF }, 0, 1); // Generate a low pulse to wake-up the interface
            _shdlc.Execute(0, 0x11, new byte[0], 5); // Send the wake-up command within 100ms to fully wake the device
        }

        /// <summary>
        /// Starts the fan-cleaning manually.
        /// </summary>
        public void StartFanCleaning()
        {
            _shdlc.Execute(0, 0x56, new byte[0], 20);
        }

        /// <summary>
        /// Reads the interval [s] of the periodic fan-cleaning.
        /// </summary>
        /// <returns>The auto cleaning interval in seconds.</returns>
        public uint GetAutoCleaningInterval()
        {
            var data = _shdlc.Execute(0, 0x80, new byte[] { 0x00 }, 20);
            return BinaryPrimitives.ReadUInt32BigEndian(data);
        }

        /// <summary>
        /// Writes the interval [s] of the periodic fan-cleaning.
        /// </summary>
        /// <param name="intervalInSeconds">The new interval in seconds.</param>
        public void SetAutoCleaningInterval(uint intervalInSeconds)
        {
            var newvalue = new byte[4];
            BinaryPrimitives.WriteUInt32BigEndian(newvalue, intervalInSeconds);
            _shdlc.Execute(0, 0x80, new byte[] { 0x00, newvalue[0], newvalue[1], newvalue[2], newvalue[3] }, 20);
        }

        /// <summary>
        /// This command returns product type with a maximum of 32 characters.
        /// </summary>
        /// <returns>The device product type as a string.</returns>
        public string GetDeviceInfoProductType()
        {
            var data = _shdlc.Execute(0, 0xD0, new byte[] { 0x00 }, 20);
            return Encoding.UTF8.GetString(data, 0, data.Length);
        }

        /// <summary>
        /// This command returns serial number with a maximum of 32 characters.
        /// </summary>
        /// <returns>The device serial number as a string.</returns>
        public string GetDeviceInfoSerialNumber()
        {
            var data = _shdlc.Execute(0, 0xD0, new byte[] { 0x03 }, 20);
            return Encoding.UTF8.GetString(data, 0, data.Length);
        }

        /// <summary>
        /// Gets version information about the firmware, hardware, and SHDLC protocol.
        /// </summary>
        /// <returns>The parsed version information.</returns>
        public VersionInformation ReadVersion()
        {
            var data = _shdlc.Execute(0, 0xD1, new byte[0], 20);
            return new VersionInformation(data);
        }

        /// <summary>
        /// Use this command to read the Device Status Register.
        /// </summary>
        /// <param name="clearBitsAfterRead">True to clear any persistent error bits after reading the status.</param>
        /// <returns>The parsed device status.</returns>
        public DeviceStatus ReadDeviceStatusRegister(bool clearBitsAfterRead = false)
        {
            var data = _shdlc.Execute(0, 0xD2, new byte[] { (byte)(clearBitsAfterRead ? 0x01 : 0x0) }, 20);
            return new DeviceStatus(data);
        }

        /// <summary>
        /// Soft reset command. After calling this command, the module is in the same state as after a Power-Reset. The reset is executed after sending the MISO response frame.
        /// </summary>
        public void DeviceReset()
        {
            _shdlc.Execute(0, 0xD3, new byte[0], 20);
        }
    }
}
