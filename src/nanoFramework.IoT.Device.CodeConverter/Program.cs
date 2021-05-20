using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace nanoFramework.IoT.Device.CodeConverter
{
    class Program
    {
        private const string _unitTestProjectGuidReplacementToken = "<!--UNIT TEST PROJECT GUID GOES HERE-->";
        private const string _sampleProjectGuidReplacementToken = "<!--SAMPLES PROJECT GUID GOES HERE-->";
        private const string _outputTypeReplacementToken = "<!-- OUTPUT TYPE -->";
        private const string _unitTestProjectPlaceholderToken = "<!-- UNIT TESTS PROJECT PLACEHOLDER -->";
        private const string _sampleProjectPlaceholderToken = "<!-- SAMPLES PROJECT PLACEHOLDER -->";


        static void Main(string[] args)
        {
            // Initialize configuration.
            // Set in appsettings.json or appsettings.Development.json.
            var configuration = InitOptions<ConverterConfiguration>();

            var outputDirectoryInfo = new DirectoryInfo(configuration.OutputDirectoryPath);
            if (outputDirectoryInfo.Exists)
            {
                outputDirectoryInfo.Delete(true);
            }

            var targetProjectTemplateDirectory = Directory.GetDirectories("../../../", configuration.TargetProjectTemplateName, new EnumerationOptions { RecurseSubdirectories = true })
                .Select(x => new DirectoryInfo(x))
                .FirstOrDefault();
            Console.WriteLine($"targetProjectTemplateDirectory={targetProjectTemplateDirectory}");

            var targetUnitTestProjectTemplateDirectory = Directory.GetDirectories("../../../", configuration.TargetUnitTestProjectTemplateName, new EnumerationOptions { RecurseSubdirectories = true })
                .Select(x => new DirectoryInfo(x))
                .FirstOrDefault();

            var sourceProjectFiles = Directory.GetFiles(configuration.SourceDirectory, "*.csproj", new EnumerationOptions { RecurseSubdirectories = true })
                .Where(x => configuration.FilePathFilters.Any(d => x.Contains(d)))
                .Select(x => new FileInfo(x));

            foreach (var sourceProjectFile in sourceProjectFiles)
            {
                // check if this a Unit Test project
                ProjectType projectType = ProjectType.Regular;

                if(sourceProjectFile.DirectoryName.EndsWith("tests") &&
                    Directory.GetFiles(sourceProjectFile.DirectoryName, "*.csproj").Length > 0)
                {
                    projectType = ProjectType.UnitTest;
                }
                else if(sourceProjectFile.DirectoryName.EndsWith("samples") &&
                    Directory.GetFiles(sourceProjectFile.DirectoryName, "*.csproj").Length > 0)
                {
                    projectType = ProjectType.Samples;
                }

                Console.WriteLine($"sourceProjectFile={sourceProjectFile}");
                var projectName = sourceProjectFile.Name.Replace(".csproj", string.Empty);
                var projectPath = sourceProjectFile.Directory.FullName.Replace(configuration.SourceDirectory, string.Empty);
                var targetDirectory = $"{configuration.OutputDirectoryPath}\\{projectPath}";
                DirectoryInfo targetDirectoryInfo;

                if (projectType == ProjectType.UnitTest)
                {
                    targetDirectoryInfo = targetUnitTestProjectTemplateDirectory.CopyDirectory(targetDirectory, new[] { ".user" });
                }
                else
                {
                    targetDirectoryInfo = targetProjectTemplateDirectory.CopyDirectory(targetDirectory, new[] { ".user" });
                }

                sourceProjectFile.Directory.CopyDirectory(targetDirectory);

                NugetPackages[] nfNugetPackages = NfNugetPackages.GetnfNugetPackages();

                var searches = nfNugetPackages.ToDictionary(x => x.Namespace, x => false);

                foreach (var file in targetDirectoryInfo.GetFiles("*.cs", new EnumerationOptions { RecurseSubdirectories = true }))
                {
                    searches = file.EditFile(
                        new Dictionary<string, string>
                        {
                            { "stackalloc", "new" },
                            { "Span<byte>", "SpanByte" },
                            { "ReadOnlySpan<byte>", "SpanByte" },
                            { ".AsSpan(start, length)", string.Empty },
                            { "Console.WriteLine(", "Debug.WriteLine(" },
                        },
                        nfNugetPackages,
                        searches);
                }

                // PROJECT FILE
                string[] oldProjectReferences;
                string projectGuid;
                CreateProjectFile(projectType, projectName, targetDirectoryInfo, nfNugetPackages, searches, out oldProjectReferences, out projectGuid);

                // PACKAGES
                CreatePackagesConfig(targetDirectoryInfo, nfNugetPackages, searches, oldProjectReferences);

                // SOLUTION File
                if (projectType == ProjectType.Regular)
                {
                    CreateSolutionFile(projectName, targetDirectoryInfo, projectGuid);
                    UpdateSolutionFile(projectName, targetDirectoryInfo, projectGuid);
                }
                else
                {
                    // fill in project GUID
                    UpdateProjectGuidInSolutionFile(
                        targetDirectoryInfo,
                        projectType,
                        projectGuid);
                }

                // NUSPEC File
                if (projectType == ProjectType.Regular)
                {
                    CreateNuspecFile(targetDirectoryInfo, projectName, targetDirectory);
                }
            }

            Console.WriteLine("Completed. Press any key to exit.");
            Console.ReadLine();
        }

        private static void CreateNuspecFile(DirectoryInfo targetDirectoryInfo, string projectName, string targetDirectory) {

            var targetNuspecFile = targetDirectoryInfo.GetFiles("template.nuspec").FirstOrDefault();
            if (targetNuspecFile != null)
            {
                targetNuspecFile.EditFile(new Dictionary<string, string>
                    {
                        { "[[Assembly]]", $"Iot.Device.{projectName}" },
                        { "[[ProjectName]]", projectName },
                    });
                targetNuspecFile.MoveTo($"{targetDirectory}\\{projectName}.nuspec", true);
            }
        }
        
        private static void CreateProjectFile(
            ProjectType projectType,
            string projectName,
            DirectoryInfo targetDirectoryInfo,
            NugetPackages[] nfNugetPackages,
            Dictionary<string, bool> searches,
            out string[] oldProjectReferences,
            out string projectGuid)
        {
            // Search for project references in old project file
            var oldProjectFile = targetDirectoryInfo.GetFiles("*.csproj").FirstOrDefault();

            string unitTestProjectReference = string.Empty;
            var oldProjectFileContents = File.ReadAllText(oldProjectFile.FullName);
            oldProjectReferences = nfNugetPackages.Where(x => oldProjectFileContents.Contains(x.Namespace)).Select(x => x.Namespace).ToArray();
            var oldFileReferences = Regex.Matches(oldProjectFileContents, "<*(?:Compile|None) Include*=[^>]*/>", RegexOptions.IgnoreCase);
            var packagesPath = Regex.Matches(oldProjectFileContents, "<*(?:Compile|None) Include*=[^>]*/>", RegexOptions.IgnoreCase);
            oldProjectFile.Delete();

            // Rename template project file
            var targetProjectFile = targetDirectoryInfo.GetFiles("*.nfproj").First();

            var projectReplacements = new Dictionary<string, string>();

            if (projectType == ProjectType.UnitTest)
            {
                targetProjectFile.MoveTo(targetProjectFile.FullName.Replace("UnitTestTemplateProject", projectName));

                // Update project name 
                projectReplacements.Add("UnitTestTemplateProject", projectName);

                // build proper project reference
                projectReplacements.Add(
                    "<!-- INSERT PROJECT UNDER TEST REFERENCE HERE -->",
                    $@"<ProjectReference Include=""..\{projectName.Replace(".Tests", "")}.nfproj"" />");
            }
            else
            {
                targetProjectFile.MoveTo(targetProjectFile.FullName.Replace("BindingTemplateProject", projectName));

                // Update project name 
                projectReplacements.Add("BindingTemplateProject", projectName);
            }

            // new GUID for project
            projectGuid = Guid.NewGuid().ToString("B").ToUpper();
            projectReplacements.Add("<!-- NEW PROJECT GUID -->", projectGuid);

            // Update project references
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
                var newProjectReferencesString = newProjectReferences.Distinct().Aggregate((seed, add) => $"{seed.Replace("$LF$", "\n")}\n    {add.Replace("$LF$", "\n")}");

                if (projectType == ProjectType.UnitTest ||
                    projectType == ProjectType.Samples)
                {
                    // add the reference to the project being tested
                    newProjectReferencesString += unitTestProjectReference;

                    // packages path are one level up, need to adjust
                    newProjectReferencesString = newProjectReferencesString.Replace("packages\\", "..\\packages\\");
                }

                projectReplacements.Add("<!-- INSERT NEW REFERENCES HERE -->", newProjectReferencesString);
            }

            if (oldFileReferences.Any())
            {
                var newFileReferencesString = oldFileReferences.Select(x => x.Value).Aggregate((seed, add) => $"{seed}\n    {add}");
                projectReplacements.Add("<!-- INSERT FILE REFERENCES HERE -->", newFileReferencesString);
            }

            // set output type
            if(projectType == ProjectType.Samples)
            {
                // samples projects are executables
                projectReplacements.Add(_outputTypeReplacementToken, "Exe");
            }
            else
            {
                // all the others are libraries
                projectReplacements.Add(_outputTypeReplacementToken, "Library");
            }

            targetProjectFile.EditFile(projectReplacements);

            if (projectType != ProjectType.UnitTest)
            {
                // process AssemblyInfo file
                var assemblyInfoFile = targetDirectoryInfo.GetFiles("Properties\\AssemblyInfo.cs").First();

                var assemblyInfoReplacements = new Dictionary<string, string>();

                assemblyInfoReplacements.Add("//< !--ASSEMBLY TITLE HERE-->", $@"[assembly: AssemblyTitle(""Iot.Device.{projectName}"")]");
                assemblyInfoReplacements.Add("//< !--ASSEMBLY COMPANY HERE-->", @"[assembly: AssemblyCompany(""nanoFramework Contributors"")]");
                assemblyInfoReplacements.Add("//< !--ASSEMBLY COPYRIGHT HERE-->", @"[assembly: AssemblyCopyright(""Copyright(c).NET Foundation and Contributors"")]");
                assemblyInfoReplacements.Add("//< !--ASSEMBLY COMVISIBLE HERE-->", @"[assembly: ComVisible(false)]");

                assemblyInfoFile.EditFile(assemblyInfoReplacements);
            }
        }

        private static void CreatePackagesConfig(
            DirectoryInfo targetDirectoryInfo,
            NugetPackages[] nfNugetPackages,
            Dictionary<string, bool> searches,
            string[] oldProjectReferences)
        {
            // Add nanoFramework nuget packages based on project references and references in the code
            var packagesFile = targetDirectoryInfo.GetFiles("packages.config").First();
            var packageReferences = nfNugetPackages
                .Where(x =>
                    // references from the old project file
                    oldProjectReferences.Any(p => p == x.Namespace) ||
                    // references in c# files
                    searches.Any(s => s.Value && s.Key == x.Namespace))
                .Distinct()
                .Select(x => x.PackageConfigReferenceString);

            if (packageReferences.Any())
            {
                var packageReferencesString = packageReferences.Distinct()
                    .Aggregate((seed, add) => $"{seed}\n{add}");
                packagesFile.EditFile(new Dictionary<string, string>
                        {
                            { "<!-- INSERT NEW PACKAGES HERE -->", packageReferencesString },
                        });
            }
        }

        private static void CreateSolutionFile(string projectName, DirectoryInfo targetDirectoryInfo, string projectGuid)
        {
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

            // find out if there are sample projects
            if(targetDirectoryInfo.GetDirectories("samples").Count() > 0)
            {
                solutionProject += $@"
Project(""{{11A8DD76-328B-46DF-9F39-F559912D0360}}"") = ""nanoFrameworkIoT.Samples"", ""samples\nanoFrameworkIoT.Samples.nfproj"", ""{_sampleProjectGuidReplacementToken}""
EndProject";
            }
            else
            {
                solutionProject += $@"
<!-- SAMPLES PROJECT PLACEHOLDER -->";
            }

            // find out if there are unit test projects
            if (targetDirectoryInfo.GetDirectories("tests").Count() > 0)
            {
                solutionProject += $@"
Project(""{{11A8DD76-328B-46DF-9F39-F559912D0360}}"") = ""nanoFrameworkIoT.Tests"", ""tests\nanoFrameworkIoT.Tests.nfproj"", ""{_unitTestProjectGuidReplacementToken}""
EndProject";
            }
            else
            {
                solutionProject += $@"
<!-- UNIT TESTS PROJECT PLACEHOLDER -->";
            }

            var solutionFileContent = solutionFileTemplate.Replace("[[ INSERT PROJECTS HERE ]]", solutionProject);
            solutionFileContent = solutionFileContent.Replace("[[ INSERT BUILD CONFIGURATIONS HERE ]]", solutionBuildConfigTemplate);
            File.WriteAllText($"{targetDirectoryInfo.FullName}\\{projectName}.sln", solutionFileContent);
        }

        private static void UpdateProjectGuidInSolutionFile(
            DirectoryInfo targetDirectoryInfo,
            ProjectType projectType,
            string projectGuid)
        {
            // find the parent solution file
            // it's OK to simplify because there will be only one SLN file there
            var slnFile = Directory.GetFiles(targetDirectoryInfo.Parent.FullName, "*.sln").FirstOrDefault();

            if (slnFile != null)
            {
                // load Solution file content
                string slnContent = File.ReadAllText(slnFile);

                // replace project GUID
                if (projectType == ProjectType.Samples)
                {
                    slnContent = slnContent.Replace(_sampleProjectGuidReplacementToken, projectGuid);
                }
                else if (projectType == ProjectType.UnitTest)
                {
                    slnContent = slnContent.Replace(_unitTestProjectGuidReplacementToken, projectGuid);
                }

                // add project, if not already there

                // find out if there are sample projects
                if (projectType == ProjectType.Samples)
                {
                    slnContent = slnContent.Replace(
                        _sampleProjectPlaceholderToken,
                        $@"Project(""{{11A8DD76-328B-46DF-9F39-F559912D0360}}"") = ""{targetDirectoryInfo.Parent.Name}.Samples"", ""samples\{targetDirectoryInfo.Parent.Name}.Samples.nfproj"", ""{projectGuid}""
EndProject");
                }
                else if (projectType == ProjectType.UnitTest)
                {
                    slnContent = slnContent.Replace(
                                  _unitTestProjectPlaceholderToken,
                                  $@"Project(""{{11A8DD76-328B-46DF-9F39-F559912D0360}}"") = ""{targetDirectoryInfo.Parent.Name}.Tests"", ""tests\{targetDirectoryInfo.Parent.Name}.Tests.nfproj"", ""{projectGuid}""
EndProject");
                }

                File.WriteAllText(slnFile, slnContent);
            }
        }

        private static void UpdateSolutionFile(string projectName, DirectoryInfo targetDirectoryInfo, string projectGuid)
        {
            var solutionFiles = targetDirectoryInfo.GetFiles("*.sln");
            foreach(var solutionFile in solutionFiles)
            {
                solutionFile.EditFile(new Dictionary<string, string>
                {
                    {"csproj", "nfproj" },
                    {"nanoFrameworkIoT", projectName }
                });
            }
        }

        private static T InitOptions<T>()
            where T : new()
        {
            var config = InitConfig();
            return config.Get<T>();
        }

        private static IConfigurationRoot InitConfig()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile($"appsettings.json", false, false)
                .AddJsonFile($"appsettings.Development.json", true, false)
                .AddEnvironmentVariables();

            return builder.Build();
        }
    }

    public class ConverterConfiguration
    {
        public string SourceDirectory { get; set; }
        public string FilePathFilters { get; set; }
        public string TargetProjectTemplateName { get; set; }
        public string TargetUnitTestProjectTemplateName { get; set; }
        public string OutputDirectoryPath { get; set; }
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
                        if (new[] { "bin", "obj" }.Any(toIgnore => path.StartsWith(toIgnore)) || file.Directory.GetFiles("*.csproj").Any())
                        {
                            continue;
                        }
                        path += "\\";
                    }
                    if (Directory.Exists($"{targetDirectory.FullName}\\{path}") == false)
                    {
                        targetDirectory.CreateSubdirectory(path);
                    }
                    file.CopyTo($"{targetDirectory.FullName}\\{path}{file.Name}", true);
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

                        // Make sure all line endings are CRLF.
                        line = line.Replace("\n", "\r\n");

                        output.WriteLine(line);
                    }
                }

                sourceFile.Delete();
                new FileInfo(tempFilename).MoveTo(sourceFile.FullName);
            }

            return checkIfFound;
        }
    }

    public enum ProjectType
    {
        None,

        Regular,

        Samples,

        UnitTest
    }
}
