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
    }
}
