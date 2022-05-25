function Expand-Tar($tarFile, $dest) {

     if (-not (Get-Command Expand-7Zip -ErrorAction Ignore)) {
         Install-Module -Name 7Zip4Powershell -RequiredVersion 2.0.0 -Force
     }
     
     Expand-7Zip $tarFile $dest
}
