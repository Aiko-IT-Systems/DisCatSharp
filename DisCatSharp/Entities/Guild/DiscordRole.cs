using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using DisCatSharp.Enums;
using DisCatSharp.Exceptions;
using DisCatSharp.Net;
using DisCatSharp.Net.Abstractions;
using DisCatSharp.Net.Models;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a discord role, to which users can be assigned.
/// </summary>
public class DiscordRole : SnowflakeObject, IEquatable<DiscordRole>
{
	/// <summary>
	/// Gets the name of this role.
	/// </summary>
	[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
	public string Name { get; internal set; }

	/// <summary>
	/// Gets the version number for this role.
	/// </summary>
	[JsonProperty("version", NullValueHandling = NullValueHandling.Ignore)]
	public ulong Version { get; internal set; }

	/// <summary>
	/// Gets the description of this role.
	/// </summary>
	[JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
	public string Description { get; internal set; }

	/// <summary>
	/// Gets the color of this role.
	/// </summary>
	[JsonIgnore]
	public DiscordColor Color
		=> new(this.ColorInternal);

	[JsonProperty("color", NullValueHandling = NullValueHandling.Ignore)]
	internal int ColorInternal;

	/// <summary>
	/// Gets whether this role is hoisted.
	/// </summary>
	[JsonProperty("hoist", NullValueHandling = NullValueHandling.Ignore)]
	public bool IsHoisted { get; internal set; }

	/// <summary>
	/// Gets the position of this role in the role hierarchy.
	/// </summary>
	[JsonProperty("position", NullValueHandling = NullValueHandling.Ignore)]
	public int Position { get; internal set; }

	/// <summary>
	/// Gets the permissions set for this role.
	/// </summary>
	[JsonProperty("permissions", NullValueHandling = NullValueHandling.Ignore)]
	public Permissions Permissions { get; internal set; }

	/// <summary>
	/// Gets whether this role is managed by an integration.
	/// </summary>
	[JsonProperty("managed", NullValueHandling = NullValueHandling.Ignore)]
	public bool IsManaged { get; internal set; }

	/// <summary>
	/// Gets whether this role is mentionable.
	/// </summary>
	[JsonProperty("mentionable", NullValueHandling = NullValueHandling.Ignore)]
	public bool IsMentionable { get; internal set; }

	/// <summary>
	/// Gets the tags this role has.
	/// </summary>
	[JsonProperty("tags", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordRoleTags Tags { get; internal set; }

	/// <summary>
	/// Gets the role icon's hash.
	/// </summary>
	[JsonProperty("icon", NullValueHandling = NullValueHandling.Ignore)]
	public string IconHash { get; internal set; }

	/// <summary>
	/// Gets the role icon's url.
	/// </summary>
	[JsonIgnore]
	public string IconUrl
		=> !string.IsNullOrWhiteSpace(this.IconHash) ? $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Url}{Endpoints.ROLE_ICONS}/{this.Id.ToString(CultureInfo.InvariantCulture)}/{this.IconHash}.png?size=64" : null;

	/// <summary>
	/// Gets the role unicode_emoji.
	/// </summary>
	[JsonProperty("unicode_emoji", NullValueHandling = NullValueHandling.Ignore)]
	internal string UnicodeEmojiString;

	/// <summary>
	/// Gets the unicode emoji.
	/// </summary>
	[JsonIgnore]
	public DiscordEmoji UnicodeEmoji
		=> this.UnicodeEmojiString != null ? DiscordEmoji.FromName(this.Discord, $":{this.UnicodeEmojiString}:", false) : null;

	/// <summary>
	/// Gets the guild this role belongs to.
	/// </summary>
	[JsonIgnore]
	public DiscordGuild Guild
	{
		get
		{
			this.GuildInternal ??= this.Discord.Guilds[this.GuildId];
			return this.GuildInternal;
		}
	}

	[JsonIgnore]
	internal DiscordGuild GuildInternal { get; set; }

	[JsonIgnore]
	internal ulong GuildId = 0;

	/// <summary>
	/// Gets the role flags.
	/// </summary>
	[JsonProperty("flags", NullValueHandling = NullValueHandling.Ignore)]
	public RoleFlags Flags { get; internal set; }

	/// <summary>
	/// Gets a mention string for this role. If the role is mentionable, this string will mention all the users that belong to this role.
	/// </summary>
	[JsonIgnore]
	public string Mention
		=> this.Mention();

	/// <summary>
	/// Gets a list of members that have this role. Requires ServerMembers Intent.
	/// </summary>
	[JsonIgnore]
	public IReadOnlyList<KeyValuePair<ulong, DiscordMember>> Members
		=> this.Guild.Members.Where(x => x.Value.RoleIds.Any(x => x == this.Id)).ToList();

#region Methods

	/// <summary>
	/// Modifies this role's position.
	/// </summary>
	/// <param name="position">New position</param>
	/// <param name="reason">Reason why we moved it</param>
	/// <returns></returns>
	/// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageRoles"/> permission.</exception>
	/// <exception cref="NotFoundException">Thrown when the role does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task ModifyPositionAsync(int position, string reason = null)
	{
		var roles = this.Guild.Roles.Values.OrderByDescending(xr => xr.Position)
			.Select(x => new RestGuildRoleReorderPayload
			{
				RoleId = x.Id,
				Position = x.Id == this.Id
					? position
					: x.Position <= position
						? x.Position - 1
						: x.Position
			});

		return this.Discord.ApiClient.ModifyGuildRolePositionAsync(this.GuildId, roles, reason);
	}

	/// <summary>
	/// Updates this role.
	/// </summary>
	/// <param name="name">New role name.</param>
	/// <param name="permissions">New role permissions.</param>
	/// <param name="color">New role color.</param>
	/// <param name="hoist">New role hoist.</param>
	/// <param name="mentionable">Whether this role is mentionable.</param>
	/// <param name="reason">Audit log reason.</param>
	/// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageRoles"/> permission.</exception>
	/// <exception cref="NotFoundException">Thrown when the role does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task ModifyAsync(string name = null, Permissions? permissions = null, DiscordColor? color = null, bool? hoist = null, bool? mentionable = null, string reason = null)
		=> this.Discord.ApiClient.ModifyGuildRoleAsync(this.GuildId, this.Id, name, permissions, color?.Value, hoist, mentionable, Optional.None, Optional.None, reason);

	/// <summary>
	/// Updates this role.
	/// </summary>
	/// <param name="action">The action.</param>
	/// <exception cref = "Exceptions.UnauthorizedException" > Thrown when the client does not have the<see cref="Permissions.ManageRoles"/> permission.</exception>
	/// <exception cref="NotFoundException">Thrown when the role does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task ModifyAsync(Action<RoleEditModel> action)
	{
		var mdl = new RoleEditModel();
		action(mdl);

		var canContinue = true;
		if (mdl.Icon is { HasValue: true, Value: not null } || (mdl.UnicodeEmoji.HasValue && mdl.UnicodeEmoji.Value != null))
			canContinue = this.Guild.Features.HasFeature(GuildFeaturesEnum.CanSetRoleIcons);

		var iconb64 = Optional.FromNullable<string>(null);
		iconb64 = mdl.Icon.HasValue switch
		{
			true when mdl.Icon.Value != null => ImageTool.Base64FromStream(mdl.Icon),
			true => Optional.Some<string?>(null),
			_ => Optional.None
		};
		var emoji = Optional.FromNullable<string?>(null);

		switch (mdl.UnicodeEmoji.HasValue)
		{
			case true when mdl.UnicodeEmoji.Value != null:
				emoji = mdl.UnicodeEmoji
					.MapOrNull(e => e.Id == 0
						? e.Name
						: throw new ArgumentException("Emoji must be unicode"));
				break;
			case true:
				emoji = Optional.Some<string?>(null);
				break;
			case false:
				emoji = Optional.None;
				break;
		}

		return canContinue ? this.Discord.ApiClient.ModifyGuildRoleAsync(this.GuildId, this.Id, mdl.Name, mdl.Permissions, mdl.Color?.Value, mdl.Hoist, mdl.Mentionable, iconb64, emoji, mdl.AuditLogReason) : throw new NotSupportedException($"Cannot modify role icon. Guild needs boost tier two.");
	}

	/// <summary>
	/// Deletes this role.
	/// </summary>
	/// <param name="reason">Reason as to why this role has been deleted.</param>
	/// <returns></returns>
	/// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageRoles"/> permission.</exception>
	/// <exception cref="NotFoundException">Thrown when the role does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task DeleteAsync(string reason = null)
		=> this.Discord.ApiClient.DeleteRoleAsync(this.GuildId, this.Id, reason);

#endregion

	/// <summary>
	/// Initializes a new instance of the <see cref="DiscordRole"/> class.
	/// </summary>
	internal DiscordRole()
	{ }

	/// <summary>
	/// Checks whether this role has specific permissions.
	/// </summary>
	/// <param name="permission">Permissions to check for.</param>
	/// <returns>Whether the permissions are allowed or not.</returns>
	public PermissionLevel CheckPermission(Permissions permission)
		=> (this.Permissions & permission) != 0 ? PermissionLevel.Allowed : PermissionLevel.Unset;

	/// <summary>
	/// Returns a string representation of this role.
	/// </summary>
	/// <returns>String representation of this role.</returns>
	public override string ToString()
		=> $"Role {this.Id}; {this.Name}";

	/// <summary>
	/// Checks whether this <see cref="DiscordRole"/> is equal to another object.
	/// </summary>
	/// <param name="obj">Object to compare to.</param>
	/// <returns>Whether the object is equal to this <see cref="DiscordRole"/>.</returns>
	public override bool Equals(object obj)
		=> this.Equals(obj as DiscordRole);

	/// <summary>
	/// Checks whether this <see cref="DiscordRole"/> is equal to another <see cref="DiscordRole"/>.
	/// </summary>
	/// <param name="e"><see cref="DiscordRole"/> to compare to.</param>
	/// <returns>Whether the <see cref="DiscordRole"/> is equal to this <see cref="DiscordRole"/>.</returns>
	public bool Equals(DiscordRole e)
		=> e switch
		{
			null => false,
			_ => ReferenceEquals(this, e) || this.Id == e.Id
		};

	/// <summary>
	/// Gets the hash code for this <see cref="DiscordRole"/>.
	/// </summary>
	/// <returns>The hash code for this <see cref="DiscordRole"/>.</returns>
	public override int GetHashCode()
		=> this.Id.GetHashCode();

	/// <summary>
	/// Gets whether the two <see cref="DiscordRole"/> objects are equal.
	/// </summary>
	/// <param name="e1">First role to compare.</param>
	/// <param name="e2">Second role to compare.</param>
	/// <returns>Whether the two roles are equal.</returns>
	public static bool operator ==(DiscordRole e1, DiscordRole e2)
		=> e1 is null == e2 is null
		   && ((e1 is null && e2 is null) || e1.Id == e2.Id);

	/// <summary>
	/// Gets whether the two <see cref="DiscordRole"/> objects are not equal.
	/// </summary>
	/// <param name="e1">First role to compare.</param>
	/// <param name="e2">Second role to compare.</param>
	/// <returns>Whether the two roles are not equal.</returns>
	public static bool operator !=(DiscordRole e1, DiscordRole e2)
		=> !(e1 == e2);
}
