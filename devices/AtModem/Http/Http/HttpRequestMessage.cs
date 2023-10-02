//
// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//

using System.Net.Http.Headers;

namespace System.Net.Http
{
    /// <summary>
    /// Represents a HTTP request message.
    /// </summary>
    public class HttpRequestMessage : IDisposable
    {
        internal static Version DefaultRequestVersion => HttpVersion.Version11;

        private const int MessageNotYetSent = 0;
        private const int MessageAlreadySent = 1;
        private const int MessageIsRedirect = 2;

        // Track whether the message has been sent.
        // The message shouldn't be sent again if this field is equal to MessageAlreadySent.
        private int _sendStatus = MessageNotYetSent;

        private HttpMethod _method;
        private Uri _requestUri;
        private HttpRequestHeaders _headers;
        private Version _version;
        private bool _disposed;
        private bool _isUsed;

        /// <summary>
        /// Gets or sets the HTTP message version.
        /// </summary>
        /// <value>The HTTP message version. The default value is 1.1.</value>
        public Version Version
        {
            get { return _version; }

            set
            {
                if (value is null)
                {
                    throw new ArgumentNullException();
                }

                CheckDisposed();

                _version = value;
            }
        }

        /// <summary>
        /// Gets or sets the HTTP method used by the HTTP request message.
        /// </summary>
        /// <value>The HTTP method used by the request message. The default is the GET method.</value>
        public HttpMethod Method
        {
            get { return _method; }

            set
            {
                if (value is null)
                {
                    throw new ArgumentNullException();
                }

                CheckDisposed();

                _method = value;
            }
        }

        /// <summary>
        /// Gets or sets the Uri used for the HTTP request.
        /// </summary>
        /// <value>The <see cref="Uri"/> used for the HTTP request.</value>
        /// <remarks>
        /// If the request Uri is a relative Uri, it will be combined with the <see cref="HttpClient.BaseAddress"/>.
        /// </remarks>
        public Uri RequestUri
        {
            get { return _requestUri; }

            set
            {
                CheckDisposed();

                _requestUri = value;
            }
        }

        /// <summary>
        /// Gets the collection of HTTP request headers.
        /// </summary>
        /// <value>The collection of HTTP request headers.</value>
        public HttpRequestHeaders Headers => _headers ??= new HttpRequestHeaders();

        /// <summary>
        /// Gets or sets the contents of the HTTP message.
        /// </summary>
        public HttpContent Content { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRequestMessage"/> class.
        /// </summary>
        public HttpRequestMessage()
          : this(HttpMethod.Get, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRequestMessage"/> class with an HTTP method and a request Uri.
        /// </summary>
        /// <param name="method">The HTTP method.</param>
        /// <param name="requestUri">A string that represents the request <see cref="Uri"/>.</param>
        public HttpRequestMessage(
            HttpMethod method,
            string requestUri)
        {
            // It's OK to have a 'null' request Uri. If HttpClient is used, the 'BaseAddress' will be added.
            // If there is no 'BaseAddress', sending this request message will throw.
            // Note that we also allow the string to be empty: null and empty are considered equivalent.
            _method = method;
            _requestUri = string.IsNullOrEmpty(requestUri) ? null : new Uri(requestUri, UriKind.RelativeOrAbsolute);
            _version = DefaultRequestVersion;
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

        internal bool SetIsUsed()
        {
            if (_isUsed)
            {
                return true;
            }

            _isUsed = true;
            return false;
        }
    }
}