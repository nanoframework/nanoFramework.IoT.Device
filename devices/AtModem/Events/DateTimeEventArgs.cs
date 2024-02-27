// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;

namespace Iot.Device.AtModem.Events
{
    /// <summary>
    /// Provides event arguments containing a DateTime value.
    /// </summary>
    public class DateTimeEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the DateTimeEventArgs class with the specified DateTime value.
        /// </summary>
        /// <param name="dateTime">The DateTime value to be encapsulated.</param>
        public DateTimeEventArgs(DateTime dateTime)
        {
            DateTime = dateTime;
        }

        /// <summary>
        /// Gets the DateTime value associated with the event.
        /// </summary>
        public DateTime DateTime { get; }
    }   
}
