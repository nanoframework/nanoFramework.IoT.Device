// Copyright (c) 2017 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Threading;

namespace System.IO.Ports
{
    internal static class SerialPortExtensions
    {
        /// <summary>
        /// Using <see cref="SerialPort.Read(byte[], int, int)"/> to fetch a specific amount of bytes when they are not
        /// yet waiting in the buffer causes a timeout. Even if the bytes are received within that time. Instead, this
        /// helper will use Thread.Sleep() to allow background processing on the serial port. When BytesToRead reaches
        /// the expected number of bytes, we're done.
        /// </summary>
        /// <param name="serialPort">The serial port on which to wait.</param>
        /// <param name="numberOfBytes">The number of bytes to wait for.</param>
        /// <param name="sleepTimeInMs">The time to sleep if we don't have enough bytes yet.</param>
        /// <exception cref="TimeoutException">When the timeout configured in the SerialPort has been reached.</exception>
        /// <exception cref="ArgumentOutOfRangeException">When the <see cref="SerialPort.ReadBufferSize"/> is too small to facilitate waiting for the amount of requested bytes.</exception>
        internal static void WaitForData(this SerialPort serialPort, int numberOfBytes, int sleepTimeInMs = 5)
        {
            if (serialPort.ReadBufferSize < numberOfBytes)
            {
                throw new ArgumentOutOfRangeException();
            }

            var sw = Stopwatch.StartNew();
            while (serialPort.ReadTimeout == 0 || sw.ElapsedMilliseconds < serialPort.ReadTimeout)
            {
                if (serialPort.BytesToRead >= numberOfBytes)
                {
                    return;
                }

                Thread.Sleep(sleepTimeInMs);
            }

            throw new TimeoutException($"Timed out after {serialPort.ReadTimeout}ms waiting for {numberOfBytes} bytes of data, currently only {serialPort.BytesToRead} bytes are available");
        }
    }
}
