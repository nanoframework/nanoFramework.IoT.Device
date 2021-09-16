// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Magnetometer;
using System;

namespace Iot.Device.Magnetometer
{
    /// <summary>
    /// Implements the Bmm150 magnetic field data (off-chip) temperature compensation functions
    /// https://www.bosch-sensortec.com/media/boschsensortec/downloads/datasheets/bst-bmm150-ds001.pdf
    /// Page 15
    /// </summary>
    public class Bmm150Compensation
    {
        /// <summary>
        /// Returns the compensated magnetometer x axis data(micro-tesla) in float.
        /// More details, permalink: https://github.com/BoschSensortec/BMM150-Sensor-API/blob/a20641f216057f0c54de115fe81b57368e119c01/bmm150.c#L1614
        /// </summary>
        /// <param name="x">axis raw value</param>
        /// <param name="rhall">temperature compensation value (RHALL) </param>
        /// <param name="trimData">trim registers values</param>
        /// <returns>compensated magnetometer x axis data(micro-tesla) in float</returns>
        public static double Compensate_x(double x, uint rhall, Bmm150TrimRegisterData trimData)
        {
            float retval = 0;
            float process_comp_x0;
            float process_comp_x1;
            float process_comp_x2;
            float process_comp_x3;
            float process_comp_x4;
            int BMM150_OVERFLOW_ADCVAL_XYAXES_FLIP = -4096;

            /* Overflow condition check */
            if ((x != BMM150_OVERFLOW_ADCVAL_XYAXES_FLIP) && (rhall != 0) && (trimData.dig_xyz1 != 0))
            {
                /* Processing compensation equations */
                process_comp_x0 = (((float)trimData.dig_xyz1) * 16384.0f / rhall);
                retval = (process_comp_x0 - 16384.0f);
                process_comp_x1 = ((float)trimData.dig_xy2) * (retval * retval / 268435456.0f);
                process_comp_x2 = process_comp_x1 + retval * ((float)trimData.dig_xy1) / 16384.0f;
                process_comp_x3 = ((float)trimData.dig_x2) + 160.0f;
                process_comp_x4 = (float)(x * ((process_comp_x2 + 256.0f) * process_comp_x3));
                retval = ((process_comp_x4 / 8192.0f) + (((float)trimData.dig_x1) * 8.0f)) / 16.0f;
            }
            else
            {
                /* Overflow, set output to 0.0f */
                retval = 0.0f;
            }

            return retval;
        }

        /// <summary>
        /// Returns the compensated magnetometer y axis data(micro-tesla) in float.
        /// More details, permalink: https://github.com/BoschSensortec/BMM150-Sensor-API/blob/a20641f216057f0c54de115fe81b57368e119c01/bmm150.c#L1648
        /// </summary>
        /// <param name="y">axis raw value</param>
        /// <param name="rhall">temperature compensation value (RHALL) </param>
        /// <param name="trimData">trim registers values</param>
        /// <returns>compensated magnetometer y axis data(micro-tesla) in float</returns>
        public static double Compensate_y(double y, uint rhall, Bmm150TrimRegisterData trimData)
        {
            float retval = 0;
            float process_comp_y0;
            float process_comp_y1;
            float process_comp_y2;
            float process_comp_y3;
            float process_comp_y4;
            int BMM150_OVERFLOW_ADCVAL_XYAXES_FLIP = -4096;

            /* Overflow condition check */
            if ((y != BMM150_OVERFLOW_ADCVAL_XYAXES_FLIP) && (rhall != 0) && (trimData.dig_xyz1 != 0))
            {
                /* Processing compensation equations */
                process_comp_y0 = ((float)trimData.dig_xyz1) * 16384.0f / rhall;
                retval = process_comp_y0 - 16384.0f;
                process_comp_y1 = ((float)trimData.dig_xy2) * (retval * retval / 268435456.0f);
                process_comp_y2 = process_comp_y1 + retval * ((float)trimData.dig_xy1) / 16384.0f;
                process_comp_y3 = ((float)trimData.dig_y2) + 160.0f;
                process_comp_y4 = (float)(y * (((process_comp_y2) + 256.0f) * process_comp_y3));
                retval = ((process_comp_y4 / 8192.0f) + (((float)trimData.dig_y1) * 8.0f)) / 16.0f;
            }
            else
            {
                /* Overflow, set output to 0.0f */
                retval = 0.0f;
            }

            return retval;
        }

        /// <summary>
        /// Returns the compensated magnetometer z axis data(micro-tesla) in float.
        /// More details, permalink: https://github.com/BoschSensortec/BMM150-Sensor-API/blob/a20641f216057f0c54de115fe81b57368e119c01/bmm150.c#L1682
        /// </summary>
        /// <param name="z">axis raw value</param>
        /// <param name="rhall">temperature compensation value (RHALL) </param>
        /// <param name="trimData">trim registers values</param>
        /// <returns>compensated magnetometer z axis data(micro-tesla) in float</returns>
        public static double Compensate_z(double z, uint rhall, Bmm150TrimRegisterData trimData)
        {
            float retval = 0;
            float process_comp_z0;
            float process_comp_z1;
            float process_comp_z2;
            float process_comp_z3;
            float process_comp_z4;
            float process_comp_z5;
            int BMM150_OVERFLOW_ADCVAL_ZAXIS_HALL = -16384;

            /* Overflow condition check */
            if ((z != BMM150_OVERFLOW_ADCVAL_ZAXIS_HALL) && (trimData.dig_z2 != 0) &&
                (trimData.dig_z1 != 0) && (trimData.dig_xyz1 != 0) && (rhall != 0))
            {
                /* Processing compensation equations */
                process_comp_z0 = ((float)z) - ((float)trimData.dig_z4);
                process_comp_z1 = ((float)rhall) - ((float)trimData.dig_xyz1);
                process_comp_z2 = (((float)trimData.dig_z3) * process_comp_z1);
                process_comp_z3 = ((float)trimData.dig_z1) * ((float)rhall) / 32768.0f;
                process_comp_z4 = ((float)trimData.dig_z2) + process_comp_z3;
                process_comp_z5 = (process_comp_z0 * 131072.0f) - process_comp_z2;
                retval = (process_comp_z5 / ((process_comp_z4) * 4.0f)) / 16.0f;
            }
            else
            {
                /* Overflow, set output to 0.0f */
                retval = 0.0f;
            }

            return retval;
        }
    }
}
