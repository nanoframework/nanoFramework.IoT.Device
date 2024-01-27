// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace IoT.Device.AtModem.DTOs
{
    /// <summary>
    /// Represents the preferred message storage.
    /// </summary>
    public class PreferredMessageStorage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PreferredMessageStorage"/> class with the specified storage details.
        /// </summary>
        /// <param name="storage1Name">The name of storage 1.</param>
        /// <param name="storage1Messages">The number of messages in storage 1.</param>
        /// <param name="storage1MessageLocations">The number of available message locations in storage 1.</param>
        public PreferredMessageStorage(string storage1Name, int storage1Messages, int storage1MessageLocations)
        {
            Storage1Name = storage1Name;
            Storage1Messages = storage1Messages;
            Storage1MessageLocations = storage1MessageLocations;
        }

        /// <summary>
        /// Gets the name of storage 1.
        /// </summary>
        public string Storage1Name { get; }

        /// <summary>
        /// Gets the number of messages in storage 1.
        /// </summary>
        public int Storage1Messages { get; }

        /// <summary>
        /// Gets the number of available message locations in storage 1.
        /// </summary>
        public int Storage1MessageLocations { get; }

        /// <summary>
        /// Returns a string representation of the preferred message storage.
        /// </summary>
        /// <returns>A string representing the storage details.</returns>
        public override string ToString()
        {
            return $"Name: {Storage1Name}, Messages: {Storage1Messages}, Locations: {Storage1MessageLocations}";
        }
    }
}
