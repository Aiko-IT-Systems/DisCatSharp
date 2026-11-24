using System;

using DisCatSharp.EventArgs;

namespace DisCatSharp.Voice.EventArgs;

/// <summary>
///     Arguments for <see cref="VoiceConnection.VoiceBackendVersionReceived" />.
/// </summary>
public sealed class VoiceBackendVersionEventArgs : DiscordEventArgs
{
	/// <summary>
	///     Initializes a new instance of the <see cref="VoiceBackendVersionEventArgs" /> class.
	/// </summary>
	internal VoiceBackendVersionEventArgs(IServiceProvider provider)
		: base(provider)
	{ }

	/// <summary>
	///     Gets the informational voice backend version.
	/// </summary>
	public string VoiceVersion { get; internal set; } = string.Empty;

	/// <summary>
	///     Gets the informational RTC worker backend version.
	/// </summary>
	public string RtcWorkerVersion { get; internal set; } = string.Empty;
}
