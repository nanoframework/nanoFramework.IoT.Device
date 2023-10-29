using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;

using Ld2410.Commands;
using Ld2410.Reporting;

namespace Ld2410
{
	/// <summary>
	/// LD2410 radar module client.
	/// </summary>
	public sealed class Radar : IDisposable
	{
		private readonly SerialPort serialPort;
		private readonly AutoResetEvent onAckReceived;

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
		/// These configurations are retrieved when <see cref="Radar.Connect"/> is executed.
		/// You must call <see cref="Radar.ReadConfigurations"/> as needed to refresh this property.
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
		/// Gets a value indicating wheter the radar is currently in engineering mode.
		/// </summary>
		public bool EngineeringModeEnabled { get; private set; }

		/// <summary>
		/// Initializes a new instance of <see cref="Radar"/>.
		/// </summary>
		/// <param name="serialPortName">The serial port name to use.</param>
		/// <param name="baudRate">The baud-rate to use.</param>
		/// <param name="commandTimeout">The command timeout. If not specified, the default is 5 seconds.</param>
		/// <exception cref="ArgumentNullException"><paramref name="serialPortName"/> was null or empty string.</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="baudRate"/> was equal or less than 0.</exception>
		/// <remarks>
		/// Initializing the <see cref="Radar"/> is not enough to connect to the radar. You must call <see cref="Radar.Connect"/>
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

		/// <summary>
		/// Initializes a new instance of <see cref="Radar"/> using the specified <see cref="SerialPort"/> instance.
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

			this.Configuration = new()
			{
				BaudRate = serialPort.BaudRate,
			};

			this.serialPort = serialPort;
			this.serialPort.DataReceived += OnSerialDataReceived;

			this.onAckReceived = new AutoResetEvent(initialState: false);
			this.CommandTimeout = commandTimeout == default ? TimeSpan.FromSeconds(5) : commandTimeout;
		}

		/// <summary>
		/// sets the baud-rate for the connected radar.
		/// </summary>
		/// <param name="baudRate">The baud-rate to use.</param>
		/// <remarks>
		/// This will only be applied after the radar is restarted it.
		/// Call <see cref="Radar.Restart"/> to restart the radar and use the new baud-rate specified here.
		/// </remarks>
		public void SetBaudRate(BaudRate baudRate)
		{
			this.ThrowIfNotInConfigurationMode();

			this.SendCommand(new SetSerialPortBaudRateCommand(baudRate));
		}

		/// <summary>
		/// Connects to the radar and attempts to read its current configurations and firmware version.
		/// </summary>
		public void Connect()
		{
			if (this.serialPort.IsOpen)
				return;

			this.serialPort.Open();

			this.EnterConfigurationMode();
			this.ReadFirmwareVersion();
			this.ReadConfigurations();
			this.ExitConfigurationMode();
		}

		/// <summary>
		/// Disconnects from the radar.
		/// </summary>
		public void Disconnect()
		{
			if (this.serialPort.IsOpen)
			{
				this.serialPort.Close();
			}
		}

		/// <summary>
		/// Puts the device into configuration mode.
		/// </summary>
		/// <remarks>
		/// Before calling any other "config" methods, you must call this one first.
		/// When a device is in configuration mode, it will stop reporting measurement data
		/// until <see cref="Radar.ExitConfigurationMode"/> is called.
		/// </remarks>
		public void EnterConfigurationMode()
		{
			this.SendCommand(new EnableConfigurationCommand());
			this.ConfigurationModeEnabled = true;

			Debug.WriteLine("Entered Config Mode.");
		}

		/// <summary>
		/// Exit the configuration mode.
		/// </summary>
		public void ExitConfigurationMode()
		{
			this.ThrowIfNotInConfigurationMode();

			this.SendCommand(new EndConfigurationCommand());
			this.ConfigurationModeEnabled = false;

			Debug.WriteLine("Exited Config Mode.");
		}

		/// <summary>
		/// Reads the current configurations from the radar and populates
		/// <see cref="Radar.Configuration"/> with that data.
		/// </summary>
		public void ReadConfigurations()
		{
			this.ThrowIfNotInConfigurationMode();

			var command = new ReadConfigurationsCommand();
			this.SendCommand(command);
		}

		/// <summary>
		/// Reads the current firmware version from the radar and populates
		/// <see cref="Radar.FirmwareVersion"/>.
		/// </summary>
		public void ReadFirmwareVersion()
		{
			this.ThrowIfNotInConfigurationMode();

			var command = new ReadFirmwareVersionCommand();
			this.SendCommand(command);
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

		/// <summary>
		/// Commits gate configurations from <see cref="Radar.Configuration"/> to the device.
		/// </summary>
		public void CommitConfigurations()
		{
			this.ThrowIfNotInConfigurationMode();

			this.SetMaxDistanceGateAndUnmannedDuration(
				this.Configuration.MaximumMovementDetectionDistanceGate,
				this.Configuration.MaximumRestingDetectionDistanceGate,
				this.Configuration.NoOneDuration
				);

			foreach (var gateConfig in this.Configuration.GateConfiguration)
			{
				this.SendCommand(new SetGateSensitivityCommand
					(
						gateConfig.Gate,
						gateConfig.MotionSensitivity,
						gateConfig.RestSensitivity
					));
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
			this.ThrowIfNotInConfigurationMode();

			this.SendCommand(new SetEngineeringModeCommand(enabled));

			this.EngineeringModeEnabled = enabled;
		}

		/// <summary>
		/// Causes the radar to reset all its configuration parameters to factory defaults.
		/// Factory default values will take effect after the radar is restarted.
		/// </summary>
		/// <param name="restartOnCompletion"><see langword="true"/> to immidately restart the radar after factory reset command completes.</param>
		public void RestoreFactorySettings(bool restartOnCompletion)
		{
			this.ThrowIfNotInConfigurationMode();

			this.SendCommand(new FactoryResetCommand());

			if (restartOnCompletion)
			{
				this.Restart();
			}
		}

		/// <summary>
		/// Restart the radar module.
		/// </summary>
		public void Restart()
		{
			this.ThrowIfNotInConfigurationMode();

			this.SendCommand(new RestartCommand());
			this.ConfigurationModeEnabled = false;
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
			try
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
				if (ReportFrameParser.TryParse(buffer, index: 0, out ReportFrame reportFrame))
				{
					if (this.OnMeasurementReceived != null)
					{
						this.OnMeasurementReceived(this, reportFrame);
					}
				}
				else if (CommandAckParser.TryParse(buffer, index: 0, out CommandAckFrame ackFrame)) // TODO: write actual condition
				{
					if (ackFrame is ReadConfigurationsCommandAck readConfigResult)
					{
						this.Configuration.MaxDistanceGateIndex = readConfigResult.MaxDistanceGate;
						this.Configuration.MaximumMovementDetectionDistanceGate = readConfigResult.MotionRangeGate;
						this.Configuration.MaximumRestingDetectionDistanceGate = readConfigResult.StaticRangeGate;

						this.Configuration.NoOneDuration = readConfigResult.NoOneDuration;

						this.Configuration.GateConfiguration = new GateConfiguration[this.Configuration.NumberOfDistanceGatesAvailable];
						for (var gate = 0; gate < this.Configuration.GateConfiguration.Length; gate++)
						{
							this.Configuration.GateConfiguration[gate] = new GateConfiguration((byte)gate)
							{
								MotionSensitivity = readConfigResult.MotionSensitivityLevelPerGate[gate],
							};

							if (gate > 1) // gate 0 & 1 cannot have custom static sensitivity
							{
								this.Configuration.GateConfiguration[gate].RestSensitivity = readConfigResult.RestingSensitivityLevelPerGate[gate];
							}
						}
					}
					else if (ackFrame is ReadFirmwareVersionCommandAck firmwareVersionCommandAck)
					{
						foreach (var b in buffer)
						{
							Console.Write(b.ToString("x2") + " ");
						}

						this.FirmwareVersion = $"{firmwareVersionCommandAck.Major}.{firmwareVersionCommandAck.Minor}.";
						foreach (var patchPart in firmwareVersionCommandAck.Patch)
						{
							this.FirmwareVersion += patchPart.ToString("X2");
						}
					}

					Debug.WriteLine("==ACK==");

					this.onAckReceived.Set();
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
			serialPort?.Dispose();
		}
	}
}
