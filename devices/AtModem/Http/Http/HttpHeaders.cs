//
// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//

using System.Diagnostics;

namespace System.Net.Http.Headers
{
    /// <summary>
    /// Key/value pairs of headers. The value is either a raw <see cref="string"/> or a <see cref="HttpHeaders.HeaderStoreItemInfo"/>.
    /// </summary>
    internal struct HeaderEntry
    {
        public HeaderDescriptor Key;
        public object Value;

        public HeaderEntry(
            HeaderDescriptor key,
            object value)
        {
            Key = key;
            Value = value;
        }
    }

    /// <summary>
    /// A collection of headers and their values as defined in RFC 2616.
    /// </summary>
    public abstract class HttpHeaders
    {
        internal WebHeaderCollection _headerStore = new WebHeaderCollection(true);

        private readonly HttpHeaderType _allowedHeaderTypes;
        private readonly HttpHeaderType _treatAsCustomHeaderTypes;

        /// <summary>
        /// Initializes a new instance of the HttpHeaders class.
        /// </summary>
        protected HttpHeaders()
          : this(
                HttpHeaderType.All,
                HttpHeaderType.None)
        {
        }

        internal HttpHeaders(
            HttpHeaderType allowedHeaderTypes,
            HttpHeaderType treatAsCustomHeaderTypes)
        {
            // Should be no overlap
            Debug.Assert((allowedHeaderTypes & treatAsCustomHeaderTypes) == 0);

            _allowedHeaderTypes = allowedHeaderTypes & ~HttpHeaderType.NonTrailing;
            _treatAsCustomHeaderTypes = treatAsCustomHeaderTypes & ~HttpHeaderType.NonTrailing;
        }


        /// <summary>
        /// Adds the specified header and its value into the <see cref="HttpHeaders"/> collection.
        /// </summary>
        /// <param name="name">The header to add to the collection.</param>
        /// <param name="value">The content of the header.</param>
        /// <exception cref="ArgumentNullException">The values cannot be <see langword="null"/> or empty.</exception>
        public void Add(
            string name,
            string value)
        {
            _headerStore.Add(name, value);
        }

        internal virtual void AddHeaders(HttpHeaders sourceHeaders)
        {
            foreach (var headerKey in sourceHeaders._headerStore.AllKeys)
            {
                _headerStore.AddInternal(headerKey, sourceHeaders._headerStore[headerKey]);
            }
        }
    }
}
