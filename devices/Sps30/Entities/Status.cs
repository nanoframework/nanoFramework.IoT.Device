// Copyright (c) 2017 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.

using System;
using System.Buffers.Binary;

namespace Iot.Device.Sps30.Entities
{
    /// <summary>
    /// Parsed response after requesting the DeviceStatus.
    /// </summary>
    public class DeviceStatus
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceStatus" /> class.
        /// </summary>
        /// <param name="data">Raw data from the response.</param>
        /// <exception cref="ArgumentOutOfRangeException">When less than 4 bytes are provided.</exception>
        public DeviceStatus(byte[] data)
        {
            if (data.Length < 4)
            {
                throw new ArgumentOutOfRangeException(nameof(data), "Unexpected array size. Expecting at least 4 bytes."); // 5th byte is reserved
            }

            RawRegister = BinaryPrimitives.ReadUInt32BigEndian(data);
            FanFailureBlockedOrBroken = (RawRegister & (1 << 4)) > 0;
            LaserFailure = (RawRegister & (1 << 5)) > 0;
            FanSpeedOutOfRange = (RawRegister & (1 << 21)) > 0;
        }

        /// <summary>
        /// Gets the raw value returned by the device.
        /// </summary>
        public uint RawRegister { get; private set; }

        /// <summary>
        /// Gets a value indicating whether "Fan speed out of range" bit is set. It's either too high or too low. Check the SPS30 data sheet for more information.
        /// </summary>
        public bool FanSpeedOutOfRange { get; private set; }

        /// <summary>
        /// Gets a value indicating whether "Laser failure" bit is set. This can occur at high temperatures outside of specifications or when the laser module is defective. Check the SPS30 data sheet for more information.
        /// </summary>
        public bool LaserFailure { get; private set; }

        /// <summary>
        /// Gets a value indicating whether "Fan failure" bit is set. The fan may be mechanically blocked or broken. Check the SPS30 data sheet for more information.
        /// </summary>
        public bool FanFailureBlockedOrBroken { get; private set; }

        /// <summary>
        /// Conveniently show the status in a single string.
        /// </summary>
        /// <returns>The device status as a convenient string.</returns>
        public override string ToString()
        {
            return $"{nameof(RawRegister)}: {RawRegister}, {nameof(FanSpeedOutOfRange)}: {FanSpeedOutOfRange}, {nameof(LaserFailure)}: {LaserFailure}, {nameof(FanFailureBlockedOrBroken)}: {FanFailureBlockedOrBroken}";
        }
    }
}
