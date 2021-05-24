# Using nanoFramework Code Converter

This converter is mainly design to help the migration from .NET IoT repository. That said, it can be used for any code transformation from .NET Core/.NET 5.0 to .NET nanoFramework.

The tools create a solution, transform the `csproj` file to a `nfproj` file, find and add automatically some of the nugets, including mscorlib, add reference to shared projects. It does as well generated static class file from a generic `List<Something>` to `ListSomething` so it can be used in nanoFramework as generics are not (yet) supported.

## Running the Code Converter

You can adjust the different paths into the `appsettings.json` file:

```json
{
  "SourceDirectory": "e:\\GitHub\\DotNet-iot\\src\\devices",
  "FilePathFilters": "src\\devices\\",
  "GenericsTemplatesFolderName": "GenericsTemplates",
  "TargetProjectTemplateName": "BindingTemplateProject",
  "TargetUnitTestProjectTemplateName": "UnitTestTemplateProject",
  "OutputDirectoryPath": "..\\..\\..\\..\\devices_generated"
}
```

- The `SourceDirectory` is where you have the project you want to transform and the `FilePathFilters` where you want the generation.
- `GenericsTemplatesFolderName` contains the generic templates you want to be analyzed and transformed.
- `TargetProjectTemplateName` contains the target project templates, for libraries and executable.
- `TargetUnitTestProjectTemplateName` contains the target project for tests.
- `OutputDirectoryPath` contains the output directory where you want the projects to be generated.

Then run the tool either from the solution either from the command line. Be aware that paths are relative to where the execution is happening.

## Customizing the generator

You can include different shared projects, those are declared into [`SharedProjectImports.cs`](./SharedProjectImports.cs). 

## Customizing the nuget packages

You can include different nuget packages, those are declared into [`NfNugetPackages.cs`](./NfNugetPackages.cs). 
