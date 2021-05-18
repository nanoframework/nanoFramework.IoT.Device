

$files = Get-ChildItem -Recurse -Include *.sln 

$results = [System.Collections.ArrayList]::new()
$success = [System.Collections.ArrayList]::new()
$failures = [System.Collections.ArrayList]::new()

foreach ($sln in $files) {
    & 'C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\MSBuild.exe' $sln -t:restore -p:RestorePackagesConfig=true -verbosity:quiet
    $output = [string](& 'C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\MSBuild.exe' $sln -t:build -verbosity:quiet)
    if ($LASTEXITCODE -eq 0) {
        write-host "$sln - SUCCESSFUL BUILD"
        $success.Add($sln)
    } else {
        $failures.Add($sln.Name)
        $option = [System.StringSplitOptions]::RemoveEmptyEntries
        $seperator = [string[]]": error"
        $errors = $output.Split($seperator, $option)
        for ($j=1; $j -lt $errors.length; $j++){
            try {
                #write-host $"error=$($errors[$j])"
                $matches = (select-string "([^[]*).*devices_generated.?\\([^\\[]*)" -InputObject $errors[$j]).Matches
                $results.Add(@{ Error = $matches.groups[1].value; Project = $matches.groups[2].value })
            } catch {
                write-host "error caught!!!!!" #+ $error
            }
        }

        write-host "$sln - FAILED BUILD ($($errors.Count - 1) ERRORS)"
        # write-host $errors
    }
}
write-host ""
write-host $"Successful builds:"
write-host $success
write-host ""
write-host $"Failed builds:"
write-host $failures

write-host ""
write-host $"ERRORS:"
$results | Group-object {$_.Error } -noelement | sort-object Count -Descending | format-table -autosize
write-host ""
write-host $"PROJECTS:"
$results | Group-object {$_.Project } -noelement | sort-object Count -Descending | format-table -autosize
#write-host $groupedResults
#$results[0]

#return $results