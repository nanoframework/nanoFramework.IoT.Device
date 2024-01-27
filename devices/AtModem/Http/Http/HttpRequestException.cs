//
// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//

namespace System.Net.Http
{
    /// <summary>
    /// A base class for exceptions thrown by the <see cref="HttpClient"/> and HttpMessageHandler classes.
    /// </summary>
    [Serializable]
    public class HttpRequestException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the HttpRequestException class.
        /// </summary>
        public HttpRequestException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the HttpRequestException class.
        /// </summary>
        /// <param name="message">A message that describes the current exception.</param>
        public HttpRequestException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the HttpRequestException class.
        /// </summary>
        /// <param name="message">A message that describes the current exception.</param>
        /// <param name="inner">The inner exception.</param>
        public HttpRequestException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
