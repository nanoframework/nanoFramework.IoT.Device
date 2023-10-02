namespace Ld2410.Reporting
{
    public abstract class ReportFrame
    {
        public static byte[] Header = new byte[4] { 0xF4, 0xF3, 0xF2, 0xF1 };
        public static byte[] End = new byte[4] { 0xF8, 0xF7, 0xF6, 0xF5 };

        public ReportingType DataType { get; internal set; }
    }
}
