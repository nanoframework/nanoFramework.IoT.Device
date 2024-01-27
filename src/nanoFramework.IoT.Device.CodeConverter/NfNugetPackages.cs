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
                                PackageConfigReferenceString = @"  <package id=""nanoFramework.System.Device.Gpio"" version=""1.0.0-preview.38"" targetFramework=""netnano1.0"" />
<package id=""nanoFramework.System.Device.Spi"" version=""1.0.0-preview.30"" targetFramework=""netnano1.0"" />"
                            },
                            */
                            new NugetPackages {
                                Namespace="System.Device.Gpio",
                                OldProjectReferenceString= @"<ProjectReference Include=""$(MainLibraryPath)System.Device.Gpio.csproj"" />",
                                NewProjectReferenceString = @"<Reference Include=""System.Device.Gpio"">$LF$      <HintPath>packages\nanoFramework.System.Device.Gpio.1.0.0-preview.40\lib\System.Device.Gpio.dll</HintPath>$LF$      <Private>True</Private>$LF$    </Reference> $LF$<Reference Include=""nanoFramework.Runtime.Events"">$LF$      <HintPath>packages\nanoFramework.Runtime.Events.1.9.0-preview.26\lib\nanoFramework.Runtime.Events.dll</HintPath>$LF$      <Private>True</Private>$LF$    </Reference> ",
                                PackageConfigReferenceString = @"  <package id=""nanoFramework.System.Device.Gpio"" version=""1.0.0-preview.40"" targetFramework=""netnano1.0"" />
  <package id=""nanoFramework.Runtime.Events"" version=""1.9.0-preview.26"" targetFramework=""netnano1.0"" />"
                            },
                            new NugetPackages {
                                Namespace="System.Device.Spi",
                                OldProjectReferenceString= @"<ProjectReference Include=""$(MainLibraryPath)System.Device.Spi.csproj"" />",
                                NewProjectReferenceString = @"<Reference Include=""System.Device.Spi"">$LF$      <HintPath>packages\nanoFramework.System.Device.Spi.1.0.0-preview.38\lib\System.Device.Spi.dll</HintPath>$LF$      <Private>True</Private>$LF$    </Reference> ",
                                PackageConfigReferenceString = @"  <package id=""nanoFramework.System.Device.Spi"" version=""1.0.0-preview.38"" targetFramework=""netnano1.0"" />"
                            },
                            new NugetPackages {
                                Namespace="System.Device.I2c",
                                OldProjectReferenceString= @"<ProjectReference Include=""$(MainLibraryPath)System.Device.I2c.csproj"" />",
                                NewProjectReferenceString = @"<Reference Include=""System.Device.I2c"">$LF$      <HintPath>packages\nanoFramework.System.Device.I2c.1.0.1-preview.33\lib\System.Device.I2c.dll</HintPath>$LF$      <Private>True</Private>$LF$    </Reference>",
                                PackageConfigReferenceString = @"  <package id=""nanoFramework.System.Device.I2c"" version=""1.0.1-preview.33"" targetFramework=""netnano1.0"" />"
                            },
                            new NugetPackages {
                                Namespace="RelativeHumidity",
                                OldProjectReferenceString= @"--NA--",
                                NewProjectReferenceString = @"<Reference Include=""UnitsNet.RelativeHumidity"">$LF$      <HintPath>packages\UnitsNet.nanoFramework.RelativeHumidity.4.92.0\lib\UnitsNet.RelativeHumidity.dll</HintPath>$LF$      <Private>True</Private>$LF$    </Reference>",
                                PackageConfigReferenceString = @"  <package id=""UnitsNet.nanoFramework.RelativeHumidity"" version=""4.92.0"" targetFramework=""netnano1.0"" />"
                            },
                            new NugetPackages {
                                Namespace="Temperature",
                                OldProjectReferenceString= @"--NA--",
                                NewProjectReferenceString = @"<Reference Include=""UnitsNet.Temperature"">$LF$      <HintPath>packages\UnitsNet.nanoFramework.Temperature.4.92.0\lib\UnitsNet.Temperature.dll</HintPath>$LF$      <Private>True</Private>$LF$    </Reference>",
                                PackageConfigReferenceString = @"  <package id=""UnitsNet.nanoFramework.Temperature"" version=""4.92.0"" targetFramework=""netnano1.0"" />"
                            },
                            new NugetPackages {
                                Namespace="ElectricPotential",
                                OldProjectReferenceString= @"--NA--",
                                NewProjectReferenceString = @"<Reference Include=""UnitsNet.ElectricPotential"">$LF$      <HintPath>packages\UnitsNet.nanoFramework.ElectricPotential.4.92.0\lib\UnitsNet.ElectricPotential.dll</HintPath>$LF$      <Private>True</Private>$LF$    </Reference>",
                                PackageConfigReferenceString = @"  <package id=""UnitsNet.nanoFramework.ElectricPotential"" version=""4.92.0"" targetFramework=""netnano1.0"" />"
                            },
                            new NugetPackages {
                                Namespace="Pressure",
                                OldProjectReferenceString= @"--NA--",
                                NewProjectReferenceString = @"<Reference Include=""UnitsNet.Pressure"">$LF$      <HintPath>packages\UnitsNet.nanoFramework.Pressure.4.92.0\lib\UnitsNet.Pressure.dll</HintPath>$LF$      <Private>True</Private>$LF$    </Reference>",
                                PackageConfigReferenceString = @"  <package id=""UnitsNet.nanoFramework.Pressure"" version=""4.92.0"" targetFramework=""netnano1.0"" />"
                            },
                            new NugetPackages {
                                Namespace="Length",
                                OldProjectReferenceString= @"--NA--",
                                NewProjectReferenceString = @"<Reference Include=""UnitsNet.Length"">$LF$      <HintPath>packages\UnitsNet.nanoFramework.Length.4.92.0\lib\UnitsNet.Length.dll</HintPath>$LF$      <Private>True</Private>$LF$    </Reference>",
                                PackageConfigReferenceString = @"  <package id=""UnitsNet.nanoFramework.Length"" version=""4.92.0"" targetFramework=""netnano1.0"" />"
                            },
                            new NugetPackages {
                                Namespace="Density",
                                OldProjectReferenceString= @"--NA--",
                                NewProjectReferenceString = @"<Reference Include=""UnitsNet.Density"">$LF$      <HintPath>packages\UnitsNet.nanoFramework.Density.4.92.0\lib\UnitsNet.Density.dll</HintPath>$LF$      <Private>True</Private>$LF$    </Reference>",
                                PackageConfigReferenceString = @"  <package id=""UnitsNet.nanoFramework.Density"" version=""4.92.0"" targetFramework=""netnano1.0"" />"
                            },
                            new NugetPackages {
                                Namespace="ElectricCurrent",
                                OldProjectReferenceString= @"--NA--",
                                NewProjectReferenceString = @"<Reference Include=""UnitsNet.ElectricCurrent"">$LF$      <HintPath>packages\UnitsNet.nanoFramework.ElectricCurrent.4.92.0\lib\UnitsNet.ElectricCurrent.dll</HintPath>$LF$      <Private>True</Private>$LF$    </Reference>",
                                PackageConfigReferenceString = @"  <package id=""UnitsNet.nanoFramework.ElectricCurrent"" version=""4.92.0"" targetFramework=""netnano1.0"" />"
                            },
                            new NugetPackages {
                                Namespace="Frequency",
                                OldProjectReferenceString= @"--NA--",
                                NewProjectReferenceString = @"<Reference Include=""UnitsNet.Frequency"">$LF$      <HintPath>packages\UnitsNet.nanoFramework.Frequency.4.92.0\lib\UnitsNet.Frequency.dll</HintPath>$LF$      <Private>True</Private>$LF$    </Reference>",
                                PackageConfigReferenceString = @"  <package id=""UnitsNet.nanoFramework.Frequency"" version=""4.92.0"" targetFramework=""netnano1.0"" />"
                            },
                            new NugetPackages {
                                Namespace="Illuminance",
                                OldProjectReferenceString= @"--NA--",
                                NewProjectReferenceString = @"<Reference Include=""UnitsNet.Illuminance"">$LF$      <HintPath>packages\UnitsNet.nanoFramework.Illuminance.4.92.0\lib\UnitsNet.Illuminance.dll</HintPath>$LF$      <Private>True</Private>$LF$    </Reference>",
                                PackageConfigReferenceString = @"  <package id=""UnitsNet.nanoFramework.Illuminance"" version=""4.92.0"" targetFramework=""netnano1.0"" />"
                            },
                            new NugetPackages {
                                Namespace="ElectricCharge",
                                OldProjectReferenceString= @"--NA--",
                                NewProjectReferenceString = @"<Reference Include=""UnitsNet.ElectricCurrent"">$LF$      <HintPath>packages\UnitsNet.nanoFramework.ElectricCurrent.4.92.0\lib\UnitsNet.ElectricCurrent.dll</HintPath>$LF$      <Private>True</Private>$LF$    </Reference>",
                                PackageConfigReferenceString = @"  <package id=""UnitsNet.nanoFramework.ElectricCurrent"" version=""4.92.0"" targetFramework=""netnano1.0"" />"
                            },
                            new NugetPackages {
                                Namespace="Ratio",
                                OldProjectReferenceString= @"--NA--",
                                NewProjectReferenceString = @"<Reference Include=""UnitsNet.Ratio"">$LF$      <HintPath>packages\UnitsNet.nanoFramework.Ratio.4.92.0\lib\UnitsNet.Ratio.dll</HintPath>$LF$      <Private>True</Private>$LF$    </Reference>",
                                PackageConfigReferenceString = @"  <package id=""UnitsNet.nanoFramework.Ratio"" version=""4.92.0"" targetFramework=""netnano1.0"" />"
                            },
                            new NugetPackages {
                                Namespace="Duration",
                                OldProjectReferenceString= @"--NA--",
                                NewProjectReferenceString = @"<Reference Include=""UnitsNet.Duration"">$LF$      <HintPath>packages\UnitsNet.nanoFramework.Duration.4.92.0\lib\UnitsNet.Duration.dll</HintPath>$LF$      <Private>True</Private>$LF$    </Reference>",
                                PackageConfigReferenceString = @"  <package id=""UnitsNet.nanoFramework.Duration"" version=""4.92.0"" targetFramework=""netnano1.0"" />"
                            },
                            new NugetPackages {
                                Namespace="VolumeConcentration",
                                OldProjectReferenceString= @"--NA--",
                                NewProjectReferenceString = @"<Reference Include=""UnitsNet.VolumeConcentration"">$LF$      <HintPath>packages\UnitsNet.nanoFramework.VolumeConcentration.4.92.0\lib\UnitsNet.VolumeConcentration.dll</HintPath>$LF$      <Private>True</Private>$LF$    </Reference>",
                                PackageConfigReferenceString = @"  <package id=""UnitsNet.nanoFramework.VolumeConcentration"" version=""4.92.0"" targetFramework=""netnano1.0"" />"
                            },
                            new NugetPackages {
                                Namespace="ElectricResistance",
                                OldProjectReferenceString= @"--NA--",
                                NewProjectReferenceString = @"<Reference Include=""UnitsNet.ElectricResistance"">$LF$      <HintPath>packages\UnitsNet.nanoFramework.ElectricResistance.4.92.0\lib\UnitsNet.ElectricResistance.dll</HintPath>$LF$      <Private>True</Private>$LF$    </Reference>",
                                PackageConfigReferenceString = @"  <package id=""UnitsNet.nanoFramework.ElectricResistance"" version=""4.92.0"" targetFramework=""netnano1.0"" />"
                            },
                            new NugetPackages {
                                Namespace="ElectricPotentialDc",
                                OldProjectReferenceString= @"--NA--",
                                NewProjectReferenceString = @"<Reference Include=""UnitsNet.ElectricPotentialDc"">$LF$      <HintPath>packages\UnitsNet.nanoFramework.ElectricPotentialDc.4.92.0\lib\UnitsNet.ElectricPotentialDc.dll</HintPath>$LF$      <Private>True</Private>$LF$    </Reference>",
                                PackageConfigReferenceString = @"  <package id=""UnitsNet.nanoFramework.ElectricPotentialDc"" version=""4.92.0"" targetFramework=""netnano1.0"" />"
                            },
                            new NugetPackages {
                                Namespace="System.Math",
                                CodeMatchString="Math.",
                                OldProjectReferenceString= @"--NA--",
                                NewProjectReferenceString = @"<Reference Include=""System.Math"">$LF$      <HintPath>packages\nanoFramework.System.Math.1.4.0-preview.7\lib\System.Math.dll</HintPath>$LF$      <Private>True</Private>$LF$    </Reference>",
                                PackageConfigReferenceString = @"  <package id=""nanoFramework.System.Math"" version=""1.4.0-preview.7"" targetFramework=""netnano1.0"" />"
                            },

                            /*
                            // Unit Tests
                            new NugetPackages {
                                Namespace="Xunit",
                                OldProjectReferenceString= @"<PackageReference Include=""xunit"" Version=""2.4.0"" />",
                                NewProjectReferenceString = @"<Reference Include=""nanoFramework.UnitTestLauncher"">$LF$      <HintPath>packages\nanoFramework.TestFramework.1.0.117\lib\nanoFramework.UnitTestLauncher.exe</HintPath>$LF$      <Private>True</Private>$LF$    </Reference>",
                                PackageConfigReferenceString = @"  <package id=""nanoFramework.TestFramework"" version=""1.0.117"" targetFramework=""netnano1.0"" />"
                            },
                            new NugetPackages {
                                Namespace="Xunit.Abstractions",
                                OldProjectReferenceString= @"<PackageReference Include=""xunit"" Version=""2.4.0"" />",
                                NewProjectReferenceString = @"<Reference Include=""nanoFramework.UnitTestLauncher"">$LF$      <HintPath>packages\nanoFramework.TestFramework.1.0.117\lib\nanoFramework.UnitTestLauncher.exe</HintPath>$LF$      <Private>True</Private>$LF$    </Reference>",
                                PackageConfigReferenceString = @"  <package id=""nanoFramework.TestFramework"" version=""1.0.117"" targetFramework=""netnano1.0"" />"
                            },
                            */

                        };
            return nfNugetPackages;
        }
    }
}
