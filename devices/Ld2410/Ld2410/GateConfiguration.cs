using System;

using UnitsNet;

namespace Ld2410
{
	/// <summary>
	/// Defines a specific gate's sensitivity configurations.
	/// </summary>
	public sealed class GateConfiguration
	{
		/// <summary>
		/// The distance, in centimeters, per each radar distance gate.
		/// </summary>
		public readonly static byte DistancePerGateInCm = 75;

		private ushort restSensitivity;
		private ushort motionSensitivity;

		/// <summary>
		/// Gets the gate number of this configuration instance.
		/// </summary>
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
		/// Gets the detection distance of this gate.
		/// </summary>
		public Length DetectionDistance
			=> Length.FromCentimeters(this.Gate * DistancePerGateInCm);

		/// <summary>
		/// Initializes a new instance of <see cref="GateConfiguration"/> with the specified gate number.
		/// </summary>
		/// <param name="gate">The gate number to initialize the <see cref="GateConfiguration"/> class for.</param>
		public GateConfiguration(byte gate)
		{
			this.Gate = gate;
		}

#if DEBUG
		/// <inheritdoc/>
		public override string ToString()
		{
			return $"Gate: {this.Gate}, Motion Sensitivity: {this.MotionSensitivity}, Rest Sensitivity: {this.RestSensitivity}";
		}
#endif
	}
}
