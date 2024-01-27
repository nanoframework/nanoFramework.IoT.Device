//
// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//

using System.IO;

namespace System.Net.Http
{
    /// <summary>
    /// Provides HTTP content based on a byte array.
    /// </summary>
    public class ByteArrayContent : HttpContent
    {
        private readonly byte[] _content;
        private readonly int _offset;
        private readonly int _count;

        /// <summary>
        /// Initializes a new instance of the <see cref="ByteArrayContent"/> class.
        /// </summary>
        /// <param name="content">The content used to initialize the <see cref="ByteArrayContent"/>.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="content"/> parameter is <see langword="null"/>.</exception>
        public ByteArrayContent(byte[] content)
        {
            if (content is null)
            {
                throw new ArgumentNullException();
            }

            _content = content;
            _count = content.Length;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ByteArrayContent"/> class.
        /// </summary>
        /// <param name="content">The content used to initialize the <see cref="ByteArrayContent"/>.</param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <exception cref="ArgumentNullException">The <paramref name="content"/> parameter is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>
        /// The <paramref name="offset"/> parameter is less than zero.
        /// </para>
        /// <para>
        /// -or-
        /// </para>
        /// <para>
        /// The <paramref name="offset"/> parameter is greater than the length of content specified by the <paramref name="content"/> parameter.
        /// </para>
        /// <para>
        /// -or-
        /// </para>
        /// <para>
        /// The <paramref name="count"/> parameter is less than zero.
        /// </para>
        /// <para>
        /// -or-
        /// </para>
        /// <para>
        /// The <paramref name="count"/> parameter is greater than the length of content specified by the <paramref name="content"/> parameter - minus the <paramref name="offset"/> parameter.
        /// </para>
        /// </exception>
        /// <remarks>
        /// Only the range specified by the <paramref name="offset"/> parameter and the <paramref name="count"/> parameter is used as content. Syntax
        /// </remarks>
        public ByteArrayContent(
            byte[] content,
            int offset,
            int count)
        {
            if (content is null)
            {
                throw new ArgumentNullException();
            }

            if ((offset < 0) || (offset > content.Length))
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            if ((count < 0) || (count > (content.Length - offset)))
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            _content = content;
            _offset = offset;
            _count = count;
        }

        /// <inheritdoc/>
        protected override void SerializeToStream(Stream stream)
        {
            stream.Write(_content, _offset, _count);
        }

        /// <inheritdoc/>
        protected internal override bool TryComputeLength(out long length)
        {
            length = _count;
            return true;
        }
    }
}
