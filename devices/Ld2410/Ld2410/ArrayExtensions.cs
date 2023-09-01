namespace Ld2410
{
	internal static class ArrayExtensions
	{
		public static byte[] Reverse(this byte[] array)
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
