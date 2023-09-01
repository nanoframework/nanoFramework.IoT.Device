using System;

namespace Ld2410
{
	internal static class EndiannessExtensions
	{
		public static byte[] ToLittleEndianBytes(this ushort value)
		{
			var valueBytes = BitConverter.GetBytes(value);

			if (!BitConverter.IsLittleEndian)
				return valueBytes.Reverse();

			return valueBytes;
		}
	}
}
