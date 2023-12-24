using System;
using System.Collections;
using System.Linq;

using DisCatSharp.Configuration.Models;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DisCatSharp.Configuration;

/// <summary>
/// The configuration extensions.
/// </summary>
internal static class ConfigurationExtensions
{
	/// <summary>
	/// The factory error message.
	/// </summary>
	private const string FACTORY_ERROR_MESSAGE = "Require a function which provides a default entity to work with";

	/// <summary>
	/// The default root lib.
	/// </summary>
	public const string DEFAULT_ROOT_LIB = "DisCatSharp";

	/// <summary>
	/// The config suffix.
	/// </summary>
	private const string CONFIG_SUFFIX = "Configuration";

	/// <summary>
	/// Easily piece together paths that will work within <see cref="IConfiguration"/>
	/// </summary>
	/// <param name="config">(not used - only for adding context based functionality)</param>
	/// <param name="values">The strings to piece together</param>
	/// <returns>Strings joined together via ':'</returns>
	public static string ConfigPath(this IConfiguration config, params string[] values) => string.Join(":", values);

	/// <summary>
	/// Skims over the configuration section and only overrides values that are explicitly defined within the config
	/// </summary>
	/// <param name="config">Instance of config</param>
	/// <param name="section">Section which contains values for <paramref name="config"/></param>
	private static void HydrateInstance(ref object config, ConfigSection section)
	{
		var props = config.GetType().GetProperties();

		foreach (var prop in props)
		{
			// Must have a set method for this to work, otherwise continue on
			if (prop.SetMethod == null)
				continue;

			var entry = section.GetValue(prop.Name);
			object? value = null;

			if (typeof(string) == prop.PropertyType)
			{
				// We do NOT want to override value if nothing was provided
				if (!string.IsNullOrEmpty(entry))
					prop.SetValue(config, entry);

				continue;
			}

			// We need to address collections a bit differently
			// They can come in the form of    "root:section:name" with a string representation OR
			// "root:section:name:0"  <--- this is not detectable when checking the above path
			if (typeof(IEnumerable).IsAssignableFrom(prop.PropertyType))
			{
				value = string.IsNullOrEmpty(section.GetValue(prop.Name))
					? section.Config
						.GetSection(section.GetPath(prop.Name)).Get(prop.PropertyType)
					: Newtonsoft.Json.JsonConvert.DeserializeObject(entry, prop.PropertyType);

				if (value == null)
					continue;

				prop.SetValue(config, value);
			}

			// From this point onward we require the 'entry' value to have something useful
			if (string.IsNullOrEmpty(entry))
				continue;

			try
			{
				// Primitive types are simple to convert
				if (prop.PropertyType.IsPrimitive)
					value = Convert.ChangeType(entry, prop.PropertyType);
				else
				{
					// The following types require a different approach
					if (prop.PropertyType.IsEnum)
						value = Enum.Parse(prop.PropertyType, entry);
					else if (typeof(TimeSpan) == prop.PropertyType)
						value = TimeSpan.Parse(entry);
					else if (typeof(DateTime) == prop.PropertyType)
						value = DateTime.Parse(entry);
					else if (typeof(DateTimeOffset) == prop.PropertyType)
						value = DateTimeOffset.Parse(entry);
				}

				// Update value within our config instance
				prop.SetValue(config, value);
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine(
					$"Unable to convert value of '{entry}' to type '{prop.PropertyType.Name}' for prop '{prop.Name}' in config '{config.GetType().Name}'\n\t\t{ex.Message}");
			}
		}
	}

	/// <summary>
	/// Instantiate an entity using <paramref name="factory"/> then walk through the specified <paramref name="section"/>
	/// and translate user-defined config values to the instantiated instance from <paramref name="factory"/>
	/// </summary>
	/// <param name="section">Section containing values for targeted config</param>
	/// <param name="factory">Function which generates a default entity</param>
	/// <returns>Hydrated instance of an entity which contains user-defined values (if any)</returns>
	/// <exception cref="ArgumentNullException">When <paramref name="factory"/> is null</exception>
	public static object ExtractConfig(this ConfigSection section, Func<object> factory)
	{
		if (factory == null)
			throw new ArgumentNullException(nameof(factory), FACTORY_ERROR_MESSAGE);

		// Create default instance
		var config = factory();

		HydrateInstance(ref config, section);

		return config;
	}

	/// <summary>
	/// Instantiate an entity using <paramref name="factory"/> then walk through the specified <paramref name="sectionName"/>
	/// in <paramref name="config"/>. Translate user-defined config values to the instantiated instance from <paramref name="factory"/>
	/// </summary>
	/// <param name="config">Loaded App Configuration</param>
	/// <param name="sectionName">Name of section to load</param>
	/// <param name="factory">Function which creates a default entity to work with</param>
	/// <param name="rootSectionName">(Optional) Used when section is nested within another. Default value is <see cref="DEFAULT_ROOT_LIB"/></param>
	/// <returns>Hydrated instance of an entity which contains user-defined values (if any)</returns>
	/// <exception cref="ArgumentNullException">When <paramref name="factory"/> is null</exception>
	public static object ExtractConfig(
		this IConfiguration config,
		string sectionName,
		Func<object> factory,
		string? rootSectionName = DEFAULT_ROOT_LIB
	)
	{
		if (factory == null)
			throw new ArgumentNullException(nameof(factory), FACTORY_ERROR_MESSAGE);

		// create default instance
		var instance = factory();

		HydrateInstance(ref instance, new(ref config, sectionName, rootSectionName));

		return instance;
	}

	/// <summary>
	/// Instantiate a new instance of <typeparamref name="TConfig"/>, then walk through the specified <paramref name="sectionName"/>
	/// in <paramref name="config"/>. Translate user-defined config values to the <typeparamref name="TConfig"/> instance.
	/// </summary>
	/// <param name="config">Loaded App Configuration</param>
	/// <param name="serviceProvider"></param>
	/// <param name="sectionName">Name of section to load</param>
	/// <param name="rootSectionName">(Optional) Used when section is nested with another. Default value is <see cref="DEFAULT_ROOT_LIB"/></param>
	/// <typeparam name="TConfig">Type of instance that <paramref name="sectionName"/> represents</typeparam>
	/// <returns>Hydrated instance of <typeparamref name="TConfig"/> which contains the user-defined values (if any).</returns>
	public static TConfig ExtractConfig<TConfig>(this IConfiguration config, IServiceProvider serviceProvider, string sectionName, string? rootSectionName = DEFAULT_ROOT_LIB)
		where TConfig : new()
	{
		// Default values should hopefully be provided from the constructor
		var configInstance = ActivatorUtilities.CreateInstance(serviceProvider, typeof(TConfig));

		HydrateInstance(ref configInstance, new(ref config, sectionName, rootSectionName));

		return (TConfig)configInstance;
	}

	/// <summary>
	/// Instantiate a new instance of <typeparamref name="TConfig"/>, then walk through the specified <paramref name="sectionName"/>
	/// in <paramref name="config"/>. Translate user-defined config values to the <typeparamref name="TConfig"/> instance.
	/// </summary>
	/// <param name="config">Loaded App Configuration</param>
	/// <param name="sectionName">Name of section to load</param>
	/// <param name="rootSectionName">(Optional) Used when section is nested with another. Default value is <see cref="DEFAULT_ROOT_LIB"/></param>
	/// <typeparam name="TConfig">Type of instance that <paramref name="sectionName"/> represents</typeparam>
	/// <returns>Hydrated instance of <typeparamref name="TConfig"/> which contains the user-defined values (if any).</returns>
	public static TConfig ExtractConfig<TConfig>(this IConfiguration config, string sectionName, string? rootSectionName = DEFAULT_ROOT_LIB)
		where TConfig : new()
	{
		// Default values should hopefully be provided from the constructor
		object configInstance = new TConfig();

		HydrateInstance(ref configInstance, new(ref config, sectionName, rootSectionName));

		return (TConfig)configInstance;
	}

	/// <summary>
	/// Determines if <paramref name="config"/> contains a particular section/object (not value)
	/// </summary>
	/// <remarks>
	/// <code>
	/// {
	///    "Discord": {  // this is a section/object
	///
	///    },
	///    "Value": "something" // this is not a section/object
	/// }
	/// </code>
	/// </remarks>
	/// <param name="config"></param>
	/// <param name="values"></param>
	/// <returns>True if section exists, otherwise false</returns>
	public static bool HasSection(this IConfiguration config, params string?[] values)
	{
		if (values == null)
			return false;

		switch (values.Length)
		{
			case 0:
				return false;
			case 1:
				return config.GetChildren().Any(x => x.Key == values[0]);
		}

		if (config.GetChildren().All(x => x.Key != values[0]))
			return false;

		var current = config.GetSection(values[0]);

		for (var i = 1; i < values.Length - 1; i++)
		{
			if (current.GetChildren().All(x => x.Key != values[i]))
				return false;

			current = current.GetSection(values[i]);
		}

		return current.GetChildren().Any(x => x.Key == values[^1]);
	}

	/// <summary>
	/// Instantiates an instance of <see cref="DiscordClient"/>, then consumes any custom
	/// configuration from user/developer from <paramref name="config"/>. <br/>
	/// View remarks for more info
	/// </summary>
	/// <remarks>
	/// This is an example of how your JSON structure should look if you wish
	/// to override one or more of the default values from <see cref="DiscordConfiguration"/>
	/// <code>
	/// {
	///   "DisCatSharp": {
	///      "Discord": { }
	///   }
	/// }
	/// </code>
	/// <br/>
	/// Alternatively, you can use the type name itself
	/// <code>
	/// {
	///   "DisCatSharp": {
	///      "DiscordConfiguration": { }
	///   }
	/// }
	/// </code>
	/// <code>
	/// {
	///   "botSectionName": {
	///      "DiscordConfiguration": { }
	///   }
	/// }
	/// </code>
	/// </remarks>
	/// <param name="config"></param>
	/// <param name="serviceProvider"></param>
	/// <param name="botSectionName"></param>
	/// <returns>Instance of <see cref="DiscordClient"/></returns>
	public static DiscordClient BuildClient(
		this IConfiguration config,
		IServiceProvider serviceProvider,
		string botSectionName = DEFAULT_ROOT_LIB
	)
	{
		var section = config.HasSection(botSectionName, "Discord")
			? "Discord"
			: config.HasSection(botSectionName, $"Discord{CONFIG_SUFFIX}")
				? $"Discord:{CONFIG_SUFFIX}"
				: null;

		return string.IsNullOrEmpty(section)
			? new(new(serviceProvider))
			: new DiscordClient(config.ExtractConfig<DiscordConfiguration>(serviceProvider, section, botSectionName));
	}
}
