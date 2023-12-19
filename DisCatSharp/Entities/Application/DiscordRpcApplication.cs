using System;
using System.Collections.Generic;
using System.Globalization;

using DisCatSharp.Enums;
using DisCatSharp.Net;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents an OAuth2 application.
/// </summary>
public sealed class DiscordRpcApplication : SnowflakeObject, IEquatable<DiscordRpcApplication>
{
	[JsonProperty("name")]
	public string Name;

	[JsonProperty("icon")]
	public string? IconHash;

	[JsonIgnore]
	public string? Icon => this.IconHash != null ? $"https://cdn.discordapp.com{Endpoints.APP_ICONS}/{this.Id}/{this.IconHash}.png" : null;

	[JsonProperty("description")]
	public string? Description;

	[JsonProperty("summary")]
	public string? Summary;

	[JsonProperty("type")]
	public string Type;

	[JsonProperty("hook")]
	public bool Hook;

	[JsonProperty("guild_id")]
	public ulong? GuildId;

	[JsonProperty("bot_public")]
	public bool IsPublic;

	[JsonProperty("bot_require_code_grant")]
	public bool RequiresCodeGrant;

	[JsonProperty("terms_of_service_url")]
	public string? TermsOfServiceUrl;

	[JsonProperty("privacy_policy_url")]
	public string? PrivacyPolicyUrl;

	[JsonProperty("install_params")]
	public DiscordApplicationInstallParams InstallParams;

	[JsonProperty("verify_key")]
	public string VerifyKey;

	[JsonProperty("flags")]
	public ApplicationFlags Flags;

	[JsonProperty("tags")]
	public List<string>? Tags;

	/// <summary>
	/// Initializes a new instance of the <see cref="DiscordRpcApplication"/> class.
	/// </summary>
	internal DiscordRpcApplication()
	{ }

	/// <summary>
	/// Generates an oauth url for the application.
	/// </summary>
	/// <param name="permissions">The permissions.</param>
	/// <returns>OAuth Url</returns>
	public string GenerateBotOAuth(Permissions permissions = Permissions.None)
	{
		permissions &= PermissionMethods.FullPerms;
		// hey look, it's not all annoying and blue :P
		return new QueryUriBuilder($"{DiscordDomain.GetDomain(CoreDomain.Discord).Url}{Endpoints.OAUTH2}{Endpoints.AUTHORIZE}")
			.AddParameter("client_id", this.Id.ToString(CultureInfo.InvariantCulture))
			.AddParameter("scope", "bot")
			.AddParameter("permissions", ((long)permissions).ToString(CultureInfo.InvariantCulture))
			.ToString();
	}

	/// <summary>
	/// Checks whether this <see cref="DiscordRpcApplication"/> is equal to another object.
	/// </summary>
	/// <param name="obj">Object to compare to.</param>
	/// <returns>Whether the object is equal to this <see cref="DiscordRpcApplication"/>.</returns>
	public override bool Equals(object obj)
		=> this.Equals(obj as DiscordRpcApplication);

	/// <summary>
	/// Checks whether this <see cref="DiscordRpcApplication"/> is equal to another <see cref="DiscordRpcApplication"/>.
	/// </summary>
	/// <param name="e"><see cref="DiscordRpcApplication"/> to compare to.</param>
	/// <returns>Whether the <see cref="DiscordRpcApplication"/> is equal to this <see cref="DiscordRpcApplication"/>.</returns>
	public bool Equals(DiscordRpcApplication e)
		=> e is not null && (ReferenceEquals(this, e) || this.Id == e.Id);

	/// <summary>
	/// Gets the hash code for this <see cref="DiscordRpcApplication"/>.
	/// </summary>
	/// <returns>The hash code for this <see cref="DiscordRpcApplication"/>.</returns>
	public override int GetHashCode()
		=> this.Id.GetHashCode();

	/// <summary>
	/// Gets whether the two <see cref="DiscordRpcApplication"/> objects are equal.
	/// </summary>
	/// <param name="e1">First application to compare.</param>
	/// <param name="e2">Second application to compare.</param>
	/// <returns>Whether the two applications are equal.</returns>
	public static bool operator ==(DiscordRpcApplication e1, DiscordRpcApplication e2)
	{
		var o1 = e1 as object;
		var o2 = e2 as object;

		return (o1 != null || o2 == null) && (o1 == null || o2 != null) && ((o1 == null && o2 == null) || e1.Id == e2.Id);
	}

	/// <summary>
	/// Gets whether the two <see cref="DiscordRpcApplication"/> objects are not equal.
	/// </summary>
	/// <param name="e1">First application to compare.</param>
	/// <param name="e2">Second application to compare.</param>
	/// <returns>Whether the two applications are not equal.</returns>
	public static bool operator !=(DiscordRpcApplication e1, DiscordRpcApplication e2)
		=> !(e1 == e2);
}
