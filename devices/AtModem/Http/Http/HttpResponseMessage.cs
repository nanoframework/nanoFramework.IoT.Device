//
// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//

using System.Net.Http.Headers;

namespace System.Net.Http
{
    /// <summary>
    /// Represents a HTTP response message including the status code and data.
    /// </summary>
    public class HttpResponseMessage : IDisposable
    {
        private static Version DefaultResponseVersion => HttpVersion.Version11;

        private HttpStatusCode _statusCode;
        private HttpResponseHeaders _headers;
        private HttpRequestMessage _requestMessage;
        private Version _version;
        private HttpContent _content;
        private bool _disposed;

        /// <summary>
        /// Gets or sets the content of a HTTP response message.
        /// </summary>
        /// <value>The content of the HTTP response message.</value>
        public HttpContent Content
        {
            get { return _content; }

            set
            {
                CheckDisposed();

                _content = value;
            }
        }

        /// <summary>
        /// Gets the collection of HTTP response headers.
        /// </summary>
        /// <value>The collection of HTTP response headers.</value>
        public HttpResponseHeaders Headers
        {
            get
            {
                return _headers ?? (_headers = new HttpResponseHeaders());
            }
        }

        /// <summary>
        /// Gets a value that indicates if the HTTP response was successful.
        /// </summary>
        /// <value><see langword="true"/> if StatusCode was in the range 200-299; otherwise, <see langword="false"/>.</value>
        public bool IsSuccessStatusCode
        {
            get { return ((int)_statusCode >= 200) && ((int)_statusCode <= 299); }
        }

        /// <summary>
        /// Gets or sets the reason phrase which typically is sent by servers together with the status code.
        /// </summary>
        /// <value>The reason phrase sent by the server.</value>
        public string ReasonPhrase { get; set; }

        /// <summary>
        /// Gets or sets the request message which led to this response message.
        /// </summary>
        /// <value>The request message which led to this response message.</value>
        /// <remarks>
        /// This property is set to the request message which led to this response message. In the case of a request sent using HttpClient, this property will point to the actual request message leading to the final response. Note that this may not be the same message the user provided when sending the request. This is typically the case if the request needs to be resent due to redirects or authentication. This property can be used to determine what URL actually created the response (useful in case of redirects).
        /// </remarks>
        public HttpRequestMessage RequestMessage
        {
            get { return _requestMessage; }

            set
            {
                CheckDisposed();

                _requestMessage = value;
            }
        }

        /// <summary>
        /// Gets or sets the status code of the HTTP response.
        /// </summary>
        /// <value>The status code of the HTTP response.</value>
        public HttpStatusCode StatusCode => _statusCode;

        /// <summary>
        /// Gets or sets the HTTP message version.
        /// </summary>
        /// <value>The HTTP message version. The default is 1.1.</value>
        public Version Version
        {
            get { return _version; }

            set
            {
                CheckDisposed();

                _version = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the HttpResponseMessage class.
        /// </summary>
        public HttpResponseMessage()
            : this(HttpStatusCode.OK)
        {
        }

        /// <summary>
        /// Initializes a new instance of the HttpResponseMessage class.
        /// </summary>
        /// <param name="statusCode">The status code of the HTTP response.</param>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="statusCode"/> has an invalid value.</exception>
        public HttpResponseMessage(HttpStatusCode statusCode)
        {
            if (((int)statusCode < 0) || ((int)statusCode > 999))
            {
                throw new ArgumentOutOfRangeException();
            }

            _statusCode = statusCode;
            _version = DefaultResponseVersion;
        }

        /// <summary>
        /// Throws an exception if the <see cref="IsSuccessStatusCode"/> property for the HTTP response is <see langword="false"/>.
        /// </summary>
        /// <returns>The HTTP response message if the call is successful.</returns>
        /// <exception cref="HttpRequestException"></exception>
        public HttpResponseMessage EnsureSuccessStatusCode()
        {
            if (!IsSuccessStatusCode)
            {
                throw new HttpRequestException();
            }

            return this;
        }

        #region IDisposable Members

        /// <inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
            // The reason for this type to implement IDisposable is that it contains instances of types that implement
            // IDisposable (content).
            if (disposing && !_disposed)
            {
                _disposed = true;
                if (_content != null)
                {
                    _content.Dispose();
                }
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        private void CheckDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(string.Empty);
            }
        }
    }
}
