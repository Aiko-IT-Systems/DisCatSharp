using System;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a field inside a discord embed.
/// </summary>
public sealed class DiscordEmbedField : ObservableApiObject
{
	private string _name;

	/// <summary>
	/// The name of the field.
	/// Must be non-null, non-empty and &lt;= 256 characters.
	/// </summary>
	[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
	public string Name
	{
		get => this._name;
		set
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				ArgumentNullException.ThrowIfNull(value);

				throw new ArgumentException("Name cannot be empty or whitespace.", nameof(value));
			}

			if (value.Length > 256)
				throw new ArgumentException("Embed field name length cannot exceed 256 characters.");

			this._name = value;
		}
	}

	private string _value;

	/// <summary>
	/// The value of the field.
	/// Must be non-null, non-empty and &lt;= 1024 characters.
	/// </summary>
	[JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
	public string Value
	{
		get => this._value;
		set
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				ArgumentNullException.ThrowIfNull(value);

				throw new ArgumentException("Value cannot be empty or whitespace.", nameof(value));
			}

			if (value.Length > 1024)
				throw new ArgumentException("Embed field value length cannot exceed 1024 characters.");

			this._value = value;
		}
	}

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
