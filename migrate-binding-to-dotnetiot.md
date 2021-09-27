# Migrate binding to .NET IoT

So you have created your shiney .NET nanoFramework binding for a cool sensor or device. **That's great!** But did you know that with minimal efforts you can make your hard word available for even more .NET developers? This article describes the steps you can take to port a .NET nanoFramework binding to [.NET IoT](https://github.com/dotnet/iot).

## Steps

### 1. Fork the .NET IoT repo

Since we can't push directly to the .NET IoT repo, we need to create a fork. This can easily be done from the GitHub web UI, or by the GitHub CLI (see the [instructions on in the GitHub docs](https://docs.github.com/en/get-started/quickstart/fork-a-repo)).

### 2. Clone and build the forked repo

Once we have created the fork, we can clone the forked repo. (update the URL in the command below, to match the URL of the forked repo):

```shell
git clone https://github.com/YOUR-GITHUB-ACCOUNT/iot.git
```

It's recommended to also create a branch for the work you are going to do:

```shell
git checkout -b BRANCHNAME
```

To verify your development environment has the correct .NET SDK's, and all dependencies are available locally it's recommneded to try to build the code. You can do that by running `Build.cmd` (on Windows) or `build.sh` (on Linux) which can be found in the root of the code base.

### 3. Create a new project for your binding and copy code

In the `/src/devices` folder, create a new subfolder for your binding (e.g. by using the name of the sensor).

In this folder , create a new empty .NET project. This can be done with the Visual Studio UI, or by using the command line:

```shell
dotnet new classlib
```

You can delete the .cs files (e.g. `Class1.cs`) which could have been created automatically based on your chosen template.

From your nanoFramework binding source code, copy over all .cs files and the .sln file to the folder you created in step 3.

Repead the process (project creation and copying .cs files) for the code samples as well.

### 4. Fix the code

At this point in time, probably your code does not build correctly. Here are some pointers on how to fix them:

- **Solution (.sln) file**: the binding and sample projects are now normal .NET projects instead of nanoFramework projects, their file extension are `.csproj` instead of `.nfproj`. In the `.sln` file, fix the projects filenames (by replacing `.nfpro` with `.csproj`). Additionally in the solution file, make sure the `ProjectConfigurationPlatforms` section, make sure all your projects have all platforms used by .NET IoT. In the sample below you see 6 lines for Release and 6 lines for Debug builds, per project, per project (typically the binding project and the samples project):

```text
    GlobalSection(ProjectConfigurationPlatforms) = postSolution
        {E6797049-4558-4E85-81CF-18576A6D384C}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
        {E6797049-4558-4E85-81CF-18576A6D384C}.Debug|Any CPU.Build.0 = Debug|Any CPU
        {E6797049-4558-4E85-81CF-18576A6D384C}.Debug|x64.ActiveCfg = Debug|Any CPU
        {E6797049-4558-4E85-81CF-18576A6D384C}.Debug|x64.Build.0 = Debug|Any CPU
        {E6797049-4558-4E85-81CF-18576A6D384C}.Debug|x86.ActiveCfg = Debug|Any CPU
        {E6797049-4558-4E85-81CF-18576A6D384C}.Debug|x86.Build.0 = Debug|Any CPU
        {E6797049-4558-4E85-81CF-18576A6D384C}.Release|Any CPU.ActiveCfg = Release|Any CPU
        {E6797049-4558-4E85-81CF-18576A6D384C}.Release|Any CPU.Build.0 = Release|Any CPU
        {E6797049-4558-4E85-81CF-18576A6D384C}.Release|x64.ActiveCfg = Release|Any CPU
        {E6797049-4558-4E85-81CF-18576A6D384C}.Release|x64.Build.0 = Release|Any CPU
        {E6797049-4558-4E85-81CF-18576A6D384C}.Release|x86.ActiveCfg = Release|Any CPU
        {E6797049-4558-4E85-81CF-18576A6D384C}.Release|x86.Build.0 = Release|Any CPU
        {4E23EAAD-D6CF-451C-AAD0-E80991CF8EDD}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
        {4E23EAAD-D6CF-451C-AAD0-E80991CF8EDD}.Debug|Any CPU.Build.0 = Debug|Any CPU
        {4E23EAAD-D6CF-451C-AAD0-E80991CF8EDD}.Debug|x64.ActiveCfg = Debug|Any CPU
        {4E23EAAD-D6CF-451C-AAD0-E80991CF8EDD}.Debug|x64.Build.0 = Debug|Any CPU
        {4E23EAAD-D6CF-451C-AAD0-E80991CF8EDD}.Debug|x86.ActiveCfg = Debug|Any CPU
        {4E23EAAD-D6CF-451C-AAD0-E80991CF8EDD}.Debug|x86.Build.0 = Debug|Any CPU
        {4E23EAAD-D6CF-451C-AAD0-E80991CF8EDD}.Release|Any CPU.ActiveCfg = Release|Any CPU
        {4E23EAAD-D6CF-451C-AAD0-E80991CF8EDD}.Release|Any CPU.Build.0 = Release|Any CPU
        {4E23EAAD-D6CF-451C-AAD0-E80991CF8EDD}.Release|x64.ActiveCfg = Release|Any CPU
        {4E23EAAD-D6CF-451C-AAD0-E80991CF8EDD}.Release|x64.Build.0 = Release|Any CPU
        {4E23EAAD-D6CF-451C-AAD0-E80991CF8EDD}.Release|x86.ActiveCfg = Release|Any CPU
        {4E23EAAD-D6CF-451C-AAD0-E80991CF8EDD}.Release|x86.Build.0 = Release|Any CPU
    EndGlobalSection
```

- **Project (.csproj) files**: modify your `.csproj` files so they match the settings used across the .NET IoT repo:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFrameworks>$(DefaultBindingTfms)</TargetFrameworks>
    <!--Disabling default items so samples source won't get build by the main library-->
    <EnableDefaultItems>false</EnableDefaultItems>
  </PropertyGroup>
  <ItemGroup>
    <Compile Include="*.cs" />
  </ItemGroup>
</Project>
```

- **Replace `SpanByte`**: in normal .NET code we have to replace `SpanByte` from nanoFramework with `Span<Byte>`. Below you can find some common code nanoFramework patterns and how they should be changed. It's also recommended to use `stackalloc` instead of the `new` keyword to allocate the memory on the stack ([see .NET docs](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/stackalloc)):

```csharp
// nanoFramework code:
public abstract void ReadBytes(I2cDevice i2CDevice, byte reg, SpanByte readBytes);
// Replace with:
public abstract void ReadBytes(I2cDevice i2CDevice, byte reg, Span<Byte> readBytes);

// nanoFramework code:
SpanByte dataout = new byte[]
{
    reg,
    data
};
i2cDevice.Write(dataout);
// Replace with:
Span<Byte> dataout = stackalloc byte[]
{
    reg,
    data
};
```

- **Replace `Debug.WriteLine` with `Console.WriteLine`**, typically in your sample code:

```csharp
// nanoFramework code:
Debug.WriteLine($"ABC: {xyz}");
// Replace with:
Console.WriteLine($"ABC: {xyz}");
```

- **Remove nanoFramework specific code**: if your (sample) code contains any code specific to the nanoFramework, it has to replaced or removed. For example sample code using ESP32 pin.

- **Infinite while loop**: in a nanoFramework app it's quite common to have an infinite `while` loop, or a `Thread.Sleep` with the `Timeout.Infinite`. Probably you want to replace them with more appropriate code, e.g.:

```csharp
// nanoFramework code:
while(true)
{ 
}
// Replace with:
while(!Console.KeyAvailable)
{
}

// nanoFramework code:
Thread.Sleep(Timeout.Infinite);
// Replace with:
Console.ReadKey();
```

### 5. Create PR

Once your code builds again, if possible test it for example on a Rapsberry Pi with your sensor attached. Now you are ready to create a PR from your newly created branch, back to the .NET IoT repo.