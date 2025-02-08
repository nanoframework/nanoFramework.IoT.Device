// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Mcp25xxx.Register;
using Iot.Device.Mcp25xxx.Register.CanControl;
using Iot.Device.Mcp25xxx.Register.Interrupt;
using Iot.Device.Mcp25xxx.Tests.Register.CanControl;
using System;
using System.Diagnostics;
using System.Threading;

namespace Iot.Device.Mcp25xxx.Can
{
    /// <summary>
    /// Represents a CAN controller on the system.
    /// </summary>
    public class CanController : ICanController
    {
        byte MCP_SIDH = 0;
        byte MCP_SIDL = 1;
        byte MCP_EID8 = 2;
        byte MCP_EID0 = 3;
        byte MCP_DLC = 4;
        byte DLC_MASK = 0x0F;
        byte TXB_EXIDE_MASK = 0x08;

        private Mcp25xxx _mcp25xxx;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mcp25xxx"></param>
        public void Initialize(Mcp25xxx mcp25xxx)
        {
            _mcp25xxx = mcp25xxx;
        }

        /// <summary>
        /// Write message to CAN Bus.
        /// </summary>
        /// <param name="message">CAN mesage to write in CAN Bus.</param>
        public void WriteMessage(CanMessage message)
        {

        }

        /// <summary>
        /// Get next <see cref="CanMessage"/> available in the _<see cref="CanController"/> internal buffer.
        /// If there are no more messages available null will be returned.
        /// </summary>
        /// <returns>
        /// A <see cref="CanMessage"/> or null if there are no more messages available.
        /// </returns>
        public CanMessage GetMessage()
        {
            var status = _mcp25xxx.ReadStatus();

            if (status.HasFlag(ReadStatusResponse.Rx0If))
            { // message in buffer 0
                return ReadDataMessage(0);
            }
            else if (status.HasFlag(ReadStatusResponse.Rx1If))
            { // message in buffer 1
                return ReadDataMessage(1);
            }
            else
            { // no messages available
                return null;
            }
        }

        /// <summary>
        /// Clear all receive buffers.
        /// </summary>
        public void Reset()
        {
            Debug.WriteLine("Reset Instruction");
            _mcp25xxx.Reset();

            Thread.Sleep(10);

            // Clear control buffers
            byte[] data14 = new byte[14];
            _mcp25xxx.Write(Address.TxB0Ctrl, data14);
            _mcp25xxx.Write(Address.TxB1Ctrl, data14);
            _mcp25xxx.Write(Address.TxB2Ctrl, data14);

            _mcp25xxx.WriteByte(Address.RxB0Ctrl, 0);
            _mcp25xxx.WriteByte(Address.RxB1Ctrl, 0);


            _mcp25xxx.WriteByte(Address.CanIntE, (byte)(InterruptEnable.RXB0 | InterruptEnable.RXB1 | InterruptEnable.ERR | InterruptEnable.MSG_ERR));

            // receives all valid messages using either Standard or Extended Identifiers that
            // meet filter criteria. RXF0 is applied for RXB0, RXF1 is applied for RXB1
            _mcp25xxx.BitModify(Address.RxB0Ctrl,
                0x60 | 0x04 | 0x07,     // moet die laatste niet 0x03 zijn? Geprobeerd maar doet niets..
                0x00 | 0x04 | 0x00);
            _mcp25xxx.BitModify(Address.RxB1Ctrl,
                0x60 | 0x07,
                0x00 | 0x01);

            // clear filters and masks
            // do not filter any standard frames for RXF0 used by RXB0
            // do not filter any extended frames for RXF1 used by RXB1
            byte[] zeros12 = new byte[12];
            _mcp25xxx.Write(Address.RxF0Sidh, zeros12);
            _mcp25xxx.Write(Address.RxF3Sidh, zeros12);

            byte[] zeros8 = new byte[8];
            _mcp25xxx.Write(Address.RxM0Sidh, zeros8);
        }

        private CanMessage ReadDataMessage(int bufferNumber)
        {
            var sidh_reg = bufferNumber == 0 ? Address.RxB0Sidh : Address.RxB1Sidh;
            var sidl_reg = bufferNumber == 0 ? Address.RxB0Sidl : Address.RxB1Sidl;

            var dlc_reg = bufferNumber == 0 ? Address.RxB0Dlc : Address.RxB1Dlc;


            var ctrl_reg = bufferNumber == 0 ? Address.RxB0Ctrl : Address.RxB1Ctrl;
            var data_reg = bufferNumber == 0 ? Address.RxB0D0 : Address.RxB1D0; //.RXB0DATA    RXB1DATA
            var int_flag = bufferNumber == 0 ? ReadStatusResponse.Rx0If : ReadStatusResponse.Rx1If;

            // read 5 bytes
            var buffer = _mcp25xxx.Read(sidh_reg, 5);

            int id = (buffer[MCP_SIDH] << 3) + (buffer[MCP_SIDL] >> 5);

            bool isExtended = false;

            // check to see if it's an extended ID
            if ((buffer[MCP_SIDL] & TXB_EXIDE_MASK) == TXB_EXIDE_MASK)
            {
                id = (id << 2) + (buffer[MCP_SIDL] & 0x03);
                id = (id << 8) + buffer[MCP_EID8];
                id = (id << 8) + buffer[MCP_EID0];
                isExtended = true;
            }

            byte dataLengthCode = (byte)(buffer[MCP_DLC] & DLC_MASK);
            if (dataLengthCode > 8) throw new Exception($"DLC of {dataLengthCode} is > 8 bytes");

            // see if it's a remote transmission request
            //var isRemoteTransmitRequest = false;
            //var ctrl = ReadRegister(ctrl_reg)[0];
            //if ((ctrl & RXBnCTRL_RTR) == RXBnCTRL_RTR)
            //{
            //    isRemoteTransmitRequest = true;
            //}

            // create the CANMessag
            CanMessage frame;

            if (isExtended)
            {
                //if (isRemoteTransmitRequest)
                //{
                //    frame = new ExtendedRtrFrame
                //    {
                //        ID = id,
                //    };
                //}
                //else
                //{
                frame = new CanMessage((uint)id,
                                        CanMessageIdType.EID,
                                        CanMessageFrameType.Data,
                                        _mcp25xxx.Read(data_reg, dataLengthCode));

                //}
            }
            else
            {
                //if (isRemoteTransmitRequest)
                //{
                //    frame = new StandardRtrFrame
                //    {
                //        ID = id,
                //    };
                //}
                //else
                //{
                frame = new CanMessage((uint)id,
                                        CanMessageIdType.SID,
                                        CanMessageFrameType.Data,
                                        _mcp25xxx.Read(data_reg, dataLengthCode));

                // read the frame data
                //}
                //}


            }
            // clear the interrupt flag
            //if (InterruptPort != null)
            //{
            //ClearInterrupt(int_flag);
            ClearInterrupt(bufferNumber);   // int_flag);

            return frame;
        }

        private void ClearInterrupt(int bufferNumber) 
        {
            var canIntf = new CanIntF(_mcp25xxx.Read(Address.CanIntF));
            if (bufferNumber == 0)
            {
                canIntf.ReceiveBuffer0FullInterruptFlag = false;
            }
            else
            {
                canIntf.ReceiveBuffer1FullInterruptFlag = false;
            }

            // Write the modified value back to the CanCtrl register
            _mcp25xxx.WriteByte(canIntf);
        }

        class Config
        {
            public long ClockFrequency { get; set; }
            public long BaudRate { get; set; }
            public byte[] Cnf { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="baudRate">CAN baudrate. For example 250.000</param>
        /// <param name="clockFrequency">mcp2515 clock frequency. For example 8.000.000</param>
        public void SetBitRate(int baudRate, int clockFrequency)
        {
            var mode = GetOperationMode();
            if(mode != OperationMode.Configuration)
            {
                SetOperationMode(OperationMode.Configuration);
            }

            var CNF_MAPPER = new Config[]
            {
                new() { ClockFrequency = 8_000_000L, BaudRate = 1_000_000L, Cnf = new byte[] { 0x00, 0x80, 0x00 } },
                new() { ClockFrequency = 8_000_000L, BaudRate = 666_666L,   Cnf = new byte[] { 0xC0, 0xB8, 0x01 } },
                new() { ClockFrequency = 8_000_000L, BaudRate = 500_000L,   Cnf = new byte[] { 0x00, 0x90, 0x02 } },
                new() { ClockFrequency = 8_000_000L, BaudRate = 250_000L,   Cnf = new byte[] { 0x00, 0xB1, 0x05 } },
                new() { ClockFrequency = 8_000_000L, BaudRate = 200_000L,   Cnf = new byte[] { 0x00, 0xB4, 0x06 } },
                new() { ClockFrequency = 8_000_000L, BaudRate = 125_000L,   Cnf = new byte[] { 0x01, 0xB1, 0x05 } },
                new() { ClockFrequency = 8_000_000L, BaudRate = 100_000L,   Cnf = new byte[] { 0x01, 0xB4, 0x06 } },
                new() { ClockFrequency = 8_000_000L, BaudRate = 80_000L,    Cnf = new byte[] { 0x01, 0xBF, 0x07 } },
                new() { ClockFrequency = 8_000_000L, BaudRate = 50_000L,    Cnf = new byte[] { 0x03, 0xB4, 0x06 } },
                new() { ClockFrequency = 8_000_000L, BaudRate = 40_000L,    Cnf = new byte[] { 0x03, 0xBF, 0x07 } },
                new() { ClockFrequency = 8_000_000L, BaudRate = 20_000L,    Cnf = new byte[] { 0x07, 0xBF, 0x07 } },
                new() { ClockFrequency = 8_000_000L, BaudRate = 10_000L,    Cnf = new byte[] { 0x0F, 0xBF, 0x07 } },
                new() { ClockFrequency = 8_000_000L, BaudRate = 5_000L,     Cnf = new byte[] { 0x1F, 0xBF, 0x07 } },
                new() { ClockFrequency = 16_000_000L, BaudRate = 1_000_000L, Cnf = new byte[] { 0x00, 0xD0, 0x82 } },
                new() { ClockFrequency = 16_000_000L, BaudRate = 666_666L,   Cnf = new byte[] { 0xC0, 0xF8, 0x81 } },
                new() { ClockFrequency = 16_000_000L, BaudRate = 500_000L,   Cnf = new byte[] { 0x00, 0xF0, 0x86 } },
                new() { ClockFrequency = 16_000_000L, BaudRate = 250_000L,   Cnf = new byte[] { 0x41, 0xF1, 0x85 } },
                new() { ClockFrequency = 16_000_000L, BaudRate = 200_000L,   Cnf = new byte[] { 0x01, 0xFA, 0x87 } },
                new() { ClockFrequency = 16_000_000L, BaudRate = 125_000L,   Cnf = new byte[] { 0x03, 0xF0, 0x86 } },
                new() { ClockFrequency = 16_000_000L, BaudRate = 100_000L,   Cnf = new byte[] { 0x03, 0xFA, 0x87 } },
                new() { ClockFrequency = 16_000_000L, BaudRate = 80_000L,    Cnf = new byte[] { 0x03, 0xFF, 0x87 } },
                new() { ClockFrequency = 16_000_000L, BaudRate = 50_000L,    Cnf = new byte[] { 0x07, 0xFA, 0x87 } },
                new() { ClockFrequency = 16_000_000L, BaudRate = 40_000L,    Cnf = new byte[] { 0x07, 0xFF, 0x87 } },
                new() { ClockFrequency = 16_000_000L, BaudRate = 20_000L,    Cnf = new byte[] { 0x0F, 0xFF, 0x87 } },
                new() { ClockFrequency = 16_000_000L, BaudRate = 10_000L,    Cnf = new byte[] { 0x1F, 0xFF, 0x87 } },
                new() { ClockFrequency = 16_000_000L, BaudRate = 5_000L,     Cnf = new byte[] { 0x3F, 0xFF, 0x87 } },
            };

            byte[] cnf = null;

            foreach (var mapper in CNF_MAPPER)
            {
                if (mapper.ClockFrequency == clockFrequency && mapper.BaudRate == baudRate)
                {
                    cnf = mapper.Cnf;
                    break;
                }
            }

            _mcp25xxx.WriteByte(Address.Cnf1, cnf[0]);
            _mcp25xxx.WriteByte(Address.Cnf2, cnf[1]);
            _mcp25xxx.WriteByte(Address.Cnf3, cnf[2]);

            SetFilterMask(MASK.MASK0, false, 0x7ff);
            SetFilter(RXF.RXF0, false, 0x154);
            SetFilterMask(MASK.MASK1, false, 0x7ff);
            SetFilter(RXF.RXF0, false, 0x154);

            SetOperationMode(OperationMode.NormalOperation);
        }

        public bool SetFilter(RXF num, bool ext, uint ulData)
        {
            var mode = GetOperationMode();
            SetOperationMode(OperationMode.Configuration);

            Address reg;

            switch (num)
            {
                case RXF.RXF0: reg = Address.RxF0Sidh; break;
                case RXF.RXF1: reg = Address.RxF1Sidh; break;
                case RXF.RXF2: reg = Address.RxF2Sidh; break;
                case RXF.RXF3: reg = Address.RxF3Sidh; break;
                case RXF.RXF4: reg = Address.RxF4Sidh; break;
                case RXF.RXF5: reg = Address.RxF5Sidh; break;
                default:
                    return false;
            }

            SpanByte tbufdata = new byte[4];
            PrepareId(tbufdata, ext, ulData);
            _mcp25xxx.Write(reg, tbufdata);

            SetOperationMode(mode);

            return true;
        }

        void PrepareId(SpanByte buffer, bool ext, uint id)
        {
            ushort canid = (ushort)(id & 0x0FFFF);

            if (ext)
            {
                buffer[MCP_EID0] = (byte)(canid & 0xFF);
                buffer[MCP_EID8] = (byte)(canid >> 8);
                canid = (ushort)(id >> 16);
                buffer[MCP_SIDL] = (byte)(canid & 0x03);
                buffer[MCP_SIDL] += (byte)((canid & 0x1C) << 3);
                buffer[MCP_SIDL] |= TXB_EXIDE_MASK;
                buffer[MCP_SIDH] = (byte)(canid >> 5);
            }
            else
            {
                buffer[MCP_SIDH] = (byte)(canid >> 3);
                buffer[MCP_SIDL] = (byte)((canid & 0x07) << 5);
                buffer[MCP_EID0] = 0;
                buffer[MCP_EID8] = 0;
            }
        }

        private bool SetFilterMask(MASK mask, bool ext, uint ulData)
        {
            var mode = GetOperationMode();
            SetOperationMode(OperationMode.Configuration);

            SpanByte tbufdata = new SpanByte(new byte[4]);
            PrepareId(tbufdata, ext, ulData);

            Address reg;
            switch (mask)
            {
                case MASK.MASK0: reg = Address.RxM0Sidh; break;
                case MASK.MASK1: reg = Address.RxM1Sidh; break;
                default:
                    return false;
            }

            _mcp25xxx.Write(reg, tbufdata);

            SetOperationMode(mode);
            return true;
        }
        private OperationMode GetOperationMode()
        {
            byte value = _mcp25xxx.Read(Address.CanStat);
            var canStat = new CanStat(value);
            return canStat.OperationMode;
        }

        /// <summary>
        /// Sets the operation mode of the MCP25xxx device.
        /// </summary>
        /// <param name="mcp25xxx">The MCP25xxx device instance.</param>
        /// <param name="mode">The desired operation mode.</param>
        private void SetOperationMode(OperationMode mode)
        {
            // Read the current value of the CanCtrl register
            var canCtrlValue = new CanCtrl(_mcp25xxx.Read(Address.CanCtrl));

            var newCanCtrlValue = new CanCtrl(
                canCtrlValue.ClkOutPinPrescaler,
                canCtrlValue.ClkOutPinEnable,
                canCtrlValue.OneShotMode,
                canCtrlValue.AbortAllPendingTransmissions,
                mode);

            // Write the modified value back to the CanCtrl register
            _mcp25xxx.WriteByte(newCanCtrlValue);
        }
    }
}
