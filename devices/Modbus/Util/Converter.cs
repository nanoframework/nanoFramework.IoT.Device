using System;

namespace Iot.Device.Modbus.Util
{
    public static class Int16Converter
    {
        // Byte
        public static ushort[] From(byte[] values, int count = -1)
        {
            if (values == null || values.Length == 0 || count == 0)
                return new ushort[0];

            if (count > values.Length)
                throw new ArgumentOutOfRangeException(nameof(count));

            if (count < 0)
                count = values.Length;

            ushort[] shorts = new ushort[count / 2];
            for (int i = 0; i < shorts.Length; i++)
            {
                var tmps = new byte[] { values[i * 2], values[i * 2 + 1] };
                tmps.JudgReverse();

                shorts[i] = BitConverter.ToUInt16(tmps, 0);
            }

            return shorts;
        }

        public static byte[] ToBytes(params ushort[] values)
        {
            if (values == null || values.Length == 0)
                return new byte[0];

            byte[] bytes = new byte[values.Length * 2];
            for (int i = 0; i < values.Length; i++)
            {
                byte[] blob = BitConverter.GetBytes(values[i]);
                blob.JudgReverse();

                var idx = (i * 2);
                bytes[idx] = blob[0];
                bytes[idx + 1] = blob[1];
            }

            return bytes;
        }

        // Int
        public static ushort From(int value)
        {
            if (value > short.MaxValue || value < short.MinValue)
                throw new ArgumentException(nameof(value));

            if (value >= 0)
                return (ushort)value;
            else
                return (ushort)ComplementCode(value);
        }

        public static int ToInt32(ushort value)
        {
            return value - ((value > short.MaxValue) ? (ushort.MaxValue + 1) : 0);
        }

        // Time
        public static ushort From(TimeSpan time)
        {
            byte bit1 = (byte)time.Hours;
            byte bit2 = (byte)time.Minutes;

            return From(new byte[] { bit1, bit2 })[0];
        }

        public static TimeSpan ToTime(ushort value)
        {
            var bytes = ToBytes(value);
            var h = (bytes[0] <= 23 ? bytes[0] : 23);
            var m = (bytes[1] <= 59 ? bytes[1] : 59);

            return new TimeSpan(h, m, 0);
        }

        // DateTime
        public static ushort[] From(DateTime dt)
        {
            var val = dt.Subtract(DateTime.UnixEpoch);
            var bytes = BitConverter.GetBytes(val.Ticks);

            return From(bytes);
        }

        public static DateTime ToDateTime(ushort[] values)
        {
            if (values == null || values.Length < 4)
                return DateTime.MinValue;

            var bytes = ToBytes(values);
            var val = BitConverter.ToInt64(bytes, 0);

            return DateTime.UnixEpoch.AddTicks(val);
        }

        // String
        public static ushort[] From(string s, int start = 0, int length = -1)
        {
            if (!String.IsNullOrEmpty(s))
            {
                if (length == -1)
                    length = s.Length - start;

                byte[] bytes = new byte[length * 2];
                var count = System.Text.Encoding.UTF8.GetBytes(s, start, length, bytes, 0);
                if (count > 0)
                    return From(bytes, count);
            }
            return new ushort[0];
        }

        public static string ToString(ushort[] values)
        {
            if (values == null || values.Length < 0)
                return String.Empty;

            var bytes = ToBytes(values);
            return System.Text.Encoding.UTF8.GetString(bytes, 0, bytes.Length);
        }

        // 计算补码
        static int ComplementCode(int original)
        {
            int a = short.MaxValue;
            int b = short.MinValue;
            int c = a - b;
            int d;
            if (original > 0)
                d = -(c - original + 1);
            else
                d = c + original + 1;

            return d;
        }
    }

    public static class BoolConverter
    {
        public static bool From(uint value)
        {
            return (value != 0);
        }

        public static bool From(int value)
        {
            return (value != 0);
        }

        public static bool From(ushort value)
        {
            return (value != 0);
        }

        public static bool From(short value)
        {
            return (value != 0);
        }

        public static ushort ToUInt16(bool value)
        {
            if (value)
                return (ushort)1;
            else
                return ushort.MinValue;
        }

        public static short ToInt16(bool value)
        {
            if (value)
                return (short)1;
            else
                return short.MinValue;
        }
    }
}

namespace System
{
    static class ByteExtension
    {
        static readonly bool IsLittleEndian = false;

        // 反转
        public static void JudgReverse(this byte[] bytes)
        {
            if (IsLittleEndian != BitConverter.IsLittleEndian &&
                bytes != null && bytes.Length != 0)
            {
                int i = 0;
                int j = bytes.Length - 1;

                while (i < j)
                {
                    byte temp = bytes[i];
                    bytes[i] = bytes[j];
                    bytes[j] = temp;
                    i++;
                    j--;
                }
            }
        }
    }
}
