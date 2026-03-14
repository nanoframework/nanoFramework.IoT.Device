// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Threading;

namespace Iot.Device.Atecc608
{
    /// <summary>
    /// Driver for the Microchip ATECC608A/B CryptoAuthentication hardware encryption device.
    /// </summary>
    public class Atecc608Device : IDisposable
    {
        /// <summary>
        /// Exception type that represents an error status returned by the ATECC608 device.
        /// </summary>
        public sealed class Atecc608StatusException : InvalidOperationException
        {
            /// <summary>
            /// Gets the raw status byte returned by the device.
            /// </summary>
            public byte Status { get; }

            public Atecc608StatusException(string message, byte status)
                : base(message)
            {
                Status = status;
            }

            public Atecc608StatusException(string message, byte status, Exception innerException)
                : base(message, innerException)
            {
                Status = status;
            }
        }

        /// <summary>
        /// Default I2C address (7-bit: 0x60, factory default for unconfigured devices).
        /// </summary>
        public const byte DefaultI2cAddress = 0x60;

        /// <summary>
        /// I2C address used on M5Stack Core2 for AWS IoT EduKit (7-bit: 0x35).
        /// </summary>
        public const byte M5StackI2cAddress = 0x35;

        // Maximum expected response size from the ATECC608 device (including count and CRC).
        private const int MaxResponseSize = 75;

        // Word address bytes for I2C write operations.
        private const byte WordAddressReset = 0x00;
        private const byte WordAddressSleep = 0x01;
        private const byte WordAddressIdle = 0x02;
        private const byte WordAddressCommand = 0x03;

        // SHA command modes.
        private const byte ShaModeStart = 0x00;
        private const byte ShaModeUpdate = 0x01;
        private const byte ShaModeEnd = 0x02;

        // Maximum data bytes per SHA Update (64 bytes per block).
        private const int ShaBlockSize = 64;

        // GenKey modes.
        private const byte GenKeyModePrivate = 0x04;
        private const byte GenKeyModePublic = 0x00;

        // Nonce modes.
        private const byte NonceModePassthrough = 0x03;

        // Sign modes.
        private const byte SignModeExternal = 0x80;

        // Verify modes.
        private const byte VerifyModeExternal = 0x02;

        // ECDH output in clear text via response.
        private const byte EcdhModeOutput = 0x00;

        // ECC P256 sizes.
        private const int EccPublicKeySize = 64;
        private const int EccSignatureSize = 64;
        private const int EcdhSecretSize = 32;
        private const int DigestSize = 32;
        private const int NonceInputSize = 32;

        // AES-128 block size and modes.
        private const int AesBlockSize = 16;
        private const byte AesModeEncrypt = 0x00;
        private const byte AesModeDecrypt = 0x01;

        // MAC mode: key from slot, challenge from input data.
        private const byte MacModeChallenge = 0x00;
        private const int MacChallengeSize = 32;
        private const int MacResultSize = 32;

        // Counter modes.
        private const byte CounterModeRead = 0x00;
        private const byte CounterModeIncrement = 0x01;

        // SelfTest mode: run all tests (RNG, ECC, AES, SHA).
        private const byte SelfTestModeAll = 0x3B;

        // Expected wake response: count=0x04, status=0x11, CRC=0x33 0x43.
        private static readonly byte[] WakeResponse = new byte[] { 0x04, 0x11, 0x33, 0x43 };

        private readonly I2cDevice _i2cDevice;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="Atecc608Device"/> class.
        /// </summary>
        /// <param name="i2cDevice">The I2C device to communicate with the ATECC608.</param>
        public Atecc608Device(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));
        }

        /// <summary>
        /// Wakes the device from sleep mode.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the device does not respond to the wake sequence.</exception>
        public void Wake()
        {
            // The ATECC608 wake sequence: write 0x00 to generate SDA low condition.
            // The device detects this as a wake pulse.
            try
            {
                _i2cDevice.WriteByte(WordAddressReset);
            }
            catch
            {
                // A NACK is expected during wake; this is normal.
            }

            // Wait for the device to wake up (tWHI = 1500 us).
            Thread.Sleep(2);

            // Read the 4-byte wake response.
            byte[] response = new byte[4];
            _i2cDevice.Read(response);

            if (response[0] != WakeResponse[0] ||
                response[1] != WakeResponse[1] ||
                response[2] != WakeResponse[2] ||
                response[3] != WakeResponse[3])
            {
                throw new InvalidOperationException("ATECC608 wake failed: unexpected response.");
            }
        }

        /// <summary>
        /// Puts the device into idle mode. The device preserves volatile state but draws less power.
        /// </summary>
        public void Idle()
        {
            _i2cDevice.WriteByte(WordAddressIdle);
        }

        /// <summary>
        /// Puts the device into sleep mode (lowest power). All volatile state is lost.
        /// </summary>
        public void Sleep()
        {
            _i2cDevice.WriteByte(WordAddressSleep);
        }

        /// <summary>
        /// Reads the unique 9-byte serial number from the device.
        /// </summary>
        /// <returns>A 9-byte array containing the serial number.</returns>
        public byte[] GetSerialNumber()
        {
            // Serial number is split across config zone:
            // Bytes 0-3 at word offset 0 (first 4 bytes of 32-bit read at address 0x0000)
            // Bytes 4-8 at word offset 2 (first 5 bytes of 32-bit read at address 0x0008)
            byte[] part1 = ReadZone(Atecc608Zone.Config, 0, 0x0000, false);
            byte[] part2 = ReadZone(Atecc608Zone.Config, 0, 0x0002, false);
            byte[] part3 = ReadZone(Atecc608Zone.Config, 0, 0x0003, false);

            byte[] serial = new byte[9];
            serial[0] = part1[0];
            serial[1] = part1[1];
            serial[2] = part1[2];
            serial[3] = part1[3];
            serial[4] = part2[0];
            serial[5] = part2[1];
            serial[6] = part2[2];
            serial[7] = part2[3];
            serial[8] = part3[0];

            return serial;
        }

        /// <summary>
        /// Reads the 4-byte revision number from the device.
        /// </summary>
        /// <returns>A 4-byte array containing the device revision.</returns>
        public byte[] GetRevisionNumber()
        {
            // Revision is at config zone word offset 1 (bytes 4-7).
            byte[] data = ReadZone(Atecc608Zone.Config, 0, 0x0001, false);
            return data;
        }

        /// <summary>
        /// Reads the complete configuration zone (128 bytes).
        /// </summary>
        /// <returns>A 128-byte array containing the full configuration zone.</returns>
        public byte[] ReadConfigZone()
        {
            byte[] config = new byte[128];
            int offset = 0;

            // Config zone is 128 bytes. Read in 32-byte blocks (4 blocks).
            for (int block = 0; block < 4; block++)
            {
                byte[] blockData = ReadZone(Atecc608Zone.Config, 0, (ushort)(block << 3), true);
                Array.Copy(blockData, 0, config, offset, 32);
                offset += 32;
            }

            return config;
        }

        /// <summary>
        /// Gets a value indicating whether the configuration zone is locked.
        /// </summary>
        /// <value><c>true</c> if the configuration zone is locked; otherwise, <c>false</c>.</value>
        public bool IsConfigLocked
        {
            get
            {
                // LockConfig is at byte 87 in the config zone (word 21, byte offset 3).
                // 0x00 = locked, any other value = unlocked.
                byte[] data = ReadZone(Atecc608Zone.Config, 0, 0x0015, false);
                return data[3] == 0x00;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the data/OTP zone is locked.
        /// </summary>
        /// <value><c>true</c> if the data/OTP zone is locked; otherwise, <c>false</c>.</value>
        public bool IsDataLocked
        {
            get
            {
                // LockValue is at byte 86 in the config zone (word 21, byte offset 2).
                // 0x00 = locked, any other value = unlocked.
                byte[] data = ReadZone(Atecc608Zone.Config, 0, 0x0015, false);
                return data[2] == 0x00;
            }
        }

        /// <summary>
        /// Generates a 32-byte random number using the hardware random number generator.
        /// </summary>
        /// <returns>A 32-byte array containing the random number.</returns>
        public byte[] GetRandomNumber()
        {
            // Random command: opcode=0x1B, param1=0x00 (Random mode), param2=0x0000.
            byte[] response = ExecuteCommand(Atecc608Command.Random, 0x00, 0x0000, null, 23);
            return response;
        }

        /// <summary>
        /// Computes a SHA-256 hash of the given data using the hardware accelerator.
        /// </summary>
        /// <param name="data">The data to hash.</param>
        /// <returns>A 32-byte array containing the SHA-256 digest.</returns>
        public byte[] ComputeSha256(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            // SHA Start: initialize the SHA engine.
            ExecuteCommand(Atecc608Command.Sha, ShaModeStart, 0x0000, null, 36);

            int offset = 0;
            int remaining = data.Length;

            // SHA Update: process full 64-byte blocks.
            while (remaining >= ShaBlockSize)
            {
                byte[] block = new byte[ShaBlockSize];
                Array.Copy(data, offset, block, 0, ShaBlockSize);
                ExecuteCommand(Atecc608Command.Sha, ShaModeUpdate, (ushort)ShaBlockSize, block, 36);
                offset += ShaBlockSize;
                remaining -= ShaBlockSize;
            }

            // SHA End (or Finalize): process the remaining bytes and get the digest.
            byte[] lastBlock = null;
            if (remaining > 0)
            {
                lastBlock = new byte[remaining];
                Array.Copy(data, offset, lastBlock, 0, remaining);
            }

            byte[] digest = ExecuteCommand(Atecc608Command.Sha, ShaModeEnd, (ushort)remaining, lastBlock, 36);
            return digest;
        }

        /// <summary>
        /// Reads data from the specified zone and address.
        /// </summary>
        /// <param name="zone">The memory zone to read from.</param>
        /// <param name="slot">The slot number (0-15 for data zone, 0 otherwise).</param>
        /// <param name="address">The word address within the zone.</param>
        /// <param name="is32Bytes">If <c>true</c>, reads a 32-byte block; otherwise reads 4 bytes.</param>
        /// <returns>The data read (4 or 32 bytes).</returns>
        public byte[] ReadZone(Atecc608Zone zone, ushort slot, ushort address, bool is32Bytes)
        {
            byte param1 = (byte)zone;
            if (is32Bytes)
            {
                param1 |= 0x80;
            }

            // Build the param2 address word.
            // For config/OTP: param2 = address (block/offset encoded).
            // For data: param2 includes slot number in bits 3+ .
            ushort param2;
            if (zone == Atecc608Zone.Data)
            {
                param2 = (ushort)((slot << 3) | (address & 0x07));
            }
            else
            {
                param2 = address;
            }

            int expectedLen = is32Bytes ? 32 : 4;
            byte[] response = ExecuteCommand(Atecc608Command.Read, param1, param2, null, 5 + expectedLen);
            return response;
        }

        /// <summary>
        /// Writes data to the specified zone and address.
        /// </summary>
        /// <param name="zone">The memory zone to write to.</param>
        /// <param name="slot">The slot number (0-15 for data zone, 0 otherwise).</param>
        /// <param name="address">The word address within the zone.</param>
        /// <param name="data">The data to write (must be 4 or 32 bytes).</param>
        public void WriteZone(Atecc608Zone zone, ushort slot, ushort address, byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (data.Length != 4 && data.Length != 32)
            {
                throw new ArgumentException("Write data must be 4 or 32 bytes.", nameof(data));
            }

            byte param1 = (byte)zone;
            if (data.Length == 32)
            {
                param1 |= 0x80;
            }

            ushort param2;
            if (zone == Atecc608Zone.Data)
            {
                param2 = (ushort)((slot << 3) | (address & 0x07));
            }
            else
            {
                param2 = address;
            }

            ExecuteCommand(Atecc608Command.Write, param1, param2, data, 45);
        }

        /// <summary>
        /// Locks the configuration zone. This operation is irreversible.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the lock command fails.</exception>
        public void LockConfigZone()
        {
            // Lock command: param1 bit7=NoCRC, bits[1:0]=0x00 (config zone).
            // param2=0x0000 (ignored when NoCRC is set).
            ExecuteCommand(Atecc608Command.Lock, 0x80, 0x0000, null, 35);
        }

        /// <summary>
        /// Locks the data and OTP zones. This operation is irreversible.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the lock command fails.</exception>
        public void LockDataZone()
        {
            // Lock command: param1 bit7=NoCRC, bits[1:0]=0x01 (data/OTP zone).
            // param2=0x0000 (ignored when NoCRC is set).
            ExecuteCommand(Atecc608Command.Lock, 0x81, 0x0000, null, 35);
        }

        /// <summary>
        /// Loads a 32-byte nonce into the device TempKey for use by subsequent commands (Sign, Verify, etc.).
        /// Uses passthrough mode to load an externally-provided value.
        /// </summary>
        /// <param name="nonce">A 32-byte value to load into TempKey.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="nonce"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="nonce"/> is not 32 bytes.</exception>
        public void LoadNonce(byte[] nonce)
        {
            if (nonce == null)
            {
                throw new ArgumentNullException(nameof(nonce));
            }

            if (nonce.Length != NonceInputSize)
            {
                throw new ArgumentException("Nonce must be 32 bytes.", nameof(nonce));
            }

            // Nonce command in passthrough mode: param1=0x03, param2=0x0000, data=32 bytes.
            ExecuteCommand(Atecc608Command.Nonce, NonceModePassthrough, 0x0000, nonce, 20);
        }

        /// <summary>
        /// Generates a new random ECC P256 private key in the specified slot.
        /// The private key is generated internally and never leaves the device.
        /// The corresponding 64-byte public key (X,Y) is returned.
        /// </summary>
        /// <param name="keySlot">The slot number (0-15) where the private key will be stored.</param>
        /// <returns>A 64-byte array containing the public key (32-byte X + 32-byte Y).</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the slot number is not between 0 and 15.</exception>
        public byte[] GeneratePrivateKey(int keySlot)
        {
            ValidateSlotNumber(keySlot);

            // GenKey with mode=0x04 (Private): generates a new private key and returns the public key.
            byte[] publicKey = ExecuteCommand(Atecc608Command.GenKey, GenKeyModePrivate, (ushort)keySlot, null, 115);

            if (publicKey.Length != EccPublicKeySize)
            {
                throw new InvalidOperationException($"Expected {EccPublicKeySize}-byte public key, got {publicKey.Length} bytes.");
            }

            return publicKey;
        }

        /// <summary>
        /// Computes and returns the 64-byte public key (X,Y) for an existing private key stored in the specified slot.
        /// </summary>
        /// <param name="keySlot">The slot number (0-15) containing the private key.</param>
        /// <returns>A 64-byte array containing the public key (32-byte X + 32-byte Y).</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the slot number is not between 0 and 15.</exception>
        public byte[] GetPublicKey(int keySlot)
        {
            ValidateSlotNumber(keySlot);

            // GenKey with mode=0x00 (Public): computes the public key from the stored private key.
            byte[] publicKey = ExecuteCommand(Atecc608Command.GenKey, GenKeyModePublic, (ushort)keySlot, null, 115);

            if (publicKey.Length != EccPublicKeySize)
            {
                throw new InvalidOperationException($"Expected {EccPublicKeySize}-byte public key, got {publicKey.Length} bytes.");
            }

            return publicKey;
        }

        /// <summary>
        /// Signs a 32-byte message digest using the private key stored in the specified slot.
        /// The digest must first be loaded into TempKey using <see cref="LoadNonce(byte[])"/>.
        /// </summary>
        /// <param name="keySlot">The slot number (0-15) containing the private key to sign with.</param>
        /// <returns>A 64-byte ECDSA signature (32-byte R + 32-byte S).</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the slot number is not between 0 and 15.</exception>
        public byte[] Sign(int keySlot)
        {
            ValidateSlotNumber(keySlot);

            // Sign command: mode=0x80 (external message in TempKey), param2=key slot.
            byte[] signature = ExecuteCommand(Atecc608Command.Sign, SignModeExternal, (ushort)keySlot, null, 115);

            if (signature.Length != EccSignatureSize)
            {
                throw new InvalidOperationException($"Expected {EccSignatureSize}-byte signature, got {signature.Length} bytes.");
            }

            return signature;
        }

        /// <summary>
        /// Signs a 32-byte message digest using the private key stored in the specified slot.
        /// This convenience method loads the digest into TempKey and then signs it.
        /// </summary>
        /// <param name="keySlot">The slot number (0-15) containing the private key to sign with.</param>
        /// <param name="digest">A 32-byte SHA-256 digest to sign.</param>
        /// <returns>A 64-byte ECDSA signature (32-byte R + 32-byte S).</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the slot number is not between 0 and 15.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="digest"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="digest"/> is not 32 bytes.</exception>
        public byte[] SignDigest(int keySlot, byte[] digest)
        {
            if (digest == null)
            {
                throw new ArgumentNullException(nameof(digest));
            }

            if (digest.Length != DigestSize)
            {
                throw new ArgumentException("Digest must be 32 bytes.", nameof(digest));
            }

            LoadNonce(digest);
            return Sign(keySlot);
        }

        /// <summary>
        /// Verifies an ECDSA signature against an externally-provided public key.
        /// The digest must first be loaded into TempKey using <see cref="LoadNonce(byte[])"/>.
        /// </summary>
        /// <param name="publicKey">A 64-byte public key (32-byte X + 32-byte Y).</param>
        /// <param name="signature">A 64-byte ECDSA signature (32-byte R + 32-byte S).</param>
        /// <returns><c>true</c> if the signature is valid; <c>false</c> otherwise.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="publicKey"/> or <paramref name="signature"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the sizes are incorrect.</exception>
        public bool Verify(byte[] publicKey, byte[] signature)
        {
            if (publicKey == null)
            {
                throw new ArgumentNullException(nameof(publicKey));
            }

            if (signature == null)
            {
                throw new ArgumentNullException(nameof(signature));
            }

            if (publicKey.Length != EccPublicKeySize)
            {
                throw new ArgumentException("Public key must be 64 bytes (X + Y).", nameof(publicKey));
            }

            if (signature.Length != EccSignatureSize)
            {
                throw new ArgumentException("Signature must be 64 bytes (R + S).", nameof(signature));
            }

            // Verify command data = signature (64 bytes) + public key (64 bytes) = 128 bytes.
            byte[] verifyData = new byte[EccSignatureSize + EccPublicKeySize];
            Array.Copy(signature, 0, verifyData, 0, EccSignatureSize);
            Array.Copy(publicKey, 0, verifyData, EccSignatureSize, EccPublicKeySize);

            // Verify command: mode=0x02 (External), param2=0x0004 (P256 curve key type).
            try
            {
                ExecuteCommand(Atecc608Command.Verify, VerifyModeExternal, 0x0004, verifyData, 115);
                return true;
            }
            catch (Atecc608StatusException ex) when (ex.Status == 0x01)
            {
                // Status 0x01 (CheckmacVerifyMiscompare) means signature is invalid.
                return false;
            }
        }

        /// <summary>
        /// Verifies an ECDSA signature against an externally-provided public key and digest.
        /// This convenience method loads the digest into TempKey and then verifies.
        /// </summary>
        /// <param name="publicKey">A 64-byte public key (32-byte X + 32-byte Y).</param>
        /// <param name="digest">A 32-byte SHA-256 digest that was signed.</param>
        /// <param name="signature">A 64-byte ECDSA signature (32-byte R + 32-byte S).</param>
        /// <returns><c>true</c> if the signature is valid; <c>false</c> otherwise.</returns>
        public bool VerifyDigest(byte[] publicKey, byte[] digest, byte[] signature)
        {
            if (digest == null)
            {
                throw new ArgumentNullException(nameof(digest));
            }

            if (digest.Length != DigestSize)
            {
                throw new ArgumentException("Digest must be 32 bytes.", nameof(digest));
            }

            LoadNonce(digest);
            return Verify(publicKey, signature);
        }

        /// <summary>
        /// Performs ECDH key agreement using the private key stored in the specified slot
        /// and an externally-provided public key.
        /// Returns the 32-byte shared secret.
        /// </summary>
        /// <param name="keySlot">The slot number (0-15) containing the private key.</param>
        /// <param name="otherPublicKey">A 64-byte public key (32-byte X + 32-byte Y) from the other party.</param>
        /// <returns>A 32-byte shared secret (the X coordinate of the ECDH point).</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the slot number is not between 0 and 15.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="otherPublicKey"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="otherPublicKey"/> is not 64 bytes.</exception>
        public byte[] Ecdh(int keySlot, byte[] otherPublicKey)
        {
            ValidateSlotNumber(keySlot);

            if (otherPublicKey == null)
            {
                throw new ArgumentNullException(nameof(otherPublicKey));
            }

            if (otherPublicKey.Length != EccPublicKeySize)
            {
                throw new ArgumentException("Public key must be 64 bytes (X + Y).", nameof(otherPublicKey));
            }

            // ECDH command: mode=0x00 (output in clear), param2=key slot, data=64-byte public key.
            byte[] secret = ExecuteCommand(Atecc608Command.Ecdh, EcdhModeOutput, (ushort)keySlot, otherPublicKey, 75);

            if (secret.Length != EcdhSecretSize)
            {
                throw new InvalidOperationException($"Expected {EcdhSecretSize}-byte ECDH secret, got {secret.Length} bytes.");
            }

            return secret;
        }

        /// <summary>
        /// Encrypts a 16-byte block using AES-128 ECB with the key stored in the specified slot.
        /// The slot must be configured with KeyType = AES.
        /// </summary>
        /// <param name="keySlot">The slot number (0-15) containing the AES key.</param>
        /// <param name="plaintext">A 16-byte plaintext block to encrypt.</param>
        /// <returns>A 16-byte encrypted ciphertext block.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the slot number is not between 0 and 15.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="plaintext"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="plaintext"/> is not 16 bytes.</exception>
        public byte[] AesEncrypt(int keySlot, byte[] plaintext)
        {
            ValidateSlotNumber(keySlot);

            if (plaintext == null)
            {
                throw new ArgumentNullException(nameof(plaintext));
            }

            if (plaintext.Length != AesBlockSize)
            {
                throw new ArgumentException("Plaintext must be 16 bytes (one AES block).", nameof(plaintext));
            }

            byte[] ciphertext = ExecuteCommand(Atecc608Command.Aes, AesModeEncrypt, (ushort)keySlot, plaintext, 27);

            if (ciphertext.Length != AesBlockSize)
            {
                throw new InvalidOperationException($"Expected {AesBlockSize}-byte AES output, got {ciphertext.Length} bytes.");
            }

            return ciphertext;
        }

        /// <summary>
        /// Decrypts a 16-byte block using AES-128 ECB with the key stored in the specified slot.
        /// The slot must be configured with KeyType = AES.
        /// </summary>
        /// <param name="keySlot">The slot number (0-15) containing the AES key.</param>
        /// <param name="ciphertext">A 16-byte ciphertext block to decrypt.</param>
        /// <returns>A 16-byte decrypted plaintext block.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the slot number is not between 0 and 15.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="ciphertext"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="ciphertext"/> is not 16 bytes.</exception>
        public byte[] AesDecrypt(int keySlot, byte[] ciphertext)
        {
            ValidateSlotNumber(keySlot);

            if (ciphertext == null)
            {
                throw new ArgumentNullException(nameof(ciphertext));
            }

            if (ciphertext.Length != AesBlockSize)
            {
                throw new ArgumentException("Ciphertext must be 16 bytes (one AES block).", nameof(ciphertext));
            }

            byte[] plaintext = ExecuteCommand(Atecc608Command.Aes, AesModeDecrypt, (ushort)keySlot, ciphertext, 27);

            if (plaintext.Length != AesBlockSize)
            {
                throw new InvalidOperationException($"Expected {AesBlockSize}-byte AES output, got {plaintext.Length} bytes.");
            }

            return plaintext;
        }

        /// <summary>
        /// Computes a SHA-256 based MAC using the key stored in the specified slot and a 32-byte challenge.
        /// Used for challenge-response authentication schemes.
        /// </summary>
        /// <param name="keySlot">The slot number (0-15) containing the MAC key.</param>
        /// <param name="challenge">A 32-byte challenge value.</param>
        /// <returns>A 32-byte MAC digest.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the slot number is not between 0 and 15.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="challenge"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="challenge"/> is not 32 bytes.</exception>
        public byte[] ComputeMac(int keySlot, byte[] challenge)
        {
            ValidateSlotNumber(keySlot);

            if (challenge == null)
            {
                throw new ArgumentNullException(nameof(challenge));
            }

            if (challenge.Length != MacChallengeSize)
            {
                throw new ArgumentException("Challenge must be 32 bytes.", nameof(challenge));
            }

            byte[] mac = ExecuteCommand(Atecc608Command.Mac, MacModeChallenge, (ushort)keySlot, challenge, 35);

            if (mac.Length != MacResultSize)
            {
                throw new InvalidOperationException($"Expected {MacResultSize}-byte MAC, got {mac.Length} bytes.");
            }

            return mac;
        }

        /// <summary>
        /// Reads the current value of a monotonic counter.
        /// The ATECC608 has two counters (index 0 and 1), each with a maximum value of 2,097,151.
        /// </summary>
        /// <param name="counterIndex">The counter index (0 or 1).</param>
        /// <returns>The current 32-bit counter value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the counter index is not 0 or 1.</exception>
        public int ReadCounter(int counterIndex)
        {
            ValidateCounterIndex(counterIndex);

            byte[] response = ExecuteCommand(Atecc608Command.Counter, CounterModeRead, (ushort)counterIndex, null, 25);

            if (response.Length < 4)
            {
                throw new InvalidOperationException("Counter read returned insufficient data.");
            }

            return response[0] | (response[1] << 8) | (response[2] << 16) | (response[3] << 24);
        }

        /// <summary>
        /// Increments a monotonic counter by one and returns the new value.
        /// This operation is irreversible — the counter cannot be decremented.
        /// The ATECC608 has two counters (index 0 and 1), each with a maximum value of 2,097,151.
        /// </summary>
        /// <param name="counterIndex">The counter index (0 or 1).</param>
        /// <returns>The new 32-bit counter value after incrementing.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the counter index is not 0 or 1.</exception>
        public int IncrementCounter(int counterIndex)
        {
            ValidateCounterIndex(counterIndex);

            byte[] response = ExecuteCommand(Atecc608Command.Counter, CounterModeIncrement, (ushort)counterIndex, null, 25);

            if (response.Length < 4)
            {
                throw new InvalidOperationException("Counter increment returned insufficient data.");
            }

            return response[0] | (response[1] << 8) | (response[2] << 16) | (response[3] << 24);
        }

        /// <summary>
        /// Runs the device self-test for all internal cryptographic engines (RNG, ECC, AES, SHA).
        /// </summary>
        /// <returns><c>true</c> if all tests passed; <c>false</c> if any test failed.</returns>
        public bool RunSelfTest()
        {
            try
            {
                ExecuteCommand(Atecc608Command.SelfTest, SelfTestModeAll, 0x0000, null, 2500);
                return true;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }

        /// <summary>
        /// Reads the complete configuration zone and parses the configuration for the specified slot.
        /// </summary>
        /// <param name="slotNumber">The slot number (0-15).</param>
        /// <returns>An <see cref="Atecc608SlotConfig"/> containing the parsed slot configuration.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the slot number is not between 0 and 15.</exception>
        public Atecc608SlotConfig GetSlotConfiguration(int slotNumber)
        {
            ValidateSlotNumber(slotNumber);
            byte[] config = ReadConfigZone();
            return Atecc608SlotConfig.Parse(slotNumber, config);
        }

        /// <summary>
        /// Locks an individual data slot. This operation is irreversible.
        /// </summary>
        /// <param name="slotNumber">The slot number (0-15) to lock.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the slot number is not between 0 and 15.</exception>
        public void LockSlot(int slotNumber)
        {
            ValidateSlotNumber(slotNumber);

            // Lock command: param1 bit7=NoCRC, bits[1:0]=0x02 (slot lock), bits[5:2]=slot number.
            byte param1 = (byte)(0x80 | 0x02 | ((slotNumber & 0x0F) << 2));
            ExecuteCommand(Atecc608Command.Lock, param1, 0x0000, null, 35);
        }

        /// <summary>
        /// Validates that a slot number is in the range 0 to 15.
        /// </summary>
        /// <param name="slotNumber">The slot number to validate.</param>
        private static void ValidateSlotNumber(int slotNumber)
        {
            if (slotNumber < 0 || slotNumber > 15)
            {
                throw new ArgumentOutOfRangeException(nameof(slotNumber), "Slot number must be between 0 and 15.");
            }
        }

        /// <summary>
        /// Validates that a counter index is 0 or 1.
        /// </summary>
        /// <param name="counterIndex">The counter index to validate.</param>
        private static void ValidateCounterIndex(int counterIndex)
        {
            if (counterIndex < 0 || counterIndex > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(counterIndex), "Counter index must be 0 or 1.");
            }
        }

        /// <summary>
        /// Executes a command on the ATECC608 device and returns the response data.
        /// </summary>
        /// <param name="command">The command opcode.</param>
        /// <param name="param1">The first parameter byte.</param>
        /// <param name="param2">The second parameter (2 bytes, little-endian).</param>
        /// <param name="data">Optional command data payload.</param>
        /// <param name="executionTimeMs">Maximum execution time to wait before reading the response.</param>
        /// <returns>The response data (excluding count byte and CRC).</returns>
        private byte[] ExecuteCommand(Atecc608Command command, byte param1, ushort param2, byte[] data, int executionTimeMs)
        {
            // Build the command packet.
            int dataLen = data != null ? data.Length : 0;

            // Count = 1 (count) + 1 (opcode) + 1 (param1) + 2 (param2) + dataLen + 2 (CRC) = 7 + dataLen.
            int count = 7 + dataLen;
            byte[] packet = new byte[1 + count]; // +1 for word address byte.
            packet[0] = WordAddressCommand;
            packet[1] = (byte)count;
            packet[2] = (byte)command;
            packet[3] = param1;
            packet[4] = (byte)(param2 & 0xFF);
            packet[5] = (byte)((param2 >> 8) & 0xFF);

            if (data != null)
            {
                Array.Copy(data, 0, packet, 6, dataLen);
            }

            // Calculate CRC over bytes from Count to end of Data (indices 1 to 1+count-2-1).
            ushort crc = ComputeCrc(packet, 1, count - 2);
            packet[1 + count - 2] = (byte)(crc & 0xFF);
            packet[1 + count - 1] = (byte)((crc >> 8) & 0xFF);

            // Send the command.
            _i2cDevice.Write(packet);

            // Wait for execution.
            Thread.Sleep(executionTimeMs);

            // Read the response. First read count byte.
            // Read the maximum possible response in a single I2C transaction.
            byte[] rawResponse = new byte[MaxResponseSize];
            _i2cDevice.Read(rawResponse);
            int responseCount = rawResponse[0];

            if (responseCount < 4 || responseCount > MaxResponseSize)
            {
                throw new InvalidOperationException("ATECC608 returned an invalid response length.");
            }

            // Copy the actual response bytes (as indicated by responseCount).
            byte[] responseFull = new byte[responseCount];
            Array.Copy(rawResponse, 0, responseFull, 0, responseCount);

            // Verify CRC.
            ushort responseCrc = ComputeCrc(responseFull, 0, responseCount - 2);
            ushort receivedCrc = (ushort)(responseFull[responseCount - 2] | (responseFull[responseCount - 1] << 8));

            if (responseCrc != receivedCrc)
            {
                throw new InvalidOperationException("ATECC608 response CRC mismatch.");
            }

            // Check for error status (single-byte response data = status code).
            if (responseCount == 4)
            {
                byte status = responseFull[1];
                if (status != (byte)Atecc608Status.Success)
                {
                    throw new InvalidOperationException($"ATECC608 command failed with status 0x{status:X2}.");
                }

                return new byte[0];
            }

            // Extract data (skip count byte, exclude CRC).
            int responseDataLen = responseCount - 3; // minus count(1) and CRC(2).
            byte[] responseData = new byte[responseDataLen];
            Array.Copy(responseFull, 1, responseData, 0, responseDataLen);
            return responseData;
        }

        /// <summary>
        /// Computes the CRC-16 used by the ATECC608 (polynomial 0x8005).
        /// </summary>
        /// <param name="data">The byte array containing data to CRC.</param>
        /// <param name="offset">The starting offset in the array.</param>
        /// <param name="length">The number of bytes to process.</param>
        /// <returns>The computed 16-bit CRC value.</returns>
        internal static ushort ComputeCrc(byte[] data, int offset, int length)
        {
            ushort crc = 0x0000;

            for (int i = offset; i < offset + length; i++)
            {
                byte b = data[i];
                for (int bit = 0; bit < 8; bit++)
                {
                    int dataVal = (b >> bit) & 1;
                    int crcBit = (crc >> 15) & 1;
                    crc = (ushort)(crc << 1);

                    if (dataVal != crcBit)
                    {
                        crc ^= 0x8005;
                    }
                }
            }

            return crc;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (!_disposed)
            {
                _i2cDevice?.Dispose();
                _disposed = true;
            }
        }
    }
}
