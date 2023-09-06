# .NET 6.0+ sample test project

This project works with .NET 6.0+. Its intended it to allow easy debugging using a USB to TTL dongle or equivalent.

Couple of important notes:

* The files used are the exact same of the ones for .NET nanoFramework build.
* If a file is added or removed or a directory adjusted, you will have to adjust the csproj file.
* The `IMqqtClient` interface and the associated requirements has been added manually. Please make sure that if the files from the [nanoFramework.M2Mqtt.Core](https://github.com/nanoframework/nanoFramework.m2mqtt/tree/main/nanoFramework.M2Mqtt.Core) are moved properly. The nuget won't work as it does only support .NET nanoFramework.
* `SpanByte.cs` has been added as well as not a native system type of .NET. If any change on this file, please make sure you'll update it as well.

You can run the project directly on Windows, Linux or MacOS using an valid serial port where you'll connect the modem.