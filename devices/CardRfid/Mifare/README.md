# Mifare card - RFID Card

This class supports Mifare cards. They are RFID cards responding to ISO 14443 type A. You need a specific card reader like [MFRC522](../../Mfrc522), [PN532](../../Pn532), [PN5180](../../Pn5180) to read, write those kind of cards.

## Creating a card and reading it

You'll need first to get the card from an RFID reader. The example below shows how to do it with a PN532 and read all the sectors and print all the sector data information.

```csharp
byte[] retData = null;
while (true)
{
    retData = pn532.ListPassiveTarget(MaxTarget.One, TargetBaudRate.B106kbpsTypeA);
    if (retData is object)
        break;
    // Give time to PN532 to process
    Thread.Sleep(200);
}
if (retData is null)
    return;
var decrypted = pn532.Decode106kbpsTypeA(retData.AsSpan().Slice(1));
if (decrypted is object)
{
    Debug.WriteLine($"Tg: {decrypted.TargetNumber}, ATQA: {decrypted.Atqa} SAK: {decrypted.Sak}, NFCID: {BitConverter.ToString(decrypted.NfcId)}");
    if (decrypted.Ats is object)
        Debug.WriteLine($", ATS: {BitConverter.ToString(decrypted.Ats)}");
    MifareCard mifareCard = new MifareCard(pn532, decrypted.TargetNumber) { BlockNumber = 0, Command = MifareCardCommand.AuthenticationA };
    mifareCard.SetCapacity(decrypted.Atqa, decrypted.Sak);
    mifareCard.SerialNumber = decrypted.NfcId;
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
                Debug.WriteLine($"Bloc: {block}, Data: {BitConverter.ToString(mifareCard.Data)}");
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
