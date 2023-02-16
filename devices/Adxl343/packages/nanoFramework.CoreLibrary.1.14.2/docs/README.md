[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=nanoframework_lib-CoreLibrary&metric=alert_status)](https://sonarcloud.io/dashboard?id=nanoframework_lib-CoreLibrary) [![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=nanoframework_lib-CoreLibrary&metric=reliability_rating)](https://sonarcloud.io/dashboard?id=nanoframework_lib-CoreLibrary) [![License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE) [![NuGet](https://img.shields.io/nuget/dt/nanoFramework.CoreLibrary.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.CoreLibrary/) [![#yourfirstpr](https://img.shields.io/badge/first--timers--only-friendly-blue.svg)](https://github.com/nanoframework/Home/blob/main/CONTRIBUTING.md) 
[![Discord](https://img.shields.io/discord/478725473862549535.svg?logo=discord&logoColor=white&label=Discord&color=7289DA)](https://discord.gg/gCyBu8T)

![nanoFramework logo](https://raw.githubusercontent.com/nanoframework/Home/main/resources/logo/nanoFramework-repo-logo.png)

-----

# Welcome to the .NET **nanoFramework** Base Class Library repository

## Build status

| Component | Build Status | NuGet Package |
|:-|---|---|
| Base Class Library | [![Build Status](https://dev.azure.com/nanoframework/CoreLibrary/_apis/build/status/nanoframework.CoreLibrary?repoName=nanoframework%2FCoreLibrary&branchName=main)](https://dev.azure.com/nanoframework/CoreLibrary/_build/latest?definitionId=24&repoName=nanoframework%2FCoreLibrary&branchName=main) | [![NuGet](https://img.shields.io/nuget/v/nanoFramework.CoreLibrary.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.CoreLibrary/)  |
| Base Class Library w/o reflection | [![Build Status](https://dev.azure.com/nanoframework/CoreLibrary/_apis/build/status/nanoframework.CoreLibrary?repoName=nanoframework%2FCoreLibrary&branchName=main)](https://dev.azure.com/nanoframework/CoreLibrary/_build/latest?definitionId=24&repoName=nanoframework%2FCoreLibrary&branchName=main) | [![NuGet](https://img.shields.io/nuget/v/nanoFramework.CoreLibrary.NoReflection.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.CoreLibrary.NoReflection/) |

## BCL Flavours

The .NET **nanoFramework** Base Class Library is provided in two flavours: with or without support for System.Reflection namespace. The reason for this is that the reflection API adds up a significant size to the DLL and image size. For targets with smaller flash this can be prohibitive.

## Unit Test

nanoFramework has a dedicated [Unit Test framework](https://github.com/nanoframework/nanoFramework.TestFramework). This repository has Unit Test and you will find all of them under the `Tests` folder. The main solution embed all all the tests as well. You can run them directly from Visual Studio and create new tests. For more information on the [Unit Test Framework](https://docs.nanoframework.net/content/unit-test/index.html).

CoreLibrary has specific needs that differ from what you'll find in the documentation:

- You need to have the nanoFramework.TestFramework as a NuGet package as it will bring the nanoCLR Win32 emulator
- You need to remove the reference to mscorlib, nanoFramework.TestFramework and nanoFramework.UnitTestLauncher
- Use project reference instead for all those 3 elements

You can then run the test either on a real device, either in the emulator as described in the documentation. You may have to manually flash your device for the mscorlib version to match the one you are building on.

**Important**: Any new code checked in this repository will have to:

- have a proper test covering for all the methods, properties, events and the possible exceptions,
- do not break more of the the existing tests meaning, in other words, it should not create more issues than already existing.

### Test structure and project reference

All the projects are referenced based and to be able to run the tests in the pipeline, in command line and in Visual Studio, it does require a specific structure:

- The `NFUnitTestAdapter` project must be present and untouched. It does contains the core elements needed to have the nanoCLR Win32 application present.
- You need to have a `nano.runsettings` file in each sub directory you want to run the tests on from Visual Studio
- If you want to run the tests in command line you have to use the `Developer Command Prompt for VS 2019` then you can use from the home cloned lib-CoreLibrary directory a command line like this one:

```cmd
vstest.console.exe .\Tests\NFUnitTestBitConverter\bin\Release\NFUnitTest.dll  /Settings:.\Tests\NFUnitTestAdapater\nano.runsettings /TestAdapterPath:.\nanoFramework.TestFramework\source\TestAdapter\bin\Debug\net4.8 /Diag:.\log.txt /Logger:trx
```

*Notes*:

- You have to build the TestAdapter from the source in this case. You can use the path to the NuGet as well, this will have the same effect.
- you have full diagnostic enabled in this case.

## Feedback and documentation

For documentation, providing feedback, issues and finding out how to contribute please refer to the [Home repo](https://github.com/nanoframework/Home).

Join our Discord community [here](https://discord.gg/gCyBu8T).

## Credits

The list of contributors to this project can be found at [CONTRIBUTORS](https://github.com/nanoframework/Home/blob/main/CONTRIBUTORS.md).

## License

The **nanoFramework** Class Libraries are licensed under the [MIT license](LICENSE.md).

## Code of Conduct

This project has adopted the code of conduct defined by the Contributor Covenant to clarify expected behaviour in our community.
For more information see the [.NET Foundation Code of Conduct](https://dotnetfoundation.org/code-of-conduct).

### .NET Foundation

This project is supported by the [.NET Foundation](https://dotnetfoundation.org).
