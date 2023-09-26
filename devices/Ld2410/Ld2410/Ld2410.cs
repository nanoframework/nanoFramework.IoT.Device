using System;
using System.IO.Ports;
using System.Threading;

namespace Ld2410
{
    public sealed class Ld2410 : IDisposable
    {
        private readonly SerialPort serialPort;
        private readonly AutoResetEvent autoResetEvent;

        public delegate void MeasurementEventHandler(object sender, ReportFrame report);

        public event MeasurementEventHandler OnMeasurementReceived;

        public DeviceConfiguration Configuration { get; }

        public bool ConfigurationModeEnabled { get; private set; }

        public bool EngineeringModeEnabled { get; private set; }

        public string FirmwareVersion { get; private set; }

        public Ld2410(string serialPortName, int baudRate = 256_000)
        {
            this.Configuration = new()
            {
                BaudRate = baudRate
            };

            this.serialPort = new SerialPort(portName: serialPortName,
                baudRate: baudRate,
                parity: Parity.None,
                dataBits: 8,
                stopBits: StopBits.One);

            this.serialPort.DataReceived += OnSerialDataReceived;

            this.autoResetEvent = new AutoResetEvent(initialState: false);
        }

        public static int FindBaudRate(string serialPortName)
        {
            return 256_000;
        }

        public void Connect()
        {
            if (!this.serialPort.IsOpen)
            {
                this.serialPort.Open();
            }
        }

        public void Disconnect()
        {
            if (this.serialPort.IsOpen)
            {
                this.serialPort.Close();
            }
        }

        public void EnterConfigurationMode()
        {
            this.SendCommand(Command.EnableConfiguration, new byte[] { 0x01, 0x00 });
            this.ConfigurationModeEnabled = true;
        }

        public void ExitConfigurationMode()
        {
            this.ThrowIfNotInConfigurationMode();

            this.SendCommand(Command.EnableConfiguration, new byte[] { 0x01, 0x00 });
            this.ConfigurationModeEnabled = false;
        }

        public void ReadConfigurations()
        {
            this.ThrowIfNotInConfigurationMode();
        }

        public void CommitConfigurations()
        {
            this.ThrowIfNotInConfigurationMode();
        }

        public void SetEngineeringMode(bool enabled)
        {
            this.ThrowIfNotInConfigurationMode();

            this.EngineeringModeEnabled = enabled;
        }

        public void RestoreFactorySettings(bool restartOnCompletion)
        {
            this.ThrowIfNotInConfigurationMode();
        }

        public void Restart()
        {
            this.ThrowIfNotInConfigurationMode();
        }

        private void ThrowIfNotInConfigurationMode()
        {
            if (!this.ConfigurationModeEnabled)
            {
                throw new InvalidOperationException();
            }
        }

        private void SendCommand(Command command, byte[] value)
        {
            var commandFrame = new ProtocolCommandFrame(command, value);
            var serializedCommandFrame = commandFrame.Serialize();
            this.serialPort.Write(serializedCommandFrame, offset: 0, count: serializedCommandFrame.Length);
            this.autoResetEvent.WaitOne();
        }

        private void OnSerialDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // need to make sure that there is data to be read, because
            // the event could have been queued several times and data read on a previous call
            if (this.serialPort.BytesToRead <= 0)
            {
                return;
            }

            var buffer = new byte[this.serialPort.BytesToRead];
            var bytesRead = this.serialPort.Read(buffer, offset: 0, count: buffer.Length);

            // figure out what we received
            if (ReportFrameDecoder.TryParse(buffer, startIndex: 0, out ReportFrame reportFrame))
            {
                if (this.OnMeasurementReceived != null)
                {
                    this.OnMeasurementReceived(this, reportFrame);
                }
            }
            else // TODO: write actual condition
            {
                this.autoResetEvent.Set();
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            serialPort?.Dispose();
        }
    }
}
