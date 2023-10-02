using System;

namespace Ld2410.Extensions
{
    internal static class ArrayExtensions
    {
        internal static byte[] Reverse(this byte[] array)
        {
            var current = 0;
            var last = array.Length - 1;

            while (current < last)
            {
                var currentVal = array[current];

                array[current] = array[last];
                array[last] = currentVal;

                current++;
                last--;
            }

            return array;
        }

        internal static void CopyToArrayWithIndexAndAdvance(
            this byte[] sourceArray,
            byte[] destinationArray,
            ref int destinationArrayIndex
            )
        {
            Array.Copy(
                sourceArray,
                sourceIndex: 0,
                destinationArray,
                destinationArrayIndex,
                length: sourceArray.Length
                );

            destinationArrayIndex += sourceArray.Length;
        }
    }
}
