// This file is part of the DisCatSharp project, based off DSharpPlus.
//
// Copyright (c) 2021-2023 AITSYS
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DisCatSharp.Common.Utilities;

/// <summary>
/// Gets information about current runtime.
/// </summary>
public static class RuntimeInformation
{
	/// <summary>
	/// Gets the current runtime's version.
	/// </summary>
	public static string Version { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="RuntimeInformation"/> class.
	/// </summary>
	static RuntimeInformation()
	{
		var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
		var mscorlib = loadedAssemblies.Select(x => new { Assembly = x, AssemblyName = x.GetName() })
			.FirstOrDefault(x => x.AssemblyName.Name == "mscorlib" || x.AssemblyName.Name == "System.Private.CoreLib");

		var location = mscorlib.Assembly.Location;
		var assemblyFile = new FileInfo(location);
		var versionFile = new FileInfo(Path.Combine(assemblyFile.Directory.FullName, ".version"));
		if (versionFile.Exists)
		{
			var lines = File.ReadAllLines(versionFile.FullName, new UTF8Encoding(false));

			if (lines.Length >= 2)
			{
				Version = lines[1];
				return;
			}
		}

		var infVersion = mscorlib.Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
		if (infVersion != null)
		{
			var infVersionString = infVersion.InformationalVersion;
			if (!string.IsNullOrWhiteSpace(infVersionString))
			{
				Version = infVersionString.Split(' ').First();
				return;
			}
		}

		Version = mscorlib.AssemblyName.Version.ToString();
	}
}
