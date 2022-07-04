$ErrorActionPreference = 'Stop'

#Consts
$nfProjXmlNamespace = "http://schemas.microsoft.com/developer/msbuild/2003"
$styleCopPackageVersion = "6.2.0"
$styleCopPackageTargetFramework = "netnano1.0"
$styleCopPackageDevelopmentDependency = "true"

#Paths
$styleCopSettingsFileName = "Settings.StyleCop";
$devicesMainFolderPath = "..\devices"

$styleCopSettingsSourceFileContent = Get-Content $styleCopSettingsFileName
$allDevicesFolders = Get-ChildItem -Path $devicesMainFolderPath -Directory

function Test-PSCustomObjectEquality {
  param(
    [Parameter(Mandatory = $true)]
    [PSCustomObject] $firstObject,

    [Parameter(Mandatory = $true)]
    [PSCustomObject] $secondObject
  )
  -not (Compare-Object $firstObject.PSObject.Properties $secondObject.PSObject.Properties)
}

function SyncStyleCopSettings {
  param(
    [Parameter(Mandatory = $true)]
    [String] $deviceFolder
  )
  $pathToSettingInCurrentFolder = $deviceFolder + '\' + $styleCopSettingsFileName

  if (Test-Path -Path $pathToSettingInCurrentFolder -PathType Leaf) {
    Write-Host "File exists in path " $pathToSettingInCurrentFolder". Checking file content" -ForegroundColor Green
    $contentOfExistingFile = Get-Content $pathToSettingInCurrentFolder

    $comapreResult = (Compare-Object $styleCopSettingsSourceFileContent $contentOfExistingFile)
    if ($comapreResult.Count -eq 0) {
      Write-Host "Existing file matches source" -ForegroundColor Green
      return
    }
  }

  Write-Host "Creating/replacing file in " $pathToSettingInCurrentFolder -ForegroundColor Yellow
  Set-Content $pathToSettingInCurrentFolder $styleCopSettingsSourceFileContent
}

function UpdatePackagesConfig
{
  param (
    [Parameter(Mandatory = $true)]
    [String] $nfProjFile
  )

  $dirPath = Split-Path -Path $nfProjFile;
  $packagesPath = "$dirPath\packages.config"

  if (!(Test-Path $packagesPath -PathType Leaf)){
    throw "$packagesPath does not exists"
  }

  [xml]$packagesContent = Get-Content $packagesPath
  #check if stylecop ms build exists
  #if not, insert
  $styleCopPackage = $packagesContent.packages | Where-Object { $_.id -eq "StyleCop.MSBuild" }
  if ($null -eq $styleCopPackage) {
    $styleCopPackage = $packagesContent.CreateElement("package")
    $styleCopPackage.SetAttribute("id", "StyleCop.MSBuild")
    $packagesContent.packages.AppendChild($styleCopPackage)
  }

  $version = $styleCopPackage.GetAttribute("version")
  if (($null -eq $version) -or ($version -ne $styleCopPackageVersion)) {
    $styleCopPackage.SetAttribute("version", $styleCopPackageVersion)
  }

  $targetFramework = $styleCopPackage.GetAttribute("targetFramework")
  if (($null -eq $targetFramework) -or ($targetFramework -ne $styleCopPackageTargetFramework)) {
    $styleCopPackage.SetAttribute("targetFramework", $styleCopPackageTargetFramework)
  }

  $developmentDependency = $styleCopPackage.GetAttribute("developmentDependency")
  if (($null -eq $targetFramework) -or ($targetFramework -ne $styleCopPackageDevelopmentDependency)) {
    $styleCopPackage.SetAttribute("developmentDependency", $styleCopPackageDevelopmentDependency)
  }

  $packagesContent.Save($packagesPath);
}

function EnsureNfProjHasStyleCopSettings 
{
  param (
    [Parameter(Mandatory = $true)]
    [String] $deviceFolder
  )
  
  $allNfProjFiles = Get-ChildItem -Path $deviceFolder -Recurse -Include *.nfproj;
  foreach ($nfProj in $allNfProjFiles) 
  {
    #Skip sample projects
    if ($nfProj -like '*sample*') {
      continue
    }

    Write-Host "Found nfProj: " $nfProj
    [xml]$nfProjContent = Get-Content $nfProj
    $propertyGroupWithProjectGuid = $nfProjContent.SelectNodes("/").Project.PropertyGroup[1];

    $styleCopErrorsSettingNode = $propertyGroupWithProjectGuid.StyleCopTreatErrorsAsWarnings #BUG:
    if ($null -eq $styleCopErrorsSettingNode){
      $styleCopErrorsSettingNode = $nfProjContent.CreateElement("StyleCopTreatErrorsAsWarnings", $nfProjXmlNamespace)
      $notUsed = $propertyGroupWithProjectGuid.AppendChild($styleCopErrorsSettingNode)
    }
    #$styleCopErrorsSettingNode.InnerText = "false" #BUG: throws error when node exists
    #We don't want unnecessery output in console, without assigment node is displayed


    #Import & Target
    $projectNode = $nfProjContent.SelectNodes("/").Project;
    $importStyleCopProject = $projectNode.Import | Where-Object { $_.Project -eq ".\packages\StyleCop.MSBuild.6.2.0\build\StyleCop.MSBuild.targets" }
    if ($null -eq $importStyleCopProject){
      $importStyleCopProject = $nfProjContent.CreateElement("Import", $nfProjXmlNamespace)
      $importStyleCopProject.SetAttribute("Project", ".\packages\StyleCop.MSBuild.6.2.0\build\StyleCop.MSBuild.targets")
      $importStyleCopProject.SetAttribute("Condition", "Exists('.\packages\StyleCop.MSBuild.6.2.0\build\StyleCop.MSBuild.targets')")

      #I assume if there is no import node, there is no target also Target
      $targetStyleCop = $nfProjContent.CreateElement("Target", $nfProjXmlNamespace)
      $targetStyleCop.SetAttribute("Name", "EnsureNuGetPackageBuildImports")
      $targetStyleCop.SetAttribute("BeforeTargets", "PrepareForBuild")

      $targetStyleCopPropertyGroup = $nfProjContent.CreateElement("PropertyGroup", $nfProjXmlNamespace)
      $notUsed = $targetStyleCop.AppendChild($targetStyleCopPropertyGroup)

      $targetStyleCopPropertyGroupErrorText = $nfProjContent.CreateElement("ErrorText", $nfProjXmlNamespace)
      $targetStyleCopPropertyGroupErrorText.InnerText = "This project references NuGet package(s) that are missing on this computer. Use NuGet Package Restore to download them.  For more information, see http://go.microsoft.com/fwlink/?LinkID=322105. The missing file is {0}."
      $notUsed = $targetStyleCopPropertyGroup.AppendChild($targetStyleCopPropertyGroupErrorText)

      $targetErrorCondition = $nfProjContent.CreateElement("Error", $nfProjXmlNamespace)
      $targetErrorCondition.SetAttribute("Condition", "!Exists('.\packages\StyleCop.MSBuild.6.2.0\build\StyleCop.MSBuild.targets')&quot; Text=&quot;`$([System.String]::Format('`$(ErrorText)', '.\packages\StyleCop.MSBuild.6.2.0\build\StyleCop.MSBuild.targets'))")
      $notUsed = $targetStyleCop.AppendChild($targetErrorCondition)

      $notUsed = $projectNode.AppendChild($importStyleCopProject)
      $notUsed = $projectNode.AppendChild($targetStyleCop)
    }

    $nfProjContent.Save($nfProj);

    #Clean file from &quot;
    #Event if we use " in SetAttribute PS is inserting &quot; instead of "
    #Same for &amp;
    $fileContent = Get-Content $nfProj
    $fileContent = $fileContent.Replace("&quot;", "`"")
    $fileContent = $fileContent.Replace("quot;", "`"")
    $fileContent = $fileContent.Replace("&amp;", "")
    $fileContent = $fileContent.Replace("amp;", "")
    Set-Content -Path $nfProj -Value $fileContent

    UpdatePackagesConfig $nfProj
  }
}

#TODO: Remove in final version
function isProjectWhitelisted($deviceFullName){
  if ($deviceFullName -like '*AD5328*'){
    return $true
  }

  return $false
}

foreach ($deviceFolder in $allDevicesFolders) {
  #TODO: Remove in final version
  if (isProjectWhitelisted $deviceFolder.FullName -eq $true) {
    Write-Host "Checking " $deviceFolder.FullName -ForegroundColor Green
    SyncStyleCopSettings $deviceFolder.FullName
    EnsureNfProjHasStyleCopSettings $deviceFolder.FullName
  }
}