// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Magnetometer
{
    /// <summary>
    ///  Represents the trim registers of the sensor (trim values in the "trim_data" of device structure).
    /// </summary>
    public class Bmm150TrimRegisterData
    {
        /// <summary>
        /// trim dig_x1 data
        /// </summary>
        public byte dig_x1 { get; set; }

        /// <summary>
        /// trim dig_y1 data
        /// </summary>
        public byte dig_y1 { get; set; }

        /// <summary>
        /// trim dig_x2 data
        /// </summary>
        public byte dig_x2 { get; set; }

        /// <summary>
        /// trim dig_y2 data
        /// </summary>
        public byte dig_y2 { get; set; }

        /// <summary>
        /// trim dig_z1 data
        /// </summary>
        public int dig_z1 { get; set; }

        /// <summary>
        /// trim dig_z2 data
        /// </summary>
        public int dig_z2 { get; set; }

        /// <summary>
        /// trim dig_z3 data
        /// </summary>
        public int dig_z3 { get; set; }

        /// <summary>
        /// trim dig_z4 data
        /// </summary>
        public int dig_z4 { get; set; }

        /// <summary>
        /// trim dig_xy1 data
        /// </summary>
        public int dig_xy1 { get; set; }

        /// <summary>
        /// trim dig_xy2 data
        /// </summary>
        public int dig_xy2 { get; set; }

        /// <summary>
        /// trim dig_xyz1 data
        /// </summary>
        public int dig_xyz1 { get; set; }

        /// <summary>
        /// Creates a new instace
        /// </summary>
        public Bmm150TrimRegisterData()
        {
        }

        /// <summary>
        /// Creates a new instace based on the trim registers SpanBytes
        /// </summary>
        /// <param name="trim_x1y1_data">trim_x1y1_data bytes</param>
        /// <param name="trim_xyz_data">trim_xyz_data bytes</param>
        /// <param name="trim_xy1xy2_data">trim_xy1xy2_data bytes</param>
        public Bmm150TrimRegisterData(SpanByte trim_x1y1_data, SpanByte trim_xyz_data, SpanByte trim_xy1xy2_data)
        {
            dig_x1 = (byte)trim_x1y1_data[0];
            dig_y1 = (byte)trim_x1y1_data[1];
            dig_x2 = (byte)trim_xyz_data[2];
            dig_y2 = (byte)trim_xyz_data[3];
            dig_z1 = trim_xy1xy2_data[3] << 8 | trim_xy1xy2_data[2];
            dig_z2 = (short)(trim_xy1xy2_data[1] << 8 | trim_xy1xy2_data[0]);
            dig_z3 = (short)(trim_xy1xy2_data[7] << 8 | trim_xy1xy2_data[6]);
            dig_z4 = (short)(trim_xyz_data[1] << 8 | trim_xyz_data[0]);
            dig_xy1 = trim_xy1xy2_data[9];
            dig_xy2 = (sbyte)trim_xy1xy2_data[8];
            dig_xyz1 = ((trim_xy1xy2_data[5] & 0x7F) << 8) | trim_xy1xy2_data[4];
        }
    }


}
