namespace DisCatSharp.Voice.Entities;

/// <summary>
///     Metrics derived from an Opus packet header.
/// </summary>
internal struct OpusPacketMetrics
{
	/// <summary>
	///     Number of channels encoded in the packet.
	/// </summary>
	public int ChannelCount { get; set; }

	/// <summary>
	///     Number of frames encoded in the packet.
	/// </summary>
	public int FrameCount { get; set; }

	/// <summary>
	///     Samples per frame for the packet.
	/// </summary>
	public int SamplesPerFrame { get; set; }

	/// <summary>
	///     Total frame size in samples per channel.
	/// </summary>
	public int FrameSize { get; set; }
}
