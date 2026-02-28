# ICODE Card - ISO 15693 (NFC-V / Vicinity)

This library provides support for NXP ICODE and other ISO 15693 NFC-V (Vicinity) cards. ISO 15693 cards operate at 13.56 MHz with a longer read range than ISO 14443 proximity cards.

## Supported card types

| Card Type | Capacity (bits) | Enum Value |
|-----------|----------------|------------|
| ICODE SLIX | 896 | `IcodeCardCapacity.IcodeSlix` |
| ICODE SLIX2 | 2528 | `IcodeCardCapacity.IcodeSlix2` |
| ICODE DNA | 2016 | `IcodeCardCapacity.IcodeDna` |
| ICODE 3 | 2400 | `IcodeCardCapacity.Icode3` |

## Usage

The `IcodeCard` class requires a `CardTransceiver` (such as a [PN5180](../../Pn5180/README.md)) and a detected card UID.

### Detecting cards

First, detect an ISO 15693 card using the reader's inventory command. With the PN5180:

```csharp
ArrayList cards;
if (pn5180.ListenToCardIso15693(
    TransmitterRadioFrequencyConfiguration.Iso15693_ASK100_26,
    ReceiverRadioFrequencyConfiguration.Iso15693_26,
    out cards,
    2000))
{
    var detectedCard = (Data26_53kbps)cards[0];
    Debug.WriteLine($"UID: {BitConverter.ToString(detectedCard.NfcId)}");
}
```

### Creating an IcodeCard

```csharp
// Reset RF config for addressed-mode communication
pn5180.ResetPN5180Configuration(
    TransmitterRadioFrequencyConfiguration.Iso15693_ASK100_26,
    ReceiverRadioFrequencyConfiguration.Iso15693_26);

var icodeCard = new IcodeCard(pn5180, detectedCard.TargetNumber)
{
    Uid = detectedCard.NfcId,
    Capacity = IcodeCardCapacity.Unknown
};
```

### Getting system information

```csharp
if (icodeCard.GetSystemInformation())
{
    Debug.WriteLine($"DSFID: 0x{icodeCard.Dsfid:X2}");
    Debug.WriteLine($"AFI: 0x{icodeCard.Afi:X2}");
    Debug.WriteLine($"System info: {BitConverter.ToString(icodeCard.Data)}");
}
```

### Reading blocks

```csharp
// Read a single block
if (icodeCard.ReadSingleBlock(0))
{
    Debug.WriteLine($"Block 0: {BitConverter.ToString(icodeCard.Data)}");
}

// Read multiple blocks at once
if (icodeCard.ReadMultipleBlocks(0, 4))
{
    Debug.WriteLine($"Blocks 0-3: {BitConverter.ToString(icodeCard.Data)}");
}
```

### Writing blocks

```csharp
// Write a single block (block size depends on card type, typically 4 bytes)
icodeCard.Data = new byte[] { 0x01, 0x02, 0x03, 0x04 };
icodeCard.WriteSingleBlock(0);

// Write multiple blocks
icodeCard.Data = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 };
icodeCard.WriteMultipleBlocks(0);
```

### Locking blocks

```csharp
// Permanently lock a block (irreversible!)
icodeCard.LockBlock(0);
```

### Card state management

```csharp
// Make the card stop responding to inventory requests
icodeCard.StayQuiet();

// Select a specific card by UID for exclusive communication
icodeCard.Select(detectedCard.NfcId);

// Wake up all quiet cards
icodeCard.ResetToReady();
```

### AFI and DSFID management

```csharp
// Write Application Family Identifier
icodeCard.WriteAfi(0x01);

// Lock the AFI (irreversible!)
icodeCard.LockAfi();

// Write Data Storage Format Identifier
icodeCard.WriteDsfid(0x00);

// Lock the DSFID (irreversible!)
icodeCard.LockDsfid();
```

### Block security status

```csharp
// Get lock status of multiple blocks
if (icodeCard.GetMultipleBlockSecurityStatus(0, 4))
{
    // icodeCard.Data contains the security status bytes
    Debug.WriteLine($"Security: {BitConverter.ToString(icodeCard.Data)}");
}
```

## Supported ISO 15693 commands

| Command | Method | Description |
|---------|--------|-------------|
| Read Single Block (0x20) | `ReadSingleBlock()` | Read one block |
| Write Single Block (0x21) | `WriteSingleBlock()` | Write one block |
| Lock Block (0x22) | `LockBlock()` | Permanently lock a block |
| Read Multiple Blocks (0x23) | `ReadMultipleBlocks()` | Read consecutive blocks |
| Write Multiple Blocks (0x24) | `WriteMultipleBlocks()` | Write consecutive blocks |
| Stay Quiet (0x02) | `StayQuiet()` | Card stops responding to inventory |
| Select (0x25) | `Select()` | Select a specific card |
| Reset to Ready (0x26) | `ResetToReady()` | Wake up quiet cards |
| Write AFI (0x27) | `WriteAfi()` | Write Application Family Identifier |
| Lock AFI (0x28) | `LockAfi()` | Permanently lock AFI |
| Write DSFID (0x29) | `WriteDsfid()` | Write Data Storage Format Identifier |
| Lock DSFID (0x2A) | `LockDsfid()` | Permanently lock DSFID |
| Get System Information (0x2B) | `GetSystemInformation()` | Get card capabilities |
| Get Multiple Block Security Status (0x2C) | `GetMultipleBlockSecurityStatus()` | Get block lock status |

## Reader support

Currently supported reader for ISO 15693:

- [PN5180](../../Pn5180/README.md) — full ISO 15693 support including 16-slot inventory
