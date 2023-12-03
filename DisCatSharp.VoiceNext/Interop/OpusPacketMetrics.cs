namespace DisCatSharp.VoiceNext.Interop;

internal struct OpusPacketMetrics
{
	public int ChannelCount { get; set; }

	public int FrameCount { get; set; }

	public int SamplesPerFrame { get; set; }

	public int FrameSize { get; set; }
}
