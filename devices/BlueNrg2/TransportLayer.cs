// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using BlueNrg2;
using BlueNrg2.Utils;
using Iot.Device.BlueNrg2.Aci;
using Iot.Device.BlueNrg2.Aci.Events;

namespace Iot.Device.BlueNrg2
{
    internal delegate void UserEventCallback(byte[] data);

    internal struct Request
    {
        public ushort OpCodeGroup;
        public ushort OpCodeCommand;
        public uint Event;
        public byte[] CommandParameter;
        public uint CommandLength;
        public byte[] ResponseParameter;
        public uint ResponseLength;
    }

    internal sealed class TransportLayer
    {
        private readonly IHardwareInterface _hardwareInterface;
        private readonly List _readPacketCallbackQueue;
        private readonly EventProcessor _eventProcessor;

        private readonly List _readPacketPool;

        internal TransportLayer(EventProcessor eventProcessor, IHardwareInterface hardwareInterface)
        {
            _readPacketPool = new List();
            _readPacketCallbackQueue = new List();

            _eventProcessor = eventProcessor;

            _hardwareInterface = hardwareInterface;
            _hardwareInterface.NotifyAsyncEvent += NotifyAsyncEvent;
        }

        public int SendRequest(ref Request r, bool async)
        {
            object readPacket = null;
            var tempQueue = new List();
            var opCode = PackCommandOpcode(r.OpCodeGroup, r.OpCodeCommand);

            FreeEventList();

            SendCommand(r.OpCodeGroup, r.OpCodeCommand, (byte) r.CommandLength, r.CommandParameter);

            if (async)
                return 0;

            var i = 0;
            while (true)
            {
                var tickStart = _hardwareInterface.GetTick();

                while (true)
                {
                    if (_hardwareInterface.GetTick() - tickStart > 1000)
                    {
                        goto failed;
                    }

                    if (!_readPacketCallbackQueue.Empty())
                    {
                        break;
                    }

                    Debug.WriteLine($"loop #{i++}");
                }

                readPacket = (DataPacket) _readPacketCallbackQueue.RemoveHead();
                if (((DataPacket) readPacket).DataBuffer is null)
                    goto failed;
                var hciHdr = ToSpiPacket(((DataPacket) readPacket).DataBuffer);

                if (hciHdr.Type == PacketType.Event)
                {
                    var eventPacket = ToEventPacket(hciHdr.Data);

                    var length = ((DataPacket) readPacket).DataLength - 3;
                    var ptr = new byte[length];
                    Array.Copy(((DataPacket) readPacket).DataBuffer, 3, ptr, 0, length);

                    switch (eventPacket.Event)
                    {
                        case EventType.CommandStatus:
                        {
                            var cs = ToCommandStatus(ptr);
                            if (cs.Opcode != opCode)
                                goto failed;

                            if (r.Event != (byte) EventType.CommandStatus)
                            {
                                if (cs.Status != 0)
                                    goto failed;

                                break;
                            }

                            r.ResponseLength = Math.Min((uint) length, r.ResponseLength);
                            r.ResponseParameter = new byte[r.ResponseLength];
                            Array.Copy(ptr, r.ResponseParameter, (int) r.ResponseLength);
                            goto done;
                        }
                        case EventType.CommandComplete:
                        {
                            var cc = ToCommandComplete(ptr);

                            if (cc.Opcode != opCode)
                                goto failed;

                            length -= 3;
                            var tmp = new byte[length];
                            Array.Copy(ptr, 3, tmp, 0, length);
                            ptr = tmp;

                            r.ResponseLength = Math.Min((uint) length, r.ResponseLength);
                            r.ResponseParameter = new byte[r.ResponseLength];
                            Array.Copy(ptr, r.ResponseParameter, (int) r.ResponseLength);
                            goto done;
                        }

                        case EventType.LeMetaEvent:
                        {
                            var me = ToMetaEvent(ptr);

                            if (me.SubEvent != r.Event)
                                break;

                            length -= 1;
                            r.ResponseLength = Math.Min((uint) length, r.ResponseLength);
                            r.ResponseParameter = new byte[r.ResponseLength];
                            Array.Copy(me!.Data, r.ResponseParameter, (int) r.ResponseLength);

                            goto done;
                        }

                        case EventType.HardWareError:
                            goto failed;
                    }
                }

                if (_readPacketPool.Empty() && _readPacketCallbackQueue.Empty())
                {
                    _readPacketPool.InsertTail(readPacket);
                }
                else
                {
                    tempQueue.InsertTail(readPacket);
                }
            }

            failed:
            if (readPacket is not null)
                _readPacketPool.InsertHead(readPacket);

            tempQueue.MoveTo(_readPacketCallbackQueue);

            return -1;

            done:
            _readPacketPool.InsertHead(readPacket);
            tempQueue.MoveTo(_readPacketCallbackQueue);

            return 0;
        }

        private static MetaEvent ToMetaEvent(byte[] data)
        {
            var result = new MetaEvent
            {
                SubEvent = BitConverter.ToUInt32(data!, 0),
                Data = new byte[data.Length - 4]
            };
            Array.Copy(data, 4, result.Data!, 0, data.Length - 4);
            return result;
        }

        private static CommandComplete ToCommandComplete(byte[] ptr)
        {
            var result = new CommandComplete
            {
                CommandCount = ptr![0],
                Opcode = BitConverter.ToUInt16(ptr, 1)
            };
            return result;
        }

        private static CommandStatus ToCommandStatus(byte[] data)
        {
            var result = new CommandStatus
            {
                Status = data![0],
                CommandCount = data[1],
                Opcode = BitConverter.ToUInt16(data, 2)
            };
            return result;
        }

        private void FreeEventList()
        {
            while (_readPacketPool.Length() < 5 && !_readPacketCallbackQueue.Empty())
            {
                var readPacket = (DataPacket) _readPacketCallbackQueue.RemoveHead();
                _readPacketPool.InsertTail(readPacket);
            }
        }

        private static byte[] ToByteArray(Command command)
        {
            var result = new byte[3];
            BitConverter.GetBytes(command.Opcode).CopyTo(result, 0);
            result[2] = command.ParameterLength;
            return result;
        }

        private static ushort PackCommandOpcode(ushort opcodeGroup, ushort opcodeCommand)
        {
            return (ushort) ((ushort) (opcodeCommand & 0x03f) | (ushort) (opcodeGroup << 10));
        }

        private void SendCommand(ushort opcodeGroup, ushort opcodeCommand, byte parameterLength, byte[] parameter)
        {
            if (parameter is null || parameterLength == 0)
                return;

            var payload = new byte[128];
            var hc = new Command
            {
                Opcode = PackCommandOpcode(opcodeGroup, opcodeCommand),
                ParameterLength = parameterLength
            };

            payload[0] = 0x01;
            ToByteArray(hc).CopyTo(payload, 1);
            parameter.CopyTo(payload, 4);

            _hardwareInterface.Send(payload, (ushort) (4 + parameterLength));
        }

        public void UserEventProcess()
        {
            while (!_readPacketCallbackQueue.Empty())
            {
                var readPacket = (DataPacket) _readPacketCallbackQueue.RemoveHead();
                HandleReceivedEvent(readPacket.DataBuffer);

                _readPacketPool.InsertTail(readPacket);
            }
        }

        private void HandleReceivedEvent(byte[] data)
        {
            if (data[0] == 0x04)
            {
                var evt = data[1];
                var pLen = data[2];

                if (evt == 0x3E)
                {
                    var subEvent = data[3];
                    var eventData = new byte[data.Length - 4];
                    Array.Copy(data, 4, eventData, 0, data.Length - 4);
                    for (var i = 0; i < _eventProcessor.LeMetaEventsTable.Length; i++)
                    {
                        if (subEvent == _eventProcessor.LeMetaEventsTable[i].EventCode)
                        {
                            _eventProcessor.LeMetaEventsTable[i].Process(eventData);
                        }
                    }
                }
                else if (evt == 0xFF)
                {
                    var eCode = BitConverter.ToUInt16(data, 3);
                    var eventData = new byte[data.Length - 5];
                    Array.Copy(data, 5, eventData, 0, data.Length - 5);
                    for (int i = 0; i < _eventProcessor.VendorSpecificEventsTable.Length; i++)
                    {
                        if (eCode == _eventProcessor.VendorSpecificEventsTable[i].EventCode)
                        {
                            _eventProcessor.VendorSpecificEventsTable[i].Process(eventData);
                        }
                    }
                }
                else
                {
                    var eventData = new byte[data.Length - 2];
                    Array.Copy(data, 2, eventData, 0, data.Length - 2);
                    for (int i = 0; i < _eventProcessor.EventsTable.Length; i++)
                    {
                        if (evt == _eventProcessor.EventsTable[i].EventCode)
                        {
                            _eventProcessor.EventsTable[i].Process(eventData);
                        }
                    }
                }
            }
        }

        private bool NotifyAsyncEvent()
        {
            if (!_readPacketPool.Empty())
            {
                var readPacket = (DataPacket) _readPacketPool.RemoveHead();
                var dataLength = _hardwareInterface.Receive(ref readPacket.DataBuffer, 128);
                if (dataLength > 0)
                {
                    readPacket.DataLength = (byte) dataLength;
                    if (VerifyPacket(readPacket))
                        _readPacketCallbackQueue.InsertTail(readPacket);
                    else
                        _readPacketPool.InsertHead(readPacket);
                }
                else
                {
                    _readPacketPool.InsertHead(readPacket);
                }
            }
            else
            {
                return true;
            }

            return false;
        }

        private static bool VerifyPacket(DataPacket readPacket)
        {
            if (readPacket.DataBuffer is null)
                return false;
            if (readPacket.DataBuffer[0] != (byte) PacketType.Event)
                return false;
            if (readPacket.DataBuffer[2] != readPacket.DataLength - 3)
                return false;

            return true;
        }

        private static SpiPacket ToSpiPacket(byte[] data)
        {
            SpiPacket result = new()
            {
                Type = (PacketType) data![0],
                Data = new byte[data.Length - 1]
            };
            Array.Copy(data, 1, result.Data!, 0, data.Length - 1);
            return result;
        }

        private EventPacket ToEventPacket(byte[] data)
        {
            EventPacket result = new()
            {
                Event = (EventType) data![0],
                ParameterLength = data[1],
                Data = new byte[data.Length - 2]
            };
            Array.Copy(data, 2, result.Data!, 0, data.Length - 2);
            return result;
        }

        private struct Command
        {
            public ushort Opcode;
            public byte ParameterLength;
        }

        private struct DataPacket
        {
            public byte[] DataBuffer;
            public byte DataLength;
        }

        private struct SpiPacket
        {
            public PacketType Type;
            public byte[] Data;
        }

        private struct EventPacket
        {
            public EventType Event;
            public byte ParameterLength;
            public byte[] Data;
        }

        private struct CommandComplete
        {
            public byte CommandCount;
            public ushort Opcode;
        }

        private struct CommandStatus
        {
            public byte Status;
            public byte CommandCount;
            public ushort Opcode;
        }

        private struct MetaEvent
        {
            public uint SubEvent;
            public byte[] Data;
        }

        private enum EventType : byte
        {
            ConnectionComplete = 0x03,
            DisconnectionComplete = 0x05,
            EncryptChange = 0x08,
            ReadRemoteVersionComplete = 0x0C,
            CommandComplete = 0x0E,
            CommandStatus = 0x0F,
            HardWareError = 0x10,
            LeMetaEvent = 0x3E
        }

        private enum PacketType : byte
        {
            Command = 0x01,
            AclData = 0x02,
            ScoData = 0x03,
            Event = 0x04,
            Vendor = 0xff
        }

        public void Reset()
        {
            _hardwareInterface.Reset();
        }
    }
}