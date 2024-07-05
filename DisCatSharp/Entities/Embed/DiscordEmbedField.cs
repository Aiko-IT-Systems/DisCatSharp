using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a field inside a discord embed.
/// </summary>
public sealed class DiscordEmbedField : ObservableApiObject
{
	/// <summary>
	/// The name of the field.
	/// Must be non-null, non-empty and &lt;= 256 characters.
	/// </summary>
	[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
	public string Name { get; set; }

	/// /// <summary>
	/// The value of the field.
	/// Must be non-null, non-empty and &lt;= 1024 characters.
	/// </summary>
	[JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
	public string Value { get; set; }

	/// <summary>
	/// Whether or not this field should display inline.
	/// </summary>
	[JsonProperty("inline", NullValueHandling = NullValueHandling.Ignore)]
	public bool Inline { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="DiscordEmbedField"/> class.
	/// </summary>
	/// <param name="name"><see cref="Name"/></param>
	/// <param name="value"><see cref="Value"/></param>
	/// <param name="inline"><see cref="Inline"/></param>
	public DiscordEmbedField(string name, string value, bool inline = false)
	{
		this.Name = name;
		this.Value = value;
		this.Inline = inline;
	}
}
