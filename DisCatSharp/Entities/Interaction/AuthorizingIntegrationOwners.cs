using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents the authorizing integration owners.
/// </summary>
public sealed class AuthorizingIntegrationOwners
{
	/// <summary>
	/// If an interaction was created for an app that was installed to a user, a <c>USER_INSTALL</c> key will be present and its value will be the user’s ID.
	/// </summary>
	[JsonProperty("1", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? UserInstallKey { get; internal set; }

	/// <summary>
	/// If an interaction was created for an app that was installed to a guild, a <c>GUILD_INSTALL</c> key will be present.
	/// <para>If the interaction was sent from a guild context, the value is the guild’s ID.</para>
	/// <para>If the interaction was sent from a bot DM context, the value is <c>0</c>.</para>
	/// </summary>
	[JsonProperty("0", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? GuildInstallKey { get; internal set; }
}
