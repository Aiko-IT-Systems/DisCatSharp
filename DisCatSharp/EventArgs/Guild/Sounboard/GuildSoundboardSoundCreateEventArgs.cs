using System;

using DisCatSharp.Entities;

namespace DisCatSharp.EventArgs;

/// <summary>
///     Represents arguments for <see cref="DiscordClient.GuildSoundboardSoundCreated" /> event.
/// </summary>
public class GuildSoundboardSoundCreateEventArgs : DiscordEventArgs
{
	/// <summary>
	///     Initializes a new instance of the <see cref="GuildSoundboardSoundCreateEventArgs" /> class.
	/// </summary>
	internal GuildSoundboardSoundCreateEventArgs(IServiceProvider provider)
		: base(provider)
	{ }

	/// <summary>
	///     Gets the guild this sound belongs to.
	/// </summary>
	public DiscordGuild Guild { get; internal set; }

	/// <summary>
	///     Gets the sound.
	/// </summary>
	public DiscordSoundboardSound SoundboardSound { get; internal set; }
}
