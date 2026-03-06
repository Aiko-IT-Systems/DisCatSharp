using DisCatSharp.Voice.Dave;
using DisCatSharp.Voice.Entities.Dave;

using Microsoft.Extensions.Logging.Abstractions;

using Xunit;

namespace DisCatSharp.Copilot.Tests.Dave;

/// <summary>Unit tests for <see cref="DaveSession"/> and <see cref="DaveTransitionTracker"/>.</summary>
public class DaveSessionTests
{
	// -------------------------------------------------------------------------
	// Helpers
	// -------------------------------------------------------------------------

	private static DaveSession CreateSession(int protocolVersion = 1)
		=> new(
			selfUserId: 111_111_111UL,
			channelId: 222_222_222UL,
			protocolVersion: protocolVersion,
			mlsProvider: new NullMlsProvider(),
			encryptorFactory: () => new DaveEncryptor(key => new TestAeadCipher(key)),
			decryptorFactory: () => new DaveDecryptor(key => new TestAeadCipher(key)),
			logger: NullLogger.Instance);

	// -------------------------------------------------------------------------
	// Test 1: Construction transitions to Pending
	// -------------------------------------------------------------------------

	[Fact]
	public void Construction_WithProtocolVersionOne_TransitionsToPending()
	{
		using var session = CreateSession(protocolVersion: 1);
		Assert.Equal(DaveSessionState.Pending, session.State);
	}

	// -------------------------------------------------------------------------
	// Test 2: PrepareKeyPackage transitions to AwaitingResponse
	// -------------------------------------------------------------------------

	[Fact]
	public void HandleExternalSender_TransitionsToAwaitingResponse()
	{
		using var session = CreateSession();
		// Per canonical flow: PrepareKeyPackage (from OP4) transitions to AwaitingResponse;
		// HandleExternalSender (from OP25) only calls SetExternalSender and does not change state.
		session.PrepareKeyPackage();
		Assert.Equal(DaveSessionState.AwaitingResponse, session.State);
	}

	// -------------------------------------------------------------------------
	// Test 3: TryEncrypt in Pending state returns false (passthrough)
	// -------------------------------------------------------------------------

	[Fact]
	public void TryEncrypt_InPendingState_ReturnsFalse()
	{
		using var session = CreateSession();
		Assert.Equal(DaveSessionState.Pending, session.State);

		var ok = session.TryEncrypt([0x01, 0x02, 0x03], ssrc: 0, out _, out _);

		Assert.False(ok);
	}

	// -------------------------------------------------------------------------
	// Test 4: TryEncrypt in Inactive state returns false
	// -------------------------------------------------------------------------

	[Fact]
	public void TryEncrypt_InInactiveState_ReturnsFalse()
	{
		using var session = CreateSession(protocolVersion: 0);
		Assert.Equal(DaveSessionState.Inactive, session.State);

		var ok = session.TryEncrypt([0x01, 0x02], ssrc: 0, out _, out _);

		Assert.False(ok);
	}

	// -------------------------------------------------------------------------
	// Test 5: TryDecrypt with unknown userId returns false
	// -------------------------------------------------------------------------

	[Fact]
	public void TryDecrypt_UnknownUserId_ReturnsFalse()
	{
		using var session = CreateSession();

		var ok = session.TryDecrypt(999_999UL, [0x01, 0x02], out _, out _);

		Assert.False(ok);
	}

	// -------------------------------------------------------------------------
	// Test 6: HandleClientsConnect updates recognized users; old user's decryptor is removed
	// -------------------------------------------------------------------------

	[Fact]
	public void HandleClientsConnect_RemovesDecryptorsForDepartedUsers()
	{
		using var session = CreateSession();

		session.HandleClientsConnect(new VoiceClientsConnectPayload { UserIds = [100UL, 200UL] });
		// Replace the set — only user 300 remains
		session.HandleClientsConnect(new VoiceClientsConnectPayload { UserIds = [300UL] });

		// Users 100 and 200 have no decryptors any more
		Assert.False(session.TryDecrypt(100UL, [0x01], out _, out _));
		Assert.False(session.TryDecrypt(200UL, [0x01], out _, out _));
	}

	// -------------------------------------------------------------------------
	// Test 7: PrepareTransition + ExecuteTransition round-trip — non-zero id
	// -------------------------------------------------------------------------

	[Fact]
	public void PrepareAndExecuteTransition_NonZeroId_ReturnsAckAndUpdatesVersion()
	{
		using var session = CreateSession(protocolVersion: 1);
		session.HandlePrepareTransition(new DavePrepareTransitionPayload { TransitionId = 5, ProtocolVersion = 2 });

		var ack = session.HandleExecuteTransition(new DaveExecuteTransitionPayload { TransitionId = 5 });

		Assert.NotNull(ack);
		Assert.Equal(5, ack!.TransitionId);
		Assert.Equal(2, session.ProtocolVersion);
		Assert.Equal(DaveSessionState.Pending, session.State);
	}

	// -------------------------------------------------------------------------
	// Test 8: HandleExecuteTransition with transitionId=0 returns null (no OP 23)
	// -------------------------------------------------------------------------

	[Fact]
	public void HandleExecuteTransition_TransitionIdZero_ReturnsNull()
	{
		using var session = CreateSession(protocolVersion: 1);
		session.HandlePrepareTransition(new DavePrepareTransitionPayload { TransitionId = 0, ProtocolVersion = 1 });

		var ack = session.HandleExecuteTransition(new DaveExecuteTransitionPayload { TransitionId = 0 });

		Assert.Null(ack);
	}

	// -------------------------------------------------------------------------
	// Test 9: HandleExecuteTransition with unknown transitionId returns null
	// -------------------------------------------------------------------------

	[Fact]
	public void HandleExecuteTransition_UnknownTransitionId_ReturnsNull()
	{
		using var session = CreateSession();

		var ack = session.HandleExecuteTransition(new DaveExecuteTransitionPayload { TransitionId = 99 });

		Assert.Null(ack);
	}

	// -------------------------------------------------------------------------
	// Test 10: HandleExecuteTransition with targetVersion=0 transitions to Inactive
	// -------------------------------------------------------------------------

	[Fact]
	public void HandleExecuteTransition_TargetVersionZero_TransitionsToInactive()
	{
		using var session = CreateSession(protocolVersion: 1);
		session.HandlePrepareTransition(new DavePrepareTransitionPayload { TransitionId = 7, ProtocolVersion = 0 });

		var ack = session.HandleExecuteTransition(new DaveExecuteTransitionPayload { TransitionId = 7 });

		Assert.Null(ack);
		Assert.Equal(DaveSessionState.Inactive, session.State);
	}

	// -------------------------------------------------------------------------
	// Test 11: HandleInvalidCommit from AwaitingResponse transitions to Pending
	// -------------------------------------------------------------------------

	[Fact]
	public void HandleInvalidCommit_FromAwaitingResponse_TransitionsToPending()
	{
		using var session = CreateSession();
		session.PrepareKeyPackage();
		Assert.Equal(DaveSessionState.AwaitingResponse, session.State);

		session.HandleInvalidCommit(new DaveMlsInvalidCommitWelcomePayload { Description = "test failure" });

		Assert.Equal(DaveSessionState.Pending, session.State);
	}

	// -------------------------------------------------------------------------
	// Test 12: Reset transitions to Pending (protocolVersion > 0)
	// -------------------------------------------------------------------------

	[Fact]
	public void Reset_WithProtocolVersionGreaterThanZero_TransitionsToPending()
	{
		using var session = CreateSession(protocolVersion: 1);
		session.PrepareKeyPackage();
		Assert.Equal(DaveSessionState.AwaitingResponse, session.State);

		session.Reset();

		Assert.Equal(DaveSessionState.Pending, session.State);
	}

	// -------------------------------------------------------------------------
	// Test 13: DaveTransitionTracker — Record/TryConsume round-trip
	// -------------------------------------------------------------------------

	[Fact]
	public void TransitionTracker_RecordAndConsume_Succeeds()
	{
		var tracker = new DaveTransitionTracker();
		tracker.Record(1, 42);

		var ok = tracker.TryConsume(1, out var version);

		Assert.True(ok);
		Assert.Equal(42, version);
	}

	// -------------------------------------------------------------------------
	// Test 14: DaveTransitionTracker — double-consume returns false
	// -------------------------------------------------------------------------

	[Fact]
	public void TransitionTracker_DoubleConsume_SecondReturnsFalse()
	{
		var tracker = new DaveTransitionTracker();
		tracker.Record(2, 10);
		tracker.TryConsume(2, out _);

		var ok = tracker.TryConsume(2, out _);

		Assert.False(ok);
	}

	// -------------------------------------------------------------------------
	// Test 15: DaveTransitionTracker — Clear removes all pending entries
	// -------------------------------------------------------------------------

	[Fact]
	public void TransitionTracker_Clear_RemovesAllPending()
	{
		var tracker = new DaveTransitionTracker();
		tracker.Record(1, 1);
		tracker.Record(2, 2);
		tracker.Record(3, 3);

		tracker.Clear();

		Assert.Equal(0, tracker.Count);
		Assert.False(tracker.TryConsume(1, out _));
		Assert.False(tracker.TryConsume(2, out _));
		Assert.False(tracker.TryConsume(3, out _));
	}
}
