using System;

using DisCatSharp.Entities;

namespace DisCatSharp.EventArgs;

/// <summary>
///     Represents arguments for <see cref="DiscordClient.GuildSoundboardSoundUpdated" /> event.
/// </summary>
public class GuildSoundboardSoundUpdateEventArgs : DiscordEventArgs
{
	/// <summary>
	///     Initializes a new instance of the <see cref="GuildSoundboardSoundUpdateEventArgs" /> class.
	/// </summary>
	internal GuildSoundboardSoundUpdateEventArgs(IServiceProvider provider)
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
