// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.I2c;

namespace Iot.Device.Mcp23xxx
{
    public abstract partial class Mcp23xxx
    {
        /// <summary>
        /// I2C adapter
        /// </summary>
        protected class I2cAdapter : BusAdapter
        {
            private I2cDevice _device;

            /// <summary>
            /// Constructs I2cAdapter instance
            /// </summary>
            /// <param name="device">I2C device</param>
            public I2cAdapter(I2cDevice device) => _device = device;

            /// <inheritdoc/>
            public override void Dispose() => _device?.Dispose();

            /// <inheritdoc/>
            public override void Read(byte registerAddress, SpanByte buffer)
            {
                // Set address to register first.
                Write(registerAddress, SpanByte.Empty);
                _device.Read(buffer);
            }

            /// <inheritdoc/>
            public override void Write(byte registerAddress, SpanByte data)
            {
                SpanByte output = new byte[data.Length + 1];
                output[0] = registerAddress;
                // SpanByte slice method is different to Span<Byte> slice method in case that index argument (here 1) is equal to size of slice
                if (data.Length > 0)
                {
                    // do not override output[0]
                    data.CopyTo(output.Slice(1));
                }
                _device.Write(output);
            }
        }
    }
}
