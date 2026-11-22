using System;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DisCatSharp.Entities;

/// <summary>A custom game-stat field, retaining its JSON value for future field types.</summary>
public sealed class DiscordGameDynamicField
{
	/// <summary>Gets or sets the Discord field type: 1 for string, 2 for number, or 3 for media.</summary>
	[JsonProperty("type")] public int Type { get; set; }
	/// <summary>Gets or sets the field's data key.</summary>
	[JsonProperty("name")] public string Name { get; set; } = string.Empty;
	/// <summary>Gets or sets the value, whose JSON shape depends on <see cref="Type"/>.</summary>
	[JsonProperty("value")] public JToken? Value { get; set; }

	/// <summary>Creates a custom text statistic.</summary>
	/// <param name="name">The field's data key.</param><param name="value">The text value.</param><returns>The custom field.</returns>
	public static DiscordGameDynamicField String(string name, string value)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(name);
		ArgumentNullException.ThrowIfNull(value);
		return new() { Type = 1, Name = name, Value = new JValue(value) };
	}

	/// <summary>Creates a custom numeric statistic.</summary>
	/// <param name="name">The field's data key.</param><param name="value">The numeric value.</param><returns>The custom field.</returns>
	public static DiscordGameDynamicField Number(string name, double value)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(name);
		if (!double.IsFinite(value)) throw new ArgumentOutOfRangeException(nameof(value));
		return new() { Type = 2, Name = name, Value = new JValue(value) };
	}

	/// <summary>Creates a custom image statistic.</summary>
	/// <param name="name">The field's data key.</param><param name="url">The publicly accessible image URL.</param><returns>The custom field.</returns>
	public static DiscordGameDynamicField Media(string name, string url)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(name);
		ArgumentException.ThrowIfNullOrWhiteSpace(url);
		return new() { Type = 3, Name = name, Value = new JObject { ["url"] = url } };
	}
}
