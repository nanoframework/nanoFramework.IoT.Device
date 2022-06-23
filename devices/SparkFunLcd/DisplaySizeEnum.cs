// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.SparkFunLcd
{
    using System;
    using System.Device.I2c;
    using System.Drawing;
    using System.Threading;
    using Iot.Device.CharacterLcd;

    /// <summary>
    /// LCD library for SparkFun RGB Serial Open LCD display (sizes 20x4 or 16x2) with I2C connection
    /// for product information see https://www.sparkfun.com/products/16398
    /// code based on https://github.com/sparkfun/OpenLCD
    /// </summary>
    public partial class SparkFunLcd
    {
        /// <summary>
        /// Display size
        /// </summary>
        public enum DisplaySizeEnum
        {
            /// <summary>
            /// Display size 20 x 4
            /// </summary>
            Size20x4,

            /// <summary>
            /// Display size 16 x 2
            /// </summary>
            Size16x2
        }
    }
}
