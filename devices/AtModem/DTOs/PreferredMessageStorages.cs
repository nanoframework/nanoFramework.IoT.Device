// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.AtModem.DTOs
{
    /// <summary>
    /// Represents the preferred message storages.
    /// </summary>
    public class PreferredMessageStorages
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PreferredMessageStorages"/> class with the specified storage details.
        /// </summary>
        /// <param name="storage1Name">The preferred message storage 1.</param>
        /// <param name="storage2Name">The preferred message storage 2.</param>
        /// <param name="storage3Name">The preferred message storage 3.</param>
        public PreferredMessageStorages(
        PreferredMessageStorage storage1Name,
        PreferredMessageStorage storage2Name,
        PreferredMessageStorage storage3Name)
        {
            Storage1Name = storage1Name;
            Storage2Name = storage2Name;
            Storage3Name = storage3Name;
        }

        /// <summary>
        /// Gets the preferred message storage 1.
        /// </summary>
        public PreferredMessageStorage Storage1Name { get; }

        /// <summary>
        /// Gets the preferred message storage 2.
        /// </summary>
        public PreferredMessageStorage Storage2Name { get; }

        /// <summary>
        /// Gets the preferred message storage 3.
        /// </summary>
        public PreferredMessageStorage Storage3Name { get; }

        /// <summary>
        /// Returns a string representation of the preferred message storages.
        /// </summary>
        /// <returns>A string representing the storage details.</returns>
        public override string ToString()
        {
            return $"Storage1: {Storage1Name}\r\n" +
                   $"Storage2: {Storage2Name}\r\n" +
                   $"Storage3: {Storage3Name}";
        }
    }
}
