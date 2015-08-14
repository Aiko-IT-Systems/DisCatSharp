using System;

using DisCatSharp.Entities;

namespace DisCatSharp.EventArgs;

/// <summary>
/// Represents arguments for application command events.
/// </summary>
public sealed class GuildApplicationCommandCountEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets the count of slash commands.
	/// </summary>
	public int SlashCommands { get; internal set; }

	/// <summary>
	/// Gets the count of user context menu commands.
	/// </summary>
	public int UserContextMenuCommands { get; internal set; }

	/// <summary>
	/// Gets the count of message context menu commands.
	/// </summary>
	public int MessageContextMenuCommands { get; internal set; }

	/// <summary>
	/// Gets the guild.
	/// </summary>
	public DiscordGuild Guild { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="GuildApplicationCommandCountEventArgs"/> class.
	/// </summary>
	/// <param name="provider">The provider.</param>
	public GuildApplicationCommandCountEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
