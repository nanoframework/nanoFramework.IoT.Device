// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Atecc608;
using nanoFramework.Hardware.Esp32;

Debug.WriteLine("ATECC608 CryptoAuthentication Sample");
Debug.WriteLine("=====================================");

//////////////////////////////////////////////////////////////////////
// When connecting to an ESP32 device, need to configure the I2C GPIOs
// used for the bus.
Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);

// Use the M5Stack Core2 for AWS address (0x35).
// For other boards, use Atecc608Device.DefaultI2cAddress (0x60).
// IMPORTANT: The I2C bus must run at 100 kHz for reliable wake-up.
// At higher speeds the SDA-low wake pulse may be too short (tWLO >= 60 us required).
I2cConnectionSettings settings = new(1, Atecc608Device.M5StackI2cAddress);
using I2cDevice i2c = I2cDevice.Create(settings);

// The constructor automatically creates an internal I2C device at address 0x00
// (General Call) on the same bus to generate the correct wake pulse.
using Atecc608Device atecc = new(i2c);

// Wake the device.
atecc.Wake();
Debug.WriteLine("Device woken up successfully.");

// Read serial number.
byte[] serial = atecc.GetSerialNumber();
Debug.Write("Serial Number: ");
for (int i = 0; i < serial.Length; i++)
{
    Debug.Write(serial[i].ToString("X2"));
    if (i < serial.Length - 1)
    {
        Debug.Write(" ");
    }
}

Debug.WriteLine(string.Empty);

// Read revision number.
byte[] revision = atecc.GetRevisionNumber();
Debug.Write("Revision: ");
for (int i = 0; i < revision.Length; i++)
{
    Debug.Write(revision[i].ToString("X2"));
    if (i < revision.Length - 1)
    {
        Debug.Write(" ");
    }
}

Debug.WriteLine(string.Empty);

// Check lock states.
Debug.WriteLine($"Config zone locked: {atecc.IsConfigLocked}");
Debug.WriteLine($"Data zone locked:   {atecc.IsDataLocked}");

// Generate a random number.
byte[] random = atecc.GetRandomNumber();
Debug.Write("Random (32 bytes): ");
for (int i = 0; i < random.Length; i++)
{
    Debug.Write(random[i].ToString("X2"));
}

Debug.WriteLine(string.Empty);

// Compute SHA-256 of a test message ("Hello" as ASCII bytes).
byte[] message = new byte[] { 0x48, 0x65, 0x6C, 0x6C, 0x6F };
byte[] hash = atecc.ComputeSha256(message);
Debug.Write("SHA-256 digest:    ");
for (int i = 0; i < hash.Length; i++)
{
    Debug.Write(hash[i].ToString("X2"));
}

Debug.WriteLine(string.Empty);

// --- Phase 2: Asymmetric Cryptography (ECDSA / ECDH) ---
Debug.WriteLine(string.Empty);
Debug.WriteLine("=== ECC Key Generation & ECDSA ===");

// Generate a new ECC P256 private key in slot 0.
// WARNING: This overwrites any existing key in the slot and is irreversible if the slot is locked.
byte[] publicKey = atecc.GeneratePrivateKey(0);
Debug.Write("Public Key X: ");
for (int i = 0; i < 32; i++)
{
    Debug.Write(publicKey[i].ToString("X2"));
}

Debug.WriteLine(string.Empty);
Debug.Write("Public Key Y: ");
for (int i = 32; i < 64; i++)
{
    Debug.Write(publicKey[i].ToString("X2"));
}

Debug.WriteLine(string.Empty);

// Sign a digest with the private key in slot 0.
// First compute SHA-256 of a test message, then sign the resulting digest.
byte[] signMessage = new byte[] { 0x54, 0x65, 0x73, 0x74 }; // "Test" in ASCII
byte[] digest = atecc.ComputeSha256(signMessage);
byte[] signature = atecc.SignDigest(0, digest);
Debug.Write("Signature R:  ");
for (int i = 0; i < 32; i++)
{
    Debug.Write(signature[i].ToString("X2"));
}

Debug.WriteLine(string.Empty);
Debug.Write("Signature S:  ");
for (int i = 32; i < 64; i++)
{
    Debug.Write(signature[i].ToString("X2"));
}

Debug.WriteLine(string.Empty);

// Verify the signature using the public key.
bool isValid = atecc.VerifyDigest(publicKey, digest, signature);
Debug.WriteLine($"Signature valid: {isValid}");

// Retrieve the public key from the stored private key (should match).
byte[] publicKey2 = atecc.GetPublicKey(0);
Debug.Write("Retrieved PK:  ");
for (int i = 0; i < 64; i++)
{
    Debug.Write(publicKey2[i].ToString("X2"));
}

Debug.WriteLine(string.Empty);

// --- Phase 3: Symmetric Crypto, Counters & Self-Test ---
Debug.WriteLine(string.Empty);
Debug.WriteLine("=== Self-Test & Counters ===");

// Run the device self-test (RNG, ECC, AES, SHA).
bool selfTestPassed = atecc.RunSelfTest();
Debug.WriteLine($"Self-test passed: {selfTestPassed}");

// Read monotonic counter 0.
int counterValue = atecc.ReadCounter(0);
Debug.WriteLine($"Counter 0 value: {counterValue}");

// Read slot 0 configuration.
Debug.WriteLine(string.Empty);
Debug.WriteLine("=== Slot Configuration ===");
Atecc608SlotConfig slotCfg = atecc.GetSlotConfiguration(0);
Debug.WriteLine($"Slot 0: KeyType={slotCfg.KeyType}, IsPrivate={slotCfg.IsPrivate}, IsSecret={slotCfg.IsSecret}, Lockable={slotCfg.Lockable}");

// Put device to sleep.
atecc.Sleep();
Debug.WriteLine("Device is now asleep.");

Thread.Sleep(Timeout.Infinite);
