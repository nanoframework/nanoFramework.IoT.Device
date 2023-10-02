//
// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//

using System.Net.Http.Headers;
using System.Text;

namespace System.Net.Http
{
    /// <summary>
    /// Provides HTTP content based on a string.
    /// </summary>
    public class StringContent : ByteArrayContent
    {
        private const string DefaultMediaType = "text/plain";
        private const string EncodingUTF8WebName = "utf-8";

        /// <summary>
        /// Creates a new instance of the <see cref="StringContent"/> class.
        /// </summary>
        /// <param name="content">The content used to initialize the <see cref="StringContent"/>.</param>
        /// <remarks>
        /// The media type for the <see cref="StringContent"/> created defaults to text/plain.
        /// </remarks>
        public StringContent(string content)
            : this(
                  content,
                  null,
                  null)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="StringContent"/> class.
        /// </summary>
        /// <param name="content">The content used to initialize the <see cref="StringContent"/>.</param>
        /// <param name="encoding">The encoding to use for the content.</param>
        /// <remarks>
        /// The media type for the <see cref="StringContent"/> created defaults to text/plain.
        /// </remarks>
        public StringContent(
            string content,
            Encoding encoding)
            : this(
                  content,
                  encoding,
                  null)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="StringContent"/> class.
        /// </summary>
        /// <param name="content">The content used to initialize the <see cref="StringContent"/>.</param>
        /// <param name="encoding">The encoding to use for the content.</param>
        /// <param name="mediaType">The media type to use for the content.</param>
        public StringContent(
            string content,
            Encoding encoding,
            string mediaType)
            : base(GetContentByteArray(
                content,
                encoding))
        {
            Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(mediaType ?? DefaultMediaType)
            {
                CharSet = EncodingUTF8WebName
            };
        }

        // A StringContent is essentially a ByteArrayContent. We serialize the string into a byte-array in the
        // constructor using encoding information provided by the caller (if any). When this content is sent, the
        // Content-Length can be retrieved easily (length of the array).
        private static byte[] GetContentByteArray(
            string content,
            Encoding encoding)
        {
            if (content is null)
            {
                throw new ArgumentNullException();
            }

            // In this case we treat 'null' strings different from string.Empty in order to be consistent with our
            // other *Content constructors: 'null' throws, empty values are allowed.
            // we only support UTF8 encoding
            encoding ??= Encoding.UTF8;

            return encoding.GetBytes(content);
        }
    }
}
