// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Text;

namespace Iot.Device.MulticastDNS.Package
{
    internal class PacketBuilder
    {
        private readonly IBitConverter _converter = EndianBitConverter.Big;
        private readonly ArrayList _list = new ArrayList();

        public void Add(byte value) => _list.Add(value);

        public void Add(ushort value) => _list.AddRange(_converter.GetBytes(value));

        public void Add(int value) => _list.AddRange(_converter.GetBytes(value));

        public void Add(byte[] bytes) => _list.AddRange(bytes);

        public void Add(string domain)
        {
            char[] dots = { '.' };
            string[] labels = domain.Trim(dots).Split(dots);
            foreach (string label in labels)
            {
                Add(new Label(label));
            }
            Add((byte)0);  // end the name
        }

        private void Add(Label label)
        {
            Add(label.Length);
            Add(label.GetBytes());
        }

        public byte[] GetBytes()
        {
            byte[] bytes = new byte[_list.Count];
            _list.CopyTo(bytes);
            return bytes;
        }

        private class Label
        {
            private string _label;

            public Label(string label) => _label = label;

            public byte Length => (byte)_label.Length;

            public byte[] GetBytes() => Encoding.UTF8.GetBytes(_label);
        }
    }
}
