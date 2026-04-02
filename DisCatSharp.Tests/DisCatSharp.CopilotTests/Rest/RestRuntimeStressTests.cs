using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Net;

using Xunit;
using Xunit.Abstractions;

namespace DisCatSharp.Copilot.Tests.Rest;

/// <summary>
///     Live REST stress tests that exercise the bucket-owned queue/worker runtime
///     against the real Discord API. Skips instantly when <c>DCS_STRESS_TEST_TOKEN</c>
///     is not set in the environment.
/// </summary>
/// <remarks>
///     Required environment variables:
///     <list type="bullet">
///         <item><c>RUN_STRESS_TEST</c> — Must be set (any value) to enable stress tests.</item>
///         <item><c>DCS_STRESS_TEST_TOKEN</c> — Bot token for bot 822627593110683718.</item>
///     </list>
///     <para>
///         Uses two guilds for cross-guild bucket isolation testing:
///     </para>
///     <list type="bullet">
///         <item>858089274087309313 (AITSYS Internal) — existing channels for read tests.</item>
///         <item>1489031307009720511 (Copilot Test Ground) — ephemeral channels for CRUD tests.</item>
///     </list>
/// </remarks>
public class RestRuntimeStressTests : IAsyncLifetime
{
	private const string TOKEN_ENV = "DCS_STRESS_TEST_TOKEN";
	private const string GATE_ENV = "RUN_STRESS_TEST";
	private const ulong EXPECTED_BOT_ID = 822627593110683718;

	// AITSYS Internal — read-only / existing channels
	private const ulong AITSYS_GUILD_ID = 858089274087309313;
	private const ulong AITSYS_ULTRA_SPAM_CHANNEL_ID = 987522384489762836;
	private const ulong AITSYS_RATELIMIT_CHANNEL_ID = 1334654538484809748;

	// Copilot Test Ground — ephemeral resources (channels created/destroyed per run)
	private const ulong PLAYGROUND_GUILD_ID = 1489031307009720511;

	private readonly ITestOutputHelper _output;
	private DiscordClient? _client;
	private bool _hasToken;

	// Ephemeral channels created in playground guild during setup
	private ulong _playgroundChannelA;
	private ulong _playgroundChannelB;

	public RestRuntimeStressTests(ITestOutputHelper output)
	{
		this._output = output;
	}

	public async Task InitializeAsync()
	{
		// Double gate: both RUN_STRESS_TEST and DCS_STRESS_TEST_TOKEN must be set
		var gate = Environment.GetEnvironmentVariable(GATE_ENV);
		if (string.IsNullOrWhiteSpace(gate))
			return;

		var token = Environment.GetEnvironmentVariable(TOKEN_ENV);
		this._hasToken = !string.IsNullOrWhiteSpace(token);

		if (!this._hasToken)
			return;

		this._client = new(new DiscordConfiguration
		{
			Token = token!,
			TokenType = TokenType.Bot,
			Intents = 0,
			Telemetry =
			{
				EnableSentry = false
			},
			Gateway =
			{
				AutoReconnect = false
			}
		});

		// Validate we're running with the expected bot — safety guard against wrong tokens
		var me = await this._client.ApiClient.GetCurrentUserAsync();
		if (me.Id != EXPECTED_BOT_ID)
		{
			this._output.WriteLine($"Token belongs to bot {me.Id} ({me.Username}), expected {EXPECTED_BOT_ID}. Skipping stress tests.");
			this._client.Dispose();
			this._client = null;
			this._hasToken = false;
			return;
		}

		this._output.WriteLine($"Authenticated as {me.Username} ({me.Id})");

		// Create ephemeral channels in playground guild
		var runId = Guid.NewGuid().ToString("N")[..6];
		var api = this._client.ApiClient;

		var chA = await api.CreateGuildChannelAsync(
			PLAYGROUND_GUILD_ID, $"stress-a-{runId}", ChannelType.Text,
			null, default, null, null, null, false, default, null, null, default, "stress test setup");
		this._playgroundChannelA = chA.Id;

		var chB = await api.CreateGuildChannelAsync(
			PLAYGROUND_GUILD_ID, $"stress-b-{runId}", ChannelType.Text,
			null, default, null, null, null, false, default, null, null, default, "stress test setup");
		this._playgroundChannelB = chB.Id;

		this._output.WriteLine($"Created playground channels: {this._playgroundChannelA}, {this._playgroundChannelB}");
	}

	public async Task DisposeAsync()
	{
		if (this._client is not null && this._hasToken)
		{
			var api = this._client.ApiClient;

			// Tear down ephemeral channels
			if (this._playgroundChannelA != 0)
			{
				try { await api.DeleteChannelAsync(this._playgroundChannelA, "stress test teardown"); }
				catch { /* best-effort */ }
			}

			if (this._playgroundChannelB != 0)
			{
				try { await api.DeleteChannelAsync(this._playgroundChannelB, "stress test teardown"); }
				catch { /* best-effort */ }
			}

			this._output.WriteLine("Playground channels cleaned up");
		}

		this._client?.Dispose();
	}

	// ── Helpers ─────────────────────────────────────────────────────────────

	private void WriteLatencyReport(string label, ConcurrentBag<long> latencies, ConcurrentBag<Exception> errors)
	{
		var sorted = latencies.OrderBy(l => l).ToList();
		if (sorted.Count > 0)
		{
			this._output.WriteLine($"{label} latency distribution ({sorted.Count} samples):");
			this._output.WriteLine($"  Min:  {sorted[0]}ms");
			this._output.WriteLine($"  P50:  {sorted[sorted.Count / 2]}ms");
			this._output.WriteLine($"  P90:  {sorted[(int)(sorted.Count * 0.9)]}ms");
			this._output.WriteLine($"  P99:  {sorted[Math.Min(sorted.Count - 1, (int)(sorted.Count * 0.99))]}ms");
			this._output.WriteLine($"  Max:  {sorted[^1]}ms");
			this._output.WriteLine($"  Avg:  {sorted.Average():F0}ms");
		}

		if (errors.Count > 0)
		{
			this._output.WriteLine($"  Errors: {errors.Count}");
			foreach (var ex in errors.Take(5))
				this._output.WriteLine($"    {ex.GetType().Name}: {ex.Message}");
		}
	}

	private static async Task RunRoute(string name, Func<Task> action, ConcurrentDictionary<string, int> results, ConcurrentBag<(string Route, Exception Error)> errors)
	{
		try
		{
			await action();
			results.AddOrUpdate(name, 1, (_, c) => c + 1);
		}
		catch (Exception ex)
		{
			errors.Add((name, ex));
		}
	}

	// ── Read-only stress tests ──────────────────────────────────────────────

	[Fact]
	public async Task ConcurrentGetChannel_NoSilentLoss()
	{
		if (!this._hasToken)
			return;

		const int requestCount = 50;
		var api = this._client!.ApiClient;
		var latencies = new ConcurrentBag<long>();
		var errors = new ConcurrentBag<Exception>();

		var sw = Stopwatch.StartNew();
		await Task.WhenAll(Enumerable.Range(0, requestCount).Select(async _ =>
		{
			var t = Stopwatch.StartNew();
			try
			{
				await api.GetChannelAsync(AITSYS_ULTRA_SPAM_CHANNEL_ID);
				t.Stop();
				latencies.Add(t.ElapsedMilliseconds);
			}
			catch (Exception ex)
			{
				t.Stop();
				errors.Add(ex);
			}
		}));
		sw.Stop();

		this._output.WriteLine($"GetChannel x{requestCount}: {latencies.Count} ok, {errors.Count} errors, {sw.ElapsedMilliseconds}ms total");
		this.WriteLatencyReport("GetChannel", latencies, errors);

		Assert.Equal(requestCount, latencies.Count + errors.Count);
		Assert.Equal(requestCount, latencies.Count);
	}

	[Fact]
	public async Task MixedRoutes_NoCrossRouteBlocking()
	{
		if (!this._hasToken)
			return;

		var api = this._client!.ApiClient;
		var results = new ConcurrentDictionary<string, int>();
		var errors = new ConcurrentBag<(string Route, Exception Error)>();
		var tasks = new List<Task>();

		var sw = Stopwatch.StartNew();

		// Route 1: Get AITSYS channel (x15)
		for (var i = 0; i < 15; i++)
			tasks.Add(RunRoute("GetChannel:AITSYS", () => api.GetChannelAsync(AITSYS_ULTRA_SPAM_CHANNEL_ID), results, errors));

		// Route 2: Get playground channel (x15) — different guild, proves bucket isolation
		for (var i = 0; i < 15; i++)
			tasks.Add(RunRoute("GetChannel:Playground", () => api.GetChannelAsync(this._playgroundChannelA), results, errors));

		// Route 3: Get AITSYS guild (x10)
		for (var i = 0; i < 10; i++)
			tasks.Add(RunRoute("GetGuild:AITSYS", () => api.GetGuildAsync(AITSYS_GUILD_ID, false), results, errors));

		// Route 4: Get playground guild (x10) — cross-guild
		for (var i = 0; i < 10; i++)
			tasks.Add(RunRoute("GetGuild:Playground", () => api.GetGuildAsync(PLAYGROUND_GUILD_ID, false), results, errors));

		await Task.WhenAll(tasks);
		sw.Stop();

		this._output.WriteLine($"Mixed routes x{tasks.Count}: {sw.ElapsedMilliseconds}ms total");
		foreach (var (route, count) in results.OrderBy(kv => kv.Key))
			this._output.WriteLine($"  {route}: {count} ok");
		foreach (var (route, ex) in errors)
			this._output.WriteLine($"  {route} ERROR: {ex.GetType().Name}: {ex.Message}");

		Assert.Equal(tasks.Count, results.Values.Sum() + errors.Count);
		Assert.Empty(errors);
	}

	// ── Write stress tests (playground guild) ───────────────────────────────

	[Fact]
	public async Task MessageBurst_AllDelivered()
	{
		if (!this._hasToken)
			return;

		const int burstSize = 15;
		var api = this._client!.ApiClient;
		var sentMessages = new ConcurrentBag<DiscordMessage>();
		var errors = new ConcurrentBag<Exception>();
		var testId = Guid.NewGuid().ToString("N")[..8];

		var sw = Stopwatch.StartNew();
		await Task.WhenAll(Enumerable.Range(0, burstSize).Select(async i =>
		{
			try
			{
				var msg = await api.CreateMessageAsync(this._playgroundChannelA, $"[burst {testId}] #{i}");
				sentMessages.Add(msg);
			}
			catch (Exception ex)
			{
				errors.Add(ex);
			}
		}));
		sw.Stop();

		this._output.WriteLine($"Message burst x{burstSize}: {sentMessages.Count} sent, {errors.Count} errors, {sw.ElapsedMilliseconds}ms");
		foreach (var ex in errors)
			this._output.WriteLine($"  ERROR: {ex.GetType().Name}: {ex.Message}");

		// Cleanup
		var deleteErrors = 0;
		foreach (var msg in sentMessages)
		{
			try { await api.DeleteMessageAsync(this._playgroundChannelA, msg.Id, "stress test cleanup"); }
			catch { deleteErrors++; }
		}

		if (deleteErrors > 0)
			this._output.WriteLine($"  Cleanup: {deleteErrors} delete failures (non-fatal)");

		Assert.Equal(burstSize, sentMessages.Count + errors.Count);
		Assert.Equal(burstSize, sentMessages.Count);
	}

	[Fact]
	public async Task RoleChurn_CreateAndDelete()
	{
		if (!this._hasToken)
			return;

		const int roleCount = 8;
		var api = this._client!.ApiClient;
		var createdRoles = new ConcurrentBag<DiscordRole>();
		var createErrors = new ConcurrentBag<Exception>();
		var testId = Guid.NewGuid().ToString("N")[..8];

		// Create roles concurrently in playground guild
		await Task.WhenAll(Enumerable.Range(0, roleCount).Select(async i =>
		{
			try
			{
				var role = await api.CreateGuildRoleAsync(PLAYGROUND_GUILD_ID, $"stress-{testId}-{i}", null, null, false, false, "stress test");
				createdRoles.Add(role);
			}
			catch (Exception ex)
			{
				createErrors.Add(ex);
			}
		}));

		this._output.WriteLine($"Role create x{roleCount}: {createdRoles.Count} ok, {createErrors.Count} errors");

		// Delete roles concurrently
		var deleteErrors = new ConcurrentBag<Exception>();
		await Task.WhenAll(createdRoles.Select(async role =>
		{
			try { await api.DeleteRoleAsync(PLAYGROUND_GUILD_ID, role.Id, "stress test cleanup"); }
			catch (Exception ex) { deleteErrors.Add(ex); }
		}));

		this._output.WriteLine($"Role delete x{createdRoles.Count}: {deleteErrors.Count} errors");

		foreach (var ex in createErrors.Concat(deleteErrors))
			this._output.WriteLine($"  ERROR: {ex.GetType().Name}: {ex.Message}");

		Assert.Equal(roleCount, createdRoles.Count);
		Assert.Empty(deleteErrors);
	}

	// ── Cross-guild mixed stress ────────────────────────────────────────────

	[Fact]
	public async Task CrossGuild_MixedReadWrite_NoSilentLoss()
	{
		if (!this._hasToken)
			return;

		var api = this._client!.ApiClient;
		var results = new ConcurrentDictionary<string, int>();
		var errors = new ConcurrentBag<(string Route, Exception Error)>();
		var messagesToClean = new ConcurrentBag<(ulong ChannelId, ulong MessageId)>();
		var testId = Guid.NewGuid().ToString("N")[..8];
		var tasks = new List<Task>();

		var sw = Stopwatch.StartNew();

		// AITSYS reads
		for (var i = 0; i < 10; i++)
			tasks.Add(RunRoute("Read:AITSYS:Channel", () => api.GetChannelAsync(AITSYS_ULTRA_SPAM_CHANNEL_ID), results, errors));

		for (var i = 0; i < 5; i++)
			tasks.Add(RunRoute("Read:AITSYS:Guild", () => api.GetGuildAsync(AITSYS_GUILD_ID, false), results, errors));

		// Playground reads
		for (var i = 0; i < 10; i++)
			tasks.Add(RunRoute("Read:PG:Channel", () => api.GetChannelAsync(this._playgroundChannelA), results, errors));

		for (var i = 0; i < 5; i++)
			tasks.Add(RunRoute("Read:PG:Guild", () => api.GetGuildAsync(PLAYGROUND_GUILD_ID, false), results, errors));

		// Playground writes — split across both ephemeral channels
		for (var i = 0; i < 5; i++)
		{
			var idx = i;
			tasks.Add(RunRoute("Write:PG:ChA", async () =>
			{
				var msg = await api.CreateMessageAsync(this._playgroundChannelA, $"[xguild {testId}] A#{idx}");
				messagesToClean.Add((this._playgroundChannelA, msg.Id));
			}, results, errors));
		}

		for (var i = 0; i < 5; i++)
		{
			var idx = i;
			tasks.Add(RunRoute("Write:PG:ChB", async () =>
			{
				var msg = await api.CreateMessageAsync(this._playgroundChannelB, $"[xguild {testId}] B#{idx}");
				messagesToClean.Add((this._playgroundChannelB, msg.Id));
			}, results, errors));
		}

		// AITSYS writes to designated spam channel
		for (var i = 0; i < 3; i++)
		{
			var idx = i;
			tasks.Add(RunRoute("Write:AITSYS:Spam", async () =>
			{
				var msg = await api.CreateMessageAsync(AITSYS_RATELIMIT_CHANNEL_ID, $"[xguild {testId}] S#{idx}");
				messagesToClean.Add((AITSYS_RATELIMIT_CHANNEL_ID, msg.Id));
			}, results, errors));
		}

		await Task.WhenAll(tasks);
		sw.Stop();

		this._output.WriteLine($"Cross-guild mixed x{tasks.Count}: {sw.ElapsedMilliseconds}ms");
		foreach (var (route, count) in results.OrderBy(kv => kv.Key))
			this._output.WriteLine($"  {route}: {count} ok");
		foreach (var (route, ex) in errors)
			this._output.WriteLine($"  {route} ERROR: {ex.GetType().Name}: {ex.Message}");

		// Cleanup all messages
		foreach (var (chId, msgId) in messagesToClean)
		{
			try { await api.DeleteMessageAsync(chId, msgId, "stress test cleanup"); }
			catch { /* non-fatal */ }
		}

		Assert.Equal(tasks.Count, results.Values.Sum() + errors.Count);
		Assert.Empty(errors);
	}

	[Fact]
	public async Task CrossGuild_ChannelCrud_FullLifecycle()
	{
		if (!this._hasToken)
			return;

		const int channelCount = 5;
		var api = this._client!.ApiClient;
		var created = new ConcurrentBag<DiscordChannel>();
		var errors = new ConcurrentBag<Exception>();
		var testId = Guid.NewGuid().ToString("N")[..6];

		// Create channels concurrently
		var sw = Stopwatch.StartNew();
		await Task.WhenAll(Enumerable.Range(0, channelCount).Select(async i =>
		{
			try
			{
				var ch = await api.CreateGuildChannelAsync(
					PLAYGROUND_GUILD_ID, $"crud-{testId}-{i}", ChannelType.Text,
					null, default, null, null, null, false, default, null, null, default, "crud stress test");
				created.Add(ch);
			}
			catch (Exception ex)
			{
				errors.Add(ex);
			}
		}));
		var createMs = sw.ElapsedMilliseconds;

		this._output.WriteLine($"Channel create x{channelCount}: {created.Count} ok, {errors.Count} errors, {createMs}ms");

		// Read each channel back (proves they exist + exercises GET after POST)
		var readErrors = 0;
		foreach (var ch in created)
		{
			try { await api.GetChannelAsync(ch.Id); }
			catch { readErrors++; }
		}

		this._output.WriteLine($"Channel read-back: {readErrors} errors");

		// Send a message to each, then delete channel (cascade deletes messages)
		var msgErrors = 0;
		foreach (var ch in created)
		{
			try { await api.CreateMessageAsync(ch.Id, $"[crud-test {testId}] hello from {ch.Name}"); }
			catch { msgErrors++; }
		}

		this._output.WriteLine($"Channel message: {msgErrors} errors");

		// Delete all created channels concurrently
		var deleteErrors = new ConcurrentBag<Exception>();
		await Task.WhenAll(created.Select(async ch =>
		{
			try { await api.DeleteChannelAsync(ch.Id, "crud stress test cleanup"); }
			catch (Exception ex) { deleteErrors.Add(ex); }
		}));
		sw.Stop();

		this._output.WriteLine($"Channel delete: {deleteErrors.Count} errors, {sw.ElapsedMilliseconds}ms total");

		Assert.Equal(channelCount, created.Count);
		Assert.Empty(errors);
		Assert.Equal(0, readErrors);
		Assert.Empty(deleteErrors);
	}

	// ── Queue pressure / latency profiling ──────────────────────────────────

	[Fact]
	public async Task HighConcurrency_CrossGuild_MeasuresLatency()
	{
		if (!this._hasToken)
			return;

		const int requestCount = 100;
		var api = this._client!.ApiClient;
		var aitsysLatencies = new ConcurrentBag<long>();
		var playgroundLatencies = new ConcurrentBag<long>();
		var errors = new ConcurrentBag<Exception>();

		// Fire 50 reads at each guild simultaneously
		await Task.WhenAll(
			Enumerable.Range(0, requestCount / 2).Select(async _ =>
			{
				var t = Stopwatch.StartNew();
				try
				{
					await api.GetChannelAsync(AITSYS_ULTRA_SPAM_CHANNEL_ID);
					t.Stop();
					aitsysLatencies.Add(t.ElapsedMilliseconds);
				}
				catch (Exception ex)
				{
					t.Stop();
					errors.Add(ex);
				}
			}).Concat(Enumerable.Range(0, requestCount / 2).Select(async _ =>
			{
				var t = Stopwatch.StartNew();
				try
				{
					await api.GetChannelAsync(this._playgroundChannelA);
					t.Stop();
					playgroundLatencies.Add(t.ElapsedMilliseconds);
				}
				catch (Exception ex)
				{
					t.Stop();
					errors.Add(ex);
				}
			})));

		this.WriteLatencyReport("AITSYS GetChannel", aitsysLatencies, []);
		this.WriteLatencyReport("Playground GetChannel", playgroundLatencies, []);
		this._output.WriteLine($"Total errors: {errors.Count}");

		Assert.Equal(requestCount, aitsysLatencies.Count + playgroundLatencies.Count + errors.Count);
		Assert.True(errors.Count <= 5, $"Too many errors: {errors.Count}. First: {errors.FirstOrDefault()?.Message}");
	}

	[Fact]
	public async Task WritePressure_DualChannel_MeasuresLatency()
	{
		if (!this._hasToken)
			return;

		const int messagesPerChannel = 8;
		var api = this._client!.ApiClient;
		var latenciesA = new ConcurrentBag<long>();
		var latenciesB = new ConcurrentBag<long>();
		var errors = new ConcurrentBag<Exception>();
		var messagesToClean = new ConcurrentBag<(ulong ChannelId, ulong MessageId)>();
		var testId = Guid.NewGuid().ToString("N")[..8];

		// Burst writes to both playground channels simultaneously
		await Task.WhenAll(
			Enumerable.Range(0, messagesPerChannel).Select(async i =>
			{
				var t = Stopwatch.StartNew();
				try
				{
					var msg = await api.CreateMessageAsync(this._playgroundChannelA, $"[wp {testId}] A#{i}");
					t.Stop();
					latenciesA.Add(t.ElapsedMilliseconds);
					messagesToClean.Add((this._playgroundChannelA, msg.Id));
				}
				catch (Exception ex)
				{
					t.Stop();
					errors.Add(ex);
				}
			}).Concat(Enumerable.Range(0, messagesPerChannel).Select(async i =>
			{
				var t = Stopwatch.StartNew();
				try
				{
					var msg = await api.CreateMessageAsync(this._playgroundChannelB, $"[wp {testId}] B#{i}");
					t.Stop();
					latenciesB.Add(t.ElapsedMilliseconds);
					messagesToClean.Add((this._playgroundChannelB, msg.Id));
				}
				catch (Exception ex)
				{
					t.Stop();
					errors.Add(ex);
				}
			})));

		this.WriteLatencyReport("Channel A writes", latenciesA, []);
		this.WriteLatencyReport("Channel B writes", latenciesB, []);
		this._output.WriteLine($"Total errors: {errors.Count}");

		// Cleanup
		foreach (var (chId, msgId) in messagesToClean)
		{
			try { await api.DeleteMessageAsync(chId, msgId, "stress test cleanup"); }
			catch { /* non-fatal */ }
		}

		var totalOk = latenciesA.Count + latenciesB.Count;
		Assert.Equal(messagesPerChannel * 2, totalOk + errors.Count);
		Assert.True(errors.Count == 0, $"Write errors: {errors.Count}. First: {errors.FirstOrDefault()?.Message}");
	}
}
