// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace IoT.Device.Sim7080
{
    /// <summary>
    /// SIM Card Information. 
    /// </summary>
    public class SimCardInformation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SimCardInformation"/> class. 
        /// </summary>
        public SimCardInformation()
        {
        }

        /// <summary>
        /// Gets or sets  the International Mobile Equipment Identity (IMEI) number.
        /// </summary>
        public long IMEI { get; set; }

        /// <summary>
        /// Gets or sets  the International Mobile Subscriber Identity (IMSI) number.
        /// </summary>
        public long IMSI { get; set; }

        /// <summary>
        /// Gets or sets  the Integrated Circuit Card Identification (ICCM) number.
        /// </summary>
        public string ICCM { get; set; }
    }
}
