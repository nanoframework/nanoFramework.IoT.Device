// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Iot.Device.Ft4222
{
    /// <summary>
    /// Common static functions for the FT4222
    /// </summary>
    public class FtCommon
    {
        /// <summary>
        /// Returns the list of FT4222 connected
        /// </summary>
        /// <returns>A list of devices connected</returns>
        public static ListFtDevice GetDevices()
        {
            ListFtDevice devInfos = new ListFtDevice();
            FtStatus ftStatus = 0;

            // Check device
            uint numOfDevices;
            ftStatus = FtFunction.FT_CreateDeviceInfoList(out numOfDevices);

            Debug.WriteLine($"Number of devices: {numOfDevices}");
            if (numOfDevices == 0)
            {
                throw new IOException($"No device found");
            }

            SpanByte sernum = new byte[16];
            SpanByte desc = new byte[64];
            for (uint i = 0; i < numOfDevices; i++)
            {
                uint flags = 0;
                FtDeviceType ftDevice;
                uint id;
                uint locId;
                IntPtr handle;
                ftStatus = FtFunction.FT_GetDeviceInfoDetail(i, out flags, out ftDevice, out id, out locId,
                    in MemoryMarshal.GetReference(sernum), in MemoryMarshal.GetReference(desc), out handle);
                if (ftStatus != FtStatus.Ok)
                {
                    throw new IOException($"Can't read device information on device index {i}, error {ftStatus}");
                }

                var devInfo = new FtDevice(
                    (FtFlag)flags,
                    ftDevice,
                    id,
                    locId,
                    Encoding.ASCII.GetString(sernum.ToArray(), 0, FindFirstZero(sernum)),
                    Encoding.ASCII.GetString(desc.ToArray(), 0, FindFirstZero(desc)));
                devInfos.Add(devInfo);
            }

            return devInfos;
        }

        private static int FindFirstZero(SpanByte span)
        {
            for (int i = 0; i < span.Length; i++)
            {
                if (span[i] == 0)
                {
                    return i;
                }
            }

            return span.Length;
        }

        /// <summary>
        /// Get the versions of the chipset and dll
        /// </summary>
        /// <returns>Both the chipset and dll versions</returns>
        public static (Version? Chip, Version? Dll) GetVersions()
        {
            // First, let's find a device
            var devices = GetDevices();
            if (devices.Count == 0)
            {
                return (null, null);
            }

            // Check if the first not open device
            int idx = 0;
            for (idx = 0; idx < devices.Count; idx++)
            {
                if ((devices[idx].Flags & FtFlag.PortOpened) != FtFlag.PortOpened)
                {
                    break;
                }
            }

            if (idx == devices.Count)
            {
                throw new InvalidOperationException($"Can't find any open device to check the versions");
            }

            var ftStatus = FtFunction.FT_OpenEx(devices[idx].LocId, FtOpenType.OpenByLocation, out SafeFtHandle ftHandle);
            if (ftStatus != FtStatus.Ok)
            {
                throw new IOException($"Can't open the device to check chipset version, status: {ftStatus}");
            }

            FtVersion ftVersion;
            ftStatus = FtFunction.FT4222_GetVersion(ftHandle, out ftVersion);
            if (ftStatus != FtStatus.Ok)
            {
                throw new IOException($"Can't find versions of chipset and FT4222, status: {ftStatus}");
            }

            ftHandle.Dispose();

            Version chip = new Version((int)(ftVersion.ChipVersion >> 24), (int)((ftVersion.ChipVersion >> 16) & 0xFF),
                (int)((ftVersion.ChipVersion >> 8) & 0xFF), (int)(ftVersion.ChipVersion & 0xFF));
            Version dll = new Version((int)(ftVersion.DllVersion >> 24), (int)((ftVersion.DllVersion >> 16) & 0xFF),
                (int)((ftVersion.DllVersion >> 8) & 0xFF), (int)(ftVersion.DllVersion & 0xFF));

            return (chip, dll);
        }
    }
}
