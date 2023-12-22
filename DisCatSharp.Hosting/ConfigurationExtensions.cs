using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using DisCatSharp.Configuration;
using DisCatSharp.Configuration.Models;

using Microsoft.Extensions.Configuration;

namespace DisCatSharp.Hosting;

internal struct ExtensionConfigResult
{
	/// <summary>
	/// Gets or sets the section.
	/// </summary>
	public ConfigSection? Section { get; set; }

	/// <summary>
	/// Gets or sets the config type.
	/// </summary>
	public Type ConfigType { get; set; }

	/// <summary>
	/// Gets or sets the implementation type.
	/// </summary>
	public Type ImplementationType { get; set; }
}

/// <summary>
/// The configuration extensions.
/// </summary>
internal static class ConfigurationExtensions
{
	/// <summary>
	/// Find assemblies that match the names provided via <paramref name="names"/>.
	/// </summary>
	/// <remarks>
	/// In some cases the assembly the user is after could be used in the application but
	/// not appear within the <see cref="AppDomain"/>. <br/>
	/// The workaround for this is to check the assemblies in the <see cref="AppDomain"/>, as well as referenced
	/// assemblies. If the targeted assembly is a reference, we need to load it into our workspace to get more info.
	/// </remarks>
	/// <param name="names">Names of assemblies to look for</param>
	/// <returns>Assemblies which meet the given names. No duplicates</returns>
	public static List<Assembly> FindAssemblies(IEnumerable<string>? names)
	{
		/*
              There is a possibility that an assembly can be referenced in multiple assemblies.
              To alleviate duplicates we need to shrink our queue as we find things
             */
		List<Assembly> results = [];

		if (names is null)
			return results;

		var queue = names.ToList();
		foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
		{
			if (queue.Count == 0)
				break;

			var loadedAssemblyName = assembly.GetName().Name;

			// Kinda need the name to do our thing
			if (loadedAssemblyName == null)
				continue;

			// Is this something we're looking for?
			if (queue.Remove(loadedAssemblyName))
				results.Add(assembly);

			// Time to check if one of the referenced assemblies is something we're looking for
			foreach (var referencedAssembly in assembly.GetReferencedAssemblies()
				.Where(x => x.Name != null && queue.Contains(x.Name)))
				try
				{
					// Must load the assembly into our workspace so we can do stuff with it later
					results.Add(Assembly.Load(referencedAssembly));
					queue.Remove(referencedAssembly.Name);
				}
				catch (Exception ex)
				{
					Console.Error.WriteLine($"Unable to load referenced assembly: '{referencedAssembly.Name}' \n\t{ex.Message}");
				}
		}

		return results;
	}

	/// <summary>
	/// Easily identify which configuration types have been added to the <paramref name="configuration"/> <br/>
	/// This way we can dynamically load extensions without explicitly doing so
	/// </summary>
	/// <param name="configuration"></param>
	/// <param name="rootName"></param>
	/// <returns>Dictionary where Key -> Name of implemented type<br/>Value -> <see cref="ExtensionConfigResult"/></returns>
	public static Dictionary<string, ExtensionConfigResult> FindImplementedExtensions(
		this IConfiguration configuration,
		string rootName = Configuration.ConfigurationExtensions.DEFAULT_ROOT_LIB
	)
	{
		if (string.IsNullOrEmpty(rootName))
			throw new ArgumentNullException(nameof(rootName), "Root name must be provided");

		Dictionary<string, ExtensionConfigResult> results = [];
		string[]? assemblyNames;

		// Has the user defined a using section within the root name?
		if (!configuration.HasSection(rootName, "Using"))
			return results;

		/*
               There are 2 ways a user could list which assemblies are used
               "Using": "[\"Assembly.Name\"]"
               "Using": ["Assembly.Name"]

               JSON or as Text.
             */
		assemblyNames = string.IsNullOrEmpty(configuration[configuration.ConfigPath(rootName, "Using")])
			? configuration.GetSection(configuration.ConfigPath(rootName, "Using")).Get<string[]>()
			: Newtonsoft.Json.JsonConvert.DeserializeObject<string[]>(
				configuration[configuration.ConfigPath(rootName, "Using")]);
		foreach (var assembly in FindAssemblies(assemblyNames.Select(x => x.StartsWith(Constants.LibName, StringComparison.Ordinal) ? x : $"{Constants.LibName}.{x}")))
		{
			ExtensionConfigResult result = new();

			foreach (var type in assembly.ExportedTypes
				.Where(x => x.Name.EndsWith(Constants.ConfigSuffix, StringComparison.Ordinal) && x is { IsAbstract: false, IsInterface: false }))
			{
				var sectionName = type.Name;
				var prefix = type.Name.Replace(Constants.ConfigSuffix, "");

				result.ConfigType = type;

				// Does a section exist with the classname? (DiscordConfiguration - for instance)
				if (configuration.HasSection(rootName, sectionName))
					result.Section = new ConfigSection(ref configuration, type.Name, rootName);

				// Does a section exist with the classname minus Configuration? (Discord - for Instance)
				else if (configuration.HasSection(rootName, prefix))
					result.Section = new ConfigSection(ref configuration, prefix, rootName);

				// IF THE SECTION IS NOT PROVIDED --> WE WILL USE DEFAULT CONFIG IMPLEMENTATION

				/*
                        Now we need to find the type which should consume our config

                        In the event a user has some "fluff" between prefix and suffix we'll
                        just check for beginning and ending values.

                        Type should not be an interface or abstract, should also be assignable to BaseExtension
                     */

				var implementationType = assembly.ExportedTypes.FirstOrDefault(x =>
					x is { IsAbstract: false, IsInterface: false } && x.Name.StartsWith(prefix, StringComparison.Ordinal) &&
					x.Name.EndsWith(Constants.ExtensionSuffix, StringComparison.Ordinal) && x.IsAssignableTo(typeof(BaseExtension)));

				// If the implementation type was found we can add it to our result set
				if (implementationType != null)
				{
					result.ImplementationType = implementationType;
					results.Add(implementationType.Name, result);
				}
			}
		}

		return results;
	}
}
