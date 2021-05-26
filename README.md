[![#yourfirstpr](https://img.shields.io/badge/first--timers--only-friendly-blue.svg)](https://github.com/nanoframework/Home/blob/main/CONTRIBUTING.md) [![Discord](https://img.shields.io/discord/478725473862549535.svg?logo=discord&logoColor=white&label=Discord&color=7289DA)](https://discord.gg/gCyBu8T)

![nanoFramework logo](https://github.com/nanoframework/Home/blob/main/resources/logo/nanoFramework-repo-logo.png)

-----

# Welcome to the **nanoFramework** IoT.Device Library repository!

This repository contains bindings which can be sensors, small screen and anything else that you can connect to your nanoFramework chip!

Most of the bindings have been migrated from [.NET IoT repository](https://github.com/dotnet/iot/tree/main/src/devices). Not all the bindings make sense to migrate to .NET nanoFramework, so the effort of migration has been placed into devices that can work with .NET nanoFramework. Please note as well that some devices have been migrated without been tested, so they main contain problems.

## List of devices

<devices>
</devices>

## Folder Structure

[/src/devices/](/src/devices/) contains devices that were cleaned up and should be working out of the box.

[/src/devices_generated/](/src/devices_generated/) contains devices that were automatically ported from [the NET Core IoT Libraries devices](https://github.com/dotnet/iot/tree/main/src/devices). They might not work or compile at this point, but are a good starting point if you need support for one of the devices contained here but missing from the [/src/devices/](/src/devices/) folder.

[/src/nanoFramework.IoT.Device.CodeConverter](/src/nanoFramework.IoT.Device.CodeConverter) contains the tool used to generate the devices from [the NET Core IoT Libraries devices](https://github.com/dotnet/iot/tree/main/src/devices).

Other folders in [/src](/src) contain nanoFramework projects that you can reference when creating/updating devices with provide functionality such as a StopWatach, a DelayHelper, BinaryPrimitives or various System.Device.Model Attributes.

## Contributing

**Important:** If you plan to clean up the code in [/src/devices_generated/](/src/devices_generated/), please copy your work to the [/src/devices/](/src/devices/) folder as the content of [/src/devices_generated/](/src/devices_generated/) will be overwritten by the generator tool.

Please check the [detail list of tips and tricks](./tips-tricks.md) to facilitate the migration. The generator takes care of some heavy lifting but there is always some manual adjustments needed.

We are using the following structure for the bindings:

```text
/devices
  /Binding1
    /samples
      Binding1.Samples.nfproj
      AssicateFile.cs
      Program.cs
    /test
      BindingA.Test.nfproj
      AssociatedTestFile.cs
    Binding1.nfproj
    Binding1.nuspec
    version.json
    OtherFiles.cs
    OtherFiles.anythingelse
    Readme.md
```

## Using the Code Converter

The Code Converter allows to facilitate migration of .NET Core/.NET 5.0 code into .NET nanoFramework. More information and how to [customize and run it here](./src/nanoFramework.IoT.Device.CodeConverter/README.md).

## Feedback and documentation

For documentation, providing feedback, issues and finding out how to contribute please refer to the [Home repo](https://github.com/nanoframework/Home).

Join our Discord community [here](https://discord.gg/gCyBu8T).

## Credits

The list of contributors to this project can be found at [CONTRIBUTORS](https://github.com/nanoframework/Home/blob/main/CONTRIBUTORS.md).

## License

The **nanoFramework** Class Libraries are licensed under the [MIT license](LICENSE.md).

## Code of Conduct

This project has adopted the code of conduct defined by the Contributor Covenant to clarify expected behavior in our community.
For more information see the [.NET Foundation Code of Conduct](https://dotnetfoundation.org/code-of-conduct).

### .NET Foundation

This project is supported by the [.NET Foundation](https://dotnetfoundation.org).
