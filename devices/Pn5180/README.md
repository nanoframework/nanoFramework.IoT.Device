# PN5180 - RFID and NFC reader

PN5180 is a RFID and NFC reader. It does supports various standards: ISO/IEC 14443 TypeA, ISO/IEC 14443 TypeB, ISO/IEC 15693 and ISO/IEC 18000-3 Mode 3. It does supports up to 848 kBit/s communication with 14443 type A cards.

## Documentation

Official documentation can be fond [here](https://www.nxp.com/docs/en/data-sheet/PN5180A0XX_C3_C4.pdf)

Application note on how to operate PN5180 without a [library](https://www.nxp.com/docs/en/application-note/AN12650.pdf)

## Board

You will find different implementation of this board. All boards should have full SPI pins plus the reset and busy ones and additionally 5V and or 3.3V plus ground.

**Important**: The PN5180 board requires **both** 3.3V and 5V power supplies to be connected:

- **3.3V** powers the PN5180 IC itself (digital logic and SPI interface).
- **5V** powers the RF transmitter output stage (antenna driver).

Without 5V connected, SPI communication will work (EEPROM reads, firmware version queries), but the antenna will not generate a strong enough RF field to power or communicate with cards.

## Usage

**Important**: make sure you properly setup the SPI pins especially for ESP32 before creating the `SpiDevice`, make sure you install the `nanoFramework.Hardware.ESP32 nuget`:

```csharp
//////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the SPI GPIOs
// used for the bus
Configuration.SetPinFunction(21, DeviceFunction.SPI1_MOSI);
Configuration.SetPinFunction(22, DeviceFunction.SPI1_MISO);
Configuration.SetPinFunction(23, DeviceFunction.SPI1_CLOCK);
// Make sure as well you are using the right chip select
```

For other devices like STM32, please make sure you're using the preset pins for the SPI bus you want to use. The chip select can as well be pre setup.

You will find a full example in the [samples directory](./samples/Program.cs). This example covers the usage of most of the public functions and properties. This example shows as well how to use [Ultralight cards](../Card/Ultralight).

PN5180 is operated thru SPI and GPIO. GPIO is used to control the SPI behavior as the PN5180 is using SPI in specific way. This does then require to manually manage the pin selection for SPI. And another pin called pin busy is used to understand when the PN5180 is available to receive and send information.

The following code shows how to create a SPI driver, reset the PN5180 and create the class.

```csharp
// Note: the chip select used here won't be used by the module, so don't use the same pin
var spi = SpiDevice.Create(new SpiConnectionSettings(1, 12) { ClockFrequency = Pn5180.SpiClockFrequency, Mode = Pn5180.SpiMode, DataFlow = DataFlow.MsbFirst });

// Reset the device
var gpioController = new GpioController();
gpioController.OpenPin(4, PinMode.Output);
gpioController.Write(4, PinValue.Low);
Thread.Sleep(10);
gpioController.Write(4, PinValue.High);
Thread.Sleep(10);

var pn5180 = new Pn5180(spi, 27, 18);
```

You will note that the SPI maximum clock frenquency is preset with ```Pn5180.MaximumSpiClockFrequency```, the maximum operation frequency is 7MHz. Same for the mode thru ```Pn5180.DefaultSpiMode```. Data Flow has to be ```DataFlow.MsbFirst```.

In the previous example the pin 2 is used for busy and the pin 3 for the SPI selection. Note that you have to use a specific pin selection and cannot use the one which is associate with the SPI channel you create. 

Reset is done thru pin 4. It is recommended to reset the board before creating the class.

Once created, you then need to select a card before you can actually exchange data with the card. Here is how to do it for an ISO 14443 Type A card:

```csharp
Data106kbpsTypeA cardTypeA;
do
{
    // This will try to select the card for 1 second and will wait 300 milliseconds before trying again if none is found
    var retok = _pn5180.ListenToCardIso14443TypeA(TransmitterRadioFrequencyConfiguration.Iso14443A_Nfc_PI_106_106, ReceiverRadioFrequencyConfiguration.Iso14443A_Nfc_PI_106_106, out cardTypeA, 1000);
    if (retok)
    {
        Debug.WriteLine($"ISO 14443 Type A found:");
        Debug.WriteLine($"  ATQA: {cardTypeA.Atqa}");
        Debug.WriteLine($"  SAK: {cardTypeA.Sak}");
        Debug.WriteLine($"  UID: {BitConverter.ToString(cardTypeA.NfcId)}");
        // This is where you do something with the card
    }
    else
    {
        Thread.Sleep(300);
    }
}
while (true);
```

And for an ISO 14443 Type B card:

```csharp
Data106kbpsTypeB card;

do
{
    // This will try to select the card for 1 second, if no card detected wait for 300 milliseconds and try again
    retok = _pn5180.ListenToCardIso14443TypeB(TransmitterRadioFrequencyConfiguration.Iso14443B_106, ReceiverRadioFrequencyConfiguration.Iso14443B_106, out card, 1000);
    if (!retok)
    {
        Thread.Sleep(300);
        continue;
    }

    Debug.WriteLine($"ISO 14443 Type B found:");
    Debug.WriteLine($"  Target number: {card.TargetNumber}");
    Debug.WriteLine($"  App data: {BitConverter.ToString(card.ApplicationData)}");
    Debug.WriteLine($"  App type: {card.ApplicationType}");
    Debug.WriteLine($"  UID: {BitConverter.ToString(card.NfcId)}");
    Debug.WriteLine($"  Bit rates: {card.BitRates}");
    Debug.WriteLine($"  Cid support: {card.CidSupported}");
    Debug.WriteLine($"  Command: {card.Command}");
    Debug.WriteLine($"  Frame timing: {card.FrameWaitingTime}");
    Debug.WriteLine($"  Iso 14443-4 compliance: {card.ISO14443_4Compliance}");
    Debug.WriteLine($"  Max frame size: {card.MaxFrameSize}");
    Debug.WriteLine($"  Nad support: {card.NadSupported}");

    // Do something else, all operations you want with the card
    // Halt card
    if (_pn5180.DeselecCardTypeB(card))
    {
        Debug.WriteLine($"Card unselected properly");
    }
    else
    {
        Debug.WriteLine($"ERROR: Card can't be unselected");
    }
}
while (true);
```

Please note that the ```ListenToCardIso14443TypeA``` and ```ListenToCardIso14443TypeB``` can be configured with different transceiver and receiver configurations. Usually the configuration need to match but you can adjust and change them. See the section with Radio Frequency configuration for more information.

A card will be continuously tried to be detected during the duration on your polling. If nothing is detected or if any issue, the function will return false.

Specific for type B cards, they have a target number. This target number is needed to transcieve any information with the card. The PN5180 can support up to 14 cards at the same time. But you can only select 1 card at a time, so if you have a need for multiple card selected at the same time, it is recommended to chain this card detection with the number of cards you need to select and operate at the same time. Note that depending on the card, they may not been seen as still selected by the reader.

You should deselect the Type B card at the end to release the target number. If not done, during the next poll, this implementation will test if the card is still present, keep it in this case.

## ISO 15693 (Vicinity Cards)

The PN5180 supports ISO 15693 (NFC-V) cards such as NXP ICODE SLIX, SLIX2, DNA, and ICODE 3. These cards operate at 13.56 MHz with a longer read range than ISO 14443 proximity cards.

### Detecting ISO 15693 cards

Use `ListenToCardIso15693` to perform a 16-slot inventory and detect one or more ISO 15693 cards:

```csharp
ArrayList cards;
do
{
    if (pn5180.ListenToCardIso15693(
        TransmitterRadioFrequencyConfiguration.Iso15693_ASK100_26,
        ReceiverRadioFrequencyConfiguration.Iso15693_26,
        out cards,
        1000))
    {
        foreach (Data26_53kbps card in cards)
        {
            Debug.WriteLine($"ISO 15693 card found:");
            Debug.WriteLine($"  UID: {BitConverter.ToString(card.NfcId)}");
            Debug.WriteLine($"  DSFID: 0x{card.Dsfid:X2}");
        }
    }
    else
    {
        Thread.Sleep(300);
    }
}
while (true);
```

### Reading and writing ICODE cards

Once a card is detected, create an `IcodeCard` instance to perform read/write operations:

```csharp
// Assuming detectedCard is a Data26_53kbps from ListenToCardIso15693
pn5180.ResetPN5180Configuration(
    TransmitterRadioFrequencyConfiguration.Iso15693_ASK100_26,
    ReceiverRadioFrequencyConfiguration.Iso15693_26);

var icodeCard = new IcodeCard(pn5180, detectedCard.TargetNumber)
{
    Uid = detectedCard.NfcId,
    Capacity = IcodeCardCapacity.Unknown
};

// Get system information
icodeCard.GetSystemInformation();

// Read a single block
icodeCard.ReadSingleBlock(0);
Debug.WriteLine($"Block 0: {BitConverter.ToString(icodeCard.Data)}");

// Write a single block
icodeCard.Data = new byte[] { 0x01, 0x02, 0x03, 0x04 };
icodeCard.WriteSingleBlock(0);

// Read multiple blocks at once
icodeCard.ReadMultipleBlocks(0, 4);
```

### Switching between ISO 14443 and ISO 15693

When switching between ISO 14443 (Type A/B) and ISO 15693 cards, use `ResetPN5180Configuration` to reload the appropriate RF configuration:

```csharp
// Switch to ISO 15693 mode
pn5180.ResetPN5180Configuration(
    TransmitterRadioFrequencyConfiguration.Iso15693_ASK100_26,
    ReceiverRadioFrequencyConfiguration.Iso15693_26);

// ... do ISO 15693 operations ...

// Switch back to ISO 14443 Type A mode
pn5180.ResetPN5180Configuration(
    TransmitterRadioFrequencyConfiguration.Iso14443A_Nfc_PI_106_106,
    ReceiverRadioFrequencyConfiguration.Iso14443A_Nfc_PI_106_106);
```

See the [Icode card documentation](../Card/Icode/README.md) for more details on the supported ISO 15693 commands.

## EEPROM

You can fully access the PN5180 EEPROM. Here is an example on how to do it:

```csharp
// Maximum size of the EEPROM
Span<byte> eeprom = stackalloc byte[255];
// This will read fully the EEPROM
var ret = _pn5180.ReadAllEeprom(eeprom);
Debug.WriteLine($"EEPROM dump: success: {ret}, Data: {BitConverter.ToString(eeprom.ToArray())}");
// This reads only the unique Identifier
ret = _pn5180.ReadEeprom(EepromAddress.DieIdentifier, eeprom.Slice(0, 16));
Debug.WriteLine($"EEPROM read, unique identifier: success: {ret}, Data: {BitConverter.ToString(eeprom.Slice(0, 16).ToArray())}");
// Same as above
ret = _pn5180.GetIdentifier(eeprom.Slice(0, 16));
// So you should see the exact same result than from reading manully the 16 bytes of the unique identifier
Debug.WriteLine($"GetIdentifier: success: {ret}, Data: {BitConverter.ToString(eeprom.Slice(0, 16).ToArray())}");
// This tries to write in a read only part of the EEPROM
ret = _pn5180.WriteEeprom(EepromAddress.DieIdentifier, eeprom.Slice(0, 1));
// So you'll receive false as an answer from the PN5180
Debug.WriteLine($"Trying to write a read only EEPROM, this should return false: {ret}");
// This is important to understand, if you write in the EEPROM and then try to read right after,
// in most of the cases, the value won't change. After a reboot, you'll get the new value
Debug.WriteLine($"EEPROM writing will not be immediate. Some are only active after a reboot");
Debug.WriteLine($"changing second byte of UUID when acting as a card (first is always fix to 0x08)");
ret = _pn5180.ReadEeprom(EepromAddress.NFCID1, eeprom.Slice(0, 3));
eeprom[0]++;
Debug.WriteLine($"IRQ_PIN_CONFIG: success: {ret}, Data: {BitConverter.ToString(eeprom.Slice(0, 3).ToArray())}");
Debug.WriteLine($"New value to write: {BitConverter.ToString(eeprom.Slice(0, 1).ToArray())}");
ret = _pn5180.WriteEeprom(EepromAddress.NFCID1, eeprom.Slice(0, 3));
Debug.WriteLine($"Wrote IRQ_PIN_CONFIG: {ret}");
ret = _pn5180.ReadEeprom(EepromAddress.NFCID1, eeprom.Slice(0, 3));
Debug.WriteLine($"IRQ_PIN_CONFIG: success: {ret}, Data: {BitConverter.ToString(eeprom.Slice(0, 3).ToArray())}");
```

Functions has been implemented to read and write part or all the EEPROM. You need to be careful of the size of the buffer, it can't exceed 255 bytes and can't be larger than the base address you want to write and total size. So if you write at position 250, your buffer size and only be 5 maximum.

## PN5180 versions

You can retreive the PN5180 version thru the ```GetVersion``` function. 3 versions will be returned, the product, firmware and EEPROM ones.

```csharp
var versions = _pn5180.GetVersion();
Debug.WriteLine($"Product: {versions.Product.ToString()}, Firmware: {versions.Firmware.ToString()}, EEPROM: {versions.Eeprom.ToString()}");
```

You should see something like this:

```
Product: 3.5, Firmware: 3.5, EEPROM: 145.0
```

Current firmware versions are 3.12 (3.C) and 4.0. That said, this implementation supports older firmware. Newer firmware have better support for auto calibration, fixes bugs and added specific EMVco (payment) low level features. Note that the product version is the original firmware version installed. so if you've done firmware upgrade, the product version will always remain the one from the original firmware.

Note that this implementation does not support firmware update. You should use NXP tools if you want to update the firmare

## Radio Frequency Configuration

The PN5180 offers the possibility to set a lot of configurations. The good news is that those configurations are stored and can be loaded. You can adjust them as well. The following code shows an example on how to load, extract the configuration and with the same way, you can write back a configuration if you need. Please refer to the documentation in this case to understand the changes you want to make:

```csharp
// Number of configuration
var sizeConfig = _pn5180.GetRadioFrequencyConfigSize(TransmitterRadioFrequencyConfiguration.Iso14443B_106);
// The RadioFrequencyConfiguraitonSize is 5, 1 for the register and 4 for the register data
SpanByte configBuff = new byte[Pn5180.RadioFrequencyConfiguraitonSize * sizeConfig];
var ret = _pn5180.RetrieveRadioFrequencyConfiguration(TransmitterRadioFrequencyConfiguration.Iso14443B_106, configBuff);
for (int i = 0; i < sizeConfig; i++)
{
    Debug.WriteLine($"Register: {configBuff[Pn5180.RadioFrequencyConfiguraitonSize * i]}, Data: {BitConverter.ToString(configBuff.Slice(Pn5180.RadioFrequencyConfiguraitonSize * i + 1, Pn5180.RadioFrequencyConfiguraitonSize - 1).ToArray())}");
}
```

Every configuration has the size of 5 bytes, first byte is the register number, and the next 4 are the data them selves.

## Transceive data with a card

Once the card is selected properly, you can use the CardTranscive class to exchange data with the card. See [Mifare](../Card/Mifare/README.md) and [Ultralight](../Card/Ultralight/README.md) for detailed examples.

## Card Emulation Mode

The PN5180 can operate as a passive NFC target (card emulation) using its built-in **Autocoll** engine. In this mode an external reader powers the PN5180 via its RF field and the chip automatically handles anti-collision, making it appear as an ISO 14443-A (or NFC-F) card.

### EEPROM identity fields

Before entering Autocoll the emulated card identity must be written to EEPROM:

| Field | EEPROM address | Size | Helper method | Description |
| --- | --- | --- | --- | --- |
| SENS_RES (ATQA) | 0x40 | 2 bytes | `SetSensRes(byte, byte)` | Anticollision / technology descriptor |
| NFCID1 (UID) | 0x42 | 3 bytes | `SetNfcId1(byte[])` | UID bytes 1–3; byte 0 is fixed to 0x08 |
| SEL_RES (SAK) | 0x45 | 1 byte | `SetSelRes(byte)` | Selection acknowledge (0x20 = ISO 14443-4) |
| FeliCa Polling Resp | 0x46 | ≤10 bytes | `SetFelicaPollingResponse(byte[])` | Only needed for NFC-F emulation |

### Minimal example

```csharp
// 1. Configure emulated card identity
pn5180.SetSensRes(0x44, 0x00);                    // ATQA
pn5180.SetNfcId1(new byte[] { 0xCA, 0xFE, 0x01 }); // UID bytes 1-3
pn5180.SetSelRes(0x20);                            // SAK (ISO 14443-4)

// 2. Enter Autocoll – listen for NFC-A readers
pn5180.SwitchToAutocoll(AutocollMode.CollisionResolutionNfcA);

// 3. Wait for a reader to activate us (30 s timeout)
var activation = pn5180.WaitForActivation(30_000);
if (activation != null)
{
    Debug.WriteLine($"Activated via {activation.ActivatedProtocol}");

    // 4. Respond to RATS with a minimal ATS
    byte[] ats = new byte[] { 0x05, 0x70, 0x80, 0x80, 0x00 };
    SpanByte rx = new byte[256];
    int len = pn5180.TransceiveTargetMode(new SpanByte(ats), rx, 5_000);

    // 5. Exchange frames until the reader leaves
    while (len > 0)
    {
        // Echo received data back (replace with real APDU handling)
        len = pn5180.TransceiveTargetMode(rx.Slice(0, len), rx, 5_000);
    }
}

// 6. Return to normal reader mode
pn5180.ExitCardEmulationMode();
```

See [samples/Program.cs](./samples/Program.cs) for a complete working example (`CardEmulation()` method).

### Limitations

- **Single-size UID only** – Autocoll supports a 4-byte UID (single size). Double- or triple-size UIDs are not available in this mode.
- **Low-level activation only** – The PN5180 handles REQA/WUPA, anticollision and selection automatically; higher-layer protocols (ISO-DEP T=CL, NDEF, HCE APDU routing) must be implemented in user code.
- **No Mifare Classic emulation** – The PN5180 cannot perform Crypto1 authentication as a target.
- **Optional IRQ pin** – Pass `pinIrq` to the `Pn5180` constructor if your board wires the PN5180 IRQ line. This enables `WaitForIrq()` to detect interrupts via GPIO instead of continuous SPI polling.


This shows how to dump a Mifare (ISO 14443 type A) card fully:

```csharp
Data106kbpsTypeA cardTypeA;

// Let's pull for 20 seconds and see the result
var retok = _pn5180.ListenToCardIso14443TypeA(TransmitterRadioFrequencyConfiguration.Iso14443A_Nfc_PI_106_106, ReceiverRadioFrequencyConfiguration.Iso14443A_Nfc_PI_106_106, out cardTypeA, 20000);
Debug.WriteLine();

if (!retok)
{
    Debug.WriteLine("Can't read properly the card");
}
else
{
    Debug.WriteLine($"ATQA: {cardTypeA.Atqa}");
    Debug.WriteLine($"SAK: {cardTypeA.Sak}");
    Debug.WriteLine($"UID: {BitConverter.ToString(cardTypeA.NfcId)}");

    MifareCard mifareCard = new MifareCard(_pn5180, cardTypeA.TargetNumber) { BlockNumber = 0, Command = MifareCardCommand.AuthenticationA };
    mifareCard.SetCapacity(cardTypeA.Atqa, cardTypeA.Sak);
    mifareCard.SerialNumber = cardTypeA.NfcId;
    mifareCard.KeyA = new byte[6] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
    mifareCard.KeyB = new byte[6] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
    for (byte block = 0; block < 64; block++)
    {
        mifareCard.BlockNumber = block;
        mifareCard.Command = MifareCardCommand.AuthenticationB;
        var ret = mifareCard.RunMifiCardCommand();
        if (ret < 0)
        {
            // Try another one
            mifareCard.Command = MifareCardCommand.AuthenticationA;
            ret = mifareCard.RunMifiCardCommand();
        }

        if (ret >= 0)
        {
            mifareCard.BlockNumber = block;
            mifareCard.Command = MifareCardCommand.Read16Bytes;
            ret = mifareCard.RunMifiCardCommand();
            if (ret >= 0)
            {
                Debug.WriteLine($"Bloc: {block}, Data: {BitConverter.ToString(mifareCard.Data)}");
            }
            else
            {
                Debug.WriteLine($"Error reading bloc: {block}, Data: {BitConverter.ToString(mifareCard.Data)}");
            }

            if (block % 4 == 3)
            {
                // Check what are the permissions
                for (byte j = 3; j > 0; j--)
                {
                    var access = mifareCard.BlockAccess((byte)(block - j), mifareCard.Data);
                    Debug.WriteLine($"Bloc: {block - j}, Access: {access}");
                }

                var sector = mifareCard.SectorTailerAccess(block, mifareCard.Data);
                Debug.WriteLine($"Bloc: {block}, Access: {sector}");
            }
        }
        else
        {
            Debug.WriteLine($"Authentication error");
        }
    }
}
```

The [example](./samples/Program.cs) contains as well an implementation to fully dump the content of other cards.

## Current implementation

Communication support:

- [X] Hardware SPI Controller fully supported
- [X] GPIO Controller fully supported

Miscellaneous:

- [X] Read fully EEPROM
- [X] Write fully EEPROM
- [X] Read any part of EEPROM
- [X] Write any part of EEPROM
- [X] Get product, hardware and firmware versions
- [X] CardTransceive support to reuse existing [Mifare](../Card/Mifare/README.md), [Icode (ISO 15693)](../Card/Icode/README.md) and [Credit Card](../Card/CreditCard/README.md), ISO 14443 support Type A or Type B protocol, ISO 15693 support
- [ ] Secure firmware update
- [ ] Own board GPIO access

RF communication commands:

- [X] Load a specific configuration
- [X] Read a specific configuration
- [X] Write a specific configuration

PN5180 as an initiator (reader) commands:

- [X] Auto poll ISO 14443 type A cards
- [X] Auto poll ISO 14443 type B cards
- [X] Deselect ISO 14443 type B cards
- [X] Multi card support at the same time: partial, depending on the card, CID mandatory in all 14443 type B communications
- [X] ISO 14443-4 communication protocol
- [X] Auto poll ISO 15693 (NFC-V / Vicinity) cards
- [X] Communication support for ISO 15693 cards (ICODE SLIX, SLIX2, DNA, ICODE 3)
- [ ] Low power card detection
- [X] Mifare specific authentication
- [ ] Fast 212, 424, 848 kbtis communication: partial

PN5180 as a Target (acting like a card):

- [X] Initialization as target
- [X] Handling communication with another reader as a target
- [X] Support for transceive data
