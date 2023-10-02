using UnitsNet;

namespace Ld2410.Reporting
{
    public class BasicReportFrame : ReportFrame
    {
        public TargetState TargetState { get; internal set; }

        public Length MovementTargetDistance { get; internal set; }

        public byte MovementTargetEnergy { get; internal set; }

        public Length StationaryTargetDistance { get; internal set; }

        public byte StationaryTargetEnergy { get; internal set; }

        public Length DetectionDistance { get; internal set; }

        internal BasicReportFrame()
        {
            this.DataType = ReportingType.BasicMode;
        }
    }
}
