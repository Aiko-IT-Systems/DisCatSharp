using System;
using System.Collections.Generic;

using DisCatSharp.Entities;

namespace DisCatSharp.EventArgs;

/// <summary>
///     Represents arguments for <see cref="DiscordClient.GuildSoundboardSoundsUpdated" /> event.
/// </summary>
public class GuildSoundboardSoundsUpdateEventArgs : DiscordEventArgs
{
	/// <summary>
	///     Initializes a new instance of the <see cref="GuildSoundboardSoundsUpdateEventArgs" /> class.
	/// </summary>
	internal GuildSoundboardSoundsUpdateEventArgs(IServiceProvider provider)
		: base(provider)
	{ }

	/// <summary>
	///     Gets the guild this sound belongs to.
	/// </summary>
	public DiscordGuild Guild { get; internal set; }

	/// <summary>
	///     Gets the sounds.
	/// </summary>
	public IReadOnlyList<DiscordSoundboardSound> SoundboardSounds { get; internal set; }
}
