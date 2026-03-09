using System;

namespace DisCatSharp.Voice.Entities;

/// <summary>
///     Represents an encoded voice packet together with duration metadata.
/// </summary>
internal struct VoicePacket
{
	/// <summary>
	///     Gets the bytes.
	/// </summary>
	public ReadOnlyMemory<byte> Bytes { get; }

	/// <summary>
	///     Gets the millisecond duration.
	/// </summary>
	public int MillisecondDuration { get; }

	/// <summary>
	///     Gets or sets a value indicating whether is silence.
	/// </summary>
	public bool IsSilence { get; set; }

	/// <summary>
	///     Initializes a new instance of the <see cref="VoicePacket" /> class.
	/// </summary>
	/// <param name="bytes">The bytes.</param>
	/// <param name="msDuration">The ms duration.</param>
	/// <param name="isSilence">If true, is silence.</param>
	public VoicePacket(ReadOnlyMemory<byte> bytes, int msDuration, bool isSilence = false)
	{
		this.Bytes = bytes;
		this.MillisecondDuration = msDuration;
		this.IsSilence = isSilence;
	}
}
