using System;

using DisCatSharp.Entities;
using DisCatSharp.EventArgs;

namespace DisCatSharp.VoiceNext.EventArgs;

/// <summary>
/// Arguments for <see cref="VoiceNextConnection.UserLeft"/>.
/// </summary>
public sealed class VoiceUserLeaveEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets the user who left.
	/// </summary>
	public DiscordUser User { get; internal set; }

	/// <summary>
	/// Gets the SSRC of the user who left.
	/// </summary>
	public uint Ssrc { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="VoiceUserLeaveEventArgs"/> class.
	/// </summary>
	internal VoiceUserLeaveEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
