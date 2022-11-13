$ErrorActionPreference = 'Stop'

#Whitelisted projects
$projectWhiteList = ("ePaper")

#Const packages.config
$styleCopPackageId = "StyleCop.MSBuild"
$styleCopPackageVersion = "6.2.0"
$styleCopPackageTargetFramework = "netnano1.0"
$styleCopPackageDevelopmentDependency = "true"

#Consts nfProj
$nfProjXmlNamespace = "http://schemas.microsoft.com/developer/msbuild/2003"
$styleCopTreatErrorsAsWarningsNodeName = "StyleCopTreatErrorsAsWarnings"
$styleCopTreatErrorsAsWarningsNodeValue = "false"
$styleCopImportPackageTargetPath = "packages\StyleCop.MSBuild.$styleCopPackageVersion\build\StyleCop.MSBuild.targets"

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

function EnsureXmlAttributeExists {
  param (
    [Parameter(Mandatory = $true)]
    [String] $attributeName,

    [Parameter(Mandatory = $true)]
    [String] $expectedValue,

    [Parameter(Mandatory = $true)]
    [System.Xml.XmlElement] $attributeParentNode
  )

  $attribute = $attributeParentNode.GetAttribute($attributeName)
  if (($null -eq $attribute) -or ($attribute -ne $expectedValue)) {
    $attributeParentNode.SetAttribute($attributeName, $expectedValue)
  }
}

function UpdatePackagesConfig {
  param (
    [Parameter(Mandatory = $true)]
    [String] $nfProjFile
  )

  $dirPath = Split-Path -Path $nfProjFile;
  $packagesPath = "$dirPath\packages.config"

  if (!(Test-Path $packagesPath -PathType Leaf)) {
    throw "$packagesPath does not exists"
  }

  [xml]$packagesContent = Get-Content $packagesPath
  #check if stylecop ms build exists
  #if not, insert
  $styleCopPackage = $packagesContent.SelectNodes("packages/package") | Where-Object { $_.id -eq $styleCopPackageId }
  if ($null -eq $styleCopPackage) {
    $styleCopPackage = $packagesContent.CreateElement("package")
    EnsureXmlAttributeExists "id" $styleCopPackageId  $styleCopPackage
    $notUsed = $packagesContent.packages.AppendChild($styleCopPackage)
  }

  EnsureXmlAttributeExists "version" $styleCopPackageVersion $styleCopPackage
  EnsureXmlAttributeExists "targetFramework" $styleCopPackageTargetFramework $styleCopPackage
  EnsureXmlAttributeExists "developmentDependency" $styleCopPackageDevelopmentDependency $styleCopPackage

  $packagesContent.Save($packagesPath);
}

function Cleanup {
  param (
    [Parameter(Mandatory = $true)]
    [String] $nfprojPath
  )

  #Clean file from &quot;
  #Event if we use " in SetAttribute PS is inserting &quot; instead of "
  #Same for &amp;
  $fileContent = Get-Content $nfprojPath
  $fileContent = $fileContent.Replace("&quot;", "`"")
  $fileContent = $fileContent.Replace("quot;", "`"")
  $fileContent = $fileContent.Replace("&amp;", "")
  $fileContent = $fileContent.Replace("amp;", "")
  Set-Content -Path $nfprojPath -Value $fileContent
}

function EnsureNfProjHasStyleCopSettings {
  param (
    [Parameter(Mandatory = $true)]
    [String] $deviceFolder
  )
  
  $allNfProjFiles = Get-ChildItem -Path $deviceFolder -Recurse -Include *.nfproj;
  foreach ($nfProj in $allNfProjFiles) {
    #Skip sample projects
    if ($nfProj -like '*sample*') {
      continue
    }

    #Skip tests project
    if ($nfProj -like '*test*') {
      continue
    }

    Write-Host "Found nfProj: " $nfProj
    [xml]$nfProjContent = Get-Content $nfProj
    $propertyGroupWithProjectGuid = $nfProjContent.SelectNodes("/").Project.PropertyGroup[1];

    $styleCopErrorsSettingNode = $propertyGroupWithProjectGuid.$styleCopTreatErrorsAsWarningsNodeName

    if ($null -eq $styleCopErrorsSettingNode) {
      $styleCopErrorsSettingNode = $nfProjContent.CreateElement($styleCopTreatErrorsAsWarningsNodeName, $nfProjXmlNamespace)
      #We don't want unnecessery output in console, without assigment node content is displayed in console
      $notUsed = $propertyGroupWithProjectGuid.AppendChild($styleCopErrorsSettingNode)
      $styleCopErrorsSettingNode.InnerText = $styleCopTreatErrorsAsWarningsNodeValue
    }
    else {
      $propertyGroupWithProjectGuid.$styleCopTreatErrorsAsWarningsNodeName = $styleCopTreatErrorsAsWarningsNodeValue
    }
    

    #Import & Target
    #TODO: Sync settings 
    $projectNode = $nfProjContent.SelectNodes("/").Project;
    $importStyleCopProject = $projectNode.Import | Where-Object { $_.Project -eq $styleCopImportPackageTargetPath }
    if ($null -eq $importStyleCopProject) {
      $importStyleCopProject = $nfProjContent.CreateElement("Import", $nfProjXmlNamespace)
      EnsureXmlAttributeExists "Project" $styleCopImportPackageTargetPath $importStyleCopProject
      $notUsed = $projectNode.AppendChild($importStyleCopProject)

      #I assume if there is no import node, there is no target also Target
      $targetStyleCop = $nfProjContent.CreateElement("Target", $nfProjXmlNamespace)
      EnsureXmlAttributeExists "Name" "EnsureNuGetPackageBuildImports" $targetStyleCop
      EnsureXmlAttributeExists "BeforeTargets" "PrepareForBuild" $targetStyleCop

      $targetStyleCopPropertyGroup = $nfProjContent.CreateElement("PropertyGroup", $nfProjXmlNamespace)
      $notUsed = $targetStyleCop.AppendChild($targetStyleCopPropertyGroup)

      $targetStyleCopPropertyGroupErrorText = $nfProjContent.CreateElement("ErrorText", $nfProjXmlNamespace)
      $targetStyleCopPropertyGroupErrorText.InnerText = "This project references NuGet package(s) that are missing on this computer. Use NuGet Package Restore to download them.  For more information, see http://go.microsoft.com/fwlink/?LinkID=322105. The missing file is {0}."
      $notUsed = $targetStyleCopPropertyGroup.AppendChild($targetStyleCopPropertyGroupErrorText)

      $targetErrorCondition = $nfProjContent.CreateElement("Error", $nfProjXmlNamespace)
      EnsureXmlAttributeExists "Condition" "!Exists('$styleCopImportPackageTargetPath')&quot; Text=&quot;`$([System.String]::Format('`$(ErrorText)', '$styleCopImportPackageTargetPath'))" $targetErrorCondition
      $notUsed = $targetStyleCop.AppendChild($targetErrorCondition)
      $notUsed = $projectNode.AppendChild($targetStyleCop)
    }

    EnsureXmlAttributeExists "Condition" "Exists('$styleCopImportPackageTargetPath')" $importStyleCopProject  

    $nfProjContent.Save($nfProj);
    UpdatePackagesConfig $nfProj
    Cleanup $nfProj
  }
}

function isProjectWhitelisted($deviceName) {
  #If no project in array, then accept all
  if ($projectWhiteList.Length -eq 0){
    return $true
  }

  if ($deviceName -in $projectWhiteList) {
    return $true
  }

  return $false
}

foreach ($deviceFolder in $allDevicesFolders) {
  if (isProjectWhitelisted $deviceFolder.Name -eq $true) {
    Write-Host "Checking " $deviceFolder.FullName -ForegroundColor Green
    SyncStyleCopSettings $deviceFolder.FullName
    EnsureNfProjHasStyleCopSettings $deviceFolder.FullName
  }
}