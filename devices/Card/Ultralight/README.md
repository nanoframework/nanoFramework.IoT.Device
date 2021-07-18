# Ultralight card - RFID Card

This class supports Ultralight cards. They are RFID cards responding to ISO 14443 type A. You need a specific card reader like PN532, PN5180 or MFRC522 to read, write those kind of cards.

## Creating a card and reading it

You will find detailed examples for PN532 [here](../../Pn532/samples), for MFRC522 [here](../../Mfrc522/samples) and for PN5180 [here](../../Pn5180/samples).

You'll need first to get the card from an RFID reader. The example below shows how to do it with a MFRC522 and read all the sectors, read the configuration data, and print all the sector data information, read NDEF, write NDEF.

```csharp
// Note: mfrc522 should not be null
var ultralight = new UltralightCard(mfrc522, 0);
ultralight.SerialNumber = card.NfcId;
Debug.WriteLine($"Type: {ultralight.UltralightCardType}, Ndef capacity: {ultralight.NdefCapacity}");

var version = ultralight.GetVersion();
if ((version != null) && (version.Length > 0))
{
    Debug.WriteLine("Get Version details: ");
    for (int i = 0; i < version.Length; i++)
    {
        Debug.Write($"{version[i]:X2} ");
    }

    Debug.WriteLine("");
}
else
{
    Debug.WriteLine("Can't read the version.");
}

var sign = ultralight.GetSignature();
if ((sign != null) && (sign.Length > 0))
{
    Debug.WriteLine("Signature: ");
    for (int i = 0; i < sign.Length; i++)
    {
        Debug.Write($"{sign[i]:X2} ");
    }

    Debug.WriteLine("");
}
else
{
    Debug.WriteLine("Can't read the signature.");
}

// The ReadFast feature can be used as well, note that the MFRC522 has a very limited FIFO
// So maximum 9 pages can be read as once.
Debug.WriteLine("Fast read example:");
var buff = ultralight.ReadFast(0, 8);
if (buff != null)
{
    for (int i = 0; i < buff.Length / 4; i++)
    {
        Debug.WriteLine($"  Block {i} - {buff[i * 4]:X2} {buff[i * 4 + 1]:X2} {buff[i * 4 + 2]:X2} {buff[i * 4 + 3]:X2}");
    }
}

Debug.WriteLine("Dump of all the card:");
for (int block = 0; block < ultralight.NumberBlocks; block++)
{
    ultralight.BlockNumber = (byte)block; // Safe cast, can't be more than 255
    ultralight.Command = UltralightCommand.Read16Bytes;
    var res = ultralight.RunUltralightCommand();
    if (res > 0)
    {
        Debug.Write($"  Block: {ultralight.BlockNumber:X2} - ");
        for (int i = 0; i < 4; i++)
        {
            Debug.Write($"{ultralight.Data![i]:X2} ");
        }

        var isReadOnly = ultralight.IsPageReadOnly(ultralight.BlockNumber);
        Debug.Write($"- Read only: {isReadOnly} ");

        Debug.WriteLine("");
    }
    else
    {
        Debug.WriteLine("Can't read card");
        break;
    }
}

Debug.WriteLine("Configuration of the card");
// Get the Configuration
res = ultralight.TryGetConfiguration(out Configuration configuration);
if (res)
{
    Debug.WriteLine("  Mirror:");
    Debug.WriteLine($"    {configuration.Mirror.MirrorType}, page: {configuration.Mirror.Page}, position: {configuration.Mirror.Position}");
    Debug.WriteLine("  Authentication:");
    Debug.WriteLine($"    Page req auth: {configuration.Authentication.AuthenticationPageRequirement}, Is auth req for read and write: {configuration.Authentication.IsReadWriteAuthenticationRequired}");
    Debug.WriteLine($"    Is write lock: {configuration.Authentication.IsWrittenLocked}, Max num tries: {configuration.Authentication.MaximumNumberOfPossibleTry}");
    Debug.WriteLine("  NFC Counter:");
    Debug.WriteLine($"    Enabled: {configuration.NfcCounter.IsEnabled}, Password protected: {configuration.NfcCounter.IsPasswordProtected}");
    Debug.WriteLine($"  Is strong modulation: {configuration.IsStrongModulation}");
}
else
{
    Debug.WriteLine("Error getting the configuration");
}

NdefMessage message;
res = ultralight.TryReadNdefMessage(out message);
if (res && message.Length != 0)
{
    foreach (var record in message.Records)
    {
        Debug.WriteLine($"Record length: {record.Length}");
        if (TextRecord.IsTextRecord(record))
        {
            var text = new TextRecord(record);
            Debug.WriteLine(text.Text);
        }
    }
}
else
{
    Debug.WriteLine("No NDEF message in this ");
}

res = ultralight.IsFormattedNdef();
if (!res)
{
    Debug.WriteLine("Card is not NDEF formatted, we will try to format it");
    res = ultralight.FormatNdef();
    if (!res)
    {
        Debug.WriteLine("Impossible to format in NDEF, we will still try to write NDEF content.");
    }
    else
    {
        res = ultralight.IsFormattedNdef();
        if (res)
        {
            Debug.WriteLine("Formatting successful");
        }
        else
        {
            Debug.WriteLine("Card is not NDEF formatted.");
        }
    }
}

NdefMessage newMessage = new NdefMessage();
newMessage.Records.Add(new TextRecord("I ❤ .NET nanoFramework", "en", Encoding.UTF8));
res = ultralight.WriteNdefMessage(newMessage);
if (res)
{
    Debug.WriteLine("NDEF data successfully written on the card.");
}
else
{
    Debug.WriteLine("Error writing NDEF data on card");
}
 ```


