// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace IoT.Device.AtModem.CodingSchemes
{
    internal class HashHelper
    {
        /// <summary>
        /// This helper to facilitate the creation of unique file names.
        /// </summary>
        /// <param name="data">The data to be processed.</param>
        /// <returns>A simple hash.</returns>
        internal static int ComputeHash(params byte[] data)
        {
            unchecked
            {
                const int P = 16777619;
                int hash = (int)2166136261;

                for (int i = 0; i < data.Length; i++)
                {
                    hash = (hash ^ data[i]) * P;
                }

                return hash;
            }
        }
    }
}
