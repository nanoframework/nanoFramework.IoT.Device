$styleCopSettingsFileName = "Settings.StyleCop";
$devicesMainFolderPath = "..\devices"
$sourceFileContent = Get-Content $styleCopSettingsFileName

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

foreach ($deviceFolder in $allDevicesFolders) {
   
   $pathToSettingInCurrentFolder = $deviceFolder.FullName + '\' + $styleCopSettingsFileName

   if (Test-Path -Path $pathToSettingInCurrentFolder -PathType Leaf) 
   {
      Write-Host "File exists in path " $pathToSettingInCurrentFolder". Checking file content" -ForegroundColor Green
      $contentOfExistingFile = Get-Content $pathToSettingInCurrentFolder
      
      $comapreResult = (Compare-Object $sourceFileContent $contentOfExistingFile)
      if ($comapreResult.Count -eq 0)
      {
        Write-Host "Existing file matches source" -ForegroundColor Green
        continue
      }
   }
   
   Write-Host "Creating/replacing file in " $pathToSettingInCurrentFolder -ForegroundColor Yellow
   Set-Content $pathToSettingInCurrentFolder $sourceFileContent
 }