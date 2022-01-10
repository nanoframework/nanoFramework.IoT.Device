# Data Exchange Format (NDEF) read and write support for NFC cards

This library supports [NDEF messages](https://nfc-forum.org/product/nfc-data-exchange-format-ndef-technical-specification/). NDEF is composed of a message with records in it. Every record can be a know type or a specific type. This library fully support all root type of messages.

NDEF messages are used on Mifare Cards and is included into this library as well. You have a full example using 23 different NFC readers [MFRC522](../../Mfrc522), [PN532](../../Pn532/README.md) and [PN1850](../../Pn1850/README.md) build in.

## Reading NDEF from a card

This operation only require a valid NFC reader which implement `CardTransceiver`, see [the class](../CardTransceiver.cs).

From the code below, the transceiver is a PN532 used with a serial port on Windows. This is just for convenience, it can be any supported reader connected on any interface and any OS.

This is a complete example from initializing the reader, detecting the card, extracting the message and displaying the detailed messages.

**Important**: NDEF is supported for both [Mifare](../Mifare) and [Ultralight](../Ultralight) cards.

```csharp
// Create a PN532
var pn532 = new Pn532("COM4", debugLevel);
byte[]? retData = null;
while (true)
{
    retData = pn532.ListPassiveTarget(MaxTarget.One, TargetBaudRate.B106kbpsTypeA);
    if (retData is object)
    {
        break;
    }

    // Give time to PN532 to process
    Thread.Sleep(200);
}

if (retData is null)
{
    return;
}

Debug.WriteLine();

// Check if it is a valid card
var card = pn532.TryDecode106kbpsTypeA(retData.AsSpan().Slice(1));
if (card is not object)
{
    Debug.WriteLine("Can't read properly the card");
    return;
}

// Create the Mifare card
MifareCard mifareCard = new MifareCard(pn532, card.TargetNumber) { BlockNumber = 0, Command = MifareCardCommand.AuthenticationA };
mifareCard.SetCapacity(card.Atqa, card.Sak);
mifareCard.SerialNumber = card.NfcId;
// Read an extract the NDEF message
// This is where you can write as well, format the card, check the card see next sections
mifareCard.TryReadNdefMessage(out NdefMessage message);

if (message.Records.Count == 0)
{
    Debug.WriteLine("Sorry, there is no NDEF message in this card or I can't find them");
}

// Display the messages
foreach (NdefRecord msg in message.Records)
{
    Debug.WriteLine("Record header:");
    Debug.WriteLine($"  Is first message: {msg.Header.IsFirstMessage}, is last message: {msg.Header.IsLastMessage}");
    Debug.Write($"  Type name format: {msg.Header.TypeNameFormat}");
    if (msg.Header.PayloadType is object)
    {
        Debug.WriteLine($", Payload type: {BitConverter.ToString(msg.Header.PayloadType)}");
    }
    else
    {
        Debug.WriteLine("");
    }

    Debug.WriteLine($"  Is composed: {msg.Header.IsComposedMessage}, is Id present: {msg.Header.MessageFlag.HasFlag(MessageFlag.IdLength)}, Id Length value: {msg.Header.IdLength}");
    Debug.WriteLine($"  Payload Length: {msg.Payload?.Length}, is short message= {msg.Header.MessageFlag.HasFlag(MessageFlag.ShortRecord)}");

    if (msg.Payload is object)
    {
        Debug.WriteLine($"Payload: {BitConverter.ToString(msg.Payload)}");
    }
    else
    {
        Debug.WriteLine("No payload");
    }

    if (UriRecord.IsUriRecord(msg))
    {
        var urirec = new UriRecord(msg);
        Debug.WriteLine($"  Type {nameof(UriRecord)}, Uri Type: {urirec.UriType}, Uri: {urirec.Uri}, Full URI: {urirec.FullUri}");
    }

    if (TextRecord.IsTextRecord(msg))
    {
        var txtrec = new TextRecord(msg);
        Debug.WriteLine($"  Type: {nameof(TextRecord)}, Encoding: {txtrec.Encoding}, Language: {txtrec.LanguageCode}, Text: {txtrec.Text}");
    }

    if (GeoRecord.IsGeoRecord(msg))
    {
        var geo = new GeoRecord(msg);
        Debug.WriteLine($"  Type: {nameof(GeoRecord)}, Lat: {geo.Latitude}, Long: {geo.Longitude}");
    }

    if (MediaRecord.IsMediaRecord(msg))
    {
        var media = new MediaRecord(msg);
        Debug.WriteLine($"  Type: {nameof(MediaRecord)}, Payload Type = {media.PayloadType}");
        if (media.IsTextType)
        {
            var ret = media.TryGetPayloadAsText(out string payloadAsText);
            if (ret)
            {
                Debug.WriteLine($"    Payload as Text:");
                Debug.WriteLine($"{payloadAsText}");
            }
            else
            {
                Debug.WriteLine($"Can't convert the payload as a text");
            }
        }
    }

    Debug.WriteLine("");
}
```

## Writing NDEF to a card

From the previous example, you will still need to get a card. From there, to create and write messages, it's quite straight forward:

```csharp
// Create the NDEF message
NdefMessage message = new();
// Create a Text record
TextRecord recordText = new("I ❤ .NET nanoFramework", "en", Encoding.UTF8);
// Add the record to the message
message.Records.Add(recordText);
// Create a Geo message
GeoRecord geoRecord = new(2.1234, -1.2345);
// Add the record to the message
message.Records.Add(geoRecord);
// Write the message, by using the default Key A
var res = mifareCard.WriteNdefMessage(message);
if (res)
{
    Debug.WriteLine($"Writing successful");
}
else
{
    Debug.WriteLine($"Error writing to the card");
}
```

Note that if you want to set specific permission and have read only with the default NDEF Keys, you can authenticate and write the NDEF  message with the Key B:

```csharp
// Your secret Key B (here the default one)
mifareCard.KeyB = new byte[6] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
// This will write using this secret key B
var res = mifareCard.WriteNdefMessage(message, false);
```

## Format a card to NDEF

You can format a card, this will be done using Key B, by default it will use the Default Key B. You can pass as well the Key B in the parameters.

```csharp
var ret = mifareCard.FormatNdef();
string msg = ret ? "Formatting successful." : "Error formatting card.";
Debug.WriteLine(msg);
```

## Check if the card is NDEF formatted

You can as well check is properly NDEF formatted:

```csharp
var ret = mifareCard.IsFormattedNdef();
var isForm = ret ? string.Empty : " not";
Debug.WriteLine($"This card is{isForm} NDEF formatted");
```

## Card type supported

NDEF per se is fully independent of cards, so the class can be used independently. A Mifare implementation has been done. All Mifare 1K, 2K and 4K are supported. The Mifare 300 are not.
