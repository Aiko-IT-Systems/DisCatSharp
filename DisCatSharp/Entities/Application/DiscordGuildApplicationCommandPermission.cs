using System;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a guild application command permission.
/// </summary>
public sealed class DiscordGuildApplicationCommandPermission : SnowflakeObject, IEquatable<DiscordGuildApplicationCommandPermission>
{
	/// <summary>
	/// Gets the id of the command.
	/// </summary>
	[JsonProperty("id")]
	public new ulong Id { get; set; }

	/// <summary>
	/// Gets the unique ID of this command's application.
	/// </summary>
	[JsonProperty("application_id")]
	public ulong ApplicationId { get; set; }

	/// <summary>
	/// Gets the guild id this permission applies to.
	/// </summary>
	[JsonProperty("guild_id")]
	public ulong GuildId { get; set; }

	/// <summary>
	/// Gets the guild this permission applies to.
	/// </summary>
	[JsonIgnore]
	public DiscordGuild Guild
		=> this.Discord.Guilds.TryGetValue(this.GuildId, out var guild) ? guild : null;

	/// <summary>
	/// Gets the permission array.
	/// </summary>
	[JsonProperty("permissions")]
	public IReadOnlyList<DiscordApplicationCommandPermission> Permissions { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="DiscordGuildApplicationCommandPermission"/> class.
	/// </summary>
	internal DiscordGuildApplicationCommandPermission()
	{ }

	/// <summary>
	/// Checks whether this <see cref="DiscordGuildApplicationCommandPermission"/> object is equal to another object.
	/// </summary>
	/// <param name="other">The command to compare to.</param>
	/// <returns>Whether the command is equal to this <see cref="DiscordGuildApplicationCommandPermission"/>.</returns>
	public bool Equals(DiscordGuildApplicationCommandPermission other)
		=> this.Id == other.Id;

	/// <summary>
	/// Determines if two <see cref="DiscordGuildApplicationCommandPermission"/> objects are equal.
	/// </summary>
	/// <param name="e1">The first command object.</param>
	/// <param name="e2">The second command object.</param>
	/// <returns>Whether the two <see cref="DiscordGuildApplicationCommandPermission"/> objects are equal.</returns>
	public static bool operator ==(DiscordGuildApplicationCommandPermission e1, DiscordGuildApplicationCommandPermission e2)
		=> e1.Equals(e2);

	/// <summary>
	/// Determines if two <see cref="DiscordGuildApplicationCommandPermission"/> objects are not equal.
	/// </summary>
	/// <param name="e1">The first command object.</param>
	/// <param name="e2">The second command object.</param>
	/// <returns>Whether the two <see cref="DiscordGuildApplicationCommandPermission"/> objects are not equal.</returns>
	public static bool operator !=(DiscordGuildApplicationCommandPermission e1, DiscordGuildApplicationCommandPermission e2)
		=> !(e1 == e2);

	/// <summary>
	/// Determines if a <see cref="object"/> is equal to the current <see cref="DiscordApplicationCommand"/>.
	/// </summary>
	/// <param name="other">The object to compare to.</param>
	/// <returns>Whether the two <see cref="DiscordGuildApplicationCommandPermission"/> objects are not equal.</returns>
	public override bool Equals(object other) => other is DiscordGuildApplicationCommandPermission dgacp && this.Equals(dgacp);

	/// <summary>
	/// Gets the hash code for this <see cref="DiscordGuildApplicationCommandPermission"/>.
	/// </summary>
	/// <returns>The hash code for this <see cref="DiscordGuildApplicationCommandPermission"/>.</returns>
	public override int GetHashCode()
		=> this.Id.GetHashCode();
}
