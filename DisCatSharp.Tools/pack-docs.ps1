param
(
    [parameter(Mandatory = $true)]
    [string] $DocsPath,
    [parameter(Mandatory = $true)]
    [string] $OutputPath,
    [parameter(Mandatory = $true)]
    [string] $PackageName
)
# Backup the environment
$current_location = Get-Location

function PackDocs([string] $target_dir_path, [string] $output_dir_path, [string] $pack_name)
{
    # Form target path
    $target_path = Get-Item "$target_dir_path"
    $target_path = $target_path.FullName
    $target_path = Join-Path "$target_path" "_site"
    # Form output path
    $output_path_dir = Get-Item "$output_dir_path"
    $output_path_dir = $output_path_dir.FullName
    $output_path = Join-Path "$output_path_dir" "$pack_name"
    # Enter target path
    Set-Location -Path "$target_path"

    # Check if target .zip exists and remove it
    if (Test-Path "$output_path.zip")
    {
        Write-Host "$output_path.zip exists, deleting"
        Remove-Item "$output_path.zip" -Force
    }

    # Package .zip archive (cross-platform)
    Write-Host "Packing $output_path.zip"

    # Diagnostics (helpful in CI logs)
    Write-Host "Diagnostics: PowerShell version: $($PSVersionTable.PSVersion.ToString())"
    Write-Host "Diagnostics: Checking for Compress-Archive and zip CLI availability..."
    $hasCompress = $false
    $hasZipCLI = $false
    if (Get-Command Compress-Archive -ErrorAction SilentlyContinue) {
        Write-Host "Found Compress-Archive"
        $hasCompress = $true
    } else {
        Write-Host "Compress-Archive not available"
    }
    if (Get-Command zip -ErrorAction SilentlyContinue) {
        Write-Host "Found zip CLI"
        $hasZipCLI = $true
    } else {
        Write-Host "zip CLI not available"
    }

    # 1) Try Compress-Archive (Windows / full PowerShell)
    if ($hasCompress) {
        try {
            Write-Host "Using Compress-Archive"
            # Compress the contents of the _site directory
            Compress-Archive -Path "$target_path/*" -DestinationPath "$output_path.zip" -Force -CompressionLevel Fastest
            Write-Host "Packing completed with Compress-Archive"
            Set-Location -Path "$current_location"
            return
        }
        catch {
            Write-Warning "Compress-Archive failed: $_"
        }
    }

    # 2) Try zip CLI (common on Linux runners)
    if ($hasZipCLI) {
        try {
            Write-Host "Using zip CLI"
            Push-Location $target_path
            # Create the zip file at the output path (zip -r <zipfile> ./*)
            & zip -r -q "$output_path.zip" ./*
            Pop-Location
            Write-Host "Packing completed with zip CLI"
            Set-Location -Path "$current_location"
            return
        }
        catch {
            Write-Warning "zip CLI failed: $_"
        }
    }

    # 3) .NET fallback using System.IO.Compression
    Write-Host "Using .NET ZipFile fallback"
    try {
        Add-Type -AssemblyName System.IO.Compression.FileSystem -ErrorAction SilentlyContinue
        if (Test-Path "$output_path.zip") { Remove-Item "$output_path.zip" -Force }
        # CreateFromDirectory zips the directory contents. Use the _site directory root.
        [System.IO.Compression.ZipFile]::CreateFromDirectory($target_path, "$output_path.zip")
        Write-Host "Packing completed with .NET ZipFile"
        Set-Location -Path "$current_location"
        return
    }
    catch {
        Write-Error "Failed to create zip archive with any available method. $_"
        Set-Location -Path "$current_location"
        exit 1
    }
}

PackDocs "$DocsPath" "$OutputPath" "$PackageName"
