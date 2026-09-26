// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.M24Sr;
using Iot.Device.Ndef;
using System;
using System.Device.I2c;
using System.Diagnostics;
using System.Text;

// The on-board M24SR64-Y on the B-L475E-IOT01A is connected to the NFC I2C bus.
// Adjust the bus number if the board firmware maps that peripheral differently.
const string TextToAdd = "I love nanoFramework";
I2cConnectionSettings settings = new(2, M24Sr.DefaultI2cAddress);
using M24Sr tag = new(I2cDevice.Create(settings));

tag.OpenSession();
try
{
    M24SrCapabilityContainer capabilityContainer = tag.ReadCapabilityContainer();
    NdefMessage message = tag.ReadNdefMessage();

    Debug.WriteLine($"NDEF capacity: {capabilityContainer.MaximumNdefMessageSize} bytes");
    Debug.WriteLine($"NDEF length: {message.Length} bytes, records: {message.Records.Count}");
    for (int index = 0; index < message.Records.Count; index++)
    {
        DisplayRecord(message.Records[index], index + 1);
    }

    if (!ContainsTextRecord(message, TextToAdd))
    {
        message.Records.Add(new TextRecord(TextToAdd, "en", Encoding.UTF8));
        tag.WriteNdefMessage(message);
        Debug.WriteLine($"Added text record: {TextToAdd}");
    }
    else
    {
        Debug.WriteLine($"Text record already exists: {TextToAdd}");
    }
}
finally
{
    tag.CloseSession();
}

bool ContainsTextRecord(NdefMessage message, string text)
{
    for (int index = 0; index < message.Records.Count; index++)
    {
        NdefRecord record = message.Records[index];
        if (TextRecord.IsTextRecord(record) && (record.Payload != null) && (record.Payload.Length > 0))
        {
            TextRecord textRecord = new(record);
            if (textRecord.Text == text)
            {
                return true;
            }
        }
    }

    return false;
}

void DisplayRecord(NdefRecord record, int recordNumber)
{
    string type = record.Header.PayloadType == null ? "(none)" : BitConverter.ToString(record.Header.PayloadType);
    string id = record.Header.PayloadId == null ? "(none)" : BitConverter.ToString(record.Header.PayloadId);
    int payloadLength = record.Payload == null ? 0 : record.Payload.Length;

    Debug.WriteLine($"Record {recordNumber}: TNF={record.Header.TypeNameFormat}, flags={record.Header.MessageFlag}");
    Debug.WriteLine($"  Type bytes: {type}, ID: {id}, payload length: {payloadLength}");

    if (payloadLength == 0)
    {
        Debug.WriteLine("  Empty payload");
        return;
    }

    try
    {
        if (TextRecord.IsTextRecord(record))
        {
            TextRecord textRecord = new(record);
            Debug.WriteLine($"  Text ({textRecord.LanguageCode}): {textRecord.Text}");
        }
        else if (GeoRecord.IsGeoRecord(record))
        {
            GeoRecord geoRecord = new(record);
            Debug.WriteLine($"  Geographic coordinates: {geoRecord.Latitude}, {geoRecord.Longitude}");
        }
        else if (UriRecord.IsUriRecord(record))
        {
            UriRecord uriRecord = new(record);
            Debug.WriteLine($"  URI: {uriRecord.FullUri}");
        }
        else if (MediaRecord.IsMediaRecord(record))
        {
            MediaRecord mediaRecord = new(record);
            Debug.WriteLine($"  Media type: {mediaRecord.PayloadType}");
            if (mediaRecord.IsTextType && mediaRecord.TryGetPayloadAsText(out string payloadAsText))
            {
                Debug.WriteLine($"  Content: {payloadAsText}");
            }
            else
            {
                Debug.WriteLine($"  Binary media payload: {payloadLength} bytes");
            }
        }
        else
        {
            Debug.WriteLine("  Unsupported NDEF record type");
        }
    }
    catch (Exception exception)
    {
        Debug.WriteLine($"  Unable to decode record: {exception.Message}");
    }
}