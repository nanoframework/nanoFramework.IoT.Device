// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Atecc608
{
    /// <summary>
    /// Represents the parsed configuration of a single ATECC608 slot,
    /// combining SlotConfig (config zone bytes 20–51) and KeyConfig (config zone bytes 96–127).
    /// </summary>
    public class Atecc608SlotConfig
    {
        // Config zone offsets.
        private const int SlotConfigOffset = 20;
        private const int KeyConfigOffset = 96;
        private const int ConfigZoneSize = 128;

        private readonly int _slotNumber;
        private readonly ushort _slotConfigValue;
        private readonly ushort _keyConfigValue;

        // SlotConfig parsed fields.
        private readonly int _readKey;
        private readonly bool _noMac;
        private readonly bool _limitedUse;
        private readonly bool _encryptRead;
        private readonly bool _isSecret;
        private readonly int _writeKey;
        private readonly int _writeConfig;

        // KeyConfig parsed fields.
        private readonly bool _isPrivate;
        private readonly bool _pubInfo;
        private readonly int _keyType;
        private readonly bool _lockable;
        private readonly bool _reqRandom;
        private readonly bool _reqAuth;
        private readonly int _authKey;
        private readonly int _x509Id;

        private Atecc608SlotConfig(int slotNumber, ushort slotConfig, ushort keyConfig)
        {
            _slotNumber = slotNumber;
            _slotConfigValue = slotConfig;
            _keyConfigValue = keyConfig;

            // Parse SlotConfig (little-endian 16-bit word).
            _readKey = slotConfig & 0x0F;
            _noMac = (slotConfig & 0x10) != 0;
            _limitedUse = (slotConfig & 0x20) != 0;
            _encryptRead = (slotConfig & 0x40) != 0;
            _isSecret = (slotConfig & 0x80) != 0;
            _writeKey = (slotConfig >> 8) & 0x0F;
            _writeConfig = (slotConfig >> 12) & 0x0F;

            // Parse KeyConfig (little-endian 16-bit word).
            _isPrivate = (keyConfig & 0x01) != 0;
            _pubInfo = (keyConfig & 0x02) != 0;
            _keyType = (keyConfig >> 2) & 0x07;
            _lockable = (keyConfig & 0x20) != 0;
            _reqRandom = (keyConfig & 0x40) != 0;
            _reqAuth = (keyConfig & 0x80) != 0;
            _authKey = (keyConfig >> 8) & 0x0F;
            _x509Id = (keyConfig >> 14) & 0x03;
        }

        /// <summary>
        /// Gets the slot number (0–15).
        /// </summary>
        public int SlotNumber
        {
            get { return _slotNumber; }
        }

        /// <summary>
        /// Gets the raw 16-bit SlotConfig value.
        /// </summary>
        public ushort SlotConfigValue
        {
            get { return _slotConfigValue; }
        }

        /// <summary>
        /// Gets the raw 16-bit KeyConfig value.
        /// </summary>
        public ushort KeyConfigValue
        {
            get { return _keyConfigValue; }
        }

        /// <summary>
        /// Gets the ReadKey field (bits [3:0] of SlotConfig). Specifies the slot used for encrypted reads.
        /// </summary>
        public int ReadKey
        {
            get { return _readKey; }
        }

        /// <summary>
        /// Gets a value indicating whether the NoMac flag is set.
        /// When set, the slot can be used without MAC verification.
        /// </summary>
        public bool NoMac
        {
            get { return _noMac; }
        }

        /// <summary>
        /// Gets a value indicating whether the LimitedUse flag is set.
        /// When set, the key usage is limited by a counter.
        /// </summary>
        public bool LimitedUse
        {
            get { return _limitedUse; }
        }

        /// <summary>
        /// Gets a value indicating whether encrypted reads are required for this slot.
        /// </summary>
        public bool EncryptRead
        {
            get { return _encryptRead; }
        }

        /// <summary>
        /// Gets a value indicating whether this slot contains a secret key.
        /// </summary>
        public bool IsSecret
        {
            get { return _isSecret; }
        }

        /// <summary>
        /// Gets the WriteKey field (bits [11:8] of SlotConfig). Specifies the slot used for encrypted writes.
        /// </summary>
        public int WriteKey
        {
            get { return _writeKey; }
        }

        /// <summary>
        /// Gets the WriteConfig field (bits [15:12] of SlotConfig). Determines write permissions.
        /// </summary>
        public int WriteConfig
        {
            get { return _writeConfig; }
        }

        /// <summary>
        /// Gets a value indicating whether this slot is configured to hold a private key.
        /// </summary>
        public bool IsPrivate
        {
            get { return _isPrivate; }
        }

        /// <summary>
        /// Gets a value indicating whether public key information can be generated from this slot.
        /// </summary>
        public bool PubInfo
        {
            get { return _pubInfo; }
        }

        /// <summary>
        /// Gets the KeyType field (bits [4:2] of KeyConfig).
        /// Common values: 4 = P256 ECC, 6 = AES, 7 = SHA or other data.
        /// </summary>
        public int KeyType
        {
            get { return _keyType; }
        }

        /// <summary>
        /// Gets a value indicating whether this slot can be individually locked.
        /// </summary>
        public bool Lockable
        {
            get { return _lockable; }
        }

        /// <summary>
        /// Gets a value indicating whether a random nonce is required before use.
        /// </summary>
        public bool ReqRandom
        {
            get { return _reqRandom; }
        }

        /// <summary>
        /// Gets a value indicating whether authorization is required before use.
        /// </summary>
        public bool ReqAuth
        {
            get { return _reqAuth; }
        }

        /// <summary>
        /// Gets the AuthKey field (bits [11:8] of KeyConfig). Specifies the authorization key slot.
        /// </summary>
        public int AuthKey
        {
            get { return _authKey; }
        }

        /// <summary>
        /// Gets the X509Id field (bits [15:14] of KeyConfig). Identifies the X.509 format template.
        /// </summary>
        public int X509Id
        {
            get { return _x509Id; }
        }

        /// <summary>
        /// Parses the slot configuration for the specified slot from a 128-byte config zone dump.
        /// </summary>
        /// <param name="slotNumber">The slot number (0–15).</param>
        /// <param name="configZone">A 128-byte array containing the complete configuration zone.</param>
        /// <returns>A new <see cref="Atecc608SlotConfig"/> with the parsed configuration.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="configZone"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="configZone"/> is not 128 bytes.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="slotNumber"/> is not between 0 and 15.</exception>
        public static Atecc608SlotConfig Parse(int slotNumber, byte[] configZone)
        {
            if (configZone == null)
            {
                throw new ArgumentNullException(nameof(configZone));
            }

            if (configZone.Length != ConfigZoneSize)
            {
                throw new ArgumentException("Config zone must be 128 bytes.", nameof(configZone));
            }

            if (slotNumber < 0 || slotNumber > 15)
            {
                throw new ArgumentOutOfRangeException(nameof(slotNumber), "Slot number must be between 0 and 15.");
            }

            int scOffset = SlotConfigOffset + (slotNumber * 2);
            ushort slotConfig = (ushort)(configZone[scOffset] | (configZone[scOffset + 1] << 8));

            int kcOffset = KeyConfigOffset + (slotNumber * 2);
            ushort keyConfig = (ushort)(configZone[kcOffset] | (configZone[kcOffset + 1] << 8));

            return new Atecc608SlotConfig(slotNumber, slotConfig, keyConfig);
        }
    }
}
