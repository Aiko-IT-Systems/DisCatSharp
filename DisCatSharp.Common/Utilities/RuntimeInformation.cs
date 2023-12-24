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
	public static string? Version { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="RuntimeInformation"/> class.
	/// </summary>
	static RuntimeInformation()
	{
		var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
		var mscorlib = loadedAssemblies.Select(x => new
			{
				Assembly = x,
				AssemblyName = x.GetName()
			})
			.FirstOrDefault(x => x.AssemblyName.Name is "mscorlib" or "System.Private.CoreLib");

		if (mscorlib is null)
			return;

		var location = mscorlib.Assembly.Location;
		var assemblyFile = new FileInfo(location);
		if (assemblyFile.Directory is not null)
		{
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
		}

		var infVersion = mscorlib.Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
		if (infVersion is not null)
		{
			var infVersionString = infVersion.InformationalVersion;
			if (!string.IsNullOrWhiteSpace(infVersionString))
			{
				Version = infVersionString.Split(' ').First();
				return;
			}
		}

		Version = mscorlib.AssemblyName.Version?.ToString();
	}
}
