using System;
using System.Collections;
using System.Linq;
using System.Net;
using System.Globalization;

using DisCatSharp.Configuration.Models;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;

namespace DisCatSharp.Configuration;

/// <summary>
///     The configuration extensions.
/// </summary>
internal static class ConfigurationExtensions
{
	/// <summary>
	///     The factory error message.
	/// </summary>
	private const string FACTORY_ERROR_MESSAGE = "Require a function which provides a default entity to work with";

	/// <summary>
	///     The default root lib.
	/// </summary>
	public const string DEFAULT_ROOT_LIB = "DisCatSharp";

	/// <summary>
	///     The config suffix.
	/// </summary>
	private const string CONFIG_SUFFIX = "Configuration";

	/// <summary>
	///     Easily piece together paths that will work within <see cref="IConfiguration" />
	/// </summary>
	/// <param name="config">(not used - only for adding context based functionality)</param>
	/// <param name="values">The strings to piece together</param>
	/// <returns>Strings joined together via ':'</returns>
	public static string ConfigPath(this IConfiguration config, params string[] values) => string.Join(":", values);

	/// <summary>
	///     Skims over the configuration section and only overrides values that are explicitly defined within the config.
	/// </summary>
	/// <param name="config">Instance of config</param>
	/// <param name="section">Section which contains values for <paramref name="config" /></param>
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

			// Handle collection types differently
			if (typeof(IEnumerable).IsAssignableFrom(prop.PropertyType) && prop.PropertyType != typeof(string))
			{
				if (string.IsNullOrEmpty(entry))
				{
					// Handle case for "root:section:name:0"
					var subSection = section.Config.GetSection(section.GetPath(prop.Name));
					if (subSection.Exists())
						value = subSection.Get(prop.PropertyType);
				}
				else
					// Handle case for "root:section:name"
					value = JsonConvert.DeserializeObject(entry, prop.PropertyType);

				if (value != null)
					prop.SetValue(config, value);

				continue;
			}

			try
			{
				var targetType = prop.PropertyType;

				// If no direct value provided, attempt nested hydration for complex/proxy types
				if (string.IsNullOrEmpty(entry))
				{
					if (typeof(IWebProxy).IsAssignableFrom(targetType))
					{
						value = TryCreateWebProxy(section, prop.Name, entry);
						if (value != null)
							prop.SetValue(config, value);
						continue;
					}

					value = TryHydrateComplexType(section, prop.Name, targetType, entry);
					if (value != null)
						prop.SetValue(config, value);
					continue;
				}

				// Handle nullable types
				if (Nullable.GetUnderlyingType(targetType) != null)
					targetType = Nullable.GetUnderlyingType(targetType);

				// Primitive types and common types conversion
				if (targetType.IsPrimitive)
					value = Convert.ChangeType(entry, targetType, CultureInfo.InvariantCulture);
				else if (targetType.IsEnum)
					value = Enum.Parse(targetType, entry.Replace('|', ','));
				else if (targetType == typeof(TimeSpan))
					value = TimeSpan.Parse(entry, CultureInfo.InvariantCulture);
				else if (targetType == typeof(DateTime))
					value = DateTime.Parse(entry, CultureInfo.InvariantCulture);
				else if (targetType == typeof(DateTimeOffset))
					value = DateTimeOffset.Parse(entry, CultureInfo.InvariantCulture);
				else if (typeof(IWebProxy).IsAssignableFrom(targetType))
					value = TryCreateWebProxy(section, prop.Name, entry) ?? throw new NotSupportedException($"Unable to parse proxy from '{entry}'.");
				else
					value = TryHydrateComplexType(section, prop.Name, targetType, entry) ?? throw new NotSupportedException($"Type '{targetType.Name}' is not supported.");

				// Update value within our config instance
				prop.SetValue(config, value);
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine(
					$"Unable to convert value of '{entry}' to type '{prop.PropertyType.Name}' for property '{prop.Name}' in config '{config.GetType().Name}':\n\t\t{ex.Message}");
			}
		}
	}

	/// <summary>
	///     Attempts to create a <see cref="IWebProxy" /> from a user-specified value or sub-section.
	/// </summary>
	/// <param name="section">Configuration section being hydrated.</param>
	/// <param name="propertyName">Property name pointing to the proxy object.</param>
	/// <param name="value">Proxy value provided via configuration.</param>
	/// <returns>Instance of <see cref="IWebProxy" /> when parsable; otherwise, <see langword="null" />.</returns>
	private static IWebProxy? TryCreateWebProxy(ConfigSection section, string propertyName, string value)
	{
		// First attempt: treat value as a direct URI (with or without scheme)
		if (!string.IsNullOrWhiteSpace(value))
		{
			var directProxy = TryCreateProxyFromString(value, null);
			if (directProxy != null)
				return directProxy;
		}

		// Second attempt: hydrate from nested object
		var proxySection = section.Config.GetSection(section.GetPath(propertyName));
		if (!proxySection.Exists())
			return null;

		var host = proxySection["Host"] ?? proxySection["Address"] ?? proxySection["Uri"] ?? proxySection["Url"];
		var port = proxySection["Port"];
		var bypassOnLocal = proxySection["BypassOnLocal"];

		return TryCreateProxyFromString(host, port, bypassOnLocal);
	}

	/// <summary>
	///     Tries to create a proxy from string values, supporting host, port and bypass-on-local.
	/// </summary>
	private static IWebProxy? TryCreateProxyFromString(string hostOrUri, string? port = null, string? bypassOnLocal = null)
	{
		if (string.IsNullOrWhiteSpace(hostOrUri))
			return null;


		// Allow host:port without scheme
		if (!Uri.TryCreate(hostOrUri, UriKind.Absolute, out var proxyUri))
		{
			var candidate = hostOrUri.Contains("://", StringComparison.Ordinal)
				? hostOrUri
				: $"http://{hostOrUri}";

			if (Uri.TryCreate(candidate, UriKind.Absolute, out var candidateUri))
				proxyUri = candidateUri;
		}

		if (proxyUri == null)
			return null;

		if (int.TryParse(port, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedPort) && parsedPort > 0)
		{
			var builder = new UriBuilder(proxyUri) { Port = parsedPort };
			proxyUri = builder.Uri;
		}

		var proxy = new WebProxy(proxyUri);
		if (bool.TryParse(bypassOnLocal, out var bypass))
			proxy.BypassProxyOnLocal = bypass;

		return proxy;
	}

	/// <summary>
	///     Attempts to hydrate a complex type using JSON value or a nested configuration section.
	/// </summary>
	private static object? TryHydrateComplexType(ConfigSection section, string propertyName, Type targetType, string value)
	{
		if (!string.IsNullOrWhiteSpace(value))
			try
			{
				return JsonConvert.DeserializeObject(value, targetType);
			}
			catch
			{ }

		var subSection = section.Config.GetSection(section.GetPath(propertyName));
		if (subSection.Exists())
			try
			{
				return subSection.Get(targetType);
			}
			catch
			{ }

		return null;
	}

	/// <summary>
	///     Instantiate an entity using <paramref name="factory" /> then walk through the specified <paramref name="section" />
	///     and translate user-defined config values to the instantiated instance from <paramref name="factory" />
	/// </summary>
	/// <param name="section">Section containing values for targeted config</param>
	/// <param name="factory">Function which generates a default entity</param>
	/// <returns>Hydrated instance of an entity which contains user-defined values (if any)</returns>
	/// <exception cref="ArgumentNullException">When <paramref name="factory" /> is null</exception>
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
	///     Instantiate an entity using <paramref name="factory" /> then walk through the specified
	///     <paramref name="sectionName" />
	///     in <paramref name="config" />. Translate user-defined config values to the instantiated instance from
	///     <paramref name="factory" />
	/// </summary>
	/// <param name="config">Loaded App Configuration</param>
	/// <param name="sectionName">Name of section to load</param>
	/// <param name="factory">Function which creates a default entity to work with</param>
	/// <param name="rootSectionName">
	///     (Optional) Used when section is nested within another. Default value is
	///     <see cref="DEFAULT_ROOT_LIB" />
	/// </param>
	/// <returns>Hydrated instance of an entity which contains user-defined values (if any)</returns>
	/// <exception cref="ArgumentNullException">When <paramref name="factory" /> is null</exception>
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
	///     Instantiate a new instance of <typeparamref name="TConfig" />, then walk through the specified
	///     <paramref name="sectionName" />
	///     in <paramref name="config" />. Translate user-defined config values to the <typeparamref name="TConfig" />
	///     instance.
	/// </summary>
	/// <param name="config">Loaded App Configuration</param>
	/// <param name="serviceProvider"></param>
	/// <param name="sectionName">Name of section to load</param>
	/// <param name="rootSectionName">
	///     (Optional) Used when section is nested with another. Default value is
	///     <see cref="DEFAULT_ROOT_LIB" />
	/// </param>
	/// <typeparam name="TConfig">Type of instance that <paramref name="sectionName" /> represents</typeparam>
	/// <returns>Hydrated instance of <typeparamref name="TConfig" /> which contains the user-defined values (if any).</returns>
	public static TConfig ExtractConfig<TConfig>(this IConfiguration config, IServiceProvider serviceProvider, string sectionName, string? rootSectionName = DEFAULT_ROOT_LIB)
		where TConfig : new()
	{
		// Default values should hopefully be provided from the constructor
		var configInstance = ActivatorUtilities.CreateInstance(serviceProvider, typeof(TConfig));

		HydrateInstance(ref configInstance, new(ref config, sectionName, rootSectionName));

		return (TConfig)configInstance;
	}

	/// <summary>
	///     Instantiate a new instance of <typeparamref name="TConfig" />, then walk through the specified
	///     <paramref name="sectionName" />
	///     in <paramref name="config" />. Translate user-defined config values to the <typeparamref name="TConfig" />
	///     instance.
	/// </summary>
	/// <param name="config">Loaded App Configuration</param>
	/// <param name="sectionName">Name of section to load</param>
	/// <param name="rootSectionName">
	///     (Optional) Used when section is nested with another. Default value is
	///     <see cref="DEFAULT_ROOT_LIB" />
	/// </param>
	/// <typeparam name="TConfig">Type of instance that <paramref name="sectionName" /> represents</typeparam>
	/// <returns>Hydrated instance of <typeparamref name="TConfig" /> which contains the user-defined values (if any).</returns>
	public static TConfig ExtractConfig<TConfig>(this IConfiguration config, string sectionName, string? rootSectionName = DEFAULT_ROOT_LIB)
		where TConfig : new()
	{
		// Default values should hopefully be provided from the constructor
		object configInstance = new TConfig();

		HydrateInstance(ref configInstance, new(ref config, sectionName, rootSectionName));

		return (TConfig)configInstance;
	}

	/// <summary>
	///     Determines if <paramref name="config" /> contains a particular section/object (not value)
	/// </summary>
	/// <remarks>
	///     <code>
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
		if (values == null || values.Length == 0)
			return false;

		if (values.Length == 1)
			return config.GetChildren().Any(x => x.Key == values[0]);

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
	///     Instantiates an instance of <see cref="DiscordClient" />, then consumes any custom
	///     configuration from user/developer from <paramref name="config" />. <br />
	///     View remarks for more info
	/// </summary>
	/// <remarks>
	///     This is an example of how your JSON structure should look if you wish
	///     to override one or more of the default values from <see cref="DiscordConfiguration" />
	///     <code>
	/// {
	///   "DisCatSharp": {
	///      "Discord": { }
	///   }
	/// }
	/// </code>
	///     <br />
	///     Alternatively, you can use the type name itself
	///     <code>
	/// {
	///   "DisCatSharp": {
	///      "DiscordConfiguration": { }
	///   }
	/// }
	/// </code>
	///     <code>
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
	/// <returns>Instance of <see cref="DiscordClient" /></returns>
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
