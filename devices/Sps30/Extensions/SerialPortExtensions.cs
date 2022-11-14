// Copyright (c) 2017 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Threading;

namespace System.IO.Ports
{
    /// <summary>
    /// Extensions on the SerialPort to facilitate unstuffing and reading reply frames.
    /// </summary>
    public static class SerialPortExtensions
    {
        /// <summary>
        /// Attempts to read an SHDLC frame, which has a 0x7e start/end bytes with stuffing/escaping in between. The
        /// returned frame will be unstuffed in the process.
        /// </summary>
        /// <param name="serialPort">Serial port instance.</param>
        /// <returns>An SHDLC frame, unstuffed, but without the 0x7e start/end bytes.</returns>
        /// <exception cref="ApplicationException">When a timeout occurs waiting for a valid frame.</exception>
        public static byte[] ReadUnstuffedFrameContent(this SerialPort serialPort)
        {
            var sw = Stopwatch.StartNew();
            using (var frame = new MemoryStream())
            {
                // Read until frame end byte
                while (true)
                {
                    // This timeout check is meant to activate for situations where garbage keeps coming in at a steady pace within the ReadTimeout, possibly causing
                    // an infinite loop.
                    if (serialPort.ReadTimeout > 0 && sw.ElapsedMilliseconds > serialPort.ReadTimeout)
                    {
                        throw new ApplicationException("Timeout waiting for frame content or end byte (0x7e)");
                    }

                    // Turns out sleeping works better than ReadByte() when we're waiting for data (nanoframework bug?)
                    if (serialPort.BytesToRead == 0)
                    {
                        Thread.Sleep(5);
                        continue;
                    }

                    // Read the next byte
                    var b = (byte)serialPort.ReadByte();

                    // If we receive 0x7e while we're below minimum frame size, we'll assume it's a (new) start byte and clean the buffer
                    if (b == 0x7e && frame.Position < 5)
                    {
                        frame.Position = 0;
                        continue;
                    }

                    // If we receive 0x7e when we already have enough data, we'll assume it's the end byte.
                    if (b == 0x7e && frame.Position >= 5)
                    {
                        break;
                    }

                    // If we receive the escape character, we'll check the next byte to see how it should be interpreted
                    if (b == 0x7d)
                    {
                        // Unstuff
                        var next = (byte)serialPort.ReadByte();
                        switch (next)
                        {
                            case 0x5e:
                                frame.WriteByte(0x7e);
                                break;
                            case 0x5d:
                                frame.WriteByte(0x7d);
                                break;
                            case 0x31:
                                frame.WriteByte(0x11);
                                break;
                            case 0x33:
                                frame.WriteByte(0x13);
                                break;
                            default:
                                throw new ApplicationException($"Error unstuffing 0x7d followed by 0x{next:X2}, which is invalid");
                        }

                        continue;
                    }

                    // Any other byte is just data to add to the frame
                    frame.WriteByte(b);
                }

                return frame.ToArray();
            }
        }
    }
}