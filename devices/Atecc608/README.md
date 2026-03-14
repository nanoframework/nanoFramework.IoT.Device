# ATECC608A/B - CryptoAuthentication Hardware Encryption Device

The [ATECC608](https://www.microchip.com/en-us/product/atecc608b) is a Microchip CryptoAuthentication™ secure element providing hardware-based key storage and cryptographic acceleration over I2C. It is the first hardware encryption chip binding in this repository.

## Documentation

### Supported Devices

| Device | Notes |
|---|---|
| [ATECC608A](https://www.microchip.com/en-us/product/atecc608b) | Original variant |
| [ATECC608B](https://www.microchip.com/en-us/product/atecc608b) | Enhanced security, backwards-compatible with 608A |
| ATECC608B-TNGTLSU (Trust&GO) | Pre-provisioned variant, used in M5Stack Core2 for AWS |

### Key Features

- Hardware-accelerated ECDSA sign/verify (NIST P256)
- ECDH key agreement
- SHA-256, HMAC-SHA256 computation
- AES-128 single-block encrypt/decrypt (ECB mode)
- Secure key storage for up to 16 keys/certificates
- Internal NIST-compliant random number generator (RNG)
- Two monotonic counters
- Unique 72-bit serial number
- I2C interface (up to 1 MHz)

### Datasheet

- [ATECC608B Summary Datasheet (HTML)](https://onlinedocs.microchip.com/g/GUID-5C346303-A9CB-4341-B67F-4354AB92FE04)
- [ATECC608B Summary Datasheet (PDF)](https://ww1.microchip.com/downloads/aemDocuments/documents/SCBU/ProductDocuments/DataSheets/ATECC608B-CryptoAuthentication-Device-Summary-Data-Sheet-DS40002239B.pdf)

## Usage

**Important**: Make sure you properly setup the I2C pins especially for ESP32 before creating the `I2cDevice`. Install the `nanoFramework.Hardware.ESP32` NuGet:

```csharp
//////////////////////////////////////////////////////////////////////
// When connecting to an ESP32 device, need to configure the I2C GPIOs
// used for the bus.
Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);
```

For other devices like STM32, use the preset pins for the I2C bus you want to use.

### Basic Setup

```csharp
// M5Stack Core2 for AWS uses address 0x35.
// For generic boards, use Atecc608Device.DefaultI2cAddress (0x60).
I2cConnectionSettings settings = new(1, Atecc608Device.M5StackI2cAddress);
I2cDevice i2c = I2cDevice.Create(settings);
Atecc608Device atecc = new(i2c);
```

### Wake, Read Info, Sleep

```csharp
atecc.Wake();

byte[] serial = atecc.GetSerialNumber();
byte[] revision = atecc.GetRevisionNumber();

bool configLocked = atecc.IsConfigLocked;
bool dataLocked = atecc.IsDataLocked;

atecc.Sleep();
```

### Generate Random Number

```csharp
atecc.Wake();
byte[] random = atecc.GetRandomNumber(); // returns 32 bytes
atecc.Sleep();
```

### Compute SHA-256 Hash

```csharp
atecc.Wake();
byte[] message = new byte[] { 0x48, 0x65, 0x6C, 0x6C, 0x6F }; // "Hello" in ASCII
byte[] hash = atecc.ComputeSha256(message); // returns 32-byte digest
atecc.Sleep();
```

### Read Configuration Zone

```csharp
atecc.Wake();
byte[] config = atecc.ReadConfigZone(); // returns 128 bytes
atecc.Sleep();
```

### Generate ECC P256 Key Pair

The ATECC608 generates a random private key inside the chip. The private key never leaves the device; only the 64-byte public key (X, Y) is returned.

```csharp
atecc.Wake();

// Generate a new private key in slot 0 and get the public key.
byte[] publicKey = atecc.GeneratePrivateKey(0);
// publicKey is 64 bytes: 32-byte X + 32-byte Y

// Retrieve the public key for an existing private key.
byte[] pk = atecc.GetPublicKey(0);

atecc.Sleep();
```

### ECDSA Sign and Verify

Sign a SHA-256 digest and verify the signature using an external public key.

```csharp
atecc.Wake();

// Hash the message on-chip.
byte[] message = new byte[] { 0x54, 0x65, 0x73, 0x74 }; // "Test"
byte[] digest = atecc.ComputeSha256(message);

// Sign the digest with the private key in slot 0.
byte[] signature = atecc.SignDigest(0, digest);
// signature is 64 bytes: 32-byte R + 32-byte S

// Verify the signature against the public key.
byte[] publicKey = atecc.GetPublicKey(0);
bool valid = atecc.VerifyDigest(publicKey, digest, signature);

atecc.Sleep();
```

For more control, load the digest into TempKey separately:

```csharp
atecc.Wake();

atecc.LoadNonce(digest);          // load digest into TempKey
byte[] sig = atecc.Sign(0);       // sign TempKey contents

atecc.LoadNonce(digest);          // reload for verify
bool ok = atecc.Verify(publicKey, sig);

atecc.Sleep();
```

### ECDH Key Agreement

Perform an Elliptic-Curve Diffie-Hellman key exchange using an on-chip private key and the other party's public key.

```csharp
atecc.Wake();

byte[] otherPartyPublicKey = ...; // 64-byte public key from the peer
byte[] sharedSecret = atecc.Ecdh(0, otherPartyPublicKey);
// sharedSecret is 32 bytes (X coordinate of the ECDH point)

atecc.Sleep();
```

### AES-128 Encrypt and Decrypt

Encrypt and decrypt a 16-byte block using AES-128 ECB. The slot must be configured with KeyType = AES and contain a 16-byte key.

```csharp
atecc.Wake();

// Encrypt a 16-byte block using the AES key in slot 9.
byte[] plaintext = new byte[16] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F };
byte[] ciphertext = atecc.AesEncrypt(9, plaintext);

// Decrypt the ciphertext back.
byte[] decrypted = atecc.AesDecrypt(9, ciphertext);

atecc.Sleep();
```

### MAC (Challenge-Response Authentication)

Compute a SHA-256 based MAC using a stored key and a 32-byte challenge. Useful for challenge-response authentication.

```csharp
atecc.Wake();

byte[] challenge = atecc.GetRandomNumber(); // 32-byte random challenge
byte[] mac = atecc.ComputeMac(4, challenge); // MAC using key in slot 4
// mac is 32 bytes

atecc.Sleep();
```

### Monotonic Counters

The ATECC608 has two monotonic counters (index 0 and 1), each with a maximum value of 2,097,151. Counter increments are irreversible.

```csharp
atecc.Wake();

int value = atecc.ReadCounter(0);       // read counter 0
int newValue = atecc.IncrementCounter(0); // increment and read new value

atecc.Sleep();
```

### Device Self-Test

Run the built-in self-test for all cryptographic engines (RNG, ECC, AES, SHA).

```csharp
atecc.Wake();
bool passed = atecc.RunSelfTest();
atecc.Sleep();
```

### Slot Configuration

Read and parse the configuration of any slot. Returns `SlotConfig` and `KeyConfig` field values.

```csharp
atecc.Wake();

Atecc608SlotConfig cfg = atecc.GetSlotConfiguration(0);
bool isEcc = cfg.KeyType == 4;   // P256 ECC key
bool isAes = cfg.KeyType == 6;   // AES key
bool isPrivate = cfg.IsPrivate;
bool lockable = cfg.Lockable;

// Or parse from a pre-read config zone dump:
byte[] configZone = atecc.ReadConfigZone();
Atecc608SlotConfig cfg2 = Atecc608SlotConfig.Parse(1, configZone);

atecc.Sleep();
```

## I2C Addresses

| Board / Variant | I2C Address (7-bit) |
|---|---|
| Factory default (unconfigured) | 0x60 |
| M5Stack Core2 for AWS (ATECC608B-TNGTLSU-G) | 0x35 |

## Hardware Use Case: M5Stack Core2 for AWS IoT EduKit

The [M5Stack Core2 ESP32 IoT Development Kit for AWS](https://shop.m5stack.com/products/m5stack-core2-esp32-iot-development-kit-for-aws-iot-edukit) integrates an ATECC608B-TNGTLSU-G (Trust&GO) chip at I2C address 0x35. This enables hardware-level security for IoT cloud communication with AWS IoT Core.

## Known Limitations

- **Lock operations are irreversible.** Once a configuration or data zone is locked, it cannot be unlocked.
- **Counter increments are irreversible.** Each counter has a maximum value of 2,097,151.
- Trust&GO variants come pre-provisioned and pre-locked. Configuration zone modification is not available on these devices.
- The ECDH `Ecdh()` method returns the shared secret in clear text over I2C. For higher-security use cases, store the secret in a slot instead.
- AES operations use ECB mode (single 16-byte block). For multi-block encryption, use a software chaining mode (e.g., CBC) around `AesEncrypt`/`AesDecrypt`.
- TLS PRF/HKDF key derivation is not implemented — these require complex multi-step operations that are not practical on nanoFramework.
- Secure boot (SecureBoot command) requires device-specific configuration setup and is not yet exposed through this binding.
