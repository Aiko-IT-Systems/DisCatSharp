using System;
using System.Collections.Generic;

using DisCatSharp.Enums;
using DisCatSharp.Net.Serialization;
using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     A component to attach to a message.
/// </summary>
[JsonConverter(typeof(DiscordComponentJsonConverter))]
public class DiscordComponent : ObservableApiObject
{
	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordComponent" /> class.
	/// </summary>
	internal DiscordComponent()
	{ }

	/// <summary>
	///     The type of component this represents.
	/// </summary>
	[JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
	public ComponentType Type { get; internal set; }

	/// <summary>
	///     The custom Id of this component, if applicable. Not applicable on ActionRow(s), link buttons, and premium buttons.
	/// </summary>
	[JsonProperty("custom_id", NullValueHandling = NullValueHandling.Ignore)]
	public virtual string? CustomId { get; internal set; }

	/// <summary>
	///     Gets the Id of the compenent. Auto populated by discord if not provided.
	/// </summary>
	[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
	public int? Id { get; internal set; }

	/// <summary>
	///     Gets the child components of this component, if any.
	/// </summary>
	public virtual IEnumerable<DiscordComponent> GetChildren()
	{
		yield break;
	}
}
