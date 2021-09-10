// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Magnetometer
{
    /// <summary>
    ///  Represents the trim registers of the sensor (trim values in the "trim_data" of device structure).
    /// </summary>
    public class Bmm150TrimRegister
    {
        public byte dig_x1 { get; set; }

        public byte dig_y1 { get; set; }

        public byte dig_x2 { get; set; }

        public byte dig_y2 { get; set; }

        public int dig_z1 { get; set; }

        public int dig_z2 { get; set; }

        public int dig_z3 { get; set; }

        public int dig_z4 { get; set; }

        public int dig_xy1 { get; set; }

        public int dig_xy2 { get; set; }

        public int dig_xyz1 { get; set; }
    }
}
