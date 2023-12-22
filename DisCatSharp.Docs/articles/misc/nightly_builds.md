---
uid: misc_nightly_builds
title: Nightly Builds
author: DisCatSharp Team
---

# Do you have nightly builds?

We offer nightly builds for DisCatSharp. They contain bugfixes and new features before the official NuGet releases, however they are
not guaranteed to be stable, or work at all.
Open the NuGet interface for your project, check **Prerelease**.

Then just select **Latest prerelease** version of DisCatSharp packages, and install them.

You might need to restart Visual Studio for changes to take effect.

If you find any problems in the nightly versions of the packages, please follow the instructions in [Reporting issues](xref:misc_reporting_issues)
article.

# But I'm running GNU/Linux, Mac OS X, or BSD!

If you're running on a non-Windows OS, you'll have to get your hands dirty. Prepare your text editor and file browser.

Run `dotnet restore`, it should be able to restore the packages without problems.

# Do you have direct builds from the main branch?

Yes we do!

To use them, follow these instructions:

```bash
# Register Registry
nuget sources add -Name discatsharp-git-releases -Source https://registry.aitsys-infra.tools/nuget/discatsharp-git-releases/index.json -Username bytesafe -Password 01HJ80HC4S65ADXD4H5SANV23E

# Restore from registry
nuget restore -Source discatsharp-git-releases

# Disable Registry
nuget sources disable -Name discatsharp-git-releases
```

These releases are considered to not be stable at all. Use them with care.

And before you ask, the token is public 😉 We created a read-only account just for this registry.
