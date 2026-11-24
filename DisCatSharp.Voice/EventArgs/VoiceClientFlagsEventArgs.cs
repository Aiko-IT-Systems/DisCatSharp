using System;

using DisCatSharp.EventArgs;
using DisCatSharp.Voice.Enums;

namespace DisCatSharp.Voice.EventArgs;

/// <summary>
///     Arguments for <see cref="VoiceConnection.ClientFlagsReceived" />.
/// </summary>
/// <remarks>
///     The flags originate from a user client and are neither verified server state nor recording consent.
/// </remarks>
public sealed class VoiceClientFlagsEventArgs : DiscordEventArgs
{
	/// <summary>
	///     Initializes a new instance of the <see cref="VoiceClientFlagsEventArgs" /> class.
	/// </summary>
	internal VoiceClientFlagsEventArgs(IServiceProvider provider)
		: base(provider)
	{ }

	/// <summary>
	///     Gets the Discord user identifier whose client reported the flags.
	/// </summary>
	public ulong UserId { get; internal set; }

	/// <summary>
	///     Gets the client-reported Discord Clips preferences.
	/// </summary>
	public VoiceClientFlags Flags { get; internal set; }
}
