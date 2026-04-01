using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Exceptions;
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
///         <item><c>DCS_STRESS_TEST_TOKEN</c> — Bot token with access to the test guild.</item>
///     </list>
///     Test guild: 858089274087309313 (AITSYS Internal)
///     Channels:
///     - 987522384489762836 (ultra-spam — write tests)
///     - 1334654538484809748 (ratelimit-tests — write tests)
/// </remarks>
public class RestRuntimeStressTests : IAsyncLifetime
{
	private const string TOKEN_ENV = "DCS_STRESS_TEST_TOKEN";
	private const ulong EXPECTED_BOT_ID = 822627593110683718;
	private const ulong TEST_GUILD_ID = 858089274087309313;
	private const ulong ULTRA_SPAM_CHANNEL_ID = 987522384489762836;
	private const ulong RATELIMIT_TESTS_CHANNEL_ID = 1334654538484809748;

	private readonly ITestOutputHelper _output;
	private DiscordClient? _client;
	private bool _hasToken;

	public RestRuntimeStressTests(ITestOutputHelper output)
	{
		this._output = output;
	}

	public async Task InitializeAsync()
	{
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
		}
		else
		{
			this._output.WriteLine($"Authenticated as {me.Username} ({me.Id})");
		}
	}

	public Task DisposeAsync()
	{
		this._client?.Dispose();
		return Task.CompletedTask;
	}

	// ── Read-only stress tests ───────────────────────────────────────────────

	[Fact]
	public async Task ConcurrentGetChannel_NoSilentLoss()
	{
		if (!this._hasToken)
			return;

		const int requestCount = 50;
		var api = this._client!.ApiClient;
		var results = new ConcurrentBag<DiscordChannel>();
		var errors = new ConcurrentBag<Exception>();

		var sw = Stopwatch.StartNew();
		var tasks = Enumerable.Range(0, requestCount).Select(async _ =>
		{
			try
			{
				var ch = await api.GetChannelAsync(ULTRA_SPAM_CHANNEL_ID);
				results.Add(ch);
			}
			catch (Exception ex)
			{
				errors.Add(ex);
			}
		});

		await Task.WhenAll(tasks);
		sw.Stop();

		this._output.WriteLine($"GetChannel x{requestCount}: {results.Count} ok, {errors.Count} errors, {sw.ElapsedMilliseconds}ms");

		foreach (var ex in errors)
			this._output.WriteLine($"  ERROR: {ex.GetType().Name}: {ex.Message}");

		Assert.Equal(requestCount, results.Count + errors.Count); // No silent loss
		Assert.Equal(requestCount, results.Count); // All should succeed
	}

	[Fact]
	public async Task MixedRoutes_NoCrossRouteBlocking()
	{
		if (!this._hasToken)
			return;

		var api = this._client!.ApiClient;
		var results = new ConcurrentDictionary<string, int>();
		var errors = new ConcurrentBag<(string Route, Exception Error)>();

		var sw = Stopwatch.StartNew();

		// Fire requests across 4 different routes simultaneously
		var tasks = new List<Task>();

		// Route 1: Get channel (x15)
		for (var i = 0; i < 15; i++)
			tasks.Add(RunRoute("GetChannel", async () =>
			{
				await api.GetChannelAsync(ULTRA_SPAM_CHANNEL_ID);
			}));

		// Route 2: Get different channel (x15) — same route template, different bucket
		for (var i = 0; i < 15; i++)
			tasks.Add(RunRoute("GetChannel2", async () =>
			{
				await api.GetChannelAsync(RATELIMIT_TESTS_CHANNEL_ID);
			}));

		// Route 3: Get guild (x10)
		for (var i = 0; i < 10; i++)
			tasks.Add(RunRoute("GetGuild", async () =>
			{
				await api.GetGuildAsync(TEST_GUILD_ID, false);
			}));

		// Route 4: Get guild channels (x10)
		for (var i = 0; i < 10; i++)
			tasks.Add(RunRoute("GetGuildChannels", async () =>
			{
				await api.GetGuildChannelsAsync(TEST_GUILD_ID);
			}));

		await Task.WhenAll(tasks);
		sw.Stop();

		this._output.WriteLine($"Mixed routes x{tasks.Count}: {sw.ElapsedMilliseconds}ms total");
		foreach (var (route, count) in results.OrderBy(kv => kv.Key))
			this._output.WriteLine($"  {route}: {count} ok");
		foreach (var (route, ex) in errors)
			this._output.WriteLine($"  {route} ERROR: {ex.GetType().Name}: {ex.Message}");

		var total = results.Values.Sum() + errors.Count;
		Assert.Equal(tasks.Count, total); // No silent loss
		Assert.Empty(errors); // All routes should succeed

		return;

		async Task RunRoute(string name, Func<Task> action)
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
	}

	// ── Write stress tests ──────────────────────────────────────────────────

	[Fact]
	public async Task MessageBurst_AllDelivered()
	{
		if (!this._hasToken)
			return;

		const int burstSize = 10;
		var api = this._client!.ApiClient;
		var sentMessages = new ConcurrentBag<DiscordMessage>();
		var errors = new ConcurrentBag<Exception>();
		var testId = Guid.NewGuid().ToString("N")[..8];

		var sw = Stopwatch.StartNew();

		// Send burst of messages to ultra-spam
		var tasks = Enumerable.Range(0, burstSize).Select(async i =>
		{
			try
			{
				var msg = await api.CreateMessageAsync(ULTRA_SPAM_CHANNEL_ID, $"[stress-test {testId}] burst #{i}");
				sentMessages.Add(msg);
			}
			catch (Exception ex)
			{
				errors.Add(ex);
			}
		});

		await Task.WhenAll(tasks);
		sw.Stop();

		this._output.WriteLine($"Message burst x{burstSize}: {sentMessages.Count} sent, {errors.Count} errors, {sw.ElapsedMilliseconds}ms");

		foreach (var ex in errors)
			this._output.WriteLine($"  ERROR: {ex.GetType().Name}: {ex.Message}");

		// Cleanup — delete all sent messages
		var deleteErrors = 0;
		foreach (var msg in sentMessages)
		{
			try
			{
				await api.DeleteMessageAsync(ULTRA_SPAM_CHANNEL_ID, msg.Id, "stress test cleanup");
			}
			catch
			{
				deleteErrors++;
			}
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

		const int roleCount = 5;
		var api = this._client!.ApiClient;
		var createdRoles = new ConcurrentBag<DiscordRole>();
		var createErrors = new ConcurrentBag<Exception>();
		var testId = Guid.NewGuid().ToString("N")[..8];

		// Create roles concurrently
		var createTasks = Enumerable.Range(0, roleCount).Select(async i =>
		{
			try
			{
				var role = await api.CreateGuildRoleAsync(TEST_GUILD_ID, $"stress-{testId}-{i}", null, null, false, false, "stress test");
				createdRoles.Add(role);
			}
			catch (Exception ex)
			{
				createErrors.Add(ex);
			}
		});

		await Task.WhenAll(createTasks);

		this._output.WriteLine($"Role create x{roleCount}: {createdRoles.Count} ok, {createErrors.Count} errors");

		// Delete roles concurrently
		var deleteErrors = new ConcurrentBag<Exception>();
		var deleteTasks = createdRoles.Select(async role =>
		{
			try
			{
				await api.DeleteRoleAsync(TEST_GUILD_ID, role.Id, "stress test cleanup");
			}
			catch (Exception ex)
			{
				deleteErrors.Add(ex);
			}
		});

		await Task.WhenAll(deleteTasks);

		this._output.WriteLine($"Role delete x{createdRoles.Count}: {deleteErrors.Count} errors");

		foreach (var ex in createErrors.Concat(deleteErrors))
			this._output.WriteLine($"  ERROR: {ex.GetType().Name}: {ex.Message}");

		Assert.Equal(roleCount, createdRoles.Count);
		Assert.Empty(deleteErrors);
	}

	[Fact]
	public async Task MixedReadWrite_NoSilentLoss()
	{
		if (!this._hasToken)
			return;

		var api = this._client!.ApiClient;
		var results = new ConcurrentDictionary<string, int>();
		var errors = new ConcurrentBag<(string Route, Exception Error)>();
		var messagesToClean = new ConcurrentBag<ulong>();
		var testId = Guid.NewGuid().ToString("N")[..8];

		var sw = Stopwatch.StartNew();
		var tasks = new List<Task>();

		// Reads
		for (var i = 0; i < 10; i++)
			tasks.Add(RunRoute("GetChannel", () => api.GetChannelAsync(ULTRA_SPAM_CHANNEL_ID)));

		for (var i = 0; i < 10; i++)
			tasks.Add(RunRoute("GetGuild", () => api.GetGuildAsync(TEST_GUILD_ID, false)));

		// Writes
		for (var i = 0; i < 5; i++)
		{
			var idx = i;
			tasks.Add(RunRoute("SendMessage", async () =>
			{
				var msg = await api.CreateMessageAsync(RATELIMIT_TESTS_CHANNEL_ID, $"[mixed-stress {testId}] #{idx}");
				messagesToClean.Add(msg.Id);
			}));
		}

		await Task.WhenAll(tasks);
		sw.Stop();

		this._output.WriteLine($"Mixed r/w x{tasks.Count}: {sw.ElapsedMilliseconds}ms");
		foreach (var (route, count) in results.OrderBy(kv => kv.Key))
			this._output.WriteLine($"  {route}: {count} ok");
		foreach (var (route, ex) in errors)
			this._output.WriteLine($"  {route} ERROR: {ex.GetType().Name}: {ex.Message}");

		// Cleanup
		foreach (var msgId in messagesToClean)
		{
			try
			{
				await api.DeleteMessageAsync(RATELIMIT_TESTS_CHANNEL_ID, msgId, "stress test cleanup");
			}
			catch
			{
				// Non-fatal
			}
		}

		var total = results.Values.Sum() + errors.Count;
		Assert.Equal(tasks.Count, total);
		Assert.Empty(errors);

		return;

		async Task RunRoute(string name, Func<Task> action)
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
	}

	// ── Queue pressure / observability ──────────────────────────────────────

	[Fact]
	public async Task HighConcurrency_GetChannel_MeasuresLatency()
	{
		if (!this._hasToken)
			return;

		const int requestCount = 100;
		var api = this._client!.ApiClient;
		var latencies = new ConcurrentBag<long>();
		var errors = new ConcurrentBag<Exception>();

		var tasks = Enumerable.Range(0, requestCount).Select(async _ =>
		{
			var sw = Stopwatch.StartNew();
			try
			{
				await api.GetChannelAsync(ULTRA_SPAM_CHANNEL_ID);
				sw.Stop();
				latencies.Add(sw.ElapsedMilliseconds);
			}
			catch (Exception ex)
			{
				sw.Stop();
				errors.Add(ex);
			}
		});

		await Task.WhenAll(tasks);

		var sorted = latencies.OrderBy(l => l).ToList();
		if (sorted.Count > 0)
		{
			this._output.WriteLine($"GetChannel x{requestCount} latency distribution:");
			this._output.WriteLine($"  Min:  {sorted[0]}ms");
			this._output.WriteLine($"  P50:  {sorted[sorted.Count / 2]}ms");
			this._output.WriteLine($"  P90:  {sorted[(int)(sorted.Count * 0.9)]}ms");
			this._output.WriteLine($"  P99:  {sorted[(int)(sorted.Count * 0.99)]}ms");
			this._output.WriteLine($"  Max:  {sorted[^1]}ms");
			this._output.WriteLine($"  Avg:  {sorted.Average():F0}ms");
		}

		this._output.WriteLine($"  Errors: {errors.Count}");

		Assert.Equal(requestCount, latencies.Count + errors.Count);
		Assert.True(errors.Count <= 5, $"Too many errors: {errors.Count}. First: {errors.FirstOrDefault()?.Message}");
	}
}
