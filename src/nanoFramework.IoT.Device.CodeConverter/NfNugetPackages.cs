namespace nanoFramework.IoT.Device.CodeConverter
{
    public static class NfNugetPackages
    {
        public static NugetPackages[] GetnfNugetPackages()
        {
            var nfNugetPackages = new[]
                    {
                            /*
                            new NugetPackages {
                                OldProjectReferenceString= @"<ProjectReference Include=""$(MainLibraryPath)System.Device.Gpio.csproj"" />",
                                NewProjectReferenceString = @"<Reference Include=""System.Device.Gpio"">$LF$      <HintPath>packages\nanoFramework.System.Device.Gpio.1.0.0-preview.38\lib\System.Device.Gpio.dll </HintPath ></Reference > 
<Reference Include=""System.Device.Spi"">$LF$      <HintPath>packages\nanoFramework.System.Device.Spi.1.0.0-preview.30\lib\System.Device.Spi.dll</HintPath ></Reference > ",
                                PackageConfigReferenceString = @"  <package id=""nanoFramework.System.Device.Gpio"" version=""1.0.0-preview.38"" targetFramework=""netnanoframework10"" />
<package id=""nanoFramework.System.Device.Spi"" version=""1.0.0-preview.30"" targetFramework=""netnanoframework10"" />"
                            },
                            */
                            new NugetPackages {
                                Namespace="System.Device.Gpio",
                                OldProjectReferenceString= @"<ProjectReference Include=""$(MainLibraryPath)System.Device.Gpio.csproj"" />",
                                NewProjectReferenceString = @"<Reference Include=""System.Device.Gpio"">$LF$      <HintPath>packages\nanoFramework.System.Device.Gpio.1.0.0-preview.40\lib\System.Device.Gpio.dll</HintPath>$LF$      <Private>True</Private>$LF$    </Reference> ",
                                PackageConfigReferenceString = @"  <package id=""nanoFramework.System.Device.Gpio"" version=""1.0.0-preview.40"" targetFramework=""netnanoframework10"" />"
                            },
                            new NugetPackages {
                                Namespace="System.Device.Spi",
                                OldProjectReferenceString= @"<ProjectReference Include=""$(MainLibraryPath)System.Device.Spi.csproj"" />",
                                NewProjectReferenceString = @"<Reference Include=""System.Device.Spi"">$LF$      <HintPath>packages\nanoFramework.System.Device.Spi.1.0.0-preview.38\lib\System.Device.Spi.dll</HintPath>$LF$      <Private>True</Private>$LF$    </Reference> ",
                                PackageConfigReferenceString = @"  <package id=""nanoFramework.System.Device.Spi"" version=""1.0.0-preview.38"" targetFramework=""netnanoframework10"" />"
                            },
                            new NugetPackages {
                                Namespace="System.Device.I2c",
                                OldProjectReferenceString= @"<ProjectReference Include=""$(MainLibraryPath)System.Device.I2c.csproj"" />",
                                NewProjectReferenceString = @"<Reference Include=""System.Device.I2c"">$LF$      <HintPath>packages\nanoFramework.System.Device.I2c.1.0.1-preview.33\lib\System.Device.I2c.dll</HintPath>$LF$      <Private>True</Private>$LF$    </Reference>",
                                PackageConfigReferenceString = @"  <package id=""nanoFramework.System.Device.I2c"" version=""1.0.1-preview.33"" targetFramework=""netnanoframework10"" />"
                            },
                            new NugetPackages {
                                Namespace="RelativeHumidity",
                                OldProjectReferenceString= @"--NA--",
                                NewProjectReferenceString = @"<Reference Include=""UnitsNet.RelativeHumidity"">$LF$      <HintPath>packages\UnitsNet.nanoFramework.RelativeHumidity.4.91.0\lib\UnitsNet.RelativeHumidity.dll</HintPath>$LF$      <Private>True</Private>$LF$    </Reference>",
                                PackageConfigReferenceString = @"  <package id=""UnitsNet.nanoFramework.RelativeHumidity"" version=""4.91.0"" targetFramework=""netnanoframework10"" />"
                            },
                            new NugetPackages {
                                Namespace="Temperature",
                                OldProjectReferenceString= @"--NA--",
                                NewProjectReferenceString = @"<Reference Include=""UnitsNet.Temperature"">$LF$      <HintPath>packages\UnitsNet.nanoFramework.Temperature.4.91.0\lib\UnitsNet.Temperature.dll</HintPath>$LF$      <Private>True</Private>$LF$    </Reference>",
                                PackageConfigReferenceString = @"  <package id=""UnitsNet.nanoFramework.Temperature"" version=""4.91.0"" targetFramework=""netnanoframework10"" />"
                            },
                            new NugetPackages {
                                Namespace="ElectricPotential",
                                OldProjectReferenceString= @"--NA--",
                                NewProjectReferenceString = @"<Reference Include=""UnitsNet.ElectricPotential"">$LF$      <HintPath>packages\UnitsNet.nanoFramework.ElectricPotential.4.91.0\lib\UnitsNet.ElectricPotential.dll</HintPath>$LF$      <Private>True</Private>$LF$    </Reference>",
                                PackageConfigReferenceString = @"  <package id=""UnitsNet.nanoFramework.ElectricPotential"" version=""4.91.0"" targetFramework=""netnanoframework10"" />"
                            },
                            new NugetPackages {
                                Namespace="Pressure",
                                OldProjectReferenceString= @"--NA--",
                                NewProjectReferenceString = @"<Reference Include=""UnitsNet.Pressure"">$LF$      <HintPath>packages\UnitsNet.nanoFramework.Pressure.4.91.0\lib\UnitsNet.Pressure.dll</HintPath>$LF$      <Private>True</Private>$LF$    </Reference>",
                                PackageConfigReferenceString = @"  <package id=""UnitsNet.nanoFramework.Pressure"" version=""4.91.0"" targetFramework=""netnanoframework10"" />"
                            },
                            new NugetPackages {
                                Namespace="Length",
                                OldProjectReferenceString= @"--NA--",
                                NewProjectReferenceString = @"<Reference Include=""UnitsNet.Length"">$LF$      <HintPath>packages\UnitsNet.nanoFramework.Length.4.91.0\lib\UnitsNet.Length.dll</HintPath>$LF$      <Private>True</Private>$LF$    </Reference>",
                                PackageConfigReferenceString = @"  <package id=""UnitsNet.nanoFramework.Length"" version=""4.91.0"" targetFramework=""netnanoframework10"" />"
                            },
                            new NugetPackages {
                                Namespace="System.Math",
                                CodeMatchString="Math.",
                                OldProjectReferenceString= @"--NA--",
                                NewProjectReferenceString = @"<Reference Include=""System.Math"">$LF$      <HintPath>packages\nanoFramework.System.Math.1.0.0-preview.5\lib\System.Math.dll</HintPath>$LF$      <Private>True</Private>$LF$    </Reference>",
                                PackageConfigReferenceString = @"  <package id=""nanoFramework.System.Math"" version=""1.0.0-preview.5"" targetFramework=""netnanoframework10"" />"
                            },

                            /*
                            // Unit Tests
                            new NugetPackages {
                                Namespace="Xunit",
                                OldProjectReferenceString= @"<PackageReference Include=""xunit"" Version=""2.4.0"" />",
                                NewProjectReferenceString = @"<Reference Include=""nanoFramework.UnitTestLauncher"">$LF$      <HintPath>packages\nanoFramework.TestFramework.1.0.117\lib\nanoFramework.UnitTestLauncher.exe</HintPath>$LF$      <Private>True</Private>$LF$    </Reference>",
                                PackageConfigReferenceString = @"  <package id=""nanoFramework.TestFramework"" version=""1.0.117"" targetFramework=""netnanoframework10"" />"
                            },
                            new NugetPackages {
                                Namespace="Xunit.Abstractions",
                                OldProjectReferenceString= @"<PackageReference Include=""xunit"" Version=""2.4.0"" />",
                                NewProjectReferenceString = @"<Reference Include=""nanoFramework.UnitTestLauncher"">$LF$      <HintPath>packages\nanoFramework.TestFramework.1.0.117\lib\nanoFramework.UnitTestLauncher.exe</HintPath>$LF$      <Private>True</Private>$LF$    </Reference>",
                                PackageConfigReferenceString = @"  <package id=""nanoFramework.TestFramework"" version=""1.0.117"" targetFramework=""netnanoframework10"" />"
                            },
                            */

                        };
            return nfNugetPackages;
        }
    }
}
