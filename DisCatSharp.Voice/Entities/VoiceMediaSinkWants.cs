using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

using Newtonsoft.Json.Linq;

namespace DisCatSharp.Voice.Entities;

/// <summary>
///     An immutable voice gateway Media Sink Wants snapshot (OP 15).
/// </summary>
/// <remarks>
///     <see cref="Any" /> applies to every SSRC not present in <see cref="SsrcWants" />. Discord accepts values from
///     0 through 100; audio callers normally use 0 to suppress a stream and 100 to receive it. Supplying a snapshot
///     replaces the previous wants at the voice gateway rather than merging with them.
/// </remarks>
public sealed class VoiceMediaSinkWants
{
	/// <summary>
	///     Gets the want value applied to SSRCs absent from <see cref="SsrcWants" />.
	/// </summary>
	public int Any { get; }

	/// <summary>
	///     Gets the want values explicitly associated with remote SSRCs.
	/// </summary>
	public IReadOnlyDictionary<uint, int> SsrcWants { get; }

	/// <summary>
	///     Initializes an immutable Media Sink Wants snapshot.
	/// </summary>
	/// <param name="any">The default want value for unspecified SSRCs, from 0 through 100.</param>
	/// <param name="ssrcWants">Explicit remote SSRC want values, each from 0 through 100.</param>
	public VoiceMediaSinkWants(int any, IReadOnlyDictionary<uint, int>? ssrcWants = null)
	{
		ValidateWant(any, nameof(any));
		var copied = new Dictionary<uint, int>(ssrcWants?.Count ?? 0);
		if (ssrcWants is not null)
		{
			foreach (var (ssrc, want) in ssrcWants)
			{
				if (ssrc == 0)
					throw new ArgumentOutOfRangeException(nameof(ssrcWants), "Remote SSRCs cannot be zero.");

				ValidateWant(want, nameof(ssrcWants));
				copied.Add(ssrc, want);
			}
		}

		this.Any = any;
		this.SsrcWants = new ReadOnlyDictionary<uint, int>(copied);
	}

	/// <summary>
	///     Determines whether another snapshot represents the same complete gateway state.
	/// </summary>
	/// <param name="other">The snapshot to compare.</param>
	/// <returns>Whether both snapshots contain the same default and explicit SSRC wants.</returns>
	internal bool ContentEquals(VoiceMediaSinkWants? other)
		=> other is not null
			&& this.Any == other.Any
			&& this.SsrcWants.Count == other.SsrcWants.Count
			&& this.SsrcWants.All(pair => other.SsrcWants.TryGetValue(pair.Key, out var otherWant) && pair.Value == otherWant);

	/// <summary>
	///     Creates the JSON object used as the data member of a Media Sink Wants gateway dispatch.
	/// </summary>
	/// <returns>The complete JSON object including the default and explicit SSRC wants.</returns>
	internal JObject ToJson()
	{
		var payload = new JObject
		{
			["any"] = this.Any
		};
		foreach (var (ssrc, want) in this.SsrcWants.OrderBy(pair => pair.Key))
			payload[ssrc.ToString(CultureInfo.InvariantCulture)] = want;

		return payload;
	}

	/// <summary>
	///     Validates one Media Sink Want value.
	/// </summary>
	/// <param name="want">The value to validate.</param>
	/// <param name="parameterName">The parameter name to include in an exception.</param>
	internal static void ValidateWant(int want, string parameterName)
	{
		if (want is < 0 or > 100)
			throw new ArgumentOutOfRangeException(parameterName, want, "Media Sink Want values must be between 0 and 100.");
	}
}
