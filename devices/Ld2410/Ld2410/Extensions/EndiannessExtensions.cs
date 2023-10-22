using System;

namespace Ld2410.Extensions
{
    internal static class EndiannessExtensions
    {
        internal static byte[] ToLittleEndianBytes(this ushort value)
        {
            return EnsureLittleEndian(BitConverter.GetBytes(value));
        }

        internal static byte[] ToLittleEndianBytes(this int value)
        {
            return EnsureLittleEndian(BitConverter.GetBytes(value));
        }

        internal static byte[] ToLittleEndianBytes(this uint value)
        {
            return EnsureLittleEndian(BitConverter.GetBytes(value));
        }

        internal static byte[] EnsureLittleEndian(this byte[] valueBytes)
        {
            if (!BitConverter.IsLittleEndian)
                return valueBytes.Reverse();

            return valueBytes;
        }
    }
}
