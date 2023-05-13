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
    # Exit back
    Set-Location -path "$current_location"
}

PackDocs "$DocsPath" "$OutputPath" "$PackageName"
