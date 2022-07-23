// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Drawing;

namespace Iot.Device.Apa102
{
    /// <summary>
    /// Provides a type- and memory-safe representation of a contiguous region of arbitrary.
    /// </summary>
    [Serializable, CLSCompliant(false)]
    public struct SpanColor
    {
        private readonly Color[] _array;
        private readonly int _start;
        private readonly int _length;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpanColor" /> struct.
        /// </summary>
        /// <param name="array">The array from which to create the System.Span object.</param>
        public SpanColor(Color[] array)
        {
            _array = array;
            _length = array != null ? array.Length : 0;
            _start = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpanColor" /> struct.
        /// </summary>
        /// <param name="array">The source array.</param>
        /// <param name="start">The index of the first element to include in the new System.Span.</param>
        /// <param name="length">The number of elements to include in the new System.Span.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Array is null, but start or length is non-zero. -or- start is outside the bounds
        /// of the array. -or- start and length exceeds the number of elements in the array.
        /// </exception>
        public SpanColor(Color[] array, int start, int length)
        {
            if (array != null)
            {
                if ((length > array.Length - start) || (start >= array.Length))
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                if ((start != 0) || (length != 0))
                {
                    throw new ArgumentOutOfRangeException();
                }
            }

            _array = array;
            _start = start;
            _length = length;
        }

        /// <summary>
        /// Gets the element at the specified zero-based index.
        /// </summary>
        /// <param name="index">The zero-based index of the element.</param>
        /// <returns>The element at the specified index.</returns>
        public Color this[int index]
        {
            // public ref Color this[int index] => ref _array[_start + index]; // <= this is not working and raises exception after few access
            get
            {
                if (index > _length)
                {
                    throw new ArgumentOutOfRangeException();
                }

                return _array[_start + index];
            }

            set
            {
                if (index > _length)
                {
                    throw new ArgumentOutOfRangeException();
                }

                _array[_start + index] = value;
            }
        }

        /// <summary>
        /// Returns an empty System.Span object.
        /// </summary>
        public static SpanColor Empty => new SpanColor();

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
        /// Converts to SpanColor from array.
        /// </summary>
        /// <param name="array">Color array.</param>
        public static implicit operator SpanColor(Color[] array)
        {
            return new SpanColor(array);
        }

        /// <summary>
        /// Copies the contents of this System.Span into a destination System.Span.
        /// </summary>
        /// <param name="destination"> The destination System.Span object.</param>
        /// <exception cref="System.ArgumentException">
        /// Destination is shorter than the source System.Span.
        /// </exception>
        public void CopyTo(SpanColor destination)
        {
            if (destination.Length < _length)
            {
                throw new ArgumentException();
            }

            for (int i = 0; i < _length; i++)
            {
                destination[i] = _array[_start + i];
            }
        }

        /// <summary>
        /// Forms a slice out of the current span that begins at a specified index.
        /// </summary>
        /// <param name="start">The index at which to begin the slice.</param>
        /// <returns>A span that consists of all elements of the current span from start to the end of the span.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Start is less than zero or greater than System.Span.Length.</exception>
        public SpanColor Slice(int start)
        {
            if ((start > _length) || (start < 0))
            {
                throw new ArgumentOutOfRangeException();
            }

            return new SpanColor(_array, start, _length - start);
        }

        /// <summary>
        /// Forms a slice out of the current span starting at a specified index for a specified length.
        /// </summary>
        /// <param name="start">The index at which to begin this slice.</param>
        /// <param name="length">The desired length for the slice.</param>
        /// <returns>A span that consists of length elements from the current span starting at start.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Start or start + length is less than zero or greater than System.Span.Length.</exception>
        public SpanColor Slice(int start, int length)
        {
            if ((start < 0) || (length < 0) || (start + length > _length))
            {
                throw new ArgumentOutOfRangeException();
            }

            return new SpanColor(_array, _start + start, length);
        }

        /// <summary>
        /// Copies the contents of this span into a new array.
        /// </summary>
        /// <returns> An array containing the data in the current span.</returns>
        public Color[] ToArray()
        {
            Color[] array = new Color[_length];
            for (int i = 0; i < _length; i++)
            {
                array[i] = _array[_start + i];
            }

            return array;
        }
    }
}