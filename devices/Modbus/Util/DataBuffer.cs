// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Modbus.Util
{
    internal class DataBuffer
    {
        public DataBuffer(int byteCount) : this(new byte[byteCount])
        {
        }

        public DataBuffer(byte[] bytes)
        {
            Buffer = bytes ?? throw new ArgumentNullException(nameof(bytes));
        }

        public byte[] Buffer { get; private set; }

        public int Length => (Buffer != null ? Buffer.Length : 0);

        public byte this[int index]
        {
            get
            {
                return Get(index);
            }

            set
            {
                Set(index, value);
            }
        }

        #region Add

        public void Add(params byte[] bytes)
        {
            byte[] newBytes = new byte[Length + bytes.Length];
            Array.Copy(Buffer, 0, newBytes, 0, Length);
            Array.Copy(bytes, 0, newBytes, Length, bytes.Length);

            Buffer = newBytes;
        }

        public void Add(ushort value)
        {
            byte[] blob = BitConverter.GetBytes(value);
            blob.JudgReverse();

            Add(blob);
        }

        #endregion

        #region Setter

        public void Set(int index, params byte[] bytes)
        {
            if (Length < index + bytes.Length)
            {
                throw new ArgumentOutOfRangeException("Buffer too small.");
            }

            if (bytes.Length == 1)
            {
                Buffer[index] = bytes[0];
            }
            else
            {
                Array.Copy(bytes, 0, Buffer, index, bytes.Length);
            }
        }

        public void Set(int index, ushort value)
        {
            byte[] blob = BitConverter.GetBytes(value);
            blob.JudgReverse();

            Set(index, blob);
        }

        public void Set(int index, short value)
        {
            byte[] blob = BitConverter.GetBytes(value);
            blob.JudgReverse();

            Set(index, blob);
        }

        #endregion

        #region Getter

        public byte[] Get(int index, int count)
        {
            if (Length < index + count)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            byte[] bytes = new byte[count];
            Array.Copy(Buffer, index, bytes, 0, count);

            return bytes;
        }

        public byte Get(int index)
        {
            return Buffer[index];
        }

        public ushort GetUInt16(int index)
        {
            byte[] blob = Get(index, 2);
            blob.JudgReverse();

            return BitConverter.ToUInt16(blob, 0);
        }

        public short GetInt16(int index)
        {
            byte[] blob = Get(index, 2);
            blob.JudgReverse();

            return BitConverter.ToInt16(blob, 0);
        }

        #endregion
    }
}