//
// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//

using System.Diagnostics;

namespace System.Net.Http.Headers
{
    /// <summary>
    /// Represents the collection of Request Headers as defined in RFC 2616.
    /// </summary>
    public sealed class HttpRequestHeaders : HttpHeaders
    {
        #region General Headers

        /// <summary>
        /// Gets the value of the Connection header for an HTTP request.
        /// </summary>
        /// <value>The value of the Connection header for an HTTP request.</value>
        public string Connection
        {
            get
            {
                var connectionHeader = _headerStore.GetValues(HttpKnownHeaderNames.Connection);
                if (connectionHeader is not null)
                {
                    return connectionHeader[0];
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates if theConnection header for an HTTP request contains Close.
        /// </summary>
        /// <value><see langword="true"/> if the Connection header contains Close, otherwise <see langword="false"/>.</value>
        public bool ConnectionClose
        {
            get
            {
                return Connection.ToLower().Contains("close");
            }

            set
            {
                var connectionHeader = _headerStore.GetValues(HttpKnownHeaderNames.Connection);
                if (connectionHeader is not null)
                {
                    connectionHeader[0] = value ? "close" : "keep-alive";
                }
                else
                {
                    _headerStore.AddInternal(HttpKnownHeaderNames.Connection, value ? "close" : "keep-alive");
                }
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates if the Transfer-Encoding header for an HTTP request contains chunked.
        /// </summary>
        /// <value><see langword="true"/> if the Transfer-Encoding header contains chunked, otherwise <see langword="false"/>.</value>
        public bool TransferEncodingChunked
        {
            get
            {
                var header = _headerStore[HttpKnownHeaderNames.TransferEncoding];

                if (header is not null)
                {
                    return header.Contains("chunked");
                }

                return false;
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        internal HttpRequestHeaders()
            : base()
        {
        }

        internal override void AddHeaders(HttpHeaders headers)
        {
            base.AddHeaders(headers);
        }
    }
}
