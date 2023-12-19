using System;

using DisCatSharp.Entities;
using DisCatSharp.EventArgs;

namespace DisCatSharp.VoiceNext.EventArgs;

/// <summary>
/// Arguments for <see cref="VoiceNextConnection.UserJoined"/>.
/// </summary>
public sealed class VoiceUserJoinEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets the user who left.
	/// </summary>
	public DiscordUser User { get; internal set; }

	/// <summary>
	/// Gets the SSRC of the user who joined.
	/// </summary>
	public uint Ssrc { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="VoiceUserJoinEventArgs"/> class.
	/// </summary>
	internal VoiceUserJoinEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
