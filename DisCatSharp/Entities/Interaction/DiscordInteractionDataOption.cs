using System.Collections.Generic;

using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents parameters for interaction commands.
/// </summary>
public sealed class DiscordInteractionDataOption : ObservableApiObject
{
	/// <summary>
	/// Gets the name of this interaction parameter.
	/// </summary>
	[JsonProperty("name")]
	public string Name { get; internal set; }

	/// <summary>
	/// Gets the type of this interaction parameter.
	/// </summary>
	[JsonProperty("type")]
	public ApplicationCommandOptionType Type { get; internal set; }

	/// <summary>
	/// Whether this option is currently focused by the user.
	/// Only applicable for autocomplete option choices.
	/// </summary>
	[JsonProperty("focused")]
	public bool Focused { get; internal set; }

	/// <summary>
	/// Gets the value of this interaction parameter.
	/// </summary>
	[JsonProperty("value")]
	internal string RawValue { get; set; }

	/// <summary>
	/// Gets the value of this interaction parameter.
	/// <para>This can be cast to a <see langword="long"/>, <see langword="bool"></see>, <see langword="string"></see>, <see langword="double"></see> or <see langword="ulong"/> depending on the <see cref="System.Type"/></para>
	/// </summary>
	[JsonIgnore]
	public object Value =>
		this.Type == ApplicationCommandOptionType.Integer && int.TryParse(this.RawValue, out var raw)
			? raw
			: this.Type == ApplicationCommandOptionType.Integer
				? long.Parse(this.RawValue)
				: this.Type switch
				{
					ApplicationCommandOptionType.Boolean => bool.Parse(this.RawValue),
					ApplicationCommandOptionType.String => this.RawValue,
					ApplicationCommandOptionType.Channel => ulong.Parse(this.RawValue),
					ApplicationCommandOptionType.User => ulong.Parse(this.RawValue),
					ApplicationCommandOptionType.Role => ulong.Parse(this.RawValue),
					ApplicationCommandOptionType.Mentionable => ulong.Parse(this.RawValue),
					ApplicationCommandOptionType.Number => double.Parse(this.RawValue),
					ApplicationCommandOptionType.Attachment => ulong.Parse(this.RawValue),
					_ => this.RawValue
				};

	/// <summary>
	/// Gets the additional parameters if this parameter is a subcommand.
	/// </summary>
	[JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
	public IReadOnlyList<DiscordInteractionDataOption> Options { get; internal set; }
}
