using System;

namespace nF.Modbus.Util
{
	static class Checksum
	{
        /// <summary>
        /// 计算16位数组的校验和
        /// </summary>
        /// <param name="array">数组</param>
        /// <returns>CRC16 校验和. [0] = low byte, [1] = high byte.</returns>
        public static byte[] CRC16(this byte[] array)
			=> array.CRC16(0, array.Length);

        /// <summary>
        /// 计算16位数组的校验和
        /// </summary>
        /// <param name="array">数组</param>
        /// <param name="start">起始位</param>
        /// <param name="length">长度</param>
        /// <returns>CRC16 校验和. [0] = low byte, [1] = high byte.</returns>
        public static byte[] CRC16(this byte[] array, int start, int length)
		{
			if (array == null || array.Length == 0)
				throw new ArgumentNullException(nameof(array));

			if (start < 0 || start >= array.Length)
				throw new ArgumentOutOfRangeException(nameof(start));

			if (length <= 0 || (start + length) > array.Length)
				throw new ArgumentOutOfRangeException(nameof(length));

			ushort crc16 = 0xFFFF;
			byte lsb;

			for (int i = start; i < (start + length); i++)
			{
				crc16 = (ushort)(crc16 ^ array[i]);
				for (int j = 0; j < 8; j++)
				{
					lsb = (byte)(crc16 & 1);
					crc16 = (ushort)(crc16 >> 1);
					if (lsb == 1)
					{
						crc16 = (ushort)(crc16 ^ 0xA001);
					}
				}
			}

			byte[] b = new byte[2];
			b[0] = (byte)crc16;
			b[1] = (byte)(crc16 >> 8);

			return b;
		}
	}
}
