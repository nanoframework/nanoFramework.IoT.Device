// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Es7243e;
using nanoFramework.Hardware.Esp32;
using System;
using System.Device.I2c;
using System.Device.I2s;
using System.Diagnostics;
using System.Threading;

// This sample targets the ESP32-S3-BOX-Lite audio capture path (microphones -> ES7243E ADC).
//
// The ES7243E is configured over I2C. On the ESP32-S3-BOX-Lite the codec control bus is:
//   SCL = GPIO18, SDA = GPIO8
// The captured samples stream over I2S: DSIN = GPIO16, WS = GPIO47, BCLK = GPIO17, MCLK = GPIO2.

// Setup ESP32-S3-BOX-Lite I2C control bus (SDA = GPIO8, SCL = GPIO18).
// GPIO8/GPIO18 are passed as pin numbers directly: the Gpio helper enum omits the classic
// ESP32 flash pins (IO6-IO11), so the ESP32-S3 pin numbers are used as integer literals.
Configuration.SetPinFunction(8, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(18, DeviceFunction.I2C1_CLOCK);

I2cConnectionSettings settings = new I2cConnectionSettings(1, Es7243e.DefaultI2cAddress);
I2cDevice i2cDevice = new I2cDevice(settings);

Es7243e adc = new Es7243e(i2cDevice);

// Quick sanity check that the ES7243E acknowledges on the I2C bus at the configured address.
// If this is not "OK", the codec is not answering - try the alternate address (0x11) or
// double-check the SDA/SCL wiring. A codec that never gets configured only ever produces
// silence (peak amplitude = 0).
SpanByte probeWrite = new byte[] { 0x00 };
SpanByte probeRead = new byte[1];
I2cTransferResult probe = i2cDevice.WriteRead(probeWrite, probeRead);
bool codecPresent = probe.Status == I2cTransferStatus.FullTransfer;
Debug.WriteLine($"ES7243E I2C probe at 0x{Es7243e.DefaultI2cAddress:X2}: {(codecPresent ? "OK (codec present)" : "FAILED - check address/wiring")}");

// Setup the ESP32 I2S receiver FIRST so MCLK/BCLK/WS are already running.
// The ES7243E is a slave on the I2S bus and needs the master clock (MCLK) present while it
// is being configured over I2C, otherwise its internal clock manager never locks and the
// ADC only ever produces silence (peak amplitude = 0).
// ESP32-S3-BOX-Lite I2S pin-out (verify against your board revision):
//   BCLK = GPIO17, WS/LRCLK = GPIO47, DSIN (data from ADC) = GPIO16, MCLK = GPIO2.
Configuration.SetPinFunction(17, DeviceFunction.I2S1_BCK);
Configuration.SetPinFunction(47, DeviceFunction.I2S1_WS);
Configuration.SetPinFunction(16, DeviceFunction.I2S1_MDATA_IN);
Configuration.SetPinFunction(2, DeviceFunction.I2S1_MCK);

I2sDevice i2s = new I2sDevice(new I2sConnectionSettings(1)
{
    Mode = I2sMode.Master | I2sMode.Rx,
    CommunicationFormat = I2sCommunicationFormat.I2S,
    ChannelFormat = I2sChannelFormat.RightLeft,
    BitsPerSample = I2sBitsPerSample.Bit16,
    SampleRate = 16_000,
});

// A first read starts the I2S master clocks so the codec sees MCLK/BCLK before it is configured.
SpanByte warmup = new byte[256];
i2s.Read(warmup);

// Configure the codec over I2C using the ported esp_codec_dev sequence. The Initialize()
// sequence already puts the ES7243E into 16-bit I2S mode, so SetFormat()/SetMute() (whose
// register bit layout still needs datasheet validation) are intentionally not called here.
// The mic gain is set AFTER Start() because Start() resets it to a low value.
adc.Initialize();
adc.Start();
adc.SetMicGain(Es7243e.MaxMicGain);

Debug.WriteLine("ES7243E initialized and capturing. Speak or tap near the microphones.");

// Read audio and report a per-channel peak so you can see the mics are alive and which
// I2S slot carries the data.
SpanByte buffer = new byte[1024];

while (true)
{
    i2s.Read(buffer);

    int peakLeft = 0;
    int peakRight = 0;
    int sampleIndex = 0;
    for (int i = 0; i + 1 < buffer.Length; i += 2)
    {
        short sample = (short)(buffer[i] | (buffer[i + 1] << 8));
        int magnitude = sample < 0 ? -sample : sample;

        if ((sampleIndex & 1) == 0)
        {
            if (magnitude > peakRight)
            {
                peakRight = magnitude;
            }
        }
        else
        {
            if (magnitude > peakLeft)
            {
                peakLeft = magnitude;
            }
        }

        sampleIndex++;
    }

    Debug.WriteLine($"Peak amplitude  left = {peakLeft}  right = {peakRight}");
    Thread.Sleep(200);
}
