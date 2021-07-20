// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using System.Collections;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.Spi;
using System.Diagnostics;
using System.IO;
using System.Threading;

using Iot.Device.Card;
using Iot.Device.Card.Mifare;
using Iot.Device.Rfid;
#if DEBUG
using Microsoft.Extensions.Logging;
using nanoFramework.Logging;
#endif

namespace Iot.Device.Pn5180
{
    /// <summary>
    /// A PN5180 class offering RFID and NFC functionalities. Implement the CardTransceiver class to
    /// allow Mifare, Credit Card support
    /// </summary>
    public class Pn5180 : CardTransceiver, IDisposable
    {
        private const int TimeoutWaitingMilliseconds = 2_000;

        private static ListSelectedPiccInformation _activeSelected = new ListSelectedPiccInformation();

        private readonly SpiDevice _spiDevice;
        private readonly GpioController _gpioController;
        private bool _shouldDispose;
        private int _pinBusy;
        private int _pinNss;
#if DEBUG
        private ILogger _logger;
#endif

        /// <summary>
        /// A radio Frequency configuration element size is 5 bytes
        /// Byte 1 = Register Address
        /// next 4 bytes = data of the register
        /// </summary>
        public const int RadioFrequencyConfigurationSize = 5;

        /// <summary>
        /// PN532 SPI Clock Frequency
        /// </summary>
        public const int MaximumSpiClockFrequency = 7_000_000;

        /// <summary>
        /// Only SPI Mode supported is Mode0
        /// </summary>
        public const SpiMode DefaultSpiMode = System.Device.Spi.SpiMode.Mode0;

        /// <summary>
        /// Create a PN5180 RFID/NFC reader
        /// </summary>
        /// <param name="spiDevice">The SPI device</param>
        /// <param name="pinBusy">The pin for the busy line</param>
        /// <param name="pinNss">The pin for the SPI select line. This has to be handle differently than thru the normal process as PN5180 has a specific way of working</param>
        /// <param name="gpioController">A GPIO controller, null will use a default one</param>
        /// <param name="shouldDispose">Dispose the SPI and the GPIO controller at the end if true</param>
        public Pn5180(SpiDevice spiDevice, int pinBusy, int pinNss, GpioController? gpioController = null, bool shouldDispose = true)
        {
            if (pinBusy < 0)
            {
                throw new ArgumentException(nameof(pinBusy), "Value must be a legal pin number. cannot be negative.");
            }

            if (pinNss < 0)
            {
                throw new ArgumentException(nameof(pinBusy), "Value must be a legal pin number. cannot be negative.");
            }

#if DEBUG
            _logger = this.GetCurrentClassLogger();
            _logger.LogDebug($"Opening PN5180, pin busy: {pinBusy}, pin NSS: {pinNss}");
#endif
            _spiDevice = spiDevice ?? throw new ArgumentNullException(nameof(spiDevice));
            _gpioController = gpioController ?? new GpioController(PinNumberingScheme.Logical);
            _shouldDispose = shouldDispose || gpioController is null;
            _pinBusy = pinBusy;
            _pinNss = pinNss;
            _gpioController.OpenPin(_pinBusy, PinMode.Input);
            _gpioController.OpenPin(_pinNss, PinMode.Output);

            // Check the version
            var versions = GetVersions();
            if ((versions.Product == null) || (versions.Product.Major == 0) || (versions.Firmware.Major == 0) || (versions.Eeprom.Major == 0))
            {
                throw new IOException($"Not a valid PN5180");
            }
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            SetRadioFrequency(false);

            if (_shouldDispose)
            {
                _spiDevice?.Dispose();
                _gpioController?.Dispose();
            }
        }

        #region EEPROM

        /// <summary>
        /// Get the Product, Firmware and EEPROM versions of the PN8150
        /// </summary>
        /// <returns>A tuple with the Product, Firmware and EEPROM versions</returns>
        public TripletVersion GetVersions()
        {
            SpanByte versionAnswer = new byte[6];

            var ret = ReadEeprom(EepromAddress.ProductVersion, versionAnswer);
            if (!ret)
            {
                return new TripletVersion(null, null, null);
            }

            var product = new Version(versionAnswer[1], versionAnswer[0]);
            var firmware = new Version(versionAnswer[3], versionAnswer[2]);
            var eeprom = new Version(versionAnswer[5], versionAnswer[4]);
            return new TripletVersion(product, firmware, eeprom);
        }

        /// <summary>
        /// Get the PN5180 identifier, this is a 16 byte long
        /// </summary>
        /// <param name="outputIdentifier">A 16 byte buffer</param>
        /// <returns>True if success</returns>
        public bool GetIdentifier(SpanByte outputIdentifier)
        {
            if (outputIdentifier.Length != 16)
            {
                throw new ArgumentException(nameof(outputIdentifier), "Value must be 16 bytes long");
            }

            return ReadEeprom(EepromAddress.DieIdentifier, outputIdentifier);
        }

        /// <summary>
        /// Read the full EEPROM
        /// </summary>
        /// <param name="eeprom">At 255 bytes buffer</param>
        /// <returns>True if success</returns>
        public bool ReadAllEeprom(SpanByte eeprom)
        {
            if (eeprom.Length != 255)
            {
                throw new ArgumentException(nameof(eeprom), "Size of EEPROM is 255 bytes. Value must match.");
            }

            return ReadEeprom(EepromAddress.DieIdentifier, eeprom);
        }

        /// <summary>
        /// Write all the EEPROM
        /// </summary>
        /// <param name="eeprom">A 255 bytes buffer</param>
        /// <returns>True if success</returns>
        public bool WriteAllEeprom(SpanByte eeprom)
        {
            if (eeprom.Length != 255)
            {
                throw new ArgumentException(nameof(eeprom), "Size of EEPROM is 255 bytes. Value must match.");
            }

            return WriteEeprom(EepromAddress.DieIdentifier, eeprom);
        }

        /// <summary>
        /// Read a specific part of the EEPROM
        /// </summary>
        /// <param name="address">The EEPROM address</param>
        /// <param name="eeprom">A span of byte to read the EEPROM</param>
        /// <returns>True if success</returns>
        public bool ReadEeprom(EepromAddress address, SpanByte eeprom)
        {
            if ((byte)address + eeprom.Length > 255)
            {
                throw new ArgumentException(nameof(eeprom), "Size of EEPROM is 255 bytes. Value must 255 bytes or less.");
            }

            SpanByte dumpEeprom = new byte[3];
            dumpEeprom[0] = (byte)Command.READ_EEPROM;
            dumpEeprom[1] = (byte)address;
            dumpEeprom[2] = (byte)eeprom.Length;
#if DEBUG
            _logger.LogDebug($"{nameof(ReadEeprom)}, {nameof(dumpEeprom)}: {BitConverter.ToString(dumpEeprom.ToArray())}");
#endif
            try
            {
                SpiWriteRead(dumpEeprom, eeprom);
#if DEBUG
                _logger.LogDebug($"{nameof(ReadEeprom)}, {nameof(dumpEeprom)}: {BitConverter.ToString(dumpEeprom.ToArray())}");
#endif
            }
            catch (TimeoutException tx)
            {
#if DEBUG
                _logger.LogError(tx, $"{nameof(ReadEeprom)}: {nameof(TimeoutException)} during {nameof(SpiWriteRead)}");
#endif
                return false;
            }

            return true;
        }

        /// <summary>
        /// Write the EEPROM at a specific address
        /// </summary>
        /// <param name="address">The EEPROM address</param>
        /// <param name="eeprom">A span of byte to write the EEPROM</param>
        /// <returns>True if success</returns>
        public bool WriteEeprom(EepromAddress address, SpanByte eeprom)
        {
            if ((byte)address + eeprom.Length > 255)
            {
                throw new ArgumentException(nameof(eeprom), "Size of EEPROM is 255 bytes. Value must 255 bytes or less.");
            }

            SpanByte dumpEeprom = new byte[2 + eeprom.Length];
            bool ret;
            dumpEeprom[0] = (byte)Command.WRITE_EEPROM;
            dumpEeprom[1] = (byte)address;
            eeprom.CopyTo(dumpEeprom.Slice(2));
#if DEBUG
            _logger.LogDebug($"{nameof(WriteEeprom)}, {nameof(eeprom)}: {BitConverter.ToString(eeprom.ToArray())}");
#endif

            try
            {
                SpiWrite(dumpEeprom);
#if DEBUG
                _logger.LogDebug($"{nameof(WriteEeprom)}, {nameof(dumpEeprom)}: {BitConverter.ToString(dumpEeprom.ToArray())}");
#endif

                SpanByte irqStatus = new byte[4];
                ret = GetIrqStatus(irqStatus);
                ret &= !((irqStatus[2] & 0b0000_0010) == 0b0000_0010);
                // Clear IRQ
                SpiWriteRegister(Command.WRITE_REGISTER, Register.IRQ_CLEAR, new byte[] { 0xFF, 0xFF, 0x0F, 0x00 });
            }
            catch (TimeoutException tx)
            {
#if DEBUG
                _logger.LogError(tx, $"{nameof(WriteEeprom)}: {nameof(TimeoutException)} during {nameof(SpiWrite)}");
#endif
                return false;
            }

            return ret;
        }

#endregion

#region SEND and READ data from card

        /// <summary>
        /// Send data to a card.
        /// </summary>
        /// <param name="toSend">The span of byte to send</param>
        /// <param name="numberValidBitsLastByte">The number of bits valid in the last byte, 8 is the default.
        /// If validBits == 3 then it's equivalent to apply a mask of 0b000_0111 to get the correct valid bits</param>
        /// <returns>True if success</returns>
        /// <remarks>Using this function you'll have to manage yourself the possible low level communication protocol.
        /// This function write directly to the card all the bytes. Please make sure you'll first load specific radio frequence settings,
        /// detect a card, select it and then send data</remarks>
        public bool SendDataToCard(SpanByte toSend, int numberValidBitsLastByte = 8)
        {
            if (toSend.Length > 260)
            {
                throw new ArgumentException(nameof(toSend), "Data to send can't be larger than 260 bytes");
            }

            if ((numberValidBitsLastByte < 1) || (numberValidBitsLastByte > 8))
            {
                throw new ArgumentException(nameof(numberValidBitsLastByte), "Number of valid bits in last byte can only be between 1 and 8");
            }

            SpanByte sendData = new byte[2 + toSend.Length];
            sendData[0] = (byte)Command.SEND_DATA;
            sendData[1] = (byte)(numberValidBitsLastByte == 8 ? 0 : numberValidBitsLastByte);
            toSend.CopyTo(sendData.Slice(2));
#if DEBUG
            _logger.LogDebug($"{nameof(SendDataToCard)}: {nameof(sendData)}, {BitConverter.ToString(sendData.ToArray())}");
#endif

            try
            {
                SpiWrite(sendData);
            }
            catch (TimeoutException tx)
            {
#if DEBUG
                _logger.LogError(tx, $"{nameof(SendDataToCard)}: {nameof(TimeoutException)} in {nameof(SpiWrite)}");
#endif
                return false;
            }

            return true;
        }

        /// <summary>
        /// Read data from a card.
        /// </summary>
        /// <param name="toRead">The span of byte to read</param>
        /// <returns>True if success</returns>
        /// <remarks>Using this function you'll have to manage yourself the possible low level communication protocol.
        /// This function write directly to the card all the bytes. Please make sure you'll first load specific radio frequence settings,
        /// detect a card, select it and then send data</remarks>
        public bool ReadDataFromCard(SpanByte toRead)
        {
            if (toRead.Length > 508)
            {
                throw new ArgumentException(nameof(toRead), "Data to read can't be larger than 508 bytes");
            }

            SpanByte sendData = new byte[2];
            sendData[0] = (byte)Command.READ_DATA;
            sendData[1] = 0x00;
#if DEBUG
            _logger.LogDebug($"{nameof(ReadDataFromCard)}: {nameof(sendData)}, {BitConverter.ToString(sendData.ToArray())}");
#endif

            try
            {
                SpiWriteRead(sendData, toRead);
#if DEBUG
                _logger.LogDebug($"{nameof(ReadDataFromCard)}: {nameof(toRead)}, {BitConverter.ToString(toRead.ToArray())}");
#endif
            }
            catch (TimeoutException tx)
            {
#if DEBUG
                _logger.LogError(tx, $"{nameof(ReadDataFromCard)}: {nameof(TimeoutException)} in {nameof(SpiWriteRead)}");
#endif
                return false;
            }

            return true;
        }

        /// <summary>
        /// Read data from a card.
        /// </summary>
        /// <param name="toRead">>The span of byte to read</param>
        /// <param name="expectedToRead">The expected number of bytes to read</param>
        /// <returns>True if success. Will return false if the number of bytes to read is not the same as the expected number to read</returns>
        /// <remarks>Using this function you'll have to manage yourself the possible low level communication protocol.
        /// This function write directly to the card all the bytes. Please make sure you'll first load specific radio frequence settings,
        /// detect a card, select it and then send data</remarks>
        private bool ReadDataFromCard(SpanByte toRead, int expectedToRead)
        {
            var ret = GetNumberOfBytesReceivedAndValidBits();
            int numBytes = ret.Bytes;
            if (numBytes == expectedToRead)
            {
#if DEBUG
                _logger.LogDebug($"{nameof(ReadDataFromCard)}: right number of expected bytes to read");
#endif

                return ReadDataFromCard(toRead);
            }
            else if (numBytes > expectedToRead)
            {
#if DEBUG
                _logger.LogDebug($"{nameof(ReadDataFromCard)}: wrong number of expected bytes, clearing the cache");
#endif

                // Clear all
                ReadDataFromCard(new byte[numBytes]);
            }

            return false;
        }

        /// <summary>
        /// Read all the data from the card
        /// </summary>
        /// <param name="toRead">>The span of byte to read</param>
        /// <param name="bytesRead">number of bytes read</param>
        /// <returns>A byte array with all the read elements, null if nothing can be read</returns>
        /// <remarks>Using this function you'll have to manage yourself the possible low level communication protocol.
        /// This function write directly to the card all the bytes. Please make sure you'll first load specific radio frequence settings,
        /// detect a card, select it and then send data</remarks>
        public bool ReadDataFromCard(SpanByte toRead, out int bytesRead)
        {
#if DEBUG
            _logger.LogDebug($"{nameof(ReadDataFromCard)}: ");
#endif
            var num = GetNumberOfBytesReceivedAndValidBits();
            int numBytes = num.Bytes;
            if (numBytes < 0)
            {
                bytesRead = 0;
                return false;
            }

            var ret = ReadDataFromCard(toRead);
            if (ret)
            {
                bytesRead = numBytes;
                return true;
            }

            bytesRead = 0;
            return false;
        }

        /// <summary>
        /// Get the number of bytes to read and the valid number of bits in the last byte
        /// If the full byte is valid then the value of the valid bit is 0
        /// </summary>
        /// <returns>A tuple whit the number of bytes to read and the number of valid bits in the last byte. If all bits are valid, then the value of valid bits is 0</returns>
        public Doublet GetNumberOfBytesReceivedAndValidBits()
        {
            try
            {
                SpanByte status = new byte[4];
                SpiReadRegister(Register.RX_STATUS, status);
                // from NXP documentation PN5180AXX-C3.pdf, Page 98
                return new Doublet((status[0] + ((status[1] & 0x01) << 8)), (status[1] & 0b1110_0000) >> 5);
            }
            catch (TimeoutException tx)
            {
#if DEBUG
                _logger.LogError(tx, $"{nameof(SendDataToCard)}: {nameof(TimeoutException)} in {nameof(SpiReadRegister)}");
#endif
                return new Doublet(-1, -1);
            }
        }

        /// <inheritdoc/>
        public override int Transceive(byte targetNumber, SpanByte dataToSend, SpanByte dataFromCard)
        {
            // Check if we have a Mifare Card authentication request
            // Only valid for Type A card so with a target number equal to 0
            if (((targetNumber == 0) && ((dataToSend[0] == (byte)MifareCardCommand.AuthenticationA) || (dataToSend[0] == (byte)MifareCardCommand.AuthenticationB))) && (dataFromCard.Length == 0))
            {
                var ret = MifareAuthenticate(dataToSend.Slice(2, 6).ToArray(), (MifareCardCommand)dataToSend[0], dataToSend[1], dataToSend.Slice(8).ToArray());
                return ret ? 0 : -1;
            }
            else
            {
                return TransceiveClassic(targetNumber, dataToSend, dataFromCard);
            }
        }

        /// <inheritdoc/>
        public override bool ReselectTarget(byte targetNumber)
        {
            if (targetNumber == 0)
            {
                // TODO: this should be implemented this for Type A card for this reader
                // This will need to send WUPA (0x52 coded on 7 bits), Anti-collision and select loops like for initial detection
                // We don't throw an exception, this is just telling that the selection failed
                return false;
            }
            else
            {
                SelectedPiccInformation card = null;
                for (int i = 0; i < _activeSelected.Count; i++)
                {
                    if (_activeSelected[i].Card.TargetNumber == targetNumber)
                    {
                        card = _activeSelected[i];
                        break;
                    }
                }

                if (card is null)
                {
                    return false;
                }

                DeselectCardTypeB(card.Card);
                // Deselect may fail but if selection succeed it's ok
                var ret = SelectCardTypeB(card.Card);
                return ret;
            }
        }

        private int TransceiveClassic(byte targetNumber, SpanByte dataToSend, SpanByte dataFromCard)
        {
            // type B card have a tag number which is always more than 1
            if (targetNumber == 0)
            {
                // Case of a type A card
                return TransceiveBuffer(dataToSend, dataFromCard);
            }
            else
            {
                const int MaxTries = 5;
                // All the type B protocol 14443-4 is from the ISO14443-4.pdf from ISO website
                // Original code: var card = _activeSelected.Where(m => m.Card.TargetNumber == targetNumber).FirstOrDefault();
                SelectedPiccInformation card = null;
                for (int i = 0; i < _activeSelected.Count; i++)
                {
                    if (_activeSelected[i].Card.TargetNumber == targetNumber)
                    {
                        card = _activeSelected[i];
                        break;
                    }
                }

                if (card is null)
                {
                    throw new ArgumentException(nameof(targetNumber), $"Device with target number {targetNumber} is not part of the list of selected devices. Card may have been removed.");
                }

                SpanByte toSend = new byte[dataToSend.Length + 2];
                SpanByte toReceive = new byte[dataFromCard.Length + 2];
                int maxTries = 0;
                int maxTriesTimeout = 0;

                // I-BLOCK command
                // bit 8, 7, 6 to 0, bit 5 is chaining but we are not supporting it, so 0,
                // bit 4 is CID and should be set to 1 as we have a device selected
                // Bit 3 is NAD but we are not adding any prologue field
                // bit 2 to 1
                // bit 1 is tracking block
                toSend[0] = (byte)(card.LastBlockMark ? 0b0000_1011 : 0b0000_1010);

                // Second byte it the number (CID)
                toSend[1] = targetNumber;
                dataToSend.CopyTo(toSend.Slice(2));
                var numBytes = TransceiveTypeB(card, toSend, toReceive);

            RetryBlock:
                if (numBytes == 0)
                {
                    // That means timeout so sending an non acknowledged
                    SpanByte rBlock = new byte[2] { (byte)(0b1010_1010 | (card.LastBlockMark ? 1 : 0)), targetNumber };
                    var ret = SendDataToCard(rBlock);
                    if (!ret)
                    {
                        return -1;
                    }

                    if (maxTriesTimeout++ > MaxTries)
                    {
                        return -1;
                    }

                    numBytes = ReadWithTimeout(dataFromCard, (int)(toReceive[2] * card.Card.FrameWaitingTime / 1000));
                    goto RetryBlock;
                }

                if (numBytes < 0)
                {
                    return -1;
                }

            IBlock:
                if (numBytes >= 2)
                {
                    // If not the right target, then not for us
                    if (toReceive[1] != targetNumber)
                    {
                        return -1;
                    }

                    // Is it a valid packet?
                    if (toReceive[0] == (0b0000_1010 | (card.LastBlockMark ? 1 : 0)))
                    {
                        toReceive.Slice(2).CopyTo(dataFromCard);
                        card.LastBlockMark = !card.LastBlockMark;
                        return numBytes - 2;
                    }

                    // Case of a chained packet, we need to send acknowledgment and read more data
                    if (toReceive[0] == (0b0001_1010 | (card.LastBlockMark ? 1 : 0)))
                    {
                        // Copy what we have already
                        toReceive.Slice(2, numBytes - 2).CopyTo(dataFromCard);
                        // Send Ack with the right mark
                        card.LastBlockMark = !card.LastBlockMark;
                        SpanByte rBlockChain = new byte[2] { (byte)(0b1010_1010 | (card.LastBlockMark ? 1 : 0)), targetNumber };
                        var numBytes2 = TransceiveTypeB(card, rBlockChain, toReceive);

                        if (numBytes2 >= 2)
                        {
                            // If not the right target, then not for us
                            if ((toReceive[1] & 0x0F) != targetNumber)
                            {
                                return -1;
                            }

                            if (toReceive[0] != (0b0000_1010 | (card.LastBlockMark ? 1 : 0)))
                            {
                                return -1;
                            }

                            toReceive.Slice(2, numBytes2 - 2).CopyTo(dataFromCard.Slice(numBytes - 2));
                            card.LastBlockMark = !card.LastBlockMark;
                            return numBytes - 2 + numBytes2 - 2;
                        }
                    }

                    // Is it an extension time block request block?
                    if (toReceive[0] == 0b1111_1010)
                    {
                        // Agree and send the same
                        SpanByte sBlock = new byte[] { 0b1111_1010, card.Card.TargetNumber, toReceive[2] };
                        var ret = SendDataToCard(sBlock);
                        if (maxTries++ == MaxTries)
                        {
                            return -1;
                        }

                        numBytes = ReadWithTimeout(dataFromCard, (int)(toReceive[2] * card.Card.FrameWaitingTime / 1000));
                        goto IBlock;
                    }
                }

                return numBytes;
            }
        }

        private int TransceiveTypeB(SelectedPiccInformation card, SpanByte dataToSend, SpanByte dataFromCard)
        {
            var ret = SendDataToCard(dataToSend.ToArray());
            if (!ret)
            {
                return -1;
            }

            return ReadWithTimeout(dataFromCard, (int)(card.Card.FrameWaitingTime / 1000));
        }

        private int ReadWithTimeout(SpanByte dataFromCard, int timeoutMilliseconds)
        {
            int numBytes = 0;
            int numBytesPrevious = 0;
            Doublet num;

            // 10 etu needed for 1 byte, 1 etu = 9.4 µs, so about 100 µs are needed to transfer 1 character
            DateTime dtTimeout = DateTime.UtcNow.AddMilliseconds(timeoutMilliseconds);
            do
            {
                num = GetNumberOfBytesReceivedAndValidBits();
                numBytes = num.Bytes;
                // Do we have bytes?
                if (numBytes > 0)
                {
                    // Do we have read the same?
                    if (numBytes == numBytesPrevious)
                    {
                        break;
                    }

                    numBytesPrevious = numBytes;
                }

                // Wait to see if more characters are transmitted
                // 10 etu needed for 1 byte, 1 etu = 9.4 µs, so about 100 µs are needed to transfer 1 character
                // So waiting for 1 milliseconds will allow to make sure once done, all characters are
                // transmitted between 2 reads
                Thread.Sleep(1);
            }
            while (dtTimeout > DateTime.UtcNow);

            if (numBytes > 0)
            {
                var ret = ReadDataFromCard(dataFromCard.Slice(0, numBytes));
                if (!ret)
                {
                    return -1;
                }
            }

            return numBytes;
        }

        private int TransceiveBuffer(SpanByte dataToSend, SpanByte dataFromCard)
        {
            var ret = SendDataToCard(dataToSend.ToArray());
            if (!ret)
            {
                return -1;
            }

            // 10 etu needed for 1 byte, 1 etu = 9.4 µs, so about 100 µs are needed to transfer 1 character
            return ReadWithTimeout(dataFromCard, dataFromCard.Length / 100);
        }

        private bool SendRBlock(byte targetNumber, RBlock ack, int blockNumber)
        {
            if (!((blockNumber == 1) || (blockNumber == 0)))
            {
                throw new ArgumentException(nameof(blockNumber), "Value can be only 0 or 1.");
            }

            SpanByte rBlock = new byte[2] { (byte)(0b1010_1010 | (byte)ack | blockNumber), targetNumber };
            var ret = SendDataToCard(rBlock);
            var num = GetNumberOfBytesReceivedAndValidBits();
            int numBytes = num.Bytes;
            if (numBytes == 2)
            {
                ret = ReadDataFromCard(rBlock, numBytes);
                return ret;
            }

            return false;
        }

#endregion

#region Mifare specific

        /// <summary>
        /// Specific function to authenticate Mifare cards
        /// </summary>
        /// <param name="key">A 6 bytes key</param>
        /// <param name="mifareCommand">MifareCardCommand.AuthenticationA or MifareCardCommand.AuthenticationB</param>
        /// <param name="blockAddress">The block address to authenticate</param>
        /// <param name="cardUid">The 4 bytes UUID of the card</param>
        /// <returns>True if success</returns>
        public bool MifareAuthenticate(SpanByte key, MifareCardCommand mifareCommand, byte blockAddress, SpanByte cardUid)
        {
#if DEBUG
            _logger.LogDebug($"{nameof(MifareAuthenticate)}: ");
#endif
            if (key.Length != 6)
            {
                throw new ArgumentException(nameof(key), "Value must be 6 bytes.");
            }

            if (cardUid.Length != 4)
            {
                throw new ArgumentException(nameof(cardUid), "Value must be 4 bytes.");
            }

            if (!((mifareCommand == MifareCardCommand.AuthenticationA) || (mifareCommand == MifareCardCommand.AuthenticationB)))
            {
                throw new ArgumentException(nameof(mifareCommand), $"{nameof(MifareCardCommand.AuthenticationA)} and {nameof(MifareCardCommand.AuthenticationB)} are the only supported commands");
            }

            SpanByte toAuthenticate = new byte[13];
            SpanByte response = new byte[1];
            toAuthenticate[0] = (byte)Command.MIFARE_AUTHENTICATE;
            key.CopyTo(toAuthenticate.Slice(1));
            // Page 32 documentation PN5180A0XX-C3
            toAuthenticate[7] = (byte)(mifareCommand == MifareCardCommand.AuthenticationA ? 0x60 : 0x61);
            toAuthenticate[8] = blockAddress;
            cardUid.CopyTo(toAuthenticate.Slice(9));
#if DEBUG
            _logger.LogDebug($"{nameof(MifareAuthenticate)}: {nameof(toAuthenticate)}: {BitConverter.ToString(toAuthenticate.ToArray())}");
#endif

            try
            {
                SpiWriteRead(toAuthenticate, response);
#if DEBUG
                _logger.LogDebug($"{nameof(MifareAuthenticate)}: {nameof(response)}: {BitConverter.ToString(response.ToArray())}");
#endif
            }
            catch (TimeoutException tx)
            {
#if DEBUG
                _logger.LogError(tx, $"{nameof(ReadDataFromCard)}: {nameof(TimeoutException)} in {nameof(SpiWriteRead)}");
#endif
                return false;
            }

            // Success is 0
            return response[0] == 0;
        }

#endregion

#region RadioFrequency

        /// <summary>
        /// Load a specific radio frequency configuration
        /// </summary>
        /// <param name="transmitter">The transmitter configuration</param>
        /// <param name="receiver">The receiver configuration</param>
        /// <returns>True if success</returns>
        public bool LoadRadioFrequencyConfiguration(TransmitterRadioFrequencyConfiguration transmitter, ReceiverRadioFrequencyConfiguration receiver)
        {
            SpanByte rfConfig = new byte[3];
            rfConfig[0] = (byte)Command.LOAD_RF_CONFIG;
            rfConfig[1] = (byte)transmitter;
            rfConfig[2] = (byte)receiver;

            try
            {
                SpiWrite(rfConfig);
            }
            catch (TimeoutException)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Get the size of the configuration of a specific transmitter configuration
        /// </summary>
        /// <param name="transmitter">The transmitter configuration</param>
        /// <returns>True if success</returns>
        public int GetRadioFrequencyConfigSize(TransmitterRadioFrequencyConfiguration transmitter) => GetRadioFrequencyConfigSize((byte)transmitter);

        /// <summary>
        /// Get the size of the configuration of a specific receiver configuration
        /// </summary>
        /// <param name="receiver">The receiver configuration</param>
        /// <returns>True if success</returns>
        public int GetRadioFrequencyConfigSize(ReceiverRadioFrequencyConfiguration receiver) => GetRadioFrequencyConfigSize((byte)receiver);

        private int GetRadioFrequencyConfigSize(byte config)
        {
            SpanByte rfConfig = new byte[2];
            SpanByte response = new byte[1];
            rfConfig[0] = (byte)Command.RETRIEVE_RF_CONFIG_SIZE;
            rfConfig[1] = config;

            try
            {
                SpiWriteRead(rfConfig, response);
            }
            catch (TimeoutException)
            {
                return -1;
            }

            return response[0];
        }

        /// <summary>
        /// Retrieve the radio frequency configuration
        /// </summary>
        /// <param name="transmitter">The transmitter configuration</param>
        /// <param name="configuration">A span of bytes for the configuration. Should be a multiple of 5 with the size of <see ref="GetRadioFrequenceConfigSize"/></param>
        /// <returns>True if success</returns>
        public bool RetrieveRadioFrequencyConfiguration(TransmitterRadioFrequencyConfiguration transmitter, SpanByte configuration) => RetrieveRadioFrequencyConfiguration((byte)transmitter, configuration);

        /// <summary>
        /// Retrieve the radio frequency configuration
        /// </summary>
        /// <param name="receiver">The receiver configuration</param>
        /// <param name="configuration">A span of bytes for the configuration. Should be a multiple of 5 with the size of <see ref="GetRadioFrequenceConfigSize"/></param>
        /// <returns>True if success</returns>
        public bool RetrieveRadioFrequencyConfiguration(ReceiverRadioFrequencyConfiguration receiver, SpanByte configuration) => RetrieveRadioFrequencyConfiguration((byte)receiver, configuration);

        private bool RetrieveRadioFrequencyConfiguration(byte config, SpanByte configuration)
        {
            // Page 41 documentation PN5180A0XX-C3.pdf
            if ((configuration.Length > 195) || (configuration.Length % 5 != 0) || (configuration.Length == 0))
            {
                throw new ArgumentException(nameof(configuration), "Value must be a positive multiple of 5 and no larger than 195 bytes.");
            }

            SpanByte rfConfig = new byte[2];
            rfConfig[0] = (byte)Command.RETRIEVE_RF_CONFIG;
            rfConfig[1] = (byte)config;

            try
            {
                SpiWriteRead(rfConfig, configuration);
            }
            catch (TimeoutException)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Update the radio frequency configuration
        /// </summary>
        /// <param name="transmitter">The transmitter configuration</param>
        /// <param name="configuration">A span of bytes for the configuration. Should be a multiple of 5 with the size of <see ref="GetRadioFrequenceConfigSize"/></param>
        /// <returns>True if success</returns>
        public bool UpdateRadioFrequencyConfiguration(TransmitterRadioFrequencyConfiguration transmitter, SpanByte configuration) => UpdateRadioFrequenceConfiguration((byte)transmitter, configuration);

        /// <summary>
        /// Update the radio frequency configuration
        /// </summary>
        /// <param name="receiver">The receiver configuration</param>
        /// <param name="configuration">A span of bytes for the configuration. Should be a multiple of 5 with the size of <see ref="GetRadioFrequenceConfigSize"/></param>
        /// <returns>True if success</returns>
        public bool UpdateRadioFrequencyConfiguration(ReceiverRadioFrequencyConfiguration receiver, SpanByte configuration) => UpdateRadioFrequenceConfiguration((byte)receiver, configuration);

        private bool UpdateRadioFrequenceConfiguration(byte config, SpanByte configuration)
        {
            // Page 41 documentation PN5180A0XX-C3.pdf
            if ((configuration.Length > 252) || (configuration.Length % 6 != 0) || (configuration.Length == 0))
            {
                throw new ArgumentException(nameof(configuration), "Value must be a positive multiple of 6 and no larger than 252 bytes.");
            }

            SpanByte rfConfig = new byte[1 + configuration.Length];
            rfConfig[0] = (byte)Command.UPDATE_RF_CONFIG;
            configuration.CopyTo(rfConfig.Slice(1));

            try
            {
                SpiWrite(rfConfig);
            }
            catch (TimeoutException)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// True to disable the Radio Frequency collision avoidance according to ISO/IEC 18092
        /// False to use Active Communication mode according to ISO/IEC 18092
        /// </summary>
        public RadioFrequencyCollision RadioFrequencyCollision { get; set; } = RadioFrequencyCollision.Normal;

        /// <summary>
        /// Get or set the radio frequency field. True for on, false for off
        /// </summary>
        public bool RadioFrequencyField
        {
            get
            {
                SpanByte status = new byte[4];
                try
                {
                    SpiReadRegister(Register.RF_STATUS, status);
                }
                catch (TimeoutException)
                {
                    return false;
                }

                return (status[2] & 0b1000_0000) == 0b1000_0000;
            }
            set
            {
                SetRadioFrequency(value);
            }
        }

        /// <summary>
        /// Get the radio frenquency status
        /// </summary>
        /// <returns>The radio frequence status</returns>
        public RadioFrequencyStatus GetRadioFrequencyStatus()
        {
            SpanByte status = new byte[4];
            try
            {
                SpiReadRegister(Register.RF_STATUS, status);
            }
            catch (TimeoutException)
            {
                return RadioFrequencyStatus.Error;
            }

            return (RadioFrequencyStatus)((status[2] >> 1) & 0x07);
        }

        /// <summary>
        /// Is the external field activated?
        /// </summary>
        /// <returns>True if active, false if not</returns>
        public bool IsRadioFrequencyFieldExternal()
        {
            SpanByte status = new byte[4];
            try
            {
                SpiReadRegister(Register.RF_STATUS, status);
            }
            catch (TimeoutException)
            {
                return false;
            }

            return (status[2] & 0b0000_0100) == 0b0000_0100;
        }

        private bool SetRadioFrequency(bool fieldOn)
        {
            SpanByte rfConfig = new byte[2];
            rfConfig[0] = (byte)(fieldOn ? Command.RF_ON : Command.RF_OFF);
            rfConfig[1] = (byte)RadioFrequencyCollision;
            try
            {
                SpiWrite(rfConfig);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

#endregion

#region Listen to cards

        /// <summary>
        /// Listen to any 14443 Type A card
        /// </summary>
        /// <param name="transmitter">The transmitter configuration, should be compatible with type A card</param>
        /// <param name="receiver">The receiver configuration, should be compatible with type A card</param>
        /// <param name="card">The type A card once detected</param>
        /// <param name="timeoutPollingMilliseconds">The time to poll the card in milliseconds. Card detection will stop once the detection time will be over</param>
        /// <returns>True if a 14443 Type A card has been detected</returns>
        public bool ListenToCardIso14443TypeA(TransmitterRadioFrequencyConfiguration transmitter, ReceiverRadioFrequencyConfiguration receiver,
#if NET5_0_OR_GREATER
        [NotNullWhen(true)]
#endif
        out Data106kbpsTypeA? card, int timeoutPollingMilliseconds)
        {
            card = null;
            // From NXP documentation AN12650.pdf, Page 8 and forward
            // From NXP documentation AN10833.pdf page 10
            // From TI documentation http://www.ti.com/lit/an/sloa136/sloa136.pdf page 7 and 6 for the flow of selecting
            // and getting the UID
            // Load the configuration for the specific card
            var ret = LoadRadioFrequencyConfiguration(transmitter, receiver);
            // Switch on the radio frequence field
            ret &= SetRadioFrequency(true);

            SpanByte atqa = new byte[2];
            SpanByte irqStatus = new byte[4];
            SpanByte uidSak = new byte[7];
            SpanByte uid = new byte[10];
            SpanByte sak = new byte[1];
            SpanByte sakInterm = new byte[5];
            int numBytes;
            Doublet num;

            DateTime dtTimeout = DateTime.UtcNow.AddMilliseconds(timeoutPollingMilliseconds);
            try
            {
                // Switches off the CRC off in RX and TX direction
                CrcReceptionTransfer = false;
                do
                {
                    // Clears all interrupt
                    SpiWriteRegister(Command.WRITE_REGISTER, Register.IRQ_CLEAR, new byte[] { 0xFF, 0xFF, 0x0F, 0x00 });
                    // Sets the PN5180 into IDLE state
                    SpiWriteRegister(Command.WRITE_REGISTER_AND_MASK, Register.SYSTEM_CONFIG, new byte[] { 0xB8, 0xFF, 0xFF, 0xFF });
                    // Activates TRANSCEIVE routine
                    SpiWriteRegister(Command.WRITE_REGISTER_OR_MASK, Register.SYSTEM_CONFIG, new byte[] { 0x03, 0x00, 0x00, 0x00 });
                    // Sends REQB command
                    ret = SendDataToCard(new byte[] { 0x26 }, 7);
                    num = GetNumberOfBytesReceivedAndValidBits();
                    numBytes = num.Bytes;
                    if (numBytes > 0)
                    {
                        ret &= ReadDataFromCard(atqa, atqa.Length);
                        if (!ret)
                        {
                            return false;
                        }
                    }
                    else if (dtTimeout < DateTime.UtcNow)
                    {
                        return false;
                    }
                }
                while (numBytes == 0);

                ushort cardAtqa = BinaryPrimitives.ReadUInt16LittleEndian(atqa);
                int numberOfUid = 0;
                // We do have a card! Now send anticollision
                // There are 3 SL maximum. For looping and adjusting the SL
                // SL1 = 0x93, SL2 = 0x95, SL3 = 0x97
                for (int i = 0; i < 3; i++)
                {
                    // Select SL1
                    uidSak[0] = (byte)(0x93 + i * 2);
                    // NVB = Number of valid bits
                    uidSak[1] = 0x20;
                    SendDataToCard(uidSak.Slice(0, 2));
                    // Check if 5 bytes are received, we can't proceed if we did not receive 5 bytes.
                    num = GetNumberOfBytesReceivedAndValidBits();
                    numBytes = num.Bytes;
                    if (numBytes != 5)
                    {
                        // This can happen if a card is pulled out of the field
#if DEBUG
                        _logger.LogWarning($"SAK length not 5");
#endif
                        return false;
                    }

                    // Read 5 bytes sak. Byte 1 will tell us if we have the full UID or if we need to read more
                    ReadDataFromCard(sakInterm.Slice(0, 5));
                    // Switches back on the CRC off in RX and TX direction
                    CrcReceptionTransfer = true;
                    // Now finish the anticollision with the new NVB
                    uidSak[1] = 0x70;
                    // Add the previous elements to this last anticollision to send
                    sakInterm.Slice(0, 5).CopyTo(uidSak.Slice(2));
                    ret &= SendDataToCard(uidSak);

                    ret &= ReadDataFromCard(sak, sak.Length);
                    if (!ret)
                    {
                        return false;
                    }

                    byte cardSak = sak[0];
                    if (((sak[0] & 0b0000_0100) == 0) && (i == 0))
                    {
                        // If the bit 3 is 0, then it's only a 4 bytes UID
                        uidSak.Slice(2, 4).CopyTo(uid);
                        byte[] cardNfcId = uid.Slice(0, 4).ToArray();
                        card = new Data106kbpsTypeA(
                            0,
                            cardAtqa,
                            cardSak,
                            cardNfcId,
                            null);
                        return true;
                    }
                    else if (((atqa[0] & 0b1100_0000) == 0b0100_0000) && (i == 1))
                    {
                        // if bit 7 is 1, then it's a 7 byte
                        uidSak.Slice(2, 4).CopyTo(uid.Slice(numberOfUid));
                        byte[] cardNfcId = uid.Slice(0, 4 + numberOfUid).ToArray();
                        card = new Data106kbpsTypeA(
                            0,
                            cardAtqa,
                            cardSak,
                            cardNfcId,
                            null);
                        return true;
                    }
                    else if (i == 2)
                    {
                        // Last case, it's for sure 10 bytes
                        uidSak.Slice(2, 4).CopyTo(uid.Slice(numberOfUid));
                        byte[] cardNfcId = uid.Slice(0, 4 + numberOfUid).ToArray();
                        card = new Data106kbpsTypeA(
                            0,
                            cardAtqa,
                            cardSak,
                            cardNfcId,
                            null);
                        return true;
                    }

                    if (sakInterm[0] != 0x88)
                    {
                        return false;
                    }

                    sakInterm.Slice(1, 3).CopyTo(uid.Slice(numberOfUid));
                    numberOfUid += 3;
                    CrcReceptionTransfer = false;
                }

                return false;
            }
            catch (TimeoutException)
            {
                return false;
            }
        }

        /// <summary>
        /// Listen to any 14443 Type B card
        /// </summary>
        /// <param name="transmitter">The transmitter configuration, should be compatible with type B card</param>
        /// <param name="receiver">The receiver configuration, should be compatible with type A card</param>
        /// <param name="card">The type B card once detected</param>
        /// <param name="timeoutPollingMilliseconds">The time to poll the card in milliseconds. Card detection will stop once the detection time will be over</param>
        /// <returns>True if a 14443 Type B card has been detected</returns>
        public bool ListenToCardIso14443TypeB(TransmitterRadioFrequencyConfiguration transmitter, ReceiverRadioFrequencyConfiguration receiver,
#if NET5_0_OR_GREATER
        [NotNullWhen(true)]
#endif
        out Data106kbpsTypeB? card, int timeoutPollingMilliseconds)
        {
            card = null;
            var ret = LoadRadioFrequencyConfiguration(transmitter, receiver);
            // Switch on the radio frequence field and check it
            ret &= SetRadioFrequency(true);

            // Find out which slot we have, we starts at 1, 0 are for Mifare cards and Type A
            byte targetNumber = 0;
            // rNak is defined outside the loop due to:
            // error CA2014: Potential stack overflow. Move the new out of the loop.
            SpanByte rNak = new byte[2];

            foreach (SelectedPiccInformation potentialActive in _activeSelected)
            {
                // In theory, this is working, practically, it depends of the cards
                // Some cards just halt and wait, some others, continue to answer to this ping

                // Check if the card is alive
                // Send a RNAK and wait for the RACK
                rNak[0] = 0b1111_1010;
                rNak[1] = potentialActive.Card.TargetNumber;
                ret = SendDataToCard(rNak);
                var numByts = ReadWithTimeout(rNak, (int)(potentialActive.Card.FrameWaitingTime / 1000));
                if ((!ret) || (numByts != 2))
                {
                    _activeSelected.Remove(potentialActive);
                    continue;
                }

                if (rNak[1] != potentialActive.Card.TargetNumber)
                {
                    _activeSelected.Remove(potentialActive);
                    continue;
                }

                if (rNak[0] != (potentialActive.LastBlockMark ? 0b1010_1010 : 0b1010_1011))
                {
                    _activeSelected.Remove(potentialActive);
                    continue;
                }

                // The card is still active, we keep it
            }

            // Find the first slot available
            ArrayList occupied = new ArrayList();
            for (int i = 0; i < _activeSelected.Count; i++)
            {
                occupied.Add(_activeSelected[i].Card.TargetNumber);
            }

            if (occupied.Count > 0)
            {
                for (int i = 1; i < 14; i++)
                {
                    bool foundi = false;
                    for (int m = 0; m < occupied.Count; m++)
                    {
                        if ((byte)occupied[m] == i)
                        {
                            foundi = true;
                            break;
                        }
                    }

                    if (!foundi)
                    {
                        targetNumber = (byte)i;
                        break;
                    }
                }
            }
            else
            {
                targetNumber = 1;
            }

            // We need to send a reb/wupb
            // Prefix anticollision 0x05, AFI (Application Family Identifier) to 0x00 to get all type of cards
            // The parameter 0x08 to have 1 slot
            // then CRC, normal value for this buffer are already in it
            SpanByte wupb = new byte[3]
            {
                0x05,
                0x00,
                0x08
            };
            // Will get the ATQB answer
            SpanByte atqb = new byte[12];
            // For this part, activate CRC
            CrcReceptionTransfer = true;
            int numBytes;
            Doublet num;

            DateTime dtTimeout = DateTime.UtcNow.AddMilliseconds(timeoutPollingMilliseconds);

            try
            {
                do
                {
                    // Clears all interrupt
                    SpiWriteRegister(Command.WRITE_REGISTER, Register.IRQ_CLEAR, new byte[] { 0xFF, 0xFF, 0x0F, 0x00 });
                    // Sets the PN5180 into IDLE state
                    SpiWriteRegister(Command.WRITE_REGISTER_AND_MASK, Register.SYSTEM_CONFIG, new byte[] { 0xF8, 0xFF, 0xFF, 0xFF });
                    // Activates TRANSCEIVE routine
                    SpiWriteRegister(Command.WRITE_REGISTER_OR_MASK, Register.SYSTEM_CONFIG, new byte[] { 0x03, 0x00, 0x00, 0x00 });
                    // Sends REQB command
                    ret &= SendDataToCard(wupb);
                    num = GetNumberOfBytesReceivedAndValidBits();
                    numBytes = num.Bytes;
                    if (numBytes > 0)
                    {
                        ret &= ReadDataFromCard(atqb, atqb.Length);
                        if (!ret)
                        {
                            return false;
                        }
                    }
                    else if (dtTimeout < DateTime.UtcNow)
                    {
                        return false;
                    }
                }
                while (numBytes == 0);

                card = new Data106kbpsTypeB(atqb.ToArray());

                card.TargetNumber = targetNumber;
                // Max target is 14, can't exceed this
                if (card.TargetNumber > 14)
                {
                    // We still output the data but we let
                    // the application know that the card can't be selected
                    return false;
                }

                // Now all communications will embedd the CRC
                CrcReceptionTransfer = true;
                ret = SelectCardTypeB(card);
                if (!ret)
                {
                    // If we can't select the card, we still output the data but we let
                    // the application know that the card can't be selected
                    return false;
                }

                // Add the card to the list
                var selected = new SelectedPiccInformation(card, false);
                _activeSelected.Add(selected);
                return true;
            }
            catch (TimeoutException)
            {
                return false;
            }
        }

        private bool SelectCardTypeB(Data106kbpsTypeB card)
        {
            // Now we need to select the card
            SpanByte attrib = new byte[9];
            SpanByte attribResponse = new byte[1];
            // The command for attrib is 0x1D
            attrib[0] = 0x1D;
            // then the 4 uid needs to be copied
            new SpanByte(card.NfcId).CopyTo(attrib.Slice(1));
            // We have then 4 params
            // Param 1
            attrib[5] = 0;
            // Param 2 is 0x08 for the maximum frame size
            // We will stick to 106 Kb, so we will leave this as 0x08
            attrib[6] = 0x08;
            // Param 3 to switch to protocol 14443-4
            attrib[7] = 0x01;
            // Param 4 is the CID selection, so the card number
            attrib[8] = card.TargetNumber;
            var ret = SendDataToCard(attrib);
            ret &= ReadDataFromCard(attribResponse, attribResponse.Length);
            if (!ret)
            {
                return false;
            }

            // make sure we have the same CID
            if (!(attribResponse[0] == card.TargetNumber))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Deselect a 14443 Type B card
        /// </summary>
        /// <param name="card">The card to deselect</param>
        /// <returns>True if success</returns>
        public bool DeselectCardTypeB(Data106kbpsTypeB card)
        {
            // Sblock to deselect is 0b1100_1010 according to ISO 14443-4
            SpanByte sBlock = new byte[2] { 0b1100_1010, card.TargetNumber };
            var ret = SendDataToCard(sBlock);
            var num = GetNumberOfBytesReceivedAndValidBits();
            int numBytes = num.Bytes;
            if (numBytes == 2)
            {
                ret = ReadDataFromCard(sBlock, numBytes);
                // Expected answer is the same as sent
                return ret && (sBlock[0] == 0b1100_1010) && (sBlock[1] == card.TargetNumber);
            }

            return false;
        }

        /// <summary>
        /// Set on of off the CRC calculation for the Transfer and Reception
        /// Switch off is needed for anticollision operation on type A cards. Otherwise normal state is on
        /// </summary>
        public bool CrcReceptionTransfer
        {
            get
            {
                SpanByte config = new byte[4];
                bool crc = true;
                SpiReadRegister(Register.CRC_RX_CONFIG, config);
                crc &= ((config[0] & 0x01) == 0x01);
                SpiReadRegister(Register.CRC_TX_CONFIG, config);
                crc &= ((config[0] & 0x01) == 0x01);
                return crc;
            }
            set
            {
                if (value)
                {
                    SpiWriteRegister(Command.WRITE_REGISTER_OR_MASK, Register.CRC_TX_CONFIG, new byte[] { 0x01, 0x00, 0x00, 0x00 });
                    SpiWriteRegister(Command.WRITE_REGISTER_OR_MASK, Register.CRC_RX_CONFIG, new byte[] { 0x01, 0x00, 0x00, 0x00 });
                }
                else
                {
                    SpiWriteRegister(Command.WRITE_REGISTER_AND_MASK, Register.CRC_TX_CONFIG, new byte[] { 0xFE, 0xFF, 0xFF, 0xFF });
                    SpiWriteRegister(Command.WRITE_REGISTER_AND_MASK, Register.CRC_RX_CONFIG, new byte[] { 0xFE, 0xFF, 0xFF, 0xFF });
                }
            }
        }

        /// <summary>
        /// Provide a calculation of CRC for Type B cards
        /// </summary>
        /// <param name="buffer">The buffer to process</param>
        /// <param name="crc">The CRC, Must be a 2 bytes buffer</param>
        public void CalculateCrcB(SpanByte buffer, SpanByte crc)
        {
            if (crc.Length != 2)
            {
                throw new ArgumentException(nameof(crc), $"Value must be 2 bytes.");
            }

            var crcRet = CalculateCrc(buffer, 0xFFFF);
            crcRet = (ushort)~crcRet;
            crc[0] = (byte)(crcRet & 0xFF);
            crc[1] = (byte)(crcRet >> 8);
        }

        /// <summary>
        /// Provide a calculation of CRC for Type A cards
        /// </summary>
        /// <param name="buffer">The buffer to process</param>
        /// <param name="crc">The CRC, Must be a 2 bytes buffer</param>
        public void CalculateCrcA(SpanByte buffer, SpanByte crc)
        {
            if (crc.Length != 2)
            {
                throw new ArgumentException(nameof(crc), "Value must be 2 bytes.");
            }

            var crcRet = CalculateCrc(buffer, 0x6363);
            crc[0] = (byte)(crcRet & 0xFF);
            crc[1] = (byte)(crcRet >> 8);
        }

        private ushort CalculateCrc(SpanByte buffer, ushort crcB)
        {
            // Page 42 of ISO14443-3.pdf
            for (int i = 0; i < buffer.Length; i++)
            {
                byte crcInterim = buffer[i];
                crcInterim = (byte)(crcInterim ^ (crcB & 0xFF));
                crcInterim = (byte)(crcInterim ^ (crcInterim << 4));
                crcB = (ushort)((crcB >> 8) ^ (crcInterim << 8) ^ (crcInterim << 3) ^ (crcInterim >> 4));
            }

            return crcB;
        }

        private bool GetRxStatus(SpanByte rxStatus)
        {
#if DEBUG
            _logger.LogDebug($"{nameof(GetRxStatus)}");
#endif
            if (rxStatus.Length != 4)
            {
                throw new ArgumentException(nameof(rxStatus), "Value must be 4 bytes.");
            }

            try
            {
                SpiReadRegister(Register.RX_STATUS, rxStatus);
#if DEBUG
                _logger.LogDebug($"{nameof(GetRxStatus)}: {nameof(rxStatus)}: {BitConverter.ToString(rxStatus.ToArray())}");
#endif
            }
            catch (TimeoutException tx)
            {
#if DEBUG
                _logger.LogError(tx, $"{nameof(GetRxStatus)}: {nameof(TimeoutException)} in {nameof(SpiReadRegister)}");
#endif
                return false;
            }

            return true;
        }

        private bool GetIrqStatus(SpanByte irqStatus)
        {
#if DEBUG
            _logger.LogDebug($"{nameof(GetIrqStatus)}");
#endif
            if (irqStatus.Length != 4)
            {
                throw new ArgumentException(nameof(irqStatus), "Value must be 4 bytes.");
            }

            try
            {
                SpiReadRegister(Register.IRQ_STATUS, irqStatus);
#if DEBUG
                _logger.LogDebug($"{nameof(GetIrqStatus)}: {nameof(irqStatus)}: {BitConverter.ToString(irqStatus.ToArray())}");
#endif
            }
            catch (TimeoutException tx)
            {
#if DEBUG
                _logger.LogError(tx, $"{nameof(GetIrqStatus)}: {nameof(TimeoutException)} in {nameof(SpiReadRegister)}");
#endif
                return false;
            }

            return true;
        }

#endregion

#region SPI primitives

        private void SpiWriteRegister(Command command, Register register, SpanByte data)
        {
            if (data.Length != 4)
            {
                throw new ArgumentException(nameof(data), "Value must be 4 bytes.");
            }

            SpanByte toSend = new byte[2 + data.Length];
            toSend[0] = (byte)command;
            toSend[1] = (byte)register;
            data.CopyTo(toSend.Slice(2));
            SpiWrite(toSend);
        }

        private void SpiReadRegister(Register register, SpanByte readBuffer)
        {
            SpanByte toSend = new byte[2]
            {
                (byte)Command.READ_REGISTER,
                (byte)register
            };
            SpiWrite(toSend);
            SpiRead(readBuffer);
        }

        private void SpiWriteRead(SpanByte toSend, SpanByte toRead)
        {
            SpiWrite(toSend);
            SpiRead(toRead);
        }

        private void SpiWrite(SpanByte toSend)
        {
            // Both primary and secondary devices must operate with the same timing.The primary device
            // always places data on the SDO line a half cycle before the clock edge SCK, in order for
            // the secondary device to latch the data.
            // The BUSY line is used to indicate that the system is BUSY and cannot receive any data
            // from a host.Recommendation for the BUSY line handling by the host:
            // 1.Assert NSS to Low
            // 2.Perform Data Exchange
            // 3.Wait until BUSY is high
            // 4.Deassert NSS
            // 5.Wait until BUSY is low
            // Wait for the PN8150 to be ready
            Stopwatch stopwatch = Stopwatch.StartNew();
            while (_gpioController.Read(_pinBusy) == PinValue.High)
            {
                if (stopwatch.Elapsed.TotalMilliseconds >= TimeoutWaitingMilliseconds)
                {
                    throw new TimeoutException($"PN8150 not ready to write");
                }
            }

            _gpioController.Write(_pinNss, PinValue.Low);
            Thread.Sleep(2);
            _spiDevice.Write(toSend);

            stopwatch = Stopwatch.StartNew();
            while (_gpioController.Read(_pinBusy) == PinValue.Low)
            {
                if (stopwatch.Elapsed.TotalMilliseconds >= TimeoutWaitingMilliseconds)
                {
                    throw new TimeoutException($"PN8150 is still busy after writting");
                }
            }

            _gpioController.Write(_pinNss, PinValue.High);
        }

        private void SpiRead(SpanByte toRead)
        {
            // Both primary and secondary devices must operate with the same timing.The primary device
            // always places data on the SDI line a half cycle before the clock edge SCK, in order for
            // the secondary device to latch the data.
            // The BUSY line is used to indicate that the system is BUSY and cannot receive any data
            // from a host.Recommendation for the BUSY line handling by the host:
            // 1.Assert NSS to Low
            // 2.Perform Data Exchange
            // 3.Wait until BUSY is high
            // 4.Deassert NSS
            // 5.Wait until BUSY is low

            // Wait for the PN8150 to be ready
            Stopwatch stopwatch = Stopwatch.StartNew();
            while (_gpioController.Read(_pinBusy) == PinValue.High)
            {
                if (stopwatch.Elapsed.TotalMilliseconds >= TimeoutWaitingMilliseconds)
                {
                    throw new TimeoutException($"PN8150 not ready to write");
                }
            }

            _gpioController.Write(_pinNss, PinValue.Low);

            // In order to write data to or read data from the PN5180, "dummy reads" shall be
            // performed.The Figure 8 and Figure 9 are illustrating the usage of this "dummy reads" on
            // the SPI interface.
            SpanByte dummyRead = new byte[1];
            dummyRead[0] = 0xFF;

            for (int i = 0; i < toRead.Length; i++)
            {
                _spiDevice.TransferFullDuplex(dummyRead, toRead.Slice(i, 1));
            }

            stopwatch = Stopwatch.StartNew();
            while (_gpioController.Read(_pinBusy) == PinValue.Low)
            {
                if (stopwatch.Elapsed.TotalMilliseconds >= TimeoutWaitingMilliseconds)
                {
                    throw new TimeoutException($"PN8150 is still busy after reading");
                }
            }

            _gpioController.Write(_pinNss, PinValue.High);
        }

#endregion
    }
}
