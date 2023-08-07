//
// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//

namespace System.Net.Http.Headers
{
    // This struct represents a particular named header --
    // if the header is one of our known headers, then it contains a reference to the KnownHeader object;
    // otherwise, for custom headers, it just contains a string for the header name.
    // Use HeaderDescriptor.TryGet to resolve an arbitrary header name to a HeaderDescriptor.
    internal readonly struct HeaderDescriptor
    {
        /// <summary>
        /// Either a <see cref="KnownHeader"/> or <see cref="string"/>.
        /// </summary>
        private readonly object _descriptor;

        // This should not be used directly; use static TryGet below
        internal HeaderDescriptor(
            string headerName,
            bool customHeader = false)
        {
            _descriptor = headerName;
        }

        public string Name => _descriptor is KnownHeader header ? header.Name : (_descriptor as string)!;

        public HttpHeaderType HeaderType => _descriptor is KnownHeader knownHeader ? knownHeader.HeaderType : HttpHeaderType.Custom;

        public KnownHeader KnownHeader => _descriptor as KnownHeader;

    }
}
