using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;

using Ld2410.Commands;
using Ld2410.Reporting;

namespace Ld2410
{
    public sealed class Ld2410 : IDisposable
    {
        private readonly SerialPort serialPort;
        private readonly AutoResetEvent onAckReceived;

        public delegate void MeasurementEventHandler(object sender, ReportFrame report);
        public event MeasurementEventHandler OnMeasurementReceived;

        public DeviceConfiguration Configuration { get; }

        public bool ConfigurationModeEnabled { get; private set; }

        public bool EngineeringModeEnabled { get; private set; }

        public string FirmwareVersion { get; private set; }

        public TimeSpan CommandTimeout { get; }

        public Ld2410(string serialPortName, int baudRate = 256_000, TimeSpan commandTimeout = default)
        {
            if (string.IsNullOrEmpty(serialPortName))
            {
                throw new ArgumentNullException();
            }

            if (baudRate <= 0)
            {
                throw new ArgumentOutOfRangeException();
            }

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

            this.onAckReceived = new AutoResetEvent(initialState: false);
            this.CommandTimeout = commandTimeout == default ? TimeSpan.FromSeconds(5) : commandTimeout;
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
            this.SendCommand(new EnableConfigurationCommand());
            this.ConfigurationModeEnabled = true;

            Debug.WriteLine("Entered Config Mode.");
        }

        public void ExitConfigurationMode()
        {
            this.ThrowIfNotInConfigurationMode();

            this.SendCommand(new EndConfigurationCommand());
            this.ConfigurationModeEnabled = false;

            Debug.WriteLine("Exited Config Mode.");
        }

        public void SetMaxDistanceGateAndUnmannedDuration(
            uint maximumMovementDistanceGate = 8,
            uint maximumRestingDistanceGate = 8,
            TimeSpan noOneDuration = default
            )
        {
            var command = new SetMaxDistanceGateAndNoOneDurationCommand(
                maximumMovementDistanceGate,
                maximumRestingDistanceGate,
                noOneDuration
                );

            this.SendCommand(command);
        }

        public void ReadConfigurations()
        {
            var command = new ReadConfigurationsCommand();
            this.SendCommand(command);

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

        private void SendCommand(CommandFrame command)
        {
            var serializedCommandFrame = command.Serialize();
            this.serialPort.Write(serializedCommandFrame, offset: 0, count: serializedCommandFrame.Length);
            this.onAckReceived.WaitOne((int)this.CommandTimeout.TotalMilliseconds, exitContext: false);
        }

        private void OnSerialDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Debug.WriteLine("===DATA===");

            try
            {
                // need to make sure that there is data to be read, because
                // the event could have been queued several times and data read on a previous call
                if (this.serialPort.BytesToRead <= 0)
                {
                    Debug.WriteLine("===0 Bytes===");
                    return;
                }

                var buffer = new byte[this.serialPort.BytesToRead];
                var bytesRead = this.serialPort.Read(buffer, offset: 0, count: buffer.Length);

                // figure out what we received
                if (ReportFrameParser.TryParse(buffer, index: 0, out ReportFrame reportFrame))
                {
                    Debug.WriteLine("===Measurement===");

                    if (this.OnMeasurementReceived != null)
                    {
                        this.OnMeasurementReceived(this, reportFrame);
                    }
                }
                else if (CommandAckParser.TryParse(buffer, index: 0, out CommandAckFrame ackFrame)) // TODO: write actual condition
                {
                    Debug.WriteLine("===ACK===");
                    foreach (var b in buffer)
                        Debug.Write($"{b:x2} ");

                    if (ackFrame is ReadConfigurationsCommandAck readConfigResult)
                    {
                        Debug.WriteLine(readConfigResult.MaxDistanceGate.ToString("x2"));
                    }

                    this.onAckReceived.Set();
                }
                else
                {
                    Debug.WriteLine("Unknown Data...");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("===ERROR===");
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            serialPort?.Dispose();
        }
    }
}
