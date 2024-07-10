using System.Collections.Generic;
using System.Linq;

using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents the inner data payload of a <see cref="DiscordInteraction"/>.
/// </summary>
public sealed class DiscordInteractionData : SnowflakeObject
{
	/// <summary>
	/// Gets the name of the invoked interaction.
	/// </summary>
	[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
	public string Name { get; internal set; }

	/// <summary>
	/// Gets the parameters and values of the invoked interaction.
	/// </summary>
	[JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
	public IReadOnlyList<DiscordInteractionDataOption> Options { get; internal set; } = [];

	/// <summary>
	/// Gets the component rows. Only applicable to modal submits.
	/// </summary>
	[JsonProperty("components", NullValueHandling = NullValueHandling.Ignore)]
	internal List<DiscordActionRowComponentResult> ComponentsInternal { get; set; } = [];

	/// <summary>
	/// Gets the components. Only applicable to modal submits.
	/// <para>If you want to get the components, use <see cref="DiscordInteraction"/>.<see cref="DiscordInteraction.Message"/>.<see cref="DiscordMessage.Components"/> instead.</para>
	/// </summary>
	[JsonIgnore]
	public IReadOnlyList<DiscordComponentResult> Components
		=> this.ComponentsInternal.Where(comp => comp.Components.All(innerComp => innerComp.Type == ComponentType.InputText)).Select(x => x.Components[0]).ToList();

	/// <summary>
	/// Gets the Discord snowflake objects resolved from this interaction's arguments.
	/// </summary>
	[JsonProperty("resolved", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordInteractionResolvedCollection Resolved { get; internal set; }

	/// <summary>
	/// The Id of the component that invoked this interaction, if applicable.
	/// </summary>
	[JsonProperty("custom_id", NullValueHandling = NullValueHandling.Ignore)]
	public string CustomId { get; internal set; }

	/// <summary>
	/// The Id of the target. Applicable for context menus.
	/// </summary>
	[JsonProperty("target_id", NullValueHandling = NullValueHandling.Ignore)]
	internal ulong? Target { get; set; }

	/// <summary>
	/// The type of component that invoked this interaction, if applicable.
	/// </summary>
	[JsonProperty("component_type", NullValueHandling = NullValueHandling.Ignore)]
	public ComponentType ComponentType { get; internal set; }

	/// <summary>
	/// Gets the values of the interaction.
	/// </summary>
	[JsonProperty("values", NullValueHandling = NullValueHandling.Ignore)]
	public string[] Values { get; internal set; } = [];

	/// <summary>
	/// Gets the type of the interaction.
	/// </summary>
	[JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
	public ApplicationCommandType Type { get; internal set; }

	/// <summary>
	/// Gets the Id of the guild this interaction was invoked in, if any.
	/// </summary>
	[JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? GuildId { get; internal set; }
}
