using System;
using System.Diagnostics;
using System.IO.Ports;

namespace Ld2410
{
    public sealed class Ld2410 : IDisposable
    {
        private readonly SerialPort serialPort;

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
            this.ConfigurationModeEnabled = true;
        }

        public void ExitConfigurationMode()
        {
            this.ThrowIfNotInConfigurationMode();

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

//#if DEBUG
//            Debug.WriteLine();
//            for (var i = 0; i < bytesRead; i++)
//            {
//                Debug.Write(buffer[i].ToString("x2"));
//            }

//#endif

            // figure out what we received
            if (ReportFrameDecoder.TryParse(buffer, startIndex: 0, out ReportFrame reportFrame))
            {
                if (this.OnMeasurementReceived != null)
                {
                    this.OnMeasurementReceived(this, reportFrame);
                }
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            serialPort?.Dispose();
        }
    }
}
