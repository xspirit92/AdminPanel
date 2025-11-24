param(
    [string]$ProjectPath = ".",
    [string]$OutputFile = "AllCode.txt"
)

$outputPath = Join-Path (Get-Location) $OutputFile
$searchPath = Resolve-Path $ProjectPath

Write-Host "Searching for .cs files in: $searchPath"
Write-Host "Output file: $outputPath"

# Remove old file if exists
if (Test-Path $outputPath) {
    Remove-Item $outputPath
}

# Find all .cs files
$csFiles = Get-ChildItem -Path $searchPath -Recurse -Filter "*.cs" | 
           Where-Object { 
               $_.FullName -notlike "*\bin\*" -and 
               $_.FullName -notlike "*\obj\*" -and
               $_.FullName -notlike "*\CubArt.Migrator\*"
           }


Write-Host "Found files: $($csFiles.Count)"

foreach ($file in $csFiles) {
    Write-Host "Processing: $($file.Name)"
    
    # Add separator with filename
    "`r`n" + "=" * 80 | Add-Content -Path $outputPath
    "FILE: $($file.FullName)" | Add-Content -Path $outputPath
    "=" * 80 + "`r`n" | Add-Content -Path $outputPath
    
    # Add file content
    Get-Content $file.FullName -Encoding UTF8 | Add-Content -Path $outputPath
}

Write-Host "Done! All files combined in: $outputPath"