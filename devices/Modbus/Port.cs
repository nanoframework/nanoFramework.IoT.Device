using System;
using System.IO.Ports;
using System.Threading;

namespace Iot.Device.Modbus
{
    public abstract class Port : IDisposable
    {
        private SerialPort _serialPort;

        public Port(
            string portName,
            int baudRate,
            Parity parity,
            int dataBits,
            StopBits stopBits,
            int ReceivedBytesThreshold = 1
            )
        {
            _serialPort = new SerialPort(portName, baudRate, parity, dataBits, stopBits);
            _serialPort.Mode = SerialMode.RS485;
            _serialPort.Handshake = Handshake.RequestToSend;
            _serialPort.ReceivedBytesThreshold = ReceivedBytesThreshold;
        }

        public Port(SerialPort port, int ReceivedBytesThreshold = 1)
        {
            _serialPort = port;
            _serialPort.Mode = SerialMode.RS485;

            if (ReceivedBytesThreshold != 1)
                _serialPort.ReceivedBytesThreshold = ReceivedBytesThreshold;
        }

        public string PortName => _serialPort != null ? _serialPort.PortName : String.Empty;
        public bool IsOpen => _serialPort != null ? _serialPort.IsOpen : false;

        public int ReadTimeout
        {
            get => _serialPort != null ? _serialPort.ReadTimeout : Timeout.Infinite;
            set { if (_serialPort != null) _serialPort.ReadTimeout = value; }
        }

        public int WriteTimeout
        {
            get => _serialPort != null ? _serialPort.WriteTimeout : Timeout.Infinite;
            set { if (_serialPort != null) _serialPort.WriteTimeout = value; }
        }

        public int ReadBufferSize
        {
            get => _serialPort != null ? _serialPort.ReadBufferSize : 0;
            set { if (_serialPort != null) _serialPort.ReadBufferSize = value; }
        }

        public int WriteBufferSize
        {
            get => _serialPort != null ? _serialPort.WriteBufferSize : 0;
            set { if (_serialPort != null) _serialPort.WriteBufferSize = value; }
        }

        protected bool CheckOpen()
        {
            if (_serialPort != null)
            {
                if (!_serialPort.IsOpen)
                {
                    _serialPort.DataReceived += (sender, args) =>
                    {
                        if (_serialPort.BytesToRead > 0)
                            this.DataReceived(_serialPort.BytesToRead);
                    };
                    _serialPort.Open();
                }

                return _serialPort.IsOpen;
            }
            return false;
        }

        protected virtual void DataReceived(int bytesToRead)
        { }

        protected byte DataRead()
        {
            //return DataRead(1)[0];
            if (this.CheckOpen())
                return (byte)_serialPort.ReadByte();
            else
                return 0;
        }

        protected byte[] DataRead(int length)
        {
            var buffer = new byte[length];
            if (this.CheckOpen())
            {
                for (int offset = 0; offset < buffer.Length;)
                {
                    int count = _serialPort.Read(buffer, offset, buffer.Length - offset);
                    if (count < 1)
                        break;

                    offset += count;
                }
            }

            return buffer;
        }

        protected void DataWrite(byte value)
        {
            if (CheckOpen())
                _serialPort.WriteByte(value);
        }

        protected void DataWrite(byte[] buffer, int offset, int count)
        {
            if (CheckOpen())
            {
                _serialPort.Write(buffer, offset, count);
#if DEBUG
                System.Diagnostics.Debug.WriteLine(String.Format("{0} TX ({1}): {2}", this.PortName, buffer.Length, Format(buffer)));
#endif
            }
        }

        protected static string Format(byte[] buffer)
        {
            if (buffer.Length > 0)
            {
                var sb = new System.Text.StringBuilder();
                foreach (var b in buffer)
                {
                    sb.Append(b.ToString("X2"));
                    sb.Append(' ');
                }
                return sb.ToString();
            }
            else
                return string.Empty;
        }

        public void Dispose()
        {
            if (_serialPort != null)
            {
                if (_serialPort.IsOpen)
                    _serialPort.Close();

                _serialPort.Dispose();
                _serialPort = null;
            }
        }
    }
}
