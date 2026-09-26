// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.M24Sr
{
    /// <summary>
    /// M24SR NFC Forum Type 4 capability container values.
    /// </summary>
    public class M24SrCapabilityContainer
    {
        /// <summary>
        /// Minimum supported capability container length.
        /// </summary>
        public const int MinimumLength = 15;

        /// <summary>
        /// Initializes a new instance of the <see cref="M24SrCapabilityContainer"/> class.
        /// </summary>
        /// <param name="data">Raw capability container bytes.</param>
        /// <exception cref="ArgumentException"><paramref name="data"/> is <see langword="null"/> or shorter than <see cref="MinimumLength"/>.</exception>
        /// <exception cref="InvalidOperationException">The capability container is invalid, unsupported, or contains invalid transfer limits.</exception>
        internal M24SrCapabilityContainer(byte[] data)
        {
            if ((data == null) || (data.Length < MinimumLength))
            {
                throw new ArgumentException();
            }

            int containerLength = (data[0] << 8) | data[1];
            if ((containerLength < MinimumLength) || (data[7] != 0x04) || (data[8] != 0x06))
            {
                throw new InvalidOperationException();
            }

            MappingVersion = data[2];
            MaximumReadLength = (data[3] << 8) | data[4];
            MaximumWriteLength = (data[5] << 8) | data[6];
            NdefFileId = (ushort)((data[9] << 8) | data[10]);
            MaximumNdefFileSize = (data[11] << 8) | data[12];
            ReadAccess = data[13];
            WriteAccess = data[14];

            if ((MaximumReadLength <= 0) || (MaximumWriteLength <= 0) || (MaximumNdefFileSize < 2))
            {
                throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Gets the NFC Forum Type 4 mapping version.
        /// </summary>
        public byte MappingVersion { get; }

        /// <summary>
        /// Gets the maximum response data length supported by the tag.
        /// </summary>
        public int MaximumReadLength { get; }

        /// <summary>
        /// Gets the maximum command data length supported by the tag.
        /// </summary>
        public int MaximumWriteLength { get; }

        /// <summary>
        /// Gets the NDEF file identifier.
        /// </summary>
        public ushort NdefFileId { get; }

        /// <summary>
        /// Gets the maximum NDEF file size in bytes, including the two-byte NLEN field.
        /// </summary>
        public int MaximumNdefFileSize { get; }

        /// <summary>
        /// Gets the NDEF read-access value.
        /// </summary>
        public byte ReadAccess { get; }

        /// <summary>
        /// Gets the NDEF write-access value.
        /// </summary>
        public byte WriteAccess { get; }

        /// <summary>
        /// Gets a value indicating whether writing is allowed without security verification.
        /// </summary>
        public bool IsWriteAllowed => WriteAccess == 0x00;
    }
}