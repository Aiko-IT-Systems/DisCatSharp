using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents a response to a field in a guild's membership screening form.
/// </summary>
public sealed class DiscordGuildMembershipScreeningFieldResponse : DiscordGuildMembershipScreeningField
{
	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordGuildMembershipScreeningFieldResponse" /> class.
	/// </summary>
	internal DiscordGuildMembershipScreeningFieldResponse()
	{ }

	/// <summary>.
	///     Gets whether the response. Might also be a boolean.
	/// </summary>
	[JsonProperty("response", NullValueHandling = NullValueHandling.Ignore)]
	public string? Response { get; internal set; }
}
