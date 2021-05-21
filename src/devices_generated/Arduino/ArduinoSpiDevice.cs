// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.Spi;
using System.Text;

namespace Iot.Device.Arduino
{
    internal sealed class ArduinoSpiDevice : SpiDevice
    {
        public ArduinoSpiDevice(ArduinoBoard board, SpiConnectionSettings connectionSettings)
        {
            Board = board;
            ConnectionSettings = connectionSettings;
            board.EnableSpi();
            board.Firmata.ConfigureSpiDevice(connectionSettings);
        }

        public ArduinoBoard Board
        {
            get;
            private set;
        }

        public override SpiConnectionSettings ConnectionSettings { get; }
        public override byte ReadByte()
        {
            SpanByte dummy = new byte[1];
            Read(dummy);
            return dummy[0];
        }

        public override void Read(SpanByte buffer)
        {
            SpanByte dummy = new byte[buffer.Length];
            Board.Firmata.SpiTransfer(ConnectionSettings.ChipSelectLine, dummy, buffer);
        }

        public override void WriteByte(byte value)
        {
            SpanByte span = new byte[1]
            {
                value
            };

            Write(span);
        }

        public override void Write(SpanByte buffer)
        {
            Board.Firmata.SpiWrite(ConnectionSettings.ChipSelectLine, buffer);
        }

        public override void TransferFullDuplex(SpanByte writeBuffer, SpanByte readBuffer)
        {
            Board.Firmata.SpiTransfer(ConnectionSettings.ChipSelectLine, writeBuffer, readBuffer);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Board != null)
                {
                    Board.DisableSpi();
                    // To make sure this is called only once (and any further attempts to use this instance fail)
                    Board = null!;
                }
            }

            base.Dispose(disposing);
        }
    }
}
