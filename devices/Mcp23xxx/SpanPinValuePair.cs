//
// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//

using System;
using System.Collections;

namespace Iot.Device.Mcp23xxx
{
    /// <summary>
    /// 
    /// </summary>
    public readonly struct SpanPinValuePair : IEnumerable
    {
        private readonly Iot.Device.Mcp23xxx.PinValuePair[] _pairs;
        private readonly int _length;

        /// <summary>
        /// Creates a new System.SpanByte object over the entirety of a specified array.
        /// </summary>
        /// <param name="array">The array from which to create the System.Span object.</param>
        public SpanPinValuePair(Iot.Device.Mcp23xxx.PinValuePair[] array)
        {
            _pairs = null;
            _length = array != null ? array.Length : 0;
            if (_length > 0)
            {
                _pairs = new Iot.Device.Mcp23xxx.PinValuePair[array.Length];
                for (int i = 0; i < _pairs.Length; i++)
                {
                    _pairs[i] = array[i];
                }
            }
        }

        /// <summary>
        /// Gets the element at the specified zero-based index.
        /// </summary>
        /// <param name="index">The zero-based index of the element.</param>
        /// <returns>The element at the specified index.</returns>
        public Iot.Device.Mcp23xxx.PinValuePair this[int index]
        {
            get
            {
                if (index >= _length)
                {
                    throw new ArgumentOutOfRangeException($"Index out of range");
                }

                return _pairs[index];
            }
            set
            {
                if (index >= _length)
                {
                    throw new ArgumentOutOfRangeException($"Index out of range");
                }

                _pairs[index] = value;
            }
        }

        /// <summary>
        /// Returns the length of the current span.
        /// </summary>
        public int Length => _length;

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumerator();
        }
        public Iot.Device.Mcp23xxx.PinValuePairEnum GetEnumerator()
        {
            return new Iot.Device.Mcp23xxx.PinValuePairEnum(_pairs);
        }

        /// <summary>
        /// Implicit conversion of an array to a span of array
        /// </summary>
        /// <param name="array"></param>
        public static implicit operator SpanPinValuePair(Iot.Device.Mcp23xxx.PinValuePair[] array)
        {
            return new SpanPinValuePair(array);
        }       
    }
}