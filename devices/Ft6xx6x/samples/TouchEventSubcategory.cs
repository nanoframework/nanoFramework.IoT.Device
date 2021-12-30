// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.


using System;

namespace Iot.Device.Ft6xx6x.Samples
{
    /// <summary>
    /// The touch event sub category.
    /// </summary>
    [Flags]
    public enum TouchEventSubcategory
    {
        /// <summary>Unkown.</summary>
        Unknown = 1000,

        /// <summary>Left button.</summary>
        LeftButton = 1001,

        /// <summary>Middle button.</summary>
        MiddleButton = 1002,

        /// <summary>Right button.</summary>
        RightButton = 1004,

        /// <summary>Single touch.</summary>
        SingleTouch = 1008,

        /// <summary>Double touch.</summary>
        DoubleTouch = 10016
    }
}
