// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.AtModem.DTOs
{
    /// <summary>
    /// Represents a mobile network operator.
    /// </summary>
    public class Operator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Operator"/> class.
        /// </summary>
        public Operator()
        {
        }

        /// <summary>
        /// Gets or sets the type of the mobile network operator.
        /// </summary>
        public OperatorType OperatorType { get; set; }

        /// <summary>
        /// Gets or sets the name of the mobile network operator.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the short name or abbreviation of the mobile network operator.
        /// </summary>
        public string ShortName { get; set; }

        /// <summary>
        /// Gets or sets the format of the mobile network operator.
        /// </summary>
        public string Format { get; set; }

        /// <summary>
        /// Gets or sets the system mode used by the mobile network operator.
        /// </summary>
        public SystemMode SystemMode { get; set; }
    }
}
