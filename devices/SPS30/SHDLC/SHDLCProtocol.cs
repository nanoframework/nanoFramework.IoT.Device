﻿//
// Copyright (c) 2017 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.
//

using System;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;

namespace Iot.Device.Sps30.Shdlc
{
    /// <summary>
    /// Minimal implementation of the SHDLC protocol used by Sensirion. This implementation is based upon the information
    /// available in the SPS30 particulate matter sensor datasheet.
    /// </summary>
    /// <remarks>
    /// Datasheet can be found at https://sensirion.com/media/documents/8600FF88/616542B5/Sensirion_PM_Sensors_Datasheet_SPS30.pdf
    /// Information about SHDLC can be found at https://sensirion.github.io/python-shdlc-driver/shdlc.html
    /// </remarks>
    public class ShdlcProtocol
    {
        /// <summary>
        /// The SerialPort is exposed as public property, but should only be used in very specific circumstances. One example is the Sensirion SPS30, for which a raw "0xFF" must be sent to wake the device from its sleep.
        /// </summary>
        public SerialPort Serial { get; private set; }

        /// <summary>
        /// Wraps a serial port to be used for the SHDLC protocol that some Sensirion devices use.
        /// </summary>
        /// <param name="serialPort">The serial port to be used.</param>
        /// <param name="timeoutInMillis">Timeout to be used on the serial port and as communication timeout for commands.</param>
        public ShdlcProtocol(SerialPort serialPort, int timeoutInMillis = 10000)
        {
            Serial = serialPort;
            Serial.ReadTimeout = timeoutInMillis;
            Serial.WriteTimeout = timeoutInMillis;
        }

        /// <summary>
        /// Execute a command on the SHDLC device. Its documentation should describe the address, commands and what data to send/receive.
        /// </summary>
        /// <returns>The reply of this command. This may be an empty array if this command has no reply.</returns>
        /// <exception cref="ApplicationException">When the frame could not be verified, a timeout occurred or the device indicates a fault status</exception>
        public byte[] Execute(byte devaddr, byte command, byte[] data)
        {
            // Make sure the serial port is opened
            if (!Serial.IsOpen)
            {
                Serial.Open();
            }

            // Generate and send our frame
            var frameBuffer = GenerateMosiFrame(devaddr, command, data);
            Serial.Write(frameBuffer, 0, frameBuffer.Length);

            // Read reply and parse response
            return ReadMosiFrameContent(devaddr, command);
        }

        private byte[] GenerateMosiFrame(byte devaddr, byte command, byte[] data)
        {
            // Calculate checksum
            byte chk = 0;
            chk += devaddr;
            chk += command;
            chk += (byte)data.Length;
            foreach (var b in data)
            {
                chk += b;
            }

            chk = (byte)(0xff ^ chk);

            // Create frame
            using var frame = new MemoryStream();
            frame.WriteByte(0x7e);
            frame.WriteByteStuffed(devaddr);
            frame.WriteByteStuffed(command);
            frame.WriteByteStuffed((byte)data.Length);
            frame.WriteStuffed(data, 0, data.Length);
            frame.WriteByteStuffed(chk);
            frame.WriteByte(0x7e);

            return frame.ToArray();
        }

        /// <summary>
        /// To allow for garbage, corruption or old responses in the serial buffer, we'll keep reading until there's no more data. Then we return the last valid frame, or the most recent error.
        /// </summary>
        /// <param name="expectedDevAddr">The expected device address, used in validation of the reply</param>
        /// <param name="expectedCommand">The expected command, used in validation of the reply</param>
        /// <returns>The actual content of the reply. Does not include frame headers such as dev address or command byte.</returns>
        /// <exception cref="ApplicationException">When the frame could not be verified, a timeout occurred or the device indicates a fault status</exception>
        private byte[] ReadMosiFrameContent(byte expectedDevAddr, byte expectedCommand)
        {
            var sw = Stopwatch.StartNew();
            Exception lastError = null;
            while (true)
            {
                // This timeout check is meant to activate for situations where garbage keeps coming in at a steady pace within the ReadTimeout, possibly causing
                // an infinite loop.
                if (Serial.ReadTimeout > 0 && sw.ElapsedMilliseconds > Serial.ReadTimeout)
                {
                    break;
                }

                try
                {
                    // Read a frame's content, so without start/end byte (0x7e)
                    var frame = Serial.ReadUnstuffedFrameContent();
                    if (frame == null || frame.Length < 5)
                    {
                        throw new ApplicationException($"Received an invalid frame (less than minimal size)");
                    }

                    // Parse and validate the content and return the data within it
                    var content = ParseAndValidateMisoFrame(expectedDevAddr, expectedCommand, frame);
                    return content;
                }
                catch (Exception ex)
                {
                    lastError = ex;
                }
            }

            // We get here when the timeout occurs.
            // - If we did have an error, we return the most recent one since that (presumably) relates to the most recently sent request.
            // - When we did not get an error, we ran into a timeout.
            if (lastError != null)
            {
                throw lastError;
            }
            else
            {
                throw new ApplicationException("Timeout waiting for a valid frame");
            }
        }

        /// <summary>
        /// Validates the MISO frame using its checksum and returns the actual reply. No exception means valid and no device error.
        /// When a MISO frame contains an error, an exception with relevant information is thrown.
        /// </summary>
        /// <param name="expectedDevAddr">The expected device address used to validate the reply</param>
        /// <param name="expectedCommand">The expected command used to validate the reply</param>
        /// <param name="unstuffedFrame">The unverified frame, without frame start and end bytes.</param>
        /// <returns>The actual command reply contained within the frame</returns>
        /// <exception cref="ApplicationException">When the frame could not be verified or the device indicates a fault status</exception>
        private byte[] ParseAndValidateMisoFrame(byte expectedDevAddr, byte expectedCommand, byte[] unstuffedFrame)
        {
            // Parse incoming fields
            var addr = unstuffedFrame[0];
            var cmd = unstuffedFrame[1];
            var state = unstuffedFrame[2];
            var len = unstuffedFrame[3];
            var data = new byte[len]; // Even on corrupted read this is max 255 byte allocation, hence no protection
            Array.Copy(unstuffedFrame, 4, data, 0, len);
            var expectedChk = unstuffedFrame[unstuffedFrame.Length - 1];

            // Verify checksum and other fields
            byte chk = 0;
            chk += addr;
            chk += cmd;
            chk += state;
            chk += len;
            foreach (var b in data)
            {
                chk += b;
            }

            chk = (byte)(0xff ^ chk);
            if (chk != expectedChk)
            {
                throw new ApplicationException($"Invalid checksum (calculated 0x{chk:X2}, got 0x{expectedChk:X2})");
            }

            if (addr != expectedDevAddr)
            {
                throw new ApplicationException($"Received response from a different device address than we were expecting (expected 0x{expectedDevAddr:X2}, got response from 0x{addr:X2})");
            }

            if (cmd != expectedCommand)
            {
                throw new ApplicationException($"Received response on a different command than we were expecting (expected cmd 0x{expectedCommand:X2}, got cmd 0x{cmd:X2})");
            }

            // Verify state
            switch (state)
            {
                case 0x00:
                    // everything gucci
                    break;
                case 0x01:
                    throw new ApplicationException($"Wrong data length for this command (too much or too little data) (cmd=0x{cmd:X2})");
                case 0x02:
                    throw new ApplicationException($"Unknown command (cmd=0x{cmd:X2})");
                case 0x03:
                    throw new ApplicationException($"No acess right for command (cmd=0x{cmd:X2})");
                case 0x04:
                    throw new ApplicationException($"Illegal command parameter or parameter out of allowed range (cmd=0x{cmd:X2})");
                case 0x28:
                    throw new ApplicationException($"Internal function argument out of range (cmd=0x{cmd:X2})");
                case 0x43:
                    throw new ApplicationException($"Command not allowed in current state (cmd=0x{cmd:X2})");
                default:
                    throw new ApplicationException($"Received an unspecified error code (cmd=0x{cmd:X2}, error=0x{state:X2})");
            }

            return data;
        }
    }
}
