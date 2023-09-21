using System;

namespace DisCatSharp.VoiceNext;

internal readonly struct RawVoicePacket
{
	/// <summary>
	/// Initializes a new instance of the <see cref="RawVoicePacket"/> class.
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
	/// Initializes a new instance of the <see cref="RawVoicePacket"/> class.
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

	public readonly Memory<byte> Bytes;
	public readonly int Duration;
	public readonly bool Silence;

	public readonly byte[] RentedBuffer;
}
