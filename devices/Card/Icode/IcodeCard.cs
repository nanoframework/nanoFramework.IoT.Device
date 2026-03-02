// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
#if DEBUG
using Microsoft.Extensions.Logging;
using nanoFramework.Logging;
#endif

namespace Iot.Device.Card.Icode
{
    /// <summary>
    /// An ICODE card class. Supports ICODE SLIX, ICODE SLIX2, ICODE DNA, ICODE 3.
    /// Implements ISO/IEC 15693 commands for reading, writing, and managing vicinity cards.
    /// </summary>
    public class IcodeCard
    {
        private const byte BytesPerBlock = 4;

        /// <summary>
        /// ISO 15693 addressed mode flags: high data rate (bit 1) + addressed (bit 5).
        /// </summary>
        private const byte AddressedModeFlags = 0x22;

        // This is the actual RFID reader
        private readonly CardTransceiver _rfid;

#if DEBUG
        private readonly ILogger _logger;
#endif

        // the expected size of the response
        private ushort _responseSize;

        // The command to execute on the card
        private IcodeCardCommand _command;

        /// <summary>
        /// The tag number detected by the reader.
        /// </summary>
        public byte Target { get; set; }

        /// <summary>
        /// Unique identifier (UID) of the card — 8 bytes for ISO 15693.
        /// </summary>
        public byte[]? Uid { get; set; }

        /// <summary>
        /// The storage capacity of the card.
        /// </summary>
        public IcodeCardCapacity Capacity { get; set; }

        /// <summary>
        /// The block number to read or write.
        /// </summary>
        public byte BlockNumber { get; set; }

        /// <summary>
        /// The block count when reading multiple blocks.
        /// </summary>
        public byte BlockCount { get; set; }

        /// <summary>
        /// The data which has been read or to write for the specific block.
        /// </summary>
        public byte[] Data { get; set; } = new byte[0];

        /// <summary>
        /// Application Family Identifier (AFI).
        /// Represents the type of application targeted by the VCD.
        /// </summary>
        public byte Afi { get; set; }

        /// <summary>
        /// Data Storage Format Identifier (DSFID).
        /// Indicates how the data is structured in the VICC memory.
        /// </summary>
        public byte Dsfid { get; set; }

        /// <summary>
        /// Constructor for IcodeCard.
        /// </summary>
        /// <param name="rfid">A card transceiver class.</param>
        /// <param name="target">The target number as some card readers attribute one.</param>
        public IcodeCard(CardTransceiver rfid, byte target)
        {
            _rfid = rfid;
            Target = target;
#if DEBUG
            _logger = this.GetCurrentClassLogger();
#endif
        }

        /// <summary>
        /// Provide a calculation of CRC for ISO 15693.
        /// Note: The PN5180 module handles CRC in hardware and does not need this for normal operation.
        /// </summary>
        /// <param name="buffer">The buffer to calculate CRC over.</param>
        /// <param name="crc">The CRC result — must be a 2-byte array.</param>
        public void CalculateCrcIso15693(SpanByte buffer, SpanByte crc)
        {
            if (crc.Length != 2)
            {
                throw new ArgumentException($"The length of crc must be 2 bytes.", nameof(crc));
            }

            ushort polynomial = 0x8408;
            ushort currentCrc = 0xFFFF;

            // ISO 15693-3 CRC-16 calculation
            for (int i = 0; i < buffer.Length; i++)
            {
                currentCrc = (ushort)(currentCrc ^ buffer[i]);
                for (int j = 0; j < 8; j++)
                {
                    if ((currentCrc & 0x0001) != 0)
                    {
                        currentCrc = (ushort)((currentCrc >> 1) ^ polynomial);
                    }
                    else
                    {
                        currentCrc = (ushort)(currentCrc >> 1);
                    }
                }
            }

            // Final XOR: invert all bits
            currentCrc = (ushort)~currentCrc;
            crc[0] = (byte)(currentCrc & 0xFF);
            crc[1] = (byte)((currentCrc >> 8) & 0xFF);
        }

        /// <summary>
        /// Run the last setup command. In case of reading bytes, they are automatically pushed
        /// into the <see cref="Data"/> property.
        /// </summary>
        /// <returns>-1 if the process fails, otherwise the number of bytes read.</returns>
        private int RunIcodeCardCommand()
        {
            byte[] requestData = Serialize();
            byte[] dataOut = new byte[_responseSize];

            if (requestData.Length > _rfid.MaximumWriteSize)
            {
#if DEBUG
                _logger.LogDebug($"{nameof(RunIcodeCardCommand)}: Request size {requestData.Length} exceeds transceiver MaximumWriteSize {_rfid.MaximumWriteSize}.");
#endif
                return -1;
            }

            if (_responseSize > _rfid.MaximumReadSize)
            {
#if DEBUG
                _logger.LogDebug($"{nameof(RunIcodeCardCommand)}: Expected response size {_responseSize} exceeds transceiver MaximumReadSize {_rfid.MaximumReadSize}.");
#endif
                return -1;
            }

            var ret = _rfid.Transceive(Target, requestData, dataOut, NfcProtocol.Iso15693);
#if DEBUG
            _logger.LogDebug($"{nameof(RunIcodeCardCommand)}: {_command}, Target: {Target}, Data: {BitConverter.ToString(requestData)}, Success: {ret}, Dataout: {BitConverter.ToString(dataOut)}");
#endif
            if (ret > 0)
            {
                if (ret < dataOut.Length)
                {
                    // Trim to actual received length to avoid trailing zeros
                    byte[] trimmed = new byte[ret];
                    Array.Copy(dataOut, 0, trimmed, 0, ret);
                    Data = trimmed;
                }
                else
                {
                    Data = dataOut;
                }
            }

            return ret;
        }

        /// <summary>
        /// Serialize request data according to ISO 15693 protocol.
        /// Request format: SOF, Flags, Command code, Parameters (opt.), Data (opt.), CRC16, EOF.
        /// The PN5180 handles SOF/EOF and CRC automatically.
        /// </summary>
        /// <returns>The serialized byte array.</returns>
        private byte[] Serialize()
        {
            byte[]? ser = null;
            switch (_command)
            {
                case IcodeCardCommand.ReadSingleBlock:
                    // Flags(1 byte), Command code(1 byte), UID(8 byte), BlockNumber(1 byte)
                    ser = new byte[2 + 8 + 1];
                    ser[0] = AddressedModeFlags;
                    ser[1] = (byte)_command;
                    ser[10] = BlockNumber;
                    Uid?.CopyTo(ser, 2);
                    _responseSize = 5; // flags(1) + data(4)
                    return ser;

                case IcodeCardCommand.WriteSingleBlock:
                    // Flags(1 byte), Command code(1 byte), UID(8 byte), BlockNumber(1 byte), Data(4 byte)
                    ser = new byte[2 + 8 + 1 + 4];
                    ser[0] = AddressedModeFlags;
                    ser[1] = (byte)_command;
                    ser[10] = BlockNumber;
                    Uid?.CopyTo(ser, 2);
                    Data.CopyTo(ser, 11);
                    _responseSize = 2; // flags(1) + potential error code(1)
                    return ser;

                case IcodeCardCommand.LockBlock:
                    // Flags(1 byte), Command code(1 byte), UID(8 byte), BlockNumber(1 byte)
                    ser = new byte[2 + 8 + 1];
                    ser[0] = AddressedModeFlags;
                    ser[1] = (byte)_command;
                    ser[10] = BlockNumber;
                    Uid?.CopyTo(ser, 2);
                    _responseSize = 2;
                    return ser;

                case IcodeCardCommand.ReadMultipleBlocks:
                    // Flags(1 byte), Command code(1 byte), UID(8 byte), FirstBlockNumber(1 byte), NumBlocks(1 byte, encoded as N-1)
                    ser = new byte[2 + 8 + 2];
                    ser[0] = AddressedModeFlags;
                    ser[1] = (byte)_command;
                    ser[10] = BlockNumber;
                    // ISO/IEC 15693 encodes "number of blocks" as (N - 1), where N is the number of blocks to read
                    ser[11] = (byte)(BlockCount - 1);
                    Uid?.CopyTo(ser, 2);
                    _responseSize = (ushort)(1 + (BlockCount * BytesPerBlock)); // flags(1) + blocks
                    return ser;

                case IcodeCardCommand.WriteMultipleBlocks:
                    // Flags(1 byte), Command code(1 byte), UID(8 byte), FirstBlockNumber(1 byte), NumBlocks(1 byte), Data
                    if (Data is null)
                    {
                        throw new ArgumentNullException(nameof(Data));
                    }

                    if (Data.Length == 0 || (Data.Length % BytesPerBlock) != 0)
                    {
                        throw new ArgumentException("Data length must be a positive multiple of the block size.", nameof(Data));
                    }

                    int blockCount = Data.Length / BytesPerBlock;

                    ser = new byte[2 + 8 + 2 + Data.Length];
                    ser[0] = AddressedModeFlags;
                    ser[1] = (byte)_command;
                    ser[10] = BlockNumber;
                    // ISO/IEC 15693 WriteMultipleBlocks encodes number of blocks as (N - 1)
                    ser[11] = (byte)(blockCount - 1);
                    Uid?.CopyTo(ser, 2);
                    Data.CopyTo(ser, 12);
                    _responseSize = 2;
                    return ser;

                case IcodeCardCommand.StayQuiet:
                case IcodeCardCommand.Select:
                case IcodeCardCommand.ResetToReady:
                case IcodeCardCommand.LockAfi:
                case IcodeCardCommand.LockDsfid:
                    // Flags(1 byte), Command code(1 byte), UID(8 byte)
                    ser = new byte[2 + 8];
                    ser[0] = AddressedModeFlags;
                    ser[1] = (byte)_command;
                    Uid?.CopyTo(ser, 2);
                    _responseSize = 2;
                    return ser;

                case IcodeCardCommand.GetSystemInformation:
                    // Flags(1 byte), Command code(1 byte), UID(8 byte)
                    ser = new byte[2 + 8];
                    ser[0] = AddressedModeFlags;
                    ser[1] = (byte)_command;
                    Uid?.CopyTo(ser, 2);
                    _responseSize = 15; // flags(1) + info flags(1) + UID(8) + DSFID(1) + AFI(1) + memory(2) + IC ref(1)
                    return ser;

                case IcodeCardCommand.WriteAfi:
                    // Flags(1 byte), Command code(1 byte), UID(8 byte), AFI(1 byte)
                    ser = new byte[2 + 8 + 1];
                    ser[0] = AddressedModeFlags;
                    ser[1] = (byte)_command;
                    ser[10] = Afi;
                    Uid?.CopyTo(ser, 2);
                    _responseSize = 2;
                    return ser;

                case IcodeCardCommand.WriteDsfid:
                    // Flags(1 byte), Command code(1 byte), UID(8 byte), DSFID(1 byte)
                    ser = new byte[2 + 8 + 1];
                    ser[0] = AddressedModeFlags;
                    ser[1] = (byte)_command;
                    ser[10] = Dsfid;
                    Uid?.CopyTo(ser, 2);
                    _responseSize = 2;
                    return ser;

                case IcodeCardCommand.GetMultipleBlockSecurityStatus:
                    // Flags(1 byte), Command code(1 byte), UID(8 byte), FirstBlockNumber(1 byte), NumBlocks(1 byte)
                    ser = new byte[2 + 8 + 2];
                    ser[0] = AddressedModeFlags;
                    ser[1] = (byte)_command;
                    ser[10] = BlockNumber;
                    ser[11] = BlockCount;
                    Uid?.CopyTo(ser, 2);
                    _responseSize = (ushort)(1 + BlockCount + 1); // flags(1) + security status bytes
                    return ser;

                default:
                    return new byte[0];
            }
        }

        /// <summary>
        /// Read a single 4-byte block and place the result into the <see cref="Data"/> property.
        /// </summary>
        /// <param name="block">The block number to read.</param>
        /// <returns>True if success. This only means whether the communication between VCD and VICC is successful or not.</returns>
        public bool ReadSingleBlock(byte block)
        {
            BlockNumber = block;
            _command = IcodeCardCommand.ReadSingleBlock;
            var ret = RunIcodeCardCommand();
            return ret >= 0;
        }

        /// <summary>
        /// Write a single 4-byte block using the data present in the <see cref="Data"/> property.
        /// </summary>
        /// <param name="block">The block number to write.</param>
        /// <returns>True if success. This only means whether the communication between VCD and VICC is successful or not.</returns>
        public bool WriteSingleBlock(byte block)
        {
            if (Data.Length < 1 || Data.Length > BytesPerBlock)
            {
#if DEBUG
                _logger.LogDebug("Length of data must be larger than zero and less than or equal to four.");
#endif
                return false;
            }

            BlockNumber = block;
            _command = IcodeCardCommand.WriteSingleBlock;
            var ret = RunIcodeCardCommand();
            return ret >= 0;
        }

        /// <summary>
        /// Permanently lock a specific block. This operation is irreversible.
        /// </summary>
        /// <param name="block">The block number to lock.</param>
        /// <returns>True if success. This only means whether the communication between VCD and VICC is successful or not.</returns>
        public bool LockBlock(byte block)
        {
            BlockNumber = block;
            _command = IcodeCardCommand.LockBlock;
            var ret = RunIcodeCardCommand();
            return ret >= 0;
        }

        /// <summary>
        /// Read multiple contiguous blocks and place the result into the <see cref="Data"/> property.
        /// </summary>
        /// <param name="block">The starting block number to read.</param>
        /// <param name="count">Total block count to read (0-based: 0 means 1 block, 1 means 2 blocks, etc.).</param>
        /// <returns>True if success. This only means whether the communication between VCD and VICC is successful or not.</returns>
        public bool ReadMultipleBlocks(byte block, byte count)
        {
            BlockNumber = block;
            BlockCount = count;
            _command = IcodeCardCommand.ReadMultipleBlocks;
            var ret = RunIcodeCardCommand();
            return ret >= 0;
        }

        /// <summary>
        /// Write multiple contiguous blocks using the data present in the <see cref="Data"/> property.
        /// Data length must be a multiple of 4 (bytes per block).
        /// </summary>
        /// <param name="block">The starting block number to write.</param>
        /// <returns>True if success. This only means whether the communication between VCD and VICC is successful or not.</returns>
        public bool WriteMultipleBlocks(byte block)
        {
            if (Data.Length < 1 || (Data.Length % BytesPerBlock) != 0)
            {
#if DEBUG
                _logger.LogDebug("Length of data must be larger than zero and a multiple of 4 bytes (block size).");
#endif
                return false;
            }

            BlockNumber = block;
            _command = IcodeCardCommand.WriteMultipleBlocks;
            var ret = RunIcodeCardCommand();
            return ret >= 0;
        }

        /// <summary>
        /// Set the VICC to quiet state. The card will not respond to inventory requests
        /// until it is powered off and on again.
        /// </summary>
        /// <returns>True if success. This only means whether the communication between VCD and VICC is successful or not.</returns>
        public bool StayQuiet()
        {
            if (Uid == null || Uid.Length != 8)
            {
#if DEBUG
                _logger.LogDebug("Uid is null or is invalid!");
#endif
                return false;
            }

            _command = IcodeCardCommand.StayQuiet;
            var ret = RunIcodeCardCommand();
            return ret >= 0;
        }

        /// <summary>
        /// Select a specific card by UID. After selection, the card enters selected state
        /// and can be communicated with using non-addressed mode.
        /// </summary>
        /// <param name="uid">The 8-byte UID of the card to select.</param>
        /// <returns>True if success. This only means whether the communication between VCD and VICC is successful or not.</returns>
        public bool Select(byte[] uid)
        {
            if (uid == null || uid.Length != 8)
            {
#if DEBUG
                _logger.LogDebug("Uid is null or is invalid!");
#endif
                return false;
            }

            Uid = uid;
            _command = IcodeCardCommand.Select;
            var ret = RunIcodeCardCommand();
            return ret >= 0;
        }

        /// <summary>
        /// Return the VICC to ready state. This reverses the effect of <see cref="StayQuiet"/> or <see cref="Select"/>.
        /// </summary>
        /// <returns>True if success. This only means whether the communication between VCD and VICC is successful or not.</returns>
        public bool ResetToReady()
        {
            _command = IcodeCardCommand.ResetToReady;
            var ret = RunIcodeCardCommand();
            return ret >= 0;
        }

        /// <summary>
        /// Write the Application Family Identifier. The value in <see cref="Afi"/> will be written.
        /// </summary>
        /// <returns>True if success. This only means whether the communication between VCD and VICC is successful or not.</returns>
        public bool WriteAfi()
        {
            _command = IcodeCardCommand.WriteAfi;
            var ret = RunIcodeCardCommand();
            return ret >= 0;
        }

        /// <summary>
        /// Permanently lock the AFI. This operation is irreversible.
        /// </summary>
        /// <returns>True if success. This only means whether the communication between VCD and VICC is successful or not.</returns>
        public bool LockAfi()
        {
            _command = IcodeCardCommand.LockAfi;
            var ret = RunIcodeCardCommand();
            return ret >= 0;
        }

        /// <summary>
        /// Write the Data Storage Format Identifier. The value in <see cref="Dsfid"/> will be written.
        /// </summary>
        /// <returns>True if success. This only means whether the communication between VCD and VICC is successful or not.</returns>
        public bool WriteDsfid()
        {
            _command = IcodeCardCommand.WriteDsfid;
            var ret = RunIcodeCardCommand();
            return ret >= 0;
        }

        /// <summary>
        /// Permanently lock the DSFID. This operation is irreversible.
        /// </summary>
        /// <returns>True if success. This only means whether the communication between VCD and VICC is successful or not.</returns>
        public bool LockDsfid()
        {
            _command = IcodeCardCommand.LockDsfid;
            var ret = RunIcodeCardCommand();
            return ret >= 0;
        }

        /// <summary>
        /// Get the system information of the VICC, including DSFID, AFI, memory capacity, and IC reference.
        /// The raw response is placed into the <see cref="Data"/> property.
        /// </summary>
        /// <returns>True if success. This only means whether the communication between VCD and VICC is successful or not.</returns>
        public bool GetSystemInformation()
        {
            _command = IcodeCardCommand.GetSystemInformation;
            var ret = RunIcodeCardCommand();
            return ret >= 0;
        }

        /// <summary>
        /// Get the security status (locked/unlocked) of multiple blocks.
        /// The raw response is placed into the <see cref="Data"/> property.
        /// </summary>
        /// <param name="block">The starting block number.</param>
        /// <param name="count">The number of blocks to query (0-based).</param>
        /// <returns>True if success. This only means whether the communication between VCD and VICC is successful or not.</returns>
        public bool GetMultipleBlockSecurityStatus(byte block, byte count)
        {
            BlockNumber = block;
            BlockCount = count;
            _command = IcodeCardCommand.GetMultipleBlockSecurityStatus;
            var ret = RunIcodeCardCommand();
            return ret >= 0;
        }
    }
}
