////
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
////

namespace Iot.Device.Bq25798
{
    /// <summary>
    /// Battery voltage thresholds for the transition from precharge to fast charge, which is defined as a ratio of battery regulation limit (VREG).
    /// </summary>
    public enum ThresholdFastCharge
    {
        /// <summary>
        /// 15% of VREG.
        /// </summary>
        Vreg15 = 0,

        /// <summary>
        /// 62.2% of VREG.
        /// </summary>
        Vreg62_2 = 1,

        /// <summary>
        /// 66.7% of VREG.
        /// </summary>
        Vreg66_7 = 2,

        /// <summary>
        /// 71.4% of VREG.
        /// </summary>
        Vreg71_4 = 3,
    }
}
