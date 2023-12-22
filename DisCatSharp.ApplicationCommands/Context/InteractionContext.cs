using System.Collections.Generic;

using DisCatSharp.Entities;

namespace DisCatSharp.ApplicationCommands.Context;

/// <summary>
/// Represents a context for an interaction.
/// </summary>
public sealed class InteractionContext : BaseContext
{
	/// <summary>
	/// Gets the users mentioned in the command parameters.
	/// </summary>
	public IReadOnlyList<DiscordUser> ResolvedUserMentions { get; internal set; } = [];

	/// <summary>
	/// Gets the roles mentioned in the command parameters.
	/// </summary>
	public IReadOnlyList<DiscordRole> ResolvedRoleMentions { get; internal set; } = [];

	/// <summary>
	/// Gets the channels mentioned in the command parameters.
	/// </summary>
	public IReadOnlyList<DiscordChannel> ResolvedChannelMentions { get; internal set; } = [];

	/// <summary>
	/// Gets the attachments in the command parameters, if applicable.
	/// </summary>
	public IReadOnlyList<DiscordAttachment> ResolvedAttachments { get; internal set; } = [];
}
