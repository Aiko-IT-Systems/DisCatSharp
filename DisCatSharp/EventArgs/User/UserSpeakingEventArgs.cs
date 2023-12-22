using System;

using DisCatSharp.Entities;
using DisCatSharp.Enums;

namespace DisCatSharp.EventArgs;

/// <summary>
/// Represents arguments for UserSpeaking event.
/// </summary>
public class UserSpeakingEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets the users whose speaking state changed.
	/// </summary>
	public DiscordUser? User { get; internal init; }

	/// <summary>
	/// Gets the SSRC of the audio source.
	/// </summary>
	public uint Ssrc { get; internal init; }

	/// <summary>
	/// Gets whether this user is speaking.
	/// </summary>
	public SpeakingFlags Speaking { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="UserSpeakingEventArgs"/> class.
	/// </summary>
	internal UserSpeakingEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
