// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;

using Iot.Device.Ld2410.Commands;
using Iot.Device.Ld2410.Reporting;

namespace Iot.Device.Ld2410
{
    /// <summary>
    /// LD2410 radar module client.
    /// </summary>
    public sealed class Radar : IDisposable
    {
        private readonly SerialPort _serialPort;
        private readonly AutoResetEvent _onAckReceived;

        /// <summary>
        /// Measurement event handler delegate.
        /// </summary>
        /// <param name="sender">The sender object instance.</param>
        /// <param name="report">The report frame instance.</param>
        public delegate void MeasurementEventHandler(object sender, ReportFrame report);

        /// <summary>
        /// Occurs when a new measurement report has been received from the radar module.
        /// </summary>
        public event MeasurementEventHandler OnMeasurementReceived;

        /// <summary>
        /// Gets the current radar configurations.
        /// </summary>
        /// <remarks>
        /// These configurations are retrieved when <see cref="Connect"/> is executed.
        /// You must call <see cref="ReadConfigurations"/> as needed to refresh this property.
        /// </remarks>
        public DeviceConfiguration Configuration { get; }

        /// <summary>
        /// Gets the current firmware version running on the connected LD2410.
        /// </summary>
        public string FirmwareVersion { get; private set; }

        /// <summary>
        /// Gets the amount of time to wait for a response before abandoning the command execution.
        /// </summary>
        public TimeSpan CommandTimeout { get; }

        /// <summary>
        /// Gets a value indicating whether the radar is currently in configuration mode.
        /// </summary>
        public bool ConfigurationModeEnabled { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the radar is currently in engineering mode.
        /// </summary>
        public bool EngineeringModeEnabled { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Radar"/> class.
        /// </summary>
        /// <param name="serialPortName">The serial port name to use.</param>
        /// <param name="baudRate">The baud-rate to use.</param>
        /// <param name="commandTimeout">The command timeout. If not specified, the default is 5 seconds.</param>
        /// <exception cref="ArgumentNullException"><paramref name="serialPortName"/> was null or empty string.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="baudRate"/> was equal or less than 0.</exception>
        /// <remarks>
        /// Initializing the <see cref="Radar"/> is not enough to connect to the radar. You must call <see cref="Connect"/>
        /// when ready to start reading measurements and controlling the radar.
        /// </remarks>
        public Radar(string serialPortName, int baudRate = 256_000, TimeSpan commandTimeout = default)
        {
            if (string.IsNullOrEmpty(serialPortName))
            {
                throw new ArgumentNullException();
            }

            if (baudRate <= 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            Configuration = new DeviceConfiguration()
            {
                BaudRate = baudRate
            };

            _serialPort = new SerialPort(
                portName: serialPortName,
                baudRate: baudRate,
                parity: Parity.None,
                dataBits: 8,
                stopBits: StopBits.One);

            _serialPort.DataReceived += OnSerialDataReceived;

            _onAckReceived = new AutoResetEvent(initialState: false);
            CommandTimeout = commandTimeout == default ? TimeSpan.FromSeconds(5) : commandTimeout;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Radar"/> class using the specified <see cref="SerialPort"/> instance.
        /// </summary>
        /// <param name="serialPort">An initialized <see cref="SerialPort"/> instance connected to the radar module.</param>
        /// <param name="commandTimeout">The command timeout. If not specified, the default is 5 seconds.</param>
        /// <exception cref="ArgumentNullException"><paramref name="serialPort"/> was null or empty string.</exception>
        public Radar(SerialPort serialPort, TimeSpan commandTimeout = default)
        {
            if (serialPort == null)
            {
                throw new ArgumentNullException();
            }

            Configuration = new DeviceConfiguration()
            {
                BaudRate = serialPort.BaudRate,
            };

            _serialPort = serialPort;
            _serialPort.DataReceived += OnSerialDataReceived;

            _onAckReceived = new AutoResetEvent(initialState: false);
            CommandTimeout = commandTimeout == default ? TimeSpan.FromSeconds(5) : commandTimeout;
        }

        /// <summary>
        /// Sets the baud-rate for the connected radar.
        /// </summary>
        /// <param name="baudRate">The baud-rate to use.</param>
        /// <remarks>
        /// This will only be applied after the radar is restarted it.
        /// Call <see cref="Restart"/> to restart the radar and use the new baud-rate specified here.
        /// </remarks>
        public void SetBaudRate(BaudRate baudRate)
        {
            ThrowIfNotInConfigurationMode();

            SendCommand(new SetSerialPortBaudRateCommand(baudRate));
        }

        /// <summary>
        /// Connects to the radar and attempts to read its current configurations and firmware version.
        /// </summary>
        public void Connect()
        {
            if (_serialPort.IsOpen)
            {
                return;
            }

            _serialPort.Open();

            EnterConfigurationMode();
            ReadFirmwareVersion();
            ReadConfigurations();
            ExitConfigurationMode();
        }

        /// <summary>
        /// Disconnects from the radar.
        /// </summary>
        public void Disconnect()
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.Close();
            }
        }

        /// <summary>
        /// Puts the device into configuration mode.
        /// </summary>
        /// <remarks>
        /// Before calling any other "config" methods, you must call this one first.
        /// When a device is in configuration mode, it will stop reporting measurement data
        /// until <see cref="ExitConfigurationMode"/> is called.
        /// </remarks>
        public void EnterConfigurationMode()
        {
            SendCommand(new EnableConfigurationCommand());
            ConfigurationModeEnabled = true;

            Debug.WriteLine("Entered Config Mode.");
        }

        /// <summary>
        /// Exit the configuration mode.
        /// </summary>
        public void ExitConfigurationMode()
        {
            ThrowIfNotInConfigurationMode();

            SendCommand(new EndConfigurationCommand());
            ConfigurationModeEnabled = false;

            Debug.WriteLine("Exited Config Mode.");
        }

        /// <summary>
        /// Reads the current configurations from the radar and populates
        /// <see cref="Configuration"/> with that data.
        /// </summary>
        public void ReadConfigurations()
        {
            ThrowIfNotInConfigurationMode();

            var command = new ReadConfigurationsCommand();
            SendCommand(command);
        }

        /// <summary>
        /// Reads the current firmware version from the radar and populates
        /// <see cref="FirmwareVersion"/>.
        /// </summary>
        public void ReadFirmwareVersion()
        {
            ThrowIfNotInConfigurationMode();

            var command = new ReadFirmwareVersionCommand();
            SendCommand(command);
        }

        /// <summary>
        /// Sets the maximum distance of detection and "No-One" duration.
        /// </summary>
        /// <param name="maximumMovementDistanceGate">The maximum gate number that can detect movement.</param>
        /// <param name="maximumRestingDistanceGate">The maximum gate number that can detect static presense.</param>
        /// <param name="noOneDuration">The amount of time to wait before the radar reports no presence.</param>
        public void SetMaxDistanceGateAndUnmannedDuration(
            uint maximumMovementDistanceGate = 8,
            uint maximumRestingDistanceGate = 8,
            TimeSpan noOneDuration = default)
        {
            var command = new SetMaxDistanceGateAndNoOneDurationCommand(
                maximumMovementDistanceGate,
                maximumRestingDistanceGate,
                noOneDuration);

            SendCommand(command);
        }

        /// <summary>
        /// Commits gate configurations from <see cref="Configuration"/> to the device.
        /// </summary>
        public void CommitConfigurations()
        {
            ThrowIfNotInConfigurationMode();

            SetMaxDistanceGateAndUnmannedDuration(
                Configuration.MaxMovementDetectionDistanceGate,
                Configuration.MaxStationaryTargetDetectionDistanceGate,
                Configuration.NoOneDuration);

            foreach (var gateConfig in Configuration.GateConfiguration)
            {
                SendCommand(new SetGateSensitivityCommand(
                    gateConfig.Gate,
                    gateConfig.MotionSensitivity,
                    gateConfig.RestSensitivity));
            }
        }

        /// <summary>
        /// Enables or disables the engineering mode on the radar.
        /// </summary>
        /// <param name="enabled"><see langword="true"/> to enable engineering mode.</param>
        /// <remarks>
        /// Enabling engineering mode makes the radar append additional data to every measurement frame.
        /// This data will include energy measurements at every gate for moving and static targets.
        /// </remarks>
        public void SetEngineeringMode(bool enabled)
        {
            ThrowIfNotInConfigurationMode();

            SendCommand(new SetEngineeringModeCommand(enabled));

            EngineeringModeEnabled = enabled;
        }

        /// <summary>
        /// Causes the radar to reset all its configuration parameters to factory defaults.
        /// Factory default values will take effect after the radar is restarted.
        /// </summary>
        /// <param name="restartOnCompletion"><see langword="true"/> to immidately restart the radar after factory reset command completes.</param>
        public void RestoreFactorySettings(bool restartOnCompletion)
        {
            ThrowIfNotInConfigurationMode();

            SendCommand(new FactoryResetCommand());

            if (restartOnCompletion)
            {
                Restart();
            }
        }

        /// <summary>
        /// Restart the radar module.
        /// </summary>
        public void Restart()
        {
            ThrowIfNotInConfigurationMode();

            SendCommand(new RestartCommand());
            ConfigurationModeEnabled = false;
        }

        private void ThrowIfNotInConfigurationMode()
        {
            if (!ConfigurationModeEnabled)
            {
                throw new InvalidOperationException();
            }
        }

        private void SendCommand(CommandFrame command)
        {
            var serializedCommandFrame = command.Serialize();
            _serialPort.Write(serializedCommandFrame, offset: 0, count: serializedCommandFrame.Length);
            _onAckReceived.WaitOne((int)CommandTimeout.TotalMilliseconds, exitContext: false);
        }

        private void OnSerialDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                // need to make sure that there is data to be read, because
                // the event could have been queued several times and data read on a previous call
                if (_serialPort.BytesToRead <= 0)
                {
                    return;
                }

                var buffer = new byte[_serialPort.BytesToRead];
                var bytesRead = _serialPort.Read(buffer, offset: 0, count: buffer.Length);

                // figure out what we received
                if (ReportFrameParser.TryParse(buffer, index: 0, out ReportFrame reportFrame))
                {
                    if (OnMeasurementReceived != null)
                    {
                        OnMeasurementReceived(this, reportFrame);
                    }
                }
                else if (CommandAckParser.TryParse(buffer, index: 0, out CommandAckFrame ackFrame))
                {
                    if (ackFrame is ReadConfigurationsCommandAck readConfigResult)
                    {
                        Configuration.MaxDistanceGateIndex = readConfigResult.MaxDistanceGate;
                        Configuration.MaxMovementDetectionDistanceGate = readConfigResult.MotionRangeGate;
                        Configuration.MaxStationaryTargetDetectionDistanceGate = readConfigResult.StaticRangeGate;

                        Configuration.NoOneDuration = readConfigResult.NoOneDuration;

                        Configuration.GateConfiguration = new GateConfiguration[Configuration.NumberOfDistanceGatesAvailable];
                        for (var gate = 0; gate < Configuration.GateConfiguration.Length; gate++)
                        {
                            Configuration.GateConfiguration[gate] = new GateConfiguration((byte)gate)
                            {
                                MotionSensitivity = readConfigResult.MotionSensitivityLevelPerGate[gate],
                            };

                            // gate 0 & 1 cannot have custom static sensitivity
                            if (gate > 1)
                            {
                                Configuration.GateConfiguration[gate].RestSensitivity = readConfigResult.RestingSensitivityLevelPerGate[gate];
                            }
                        }
                    }
                    else if (ackFrame is ReadFirmwareVersionCommandAck firmwareVersionCommandAck)
                    {
                        foreach (var b in buffer)
                        {
                            Console.Write(b.ToString("x2") + " ");
                        }

                        FirmwareVersion = $"{firmwareVersionCommandAck.Major}.{firmwareVersionCommandAck.Minor}.";
                        foreach (var patchPart in firmwareVersionCommandAck.Patch)
                        {
                            FirmwareVersion += patchPart.ToString("X2");
                        }
                    }

                    Debug.WriteLine("==ACK==");

                    _onAckReceived.Set();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LD2410 ERROR: {ex.Message}");
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _serialPort?.Dispose();
        }
    }
}
