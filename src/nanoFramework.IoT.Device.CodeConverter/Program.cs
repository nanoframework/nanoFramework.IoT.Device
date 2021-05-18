using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace nanoFramework.IoT.Device.CodeConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            var sourceDirectory = @"D:\Temp\src\devices";
            var filePathFilters = new[] { @"\src\devices\" };
            var targetProjectTemplateName = "BindingTemplateProject";
            var outputDirectoryPath = @"..\..\..\..\devices_generated";

            var outputDirectoryInfo = new DirectoryInfo(outputDirectoryPath);
            if (outputDirectoryInfo.Exists)
            {
                outputDirectoryInfo.Delete(true);
            }

            var targetProjectTemplateDirectory = Directory.GetDirectories("../../../", targetProjectTemplateName, new EnumerationOptions { RecurseSubdirectories = true })
                .Select(x => new DirectoryInfo(x))
                .FirstOrDefault();
            Console.WriteLine($"targetProjectTemplateDirectory={targetProjectTemplateDirectory}");

            var sourceProjectFiles = Directory.GetFiles(sourceDirectory, "*.csproj", new EnumerationOptions { RecurseSubdirectories = true })
                .Where(x => filePathFilters.Any(d => x.Contains(d)))
                .Select(x => new FileInfo(x));
            foreach (var sourceProjectFile in sourceProjectFiles)
            {
                Console.WriteLine($"sourceProjectFile={sourceProjectFile}");
                var projectName = sourceProjectFile.Name.Replace(".csproj", string.Empty);
                var targetDirectory = $"{outputDirectoryPath}\\{projectName}";
                var targetDirectoryInfo = targetProjectTemplateDirectory.CopyDirectory(targetDirectory, new[] { ".user" });
                sourceProjectFile.Directory.CopyDirectory(targetDirectory);

                var nfNugetPackages = new[]
                        {
//                            new NugetPackages {
//                                OldProjectReferenceString= @"<ProjectReference Include=""$(MainLibraryPath)System.Device.Gpio.csproj"" />",
//                                NewProjectReferenceString = @"<Reference Include=""System.Device.Gpio""><HintPath>packages\nanoFramework.System.Device.Gpio.1.0.0-preview.38\lib\System.Device.Gpio.dll </HintPath ></Reference > 
//<Reference Include=""System.Device.Spi""><HintPath>packages\nanoFramework.System.Device.Spi.1.0.0-preview.30\lib\System.Device.Spi.dll</HintPath ></Reference > ",
//                                PackageConfigReferenceString = @"<package id=""nanoFramework.System.Device.Gpio"" version=""1.0.0-preview.38"" targetFramework=""netnanoframework10"" />
//<package id=""nanoFramework.System.Device.Spi"" version=""1.0.0-preview.30"" targetFramework=""netnanoframework10"" />"
//                            },
                            new NugetPackages {
                                Namespace="System.Device.Gpio",
                                OldProjectReferenceString= @"<ProjectReference Include=""$(MainLibraryPath)System.Device.Gpio.csproj"" />",
                                NewProjectReferenceString = @"<Reference Include=""packages\nanoFramework.System.Device.Gpio.1.0.0-preview.31\lib\System.Device.Gpio.dll""></Reference > ",
                                PackageConfigReferenceString = @"<package id=""nanoFramework.System.Device.Gpio"" version=""1.0.0-preview.31"" targetFramework=""netnanoframework10"" />"
                            },
                            new NugetPackages {
                                Namespace="System.Device.Spi",
                                OldProjectReferenceString= @"<ProjectReference Include=""$(MainLibraryPath)System.Device.Spi.csproj"" />",
                                NewProjectReferenceString = @"<Reference Include=""packages\nanoFramework.System.Device.Spi.1.0.0-preview.28\lib\System.Device.Spi.dll""></Reference > ",
                                PackageConfigReferenceString = @"<package id=""nanoFramework.System.Device.Spi"" version=""1.0.0-preview.28"" targetFramework=""netnanoframework10"" />"
                            },
                            new NugetPackages {
                                Namespace="System.Device.I2c",
                                OldProjectReferenceString= @"<ProjectReference Include=""$(MainLibraryPath)System.Device.I2c.csproj"" />",
                                NewProjectReferenceString = @"<Reference Include=""packages\nanoFramework.System.Device.I2c.1.0.1-preview.31\lib\System.Device.I2c.dll""></Reference>",
                                PackageConfigReferenceString = @"<package id=""nanoFramework.System.Device.I2c"" version=""1.0.1-preview.31"" targetFramework=""netnanoframework10"" />"
                            },
                            new NugetPackages {
                                Namespace="RelativeHumidity",
                                OldProjectReferenceString= @"--NA--",
                                NewProjectReferenceString = @"<Reference Include=""packages\UnitsNet.nanoFramework.RelativeHumidity.4.91.0\lib\UnitsNet.RelativeHumidity.dll""></Reference>",
                                PackageConfigReferenceString = @"<package id=""UnitsNet.nanoFramework.RelativeHumidity"" version=""4.91.0"" targetFramework=""netnanoframework10"" />"
                            },
                            new NugetPackages {
                                Namespace="Temperature",
                                OldProjectReferenceString= @"--NA--",
                                NewProjectReferenceString = @"<Reference Include=""packages\UnitsNet.nanoFramework.Temperature.4.91.0\lib\UnitsNet.Temperature.dll""></Reference>",
                                PackageConfigReferenceString = @"<package id=""UnitsNet.nanoFramework.Temperature"" version=""4.91.0"" targetFramework=""netnanoframework10"" />"
                            },
                            new NugetPackages {
                                Namespace="ElectricPotential",
                                OldProjectReferenceString= @"--NA--",
                                NewProjectReferenceString = @"<Reference Include=""packages\UnitsNet.nanoFramework.ElectricPotential.4.91.0\lib\UnitsNet.ElectricPotential.dll""></Reference>",
                                PackageConfigReferenceString = @"<package id=""UnitsNet.nanoFramework.ElectricPotential"" version=""4.91.0"" targetFramework=""netnanoframework10"" />"
                            },
                            new NugetPackages {
                                Namespace="Pressure",
                                OldProjectReferenceString= @"--NA--",
                                NewProjectReferenceString = @"<Reference Include=""packages\UnitsNet.nanoFramework.Pressure.4.91.0\lib\UnitsNet.Pressure.dll""></Reference>",
                                PackageConfigReferenceString = @"<package id=""UnitsNet.nanoFramework.Pressure"" version=""4.91.0"" targetFramework=""netnanoframework10"" />"
                            },
                            new NugetPackages {
                                Namespace="Length",
                                OldProjectReferenceString= @"--NA--",
                                NewProjectReferenceString = @"<Reference Include=""packages\UnitsNet.nanoFramework.Length.4.91.0\lib\UnitsNet.Length.dll""></Reference>",
                                PackageConfigReferenceString = @"<package id=""UnitsNet.nanoFramework.Length"" version=""4.91.0"" targetFramework=""netnanoframework10"" />"
                            },
                            new NugetPackages {
                                Namespace="System.Math",
                                CodeMatchString="Math.",
                                OldProjectReferenceString= @"--NA--",
                                NewProjectReferenceString = @"<Reference Include=""System.Math, Version=1.4.0.0, Culture=neutral, PublicKeyToken=c07d481e9758c731""></Reference>",
                                PackageConfigReferenceString = @"<package id=""nanoFramework.System.Math"" version=""1.4.0-preview.1"" targetFramework=""netnanoframework10"" />"
                            },

                            // Unit Tests
                            new NugetPackages {
                                Namespace="Xunit",
                                OldProjectReferenceString= @"<PackageReference Include=""xunit"" Version=""2.4.0"" />",
                                NewProjectReferenceString = @"<Reference Include=""packages\nanoFramework.TestFramework.1.0.114\lib\nanoFramework.UnitTestLauncher.exe""></Reference>",
                                PackageConfigReferenceString = @"<package id=""nanoFramework.TestFramework"" version=""1.0.114"" targetFramework=""netnanoframework10"" />"
                            },
                            new NugetPackages {
                                Namespace="Xunit.Abstractions",
                                OldProjectReferenceString= @"<PackageReference Include=""xunit"" Version=""2.4.0"" />",
                                NewProjectReferenceString = @"<Reference Include=""packages\nanoFramework.TestFramework.1.0.114\lib\nanoFramework.UnitTestLauncher.exe""></Reference>",
                                PackageConfigReferenceString = @"<package id=""nanoFramework.TestFramework"" version=""1.0.114"" targetFramework=""netnanoframework10"" />"
                            },

                        };

                var searches = nfNugetPackages.ToDictionary(x => x.Namespace, x => false);

                foreach (var file in targetDirectoryInfo.GetFiles("*.cs",new EnumerationOptions { RecurseSubdirectories = true }))
                {
                    searches = file.EditFile(
                        new Dictionary<string, string>
                        {
                            { "stackalloc", "new" },
                            { "Span<byte>", "SpanByte" },
                            { ".AsSpan(start, length)", string.Empty },
                        }, 
                        nfNugetPackages, 
                        searches);
                }

                // PROJECT FILE
                // Search for project references in old project file
                var oldProjectFile = targetDirectoryInfo.GetFiles("*.csproj").FirstOrDefault();

                // check if this a Unit Test project
                var isUnitTestProject = oldProjectFile.DirectoryName.EndsWith(".Tests");

                var oldProjectFileContents = File.ReadAllText(oldProjectFile.FullName);
                var oldProjectReferences = nfNugetPackages.Where(x => oldProjectFileContents.Contains(x.Namespace)).Select(x => x.Namespace).ToArray();
                var oldFileReferences = Regex.Matches(oldProjectFileContents, "<*(?:Compile|None) Include*=[^>]*/>", RegexOptions.IgnoreCase);
                oldProjectFile.Delete();

                // Rename template project file
                var targetProjectFile = targetDirectoryInfo.GetFiles("*.nfproj").First();
                targetProjectFile.MoveTo(targetProjectFile.FullName.Replace("BindingTemplateProject", projectName));

                // Update project name and references in new project file
                var projectReplacements = new Dictionary<string, string> {
                    {"BindingTemplateProject", projectName }
                };

                // new GUID for project
                var projectGuid = Guid.NewGuid().ToString("B").ToUpper();

                projectReplacements.Add("<!-- NEW PROJECT GUID -->", projectGuid);

                var newProjectReferences = new List<string>();
                if (oldProjectReferences.Any())
                {
                    newProjectReferences.AddRange(oldProjectReferences.Select(x => nfNugetPackages.FirstOrDefault(r => r.Namespace == x).NewProjectReferenceString));
                }

                newProjectReferences.AddRange(nfNugetPackages
                        .Where(x => searches.Any(s => s.Value && s.Key == x.Namespace))
                        .Select(x => x.NewProjectReferenceString));

                if (newProjectReferences.Any())
                {
                    var newProjectReferencesString = newProjectReferences.Aggregate((seed, add) => $"{seed}\n{add}");
                    projectReplacements.Add("<!-- INSERT NEW REFERENCES HERE -->", newProjectReferencesString);
                }
                if (oldFileReferences.Any())
                {
                    var newFileReferencesString = oldFileReferences.Select(x => x.Value).Aggregate((seed, add) => $"{seed}\n{add}");
                    projectReplacements.Add("<!-- INSERT FILE REFERENCES HERE -->", newFileReferencesString);
                }
                targetProjectFile.EditFile(projectReplacements);

                // PACKAGES
                // Add nanoFramework nuget packages based on project references and references in the code
                var packagesFile = targetDirectoryInfo.GetFiles("packages.config").First();
                var packageReferences = nfNugetPackages
                    .Where(x =>
                        // references from the old project file
                        oldProjectReferences.Any(p => p == x.Namespace) ||
                        // references in c# files
                        searches.Any(s => s.Value && s.Key == x.Namespace))
                    .Select(x => x.PackageConfigReferenceString);

                if (packageReferences.Any())
                {
                    var packageReferencesString = packageReferences
                        .Aggregate((seed, add) => $"{seed}\n{add}");
                    packagesFile.EditFile(new Dictionary<string, string>
                        {
                            { "<!-- INSERT NEW PACKAGES HERE -->", packageReferencesString },
                        });
                }


                var solutionFileTemplate = @"
Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio Version 16
VisualStudioVersion = 16.0.30413.136
MinimumVisualStudioVersion = 10.0.40219.1
[[ INSERT PROJECTS HERE ]]
Global
	GlobalSection(SolutionConfigurationPlatforms) = preSolution
		Debug|Any CPU = Debug|Any CPU
		Release|Any CPU = Release|Any CPU
	EndGlobalSection
	GlobalSection(ProjectConfigurationPlatforms) = postSolution
		[[ INSERT BUILD CONFIGURATIONS HERE ]]
	EndGlobalSection
EndGlobal";
                var solutionProjectTemplate = $@"Project(""{{11A8DD76-328B-46DF-9F39-F559912D0360}}"") = ""nanoFrameworkIoT"", ""nanoFrameworkIoT.nfproj"", ""{projectGuid}""
EndProject";
                var solutionBuildConfigTemplate = $@"{projectGuid}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{projectGuid}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{projectGuid}.Debug|Any CPU.Deploy.0 = Debug|Any CPU
		{projectGuid}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{projectGuid}.Release|Any CPU.Build.0 = Release|Any CPU
		{projectGuid}.Release|Any CPU.Deploy.0 = Release|Any CPU";

                var solutionProject = solutionProjectTemplate.Replace("nanoFrameworkIoT", projectName);
                var solutionFileContent = solutionFileTemplate.Replace("[[ INSERT PROJECTS HERE ]]", solutionProject);
                solutionFileContent = solutionFileContent.Replace("[[ INSERT BUILD CONFIGURATIONS HERE ]]", solutionBuildConfigTemplate);
                File.WriteAllText($"{targetDirectoryInfo.FullName}\\{projectName}.sln", solutionFileContent);

            }

            Console.WriteLine("Completed. Press any key to exit.");
            Console.ReadLine();
        }

    }

    public class NugetPackages
    {
        public string OldProjectReferenceString { get; set; }
        public string CodeMatchString { get; set; }
        public string NewProjectReferenceString { get; set; }
        public string PackageConfigReferenceString { get; set; }
        public string Namespace { get; internal set; }
    }

    public static class DirectoryInfoExtensions
    {
        public static DirectoryInfo CopyDirectory(this DirectoryInfo sourceDirectory, string targetPath, string[] filePathFilters = null)
        {
            if (sourceDirectory.Exists)
            {
                var targetDirectory = Directory.CreateDirectory(targetPath);
                foreach (var file in sourceDirectory.GetFiles("*", new EnumerationOptions { RecurseSubdirectories = true }).Where(f => filePathFilters == null || filePathFilters.Any(filter => f.FullName.Contains(filter)) == false))
                {
                    var path = file.FullName.Replace(sourceDirectory.FullName, string.Empty).Replace(file.Name, string.Empty).Trim('\\');
                    if (string.IsNullOrEmpty(path) == false)
                    {
                        if (new[] { "bin", "obj" }.Contains(path) || file.Directory.GetFiles("*.csproj").Any())
                        {
                            continue;
                        }
                        path += "\\";
                    }
                    if (Directory.Exists($"{targetDirectory.FullName}\\{path}") == false)
                    {
                        targetDirectory.CreateSubdirectory(path);
                    }
                    file.CopyTo($"{targetDirectory.FullName}\\{path}{file.Name}");
                }
                return targetDirectory;
            }
            return null;
        }
    }

    public static class FileInfoExtensions
    {
        public static Dictionary<string, bool> EditFile(
            this FileInfo sourceFile, 
            Dictionary<string, string> replacements, 
            NugetPackages[] nugetPackages = null,
            Dictionary<string, bool> checkIfFound = null)
        {
            var replacedKeys = new List<string>();
            if (sourceFile.Exists)
            {
                var tempFilename = $"{sourceFile.FullName}.edited";
                using (var input = sourceFile.OpenText())
                using (var output = new StreamWriter(tempFilename))
                {
                    string line;
                    while (null != (line = input.ReadLine()))
                    {
                        foreach (var replacement in replacements)
                        {
                            if (line.Contains(replacement.Key))
                            {
                                line = line.Replace(replacement.Key, replacement.Value);
                                replacedKeys.Add(replacement.Key);
                            }
                        }

                        if (checkIfFound != null)
                        {
                            foreach (var check in checkIfFound)
                            {
                                if (line.Contains(check.Key))
                                {
                                    checkIfFound[check.Key] = true;
                                }
                            }
                        }

                        if (nugetPackages != null && nugetPackages.Length > 0)
                        {
                            foreach (var nugetPackage in nugetPackages)
                            {
                                if (nugetPackage.CodeMatchString != null && line.Contains(nugetPackage.CodeMatchString))
                                {
                                    checkIfFound[nugetPackage.Namespace] = true;
                                }
                            }
                        }

                        output.WriteLine(line);
                    }
                }

                sourceFile.Delete();
                new FileInfo(tempFilename).MoveTo(sourceFile.FullName);
            }

            return checkIfFound;
        }
    }
}
