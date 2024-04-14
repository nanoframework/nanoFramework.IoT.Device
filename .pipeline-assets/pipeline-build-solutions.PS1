# Copyright (c) .NET Foundation and Contributors
# See LICENSE file in the project root for full license information.

# This PS checks what binding need to be build in a PR or regular commit and takes care of performing the various checks and build the solution

# setup msbuild
$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
$msbuild = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -property installationPath
if ($msbuild)
{
    $msbuild = join-path $msbuild 'MSBuild\Current\Bin\amd64\MSBuild.exe'
}

$auth = "basic $([System.Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes("nfbot:${env:MY_GITHUBTOKEN}"))))"

if($env:DEVICE_TO_BUILD -eq "")
{
    if($null -ne $env:System_PullRequest_PullRequestId)
    {
        "" | Write-Host -ForegroundColor Yellow
        "**********************" | Write-Host -ForegroundColor Yellow
        "* Building from a PR *" | Write-host -ForegroundColor Yellow
        "**********************" | Write-Host -ForegroundColor Yellow
        "" | Write-Host -ForegroundColor Yellow

        # get files changed in PR
        $pageCounter = 1

        do
        {
            "##[debug] INFO: iteration $pageCounter" | Write-Host

            $batch = Invoke-RestMethod -Uri "https://api.github.com/repos/nanoframework/nanoFramework.IoT.Device/pulls/$env:System_PullRequest_PullRequestNumber/files?per_page=100&page=$pageCounter" -Header @{"Authorization"="$auth"} -ContentType "application/json" -Method GET

            if($null -eq $commit)
            {
                $commit = $batch
            }
            else
            {
                $commit += $batch
            }
            
            $pageCounter++

        } until ($batch.Count -eq 0)

        # filter removed files              
        $files = $commit.where{$_.status -ne 'removed'}
    }
    else
    {
        "" | Write-Host -ForegroundColor Yellow
        "*****************************************************************" | Write-Host -ForegroundColor Yellow
        "* Building from commit $env:Build_SourceVersion *" | Write-host -ForegroundColor Yellow
        "*****************************************************************" | Write-Host -ForegroundColor Yellow
        "" | Write-Host -ForegroundColor Yellow

        # get files changed in the commit
        $pageCounter = 1

        do
        {
            "##[command] Get API file change page: $pageCounter" | Write-Host

            $batch = Invoke-RestMethod -Uri "https://api.github.com/repos/nanoframework/nanoFramework.IoT.Device/commits/$env:Build_SourceVersion`?per_page=100&page=$pageCounter" -Header @{"Authorization"="$auth"} -ContentType "application/json" -Method GET

            if($null -eq $commit)
            {
                $commit = $batch.files
            }
            else
            {
                $commit += $batch.files
            }
            
            $pageCounter++

        } until ($batch.files.Count -eq 0)

        # filter removed files              
        $files = $commit.where{$_.status -ne 'removed' -and $_.filename -match '(devices)'}
    }

    # get file names only (filter out samples folders)
    $files1 = $files | ForEach-Object {$_.filename} | Where-Object {$_ -match '^((?!samples).)*$'} 

    if($null -eq $files1)
    {
        Write-host "No 'bindings' to build"
        exit
    }

    Write-host "##[group] Files changed:"
    $files1 | ForEach-Object { Write-host $_ }
    Write-host "##[endgroup]"

    # pattern to select device folder name
    $pattern = '(devices\/)(?<folder>[a-zA-Z0-9._]+)(?>\/)'

    # filter out the collection
    $results = [Regex]::Matches($files1, $pattern)

    # get unique folder names
    $deviceFolders = $results | Sort-Object | Select-Object | Foreach-Object {$_.Groups["folder"].Value} | Get-Unique 

    Write-Host "Found $($deviceFolders.count) package(s) to build"
}
elseif($env:DEVICE_TO_BUILD -eq "ALL")
{
    Write-Host "Building 'all' packages"

    $files = (Get-ChildItem -Path "$env:BUILD_SOURCESDIRECTORY\nanoFramework.IoT.Device\devices\" -Include "*.sln"  -Recurse)
    $deviceFolders = $files | Foreach-Object {$_.Directory.Name}
}
else
{
    # build for a specific device

    $deviceFolders = New-Object System.Collections.ArrayList
    $deviceFolders.Add("$env:DEVICE_TO_BUILD")
}

# flag for any build 'failure'
$buildFailed = $false


write-host "##[section] Processing devices..."
foreach ($folder in $deviceFolders)
{
    "" | Write-Host -ForegroundColor Yellow
    "***********************" | Write-Host -ForegroundColor Yellow
    "##[group] Processing binding '$folder'..." | Write-Host -ForegroundColor Yellow
    "***********************" | Write-Host -ForegroundColor Yellow
    "" | Write-Host -ForegroundColor Yellow
    
    # try to find the solution
    $solutionFile = Get-ChildItem -Path "$env:BUILD_SOURCESDIRECTORY\nanoFramework.IoT.Device\devices\$folder\" -Include "$folder.sln" -Recurse

    if($null -eq $solutionFile)
    {
        "" | Write-Host -ForegroundColor Red
        "*****************************************" | Write-Host -ForegroundColor Red
        "##[error] >>>ERROR: Couldn't find any solution files!"  | Write-Host -ForegroundColor Red
        throw "Couldn't find the solution for '$folder'..."
    }
    else
    {
        "" | Write-Host -ForegroundColor Yellow
        "##[command] INFO: Processing '$solutionFile'" | Write-Host -ForegroundColor Yellow
        "" | Write-Host -ForegroundColor Yellow
    }

    # try to find the nuspec
    $nuspecFile = Get-ChildItem -Path "$env:BUILD_SOURCESDIRECTORY\nanoFramework.IoT.Device\devices\$folder\" -Include "$folder.nuspec" -Recurse

    if($null -eq $nuspecFile)
    {
        "" | Write-Host -ForegroundColor Red
        "*****************************************" | Write-Host -ForegroundColor Red
        "##[error] >>>ERROR: Couldn't find the nuspec file!"  | Write-Host -ForegroundColor Red
        throw "Couldn't find the nuspec for '$folder'..."
    }
    else
    {
        "##[command] INFO: nuspec file is: '$nuspecFile'" | Write-Host -ForegroundColor Yellow
    }

    # need to restore NuGets first
    write-host "##[command] Performing nuget restore for '$solutionFile'."
    nuget restore $solutionFile
    if (-not $?)
    { 
        "" | Write-Host -ForegroundColor Red
        "*****************************************" | Write-Host -ForegroundColor Red
        "##[error] >>>ERROR: Couldn't restore solution: '$solutionFile'!"  | Write-Host -ForegroundColor Red
        # throw "Error restoring '$solutionFile'"
	# set flag
        $buildFailed = $true    
    }

    #################################################################
    ### Some bindings reference other bindings or helper projects ###
    ### These need to be restored otherwise the build will fail   ###
    #################################################################

    # NFC solutions require the projects on Card solution to be restored too
    if ($solutionFile.Name -eq "Mfrc522.sln" -or
        $solutionFile.Name -eq "Pn5180.sln" -or 
        $solutionFile.Name -eq "Pn532.sln")
    {
        nuget restore $env:BUILD_SOURCESDIRECTORY\nanoFramework.IoT.Device\devices\Card\Card.sln
    }

    # Mpu9250 solutions requires Ak8963
    if ($solutionFile.Name -eq "Mpu9250.sln")
    {
        nuget restore $env:BUILD_SOURCESDIRECTORY\nanoFramework.IoT.Device\devices\Ak8963\Ak8963.sln
    }

    ### done handling references to other bindings or helper libs ###
    #################################################################

    try
    {
        Clear-Variable LastExitCode

        "##[command] INFO: Performing versions check..." | Write-Host 

        ########################
        # perform versions check

        if($null -ne $env:System_PullRequest_PullRequestId)
        {
            # this is a PR build, perform analysis on nuspec dependencies
            write-host "##[command] Running nanovc for PR (analyse nuspec)."
            nanovc --working-directory $env:BUILD_SOURCESDIRECTORY\nanoFramework.IoT.Device\devices\$folder --solution-to-check $solutionFile --nuspec-file $nuspecFile --analyse-nuspec
        }
        else
        {
            write-host "##[command] Running nanovc."
            nanovc --working-directory $env:BUILD_SOURCESDIRECTORY\nanoFramework.IoT.Device\devices\$folder --solution-to-check $solutionFile --nuspec-file $nuspecFile
        }

        if($LASTEXITCODE -eq 0)
        {
            "" | Write-Host -ForegroundColor Yellow
            "##[command] INFO: Building '$solutionFile'" | Write-Host -ForegroundColor Yellow
            "" | Write-Host -ForegroundColor Yellow

            #############
            # get version
            $nbgvArgs = @('get-version', '-p', "$env:BUILD_SOURCESDIRECTORY\nanoFramework.IoT.Device\devices\$folder", '-f', 'json')
            $versionData = & 'nbgv' $nbgvArgs | ConvertFrom-Json

            $versionReplacement = "-g"+$versionData.GitCommitIdShort
            $MyNuGetVersion = $versionData.NuGetPackageVersion -replace $versionReplacement, ""

            # replace preview with alpha if this is a PR build
            if($null -ne $env:System_PullRequest_PullRequestId)
            {
                # mix PR and VersionHeight
                $alphaNumber = [int]$env:System_PullRequest_PullRequestNumber + $versionData.VersionHeight;
        
                $alphaPrString = "-alpha." + $alphaNumber.ToString()
                $MyNuGetVersion = $MyNuGetVersion+$alphaPrString
            }
        
            Write-Host "##[debug] NuGet build is: $MyNuGetVersion"
            Write-Host "##[debug] Assembly version is: "+$versionData.AssemblyVersion

            ################
            # build solution
            write-host "##[command] Running msbuild."
            & "$msbuild" "$solutionFile" /verbosity:normal /p:Configuration=Release /p:Platform="Any CPU" /p:PublicRelease=true
            if (-not $?) { throw "Error building '$solutionFile'" }

            ###############
            # package NuGet
            write-host "##[command] Running nuget pack."
            nuget pack $nuspecFile -Version $MyNuGetVersion -Symbols -SymbolPackageFormat snupkg -properties commit="$Build_SourceVersion" -OutputDirectory $env:Agent_TempDirectory
            if (-not $?) { throw "Error packaging '$nuspecFile'" }
        }
        else
        {
            "" | Write-Host -ForegroundColor Red
            "*****************************************" | Write-Host -ForegroundColor Red
            "##[error] >>>ERROR: build failed, check output <<<" | Write-Host -ForegroundColor Red
            "*****************************************" | Write-Host -ForegroundColor Red
            "" | Write-Host -ForegroundColor Red
            "Exception was: $_" | Write-Host -ForegroundColor Red
            "" | Write-Host -ForegroundColor Red

            # set flag
            $buildFailed = $true                  
        }
    }
    catch
    {
        "" | Write-Host -ForegroundColor Red
        "*****************************************" | Write-Host -ForegroundColor Red
        "##[error] >>>ERROR: build failed, check output <<<" | Write-Host -ForegroundColor Red
        "*****************************************" | Write-Host -ForegroundColor Red
        "" | Write-Host -ForegroundColor Red

        "" | Write-Host -ForegroundColor Red
        "##[error] Thown Exception was: $_" | Write-Host -ForegroundColor Red
        "" | Write-Host -ForegroundColor Red

        # set flag
        $buildFailed = $true
    }
    write-host "##[endgroup]"
}

if($buildFailed)
{
    "********************************" | Write-Host -ForegroundColor Red
    "##[error] Check >>>ERROR<<< messages above" | Write-Host -ForegroundColor Red
    "********************************" | Write-Host -ForegroundColor Red

    exit 1
}
