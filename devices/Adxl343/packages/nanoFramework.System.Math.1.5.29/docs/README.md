[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=nanoframework_lib-nanoFramework.System.Math&metric=alert_status)](https://sonarcloud.io/dashboard?id=nanoframework_lib-nanoFramework.System.Math) [![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=nanoframework_lib-nanoFramework.System.Math&metric=reliability_rating)](https://sonarcloud.io/dashboard?id=nanoframework_lib-nanoFramework.System.Math) [![License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE) [![NuGet](https://img.shields.io/nuget/dt/nanoFramework.System.Math.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.System.Math/) [![#yourfirstpr](https://img.shields.io/badge/first--timers--only-friendly-blue.svg)](https://github.com/nanoframework/Home/blob/main/CONTRIBUTING.md) [![Discord](https://img.shields.io/discord/478725473862549535.svg?logo=discord&logoColor=white&label=Discord&color=7289DA)](https://discord.gg/gCyBu8T)

![nanoFramework logo](https://raw.githubusercontent.com/nanoframework/Home/main/resources/logo/nanoFramework-repo-logo.png)

-----

### Welcome to the .NET **nanoFramework** System.Math Class Library repository

## Build status

| Component | Build Status | NuGet Package |
|:-|---|---|
| nanoFramework.System.Math | [![Build Status](https://dev.azure.com/nanoframework/System.Math/_apis/build/status/System.Math?repoName=nanoframework%2FSystem.Math&branchName=main)](https://dev.azure.com/nanoframework/System.Math/_build/latest?definitionId=10&repoName=nanoframework%2FSystem.Math&branchName=main) | [![NuGet](https://img.shields.io/nuget/v/nanoFramework.System.Math.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.System.Math/) |

## Available APIs and floating-point implementations

The .NET [System.Math](https://docs.microsoft.com/en-us/dotnet/api/system.math) APIs are available with `double` parameters. No sweat for the CPUs where the code usually runs.
When we move to embedded systems that's a totally different story.

A few more details to properly set context:

- [`double` type](https://docs.microsoft.com/en-us/dotnet/api/system.double): represents a double-precision 64-bit number with values ranging from negative 1.79769313486232e308 to positive 1.79769313486232e308. Precision ~15-17 digits. Size 8 bytes.
- [`float` type](https://docs.microsoft.com/en-us/dotnet/api/system.single): represents a single-precision 32-bit number with values ranging from negative 3.402823e38 to positive 3.402823e38. Precision ~6-9 digits. Size 4 bytes.
- Comparison of [floating-point numeric types](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/floating-point-numeric-types).

There are all sorts of variants and combinations on how to deal with FP and DP in the embedded world. From hardware support on the CPU to libraries that perform those calculations at the expense of more code and execution speed. .NET **nanoFramework** targets 32-bit MCUs, therefore support for 64-bits calculations requires extra code and processing.

Adding to the above, the extra precision provided by the `double` type is seldom required on typical embedded application use cases.

Considering all this and the ongoing quest to save flash space, despite System.Math API offering `double` type parameters, the type handling at the native code will depend on build options of the firmware. The default implementation works with `float` under the hood. There is a build option (`DP_FLOATINGPOINT`) to build the image with DP floating point, which should be used when 'that' extra precision is required.

In case this is relevant, the capability of dealing with double floating point is exposed through this property [`SystemInfo.FloatingPointSupport`](https://docs.nanoframework.net/api/nanoFramework.Runtime.Native.SystemInfo.html#nanoFramework_Runtime_Native_SystemInfo_FloatingPointSupport).

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
