using System;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a command parameter choice for a <see cref="DiscordApplicationCommandOption"/>.
/// </summary>
public sealed class DiscordApplicationCommandOptionChoice
{
	/// <summary>
	/// Gets the name of this choice parameter.
	/// </summary>
	[JsonProperty("name")]
	public string Name { get; set; }

	/// <summary>
	/// Sets the name localizations.
	/// </summary>
	[JsonProperty("name_localizations", NullValueHandling = NullValueHandling.Ignore)]
	internal Dictionary<string, string> RawNameLocalizations { get; set; }

	/// <summary>
	/// Gets the name localizations.
	/// </summary>
	[JsonIgnore]
	public DiscordApplicationCommandLocalization NameLocalizations
		=> new(this.RawNameLocalizations);

	/// <summary>
	/// Gets the value of this choice parameter. This will either be a type of <see cref="int"/>, <see cref="long"/>, <see cref="double"/> or <see cref="string"/>.
	/// </summary>
	[JsonProperty("value")]
	public object Value { get; set; }

	/// <summary>
	/// Creates a new instance of a <see cref="DiscordApplicationCommandOptionChoice"/>.
	/// </summary>
	/// <param name="name">The name of the parameter choice.</param>
	/// <param name="value">The value of the parameter choice.</param>
	/// <param name="nameLocalizations">The localizations of the parameter choice name.</param>
	public DiscordApplicationCommandOptionChoice(string name, object value, DiscordApplicationCommandLocalization nameLocalizations = null)
	{
		if (!(value is string || value is long || value is int || value is double))
			throw new InvalidOperationException($"Only {typeof(string)}, {typeof(long)}, {typeof(double)} or {typeof(int)} types may be passed to a command option choice.");

		if (name.Length > 100)
			throw new ArgumentException("Application command choice name cannot exceed 100 characters.", nameof(name));
		if (value is string { Length: > 100 })
			throw new ArgumentException("Application command choice value cannot exceed 100 characters.", nameof(value));

		this.Name = name;
		this.RawNameLocalizations = nameLocalizations?.GetKeyValuePairs();
		this.Value = value;
	}
}
