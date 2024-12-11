// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Text;

namespace Iot.Device.MulticastDns.Package
{
    internal class PacketParser
    {
        private readonly IBitConverter _converter = EndianBitConverter.Big;
        private readonly Encoding _encoding = Encoding.UTF8;
        private readonly byte[] _data;
        private int _position = 0;

        public PacketParser(byte[] bytes)
        {
            _data = bytes;
        }

        private void MovePosition(int n)
        {
            _position += n;

            if (_position > _data.Length)
            {
                throw new IndexOutOfRangeException("No more data in packet");
            }
        }

        public byte ReadByte() => _data[_position++];

        public ushort ReadUShort()
        {
            ushort value = _converter.ToUInt16(_data, _position);
            MovePosition(2);
            return value;
        }

        public int ReadInt()
        {
            int value = _converter.ToInt32(_data, _position);
            MovePosition(4);
            return value;
        }

        public byte[] ReadBytes(int count)
        {
            byte[] value = new byte[count];
            System.Array.Copy(_data, _position, value, 0, count);
            MovePosition(count);
            return value;
        }

        public string ReadString()
        {
            int length = ReadByte();
            int remaining = _data.Length - _position;

            if (length > remaining)
            {
                length = remaining;
            }

            string value = _encoding.GetString(_data, _position, length);
            MovePosition(length);
            return value;
        }

        public string ReadDomain()
        {
            int dot = 0;
            string domain = string.Empty;

            while (true)
            {
                string label = PopLabel(ref dot);

                if (label == null)
                {
                    break;
                }

                domain += label + ".";
            }

            return domain.Trim('.');
        }

        private string PopLabel(ref int dot)
        {
            int length = PopLabelLength(ref dot);
            if (length == 0)
            {
                return null;
            }

            if (dot != 0)
            {
                return GetPointer(length, ref dot);
            }

            string label = _encoding.GetString(_data, _position, length);
            MovePosition(length);

            return label;
        }

        private int PopLabelLength(ref int dot)
        {
            if (dot != 0)
            {
                return GetPointerLength(ref dot);
            }

            int length = _data[_position++];

            // The length we found at this position has two possible meanings:
            // - Either we have a range between 0 and 192 (0xc0) which indicates the length of the label.
            //   which will follow in the next bytes.
            // - Or we have a pointer to another section in the data, see following comment.
            if ((length & 0xc0) != 0xc0)
            {
                return length;
            }

            // If we get here we are dealing with a pointer to another section in the data.
            // The pointer consists of 16 bits:
            // - The first 8 bits are equal to the value over 192 (0xc0) in the first (previous) byte.
            // - We shift these 8 bits to the left and add the value of the next (current) byte.
            // - This gives us the index where we will find the length of the next label.
            // Example:
            // - First byte 0xc2, second byte 0x11
            // - 0xc2 & 0x3f = 0x02
            // - 0x02 << 8 = 0x0200
            // - 0x0200 + 0x11 = 0x0211
            dot = ((length & 0x3f) << 8) + _data[_position++];

            return GetPointerLength(ref dot);
        }

        private string GetPointer(int length, ref int dot)
        {
            if (length == 0)
            {
                return null;
            }

            string label = _encoding.GetString(_data, dot, length);

            dot += length;
            return label;
        }

        private int GetPointerLength(ref int dot)
        {
            if (dot > _data.Length)
            {
                throw new IndexOutOfRangeException($"Read past end of packet {dot}/{_data.Length}");
            }

            int length = _data[dot++];

            // Magic numbers got explained above
            if ((length & 0xc0) != 0xc0)
            {
                return length;
            }

            // Magic numbers got explained above
            dot = ((length & 0x3f) << 8) + _data[dot];

            return GetPointerLength(ref dot);
        }
    }
}
