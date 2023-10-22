namespace Ld2410.Reporting
{
	/// <summary>
	/// Represents the engineering more report frame produced by the radar when engineering mode is enabled.
	/// </summary>
	public sealed class EngineeringModeReportFrame : BasicReportFrame
	{
		/// <summary>
		/// Gets the maximum distance gate supported by the radar for moving targets.
		/// </summary>
		public byte MaxMovingDistanceGate { get; internal set; }

		/// <summary>
		/// Gets the maximum distance gate supported by the radar for static targets.
		/// </summary>
		public byte MaxStaticDistanceGate { get; internal set; }

		/// <summary>
		/// Gets the movement energy at distance gate 0.
		/// </summary>
		public byte Gate0MovementEnergy { get; internal set; }

		/// <summary>
		/// Gets the movement energy at distance gate 1.
		/// </summary>
		public byte Gate1MovementEnergy { get; internal set; }

		/// <summary>
		/// Gets the movement energy at distance gate 2.
		/// </summary>
		public byte Gate2MovementEnergy { get; internal set; }

		/// <summary>
		/// Gets the movement energy at distance gate 3.
		/// </summary>
		public byte Gate3MovementEnergy { get; internal set; }


		/// <summary>
		/// Gets the movement energy at distance gate 4.
		/// </summary>
		public byte Gate4MovementEnergy { get; internal set; }

		/// <summary>
		/// Gets the movement energy at distance gate 5.
		/// </summary>
		public byte Gate5MovementEnergy { get; internal set; }

		/// <summary>
		/// Gets the movement energy at distance gate 6.
		/// </summary>
		public byte Gate6MovementEnergy { get; internal set; }

		/// <summary>
		/// Gets the movement energy at distance gate 7.
		/// </summary>
		public byte Gate7MovementEnergy { get; internal set; }

		/// <summary>
		/// Gets the movement energy at distance gate 8.
		/// </summary>
		public byte Gate8MovementEnergy { get; internal set; }

		/// <summary>
		/// Gets the static energy at distance gate 0.
		/// </summary>
		public byte Gate0StaticEnergy { get; internal set; }

		/// <summary>
		/// Gets the static energy at distance gate 1.
		/// </summary>
		public byte Gate1StaticEnergy { get; internal set; }

		/// <summary>
		/// Gets the static energy at distance gate 2.
		/// </summary>
		public byte Gate2StaticEnergy { get; internal set; }

		/// <summary>
		/// Gets the static energy at distance gate 3.
		/// </summary>
		public byte Gate3StaticEnergy { get; internal set; }

		/// <summary>
		/// Gets the static energy at distance gate 4.
		/// </summary>
		public byte Gate4StaticEnergy { get; internal set; }

		/// <summary>
		/// Gets the static energy at distance gate 5.
		/// </summary>
		public byte Gate5StaticEnergy { get; internal set; }

		/// <summary>
		/// Gets the static energy at distance gate 6.
		/// </summary>
		public byte Gate6StaticEnergy { get; internal set; }

		/// <summary>
		/// Gets the static energy at distance gate 7.
		/// </summary>
		public byte Gate7StaticEnergy { get; internal set; }

		/// <summary>
		/// Gets the static energy at distance gate 8.
		/// </summary>
		public byte Gate8StaticEnergy { get; internal set; }

		/// <summary>
		/// Gets the additional data the radar might have appended to the report.
		/// </summary>
		public byte[] AdditionalData { get; private set; }

		/// <summary>
		/// Initializes a new instance of <see cref="EngineeringModeReportFrame"/>.
		/// </summary>
		public EngineeringModeReportFrame()
		{
			this.DataType = ReportingType.EngineeringMode;
		}
	}
}
