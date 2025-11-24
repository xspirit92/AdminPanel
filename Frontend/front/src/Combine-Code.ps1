param(
    [string]$ProjectPath = ".",
    [string]$OutputFile = "AllCode.txt"
)

$outputPath = Join-Path (Get-Location) $OutputFile
$searchPath = Resolve-Path $ProjectPath

Write-Host "Searching for .html and .ts files in: $searchPath"
Write-Host "Output file: $outputPath"

# Remove old file if exists
if (Test-Path $outputPath) {
    Remove-Item $outputPath
}

# Find all .html and .ts files
$codeFiles = Get-ChildItem -Path $searchPath -Recurse -Include "*.html", "*.ts" | 
           Where-Object { 
               $_.FullName -notlike "*\bin\*" -and 
               $_.FullName -notlike "*\obj\*" -and
               $_.FullName -notlike "*\node_modules\*" -and
               $_.FullName -notlike "*\dist\*" -and
               $_.FullName -notlike "*\build\*"
           }

Write-Host "Found files: $($codeFiles.Count)"

foreach ($file in $codeFiles) {
    Write-Host "Processing: $($file.Name)"
    
    # Add separator with filename
    "`r`n" + "=" * 80 | Add-Content -Path $outputPath
    "FILE: $($file.FullName)" | Add-Content -Path $outputPath
    "=" * 80 + "`r`n" | Add-Content -Path $outputPath
    
    # Add file content
    Get-Content $file.FullName -Encoding UTF8 | Add-Content -Path $outputPath
}

Write-Host "Done! All files combined in: $outputPath"