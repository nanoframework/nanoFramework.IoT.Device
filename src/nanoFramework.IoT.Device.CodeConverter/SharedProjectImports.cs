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
                                NewProjectImport = @"<Import Project=""..\..\src\Stopwatch\Stopwatch.projitems"" Label=""Shared"" />"
                            },
                            new SharedProjectImport {
                                Namespace="System.Buffers.Binary",
                                CodeMatchString="BinaryPrimitives",
                                NewProjectImport = @"<Import Project=""..\..src\BinaryPrimitives\BinaryPrimitives.projitems"" Label=""Shared"" />"
                            },
                            new SharedProjectImport {
                                Namespace="System.Device.Model",
                                NewProjectImport = @"<Import Project=""..\..\src\System.Device.Model\System.Device.Model.projitems"" Label=""Shared"" />"
                            },
                            new SharedProjectImport {
                                Namespace="System.Numerics",
                                NewProjectImport = @"<Import Project=""..\..\src\System.Numerics\System.Numerics.projitems"" Label=""Shared"" />"
                            },
                            new SharedProjectImport {
                                Namespace="System.Runtime.CompilerService",
                                NewProjectImport = @"<Import Project=""..\..\src\System.Runtime.CompilerService\System.Runtime.CompilerService.projitems"" Label=""Shared"" />"
                            },
                            new SharedProjectImport {
                                Namespace="System.Drawing",
                                NewProjectImport = @"<Import Project=""..\..\src\System.Drawing\System.Drawing.projitems"" Label=""Shared"" />"
                            },
                            new SharedProjectImport {
                                Namespace="Iot.Device.Common",
                                NewProjectImport = @"<Import Project=""..\..\src\Iot.Device.Common\Iot.Device.Common.projitems"" Label=""Shared"" />"
                            }                            
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
