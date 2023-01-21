# Description

This script was created to sync Stylecop settings across all projects in pointed path. It's iterating through all folders looking for .nfproj files.
If any found, then update it configuration to use Stylecop with settings from this folder. 

## How to use

Run this script from power shell console.

```shell
.\syncSettingsOverEveryDevice.ps1

## Configuration
All configurable options are available at the top of script as variables. You can change few parameters inside the script.

$projectWhiteList = ("AD5328", "4Relay") - **Project folders which should be considered when running script. Leave empty for all.**

$styleCopPackageVersion = "6.2.0" **- version of StyleCop.MSBuild package**

$styleCopTreatErrorsAsWarningsNodeValue = "false" **- should stylecop errors be treated as compilation warnings or errors**

$styleCopSettingsFileName = "Settings.StyleCop" **- path to main Settings.StyleCop file which is used to sync settings for all projects**

$devicesMainFolderPath = "..\devices"