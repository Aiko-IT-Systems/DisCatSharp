using System.Collections.Generic;

using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents a field in a guild's membership screening form
/// </summary>
public class DiscordGuildMembershipScreeningField : ObservableApiObject
{
	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordGuildMembershipScreeningField" /> class.
	/// </summary>
	/// <param name="type">The type.</param>
	/// <param name="label">The label.</param>
	/// <param name="values">The values.</param>
	/// <param name="required">If true, required.</param>
	/// <param name="description">The description.</param>
	/// <param name="placeholder">The placeholder.</param>
	public DiscordGuildMembershipScreeningField(MembershipScreeningFieldType type, string label, List<string>? values = null, bool required = true, string? description = null, string? placeholder = null)
	{
		this.Type = type;
		this.Label = label;
		this.Values = values;
		this.IsRequired = required;
		this.Description = description;
		this.Placeholder = placeholder;
	}

	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordGuildMembershipScreeningField" /> class.
	/// </summary>
	internal DiscordGuildMembershipScreeningField()
	{ }

	/// <summary>
	///     Gets the type of the field.
	/// </summary>
	[JsonProperty("field_type", NullValueHandling = NullValueHandling.Ignore)]
	public MembershipScreeningFieldType Type { get; internal set; }

	/// <summary>
	///     Gets the title of the field.
	/// </summary>
	[JsonProperty("label", NullValueHandling = NullValueHandling.Ignore)]
	public string Label { get; internal set; }

	/// <summary>
	///     Gets the list of rules
	/// </summary>
	[JsonProperty("values", NullValueHandling = NullValueHandling.Ignore)]
	public IReadOnlyList<string>? Values { get; internal set; }

	/// <summary>
	///     .
	///     Gets whether the user has to fill out this field
	/// </summary>
	[JsonProperty("required", NullValueHandling = NullValueHandling.Ignore)]
	public bool IsRequired { get; internal set; }

	/// <summary>
	///     Gets the placeholder.
	/// </summary>
	[JsonProperty("placeholder", NullValueHandling = NullValueHandling.Ignore)]
	public string? Placeholder { get; internal set; }

	/// <summary>
	///     Gets the placeholder.
	/// </summary>
	[JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
	public string? Description { get; internal set; }

	/// <summary>
	///     Gets the placeholder.
	/// </summary>
	[JsonProperty("automations", NullValueHandling = NullValueHandling.Ignore)]
	public object? Automations { get; internal set; }
}
