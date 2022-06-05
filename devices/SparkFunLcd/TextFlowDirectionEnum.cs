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
        /// Text flow direction
        /// </summary>
        /// <see cref="SetTextFlowDirectionState"/>
        public enum TextFlowDirectionEnum
        {
            /// <summary>
            /// Left to right, the direction common to most Western languages
            /// </summary>
            LeftToRight,

            /// <summary>
            /// Right to left
            /// </summary>
            RightToLeft
        }
    }
}
