// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.SparkfunLcd
{
    using System;
    using System.Device.I2c;
    using System.Drawing;
    using System.Threading;
    using Iot.Device.CharacterLcd;

    public partial class SparkfunLcd
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
