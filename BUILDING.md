# Building DisCatSharp
These are detailed instructions on how to build the DisCatSharp library under various environmnets.

It is recommended you have prior experience with multi-target .NET Core/Standard projects, as well as the `dotnet` CLI utility, and MSBuild.

## Requirements
In order to build the library, you will first need to install some software.

### Windows
On Windows, we only officially support Visual Studio 2019 16.10 or newer. Visual Studio Code and other IDEs might work, but are generally not supported or even guaranteed to work properly.

* **Windows 10** - while we support running the library on Windows 7 and above, we only support building on Windows 10 and better.
* [**Git for Windows**](https://git-scm.com/download/win) - required to clone the repository.
* [**Visual Studio 2022**](https://www.visualstudio.com/downloads/) - community edition or better. We do not support Visual Studio 2021 and older.
* **Windows PowerShell** - required to run the build scripts. You need to make sure your script execution policy allows execution of unsigned scripts.
* [**.NET SDK 7.0**](https://www.microsoft.com/net/download) - required to build the project.

### GNU/Linux
On GNU/Linux, we support building via Visual Studio Code and .NET Core SDK. Other IDEs might work, but are not supported or guaranteed to work properly.

While these should apply to any modern distribution, we only test against Debian 10. Your mileage may vary.

When installing the below, make sure you install all the dependencies properly. We might ship a build environment as a docker container in the future.

* **Any modern GNU/Linux distribution** - like Debian 10.
* **Git** - to clone the repository.
* [**Visual Studio Code**](https://code.visualstudio.com/Download) - a recent version is required.
   * **C# for Visual Studio Code (powered by OmniSharp)** - required for syntax highlighting and basic Intellisense
* [**.NET SDK 7.0**](https://www.microsoft.com/net/download) - required to build the project.
* [**Mono 5.x**](http://www.mono-project.com/download/#download-lin) - required to build the .NETFX 4.5, 4.6, and 4.7 targets, as well as to build the docs.
* [**PowerShell Core**](https://docs.microsoft.com/en-us/powershell/scripting/install/installing-powershell-core-on-linux?view=powershell-7.3) - required to execute the build scripts.

## Instructions
Once you install all the necessary prerequisites, you can proceed to building. These instructions assume you have already cloned the repository.

### Windows
Building on Windows is relatively easy. There's 2 ways to build the project:

#### Building through Visual Studio
Building through Visual Studio yields just binaries you can use in your projects.

1. Open the solution in Visual Studio.
2. Set the configuration to Release.
3. Select Build > Build Solution to build the project.
4. Select Build > Publish DisCatSharp to publish the binaries.

#### Building with the build script
Building this way outputs NuGet packages, and a documentation package. Ensure you have an internet connection available, as the script will install programs necessary to build the documentation.

1. Open PowerShell and navigate to the directory which you cloned DisCatSharp to.
2. Execute `.\DisCatSharp.Tools\rebuild-lib.ps1 -ArtifactLocation .\dcs-artifacts -Configuration Release` and wait for the script to finish execution.
3. Once it's done, the artifacts will be available in *dcs-artifacts* directory, next to the directory to which the repository is cloned.

### GNU/Linux
When all necessary prerequisites are installed, you can proceed to building. There are technically 2 ways to build the library, though both of them perform the same steps, they are just invoked slightly differently.

#### Through Visual Studio Code
1. Open Visual Studio Code and open the folder to which you cloned DisCatSharp as your workspace.
2. Select Build > Run Task...
3. Select `buildRelease` task and wait for it to finish.
4. The artifacts will be placed in *dcs-artifacts* directory, next to where the repository is cloned.

#### Through PowerShell
1. Open PowerShell (`pwsh`) and navigate to the directory which you cloned DisCatSharp to.
2. Execute `./DisCatSharp.Tools/rebuild-lib.ps1 -ArtifactLocation ./dcs-artifacts -Configuration Release` and wait for the script to finish execution.
3. Once it's done, the artifacts will be available in *dcs-artifacts* directory, next to the directory to which the repository is cloned.
