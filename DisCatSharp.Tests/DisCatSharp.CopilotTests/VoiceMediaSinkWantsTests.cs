using System;
using System.Collections.Generic;

using DisCatSharp.Voice.Entities;

using Newtonsoft.Json.Linq;

using Xunit;

namespace DisCatSharp.CopilotTests;

/// <summary>
///     Regression coverage for voice gateway Media Sink Wants policy state.
/// </summary>
public sealed class VoiceMediaSinkWantsTests
{
	/// <summary>
	///     Verifies that raw snapshots validate values and defensively copy their input mappings.
	/// </summary>
	[Fact]
	public void RawWantsValidateAndCopyInput()
	{
		var source = new Dictionary<uint, int> { [2] = 100 };
		var wants = new VoiceMediaSinkWants(0, source);
		source[2] = 0;

		Assert.Equal(100, wants.SsrcWants[2]);
		Assert.Throws<ArgumentOutOfRangeException>(() => new VoiceMediaSinkWants(-1));
		Assert.Throws<ArgumentOutOfRangeException>(() => new VoiceMediaSinkWants(101));
		Assert.Throws<ArgumentOutOfRangeException>(() => new VoiceMediaSinkWants(0, new Dictionary<uint, int> { [2] = 101 }));
		Assert.Throws<ArgumentOutOfRangeException>(() => new VoiceMediaSinkWants(0, new Dictionary<uint, int> { [0] = 100 }));
	}

	/// <summary>
	///     Verifies raw wants serialize as Discord's string-keyed complete OP 15 map.
	/// </summary>
	[Fact]
	public void RawWantsSerializeAsStringKeyedMap()
	{
		var json = new VoiceMediaSinkWants(0, new Dictionary<uint, int> { [42] = 100, [2] = 50 }).ToJson();

		Assert.Equal(0, json.Value<int>("any"));
		Assert.Equal(100, json.Value<int>("42"));
		Assert.Equal(50, json.Value<int>("2"));
	}

	/// <summary>
	///     Verifies policy factories reject invalid user IDs and preserve an immutable user snapshot.
	/// </summary>
	[Fact]
	public void UserPoliciesValidateAndCopyInput()
	{
		var users = new List<ulong> { 10 };
		var policy = VoiceMediaReceivePolicy.OnlyUsers(users);
		users.Clear();

		Assert.Contains(10UL, policy.UserIds);
		Assert.Throws<ArgumentOutOfRangeException>(() => VoiceMediaReceivePolicy.ExceptUsers([0]));
	}

	/// <summary>
	///     Verifies an allow-list tracks current user SSRCs and discards stale session mappings.
	/// </summary>
	[Fact]
	public void OnlyUsersTracksCurrentSsrcMappings()
	{
		var state = new VoiceMediaSinkState(VoiceMediaReceivePolicy.OnlyUsers([10]), enableIncoming: true);
		Assert.Equal(0, state.GetWants().Any);
		Assert.Empty(state.GetWants().SsrcWants);

		Assert.True(state.SetUserSsrc(10, 2));
		Assert.Equal(100, state.GetWants().SsrcWants[2]);
		Assert.True(state.SetUserSsrc(10, 3));
		Assert.DoesNotContain(2U, state.GetWants().SsrcWants.Keys);
		Assert.Equal(100, state.GetWants().SsrcWants[3]);
		Assert.True(state.ResetMappings());
		Assert.Empty(state.GetWants().SsrcWants);
	}

	/// <summary>
	///     Verifies block-list policies and raw mode retain the appropriate complete snapshot.
	/// </summary>
	[Fact]
	public void ExceptUsersAndRawModeUseExpectedSnapshots()
	{
		var state = new VoiceMediaSinkState(VoiceMediaReceivePolicy.ExceptUsers([10]), enableIncoming: true);
		Assert.True(state.SetUserSsrc(10, 2));
		Assert.Equal(100, state.GetWants().Any);
		Assert.Equal(0, state.GetWants().SsrcWants[2]);

		var raw = new VoiceMediaSinkWants(25, new Dictionary<uint, int> { [99] = 75 });
		state.SetRawWants(raw);
		Assert.False(state.SetUserSsrc(10, 3));
		Assert.True(state.GetWants().ContentEquals(raw));
	}

	/// <summary>
	///     Verifies omitted configuration policies continue to derive their network behavior from local receive enablement.
	/// </summary>
	[Theory]
	[InlineData(true, 100)]
	[InlineData(false, 0)]
	public void DefaultPolicyFollowsIncomingEnablement(bool enableIncoming, int expectedAny)
	{
		var state = new VoiceMediaSinkState(null, enableIncoming);
		Assert.Equal(expectedAny, state.GetWants().Any);
		Assert.Empty(state.GetWants().SsrcWants);
	}
}
