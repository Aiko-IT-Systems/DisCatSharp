#!/usr/bin/env pwsh
# Rebuild-docs
#
# Rebuilds the documentation for DisCatSharp project, and places artifacts in specified directory.
#
# Author:       Emzi0767
# Version:      2017-09-11 14:20
#
# Arguments:
#   .\rebuild-docs.ps1 <path to docfx> <output path> <docs project path>
#
# Run as:
#   .\rebuild-docs.ps1 .\path\to\docfx\project .\path\to\output project-docs

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
$current_path = $Env:PATH
$current_location = Get-Location

# Tool paths
$docfx_path = Join-Path "$current_location" "docfx"

# Restores the environment
function Restore-Environment()
{
    Write-Host "Restoring environment variables"
    $Env:PATH = $current_path
    Set-Location -path "$current_location"

    if (Test-Path "$docfx_path")
    {
        Remove-Item -recurse -force "$docfx_path"
    }
}

# Downloads and installs latest version of DocFX
function Install-DocFX([string] $target_dir_path)
{
    Write-Host "Installing DocFX"

    # Check if the target directory exists
    # If it does, remove it
    if (Test-Path "$target_dir_path")
    {
        Write-Host "Target directory exists, deleting"
        Remove-Item -recurse -force "$target_dir_path"
    }

    # Create target directory
    $target_dir = New-Item -type directory "$target_dir_path"
    $target_fn = "docfx.zip"

    # Form target path
    $target_dir = $target_dir.FullName
    $target_path = Join-Path "$target_dir" "$target_fn"

    # Download release info from Chocolatey API
    try
    {
        Write-Host "Getting latest DocFX release"
        #$release_json = Invoke-WebRequest -uri "https://chocolatey.org/api/v2/package-versions/docfx" | ConvertFrom-JSON
        #$release_json = $release_json | ForEach-Object { [System.Version]::Parse($_) } | Sort-Object -Descending
    }
    catch
    {
        Return 1
    }

    # Set TLS version to 1.2
    [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

    # Download the release
    # Since GH releases are unreliable, we have to try up to 3 times
    $tries = 0
    $fail = $true
    while ($tries -lt 3)
    {
        # Prepare the assets
        #$release = $release_json[$tries]        # Pick the next available release
        #$release_version = $release.ToString()  # Convert to string
		$release_version = "2.60.0"
        $release_asset = "https://github.com/Aiko-IT-Systems/docfx/releases/download/v$release_version/docfx.zip"
		#$release_asset = "https://github.com/dotnet/docfx/releases/download/v$release_version/docfx.zip"

        # increment try counter
        $tries = $tries + 1

        try
        {
            Write-Host "Downloading DocFX $release_version to $target_path"
            Invoke-WebRequest -uri "$release_asset" -outfile "$target_path"

            # No failure, carry on
            Write-Host "DocFX version $release_version downloaded successfully"
            $fail = $false
            Break
        }
        catch
        {
            Write-Host "Downloading DocFX version $release_version failed, trying next ($tries / 3)"
            #Return 1
        }
    }

    # Check if we succeeded in downloading
    if ($fail)
    {
        Return 1
    }

    # Switch directory
    Set-Location -Path "$target_dir"

    # Extract the release
    try
    {
        Write-Host "Extracting DocFX"
        Expand-Archive -path "$target_path" -destinationpath "$target_dir"
    }
    catch
    {
        Return 1
    }

    # Remove the downloaded zip
    Write-Host "Removing temporary files"
    Remove-Item "$target_path"

    # Add DocFX to PATH
    Write-Host "Adding DocFX to PATH"
    if ($null -eq $Env:OS)
    {
        $Env:DOCFX_PATH = "$target_dir"
    }
    else
    {
        $Env:PATH = "$target_dir;$current_path"
    }
    Set-Location -path "$current_location"

    Return 0
}

# Builds the documentation using available DocFX
function BuildDocs([string] $target_dir_path)
{
    # Check if documentation source path exists
    if (-not (Test-Path "$target_dir_path"))
    {
        #Write-Host "Specified path does not exist"
        Return 65536
    }

    # Check if documentation source path is a directory
    $target_path = Get-Item "$target_dir_path"
    if (-not ($target_path -is [System.IO.DirectoryInfo]))
    {
        #Write-Host "Specified path is not a directory"
        Return 65536
    }

    # Form target path
    $target_path = $target_path.FullName

    # Form component paths
    $docs_site = Join-Path "$target_path" "_site"
    # $libdev_api = Join-Path "$target_path" "libdev"
    $docs_api = Join-Path "$target_path" "api"
    #$docs_obj = Join-Path "$target_path" "obj"

    # Check if API documentation source path exists
    if (-not (Test-Path "$docs_api"))
    {
        #Write-Host "API build target directory does not exist"
        Return 32768
    }

    # Check if API documentation source path is a directory
    $docs_api_dir = Get-Item "$docs_api"
	# $libdev_api_dir = Get-Item "$libdev_api"
    if (-not ($docs_api_dir -is [System.IO.DirectoryInfo]))
    {
        #Write-Host "API build target directory is not a directory"
        Return 32768
    }
	#if (-not ($libdev_api_dir -is [System.IO.DirectoryInfo]))
    #{
    #    #Write-Host "LibDev API build target directory is not a directory"
    #    Return 32768
    #}

    # Purge old API documentation
    Write-Host "Purging old API documentation"
    Set-Location -path "$docs_api"
    Remove-Item "*.yml"
    #Set-Location -path "$libdev_api"
    #Remove-Item "*.yml"
    Set-Location -path "$current_location"

    # Check if old built site exists
    # If it does, remove it
    if (Test-Path "$docs_site")
    {
        Write-Host "Purging old products"
        Remove-Item -recurse -force "$docs_site"
    }

    # Create target directory for the built site
    $docs_site = New-Item -type directory "$docs_site"
    $docs_site = $docs_site.FullName

    # Check if old object cache exists
    # If it does, remove it
    #if (Test-Path "$docs_obj")
    #{
    #    Write-Host "Purging object cache"
    #    Remove-Item -recurse -force "$docs_obj"
    #}

    # Create target directory for the object cache
    #$docs_obj = New-Item -type directory "$docs_obj"
    #$docs_obj = $docs_obj.FullName

    # Enter the documentation directory
    Set-Location -path "$target_path"

	if (Test-Path ".htaccess")
	{
		# Add .htaccess to docs_site
		Copy-Item ".htaccess" "$docs_site/.htaccess"
	}

    # Check OS
    # Null means non-Windows
    if ($null -eq $Env:OS)
    {
        # Generate new API documentation
        & mono "$Env:DOCFX_PATH/docfx.exe" metadata docfx.json | Out-Host

        # Check if successful
        if ($LastExitCode -eq 0)
        {
            # Build new documentation site
            & mono "$Env:DOCFX_PATH/docfx.exe" build docfx.json | Out-Host
        }
    }
    else
    {
        # Generate new API documentation
        & docfx metadata docfx.json | Out-Host

        # Check if successful
        if ($LastExitCode -eq 0)
        {
            # Build new documentation site
            & docfx build docfx.json | Out-Host
        }
    }

    # Exit back
    Set-Location -path "$current_location"

    # Check if building was a success
    if ($LastExitCode -eq 0)
    {
        Return 0
    }
    else
    {
        Return $LastExitCode
    }
}

# Packages the build site to a .zip archive
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
    Set-Location -path "$target_path"

    # Check if target .zip exists
    # If it does, remove it
    if (Test-Path "$output_path.zip")
    {
        Write-Host "$output_path.zip exists, deleting"
        Remove-Item "$output_path.zip"
    }

	# Package .zip archive
	Write-Host "Packing $output_path.zip"
	Compress-Archive -Path "$target_path/*" -DestinationPath "$output_path.zip" -Force -CompressionLevel Fastest
	Write-Host "Adding .htaccess to $output_path.zip"
	try {
		Compress-Archive -Path "$target_path/.htaccess" -Update -DestinationPath "$output_path.zip"
	}
	catch {
		Write-Host "Could not add .htaccess to archive"
	}
    # Exit back
    Set-Location -path "$current_location"

    # Check if packaging was a success
    if ($LastExitCode -eq 0)
    {
        Return 0
    }
    else
    {
        Return $LastExitCode
    }
}

# Install DocFX
$result = Install-DocFX "$docfx_path"
if ($result -ne 0)
{
    Write-Host "Installing DocFX failed"
    Restore-Environment
    $host.SetShouldExit(1)
    Exit 1
}

# Build and package docs
# At this point nothing should fail as everything is already set up
$result = BuildDocs "$DocsPath"
if ($result -eq 0)
{
    $result = PackDocs "$DocsPath" "$OutputPath" "$PackageName"
    if ($result -ne 0)
    {
        Write-Host "Packaging API documentation failed"
    }
}
else
{
    Write-Host "Building API documentation failed"
}

# Restore the environment
Restore-Environment

# All was well, exit with success
if ($result -eq 0)
{
    Write-Host "All operations completed"
    Exit 0
}
else
{
    $host.SetShouldExit($result)
    Exit $result
}
