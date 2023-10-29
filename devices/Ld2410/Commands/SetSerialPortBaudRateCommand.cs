using System.Buffers.Binary;

namespace Ld2410.Commands
{
	internal sealed class SetSerialPortBaudRateCommand : CommandFrame
    {
        public SetSerialPortBaudRateCommand(BaudRate baudRate = BaudRate.BaudRate256000)
            : base(CommandWord.SetBaudRate)
        {
            BinaryPrimitives.WriteUInt16LittleEndian((this.Value = new byte[2]), (ushort)baudRate);
        }
    }

    internal sealed class SetSerialPortBaudRateCommandAck : CommandAckFrame
    {
        public SetSerialPortBaudRateCommandAck(bool isSuccess)
            : base(CommandWord.SetBaudRate, isSuccess)
        {
        }
    }
}
