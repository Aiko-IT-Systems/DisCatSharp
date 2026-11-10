using System;
using System.Collections.Generic;
using System.Collections.Frozen;
using System.Linq;

using DisCatSharp.Voice.Enums;

namespace DisCatSharp.Voice.Entities;

/// <summary>
///     A persistent, user-oriented policy for selecting which inbound voice media Discord forwards to this connection.
/// </summary>
/// <remarks>
///     Discord's voice gateway identifies media streams by SSRC rather than Discord user ID. The owning
///     <see cref="VoiceConnection" /> resolves this policy against the current voice-session mappings and updates
///     the gateway whenever those mappings change.
/// </remarks>
public sealed class VoiceMediaReceivePolicy
{
	/// <summary>
	///     Gets the selection behavior represented by this policy.
	/// </summary>
	public VoiceMediaReceivePolicyKind Kind { get; }

	/// <summary>
	///     Gets the Discord user IDs selected by an <see cref="VoiceMediaReceivePolicyKind.OnlyUsers" /> or
	///     <see cref="VoiceMediaReceivePolicyKind.ExceptUsers" /> policy.
	/// </summary>
	public IReadOnlySet<ulong> UserIds { get; }

	/// <summary>
	///     Initializes a new immutable media receive policy.
	/// </summary>
	/// <param name="kind">The selection behavior.</param>
	/// <param name="userIds">The user IDs selected by user-oriented behaviors.</param>
	private VoiceMediaReceivePolicy(VoiceMediaReceivePolicyKind kind, IEnumerable<ulong>? userIds = null)
	{
		this.Kind = kind;
		var ids = new HashSet<ulong>(userIds ?? Array.Empty<ulong>());
		if (ids.Contains(0))
			throw new ArgumentOutOfRangeException(nameof(userIds), "Discord user IDs cannot be zero.");

		this.UserIds = ids.ToFrozenSet();
	}

	/// <summary>
	///     Creates a policy that receives media from every remote user.
	/// </summary>
	/// <returns>A policy that forwards all remote media.</returns>
	public static VoiceMediaReceivePolicy ReceiveAll()
		=> new(VoiceMediaReceivePolicyKind.ReceiveAll);

	/// <summary>
	///     Creates a policy that receives no remote media.
	/// </summary>
	/// <returns>A policy that suppresses all remote media.</returns>
	public static VoiceMediaReceivePolicy ReceiveNone()
		=> new(VoiceMediaReceivePolicyKind.ReceiveNone);

	/// <summary>
	///     Creates a policy that receives media only from the specified Discord users.
	/// </summary>
	/// <param name="userIds">The Discord users whose media should be forwarded.</param>
	/// <returns>A policy that allow-lists the specified users.</returns>
	public static VoiceMediaReceivePolicy OnlyUsers(IEnumerable<ulong> userIds)
	{
		ArgumentNullException.ThrowIfNull(userIds);
		return new(VoiceMediaReceivePolicyKind.OnlyUsers, userIds);
	}

	/// <summary>
	///     Creates a policy that receives media from every user except the specified Discord users.
	/// </summary>
	/// <param name="userIds">The Discord users whose media should be suppressed.</param>
	/// <returns>A policy that block-lists the specified users.</returns>
	public static VoiceMediaReceivePolicy ExceptUsers(IEnumerable<ulong> userIds)
	{
		ArgumentNullException.ThrowIfNull(userIds);
		return new(VoiceMediaReceivePolicyKind.ExceptUsers, userIds);
	}
}
