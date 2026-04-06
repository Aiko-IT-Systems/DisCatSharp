using System;

namespace DisCatSharp.Voice.Entities;

/// <summary>
///     Represents a single pre-encoded Opus audio frame from an external source.
/// </summary>
/// <param name="Payload">The Opus-encoded audio payload.</param>
/// <param name="DurationMs">Frame duration in milliseconds (typically 20).</param>
/// <param name="Sequence">Source-side sequence number (informational; VoiceConnection uses its own).</param>
/// <param name="Timestamp">Source-side RTP timestamp (informational; VoiceConnection uses its own).</param>
/// <param name="IsSilence">Whether this frame represents silence (no real audio). Used to control speaking indicators.</param>
public sealed record ExternalOpusFrame(
	ReadOnlyMemory<byte> Payload,
	int DurationMs,
	uint Sequence,
	uint Timestamp,
	bool IsSilence = false);
