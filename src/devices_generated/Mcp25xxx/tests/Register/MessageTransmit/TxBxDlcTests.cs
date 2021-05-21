// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Mcp25xxx.Register;
using Iot.Device.Mcp25xxx.Register.MessageTransmit;
using Xunit;
using static Iot.Device.Mcp25xxx.Register.MessageTransmit.TxBxDlc;

namespace Iot.Device.Mcp25xxx.Tests.Register.MessageTransmit
{
    public class TxBxDlcTests
    {
        [Theory]
        [InlineData(0, Address.TxB0Dlc)]
        [InlineData(1, Address.TxB1Dlc)]
        [InlineData(2, Address.TxB2Dlc)]
        public void Get_TxBufferNumber_Address(byte txBufferNumber, Address address)
        {
            Assert.Equal(txBufferNumber, GetTxBufferNumber(address));
            Assert.Equal(address, new TxBxDlc(txBufferNumber, 0, false).Address);
        }

        [Theory]
        [InlineData(0b0000, false, 0b0000_0000)]
        [InlineData(0b0000, true, 0b0100_0000)]
        [InlineData(0b1000, false, 0b0000_1000)]
        public void From_To_Byte(byte dataLengthCode, bool remoteTransmissionRequest, byte expectedByte)
        {
            var txBxDlc = new TxBxDlc(1, dataLengthCode, remoteTransmissionRequest);
            Assert.Equal(dataLengthCode, txBxDlc.DataLengthCode);
            Assert.Equal(remoteTransmissionRequest, txBxDlc.RemoteTransmissionRequest);
            Assert.Equal(expectedByte, txBxDlc.ToByte());
            Assert.Equal(expectedByte, new TxBxDlc(1, expectedByte).ToByte());
        }
    }
}
