// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Iot.Tools.DeviceListing;

Dictionary<string, string?> categoriesDescriptions = JsonSerializer.Deserialize<Dictionary<string, string?>>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "categories.json")));

HashSet<string> ignoredDeviceDirectories = new()
{
    "Common",
    "Units",
    "Interop",
};

string repoRootPath;
if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("BUILD_SOURCESDIRECTORY")))
{
    repoRootPath = Path.Combine(Environment.GetEnvironmentVariable("BUILD_SOURCESDIRECTORY"), "nanoFramework.IoT.Device");
}
else
{
    // Find the first directory that contains a .git folder
    repoRootPath = FindGitRepositoryRoot(Environment.CurrentDirectory);
}

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

        beingDisplayed |= !beingDisplayed && categoriesDescriptions.Keys.Contains(category);
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
        deviceListing.AppendLine($"* [![NuGet](https://img.shields.io/nuget/v/{device.NuGetPackageId}.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/{device.NuGetPackageId}/) [{device.Title}]({CreateMarkdownLinkFromPath(device.ReadmePath, devicesPath)})");
    }

    return deviceListing.ToString();
}

string GetCategorizedDeviceListing(string devicesPath, IEnumerable<DeviceInfo> devices)
{
    var deviceListing = new StringBuilder();
    foreach (string categoryToDisplay in categoriesDescriptions.Keys)
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

string? FindGitRepositoryRoot(string startDirectory)
{
    DirectoryInfo? currentDirectory = new DirectoryInfo(startDirectory);

    while (currentDirectory != null)
    {
        if (Directory.Exists(Path.Combine(currentDirectory.FullName, ".git")))
        {
            return currentDirectory.FullName;
        }

        currentDirectory = currentDirectory.Parent;
    }

    return null;
}