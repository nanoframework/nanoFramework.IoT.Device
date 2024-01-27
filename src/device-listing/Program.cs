// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Iot.Tools.DeviceListing;

string[] categoriesToDisplay = new string[]
{
    "adc",
    "accelerometer",
    "gas",
    "liquid",
    "light",
    "barometer",
    "altimeter",
    "thermometer",
    "infrared",
    "gyroscope",
    "compass",
    "lego",
    "motor",
    "imu",
    "magnetometer",
    "hygrometer",
    "clock",
    "sonar",
    "distance",
    "pir",
    "motion",
    "display",
    "io-expander",
    "canbus",
    "proximity",
    "touch",
    "wireless",
    "joystick",
    "color",
    "led",
    "nfc",
    "media",
    "usb",
    "gpio",
    "multi",
    "protocol",
    "power",
    "dac",
    "eeprom",
    "helper",
    "system",
    "lidar",
    "reader",
    "satellite",
    "particulatematter",
};

Dictionary<string, string?> categoriesDescriptions = new()
{
    { "adc", "Analog/Digital converters" },
    { "accelerometer", "Accelerometers" },
    { "voc", "Volatile Organic Compound sensors" },
    { "gas", "Gas sensors" },
    { "liquid", "Liquid sensors" },
    { "light", "Light sensor" },
    { "barometer", "Barometers" },
    { "altimeter", "Altimeters" },
    { "thermometer", "Thermometers" },
    { "infrared", "Infrared sensors" },
    { "gyroscope", "Gyroscopes" },
    { "compass", "Compasses" },
    { "lego", "Lego related devices" },
    { "motor", "Motor controllers/drivers" },
    { "imu", "Inertial Measurement Units" },
    { "magnetometer", "Magnetometers" },
    { "lcd", "Liquid Crystal Displays" },
    { "hygrometer", "Hygrometers" },
    { "rtc", "Real Time Clocks" },
    { "clock", "Clocks" },
    { "sonar", "Sonars" },
    { "distance", "Distance sensors" },
    { "pir", "Passive InfraRed (motion) sensors" },
    { "motion", "Motion sensors" },
    { "display", "Displays" },
    { "segment", "Segment displays" },
    { "io-expander", "GPIO Expanders" },
    { "canbus", "CAN BUS libraries/modules" },
    { "proximity", "Proximity sensors" },
    { "touch", "Touch sensors" },
    { "wireless", "Wireless communication modules" },
    { "radio", "Radio modules" },
    { "pwm", "PWM libraries/modules" },
    { "spi", "SPI libraries/modules" },
    { "joystick", "Joysticks" },
    { "color", "Color sensors" },
    { "led", "LED drivers" },
    { "nfc", "RFID/NFC modules" },
    { "media", "Media libraries" },
    { "usb", "USB devices" },
    { "gpio", "GPIO or bit operating devices" },
    { "power", "Power monitors and related devices" },
    { "multi", "Multi-device or robot kit" },
    // Bucket for stuff we want mentioned but there is no clear category
    // In other words: anything allowing a way to create PWM channel, SPI/I2C/... device
    { "protocol", "Protocols providers/libraries" },
    { "characterlcd", null },
    { "brickpi3", null },
    { "buzzer", null },
    { "gopigo3", null },
    { "grovepi", null },
    { "i2c", null },
    { "multiplexer", null },
    { "dac", "Digital/Analog converters" },
    { "eeprom", "EEPROM" },
    { "helper", "Iot.Device helpers and common" },
    { "system", ".NET System libraries" },
    { "lidar", "Lidar" },
    { "reader", "Readers" },
    { "satellite", " modules" },
    { "particulatematter", "Particulate Matter Sensor" },
};

HashSet<string> ignoredDeviceDirectories = new()
{
    "Common",
    "Units",
    "Interop",
};

var repoRootPath = Path.Combine(Environment.GetEnvironmentVariable("BUILD_SOURCESDIRECTORY"), "nanoFramework.IoT.Device");

Console.WriteLine($"Finding repository root. Starting point is: {repoRootPath}");

string? repoRoot = CheckRepoRoot(repoRootPath);

if (repoRoot is null)
{
    Console.WriteLine("Error: not in a git repository");
    return;
}

string devicesPath = Path.Combine(repoRoot, "devices");

List<DeviceInfo> devices = new();

foreach (string directory in Directory.EnumerateDirectories(devicesPath))
{
    if (IsIgnoredDevice(directory))
    {
        continue;
    }

    string readme = Path.Combine(directory, "README.md");
    string categories = Path.Combine(directory, "category.txt");

    if (File.Exists(readme))
    {
        var device = new DeviceInfo(readme, categories);

        if (device.Title == null)
        {
            Console.WriteLine($"Warning: Device directory contains readme file without title on the first line. [{directory}]");
            continue;
        }

        devices.Add(device);
    }
    else
    {
        Console.WriteLine($"Warning: Device directory does not have a README.md file. [{directory}]");
    }
}

devices.Sort();

var allCategories = new HashSet<string>();

foreach (DeviceInfo device in devices)
{
    bool beingDisplayed = false;
    foreach (string category in device.Categories)
    {
        if (allCategories.Add(category))
        {
            if (!categoriesDescriptions.ContainsKey(category))
            {
                Console.WriteLine($"Warning: Category `{category}` is missing description (`{device.Title}`). [{device.ReadmePath}]");
            }
        }

        beingDisplayed |= !beingDisplayed && categoriesToDisplay.Contains(category);
    }

    if (!beingDisplayed && device.CategoriesFileExists)
    {
        // We do not want to show the warning when file doesn't exist as you will get separate warning that category.txt is missing in that case.
        Console.WriteLine($"Warning: Device `{device.Title}` is not being displayed under any category. [{device.CategoriesFilePath}]");
    }
}

string alphabeticalDevicesIndex = Path.Combine(repoRoot, "README.md");
string deviceListing = GetDeviceListing(repoRoot, devices);
ReplacePlaceholder(alphabeticalDevicesIndex, "devices", deviceListing);

string categorizedDeviceListing = GetCategorizedDeviceListing(devicesPath, devices);
string devicesReadme = Path.Combine(devicesPath, "README.md");
ReplacePlaceholder(devicesReadme, "categorizedDevices", categorizedDeviceListing);

string GetDeviceListing(string devicesPath, IEnumerable<DeviceInfo> devices)
{
    var deviceListing = new StringBuilder();
    foreach (DeviceInfo device in devices)
    {
        if (device.Name.StartsWith("System"))
        {
            deviceListing.AppendLine($"* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.{device.Name}.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.{device.Name}/) [{device.Title}]({CreateMarkdownLinkFromPath(device.ReadmePath, devicesPath)})");
        }
        else
        {
            if ((device.Name == "NumberHelper") || (device.Name == "WeatherHelper"))
            {
                deviceListing.AppendLine($"* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Common.{device.Name}.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Common.{device.Name}/) [{device.Title}]({CreateMarkdownLinkFromPath(device.ReadmePath, devicesPath)})");
            }
            else if (device.Name == "Card")
            {
                deviceListing.AppendLine($"* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.Card.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.Card/) [{device.Title}]({CreateMarkdownLinkFromPath(device.ReadmePath, devicesPath)})");
            }
            else
            {
                deviceListing.AppendLine($"* [![NuGet](https://img.shields.io/nuget/v/nanoFramework.IoT.Device.{device.Name}.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.IoT.Device.{device.Name}/) [{device.Title}]({CreateMarkdownLinkFromPath(device.ReadmePath, devicesPath)})");
            }
        }
    }

    return deviceListing.ToString();
}

string GetCategorizedDeviceListing(string devicesPath, IEnumerable<DeviceInfo> devices)
{
    var deviceListing = new StringBuilder();
    foreach (string categoryToDisplay in categoriesToDisplay)
    {
        if (categoriesDescriptions.TryGetValue(categoryToDisplay, out string? categoryDescription))
        {
            string listingInCurrentCategory = GetDeviceListing(devicesPath, devices.Where((d) => d.Categories.Contains(categoryToDisplay)));
            if (!string.IsNullOrEmpty(listingInCurrentCategory))
            {
                deviceListing.AppendLine($"## {categoryDescription}");
                deviceListing.AppendLine();
                deviceListing.AppendLine(listingInCurrentCategory);
            }
        }
        else
        {
            Console.WriteLine($"Warning: Category `{categoryToDisplay}` should be displayed but is missing description.");
        }
    }

    return deviceListing.ToString();
}

string? CheckRepoRoot(string dir)
{
    if (dir is { Length: > 0 })
    {
        if (Directory.Exists(Path.Combine(dir, ".git")))
        {
            return dir;
        }
        else
        {
            DirectoryInfo? parentDir = new DirectoryInfo(dir).Parent;
            return parentDir?.FullName == null ? null : CheckRepoRoot(parentDir.FullName);
        }
    }

    return null;
}

string CreateMarkdownLinkFromPath(string path, string parentPath)
{
    if (path.StartsWith(parentPath))
    {
        if (!path.Contains(parentPath))
        {
            throw new Exception($"No common path between `{path}` and `{parentPath}`");
        }

        // Removing this code for now to allow the docs to build properly, experience is still very good on GitHub: .Replace("\\README.md", "");
        var relativePath = path.Substring(parentPath.Length + 1).Replace("\\README.md", "");
        UriBuilder uriBuilder = new UriBuilder() { Path = relativePath };

        return uriBuilder.Path;
    }

    throw new Exception($"No common path between `{path}` and `{parentPath}`");
}

bool IsIgnoredDevice(string path)
{
    string dirName = new DirectoryInfo(path).Name;
    return ignoredDeviceDirectories.Contains(dirName);
}

void ReplacePlaceholder(string filePath, string placeholderName, string newContent)
{
    string fileContent = File.ReadAllText(filePath);

    string startTag = $"<{placeholderName}>";
    string endTag = $"</{placeholderName}>";

    int startIdx = fileContent.IndexOf(startTag);
    int endIdx = fileContent.IndexOf(endTag);

    if (startIdx == -1 || endIdx == -1)
    {
        throw new Exception($"`{startTag}` not found in `{filePath}`");
    }

    startIdx += startTag.Length;

    File.WriteAllText(
        filePath,
        fileContent.Substring(0, startIdx) +
        Environment.NewLine +
        // Extra empty line is needed so that github does not break bullet points
        Environment.NewLine +
        newContent +
        fileContent.Substring(endIdx));
}
