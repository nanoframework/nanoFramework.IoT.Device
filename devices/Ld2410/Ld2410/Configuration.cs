using System;

using UnitsNet;

namespace Ld2410
{
	/// <summary>
	/// Defines the configurations for a <see cref="Ld2410"/> device.
	/// </summary>
	public sealed class Configuration
	{
		private const ushort MaxSupportedDistanceGates = 8;
		private const float DistanceGate = 0.75f;

		public const float MaxSupportedDistanceInCentemeters = MaxSupportedDistanceGates * DistanceGate;

		private Length maximumMovementDetectionDistance;
		private Length maximumRestingDetectionDistance;

		/// <summary>
		/// Gets or sets the farthest detectable distance of moving targets.
		/// Only human targets appearing within this distance range will be detected.
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException">The specified distance is greater than the maximum supported distance as per <see cref="MaxSupportedDistanceInCentemeters"/></exception>
		public Length MaximumMovementDetectionDistance
		{
			get => this.maximumMovementDetectionDistance;
			set
			{
				if (value.Centimeters > MaxSupportedDistanceInCentemeters)
				{
					throw new ArgumentOutOfRangeException();
				}

				this.maximumMovementDetectionDistance = value;
			}
		}

		/// <summary>
		/// Gets or sets the farthest detectable distance of resting targets.
		/// Only human targets appearing within this distance range will be detected.
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException">The specified distance is greater than the maximum supported distance as per <see cref="MaxSupportedDistanceInCentemeters"/></exception>
		public Length MaximumRestingDetectionDistance
		{
			get => this.maximumRestingDetectionDistance;
			set
			{
				if (value.Centimeters > MaxSupportedDistanceInCentemeters)
				{
					throw new ArgumentOutOfRangeException();
				}

				this.maximumRestingDetectionDistance = value;
			}
		}

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
			this.MaximumMovementDetectionDistance = this.MaximumRestingDetectionDistance = Length.FromCentimeters(MaxSupportedDistanceInCentemeters);
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

	/// <summary>
	/// Defines a specific gate's sensitivity configurations.
	/// </summary>
	public sealed class GateConfiguration
	{
		private ushort gate;
		private ushort restSensitivity;
		private ushort motionSensitivity;

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
		/// The motion sensitivity threshold. Any target with a movement "energy" greater than the specified range will be detected, otherwise it will be ignored.
		/// Setting a gate's sensitivity to 100 will effectively disable detection of movement within that distance gate.
		/// This can enable fine control over the range of distance to detect movements in. 
		/// For example, All gates except 4 and 7 are set to 100. This means movement will only be detected in gates 4 and 5 (between 3 meters and 5.25 meters from the radar).
		/// </summary>
		/// <remarks>There is no defined unit of measurement for this and should be treated as a percentage. It is recommended to experiment with the values to find the right threshold for the use-case at hand.</remarks>
		/// <exception cref="ArgumentOutOfRangeException">This value cannot be less than 0 or greater than 100.</exception>
		public ushort MotionSensitivity
		{
			get => this.motionSensitivity;
			set
			{
				if (value is < 0 or > 100)
				{
					throw new ArgumentOutOfRangeException();
				}

				this.motionSensitivity = value;
			}
		}

		/// <summary>
		/// The resting sensitivity threshold. Any target without movement "energy" (resting) greater than the specified range will be detected, otherwise it will be ignored.
		/// Setting a gate's sensitivity to 100 will effectively disable detection of resting targets within that distance gate.
		/// This can enable fine control over the range of distance to detect resting targets in. 
		/// For example, All gates except 4 and 7 are set to 100. This means resting targets will only be detected in gates 4 and 5 (between 3 meters and 5.25 meters from the radar).
		/// </summary>
		/// <remarks>There is no defined unit of measurement for this and should be treated as a percentage. It is recommended to experiment with the values to find the right threshold for the use-case at hand.</remarks>
		/// <exception cref="InvalidOperationException">Cannot set this value for Gate 0 and 1.</exception>
		/// <exception cref="ArgumentOutOfRangeException">This value cannot be less than 0 or greater than 100.</exception>
		public ushort RestSensitivity
		{
			get => this.restSensitivity;
			set
			{
				if (this.Gate == 0 || this.Gate == 1)
				{
					throw new InvalidOperationException();
				}

				if (value is < 0 or > 100)
				{
					throw new ArgumentOutOfRangeException();
				}

				this.restSensitivity = value;
			}
		}
	}
}
