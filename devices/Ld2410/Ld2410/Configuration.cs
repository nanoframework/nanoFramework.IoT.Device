using System;

namespace Ld2410
{
	public sealed class Configuration
	{
		public ushort MaximumMovingDistanceGate { get; set; } = 8;

		public ushort MaximumRestingDistanceDoor { get; set; } = 8;

		/// <summary>
		/// The duration to wait before no movement is confirmed.
		/// </summary>
		public TimeSpan NoOneDuration { get; set; }

		/// <summary>
		/// Gets or sets the serial port baud rate.
		/// </summary>
		public BaudRate BaudRate { get; set; } = BaudRate.BaudRate256000;

		/// <summary>
		/// Gets all available gate configurations.
		/// </summary>
		public GateConfiguration[] GateConfiguration { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="Configuration"/> class.
		/// </summary>
		public Configuration()
		{
			this.NoOneDuration = TimeSpan.FromSeconds(5);

			this.GateConfiguration = new GateConfiguration[]
			{
				new() { Gate = 0, MotionSensitivity = 50 },
				new() { Gate = 1, MotionSensitivity = 50 },
				new() { Gate = 2, MotionSensitivity = 40, RestSensitivity = 40 },
				new() { Gate = 3, MotionSensitivity = 30, RestSensitivity = 40 },
				new() { Gate = 4, MotionSensitivity = 20, RestSensitivity = 30 },
				new() { Gate = 5, MotionSensitivity = 15, RestSensitivity = 30 },
				new() { Gate = 6, MotionSensitivity = 15, RestSensitivity = 20 },
				new() { Gate = 7, MotionSensitivity = 15, RestSensitivity = 20 },
				new() { Gate = 8, MotionSensitivity = 15, RestSensitivity = 20 },
			};
		}
	}

	public sealed class GateConfiguration
	{
		private ushort gate;
		private ushort restSensitivity;

		/// <summary>
		/// The gate number.
		/// </summary>
		public ushort Gate
		{
			get => this.gate;
			set
			{
				if (value > 8)
				{
					throw new ArgumentOutOfRangeException();
				}

				this.gate = value;
			}
		}

		/// <summary>
		/// The motion sensitivity threshold.
		/// </summary>
		public ushort MotionSensitivity { get; set; }

		/// <summary>
		/// Rest sensitivity threshold for the specified gate.
		/// </summary>
		/// <exception cref="InvalidOperationException">Cannot set this value for Gate 0 and 1.</exception>
		public ushort RestSensitivity
		{
			get => this.restSensitivity;
			set
			{
				if (this.Gate == 0 || this.Gate == 1)
				{
					throw new InvalidOperationException();
				}

				this.restSensitivity = value;
			}
		}
	}
}
