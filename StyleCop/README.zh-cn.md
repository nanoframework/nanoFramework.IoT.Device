文档语言: [English](README.md) | [简体中文](README.zh-cn.md)

# 描述

创建此脚本是为了在指定路径下的所有项目中同步Stylecop设置。它正在遍历所有文件夹，寻找.nfproj文件。
如果找到，那么更新它的配置，使用这个文件夹中的设置Stylecop。

## 如何使用

从powershell控制台运行此脚本。

```shell
.\syncSettingsOverEveryDevice.ps1

## Configuration
所有可配置的选项都可以在脚本的顶部作为变量使用。您可以在脚本中更改一些参数。

$projectWhiteList = ("AD5328", "4Relay") - **运行脚本时应该考虑的项目文件夹。空着给所有的人。**

$styleCopPackageVersion = "6.2.0" **- 版本的StyleCop.MSBuild包**

$styleCopTreatErrorsAsWarningsNodeValue = "false" **- 是否应该将stylecop错误视为编译警告或错误**

$styleCopSettingsFileName = "Settings.StyleCop" **- 路径到主Settings.StyleCop 文件，用于同步所有项目的设置**

$devicesMainFolderPath = "..\devices"
