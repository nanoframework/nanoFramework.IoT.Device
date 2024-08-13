// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Iot.Device.AtModem.Events;
using Iot.Device.Common.GnssDevice;

namespace Iot.Device.AtModem.Gnss
{
    /// <summary>
    /// Represents the base class for a Global Navigation Satellite System device.
    /// </summary>
    public abstract class GnssBase
    {
        /// <summary>
        /// Represents the handler for a GNSS position event.
        /// </summary>
        /// <param name="sender">THe sender modem.</param>
        /// <param name="e">The GNSS positioning information.</param>
        public delegate void GnssPositionHandler(object sender, GnssPositionArgs e);

        /// <summary>
        /// Occurs when the GNSS position is updated.
        /// </summary>
        public event GnssPositionHandler GnssPositionUpdate;

        /// <summary>
        /// Event invoking method to be used in derived classes.
        /// </summary>
        /// <param name="e">The GNSS positioning information.</param>
        protected virtual void OnGnssPositionUpdate(GnssPositionArgs e)
        {
            GnssPositionUpdate?.Invoke(this, e);
        }

        /// <summary>
        /// Starts the GNSS device.
        /// </summary>
        /// <returns>A value indicating whether the start was successful.</returns>
        public abstract bool Start();

        /// <summary>
        /// Stops the GNSS device.
        /// </summary>
        /// <returns>A value indicating whether the stop was successful.</returns>
        public abstract bool Stop();

        /// <summary>
        /// Gets a value indicating whether the GNSS device is running.
        /// </summary>
        public abstract bool IsRunning
        {
            get;
        }

        /// <summary>
        /// Gets or sets the mode of the GNSS device.
        /// </summary>
        public virtual GnssMode GnssMode { get; set; }

        /// <summary>
        /// Gets or sets the start mode for the GNSS device.
        /// </summary>
        public virtual GnssStartMode StartMode { get; set; }

        /// <summary>
        /// Gets the position of the GNSS device.
        /// </summary>
        /// <returns>A GNSS position or null if none.</returns>
        public abstract Location GetLocation();

        /// <summary>
        /// Gets the product details.
        /// </summary>
        /// <returns>A string representing the product details.</returns>
        public abstract string GetProductDetails();

        /// <summary>
        /// Gets or sets the interval between wich the GNSS position is updated.
        /// An event is raised when a new valid position is received.
        /// </summary>
        public virtual TimeSpan AutomaticUpdate { get; set; }
    }
}