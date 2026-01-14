// Copyright (c) 2017 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.
using System;
using System.Buffers.Binary;

namespace Iot.Device.Sen5x.Entities
{
    /// <summary>
    /// Represents measured values from the sensor.
    /// </summary>
    public class DeviceStatus : AbstractReadEntity
    {
        internal DeviceStatus()
        {
            // Internal constructor, because this is a read-only entity.
        }

        internal override int ByteCount => 6;

        internal override void FromSpanByte(Span<byte> data)
        {
            Span<byte> status = new byte[4];
            status[0] = data[0];
            status[1] = data[1];
            status[2] = data[3];
            status[3] = data[4];
            RawDeviceStatus = BinaryPrimitives.ReadUInt32BigEndian(status);
        }

        /// <summary>
        /// Gets or sets the plain uint32 status register from the sensor.
        /// </summary>
        public uint RawDeviceStatus { get; protected set; }

        /// <summary>
        /// Fan speed is too high or too low.
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item>During the first 3 seconds after starting the measurement (fan start-up) the fan speed is not checked.</item>
        /// <item>The fan speed is also not checked during the auto cleaning procedure.</item>
        /// <item>Apart from the two exceptions mentioned above, the fan speed is checked once per second in the measurement mode. If it is out of range twice in succession, the SPEED-bit is set.</item>
        /// <item>At very high or low ambient temperatures, the fan may take longer to reach its target speed after start-up. In this case, the bit will be set. As soon as the target speed is reached, this bit is cleared automatically.</item>
        /// <item>If this bit is constantly set, this indicates a problem with the power supply or that the fan is no longer working properly.</item>
        /// </list>
        /// </remarks>
        public bool FanSpeedWarning => (RawDeviceStatus & (1 << 21)) > 0;

        /// <summary>
        /// Active during the automatic cleaning procedure of the fan.
        /// </summary>
        public bool FanCleaningActive => (RawDeviceStatus & (1 << 19)) > 0;

        /// <summary>
        /// Gas sensor error.
        /// </summary>
        public bool GasSensorError => (RawDeviceStatus & (1 << 7)) > 0;

        /// <summary>
        /// Error in internal communication with the RHT sensor.
        /// </summary>
        public bool RhtCommunicationError => (RawDeviceStatus & (1 << 6)) > 0;

        /// <summary>
        /// Laser is switched on and current is out of range.
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item>The laser current is checked once per second in the measurement mode. If it is out of range, the LASER-bit is set.</item>
        /// <item>If the laser current is back within limits, this bit will be not cleared automatically.</item>
        /// <item>A laser failure can occur at very high temperatures outside of specifications or when the laser module is defective</item>
        /// </list>
        /// </remarks>
        public bool LaserFailure => (RawDeviceStatus & (1 << 5)) > 0;

        /// <summary>
        /// Fan is switched on, but the measured fan speed is 0 RPM.
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item>The fan is checked once per second in the measurement mode. If 0 RPM is measured twice in succession, the FAN bit is set.</item>
        /// <item>The FAN-bit will not be cleared automatically.</item>
        /// <item>A fan failure can occur if the fan is mechanically blocked or broken.</item>
        /// </list>
        /// </remarks>
        public bool FanSpeedError => (RawDeviceStatus & (1 << 4)) > 0;
    }
}
