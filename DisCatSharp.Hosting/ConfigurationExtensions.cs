// This file is part of the DisCatSharp project, based off DSharpPlus.
//
// Copyright (c) 2021-2022 AITSYS
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
	public static Assembly[] FindAssemblies(string[]? names)
	{

		/*
              There is a possibility that an assembly can be referenced in multiple assemblies.
              To alleviate duplicates we need to shrink our queue as we find things
             */
		List<Assembly> results = new();

		if (names is null)
			return results.ToArray();

		List<string> queue = new(names);
		foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
		{
			if (!queue.Any())
				break;

			var loadedAssemblyName = assembly.GetName().Name;

			// Kinda need the name to do our thing
			if (loadedAssemblyName == null)
				continue;

			// Is this something we're looking for?
			if (queue.Contains(loadedAssemblyName))
			{
				results.Add(assembly);

				// Shrink queue so we don't accidentally add the same assembly > 1 times
				queue.Remove(loadedAssemblyName);
			}

			// Time to check if one of the referenced assemblies is something we're looking for
			foreach (var referencedAssembly in assembly.GetReferencedAssemblies()
													  .Where(x => x.Name != null && queue.Contains(x.Name)))
				try
				{
					// Must load the assembly into our workspace so we can do stuff with it later
					results.Add(Assembly.Load(referencedAssembly));

#pragma warning disable 8604
					queue.Remove(referencedAssembly.Name);
#pragma warning restore 8604
				}
				catch (Exception ex)
				{
					Console.Error.WriteLine($"Unable to load referenced assembly: '{referencedAssembly.Name}' \n\t{ex.Message}");
				}
		}

		return results.ToArray();
	}

	/// <summary>
	/// Easily identify which configuration types have been added to the <paramref name="configuration"/> <br/>
	/// This way we can dynamically load extensions without explicitly doing so
	/// </summary>
	/// <param name="configuration"></param>
	/// <param name="rootName"></param>
	/// <returns>Dictionary where Key -> Name of implemented type<br/>Value -> <see cref="ExtensionConfigResult"/></returns>
	public static Dictionary<string, ExtensionConfigResult> FindImplementedExtensions(this IConfiguration configuration,
		string rootName = Configuration.ConfigurationExtensions.DEFAULT_ROOT_LIB)
	{
		if (string.IsNullOrEmpty(rootName))
			throw new ArgumentNullException(nameof(rootName), "Root name must be provided");

		Dictionary<string, ExtensionConfigResult> results = new();
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

#pragma warning disable 8604
		foreach (var assembly in FindAssemblies(assemblyNames.Select(x => x.StartsWith(Constants.LibName) ? x : $"{Constants.LibName}.{x}").ToArray()))
		{
			ExtensionConfigResult result = new();

			foreach (var type in assembly.ExportedTypes
				.Where(x => x.Name.EndsWith(Constants.ConfigSuffix) && !x.IsAbstract && !x.IsInterface))
			{
				string sectionName = type.Name;
				string prefix = type.Name.Replace(Constants.ConfigSuffix, "");

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
					!x.IsAbstract && !x.IsInterface && x.Name.StartsWith(prefix) &&
					x.Name.EndsWith(Constants.ExtensionSuffix) && x.IsAssignableTo(typeof(BaseExtension)));

				// If the implementation type was found we can add it to our result set
				if (implementationType != null)
				{
					result.ImplementationType = implementationType;
					results.Add(implementationType.Name, result);
				}
			}
		}
#pragma warning restore 8604

		return results;
	}

}
