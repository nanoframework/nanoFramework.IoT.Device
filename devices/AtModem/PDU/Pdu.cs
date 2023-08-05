// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Text;
using IoT.Device.AtModem.CodingSchemes;
using IoT.Device.AtModem.DTOs;

namespace IoT.Device.AtModem.PDU
{
    /// <summary>
    /// Provides methods to encode and decode SMS Protocol Data Unit (PDU) messages.
    /// </summary>
    public class Pdu
    {
        /// <summary>
        /// Gets the PDU type of a message.
        /// </summary>
        /// <param name="text">The character span representing the PDU message.</param>
        /// <returns>The PDU type.</returns>
        public static PduType GetPduType(SpanChar text)
        {
            int offset = 0;

            // SMSC information
            byte smsc_length = HexToByte(text.SliceOnIndex(offset, offset += 2));
            text.SliceOnIndex(offset, offset += smsc_length * 2);
            byte header = HexToByte(text.SliceOnIndex(offset, offset += 2));
            int pduType = header & 0b0000_0011;
            return (PduType)pduType;
        }

        /// <summary>
        /// Encodes an SMS-SUBMIT PDU message with the specified parameters.
        /// </summary>
        /// <param name="phoneNumber">The phone number of the recipient.</param>
        /// <param name="encodedMessage">The encoded message content.</param>
        /// <param name="dataCodingScheme">The data coding scheme for the message.</param>
        /// <param name="includeEmptySmscLength">Indicates whether to include an empty SMSC length.</param>
        /// <returns>The encoded SMS-SUBMIT PDU message as a string.</returns>
        public static string EncodeSmsSubmit(PhoneNumber phoneNumber, string encodedMessage, byte dataCodingScheme, bool includeEmptySmscLength = true)
        {
            StringBuilder sb = new StringBuilder();

            // Length of SMSC information
            if (includeEmptySmscLength)
            {
                sb.Append("00");
            }

            // First octed of the SMS-SUBMIT message
            sb.Append("11");

            // TP-Message-Reference. '00' lets the phone set the message reference number itself
            sb.Append("00");

            // Address length. Length of phone number (number of digits)
            sb.Append(phoneNumber.ToString().TrimStart('+').Length.ToString("X2"));

            // Type-of-Address
            sb.Append(GetAddressType(phoneNumber).ToString("X2"));

            // Phone number in semi octets. 12345678 is represented as 21436587
            sb.Append(SwapPhoneNumberDigits(phoneNumber.ToString().TrimStart('+')));

            // TP-PID Protocol identifier
            sb.Append("00");

            // TP-DCS Data Coding Scheme. '00'-7bit default alphabet. '04'-8bit
            sb.Append(dataCodingScheme.ToString("X2"));

            // TP-Validity-Period. 'AA'-4 days
            sb.Append("AA");

            // TP-User-Data-Length. If TP-DCS field indicates 7-bit data, the length is the number of septets.
            // If TP-DCS indicates 8-bit data or Unicode, the length is the number of octets.
            if (dataCodingScheme != 0)
            {
                int messageBitLength = encodedMessage.Length * 7 / 2;
                int messageLength = messageBitLength % 8 == 0 ? messageBitLength / 8 : (messageBitLength / 8) + 1;
                sb.Append(messageLength.ToString("X2"));
            }
            else
            {
                sb.Append((encodedMessage.Length / 2 * 8 / 7).ToString("X2"));
            }

            sb.Append(encodedMessage);

            return sb.ToString();
        }

        /// <summary>
        /// Decodes an SMS-DELIVER PDU message from the given character span.
        /// </summary>
        /// <param name="text">The character span representing the PDU message.</param>
        /// <param name="timestampYearOffset">The offset to adjust the year in the timestamp.</param>
        /// <returns>The decoded SMS-DELIVER PDU message as a <see cref="SmsDeliver"/> instance.</returns>
        public static SmsDeliver DecodeSmsDeliver(SpanChar text, int timestampYearOffset = 2000)
        {
            int offset = 0;

            // SMSC information
            byte smsc_length = HexToByte(text.SliceOnIndex(offset, offset += 2));
            PhoneNumber serviceCenterNumber = null;
            if (smsc_length > 0)
            {
                serviceCenterNumber = DecodePhoneNumber(text.SliceOnIndex(offset, offset += smsc_length * 2));
            }

            // SMS-DELIVER start
            byte header = HexToByte(text.SliceOnIndex(offset, offset += 2));

            int tp_mti = header & 0b0000_0011;
            if (tp_mti != (byte)PduType.SMS_DELIVER)
            {
                throw new ArgumentException("Invalid SMS-DELIVER data");
            }

            int tp_mms = header & 0b0000_0100;
            int tp_rp = header & 0b1000_0000;

            byte tp_oa_length = HexToByte(text.SliceOnIndex(offset, offset += 2));
            tp_oa_length = (byte)(tp_oa_length % 2 == 0 ? tp_oa_length : tp_oa_length + 1);
            PhoneNumber oa = null;
            if (tp_oa_length > 0)
            {
                int oa_digits = tp_oa_length + 2; // Add 2 for TON
                oa = DecodePhoneNumber(text.SliceOnIndex(offset, offset += oa_digits));
            }

            byte tp_pid = HexToByte(text.SliceOnIndex(offset, offset += 2));
            byte tp_dcs = HexToByte(text.SliceOnIndex(offset, offset += 2));
            SpanChar tp_scts = text.SliceOnIndex(offset, offset += 14);
            byte tp_udl = HexToByte(text.SliceOnIndex(offset, offset += 2));
            int udlBytes = (int)Math.Ceiling(tp_udl * 7 / 8.0);

            SpanChar tp_ud = text.SliceOnIndex(offset, offset += udlBytes * 2);
            string message = null;
            switch (tp_dcs)
            {
                case 0x00:
                    message = Gsm7.Decode(new string(tp_ud.ToArray()));
                    break;
                default:
                    break;
            }

            DateTime scts = DecodeTimestamp(tp_scts, timestampYearOffset);
            return new SmsDeliver(serviceCenterNumber, oa, message, scts);
        }

        /// <summary>
        /// Decodes an SMS-SUBMIT PDU message from the given character span.
        /// </summary>
        /// <param name="text">The character span representing the PDU message.</param>
        /// <param name="timestampYearOffset">The offset to adjust the year in the timestamp.</param>
        /// <returns>The decoded SMS-SUBMIT PDU message as a <see cref="SmsSubmit"/> instance.</returns>
        public static SmsSubmit DecodeSmsSubmit(SpanChar text, int timestampYearOffset = 2000)
        {
            int offset = 0;

            // SMSC information
            byte smsc_length = HexToByte(text.SliceOnIndex(offset, offset += 2));
            PhoneNumber serviceCenterNumber = null;
            if (smsc_length > 0)
            {
                serviceCenterNumber = DecodePhoneNumber(text.SliceOnIndex(offset, offset += smsc_length * 2));
            }

            // header start
            byte header = HexToByte(text.SliceOnIndex(offset, offset += 2));

            int tp_mti = header & 0b0000_0011;
            if (tp_mti != (byte)PduType.SMS_SUBMIT)
            {
                throw new ArgumentException("Invalid SMS-SUBMIT data");
            }

            int tp_rd = header & 0b0000_0100 >> 2;
            int tp_vpf = header & 0b0001_1000 >> 3;
            int tp_rp = header & 0b1000_0000 >> 7;

            byte tp_mr = HexToByte(text.SliceOnIndex(offset, offset += 2));
            byte tp_oa_length = HexToByte(text.SliceOnIndex(offset, offset += 2));
            tp_oa_length = (byte)(tp_oa_length % 2 == 0 ? tp_oa_length : tp_oa_length + 1);
            PhoneNumber oa = null;
            if (tp_oa_length > 0)
            {
                int oa_digits = tp_oa_length + 2; // Add 2 for TON
                oa = DecodePhoneNumber(text.SliceOnIndex(offset, offset += oa_digits));
            }

            byte tp_pid = HexToByte(text.SliceOnIndex(offset, offset += 2));
            byte tp_dcs = HexToByte(text.SliceOnIndex(offset, offset += 2));
            byte tp_vp = 0;
            if (tp_vpf == 0b0000)
            {
                tp_vp = HexToByte(text.SliceOnIndex(offset, offset += 0));
            }
            else if (tp_vpf == 0b0010)
            {
                tp_vp = HexToByte(text.SliceOnIndex(offset, offset += 14));
            }
            else if (tp_vpf == 0b0001)
            {
                tp_vp = HexToByte(text.SliceOnIndex(offset, offset += 2));
            }
            else if (tp_vpf == 0b0011)
            {
                tp_vp = HexToByte(text.SliceOnIndex(offset, offset += 14));
            }

            byte tp_udl = HexToByte(text.SliceOnIndex(offset, offset += 2));

            string message = null;
            switch (tp_dcs)
            {
                case 0x00:
                    int length = tp_udl % 8 == 0 ? (tp_udl * 7 / 8) : (tp_udl * 7 / 8) + 1;
                    SpanChar tp_ud = text.SliceOnIndex(offset, offset += length * 2);
                    message = Gsm7.Decode(new string(tp_ud.ToArray()));
                    break;
                default:
                    break;
            }

            return new SmsSubmit(serviceCenterNumber, oa, message);
        }

        private static byte HexToByte(SpanChar text)
        {
            byte retVal = Convert.ToByte(new string(text.ToArray()), 16);
            return retVal;
        }

        private static char[] SwapPhoneNumberDigits(string data)
        {
            if (data.Length % 2 != 0)
            {
                data += 'F';
            }

            char[] swappedData = new char[data.Length];
            for (int i = 0; i < swappedData.Length; i += 2)
            {
                swappedData[i] = data[i + 1];
                swappedData[i + 1] = data[i];
            }

            if (swappedData[swappedData.Length - 1] == 'F')
            {
                char[] subArray = new char[swappedData.Length - 1];
                Array.Copy(swappedData, subArray, subArray.Length);
                return subArray;
            }

            return swappedData;
        }

        private static byte GetAddressType(PhoneNumber phoneNumber)
        {
            return (byte)(0b1000_0000 + (byte)(phoneNumber.GetTypeOfNumber() == TypeOfNumber.International ? 0b0001_0001 : 0b0000_0001));
        }

        private static PhoneNumber DecodePhoneNumber(SpanChar data)
        {
            if (data.Length < 4)
            {
                return default;
            }

            TypeOfNumber ton = (TypeOfNumber)((HexToByte(data.Slice(0, 2)) & 0b0111_0000) >> 4);
            string number = string.Empty;
            if (ton == TypeOfNumber.International)
            {
                number = "+";
            }

            number += new string(SwapPhoneNumberDigits(new string(data.Slice(2).ToArray())));
            return new PhoneNumber(number);
        }

        private static DateTime DecodeTimestamp(SpanChar data, int timestampYearOffset = 2000)
        {
            char[] swappedData = new char[data.Length];
            for (int i = 0; i < swappedData.Length; i += 2)
            {
                swappedData[i] = data[i + 1];
                swappedData[i + 1] = data[i];
            }

            SpanChar swappedSpan = swappedData;

            byte offset = DecimalToByte(swappedSpan.SliceOnIndex(12, 14));
            bool positive = (offset & (1 << 7)) == 0;
            byte offsetQuarters = (byte)(offset & 0b0111_1111);

            DateTime timestamp = new DateTime(
                DecimalToByte(swappedSpan.SliceOnIndex(0, 2)) + timestampYearOffset,
                DecimalToByte(swappedSpan.SliceOnIndex(2, 4)),
                DecimalToByte(swappedSpan.SliceOnIndex(4, 6)),
                DecimalToByte(swappedSpan.SliceOnIndex(6, 8)),
                DecimalToByte(swappedSpan.SliceOnIndex(8, 10)),
                DecimalToByte(swappedSpan.SliceOnIndex(10, 12))).Add(TimeSpan.FromMinutes(offsetQuarters * 15)); // Offset in quarter of hours
            return timestamp;
        }

        private static byte DecimalToByte(SpanChar text)
        {
            return Convert.ToByte(new string(text.ToArray()), 10);
        }
    }
}
