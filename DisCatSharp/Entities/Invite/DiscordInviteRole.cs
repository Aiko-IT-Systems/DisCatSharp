using System;
using System.Globalization;

using DisCatSharp.Attributes;
using DisCatSharp.Enums;
using DisCatSharp.Net;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents the role an invite assigns.
/// </summary>
public class DiscordInviteRole : SnowflakeObject
{
	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordInviteRole" /> class.
	/// </summary>
	internal DiscordInviteRole()
	{ }

	/// <summary>
	///     Gets the name of the role.
	/// </summary>
	[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
	public string Name { get; internal set; }

	/// <summary>
	///     Gets the position of the role.
	/// </summary>
	[JsonProperty("position", NullValueHandling = NullValueHandling.Ignore)]
	public int Position { get; internal set; }

	/// <summary>
	///     Gets the color of the role.
	/// </summary>
	[JsonProperty("color", NullValueHandling = NullValueHandling.Ignore), DiscordDeprecated("Use Colors instead")]
	internal int ColorInternal { get; set; }

	/// <summary>
	///     Gets the color of the role.
	/// </summary>
	[JsonIgnore, DiscordDeprecated("Use Colors instead")]
	public DiscordColor Color
		=> new(this.ColorInternal);

	/// <summary>
	///     Gets the colors of the role.
	/// </summary>
	[JsonProperty("colors", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordRoleColors Colors { get; internal set; }

	/// <summary>
	///     Gets the icon hash of the role.
	/// </summary>
	[JsonProperty("icon", NullValueHandling = NullValueHandling.Ignore)]
	internal string? IconHash { get; set; }

	/// <summary>
	///     Gets the icon of the role.
	/// </summary>
	[JsonIgnore]
	public string? IconUrl
		=> !string.IsNullOrWhiteSpace(this.IconHash) ? $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Url}{Endpoints.ROLE_ICONS}/{this.Id.ToString(CultureInfo.InvariantCulture)}/{this.IconHash}.png?size=64" : null;

	/// <summary>
	///     Gets the unicode emoji of the role.
	/// </summary>
	[JsonProperty("unicode_emoji", NullValueHandling = NullValueHandling.Ignore)]
	internal string? UnicodeEmojiString { get; set; }

	/// <summary>
	///     Gets the unicode emoji of the role.
	/// </summary>

	[JsonIgnore]
	public DiscordEmoji? UnicodeEmoji
		=> !string.IsNullOrEmpty(this.UnicodeEmojiString) ? DiscordEmoji.FromUnicode(this.UnicodeEmojiString) : null;

	/// <summary>
	///     Gets the optional permissions of the role.
	/// </summary>
	[JsonProperty("permissions", NullValueHandling = NullValueHandling.Ignore)]
	public Optional<Permissions> Permissions { get; internal set; }

	/// <summary>
	///    Creates a <see cref="DiscordInviteRole"/> from a <see cref="DiscordRole"/>.
	/// </summary>
	/// <param name="role">The role to create from.</param>
	/// <returns>The created <see cref="DiscordInviteRole"/>.</returns>
	public static DiscordInviteRole FromRole(DiscordRole role)
	{
		ArgumentNullException.ThrowIfNull(role, nameof(role));

		var inviteRole = new DiscordInviteRole
		{
			Id = role.Id,
			Name = role.Name,
			Position = role.Position,
			ColorInternal = role.ColorInternal,
			Colors = role.Colors,
			IconHash = role.IconHash,
			UnicodeEmojiString = role.UnicodeEmojiString,
			Permissions = role.Permissions
		};

		return inviteRole;
	}

	/// <summary>
	///     Implicitly creates a <see cref="DiscordInviteRole"/> from a <see cref="DiscordRole"/>.
	/// </summary>
	/// <param name="role">The role to convert.</param>
	/// <returns>The created <see cref="DiscordInviteRole"/>.</returns>
	public static implicit operator DiscordInviteRole(DiscordRole role)
		=> FromRole(role);
}
