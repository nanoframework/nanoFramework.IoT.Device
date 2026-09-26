// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Threading;
using Iot.Device.Ndef;

namespace Iot.Device.M24Sr
{
    /// <summary>
    /// Driver for the ST M24SR dynamic NFC tag.
    /// </summary>
    public class M24Sr : IDisposable
    {
        /// <summary>
        /// Default 7-bit I2C address.
        /// </summary>
        public const byte DefaultI2cAddress = 0x56;

        private const byte OpenSessionCommand = 0x26;
        private const byte KillSessionCommand = 0x52;
        private const byte SelectFileInstruction = 0xA4;
        private const byte ReadBinaryInstruction = 0xB0;
        private const byte UpdateBinaryInstruction = 0xD6;
        private const ushort CapabilityContainerFileId = 0xE103;
        private const ushort SuccessStatus = 0x9000;
        private const int NdefLengthFieldSize = 2;
        private const int StatusResponseLength = 5;
        private const int FrameWaitingTimeExtensionLength = 4;
        private const int MaximumWritePayloadLength = 246;
        private const int PollDelayMilliseconds = 1;

        private static readonly byte[] NdefApplicationId = new byte[] { 0xD2, 0x76, 0x00, 0x00, 0x85, 0x01, 0x01 };

        private I2cDevice _i2cDevice;
        private byte _blockNumber;
        private bool _sessionOpen;

        /// <summary>
        /// Initializes a new instance of the <see cref="M24Sr"/> class.
        /// </summary>
        /// <param name="i2cDevice">I2C device used to communicate with the tag.</param>
        /// <param name="answerPollingAttempts">Maximum number of one-millisecond answer and session-acquisition polling attempts.</param>
        /// <exception cref="ArgumentNullException"><paramref name="i2cDevice"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="answerPollingAttempts"/> is less than or equal to zero.</exception>
        public M24Sr(I2cDevice i2cDevice, int answerPollingAttempts = 80)
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentNullException();
            if (answerPollingAttempts <= 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            AnswerPollingAttempts = answerPollingAttempts;
        }

        /// <summary>
        /// Gets the maximum number of answer and session-acquisition polling attempts.
        /// </summary>
        public int AnswerPollingAttempts { get; }

        /// <summary>
        /// Opens an I2C session with the tag, waiting for an active RF session to be released.
        /// </summary>
        /// <exception cref="InvalidOperationException">The session could not be acquired before the polling limit was reached.</exception>
        public void OpenSession()
        {
            for (int attempt = 0; attempt < AnswerPollingAttempts; attempt++)
            {
                I2cTransferResult result = _i2cDevice.WriteByte(OpenSessionCommand);
                if (result.Status == I2cTransferStatus.FullTransfer)
                {
                    _blockNumber = 0;
                    _sessionOpen = true;
                    return;
                }

                Thread.Sleep(PollDelayMilliseconds);
            }

            throw new InvalidOperationException();
        }

        /// <summary>
        /// Forcefully terminates the current session and opens an I2C session.
        /// </summary>
        /// <remarks>This command can interrupt an active RF session.</remarks>
        /// <exception cref="InvalidOperationException">The I2C write did not complete.</exception>
        public void KillSession()
        {
            Write(new byte[] { KillSessionCommand });
            _blockNumber = 0;
            _sessionOpen = true;
        }

        /// <summary>
        /// Closes the current I2C session.
        /// </summary>
        /// <exception cref="InvalidOperationException">No I2C session is open, an I2C transfer failed, the response timed out, or the response CRC is invalid.</exception>
        public void CloseSession()
        {
            EnsureSessionOpen();

            byte[] frame = new byte[3];
            frame[0] = 0xC2;
            AppendCrc(frame, 1);
            Write(frame);

            byte[] response = ReadResponse(3);
            if (!HasValidCrc(response))
            {
                throw new InvalidOperationException();
            }

            _sessionOpen = false;
        }

        /// <summary>
        /// Reads the NDEF capability container.
        /// </summary>
        /// <returns>The parsed capability container.</returns>
        /// <exception cref="ArgumentException">The capability container data is missing or shorter than the minimum supported length.</exception>
        /// <exception cref="InvalidOperationException">No I2C session is open, an I2C transfer failed, the response timed out, the response or capability container is invalid, or the tag returned an unsuccessful status.</exception>
        public M24SrCapabilityContainer ReadCapabilityContainer()
        {
            EnsureSessionOpen();
            SelectApplication();
            SelectFile(CapabilityContainerFileId);

            byte[] buffer = ReadBinary(0, M24SrCapabilityContainer.MinimumLength);
            return new M24SrCapabilityContainer(buffer);
        }

        /// <summary>
        /// Reads the NDEF message stored in the tag.
        /// </summary>
        /// <returns>The NDEF message.</returns>
        /// <exception cref="ArgumentException">The capability container data is missing or shorter than the minimum supported length.</exception>
        /// <exception cref="InvalidOperationException">No I2C session is open, an I2C transfer failed, the response timed out, the response or capability container is invalid, the tag returned an unsuccessful status, or the NDEF length exceeds the advertised capacity.</exception>
        public NdefMessage ReadNdefMessage()
        {
            M24SrCapabilityContainer capabilityContainer = ReadCapabilityContainer();
            SelectFile(capabilityContainer.NdefFileId);

            byte[] lengthBuffer = ReadBinary(0, NdefLengthFieldSize);
            int messageLength = (lengthBuffer[0] << 8) | lengthBuffer[1];
            if (messageLength + NdefLengthFieldSize > capabilityContainer.MaximumNdefFileSize)
            {
                throw new InvalidOperationException();
            }

            byte[] message = new byte[messageLength];
            int offset = 0;
            int maximumReadLength = capabilityContainer.MaximumReadLength;
            while (offset < messageLength)
            {
                int count = messageLength - offset;
                if (count > maximumReadLength)
                {
                    count = maximumReadLength;
                }

                if (count > 250)
                {
                    count = 250;
                }

                byte[] block = ReadBinary((ushort)(offset + NdefLengthFieldSize), count);
                new SpanByte(block).CopyTo(new SpanByte(message).Slice(offset, count));
                offset += count;
            }

            return new NdefMessage(message);
        }

        /// <summary>
        /// Writes an NDEF message to the tag.
        /// </summary>
        /// <param name="message">The NDEF message to write.</param>
        /// <exception cref="ArgumentNullException"><paramref name="message"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The capability container data is invalid or the NDEF message exceeds the advertised capacity.</exception>
        /// <exception cref="InvalidOperationException">No I2C session is open, the NDEF file is write-protected, an I2C transfer failed, the response timed out, the response or capability container is invalid, or the tag returned an unsuccessful status.</exception>
        public void WriteNdefMessage(NdefMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException();
            }

            M24SrCapabilityContainer capabilityContainer = ReadCapabilityContainer();
            if (!capabilityContainer.IsWriteAllowed)
            {
                throw new InvalidOperationException();
            }

            if (message.Length + NdefLengthFieldSize > capabilityContainer.MaximumNdefFileSize)
            {
                throw new ArgumentException();
            }

            byte[] serializedMessage = new byte[message.Length];
            message.Serialize(serializedMessage);
            SelectFile(capabilityContainer.NdefFileId);

            UpdateBinary(0, new byte[] { 0, 0 });

            int offset = 0;
            int maximumWriteLength = capabilityContainer.MaximumWriteLength;
            if (maximumWriteLength > MaximumWritePayloadLength)
            {
                maximumWriteLength = MaximumWritePayloadLength;
            }

            while (offset < serializedMessage.Length)
            {
                int count = serializedMessage.Length - offset;
                if (count > maximumWriteLength)
                {
                    count = maximumWriteLength;
                }

                UpdateBinary((ushort)(offset + NdefLengthFieldSize), new SpanByte(serializedMessage).Slice(offset, count));
                offset += count;
            }

            UpdateBinary(0, new byte[] { (byte)(message.Length >> 8), (byte)message.Length });
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _i2cDevice?.Dispose();
            _i2cDevice = null;
        }

        private static void ValidateResponse(byte[] response)
        {
            if (!HasValidCrc(response))
            {
                throw new InvalidOperationException();
            }

            ushort status = (ushort)((response[response.Length - 4] << 8) | response[response.Length - 3]);
            if (status != SuccessStatus)
            {
                throw new InvalidOperationException();
            }
        }

        private static bool HasValidCrc(byte[] data)
        {
            return ComputeCrc(data, data.Length) == 0;
        }

        private static bool HasValidCrc(byte[] data, int length)
        {
            return ComputeCrc(data, length) == 0;
        }

        private static bool IsFrameWaitingTimeExtension(byte[] response)
        {
            return (response[0] & 0xC0) == 0xC0;
        }

        private static void AppendCrc(byte[] data, int length)
        {
            if ((data == null) || (length <= 0) || (data.Length + 2 < length))
            {
                throw new ArgumentException();
            }

            ushort crc = ComputeCrc(data, length);
            data[length] = (byte)crc;
            data[length + 1] = (byte)(crc >> 8);
        }

        private static ushort ComputeCrc(byte[] data, int length)
        {
            ushort crc = 0x6363;
            for (int index = 0; index < length; index++)
            {
                byte value = (byte)(data[index] ^ (byte)crc);
                value = (byte)(value ^ (value << 4));
                crc = (ushort)((crc >> 8) ^ (value << 8) ^ (value << 3) ^ (value >> 4));
            }

            return crc;
        }

        private void SelectApplication()
        {
            ExecuteCommand(SelectFileInstruction, 0x04, 0x00, NdefApplicationId, false, 0, 0);
        }

        private void SelectFile(ushort fileId)
        {
            byte[] file = new byte[] { (byte)(fileId >> 8), (byte)fileId };
            ExecuteCommand(SelectFileInstruction, 0x00, 0x0C, file, false, 0, 0);
        }

        private byte[] ReadBinary(ushort offset, int count)
        {
            if ((count <= 0) || (count > 250))
            {
                throw new ArgumentOutOfRangeException();
            }

            return ExecuteCommand(
                ReadBinaryInstruction,
                (byte)(offset >> 8),
                (byte)offset,
                null,
                true,
                (byte)count,
                count);
        }

        private void UpdateBinary(ushort offset, SpanByte data)
        {
            if ((data.Length <= 0) || (data.Length > MaximumWritePayloadLength))
            {
                throw new ArgumentOutOfRangeException();
            }

            byte[] dataArray = new byte[data.Length];
            data.CopyTo(dataArray);
            ExecuteCommand(
                UpdateBinaryInstruction,
                (byte)(offset >> 8),
                (byte)offset,
                dataArray,
                false,
                0,
                0);
        }

        private byte[] ExecuteCommand(byte instruction, byte parameter1, byte parameter2, byte[] data, bool hasExpectedLength, byte expectedLength, int responseDataLength)
        {
            int dataLength = data == null ? 0 : data.Length;
            int frameLength = 7 + dataLength + (data == null ? 0 : 1) + (hasExpectedLength ? 1 : 0);
            byte[] frame = new byte[frameLength];
            int index = 0;

            frame[index++] = (byte)(0x02 | _blockNumber);
            _blockNumber ^= 0x01;
            frame[index++] = 0x00;
            frame[index++] = instruction;
            frame[index++] = parameter1;
            frame[index++] = parameter2;

            if (data != null)
            {
                frame[index++] = (byte)dataLength;
                new SpanByte(data).CopyTo(new SpanByte(frame).Slice(index, dataLength));
                index += dataLength;
            }

            if (hasExpectedLength)
            {
                frame[index++] = expectedLength;
            }

            AppendCrc(frame, index);
            Write(frame);

            int responseLength = responseDataLength + StatusResponseLength;
            byte[] response = ReadResponse(responseLength);
            int extensionCount = 0;
            while (IsFrameWaitingTimeExtension(response))
            {
                if (!HasValidCrc(response, FrameWaitingTimeExtensionLength) || (++extensionCount > AnswerPollingAttempts))
                {
                    throw new InvalidOperationException();
                }

                response = HandleFrameWaitingTimeExtension(response, responseLength);
            }

            ValidateResponse(response);

            byte[] responseData = new byte[responseDataLength];
            if (responseDataLength > 0)
            {
                new SpanByte(response).Slice(1, responseDataLength).CopyTo(responseData);
            }

            return responseData;
        }

        private byte[] HandleFrameWaitingTimeExtension(byte[] response, int responseLength)
        {
            byte[] extensionResponse = new byte[FrameWaitingTimeExtensionLength];
            new SpanByte(response).Slice(0, 2).CopyTo(extensionResponse);
            AppendCrc(extensionResponse, 2);
            Write(extensionResponse);
            return ReadResponse(responseLength);
        }

        private byte[] ReadResponse(int length)
        {
            byte[] response = new byte[length];
            for (int attempt = 0; attempt < AnswerPollingAttempts; attempt++)
            {
                I2cTransferResult result = _i2cDevice.Read(response);
                if (result.Status == I2cTransferStatus.FullTransfer)
                {
                    return response;
                }

                if ((result.BytesTransferred >= FrameWaitingTimeExtensionLength) &&
                    IsFrameWaitingTimeExtension(response) &&
                    HasValidCrc(response, FrameWaitingTimeExtensionLength))
                {
                    return response;
                }

                Thread.Sleep(PollDelayMilliseconds);
            }

            throw new InvalidOperationException();
        }

        private void Write(SpanByte data)
        {
            I2cTransferResult result = _i2cDevice.Write(data);
            if (result.Status != I2cTransferStatus.FullTransfer)
            {
                throw new InvalidOperationException();
            }
        }

        private void EnsureSessionOpen()
        {
            if (!_sessionOpen)
            {
                throw new InvalidOperationException();
            }
        }
    }
}