// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.BlueNrg2
{
    internal struct Descriptor
    {
        public string Uuid;
        public byte AttributeFlags;
        public byte MinimumKeySize;
        public GattAccess AccessCallback;
        public byte[] Arg;
    }
}