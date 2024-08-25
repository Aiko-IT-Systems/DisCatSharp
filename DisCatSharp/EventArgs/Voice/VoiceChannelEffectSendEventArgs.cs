using System;

using DisCatSharp.Entities;
using DisCatSharp.Enums;

namespace DisCatSharp.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.VoiceChannelEffectSend"/> event.
/// </summary>
public class VoiceChannelEffectSendEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets the user whose voice state was updated.
	/// </summary>
	public DiscordUser User { get; internal set; }

	/// <summary>
	/// Gets the guild in which the update occurred.
	/// </summary>
	public DiscordGuild Guild { get; internal set; }

	/// <summary>
	/// Gets the related voice channel.
	/// </summary>
	public DiscordChannel Channel { get; internal set; }

	/// <summary>
	/// Gets the emoji sent, for emoji reactions and soundboard effects.
	/// </summary>
	public DiscordEmoji? Emoji { get; internal set; }

	/// <summary>
	/// Gets the type of emoji animation, for emoji reactions and soundboard effects.
	/// </summary>
	public AnimationType? AnimationType { get; internal set; }

	/// <summary>
	/// Gets the ID of the emoji animation, for emoji reactions and soundboard effects.
	/// </summary>
	public int? AnimationId { get; internal set; }

	/// <summary>
	/// Gets the ID of the soundboard sound, for soundboard effects.
	/// </summary>
	public ulong? SoundId { get; internal set; }

	/// <summary>
	/// Gets the volume of the soundboard sound, from 0 to 1, for soundboard effects.
	/// </summary>
	public double? SoundVolume { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="VoiceStateUpdateEventArgs"/> class.
	/// </summary>
	internal VoiceChannelEffectSendEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
