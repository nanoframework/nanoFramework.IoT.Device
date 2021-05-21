namespace nanoFramework.IoT.Device.CodeConverter
{
    public static class SharedProjectImports
    {
        public static SharedProjectImport[] GetnfSharedProjectImports()
        {
            return new[]
            {

                            new SharedProjectImport {
                                Namespace="System.Diagnostics",
                                CodeMatchString="Stopwatch",
                                NewProjectImport = @"<Import Project=""..\..\Stopwatch\Stopwatch.projitems"" Label=""Shared"" />"
                            },
                            new SharedProjectImport {
                                Namespace="System.Buffers.Binary",
                                CodeMatchString="BinaryPrimitives",
                                NewProjectImport = @"<Import Project=""..\..\BinaryPrimitives\BinaryPrimitives.projitems"" Label=""Shared"" />"
                            },
                            new SharedProjectImport {
                                Namespace="System.Device.Model",
                                NewProjectImport = @"<Import Project=""..\..\System.Device.Model\System.Device.Model.projitems"" Label=""Shared"" />"
                            },
                            new SharedProjectImport {
                                Namespace="System.Numerics",
                                NewProjectImport = @"<Import Project=""..\..\System.Numerics\System.Numerics.projitems"" Label=""Shared"" />"
                            },
            };
        }
    }
    public class SharedProjectImport
    {
        public string Namespace { get; set; }
        public string CodeMatchString { get; set; }
        public string NewProjectImport { get; set; }
    }
}
