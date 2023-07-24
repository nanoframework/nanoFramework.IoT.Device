// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace IoT.Device.AtModem.DTOs
{
    /// <summary>
    /// Supported preferred name storages.
    /// </summary>
    public class SupportedPreferredMessageStorages
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SupportedPreferredMessageStorages"/> class.
        /// </summary>
        /// <param name="storage1">The array of storage names for Storage1.</param>
        /// <param name="storage2">The array of storage names for Storage2.</param>
        /// <param name="storage3">The array of storage names for Storage3.</param>
        public SupportedPreferredMessageStorages(string[] storage1, string[] storage2, string[] storage3)
        {
            Storage1 = storage1 ?? new string[0];
            Storage2 = storage2 ?? new string[0];
            Storage3 = storage3 ?? new string[0];
        }

        /// <summary>
        /// Gets the array of storage names for Storage1.
        /// </summary>
        public string[] Storage1 { get; }

        /// <summary>
        /// Gets the array of storage names for Storage2.
        /// </summary>
        public string[] Storage2 { get; }

        /// <summary>
        /// Gets the array of storage names for Storage3.
        /// </summary>
        public string[] Storage3 { get; }

        /// <summary>
        /// Returns a string representation of the SupportedPreferredMessageStorages object.
        /// </summary>
        /// <returns>A string representing the storage names for each storage.</returns>
        public override string ToString()
        {
            string storageStr = string.Empty;

            foreach (var storage in Storage1)
            {
                storageStr += storage + ",";
            }

            storageStr = storageStr.TrimEnd(',');
            storageStr += "\r\n";

            foreach (var storage in Storage2)
            {
                storageStr += storage + ",";
            }

            storageStr = storageStr.TrimEnd(',');
            storageStr += "\r\n";

            foreach (var storage in Storage3)
            {
                storageStr += storage + ",";
            }

            storageStr = storageStr.TrimEnd(',');

            return storageStr;
        }
    }
}
