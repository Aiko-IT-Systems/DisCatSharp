using System;

using DisCatSharp.Entities;
using DisCatSharp.VoiceNext.Codec;

namespace DisCatSharp.VoiceNext.Entities;

/// <summary>
/// The audio sender.
/// </summary>
internal class AudioSender : IDisposable
{
	// starting the counter a full wrap ahead handles an edge case where the VERY first packets
	// we see are right around the wraparound line.
	private ulong _sequenceBase = 1 << 16;

	private SequenceWrapState _currentSequenceWrapState = SequenceWrapState.AssumeNextHighSequenceIsOutOfOrder;

	private enum SequenceWrapState
	{
		Normal,
		AssumeNextLowSequenceIsOverflow,
		AssumeNextHighSequenceIsOutOfOrder
	}

	/// <summary>
	/// Gets the s s r c.
	/// </summary>
	public uint Ssrc { get; }

	/// <summary>
	/// Gets the id.
	/// </summary>
	public ulong Id => this.User?.Id ?? 0;

	/// <summary>
	/// Gets the decoder.
	/// </summary>
	public OpusDecoder Decoder { get; }

	/// <summary>
	/// Gets or sets the user.
	/// </summary>
	public DiscordUser? User { get; set; } = null;

	/// <summary>
	/// Gets or sets the last sequence.
	/// </summary>
	public ulong? LastTrueSequence { get; set; } = null;

	/// <summary>
	/// Initializes a new instance of the <see cref="AudioSender"/> class.
	/// </summary>
	/// <param name="ssrc">The ssrc.</param>
	/// <param name="decoder">The decoder.</param>
	public AudioSender(uint ssrc, OpusDecoder decoder)
	{
		this.Ssrc = ssrc;
		this.Decoder = decoder;
	}

	/// <summary>
	/// Disposes .
	/// </summary>
	public void Dispose()
		=> this.Decoder?.Dispose();

	/// <summary>
	/// Accepts the 16-bit sequence number from the next RTP header in the associated stream and
	/// uses heuristics to (attempt to) convert it into a 64-bit counter that takes into account
	/// overflow wrapping around to zero.
	/// <para/>
	/// This method only works properly if it is called for <b>every</b> sequence number that we
	/// see in the stream.
	/// </summary>
	/// <param name="originalSequence">
	/// The 16-bit sequence number from the next RTP header.
	/// </param>
	/// <returns>
	/// Our best-effort guess of the value that <paramref name="originalSequence"/> <b>would</b>
	/// have been, if the server had given us a 64-bit integer instead of a 16-bit one.
	/// </returns>
	public ulong GetTrueSequenceAfterWrapping(ushort originalSequence)
	{
		// section off a smallish zone at either end of the 16-bit integer range.  whenever the
		// sequence numbers creep into the higher zone, we start keeping an eye out for when
		// sequence numbers suddenly start showing up in the lower zone.  we expect this to mean
		// that the sequence numbers overflowed and wrapped around.  there's a bit of a balance
		// when determining an appropriate size for the buffer zone: if it's too small, then a
		// brief (but recoverable) network interruption could cause us to miss the lead-up to
		// the overflow.  on the other hand, if it's too large, then such a network interruption
		// could cause us to misinterpret a normal sequence for one that's out-of-order.
		//
		// at 20 milliseconds per packet, 3,000 packets means that the buffer zone is one minute
		// on either side.  in other words, as long as we're getting packets delivered within a
		// minute or so of when they should be, the 64-bit sequence numbers coming out of this
		// method will be perfectly consistent with reality.
		const ushort OVERFLOW_BUFFER_ZONE = 3_000;
		const ushort HIGH_THRESHOLD = ushort.MaxValue - OVERFLOW_BUFFER_ZONE;

		ulong wrappingAdjustment = 0;
		switch (this._currentSequenceWrapState)
		{
			case SequenceWrapState.Normal when originalSequence > HIGH_THRESHOLD:
				// we were going about our business up to this point.  the sequence numbers have
				// gotten a bit high, so let's start looking out for any sequence numbers that
				// are suddenly WAY lower than where they are right now.
				this._currentSequenceWrapState = SequenceWrapState.AssumeNextLowSequenceIsOverflow;
				break;

			case SequenceWrapState.AssumeNextLowSequenceIsOverflow when originalSequence < OVERFLOW_BUFFER_ZONE:
				// we had seen some sequence numbers that got a bit high, and now we see this
				// sequence number that's WAY lower than before.  this is a classic sign that
				// the sequence numbers have wrapped around.  in order to present a consistently
				// increasing "true" sequence number, add another 65,536 and keep counting.  if
				// we see another high sequence number in the near future, assume that it's a
				// packet coming in out of order.
				this._sequenceBase += 1 << 16;
				this._currentSequenceWrapState = SequenceWrapState.AssumeNextHighSequenceIsOutOfOrder;
				break;

			case SequenceWrapState.AssumeNextHighSequenceIsOutOfOrder when originalSequence > HIGH_THRESHOLD:
				// we're seeing some high sequence numbers EITHER at the beginning of the stream
				// OR very close to the time when we saw some very low sequence numbers.  in the
				// latter case, it happened because the packets came in out of order, right when
				// the sequence numbers wrapped around.  in the former case, we MIGHT be in the
				// same kind of situation (we can't tell yet), so we err on the side of caution
				// and burn a full cycle before we start counting so that we can handle both
				// cases with the exact same adjustment.
				wrappingAdjustment = 1 << 16;
				break;

			case SequenceWrapState.AssumeNextHighSequenceIsOutOfOrder when originalSequence > OVERFLOW_BUFFER_ZONE:
				// EITHER we're at the very beginning of the stream OR very close to the time
				// when we saw some very low sequence numbers.  either way, we're out of the
				// zones where we should consider very low sequence numbers to come AFTER very
				// high ones, so we can go back to normal now.
				this._currentSequenceWrapState = SequenceWrapState.Normal;
				break;
			default:
				throw new ArgumentOutOfRangeException(null, "CurrentSequenceWrapState was out of range");
		}

		return this._sequenceBase + originalSequence - wrappingAdjustment;
	}
}
