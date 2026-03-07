using System;

using DisCatSharp.Entities;
using DisCatSharp.EventArgs;

namespace DisCatSharp.Voice.EventArgs;

/// <summary>
///     Represents arguments for dropped inbound voice packet notifications.
/// </summary>
public sealed class VoicePacketDroppedEventArgs : DiscordEventArgs
{
	/// <summary>
	///     Initializes a new instance of the <see cref="VoicePacketDroppedEventArgs"/> class.
	/// </summary>
	internal VoicePacketDroppedEventArgs(IServiceProvider provider)
		: base(provider)
	{ }

	/// <summary>
	///     Gets the SSRC associated with the dropped packet.
	/// </summary>
	public uint Ssrc { get; internal set; }

	/// <summary>
	///     Gets the user associated with the dropped packet, when known.
	/// </summary>
	public DiscordUser? User { get; internal set; }

	/// <summary>
	///     Gets the wrapped sequence number for the dropped packet, when known.
	/// </summary>
	public ulong? Sequence { get; internal set; }

	/// <summary>
	///     Gets the packet-drop classification.
	/// </summary>
	public VoicePacketDropReason Reason { get; internal set; }

	/// <summary>
	///     Gets optional diagnostic details.
	/// </summary>
	public string? Detail { get; internal set; }
}
