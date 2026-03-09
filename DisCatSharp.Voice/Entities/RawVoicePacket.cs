
using System;

namespace DisCatSharp.Voice.Entities;

/// <summary>
///     Represents a raw outbound voice packet before transport framing.
/// </summary>
internal readonly struct RawVoicePacket
{
	/// <summary>
	///     Initializes a new instance of the <see cref="RawVoicePacket" /> class.
	/// </summary>
	/// <param name="bytes">The bytes.</param>
	/// <param name="duration">The duration.</param>
	/// <param name="silence">If true, silence.</param>
	public RawVoicePacket(Memory<byte> bytes, int duration, bool silence)
	{
		this.Bytes = bytes;
		this.Duration = duration;
		this.Silence = silence;
		this.RentedBuffer = null;
	}

	/// <summary>
	///     Initializes a new instance of the <see cref="RawVoicePacket" /> class.
	/// </summary>
	/// <param name="bytes">The bytes.</param>
	/// <param name="duration">The duration.</param>
	/// <param name="silence">If true, silence.</param>
	/// <param name="rentedBuffer">The rented buffer.</param>
	public RawVoicePacket(Memory<byte> bytes, int duration, bool silence, byte[] rentedBuffer)
		: this(bytes, duration, silence)
	{
		this.RentedBuffer = rentedBuffer;
	}

	/// <summary>
	///     Packet payload bytes.
	/// </summary>
	public readonly Memory<byte> Bytes;
	/// <summary>
	///     Packet duration in milliseconds.
	/// </summary>
	public readonly int Duration;
	/// <summary>
	///     Indicates whether this packet contains generated silence.
	/// </summary>
	public readonly bool Silence;

	/// <summary>
	///     Optional rented backing buffer that should be returned to the pool by the consumer when no longer needed.
	/// </summary>
	public readonly byte[] RentedBuffer;
}
