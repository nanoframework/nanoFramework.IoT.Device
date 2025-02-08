// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Mcp25xxx.Can
{
    public interface ICanController
    {
        CanMessage GetMessage();
        void Initialize(Mcp25xxx mcp25xxx);
        void Reset();
        void SetBitRate(int baudRate, int clockFrequency);
        void WriteMessage(CanMessage message);
    }
}