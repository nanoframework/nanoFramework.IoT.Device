// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Atecc608
{
    /// <summary>
    /// ATECC608 command opcodes sent to the device.
    /// </summary>
    internal enum Atecc608Command : byte
    {
        /// <summary>Returns device state information.</summary>
        Info = 0x30,

        /// <summary>Reads data from the specified zone.</summary>
        Read = 0x02,

        /// <summary>Writes data to the specified zone.</summary>
        Write = 0x12,

        /// <summary>Locks the configuration, data, or OTP zone.</summary>
        Lock = 0x17,

        /// <summary>Generates a random number.</summary>
        Random = 0x1B,

        /// <summary>Computes a SHA-256 digest.</summary>
        Sha = 0x47,

        /// <summary>Generates a nonce for subsequent commands.</summary>
        Nonce = 0x16,

        /// <summary>Generates an ECC key pair or computes the public key from an existing private key.</summary>
        GenKey = 0x40,

        /// <summary>Creates an ECDSA signature using a private key stored in a slot.</summary>
        Sign = 0x41,

        /// <summary>Verifies an ECDSA signature against a public key.</summary>
        Verify = 0x45,

        /// <summary>Performs ECDH key agreement using a private key stored in a slot.</summary>
        Ecdh = 0x43,

        /// <summary>Computes a SHA-256 based MAC using a stored key.</summary>
        Mac = 0x08,

        /// <summary>Performs AES-128 encrypt or decrypt operations.</summary>
        Aes = 0x51,

        /// <summary>Reads or increments a monotonic counter.</summary>
        Counter = 0x24,

        /// <summary>Performs a device self-test of the cryptographic engines.</summary>
        SelfTest = 0x77,
    }
}
