---
uid: misc_nightly_builds
title: Nightly Builds
---

# I like living on the edge - give me the freshest builds

We offer nightly builds for DisCatSharp. They contain bugfixes and new features before the NuGet releases, however they are
not guaranteed to be stable, or work at all.
Open the NuGet interface for your project, check **Prerelease**.

Then just select **Latest prerelease** version of DisCatSharp packages, and install them.

You might need to restart Visual Studio for changes to take effect.

If you find any problems in the SlimGet versions of the packages, please follow the instructions in [Reporting issues](xref:misc_reporting_issues)
article.

## But I'm running GNU/Linux, Mac OS X, or BSD!

If you're running on a non-Windows OS, you'll have to get your hands dirty. Prepare your text editor and file browser.

Run `dotnet restore`, it should be able to restore the packages without problems.
