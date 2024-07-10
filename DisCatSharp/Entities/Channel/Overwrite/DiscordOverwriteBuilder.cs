using System.Collections.Generic;
using System.Linq;

using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a Discord permission overwrite builder.
/// </summary>
public sealed class DiscordOverwriteBuilder
{
	/// <summary>
	/// Gets or sets the allowed permissions for this overwrite.
	/// </summary>
	public Permissions Allowed { get; set; }

	/// <summary>
	/// Gets or sets the denied permissions for this overwrite.
	/// </summary>
	public Permissions Denied { get; set; }

	/// <summary>
	/// Sets all unset permissions for this overwrite.
	/// </summary>
	internal Permissions Unset
	{
		set => this.Remove(value);
	}

	/// <summary>
	/// Gets or sets the type of this overwrite's target.
	/// </summary>
	public OverwriteType Type { get; set; }

	/// <summary>
	/// Gets or sets the target for this overwrite.
	/// </summary>
	public ulong Target { get; set; }

	/// <summary>
	/// Creates a new <see cref="DiscordOverwriteBuilder"/> for a <see cref="SnowflakeObject"/>.
	/// </summary>
	public DiscordOverwriteBuilder(ulong id, OverwriteType type = OverwriteType.Member)
	{
		this.Target = id;
		this.Type = type;
	}

	/// <summary>
	/// Creates a new <see cref="DiscordOverwriteBuilder"/> for a <see cref="DiscordMember"/>.
	/// </summary>
	public DiscordOverwriteBuilder(DiscordMember member)
	{
		this.Target = member.Id;
		this.Type = OverwriteType.Member;
	}

	/// <summary>
	/// Creates a new <see cref="DiscordOverwriteBuilder"/> for a <see cref="DiscordRole"/>.
	/// </summary>
	public DiscordOverwriteBuilder(DiscordRole role)
	{
		this.Target = role.Id;
		this.Type = OverwriteType.Role;
	}

	/// <summary>
	/// Creates a new <see cref="DiscordOverwriteBuilder"/> from <see cref="DiscordOverwrite"/>.
	/// </summary>
	public DiscordOverwriteBuilder(DiscordOverwrite old)
	{
		this.Allowed = old.Allowed;
		this.Denied = old.Denied;
		this.Type = old.Type;
		this.Target = old.Id;
	}

	/// <summary>
	/// Creates a new and empty <see cref="DiscordOverwriteBuilder"/>.
	/// </summary>
	public DiscordOverwriteBuilder()
	{ }

	/// <summary>
	/// Allows a permission for this overwrite.
	/// </summary>
	/// <param name="permission">Permission or permission set to allow for this overwrite.</param>
	/// <returns>This builder.</returns>
	public DiscordOverwriteBuilder Allow(Permissions permission)
	{
		this.Remove(permission);
		this.Allowed |= permission;
		return this;
	}

	/// <summary>
	/// Denies a permission for this overwrite.
	/// </summary>
	/// <param name="permission">Permission or permission set to deny for this overwrite.</param>
	/// <returns>This builder.</returns>
	public DiscordOverwriteBuilder Deny(Permissions permission)
	{
		this.Remove(permission);
		this.Denied |= permission;
		return this;
	}

	/// <summary>
	/// Unsets a permission for this overwrite.
	/// </summary>
	/// <param name="permission">Permission or permission set to unset for this overwrite.</param>
	/// <returns>This builder.</returns>
	public DiscordOverwriteBuilder Remove(Permissions permission)
	{
		this.Allowed = this.Allowed.Revoke(permission);
		this.Denied = this.Denied.Revoke(permission);
		return this;
	}

	/// <summary>
	/// Sets the member to which this overwrite applies.
	/// </summary>
	/// <param name="member">Member to which apply this overwrite's permissions.</param>
	/// <returns>This builder.</returns>
	public DiscordOverwriteBuilder SetTarget(DiscordMember member)
	{
		this.Target = member.Id;
		this.Type = OverwriteType.Member;
		return this;
	}

	/// <inheritdoc cref="SetTarget(DiscordMember)"/>
	public DiscordOverwriteBuilder For(DiscordMember member)
		=> this.SetTarget(member);

	/// <summary>
	/// Sets the role to which this overwrite applies.
	/// </summary>
	/// <param name="role">Role to which apply this overwrite's permissions.</param>
	/// <returns>This builder.</returns>
	public DiscordOverwriteBuilder SetTarget(DiscordRole role)
	{
		this.Target = role.Id;
		this.Type = OverwriteType.Role;
		return this;
	}

	/// <inheritdoc cref="SetTarget(DiscordRole)"/>
	public DiscordOverwriteBuilder For(DiscordRole role)
		=> this.SetTarget(role);

	/// <summary>
	/// Builds this DiscordOverwrite.
	/// </summary>
	/// <returns>Use this object for creation of new overwrites.</returns>
	internal DiscordRestOverwrite Build()
		=> new()
		{
			Allow = this.Allowed,
			Deny = this.Denied,
			Id = this.Target,
			Type = this.Type
		};
}

public static class DiscordOverwriteBuilderExtensions
{
	/// <summary>
	/// Merges new permissions for a target with target's existing permissions.
	/// </summary>
	/// <param name="builderList">The PermissionOverwrites to apply this to.</param>
	/// <param name="type">What type of overwrite you want to target.</param>
	/// <param name="target">The target's id.</param>
	/// <param name="allowed">The permissions to allow.</param>
	/// <param name="denied">The permissions deny.</param>
	/// <param name="unset">The permissions to unset.</param>
	/// <returns>A new <see cref="List{DiscordOverwriteBuilder}"/> containing the merged role.</returns>
	public static List<DiscordOverwriteBuilder> Merge(this IEnumerable<DiscordOverwriteBuilder> builderList, OverwriteType type, ulong target, Permissions allowed, Permissions denied, Permissions unset = Permissions.None)
	{
		var newList = builderList.ToList();

		if (!newList.Any(x => x.Target == target && x.Type == type))
			newList.Add(new()
			{
				Type = type,
				Target = target
			});

		var discordOverwriteBuilder = newList.First(x => x.Target == target && x.Type == type);
		discordOverwriteBuilder.Allow(allowed);
		discordOverwriteBuilder.Deny(denied);
		discordOverwriteBuilder.Remove(unset);
		return newList;
	}

	/// <summary>
	/// Merges new permissions for member with member's existing permissions.
	/// </summary>
	/// <param name="builderList">The PermissionOverwrites to apply this to.</param>
	/// <param name="member">The member of which to modify their permissions.</param>
	/// <param name="allowed">The permissions to allow.</param>
	/// <param name="denied">The permissions to deny.</param>
	/// <param name="unset">The permissions to unset.</param>
	/// <returns>A new <see cref="List{DiscordOverwriteBuilder}"/> containing the merged member.</returns>
	public static List<DiscordOverwriteBuilder> Merge(this IEnumerable<DiscordOverwriteBuilder> builderList, DiscordMember member, Permissions allowed, Permissions denied, Permissions unset = Permissions.None)
		=> builderList.Merge(OverwriteType.Member, member.Id, allowed, denied, unset);

	/// <summary>
	/// Merges new permissions for role with role's existing permissions.
	/// </summary>
	/// <param name="builderList">The PermissionOverwrites to apply this to.</param>
	/// <param name="role">The role of which to modify their permissions.</param>
	/// <param name="allowed">The permissions to allow.</param>
	/// <param name="denied">The permissions deny.</param>
	/// <param name="unset">The permissions to unset.</param>
	/// <returns>A new <see cref="List{DiscordOverwriteBuilder}"/> containing the merged role.</returns>
	public static List<DiscordOverwriteBuilder> Merge(this IEnumerable<DiscordOverwriteBuilder> builderList, DiscordRole role, Permissions allowed, Permissions denied, Permissions unset = Permissions.None)
		=> builderList.Merge(OverwriteType.Role, role.Id, allowed, denied, unset);

	/// <inheritdoc cref="Merge(IEnumerable{DiscordOverwriteBuilder}, DiscordMember, Permissions, Permissions, Permissions)"/>
	public static List<DiscordOverwriteBuilder> Merge(this IEnumerable<DiscordOverwrite> builderList, DiscordMember member, Permissions allowed, Permissions denied, Permissions unset = Permissions.None)
		=> builderList.Select(x => new DiscordOverwriteBuilder(x)).Merge(OverwriteType.Member, member.Id, allowed, denied, unset);

	/// <inheritdoc cref="Merge(IEnumerable{DiscordOverwriteBuilder}, DiscordRole, Permissions, Permissions, Permissions)"/>
	public static List<DiscordOverwriteBuilder> Merge(this IEnumerable<DiscordOverwrite> builderList, DiscordRole role, Permissions allowed, Permissions denied, Permissions unset = Permissions.None)
		=> builderList.Select(x => new DiscordOverwriteBuilder(x)).Merge(OverwriteType.Role, role.Id, allowed, denied, unset);

	/// <inheritdoc cref="Merge(IEnumerable{DiscordOverwriteBuilder}, OverwriteType, ulong, Permissions, Permissions, Permissions)"/>
	public static List<DiscordOverwriteBuilder> Merge(this IEnumerable<DiscordOverwrite> builderList, OverwriteType type, ulong target, Permissions allowed, Permissions denied, Permissions unset = Permissions.None)
		=> builderList.Select(x => new DiscordOverwriteBuilder(x)).Merge(type, target, allowed, denied, unset);
}

internal struct DiscordRestOverwrite
{
	/// <summary>
	/// Determines what is allowed.
	/// </summary>
	[JsonProperty("allow", NullValueHandling = NullValueHandling.Ignore)]
	internal Permissions Allow { get; set; }

	/// <summary>
	/// Determines what is denied.
	/// </summary>
	[JsonProperty("deny", NullValueHandling = NullValueHandling.Ignore)]
	internal Permissions Deny { get; set; }

	/// <summary>
	/// Gets or sets the id.
	/// </summary>
	[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
	internal ulong Id { get; set; }

	/// <summary>
	/// Gets or sets the overwrite type.
	/// </summary>
	[JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
	internal OverwriteType Type { get; set; }
}
