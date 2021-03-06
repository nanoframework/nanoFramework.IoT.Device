// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Iot.Device.SocketCan;

CanId id = new CanId()
{
    Standard = 0x1A // arbitrary id
};

Dictionary<string, Action> samples = new()
{
    { "send", SendExample },
    { "receive", ReceiveAllExample },
    { "receive-on-specific-address", ReceiveOnAddressExample },
};

if (args.Length == 0 || !samples.ContainsKey(args[0]))
{
    Debug.WriteLine("Usage: SocketCan.Sample <sample-name>");
    Debug.WriteLine("Available samples:");

    foreach (var kv in samples)
    {
        Debug.WriteLine($"- {kv.Key}");
    }

    return;
}

samples[args[0]]();

void SendExample()
{
    Debug.WriteLine($"Sending to id = 0x{id.Value:X2}");

    using CanRaw can = new CanRaw();
    byte[][] buffers = new byte[][]
    {
        new byte[8] { 1, 2, 3, 40, 50, 60, 70, 80 },
        new byte[7] { 1, 2, 3, 40, 50, 60, 70 },
        new byte[0] { },
        new byte[1] { 254 },
    };

    if (!id.IsValid)
    {
        // This is more form of the self-test rather than actual part of the sample
        throw new Exception("Id is invalid");
    }

    while (true)
    {
        foreach (byte[] buffer in buffers)
        {
            can.WriteFrame(buffer, id);
            string dataAsHex = string.Join(string.Empty, buffer.Select((x) => x.ToString("X2")));
            Debug.WriteLine($"Sending: {dataAsHex}");
            Thread.Sleep(1000);
        }
    }
}

void ReceiveAllExample()
{
    Debug.WriteLine("Listening for any id");

    using CanRaw can = new();
    byte[] buffer = new byte[8];

    while (true)
    {
        if (can.TryReadFrame(buffer, out int frameLength, out CanId id))
        {
            SpanByte data = new SpanByte(buffer, 0, frameLength);
            string type = id.ExtendedFrameFormat ? "EFF" : "SFF";
            string dataAsHex = string.Join(string.Empty, data.ToArray().Select((x) => x.ToString("X2")));
            Debug.WriteLine($"Id: 0x{id.Value:X2} [{type}]: {dataAsHex}");
        }
        else
        {
            Debug.WriteLine($"Invalid frame received!");
            SpanByte data = new SpanByte(buffer, 0, frameLength);
            string type = id.ExtendedFrameFormat ? "EFF" : "SFF";
            string dataAsHex = string.Join(string.Empty, data.ToArray().Select((x) => x.ToString("X2")));
            Debug.WriteLine($"Id: 0x{id.Value:X2} [{type}]: {dataAsHex}");
        }
    }
}

void ReceiveOnAddressExample()
{
    Debug.WriteLine($"Listening for id = 0x{id.Value:X2}");

    using CanRaw can = new();
    byte[] buffer = new byte[8];
    can.Filter(id);

    while (true)
    {
        if (can.TryReadFrame(buffer, out int frameLength, out CanId id))
        {
            SpanByte data = new SpanByte(buffer, 0, frameLength);
            string type = id.ExtendedFrameFormat ? "EFF" : "SFF";
            string dataAsHex = string.Join(string.Empty, data.ToArray().Select((x) => x.ToString("X2")));
            Debug.WriteLine($"Id: 0x{id.Value:X2} [{type}]: {dataAsHex}");
        }
        else
        {
            Debug.WriteLine($"Invalid frame received!");
        }
    }
}
