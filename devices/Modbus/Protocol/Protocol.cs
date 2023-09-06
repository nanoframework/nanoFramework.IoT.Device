// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Modbus.Util;

namespace Iot.Device.Modbus.Protocol
{
    internal abstract class Protocol
    {
        public DataBuffer Data { get; set; }

        public bool IsValid { get; protected set; } = true;

        protected bool IsEmpty(byte[] bytes)
        {
            if (bytes != null && bytes.Length != 0)
            {
                foreach (byte b in bytes)
                {
                    if (b != 0)
                    {
                        return false;
                    }
                }
            }

            IsValid = false;
            return true;
        }
    }
}
