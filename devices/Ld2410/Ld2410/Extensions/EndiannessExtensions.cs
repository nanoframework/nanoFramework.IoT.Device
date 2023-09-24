using System;

namespace Ld2410.Extensions
{
    internal static class EndiannessExtensions
    {
        internal static byte[] ToLittleEndianBytes(this ushort value)
        {
            var valueBytes = BitConverter.GetBytes(value);

            if (!BitConverter.IsLittleEndian)
                return valueBytes.Reverse();

            return valueBytes;
        }
    }
}
