// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.BlueNrg2
{
    internal delegate int GattAccess(ushort connectionHandle, ushort attributeHandle, byte[] arg);

    internal struct Characteristic
    {
        public string Uuid;
        public GattAccess AccessCallback;
        public byte[] arg;
        public Descriptor[] Descriptors;
        public ushort Flags;
        public byte MinimumKeySize;
        public ushort ValueHandle;
    }
}