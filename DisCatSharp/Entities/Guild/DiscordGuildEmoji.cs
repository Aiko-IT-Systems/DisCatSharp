using System.Collections.Generic;
using System.Threading.Tasks;

using DisCatSharp.Enums;
using DisCatSharp.Exceptions;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a guild emoji.
/// </summary>
public sealed class DiscordGuildEmoji : DiscordEmoji
{
	/// <summary>
	/// Gets the user that created this emoji.
	/// </summary>
	[JsonIgnore]
	public DiscordUser User { get; internal set; }

	/// <summary>
	/// Gets the guild to which this emoji belongs.
	/// </summary>
	[JsonIgnore]
	public DiscordGuild Guild { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="DiscordGuildEmoji"/> class.
	/// </summary>
	internal DiscordGuildEmoji()
	{ }

	/// <summary>
	/// Modifies this emoji.
	/// </summary>
	/// <param name="name">New name for this emoji.</param>
	/// <param name="roles">Roles for which this emoji will be available. This works only if your application is whitelisted as integration.</param>
	/// <param name="reason">Reason for audit log.</param>
	/// <returns>The modified emoji.</returns>
	/// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageGuildExpressions"/> permission.</exception>
	/// <exception cref="NotFoundException">Thrown when the emoji does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordGuildEmoji> ModifyAsync(string name, IEnumerable<DiscordRole> roles = null, string reason = null)
		=> this.Guild.ModifyEmojiAsync(this, name, roles, reason);

	/// <summary>
	/// Deletes this emoji.
	/// </summary>
	/// <param name="reason">Reason for audit log.</param>
	/// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageGuildExpressions"/> permission.</exception>
	/// <exception cref="NotFoundException">Thrown when the emoji does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task DeleteAsync(string reason = null)
		=> this.Guild.DeleteEmojiAsync(this, reason);
}
