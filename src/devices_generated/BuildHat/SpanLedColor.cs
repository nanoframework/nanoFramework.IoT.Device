namespace System
{
    /// <summary>
    /// Provides a type- and memory-safe representation of a contiguous region of arbitrary array.
    /// </summary>
    [Serializable, CLSCompliant(false)]
    public readonly ref struct SpanLedColor
    {
        private readonly LedColor[] _array;  // internal array
        private readonly int _start;  // offset in the internal array
        private readonly int _length; // accessible length after offset in the internal array

        /// <summary>
        /// Creates a new Span object over the entirety of a specified array.
        /// </summary>
        /// <param name="array">The array from which to create the System.Span object.</param>
        public SpanLedColor(LedColor[] array)
        {
            _array = array;
            _length = array?.Length ?? 0;
            _start = 0;
        }

        /// <summary>
        /// Creates a new Span object that includes a specified number of elements
        /// of an array starting at a specified index.
        /// </summary>
        /// <param name="array">The source array.</param>
        /// <param name="start">The index of the first element to include in the new System.Span</param>
        /// <param name="length">The number of elements to include in the new System.Span</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// array is null, but start or length is non-zero. -or- start is outside the bounds
        /// of the array. -or- start and length exceeds the number of elements in the array.
        /// </exception>
        public SpanLedColor(LedColor[] array, int start, int length)
        {
            if (array != null)
            {
                if (start < 0 ||
                    length < 0 ||
                    start + length > array.Length ||
                    (start == array.Length && start > 0))
                {
                    // Array length too small
                    throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                if ((start != 0) || (length != 0))
                {
                    // Array is null but start and length are not 0
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
        // public ref LedColor this[int index] => ref _array[_start + index]; // <= this is not working and raises exception after few access
        public LedColor this[int index]
        {
            get
            {
                if (index >= _length)
                {
                    throw new ArgumentOutOfRangeException();
                }

                return _array[_start + index];
            }

            set
            {
                if (index >= _length)
                {
                    throw new ArgumentOutOfRangeException();
                }

                _array[_start + index] = value;
            }
        }

        /// <summary>
        /// Returns an empty System.Span object.
        /// </summary>
        public static SpanLedColor Empty => new SpanLedColor();

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
        /// destination is shorter than the source System.Span.
        /// </exception>
        public void CopyTo(SpanLedColor destination)
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
        /// <exception cref="System.ArgumentOutOfRangeException">start is less than zero or greater than System.Span.Length.</exception>
        public SpanLedColor Slice(int start)
        {
            return Slice(start, _length - start);
        }

        /// <summary>
        /// Forms a slice out of the current span starting at a specified index for a specified length.
        /// </summary>
        /// <param name="start">The index at which to begin this slice.</param>
        /// <param name="length">The desired length for the slice.</param>
        /// <returns>A span that consists of length elements from the current span starting at start.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">start or start + length is less than zero or greater than System.Span.Length.</exception>
        public SpanLedColor Slice(int start, int length)
        {
            if ((start < 0) || (length < 0) || (start + length > _length))
            {
                // start or start + length is less than zero or greater than length
                throw new ArgumentOutOfRangeException();
            }

            return new SpanLedColor(_array, _start + start, length);
        }

        /// <summary>
        /// Copies the contents of this span into a new array.
        /// </summary>
        /// <returns> An array containing the data in the current span.</returns>
        public LedColor[] ToArray()
        {
            var array = new LedColor[_length];
            for (int i = 0; i < _length; i++)
            {
                array[i] = _array[_start + i];
            }

            return array;
        }

        public static implicit operator SpanLedColor(LedColor[] array)
        {
            return new(array);
        }
    }
}
