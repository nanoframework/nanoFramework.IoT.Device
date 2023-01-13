using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace nanoFramework.IoT.Device.CodeConverter
{
    class Program
    {
        private const string _repoRoot = "../../../";
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

            var targetProjectTemplateDirectory = Directory
                .GetDirectories(_repoRoot, configuration.TargetProjectTemplateName, new EnumerationOptions { RecurseSubdirectories = true })
                .Select(x => new DirectoryInfo(x))
                .First();

            Console.WriteLine($"targetProjectTemplateDirectory={targetProjectTemplateDirectory}");

            var targetUnitTestProjectTemplateDirectory = Directory
                .GetDirectories(_repoRoot, configuration.TargetUnitTestProjectTemplateName, new EnumerationOptions { RecurseSubdirectories = true })
                .Select(x => new DirectoryInfo(x))
                .First();

            var genericsTemplatesDirectory = Directory
                .GetDirectories(_repoRoot, configuration.GenericsTemplatesFolderName, new EnumerationOptions { RecurseSubdirectories = true })
                .Select(x => new DirectoryInfo(x))
                .First();

            var projectsToFilterOut = configuration.ProjectConversionBlackList.Split(',').ToList();
            var convertedProjects = new DirectoryInfo(configuration.ConvertedProjectsPath).GetDirectories().Select(x => x.Name);
            projectsToFilterOut.AddRange(convertedProjects);

            var sourceProjectFiles = Directory
                .GetFiles(configuration.SourceDirectory, "*.csproj", new EnumerationOptions { RecurseSubdirectories = true })
                .Where(x => configuration.FilePathFilters.Any(x.Contains))
                .Select(x => new FileInfo(x))
                .Where(x => projectsToFilterOut.Any(f => x.FullName.Split('\\').Contains(f)) == false)
                .ToList();

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
                var projectPath = sourceProjectFile.Directory.FullName.Replace(configuration.SourceDirectory, string.Empty).Trim(Path.DirectorySeparatorChar);
                var targetDirectory = Path.Combine(configuration.OutputDirectoryPath, projectPath);
                DirectoryInfo targetDirectoryInfo;

                if (projectType == ProjectType.UnitTest)
                {
                    targetDirectoryInfo = targetUnitTestProjectTemplateDirectory.CopyDirectory(targetDirectory, new[] { ".user" });
                }
                else
                {
                    targetDirectoryInfo = targetProjectTemplateDirectory.CopyDirectory(targetDirectory, new[] { ".user" });

                    if (projectType == ProjectType.Samples)
                    {
                        // need to remove the nuspec template
                        File.Delete(Path.Combine(targetDirectory, "template.nuspec"));
                    }
                }

                sourceProjectFile.Directory.CopyDirectory(targetDirectory);

                NugetPackages[] nfNugetPackages = NfNugetPackages.GetnfNugetPackages();

                var searches = nfNugetPackages.ToDictionary(x => x.Namespace, x => false);
                SharedProjectImports.GetnfSharedProjectImports().ToList().ForEach(i =>
                {
                    if (string.IsNullOrEmpty(i.Namespace) == false)
                    {
                        searches.Add(i.Namespace, false);
                    }
                    if (string.IsNullOrEmpty(i.CodeMatchString) == false)
                    {
                        searches.Add(i.CodeMatchString, false);
                    }
                });

                foreach (var file in targetDirectoryInfo.GetFiles("*.cs", new EnumerationOptions { RecurseSubdirectories = true }))
                {
                    Dictionary<string, string> replacements = new () {
                        { "stackalloc", "new" },
                        { "ReadOnlySpan<byte>", "SpanByte" },
                        { "Span<byte>", "SpanByte" },
                        { "DateTime.Now", "DateTime.UtcNow" },
                        { ".AsSpan(start, length)", string.Empty },
                        { "Console.WriteLine(", "Debug.WriteLine(" },
                        { "Console.WriteLine()", "Debug.WriteLine(\"\")" },
                        { "Console.Write(", "Debug.Write(" },
                        { "using System.Diagnostics.CodeAnalysis;", "" },
                        { "\\[MemberNotNull.*\\]", "" },
                        { "Environment.TickCount", "DateTime.UtcNow.Ticks" },
                    };
                    
                    // All generics go to the main project of the binding to avoid conflicts between the projects.
                    // This will cause rare issues when a type is defined int the samples/tests project,
                    // but the generic container is placed to the main project from where it's not visible.
                    // There's also an issue if the main project isn't visible from samples/tests which want 
                    // to use this generic.
                    // If this proves problematic, one needs to refine the algorithm to choose the target
                    // directory for the generics individually and more precisely.
                    var genericsOutputDirectory = projectType is ProjectType.Regular
                        ? file.DirectoryName
                        : file.Directory!.Parent!.FullName;

                    var listReplacements = file.GenerateGenerics(
                        templatesFolder: genericsTemplatesDirectory.FullName, 
                        containerType: "List",
                        outputDirectory: genericsOutputDirectory,
                        containerTypeSynonyms: new [] {"IList", "IReadOnlyList"});
                    
                    var spanReplacements = file.GenerateGenerics(
                        templatesFolder: genericsTemplatesDirectory.FullName, 
                        containerType: "Span",
                        outputDirectory: genericsOutputDirectory,
                        containerTypeSynonyms: new [] {"ReadOnlySpan"},
                        alreadyExistingContainers: new [] {"SpanByte"});
                    
                    foreach (var (key, value) in listReplacements.Concat(spanReplacements))
                    {
                        replacements[key] = value;
                    }

                    searches = file.EditFile(
                        replacements,
                        nfNugetPackages,
                        searches);
                }
                

                // PROJECT FILE
                string[] oldProjectReferences;
                string projectGuid;
                CreateProjectFile(
                    projectType,
                    projectName,
                    targetDirectoryInfo,
                    nfNugetPackages,
                    searches,
                    out oldProjectReferences,
                    out projectGuid);

                // PACKAGES
                CreatePackagesConfig(
                    projectType,
                    targetDirectoryInfo,
                    nfNugetPackages,
                    searches,
                    oldProjectReferences);

                // NUSPEC File
                if (projectType == ProjectType.Regular)
                {
                    CreateNuspecFile(targetDirectoryInfo, projectName, targetDirectory);
                }
            }

            UpdateSolutionFiles(outputDirectoryInfo);

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
                targetNuspecFile.MoveTo(Path.Combine(targetDirectory, $"{projectName}.nuspec"), true);
            }
        }
        public static Guid ToHashGuid(string src)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(src);

            using var sha256 = System.Security.Cryptography.SHA256.Create();
            byte[] hashedBytes = sha256.ComputeHash(bytes);

            Array.Resize(ref hashedBytes, 16);
            return new Guid(hashedBytes);
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
            var oldProjectFile = targetDirectoryInfo.GetFiles("*.csproj").First();

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
                targetProjectFile.MoveTo(targetProjectFile.FullName.Replace("UnitTestTemplateProject", projectName), overwrite: true);

                // Update project name 
                projectReplacements.Add("UnitTestTemplateProject", projectName);

                // build proper project reference
                projectReplacements.Add(
                    "<!-- INSERT PROJECT UNDER TEST REFERENCE HERE -->",
                    $@"<ProjectReference Include=""..\{projectName.Replace(".Tests", "")}.nfproj"" />");
            }
            else
            {
                targetProjectFile.MoveTo(targetProjectFile.FullName.Replace("BindingTemplateProject", projectName), overwrite: true);

                // Update project name 
                projectReplacements.Add("BindingTemplateProject", projectName);
            }

            // new GUID for project
            projectGuid = ToHashGuid(projectName).ToString("B").ToUpper();
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
                var newProjectReferencesString = newProjectReferences.Distinct().Aggregate((seed, add) => $"{seed}\n    {add}");
                newProjectReferencesString = newProjectReferencesString.Replace("$LF$", "\n");

                if (projectType == ProjectType.UnitTest ||
                    projectType == ProjectType.Samples)
                {
                    // add the reference to the project being tested
                    newProjectReferencesString += unitTestProjectReference;

                    // packages path are one level up, need to adjust
                    newProjectReferencesString = newProjectReferencesString.Replace("packages\\", "..\\packages\\");

                    // need to also adjust mscorlib from template
                    projectReplacements.Add("<HintPath>packages\\nanoFramework.CoreLibrary", "<HintPath>..\\packages\\nanoFramework.CoreLibrary");
                }

                projectReplacements.Add("<!-- INSERT NEW REFERENCES HERE -->", newProjectReferencesString);
            }

            var newImports = SharedProjectImports.GetnfSharedProjectImports()
                    .Where(x => searches.Any(s => s.Value && (s.Key == x.Namespace || s.Key == x.CodeMatchString)))
                    .Select(x => x.NewProjectImport);
            if (newImports.Any())
            {
                var newImportsString = newImports.Distinct().Aggregate((seed, add) => $"{seed}\n    {add}");
                projectReplacements.Add("<!-- INSERT IMPORTS HERE -->", newImportsString);

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

            if(projectType == ProjectType.Regular)
            {
                projectReplacements.Add("<!-- INSERT NBGV IMPORT HERE -->",
                    @"<Import Project=""packages\Nerdbank.GitVersioning.3.5.103\build\Nerdbank.GitVersioning.targets"" Condition=""Exists('packages\Nerdbank.GitVersioning.3.5.103\build\Nerdbank.GitVersioning.targets')"" />
  <Target Name = ""EnsureNuGetPackageBuildImports"" BeforeTargets = ""PrepareForBuild"">
    <PropertyGroup>
      <ErrorText> This project references NuGet package(s) that are missing on this computer.Enable NuGet Package Restore to download them.For more information, see http://go.microsoft.com/fwlink/?LinkID=322105.The missing file is {0}.</ErrorText>
    </PropertyGroup>
    <Error Condition = ""!Exists('packages\Nerdbank.GitVersioning.3.5.103\build\Nerdbank.GitVersioning.targets')"" Text = ""$([System.String]::Format('$(ErrorText)', 'packages\Nerdbank.GitVersioning.3.5.103\build\Nerdbank.GitVersioning.targets'))"" />
  </Target>");

            }

            targetProjectFile.EditFile(projectReplacements);

            if (projectType != ProjectType.UnitTest)
            {
                // process AssemblyInfo file
                var assemblyInfoFile = targetDirectoryInfo.GetFiles(Path.Combine("Properties", "AssemblyInfo.cs")).First();

                var assemblyInfoReplacements = new Dictionary<string, string>();

                assemblyInfoReplacements.Add("//< !--ASSEMBLY TITLE HERE-->", $@"[assembly: AssemblyTitle(""Iot.Device.{projectName}"")]");
                assemblyInfoReplacements.Add("//< !--ASSEMBLY COMPANY HERE-->", @"[assembly: AssemblyCompany(""nanoFramework Contributors"")]");
                assemblyInfoReplacements.Add("//< !--ASSEMBLY COPYRIGHT HERE-->", @"[assembly: AssemblyCopyright(""Copyright(c).NET Foundation and Contributors"")]");
                assemblyInfoReplacements.Add("//< !--ASSEMBLY COMVISIBLE HERE-->", @"[assembly: ComVisible(false)]");

                assemblyInfoFile.EditFile(assemblyInfoReplacements);
            }

        }

        private static void CreatePackagesConfig(
            ProjectType projectType,
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

            if(projectType == ProjectType.Regular)
            {
                packageReferences = packageReferences.Append(@"  <package id=""Nerdbank.GitVersioning"" version=""3.5.103"" developmentDependency=""true"" targetFramework=""netnano1.0"" />");
            }

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

        static void UpdateSolutionFiles(DirectoryInfo outputDirectory)
        {
            foreach (var solutionFile in outputDirectory.GetFiles("*.sln", new EnumerationOptions { RecurseSubdirectories = true }))
            {
                solutionFile.EditFile(new Dictionary<string, string>
                {
                    { ".csproj", ".nfproj" },
                    { "FAE04EC0-301F-11D3-BF4B-00C04F79EFBC", "11A8DD76-328B-46DF-9F39-F559912D0360" },
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
        public string GenericsTemplatesFolderName { get; set; }
        public string OutputDirectoryPath { get; set; }
        public string ConvertedProjectsPath { get; set; }
        public string ProjectConversionBlackList { get; set; }
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
                    var path = file.FullName.Replace(sourceDirectory.FullName, string.Empty).Replace(file.Name, string.Empty).Trim(Path.DirectorySeparatorChar);
                    if (string.IsNullOrEmpty(path) == false)
                    {
                        if (new[] { "bin", "obj" }.Any(toIgnore => path.StartsWith(toIgnore)) || file.Directory.GetFiles("*.csproj").Any())
                        {
                            continue;
                        }
                        path += Path.DirectorySeparatorChar;
                    }
                    if (Directory.Exists(Path.Combine(targetDirectory.FullName, path)) == false)
                    {
                        targetDirectory.CreateSubdirectory(path);
                    }
                    file.CopyTo(Path.Combine(targetDirectory.FullName, path, file.Name), true);
                }
                return targetDirectory;
            }
            return null;
        }
    }
    
    public static class StringExtensions
    {
        public static string FirstCharToUpper(this string input) =>
            input switch
            {
                null => throw new ArgumentNullException(nameof(input)),
                "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
                _ => input.First().ToString().ToUpper() + input.Substring(1),
            };
    }


    public static class FileInfoExtensions
    {
        private static readonly string[] RegexHints = {".*", "^"};
        
        /// <summary>
        /// This is a complex operation that:
        /// 1. Finds all generics of a given type in the source file;
        /// 2. Generates non-generic classes that simulate behaviour of these generics (such as Span<byte> -> SpanByte);
        /// 3. Writes these new classes to the files in the given folder;
        /// 4. Returns the replacements one has to do in the source file so that it uses the new classes instead of generics.
        /// </summary>
        /// <param name="sourceFile">Source code file in which to search for generics usage</param>
        /// <param name="templatesFolder">Folder that contains all .cs templates for generics</param>
        /// <param name="containerType">
        /// Generic container such as List<int> or Span<byte>, for which we will search in the source file
        /// </param>
        /// <param name="containerTypeSynonyms">
        /// Synonymous generics that we also count as containerType usage.
        /// E.g. if you want to replace both IList<byte> and List<byte> with ListByte, make IList a synonym for List.
        /// </param>
        /// <param name="alreadyExistingContainers">
        /// Containers that don't need to be created from the template because they already exist in the visible scope.
        /// E.g. SpanByte exists in mscorlib, and there's no need to generate SpanByte.cs. 
        /// </param>
        /// <param name="outputDirectory">Where to put the generated replacements for the generics.</param>
        /// <returns>
        /// A dictionary of replacements one has to do in the file so that it uses the new classes instead of generics
        /// </returns>
        public static Dictionary<string, string> GenerateGenerics(
            this FileInfo sourceFile,
            string templatesFolder,
            string containerType,
            string outputDirectory,
            IList<string> containerTypeSynonyms = null,
            IList<string> alreadyExistingContainers = null
        )
        {
            if (!sourceFile.Exists)
            {
                return null;
            }

            containerTypeSynonyms ??= new List<string>();
            alreadyExistingContainers ??= new List<string>();

            containerTypeSynonyms = containerTypeSynonyms.Append(containerType).ToList();

            string fileContent = File.ReadAllText(sourceFile.FullName);

            // e.g. match List<byte> and IList<byte> but avoid matching List<T>, List<byte[]> or MyList<byte>
            var listRegex = new Regex(
                @"\b(" + 
                string.Join('|', containerTypeSynonyms.Select(Regex.Escape)) +
                @")<(\w{2,})>");  

            // Extract all used generic types, such as "byte" from "List<byte>" 
            var types = listRegex.Matches(fileContent).Select(match => match.Groups[2].Value).ToHashSet();

            Dictionary<string, string> result = new();
            foreach (string type in types)
            {
                var newType = GenerateGenericContainer(
                    templatePath: Path.Combine(templatesFolder, $"{containerType}.cs"),
                    containerType: containerType,
                    genericType: type,
                    alreadyExistingContainers: alreadyExistingContainers,
                    outputDirectory: outputDirectory
                );

                foreach (var oldType in containerTypeSynonyms)
                {
                    foreach (var prefix in new[] {" ", "\t", "(", "<"})
                    {
                        result[$"{prefix}{oldType}<{type}>"] = prefix + newType;
                    }

                    // replace in the very beginning of the line using regex
                    result["^" + Regex.Escape($"{oldType}<{type}>")] = newType; 
                }
            }
            return result;
        }

        private static string GenerateGenericContainer(
            string templatePath, 
            string containerType, 
            string genericType, 
            IEnumerable<string> alreadyExistingContainers,
            string outputDirectory)
        {
            var template = File.ReadAllText(templatePath);
            var newContainerType = $"{containerType}{genericType.FirstCharToUpper()}";

            if (alreadyExistingContainers.Contains(newContainerType))
            {
                // Don't create a container if a container with the same name is visible in the scope of this project.
                // E.g. a container SpanByte from mscorlib is globally visible, and we want to replace
                // Span<byte> and ReadOnlySpan<byte> with SpanByte, but we don't want to generate our own SpanByte.cs.
                return newContainerType;
            }

            template = template
                // Replace generic interfaces by their normal forms
                .Replace("IEnumerable<T>", "IEnumerable")
                .Replace("IEnumerator<T>", "IEnumerator")
                // constructor, always public containerType( as it's a function
                .Replace($"public {containerType}(", $"public {newContainerType}(")
                .Replace("<T>", genericType.FirstCharToUpper())
                // then arrays
                .Replace("T[]", $"{genericType}[]")
                // Then simple T but check few combinations
                .Replace(" T ", $" {genericType} ")
                .Replace(" T>", $" {genericType}>")
                .Replace("(T)", $"({genericType})")
                .Replace("(T ", $"({genericType} ")
                .Replace(" T)", $" {genericType})")
                .Replace(" T[", $" {genericType}[");

            File.WriteAllText(Path.Combine(outputDirectory, $"{newContainerType}.cs"), template);

            return newContainerType;
        }
        
        public static Dictionary<string, bool> EditFile(
            this FileInfo sourceFile,
            Dictionary<string, string> replacements,
            NugetPackages[] nugetPackages = null,
            Dictionary<string, bool> checkIfFound = null)
        {
            if (!sourceFile.Exists)
            {
                return checkIfFound;
            }
            
            var tempFilename = $"{sourceFile.FullName}.edited";
            using (var input = sourceFile.OpenText())
            using (var output = new StreamWriter(tempFilename))
            {
                string line;
                while (null != (line = input.ReadLine()))
                {
                    // Replacing longer matches first is a safeguard heuristic.
                    // It ensures we don't accidentally replace "List<int>"
                    // before "IList<int>", which would break "IList<int>" replacement.
                    foreach (var (key, value) in replacements.OrderByDescending(r => r.Key.Length))
                    {
                        if (line.Contains(key))
                        {
                            line = line.Replace(key, value);
                        }
                        try
                        {
                            if (RegexHints.Any(regexHint => key.Contains(regexHint)))
                            {
                                var regexMatch = Regex.Match(line, key).Value;
                                if (string.IsNullOrEmpty(regexMatch) == false)
                                {
                                    line = line.Replace(regexMatch, value);
                                }
                            }
                        }
                        catch (RegexParseException) { }
                    }

                    if (checkIfFound != null)
                    {
                        foreach (var check in checkIfFound.Keys)
                        {
                            if (line.Contains(check))
                            {
                                checkIfFound[check] = true;
                            }
                        }

                        if (nugetPackages is {Length: > 0})
                        {
                            foreach (var nugetPackage in nugetPackages)
                            {
                                if (nugetPackage.CodeMatchString != null && line.Contains(nugetPackage.CodeMatchString))
                                {
                                    checkIfFound[nugetPackage.Namespace] = true;
                                }
                            }
                        }
                    }

                    // Make sure all line endings on Windows are CRLF.
                    // This is important for opening .nfproj flies in Visual Studio,
                    // and maybe for some other files too.
                    line = line.Replace("\r", "").Replace("\n", Environment.NewLine);

                    output.WriteLine(line);
                }
            }

            sourceFile.Delete();
            new FileInfo(tempFilename).MoveTo(sourceFile.FullName);

            return checkIfFound;
        }
    }

    public enum ProjectType
    {
        Regular,

        Samples,

        UnitTest
    }
}
