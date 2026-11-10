using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using DisCatSharp.Voice.Enums;

namespace DisCatSharp.Voice.Entities;

/// <summary>
///     Maintains the desired media-sink mode and current Discord-user-to-SSRC mappings for one voice connection.
/// </summary>
internal sealed class VoiceMediaSinkState
{
	/// <summary>
	///     Synchronizes mutable desired policy and mapping state.
	/// </summary>
	private readonly Lock _syncRoot = new();

	/// <summary>
	///     Maps remote Discord users to their current voice SSRCs.
	/// </summary>
	private readonly Dictionary<ulong, uint> _userSsrcs = [];

	/// <summary>
	///     Gets or sets the configured user-oriented policy, when not in raw mode.
	/// </summary>
	private VoiceMediaReceivePolicy _policy;

	/// <summary>
	///     Gets or sets the explicit raw snapshot, when raw mode is selected.
	/// </summary>
	private VoiceMediaSinkWants? _rawWants;

	/// <summary>
	///     Initializes state from an explicit configuration policy or the historical local incoming-audio setting.
	/// </summary>
	/// <param name="policy">An explicit configured policy, if any.</param>
	/// <param name="enableIncoming">Whether local incoming audio is enabled when no explicit policy is configured.</param>
	public VoiceMediaSinkState(VoiceMediaReceivePolicy? policy, bool enableIncoming)
		=> this._policy = policy ?? (enableIncoming ? VoiceMediaReceivePolicy.ReceiveAll() : VoiceMediaReceivePolicy.ReceiveNone());

	/// <summary>
	///     Replaces the current state with a user-oriented policy.
	/// </summary>
	/// <param name="policy">The new policy.</param>
	/// <returns>The full gateway snapshot that implements the policy.</returns>
	public VoiceMediaSinkWants SetPolicy(VoiceMediaReceivePolicy policy)
	{
		lock (this._syncRoot)
		{
			this._policy = policy;
			this._rawWants = null;
			return this.BuildWants();
		}
	}

	/// <summary>
	///     Replaces the current state with a raw gateway snapshot.
	/// </summary>
	/// <param name="wants">The raw snapshot to retain and reapply.</param>
	/// <returns>The retained snapshot.</returns>
	public VoiceMediaSinkWants SetRawWants(VoiceMediaSinkWants wants)
	{
		lock (this._syncRoot)
		{
			this._rawWants = wants;
			return wants;
		}
	}

	/// <summary>
	///     Updates one remote user's current SSRC.
	/// </summary>
	/// <param name="userId">The remote Discord user ID.</param>
	/// <param name="ssrc">The user's current SSRC.</param>
	/// <returns>Whether the mapping changed.</returns>
	public bool SetUserSsrc(ulong userId, uint ssrc)
	{
		if (userId == 0 || ssrc == 0)
			return false;

		lock (this._syncRoot)
		{
			if (this._userSsrcs.TryGetValue(userId, out var existing) && existing == ssrc)
				return false;

			this._userSsrcs[userId] = ssrc;
			return this._rawWants is null;
		}
	}

	/// <summary>
	///     Removes a remote user's current SSRC mapping.
	/// </summary>
	/// <param name="userId">The remote Discord user ID.</param>
	/// <returns>Whether a managed gateway snapshot changed.</returns>
	public bool RemoveUser(ulong userId)
	{
		lock (this._syncRoot)
			return this._userSsrcs.Remove(userId) && this._rawWants is null;
	}

	/// <summary>
	///     Clears mappings that belonged to a superseded voice-gateway session.
	/// </summary>
	/// <returns>Whether a managed gateway snapshot may have changed.</returns>
	public bool ResetMappings()
	{
		lock (this._syncRoot)
		{
			var changed = this._userSsrcs.Count > 0;
			this._userSsrcs.Clear();
			return changed && this._rawWants is null;
		}
	}

	/// <summary>
	///     Gets the complete raw gateway snapshot represented by the current state.
	/// </summary>
	/// <returns>The desired Media Sink Wants payload state.</returns>
	public VoiceMediaSinkWants GetWants()
	{
		lock (this._syncRoot)
			return this._rawWants ?? this.BuildWants();
	}

	/// <summary>
	///     Builds a raw gateway snapshot from the managed policy and known SSRC mappings.
	/// </summary>
	/// <returns>A complete Media Sink Wants snapshot.</returns>
	private VoiceMediaSinkWants BuildWants()
	{
		return this._policy.Kind switch
		{
			VoiceMediaReceivePolicyKind.ReceiveAll => new(100),
			VoiceMediaReceivePolicyKind.ReceiveNone => new(0),
			VoiceMediaReceivePolicyKind.OnlyUsers => new(0, this._userSsrcs
				.Where(pair => this._policy.UserIds.Contains(pair.Key))
				.ToDictionary(pair => pair.Value, _ => 100)),
			VoiceMediaReceivePolicyKind.ExceptUsers => new(100, this._userSsrcs
				.Where(pair => this._policy.UserIds.Contains(pair.Key))
				.ToDictionary(pair => pair.Value, _ => 0)),
			_ => throw new InvalidOperationException($"Unsupported {nameof(VoiceMediaReceivePolicyKind)} value: {this._policy.Kind}.")
		};
	}
}
