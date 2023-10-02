//
// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//

namespace System
{
    /// <summary>
    /// Provides a type- and memory-safe representation of a contiguous region of arbitrary byte array
    /// </summary>
    [Serializable, CLSCompliant(false)]
    public readonly ref struct SpanByte
    {
        private readonly byte[] _array;
        private readonly int _start;
        private readonly int _length;

        /// <summary>
        /// Creates a new System.SpanByte object over the entirety of a specified array.
        /// </summary>
        /// <param name="array">The array from which to create the System.Span object.</param>
        public SpanByte(byte[] array)
        {
            _array = array;
            _length = array != null ? array.Length : 0;
            _start = 0;
        }

        /// <summary>
        /// Creates a new System.SpanByte object that includes a specified number of elements
        /// of an array starting at a specified index.
        /// </summary>
        /// <param name="array">The source array.</param>
        /// <param name="start">The index of the first element to include in the new System.Span</param>
        /// <param name="length">The number of elements to include in the new System.Span</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// <para>
        /// array is null, but start or length is non-zero
        /// </para>
        /// <para>-or-</para>
        /// <para>
        /// start is outside the bounds of the array.
        /// </para>
        /// <para>-or-</para>
        /// <para>
        /// <paramref name="start"/> + <paramref name="length"/> exceed the number of elements in the array.
        /// </para>
        /// </exception>
        public SpanByte(byte[] array, int start, int length)
        {
            if (array != null)
            {
                if (start < 0 ||
                    length < 0 ||
                    start + length > array.Length ||
                    start > array.Length)
                {
#pragma warning disable S3928 // Parameter names used into ArgumentException constructors should match an existing one 
                    throw new ArgumentOutOfRangeException();
#pragma warning restore S3928 // Parameter names used into ArgumentException constructors should match an existing one 
                }
                else
                {
                    _array = array;
                    _start = start;
                    _length = length;
                }
            }
            else if ((start != 0) || (length != 0))
            {
#pragma warning disable S3928 // Parameter names used into ArgumentException constructors should match an existing one 
                throw new ArgumentOutOfRangeException();
#pragma warning restore S3928 // Parameter names used into ArgumentException constructors should match an existing one 
            }
            else
            {
                _start = 0;
                _length = 0;
                _array = null;
            }
        }

        /// <summary>
        /// Gets the element at the specified zero-based index.
        /// </summary>
        /// <param name="index">The zero-based index of the element.</param>
        /// <returns>The element at the specified index.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// <paramref name="index"/> is out of range.
        /// </exception>
        public byte this[int index]
        {
            get
            {
                if (index >= _length)
                {
#pragma warning disable S3928 // Parameter names used into ArgumentException constructors should match an existing one 
                    throw new ArgumentOutOfRangeException();
#pragma warning restore S3928 // Parameter names used into ArgumentException constructors should match an existing one 
                }

                return _array[_start + index];
            }

            set
            {
                if (index >= _length)
                {
#pragma warning disable S3928 // Parameter names used into ArgumentException constructors should match an existing one 
                    throw new ArgumentOutOfRangeException();
#pragma warning restore S3928 // Parameter names used into ArgumentException constructors should match an existing one 
                }

                _array[_start + index] = value;
            }
        }

        /// <summary>
        /// Returns an empty System.Span object.
        /// </summary>
        public static SpanByte Empty => new SpanByte();

        /// <summary>
        /// Returns the length of the current span.
        /// </summary>
        public int Length => _length;

        /// <summary>
        /// Returns a value that indicates whether the current System.Span is empty.
        /// true if the current span is empty; otherwise, false.
        /// </summary>
        public bool IsEmpty => _length == 0;

        /// <summary>
        /// Copies the contents of this System.Span into a destination System.Span.
        /// </summary>
        /// <param name="destination"> The destination System.Span object.</param>
        /// <exception cref="System.ArgumentException">
        /// destination is shorter than the source <see cref="SpanByte"/>.
        /// </exception>
        public void CopyTo(SpanByte destination)
        {
            if (destination.Length < _length)
            {
#pragma warning disable S3928 // Parameter names used into ArgumentException constructors should match an existing one 
                throw new ArgumentException();
#pragma warning restore S3928 // Parameter names used into ArgumentException constructors should match an existing one 
            }

            for (int i = 0; i < _length; i++)
            {
                destination[i] = _array[_start + i];
            }
        }

        /// <summary>
        /// Forms a slice out of the current <see cref="SpanByte"/> that begins at a specified index.
        /// </summary>
        /// <param name="start">The index at which to begin the slice.</param>
        /// <returns>A span that consists of all elements of the current span from start to the end of the span.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="start"/> is &lt; zero or &gt; <see cref="Length"/>.</exception>
        public SpanByte Slice(int start)
        {
            return Slice(start, _length - start);
        }

        /// <summary>
        /// Forms a slice out of the current <see cref="SpanByte"/> starting at a specified index for a specified length.
        /// </summary>
        /// <param name="start">The index at which to begin this slice.</param>
        /// <param name="length">The desired length for the slice.</param>
        /// <returns>A <see cref="SpanByte"/> that consists of <paramref name="length"/> number of elements from the current <see cref="SpanByte"/> starting at <paramref name="start"/>.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="start"/> or <paramref name="start"/> + <paramref name="length"/> is &lt; zero or &gt; <see cref="Length"/>.</exception>
        public SpanByte Slice(int start, int length)
        {
            if ((start < 0) || (length < 0) || (start + length > _length))
            {
#pragma warning disable S3928 // Parameter names used into ArgumentException constructors should match an existing one 
                throw new ArgumentOutOfRangeException();
#pragma warning restore S3928 // Parameter names used into ArgumentException constructors should match an existing one 
            }

            return new SpanByte(_array, _start + start, length);
        }

        /// <summary>
        /// Copies the contents of this <see cref="SpanByte"/> into a new array.
        /// </summary>
        /// <returns> An array containing the data in the current span.</returns>
        public byte[] ToArray()
        {
            byte[] array = new byte[_length];
            for (int i = 0; i < _length; i++)
            {
                array[i] = _array[_start + i];
            }

            return array;
        }

        /// <summary>
        /// Implicit conversion of an array to a <see cref="SpanByte"/>.
        /// </summary>
        /// <param name="array"></param>
        public static implicit operator SpanByte(byte[] array)
        {
            return new SpanByte(array);
        }
    }
}
