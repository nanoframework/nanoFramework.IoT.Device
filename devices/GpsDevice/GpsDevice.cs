// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Common.GpsDevice
{
    /// <summary>
    /// Base class for all GPS devices.
    /// </summary>
    public abstract class GpsDevice
    {
        private Fix _fix = Fix.NoFix;
        private Mode _mode = Mode.Unknown;
        private GeoPosition _location;

        /// <summary>
        /// Delegate type to handle the event when the GPS module fix status changes.
        /// </summary>
        /// <param name="fix">The new fix status.</param>
        public delegate void FixChangedHandler(Fix fix);

        /// <summary>
        /// Delegate type to handle the event when the GPS module mode changes.
        /// </summary>
        /// <param name="mode">The new GPS module mode.</param>
        public delegate void ModeChangedHandler(Mode mode);
        
        /// <summary>
        /// Delegate type to handle the event when the GPS module location changes.
        /// </summary>
        /// <param name="position">The new position.</param>
        public delegate void LocationChangeHandler(GeoPosition position);

        /// <summary>
        /// Gets or sets the fix status of the GPS module.
        /// </summary>
        public Fix Fix
        {
            get => _fix;
            protected set
            {
                if (_fix == value)
                {
                    return;
                }
                
                _fix = value;
                FixChanged?.Invoke(value);
            }
        }

        /// <summary>
        /// Gets or sets the mode of the GPS module.
        /// </summary>
        /// <value>
        /// The mode of the GPS module.
        /// </value>
        public Mode Mode
        {
            get => _mode;
            protected set
            {
                if (value == _mode)
                {
                    return;
                }

                _mode = value;
                ModeChanged?.Invoke(_mode);
            }
        }

        /// <summary>
        /// Gets or sets the last known location.
        /// </summary>
        public GeoPosition Location
        {
            get => _location;
            protected set
            {
                _location = value;
                LocationChanged?.Invoke(_location);
            }
        }

        /// <summary>
        /// Represents the event handler for when the fix status of the GPS module changes.
        /// </summary>
        public event FixChangedHandler FixChanged;

        /// <summary>
        /// Event that occurs when the location changes.
        /// </summary>
        public event LocationChangeHandler LocationChanged;

        /// <summary>
        /// Represents the event that is raised when the mode of the GPS module is changed.
        /// </summary>
        public event ModeChangedHandler ModeChanged;
    }
}