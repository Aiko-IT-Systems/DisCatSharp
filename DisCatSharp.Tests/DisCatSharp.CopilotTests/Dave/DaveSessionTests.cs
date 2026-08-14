using System;
using System.Collections.Generic;

using DisCatSharp.Voice.Entities.Dave;
using DisCatSharp.Voice.Enums.Dave;
using DisCatSharp.Voice.Interfaces.Dave;
using DisCatSharp.Voice.Payloads;

using Microsoft.Extensions.Logging.Abstractions;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Xunit;

namespace DisCatSharp.Copilot.Tests.Dave;

/// <summary>
///     Regression tests for DAVE prepare, ready, execute, and recovery ordering.
/// </summary>
public sealed class DaveSessionTests
{
	/// <summary>
	///     Discord user ID used for the local sender in all session tests.
	/// </summary>
	private const ulong SelfUserId = 111_111_111UL;

	/// <summary>
	///     Discord user ID used for one remote media sender.
	/// </summary>
	private const ulong RemoteUserId = 222_222_222UL;

	/// <summary>
	///     Verifies that media readiness is distinct from the MLS control-plane state.
	/// </summary>
	[Fact]
	public void Construction_ReportsMediaReadinessForProtocolZeroOnly()
	{
		using var encrypted = new SessionHarness(protocolVersion: 1);
		using var passthrough = new SessionHarness(protocolVersion: 0);

		Assert.Equal(DaveSessionState.Pending, encrypted.Session.State);
		Assert.False(encrypted.Session.IsActive);
		Assert.False(encrypted.Session.IsMediaReady);
		Assert.Equal(DaveSessionState.Inactive, passthrough.Session.State);
		Assert.False(passthrough.Session.IsActive);
		Assert.True(passthrough.Session.IsMediaReady);
	}

	/// <summary>
	///     Reproduces the captured OP29 transition 28 and verifies OP22 switches only the sender.
	/// </summary>
	[Fact]
	public void AnnounceCommit_NonZeroTransition_StagesReceiversBeforeSenderExecution()
	{
		using var harness = new SessionHarness(protocolVersion: 1);
		harness.ActivateInitialEpoch();
		var receiver = Assert.Single(harness.Decryptors);
		var senderTransitionsBeforePrepare = harness.Encryptor.Transitions.Count;
		var resetCountBeforePrepare = harness.Provider.ResetCount;

		var prepared = harness.Session.HandleAnnounceCommit([0x29], transitionId: 28);

		Assert.Equal(DaveTransitionAction.Ready, prepared.Action);
		Assert.Equal((ushort)28, prepared.TransitionId);
		Assert.Same(receiver, Assert.Single(harness.Decryptors));
		Assert.Equal(2, receiver.Transitions.Count);
		Assert.Equal(senderTransitionsBeforePrepare, harness.Encryptor.Transitions.Count);
		Assert.Equal(resetCountBeforePrepare, harness.Provider.ResetCount);
		Assert.Equal(DaveSessionState.ReadyForTransition, harness.Session.State);
		Assert.True(harness.Session.IsActive);
		Assert.True(harness.Session.IsMediaReady);

		Assert.True(harness.Session.HandleExecuteTransition(new() { TransitionId = 28 }));
		Assert.Equal(senderTransitionsBeforePrepare + 1, harness.Encryptor.Transitions.Count);
		Assert.Equal(resetCountBeforePrepare, harness.Provider.ResetCount);
		Assert.Equal(DaveSessionState.Active, harness.Session.State);
		Assert.True(harness.Session.IsActive);

		Assert.False(harness.Session.HandleExecuteTransition(new() { TransitionId = 28 }));
		Assert.Equal(senderTransitionsBeforePrepare + 1, harness.Encryptor.Transitions.Count);
	}

	/// <summary>
	///     Verifies that OP30 preserves transition ID 27 and waits for OP22 before activating a joining sender.
	/// </summary>
	[Fact]
	public void Welcome_NonZeroTransition_WaitsForExecuteTransition()
	{
		using var harness = new SessionHarness(protocolVersion: 1);
		harness.Session.HandleClientsConnect(new() { UserIds = [RemoteUserId] });
		harness.Provider.SetRatchets(SelfUserId, RemoteUserId);

		var prepared = harness.Session.HandleWelcome([0x30], transitionId: 27);

		Assert.Equal(DaveTransitionAction.Ready, prepared.Action);
		Assert.Equal((ushort)27, prepared.TransitionId);
		Assert.Equal(DaveSessionState.ReadyForTransition, harness.Session.State);
		Assert.False(harness.Session.IsActive);
		Assert.False(harness.Session.IsMediaReady);
		Assert.Empty(harness.Encryptor.Transitions);

		Assert.True(harness.Session.HandleExecuteTransition(new() { TransitionId = 27 }));
		Assert.Equal(DaveSessionState.Active, harness.Session.State);
		Assert.True(harness.Session.IsActive);
		Assert.Single(harness.Encryptor.Transitions);
	}

	/// <summary>
	///     Verifies downgrade ordering: receivers enter passthrough before OP23 and the sender waits for OP22.
	/// </summary>
	[Fact]
	public void PrepareTransition_Downgrade_KeepsOldSenderActiveUntilExecute()
	{
		using var harness = new SessionHarness(protocolVersion: 1);
		harness.ActivateInitialEpoch();
		var receiver = Assert.Single(harness.Decryptors);
		var senderTransitionsBeforePrepare = harness.Encryptor.Transitions.Count;
		var resetsBeforeExecute = harness.Provider.ResetCount;

		var prepared = harness.Session.HandlePrepareTransition(new()
		{
			TransitionId = 7,
			ProtocolVersion = 0
		});

		Assert.Equal(DaveTransitionAction.Ready, prepared.Action);
		Assert.Equal(DaveSessionState.Downgrading, harness.Session.State);
		Assert.True(harness.Session.IsActive);
		Assert.True(harness.Session.IsMediaReady);
		Assert.Equal(senderTransitionsBeforePrepare, harness.Encryptor.Transitions.Count);
		Assert.Equal((true, false), receiver.Transitions[^1]);

		Assert.True(harness.Session.HandleExecuteTransition(new() { TransitionId = 7 }));
		Assert.Equal((true, false), harness.Encryptor.Transitions[^1]);
		Assert.Equal(0, harness.Session.ProtocolVersion);
		Assert.Equal(DaveSessionState.Inactive, harness.Session.State);
		Assert.False(harness.Session.IsActive);
		Assert.True(harness.Session.IsMediaReady);
		Assert.Equal(resetsBeforeExecute + 1, harness.Provider.ResetCount);
	}

	/// <summary>
	///     Verifies that initialization transition ID 0 executes immediately without reporting readiness.
	/// </summary>
	[Fact]
	public void TransitionIdZero_ExecutesImmediatelyWithoutReadyAction()
	{
		using var harness = new SessionHarness(protocolVersion: 1);
		harness.Session.HandleClientsConnect(new() { UserIds = [RemoteUserId] });
		harness.Provider.SetRatchets(SelfUserId, RemoteUserId);

		var result = harness.Session.HandleAnnounceCommit([0x29], transitionId: 0);

		Assert.Equal(DaveTransitionAction.None, result.Action);
		Assert.Equal((ushort)0, result.TransitionId);
		Assert.Equal(DaveSessionState.Active, harness.Session.State);
		Assert.True(harness.Session.IsActive);
		Assert.False(harness.Session.HandleExecuteTransition(new() { TransitionId = 0 }));
	}

	/// <summary>
	///     Verifies a Welcome with initialization ID 0 installs the joining sender immediately.
	/// </summary>
	[Fact]
	public void Welcome_TransitionIdZero_ExecutesImmediatelyWithoutReadyAction()
	{
		using var harness = new SessionHarness(protocolVersion: 1);
		harness.Session.HandleClientsConnect(new() { UserIds = [RemoteUserId] });
		harness.Provider.SetRatchets(SelfUserId, RemoteUserId);

		var result = harness.Session.HandleWelcome([0x30], transitionId: 0);

		Assert.Equal(DaveTransitionAction.None, result.Action);
		Assert.Equal((ushort)0, result.TransitionId);
		Assert.Equal(DaveSessionState.Active, harness.Session.State);
		Assert.True(harness.Session.IsActive);
		Assert.Single(harness.Encryptor.Transitions);
	}

	/// <summary>
	///     Verifies an OP21 downgrade with initialization ID 0 executes immediately without staging OP23.
	/// </summary>
	[Fact]
	public void PrepareTransition_TransitionIdZero_ExecutesDowngradeImmediately()
	{
		using var harness = new SessionHarness(protocolVersion: 1);
		harness.ActivateInitialEpoch();

		var result = harness.Session.HandlePrepareTransition(new()
		{
			TransitionId = 0,
			ProtocolVersion = 0
		});

		Assert.Equal(DaveTransitionAction.None, result.Action);
		Assert.Equal((ushort)0, result.TransitionId);
		Assert.Equal(DaveSessionState.Inactive, harness.Session.State);
		Assert.Equal(0, harness.Session.ProtocolVersion);
		Assert.False(harness.Session.IsActive);
		Assert.True(harness.Session.IsMediaReady);
	}

	/// <summary>
	///     Verifies the sole-member OP24 reset prepares a fresh group while old media remains usable,
	///     then an ID-0 commit immediately installs the replacement sender epoch.
	/// </summary>
	[Fact]
	public void SoleMemberPrepareEpoch_ThenTransitionIdZeroCommit_ExecutesImmediately()
	{
		using var harness = new SessionHarness(protocolVersion: 1);
		harness.Provider.SetRatchets(SelfUserId);
		var initial = harness.Session.HandleAnnounceCommit([0x29], transitionId: 0);
		Assert.Equal(DaveTransitionAction.None, initial.Action);

		var keyPackage = harness.Session.HandlePrepareEpoch(new() { Epoch = 1, ProtocolVersion = 1 });

		Assert.Equal(new byte[] { 0x26 }, keyPackage);
		Assert.Equal(DaveSessionState.AwaitingResponse, harness.Session.State);
		Assert.True(harness.Session.IsActive);
		Assert.True(harness.Session.IsMediaReady);

		var replacement = harness.Session.HandleAnnounceCommit([0x29], transitionId: 0);

		Assert.Equal(DaveTransitionAction.None, replacement.Action);
		Assert.Equal(DaveSessionState.Active, harness.Session.State);
		Assert.True(harness.Session.IsActive);
		Assert.Equal(2, harness.Encryptor.Transitions.Count);
	}

	/// <summary>
	///     Verifies commit and Welcome failures retain their authoritative transition IDs for OP31 recovery.
	/// </summary>
	[Fact]
	public void InvalidCommitAndWelcome_ReturnRecoverActionsWithOriginalIds()
	{
		using var harness = new SessionHarness(protocolVersion: 1);
		harness.ActivateInitialEpoch();
		harness.Provider.CommitOutcome = new() { IsFailed = true };

		var commitResult = harness.Session.HandleAnnounceCommit([0x29], transitionId: 28);
		Assert.Equal(DaveTransitionAction.RecoverInvalid, commitResult.Action);
		Assert.Equal((ushort)28, commitResult.TransitionId);

		harness.Provider.WelcomeSucceeds = false;
		var welcomeResult = harness.Session.HandleWelcome([0x30], transitionId: 27);
		Assert.Equal(DaveTransitionAction.RecoverInvalid, welcomeResult.Action);
		Assert.Equal((ushort)27, welcomeResult.TransitionId);

		var keyPackage = harness.Session.RecoverFromInvalidTransition(welcomeResult.TransitionId);
		Assert.Equal(new byte[] { 0x26 }, keyPackage);
		Assert.Equal(1, harness.Provider.ResetCount);
		Assert.Equal(1, harness.Provider.InitCount);
		Assert.Equal(DaveSessionState.AwaitingResponse, harness.Session.State);
		Assert.True(harness.Session.IsActive);
		Assert.True(harness.Session.IsMediaReady);
	}

	/// <summary>
	///     Verifies OP24's official wire shape and use of its announced protocol version for epoch 1.
	/// </summary>
	[Fact]
	public void PrepareEpoch_EpochOne_HasNoTransitionIdAndUsesPayloadVersion()
	{
		using var harness = new SessionHarness(protocolVersion: 0);
		var payload = new DavePrepareEpochPayload { Epoch = 1, ProtocolVersion = 1 };

		var keyPackage = harness.Session.HandlePrepareEpoch(payload);
		var json = JObject.Parse(JsonConvert.SerializeObject(payload));

		Assert.False(json.ContainsKey("transition_id"));
		Assert.Equal(new byte[] { 0x26 }, keyPackage);
		Assert.Equal((ushort)1, harness.Provider.ProtocolVersion);
		Assert.Equal(1, harness.Provider.InitCount);
		Assert.Equal(0, harness.Session.ProtocolVersion);
	}

	/// <summary>
	///     Verifies OP31 serializes the transition ID required by the whitepaper.
	/// </summary>
	[Fact]
	public void InvalidCommitWelcomePayload_SerializesTransitionId()
	{
		var json = JObject.Parse(JsonConvert.SerializeObject(new DaveMlsInvalidCommitWelcomePayload
		{
			TransitionId = 28
		}));

		Assert.Equal(28, json.Value<int>("transition_id"));
	}

	/// <summary>
	///     Verifies pending transition IDs are consumed exactly once.
	/// </summary>
	[Fact]
	public void TransitionTracker_ConsumesEachIdExactlyOnce()
	{
		var tracker = new DaveTransitionTracker();
		tracker.Record(28, 1);

		Assert.True(tracker.TryConsume(28, out var version));
		Assert.Equal((ushort)1, version);
		Assert.False(tracker.TryConsume(28, out _));
	}

	/// <summary>
	///     Owns a session and recording test doubles used by one regression scenario.
	/// </summary>
	private sealed class SessionHarness : IDisposable
	{
		/// <summary>
		///     Initializes a harness for one executing protocol version.
		/// </summary>
		/// <param name="protocolVersion">The session's initial executing protocol version.</param>
		public SessionHarness(int protocolVersion)
		{
			this.Provider = new();
			this.Encryptor = new();
			this.Session = new(
				selfUserId: SelfUserId,
				protocolVersion: protocolVersion,
				mlsProvider: this.Provider,
				encryptorFactory: () => this.Encryptor,
				decryptorFactory: () =>
				{
					var decryptor = new RecordingDecryptor();
					this.Decryptors.Add(decryptor);
					return decryptor;
				},
				logger: NullLogger.Instance);
		}

		/// <summary>
		///     Gets the session under test.
		/// </summary>
		public DaveSession Session { get; }

		/// <summary>
		///     Gets the recording MLS provider.
		/// </summary>
		public RecordingMlsProvider Provider { get; }

		/// <summary>
		///     Gets the recording sender transform.
		/// </summary>
		public RecordingEncryptor Encryptor { get; }

		/// <summary>
		///     Gets every receiver transform created by the session.
		/// </summary>
		public List<RecordingDecryptor> Decryptors { get; } = [];

		/// <summary>
		///     Establishes transition ID 0 so later tests begin with a live old epoch.
		/// </summary>
		public void ActivateInitialEpoch()
		{
			this.Session.HandleClientsConnect(new() { UserIds = [RemoteUserId] });
			this.Provider.SetRatchets(SelfUserId, RemoteUserId);
			var result = this.Session.HandleAnnounceCommit([0x29], transitionId: 0);
			Assert.Equal(DaveTransitionAction.None, result.Action);
			Assert.True(this.Session.IsActive);
		}

		/// <inheritdoc/>
		public void Dispose()
			=> this.Session.Dispose();
	}

	/// <summary>
	///     Records MLS calls and exports deterministic managed ratchets.
	/// </summary>
	private sealed class RecordingMlsProvider : IMlsProvider
	{
		/// <summary>
		///     Ratchets currently available by Discord user ID.
		/// </summary>
		private readonly Dictionary<ulong, byte[]> _ratchets = [];

		/// <inheritdoc/>
		public bool IsSessionInitialized { get; private set; }

		/// <inheritdoc/>
		public bool IsGroupReady { get; private set; }

		/// <inheritdoc/>
		public ushort ProtocolVersion { get; private set; } = 1;

		/// <summary>
		///     Gets or sets the next commit outcome.
		/// </summary>
		public MlsCommitOutcome CommitOutcome { get; set; }

		/// <summary>
		///     Gets or sets whether the next Welcome succeeds.
		/// </summary>
		public bool WelcomeSucceeds { get; set; } = true;

		/// <summary>
		///     Gets the number of MLS initialization calls.
		/// </summary>
		public int InitCount { get; private set; }

		/// <summary>
		///     Gets the number of MLS reset calls.
		/// </summary>
		public int ResetCount { get; private set; }

		/// <summary>
		///     Makes deterministic ratchets available for the supplied senders.
		/// </summary>
		/// <param name="userIds">The senders whose ratchets should become available.</param>
		public void SetRatchets(params ulong[] userIds)
		{
			foreach (var userId in userIds)
			{
				var secret = new byte[32];
				secret[0] = (byte)userId;
				this._ratchets[userId] = secret;
			}
		}

		/// <inheritdoc/>
		public void InitGroup(ulong selfUserId, int protocolVersion, byte[] groupId)
		{
			this.InitCount++;
			this.IsSessionInitialized = true;
			this.IsGroupReady = false;
			this.ProtocolVersion = checked((ushort)protocolVersion);
		}

		/// <inheritdoc/>
		public void SetExternalSender(byte[] externalSenderBytes) { }

		/// <inheritdoc/>
		public byte[] GetKeyPackage()
			=> [0x26];

		/// <inheritdoc/>
		public MlsCommitResult ProcessProposals(byte[] proposalsBytes, IReadOnlySet<ulong> recognizedUserIds)
			=> new() { CommitBytes = [0x28] };

		/// <inheritdoc/>
		public MlsCommitOutcome ProcessCommit(byte[] commitBytes)
		{
			this.IsGroupReady = !this.CommitOutcome.IsFailed && !this.CommitOutcome.IsIgnored;
			return this.CommitOutcome;
		}

		/// <inheritdoc/>
		public bool ProcessWelcome(byte[] welcomeBytes, byte[] ratchetKey, IReadOnlySet<ulong> recognizedUserIds)
		{
			this.IsGroupReady = this.WelcomeSucceeds;
			return this.WelcomeSucceeds;
		}

		/// <inheritdoc/>
		public DaveRatchetInstaller? GetRatchetInstaller(ulong userId)
			=> this._ratchets.TryGetValue(userId, out var secret)
				? DaveRatchetInstaller.FromManaged(secret)
				: null;

		/// <inheritdoc/>
		public void Reset()
		{
			this.ResetCount++;
			this.IsSessionInitialized = false;
			this.IsGroupReady = false;
		}
	}

	/// <summary>
	///     Records sender ratchet and passthrough transitions without performing encryption.
	/// </summary>
	private sealed class RecordingEncryptor : IDaveEncryptor
	{
		/// <inheritdoc/>
		public bool IsActive { get; private set; }

		/// <summary>
		///     Gets ordered transition records as <c>(passthrough, hasRatchet)</c> pairs.
		/// </summary>
		public List<(bool Passthrough, bool HasRatchet)> Transitions { get; } = [];

		/// <inheritdoc/>
		public bool TryEncrypt(ReadOnlySpan<byte> frame, uint ssrc, out byte[] result, out int resultLength)
		{
			result = null!;
			resultLength = 0;
			return false;
		}

		/// <inheritdoc/>
		public void TransitionTo(DaveRatchetInstaller? installer, bool passthrough)
		{
			this.Transitions.Add((passthrough, installer.HasValue));
			this.IsActive = !passthrough && installer.HasValue;
		}

		/// <inheritdoc/>
		public void Dispose() { }
	}

	/// <summary>
	///     Records receiver ratchet and passthrough transitions while preserving object identity.
	/// </summary>
	private sealed class RecordingDecryptor : IDaveDecryptor
	{
		/// <summary>
		///     Gets ordered transition records as <c>(passthrough, hasRatchet)</c> pairs.
		/// </summary>
		public List<(bool Passthrough, bool HasRatchet)> Transitions { get; } = [];

		/// <inheritdoc/>
		public bool TryDecrypt(ReadOnlySpan<byte> frame, out byte[] result, out int resultLength)
		{
			result = null!;
			resultLength = 0;
			return false;
		}

		/// <inheritdoc/>
		public void TransitionTo(DaveRatchetInstaller? installer, bool passthrough)
			=> this.Transitions.Add((passthrough, installer.HasValue));

		/// <inheritdoc/>
		public void Dispose() { }
	}
}
