// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.BlueNrg2
{
    internal struct ServiceContext
    {
        public string ServiceUuid;
        public Characteristic[] Characteristics;
        public ushort[] AttributeHandles;
        public Descriptor[] Descriptors;
    }
}
