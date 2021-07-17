// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Pn5180
{
    /// <summary>
    /// Versions of the reader
    /// </summary>
    public class TripletVersion
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="product">Product version</param>
        /// <param name="firmware">Firmware version</param>
        /// <param name="eeprom">EEPROM version</param>
        public TripletVersion(Version product, Version firmware, Version eeprom)
        {
            Product = product;
            Firmware = firmware;
            Eeprom = eeprom;
        }

        /// <summary>
        /// Product version
        /// </summary>
        public Version Product { get; set; }

        /// <summary>
        /// Firmware version
        /// </summary>
        public Version Firmware { get; set; }

        /// <summary>
        /// EEPROM version
        /// </summary>
        public Version Eeprom { get; set; }
    }
}
