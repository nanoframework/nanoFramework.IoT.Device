using System;
using System.Reflection;

using UnitsNet;

namespace Ld2410
{
	/// <summary>
	/// Defines the configurations for a <see cref="Ld2410"/> device.
	/// </summary>
	public sealed class DeviceConfiguration
	{
		public readonly static byte DistancePerGateInCm = 75;
		public const ushort MaxSupportedNoOneDuration = ushort.MaxValue;

		private Length maximumMovementDetectionDistance;
		private Length maximumRestingDetectionDistance;
		private TimeSpan noOneDuration;

		/// <summary>
		/// Gets or sets the max number of distance gates available on the radar module.
		/// </summary>
		/// <remarks>
		/// Each gate covers a distance of 75cm. If a module reports max distance gate of 8, then it covers 8x75=600cm or 6m.
		/// </remarks>
		public byte MaxDistanceGate { get; set; }

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
				if (value.Centimeters > GetMaxSupportedDistance(this.MaxDistanceGate).Centimeters)
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
				if (value.Centimeters > GetMaxSupportedDistance(this.MaxDistanceGate).Centimeters)
				{
					throw new ArgumentOutOfRangeException();
				}

				this.maximumRestingDetectionDistance = value;
			}
		}

		/// <summary>
		/// The duration to wait before no movement is confirmed.
		/// </summary>
		public TimeSpan NoOneDuration
		{
			get => noOneDuration;
			set
			{
				if (value.TotalSeconds < 0 || value.TotalSeconds > MaxSupportedNoOneDuration)
				{
					throw new ArgumentOutOfRangeException();
				}

				this.noOneDuration = value;
			}
		}

		/// <summary>
		/// Gets or sets the serial port baud rate.
		/// </summary>
		public int BaudRate { get; set; } = -1;

		/// <summary>
		/// Gets all available gate configurations.
		/// </summary>
		public GateConfiguration[] GateConfiguration { get; set; }

		public static Length GetMaxSupportedDistance(byte numberOfGates)
			=> Length.FromCentimeters(numberOfGates * DistancePerGateInCm);
	}

	/// <summary>
	/// Defines a specific gate's sensitivity configurations.
	/// </summary>
	public sealed class GateConfiguration
	{
		private ushort restSensitivity;
		private ushort motionSensitivity;

		public byte Gate { get; }

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

		/// <summary>
		/// Initializes a new instance of <see cref="GateConfiguration"/> with the specified gate number.
		/// </summary>
		/// <param name="gate">The gate number to initialize the <see cref="GateConfiguration"/> class for.</param>
        public GateConfiguration(byte gate)
        {
			this.Gate = gate;
        }

#if DEBUG
        public override string ToString()
        {
			return $"Gate: {this.Gate}, Motion Sensitivity: {this.MotionSensitivity}, Rest Sensitivity: {this.RestSensitivity}";
        }
#endif
    }
}
