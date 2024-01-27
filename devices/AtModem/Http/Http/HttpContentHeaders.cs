//
// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//

using System.Net.Http.Headers;

namespace System.Net.Http.Headers
{
    /// <summary>
    /// Represents the collection of Content Headers as defined in RFC 2616.
    /// </summary>
    public sealed class HttpContentHeaders : HttpHeaders
    {
        private readonly HttpContent _content;

        /// <summary>
        /// Gets or sets the value of the Content-Length content header on an HTTP response.
        /// </summary>
        /// <value>The value of the Content-Length content header on an HTTP response.</value>
        /// <remarks>
        /// In .NET nanoFramework this property is read-only.
        /// </remarks>
        public long ContentLength
        {
            get
            {
                if (_content.Headers is not null
                    && _content.Headers._headerStore is not null)
                {
                    var contentLengthValue = _content.Headers._headerStore.GetValues(HttpKnownHeaderNames.ContentLength);

                    if (contentLengthValue is not null
                        && contentLengthValue.Length > 0)
                    {
                        return Convert.ToInt64(contentLengthValue[0]);
                    }
                }

                return -1;
            }

            set
            {
                throw new PlatformNotSupportedException();
            }
        }

        /// <summary>
        /// Gets or sets the value of the Content-Type content header on an HTTP response.
        /// </summary>
        /// <value>The value of the Content-Type content header on an HTTP response.</value>
        public MediaTypeHeaderValue ContentType
        {
            get
            {
                return MediaTypeHeaderValue.Parse(_headerStore[HttpKnownHeaderNames.ContentType]);
            }

            set
            {
                // build header value, OK to add ; even if CharSet is empty
                _headerStore.Add(HttpKnownHeaderNames.ContentType, value.ToString());
            }
        }

        internal HttpContentHeaders(HttpContent parent)
          : base(HttpHeaderType.Content | HttpHeaderType.Custom, HttpHeaderType.None)
        {
            _content = parent;
        }
    }
}
