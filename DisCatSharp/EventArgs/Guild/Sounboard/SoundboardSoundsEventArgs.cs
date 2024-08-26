using System;
using System.Collections.Generic;

using DisCatSharp.Entities;

namespace DisCatSharp.EventArgs;

/// <summary>
///     Represents arguments for <see cref="DiscordClient.SoundboardSounds" /> event.
/// </summary>
public class SoundboardSoundsEventArgs : DiscordEventArgs
{
	/// <summary>
	///     Initializes a new instance of the <see cref="SoundboardSoundsEventArgs" /> class.
	/// </summary>
	internal SoundboardSoundsEventArgs(IServiceProvider provider)
		: base(provider)
	{ }

	/// <summary>
	///     Gets the guild where the soundboard sounds were received.
	/// </summary>
	public DiscordGuild Guild { get; internal set; }

	/// <summary>
	///     Gets the guild id where the soundboard sounds were received.
	/// </summary>
	internal ulong GuildId { get; set; }

	/// <summary>
	///     Gets the list of soundboard sounds.
	/// </summary>
	public IReadOnlyList<DiscordSoundboardSound> Sounds { get; internal set; }
}
