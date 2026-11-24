using System;

using DisCatSharp.EventArgs;
using DisCatSharp.Voice.Entities;

namespace DisCatSharp.Voice.EventArgs;

/// <summary>
///     Arguments for <see cref="VoiceConnection.MediaSinkWantsReceived" />.
/// </summary>
public sealed class VoiceMediaSinkWantsEventArgs : DiscordEventArgs
{
	/// <summary>
	///     Initializes a new instance of the <see cref="VoiceMediaSinkWantsEventArgs" /> class.
	/// </summary>
	internal VoiceMediaSinkWantsEventArgs(IServiceProvider provider)
		: base(provider)
	{ }

	/// <summary>
	///     Gets the complete media wants requested by Discord's voice server for outbound media.
	/// </summary>
	public VoiceMediaSinkWants Wants { get; internal set; } = null!;
}
