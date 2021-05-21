// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.I2c;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Iot.Device.Ft4222
{
    /// <summary>
    /// FT4222 I2C Device
    /// </summary>
    internal class Ft4222I2cBus : I2cBus
    {
        private const uint I2cMasterFrequencyKbps = 400;

        private SafeFtHandle _ftHandle;
        private HashSet<int> _usedAddresses = new HashSet<int>();

        /// <summary>
        /// Store the FTDI Device Information
        /// </summary>
        public FtDevice DeviceInformation { get; private set; }

        /// <summary>
        /// Create a FT4222 I2C Device
        /// </summary>
        /// <param name="deviceInformation">Device information. Use FtCommon.GetDevices to get it.</param>
        public Ft4222I2cBus(FtDevice deviceInformation)
        {
            switch (deviceInformation.Type)
            {
                case FtDeviceType.Ft4222HMode0or2With2Interfaces:
                case FtDeviceType.Ft4222HMode1or2With4Interfaces:
                case FtDeviceType.Ft4222HMode3With1Interface:
                    break;
                default: throw new ArgumentException($"Unknown device type: {deviceInformation.Type}");
            }

            DeviceInformation = deviceInformation;
            // Open device
            var ftStatus = FtFunction.FT_OpenEx(DeviceInformation.LocId, FtOpenType.OpenByLocation, out _ftHandle);

            if (ftStatus != FtStatus.Ok)
            {
                throw new IOException($"Failed to open device {DeviceInformation.Description}, status: {ftStatus}");
            }

            // Set the clock
            FtClockRate ft4222Clock = FtClockRate.Clock24MHz;

            ftStatus = FtFunction.FT4222_SetClock(_ftHandle, ft4222Clock);
            if (ftStatus != FtStatus.Ok)
            {
                throw new IOException($"Failed set clock rate {ft4222Clock} on device: {DeviceInformation.Description}, status: {ftStatus}");
            }

            // Set the device as I2C Master
            ftStatus = FtFunction.FT4222_I2CMaster_Init(_ftHandle, I2cMasterFrequencyKbps);
            if (ftStatus != FtStatus.Ok)
            {
                throw new IOException($"Failed to initialize I2C Master mode on device: {DeviceInformation.Description}, status: {ftStatus}");
            }
        }

        internal void Read(int deviceAddress, SpanByte buffer)
        {
            if (deviceAddress < 0 || deviceAddress > ushort.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(deviceAddress));
            }

            if (buffer.Length > ushort.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(buffer), $"Buffer is too large. Maximum length is {ushort.MaxValue}.");
            }

            ushort bytesRead;
            var ftStatus = FtFunction.FT4222_I2CMaster_Read(_ftHandle, (ushort)deviceAddress, in MemoryMarshal.GetReference(buffer), (ushort)buffer.Length, out bytesRead);
            if (ftStatus != FtStatus.Ok)
            {
                throw new IOException($"{nameof(Read)} failed to read, error: {ftStatus}");
            }

            if (bytesRead != buffer.Length)
            {
                throw new IOException($"Number of bytes read ({bytesRead}) doesn't match length of the buffer ({buffer.Length}).");
            }
        }

        internal void Write(int deviceAddress, SpanByte buffer)
        {
            if (deviceAddress < 0 || deviceAddress > ushort.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(deviceAddress));
            }

            if (buffer.Length > ushort.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(buffer), $"Buffer is too large. Maximum length is {ushort.MaxValue}.");
            }

            ushort bytesSent;
            var ftStatus = FtFunction.FT4222_I2CMaster_Write(_ftHandle, (ushort)deviceAddress, in MemoryMarshal.GetReference(buffer), (ushort)buffer.Length, out bytesSent);
            if (ftStatus != FtStatus.Ok)
            {
                throw new IOException($"{nameof(Write)} failed to write, error: {ftStatus}");
            }
        }

        /// <inheritdoc/>
        public override I2cDevice CreateDevice(int deviceAddress)
        {
            if (!_usedAddresses.Add(deviceAddress))
            {
                throw new ArgumentException($"Device with address 0x{deviceAddress,0X2} is already open.", nameof(deviceAddress));
            }

            return new Ft4222I2c(this, deviceAddress);
        }

        /// <inheritdoc/>
        public override void RemoveDevice(int deviceAddress)
        {
            if (!_usedAddresses.Remove(deviceAddress))
            {
                throw new ArgumentException($"Device with address 0x{deviceAddress,0X2} was not open.", nameof(deviceAddress));
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            _ftHandle?.Dispose();
            _ftHandle = null!;
        }
    }
}
