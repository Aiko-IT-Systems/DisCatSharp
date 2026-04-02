using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

using DisCatSharp.Common.Utilities;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.EventArgs;
using DisCatSharp.Net.Abstractions;
using DisCatSharp.Net.WebSocket;
using DisCatSharp.Telemetry;

using Newtonsoft.Json.Linq;

using Xunit;

namespace DisCatSharp.Copilot.Tests.Store;

public class StoreGatewayDispatchFollowupRegressionTests
{
	[Fact]
	public async Task MessageReactionHandlers_BackfillGuildIdsForFallbackMessages()
	{
		var client = CreateClient();
		client.CurrentUser = new DiscordUser
		{
			Id = 42,
			Discord = client,
			Username = "Current user",
			Discriminator = "0001"
		};

		var guildId = 804032421678153819UL;
		var channelId = 9001UL;
		var emoji = new DiscordEmoji
		{
			Id = 777,
			Name = "wave",
			Discord = client
		};

		MessageReactionAddEventArgs? added = null;
		MessageReactionRemoveEventArgs? removed = null;
		client.MessageReactionAdded += (_, args) =>
		{
			added = args;
			return Task.CompletedTask;
		};
		client.MessageReactionRemoved += (_, args) =>
		{
			removed = args;
			return Task.CompletedTask;
		};

		await client.OnMessageReactionAddAsync(100, 200, channelId, guildId, null!, emoji, false);
		await client.OnMessageReactionRemoveAsync(100, 201, channelId, guildId, emoji, false);

		Assert.NotNull(added);
		Assert.NotNull(removed);
		Assert.Equal(guildId, added!.Message.GuildId);
		Assert.Equal(guildId, removed!.Message.GuildId);
	}

	[Fact]
	public async Task MessageDeleteHandlers_BackfillGuildIdsForFallbackMessages()
	{
		var client = CreateClient();
		var guildId = 804032421678153819UL;
		var channelId = 9001UL;

		MessageDeleteEventArgs? deleted = null;
		MessageBulkDeleteEventArgs? bulkDeleted = null;
		client.MessageDeleted += (_, args) =>
		{
			deleted = args;
			return Task.CompletedTask;
		};
		client.MessagesBulkDeleted += (_, args) =>
		{
			bulkDeleted = args;
			return Task.CompletedTask;
		};

		await client.OnMessageDeleteEventAsync(200, channelId, guildId);
		await client.OnMessageBulkDeleteEventAsync([201, 202], channelId, guildId);

		Assert.NotNull(deleted);
		Assert.Equal(guildId, deleted!.Message.GuildId);
		Assert.NotNull(bulkDeleted);
		Assert.All(bulkDeleted!.Messages, message => Assert.Equal(guildId, message.GuildId));
	}

	[Fact]
	public async Task MessageReactionRemoveEmojiEvent_MissingGuildCache_DoesNotThrowAndBackfillsGuildId()
	{
		var client = CreateClient();
		var guildId = 804032421678153819UL;
		var channelId = 9001UL;
		var partialEmoji = new DiscordEmoji
		{
			Id = 777,
			Name = "wave"
		};

		MessageReactionRemoveEmojiEventArgs? captured = null;
		client.MessageReactionRemovedEmoji += (_, args) =>
		{
			captured = args;
			return Task.CompletedTask;
		};

		var exception = await Record.ExceptionAsync(() => client.OnMessageReactionRemoveEmojiAsync(200, channelId, guildId, partialEmoji));

		Assert.Null(exception);
		Assert.NotNull(captured);
		Assert.Equal(guildId, captured!.Message.GuildId);
		Assert.Same(partialEmoji, captured.Emoji);
	}

	[Fact]
	public async Task MessageUpdateEvent_RefreshesMutableCachedFieldsThatAppearInPayload()
	{
		var client = CreateClient();
		var guild = new DiscordGuild
		{
			Id = 804032421678153819,
			Discord = client
		};
		var channel = new DiscordChannel
		{
			Id = 9001,
			Discord = client,
			GuildId = guild.Id,
			Name = "updates"
		};
		var mentionedChannel = new DiscordChannel
		{
			Id = 9002,
			Discord = client,
			GuildId = guild.Id,
			Name = "mentions"
		};
		var oldRole = new DiscordRole
		{
			Id = 101,
			Discord = client,
			GuildId = guild.Id,
			Name = "old-role"
		};
		var newRole = new DiscordRole
		{
			Id = 102,
			Discord = client,
			GuildId = guild.Id,
			Name = "new-role"
		};
		var oldMention = new DiscordUser
		{
			Id = 201,
			Discord = client,
			Username = "old-user",
			Discriminator = "0001"
		};
		var newMention = new DiscordUser
		{
			Id = 202,
			Discord = client,
			Username = "new-user",
			Discriminator = "0001"
		};
		client.GuildsInternal[guild.Id] = guild;
		guild.ChannelsInternal[channel.Id] = channel;
		guild.ChannelsInternal[mentionedChannel.Id] = mentionedChannel;
		guild.RolesInternal[oldRole.Id] = oldRole;
		guild.RolesInternal[newRole.Id] = newRole;

		var cachedMessage = new DiscordMessage
		{
			Id = 500,
			ChannelId = channel.Id,
			GuildId = guild.Id,
			Discord = client,
			Content = "old content <#9002>",
			EditedTimestampRaw = "2025-01-01T00:00:00+00:00",
			Components = [new DiscordButtonComponent(ButtonStyle.Primary, "old-button", "Old button")],
			MentionEveryone = false,
			Pinned = false,
			IsTts = false,
			Flags = MessageFlags.Crossposted
		};
		cachedMessage.AttachmentsInternal.Add(new DiscordAttachment
		{
			Id = 1,
			Filename = "old.txt"
		});
		cachedMessage.EmbedsInternal.Add(new DiscordEmbed
		{
			Title = "Old embed"
		});
		cachedMessage.MentionedUsersInternal.Add(oldMention);
		cachedMessage.MentionedRoleIds = [oldRole.Id];
		cachedMessage.MentionedRolesInternal.Add(oldRole);
		cachedMessage.MentionedChannelsInternal.Add(channel);
		cachedMessage.Poll = new DiscordPoll
		{
			AllowMultiselect = false,
			ChannelId = channel.Id,
			MessageId = 500
		};
		client.MessageCache!.Add(cachedMessage);

		var incomingMessage = new DiscordMessage
		{
			Id = cachedMessage.Id,
			ChannelId = channel.Id,
			GuildId = guild.Id,
			Content = $"new content <#{mentionedChannel.Id}> <@&{newRole.Id}>",
			EditedTimestampRaw = "2025-02-02T00:00:00+00:00",
			Components = [new DiscordButtonComponent(ButtonStyle.Success, "new-button", "New button")],
			MentionEveryone = true,
			Pinned = true,
			IsTts = true,
			Flags = MessageFlags.SuppressNotifications
		};
		incomingMessage.AttachmentsInternal.Add(new DiscordAttachment
		{
			Id = 2,
			Filename = "new.txt"
		});
		incomingMessage.EmbedsInternal.Add(new DiscordEmbed
		{
			Title = "New embed"
		});
		incomingMessage.MentionedUsersInternal.Add(newMention);
		incomingMessage.MentionedRoleIds = [newRole.Id];
		incomingMessage.MentionedRolesInternal.Add(newRole);
		incomingMessage.MentionedChannelsInternal.Add(mentionedChannel);
		incomingMessage.Poll = new DiscordPoll
		{
			AllowMultiselect = true
		};

		MessageUpdateEventArgs? captured = null;
		client.MessageUpdated += (_, args) =>
		{
			captured = args;
			return Task.CompletedTask;
		};

		var rawData = new JObject
		{
			["content"] = incomingMessage.Content,
			["edited_timestamp"] = incomingMessage.EditedTimestampRaw,
			["attachments"] = new JArray(),
			["embeds"] = new JArray(),
			["components"] = new JArray(),
			["mention_everyone"] = incomingMessage.MentionEveryone,
			["mentions"] = new JArray(),
			["mention_roles"] = new JArray(newRole.Id),
			["mention_channels"] = new JArray(),
			["flags"] = (long)incomingMessage.Flags.Value,
			["pinned"] = incomingMessage.Pinned,
			["tts"] = incomingMessage.IsTts,
			["poll"] = new JObject()
		};

		await client.OnMessageUpdateEventAsync(incomingMessage, rawData, null!, null!, null!, null!);

		Assert.NotNull(captured);
		Assert.Same(cachedMessage, captured!.Message);
		Assert.Equal(incomingMessage.Content, cachedMessage.Content);
		Assert.Equal(incomingMessage.EditedTimestampRaw, cachedMessage.EditedTimestampRaw);
		Assert.Single(cachedMessage.AttachmentsInternal);
		Assert.Equal((ulong?)2, cachedMessage.AttachmentsInternal[0].Id);
		Assert.Single(cachedMessage.EmbedsInternal);
		Assert.Equal("New embed", cachedMessage.EmbedsInternal[0].Title);
		Assert.Single(cachedMessage.Components);
		Assert.True(cachedMessage.MentionEveryone);
		Assert.True(cachedMessage.Pinned);
		Assert.True(cachedMessage.IsTts);
		Assert.Equal(MessageFlags.SuppressNotifications, cachedMessage.Flags);
		Assert.Single(cachedMessage.MentionedUsersInternal);
		Assert.Equal(newMention.Id, captured.MentionedUsers[0].Id);
		Assert.Single(cachedMessage.MentionedRolesInternal);
		Assert.Equal(newRole.Id, captured.MentionedRoles![0].Id);
		Assert.Single(cachedMessage.MentionedChannelsInternal);
		Assert.Equal(mentionedChannel.Id, captured.MentionedChannels![0].Id);
		Assert.NotNull(cachedMessage.Poll);
		Assert.True(cachedMessage.Poll!.AllowMultiselect);
		Assert.Equal(channel.Id, cachedMessage.Poll.ChannelId);
		Assert.Equal(cachedMessage.Id, cachedMessage.Poll.MessageId);
	}

	[Fact]
	public async Task InviteDeleteEvent_MissingGuildCache_DoesNotThrowForFallbackInvite()
	{
		var client = CreateClient();
		var guildId = 804032421678153819UL;
		var channelId = 9001UL;
		var payload = new JObject
		{
			["code"] = "discatsharp",
			["guild_id"] = guildId,
			["channel_id"] = channelId
		};

		InviteDeleteEventArgs? captured = null;
		client.InviteDeleted += (_, args) =>
		{
			captured = args;
			return Task.CompletedTask;
		};

		var exception = await Record.ExceptionAsync(() => client.OnInviteDeleteEventAsync(channelId, guildId, payload));

		Assert.Null(exception);
		Assert.NotNull(captured);
		Assert.Equal(guildId, captured!.Guild.Id);
		Assert.Equal("discatsharp", captured.Invite.Code);
		Assert.True(captured.Invite.IsRevoked);
	}

	[Fact]
	public async Task HandleDispatchSafelyAsync_DispatchFailure_CapturesDiagnosticsSinkException()
	{
		var client = CreateClient();
		var sink = new TestDiagnosticsSink();
		client.DiagnosticsSink = sink;

		// Use a null EventName so HandleDispatchAsync crashes immediately at
		// EventName.ToLowerInvariant() — avoids the full invite_create deserialization
		// pipeline that behaves differently across platforms.
		var payload = new GatewayPayload
		{
			OpCode = GatewayOpCode.Dispatch,
			EventName = null!,
			Sequence = 42,
			Data = JObject.Parse("""{"placeholder":true}""")
		};

		await client.HandleDispatchSafelyAsync(payload);

		var captured = await sink.WaitForExceptionAsync();

		Assert.NotNull(captured.Exception);
		Assert.IsType<NullReferenceException>(captured.Exception);
		Assert.Equal("DisCatSharp", captured.Source);
		Assert.Equal("unknown", captured.Tags["dcs.gateway_event"]);
		Assert.Equal("0", captured.Tags["dcs.gateway_opcode"]);
		Assert.Equal(DiagnosticTags.OriginLibrary, captured.Tags[DiagnosticTags.ErrorOrigin]);
		Assert.Equal("unknown", captured.Context["event_name"]);
		Assert.Equal(0, captured.Context["opcode"]);
		Assert.Equal(42, captured.Context["sequence"]);
	}

	[Fact]
	public async Task HandleSocketMessageAsync_HeartbeatWithNullData_UsesLastKnownSequence()
	{
		var client = CreateClient();
		var socket = new TestWebSocketClient();
		client.WebSocketClient = socket;

		const string payload = """
		                       {"op":1,"d":null}
		                       """;

		var exception = await Record.ExceptionAsync(() => client.HandleSocketMessageAsync(payload));

		Assert.Null(exception);
		Assert.NotNull(socket.LastTextMessage);
		Assert.Contains("\"op\":1", socket.LastTextMessage);
		Assert.Contains("\"d\":0", socket.LastTextMessage);
	}

	[Fact]
	public async Task HandleSocketMessageAsync_UnknownOpcode_CapturesDiagnosticsSinkReport()
	{
		var client = CreateClient();
		var sink = new TestDiagnosticsSink();
		client.DiagnosticsSink = sink;

		const string payload = """
		                       {"op":99,"t":"mystery_event","s":7,"d":{"foo":"bar"}}
		                       """;

		await client.HandleSocketMessageAsync(payload);

		var report = await sink.WaitForReportAsync();

		Assert.Equal("DisCatSharp", report.Source);
		Assert.Equal(DiagnosticSeverity.Warning, report.Severity);
		Assert.Equal("DiscordClient.WebSocket", report.Logger);
		Assert.Equal("Unknown gateway opcode 99", report.Message);
		Assert.Equal("99", report.Tags["dcs.gateway_opcode"]);
		Assert.Equal("mystery_event", report.Tags["dcs.gateway_event"]);
		Assert.Equal("99", report.Extra["opcode_name"]);
		Assert.Equal(7, report.Extra["sequence"]);
		Assert.Equal(payload, report.Extra["Scrubbed Payload"]);
	}

	[Fact]
	public async Task HandleSocketMessageAsync_UnknownOpcode_ScrubsEmbeddedDiscordIdsInJsonPayload()
	{
		var client = CreateClient(config => config.EnableDiscordIdScrubber = true);
		var sink = new TestDiagnosticsSink();
		client.DiagnosticsSink = sink;

		const string payload = """
		                       {"op":99,"t":"mystery_event","s":7,"d":{"guild_id":804032421678153819,"roles":["1480674874463752384","804032421757976697"],"user":{"id":"773493116404629504"}}}
		                       """;

		await client.HandleSocketMessageAsync(payload);

		var report = await sink.WaitForReportAsync();
		var scrubbedPayload = Assert.IsType<string>(report.Extra["Scrubbed Payload"]);

		Assert.DoesNotContain("804032421678153819", scrubbedPayload);
		Assert.DoesNotContain("1480674874463752384", scrubbedPayload);
		Assert.DoesNotContain("804032421757976697", scrubbedPayload);
		Assert.DoesNotContain("773493116404629504", scrubbedPayload);
		Assert.Contains("{DISCORD_ID}", scrubbedPayload);
	}

	[Fact]
	public async Task ThreadCreateEvent_MissingGuildCache_DoesNotThrowAndStillRaisesEvent()
	{
		var client = CreateClient();
		var thread = new DiscordThreadChannel
		{
			Id = 123,
			GuildId = 804032421678153819,
			ParentId = 9001,
			Name = "fresh-thread"
		};

		ThreadCreateEventArgs? captured = null;
		client.ThreadCreated += (_, args) =>
		{
			captured = args;
			return Task.CompletedTask;
		};

		var exception = await Record.ExceptionAsync(() => client.OnThreadCreateEventAsync(thread));

		Assert.Null(exception);
		Assert.NotNull(captured);
		Assert.Same(thread, captured!.Thread);
		Assert.Equal(thread.GuildId, captured.Guild.Id);
		Assert.Equal(thread.ParentId, captured.Parent.Id);
		Assert.False(client.GuildsInternal.ContainsKey(thread.GuildId.GetValueOrDefault()));
	}

	[Fact]
	public async Task WebhooksUpdate_MissingGuildCache_DoesNotThrowAndStillRaisesEvent()
	{
		var client = CreateClient();
		const ulong guildId = 804032421678153819;
		const ulong channelId = 9001;

		WebhooksUpdateEventArgs? captured = null;
		client.WebhooksUpdated += (_, args) =>
		{
			captured = args;
			return Task.CompletedTask;
		};

		var exception = await Record.ExceptionAsync(() => client.OnWebhooksUpdateAsync(channelId, guildId));

		Assert.Null(exception);
		Assert.NotNull(captured);
		Assert.Equal(guildId, captured!.Guild.Id);
		Assert.Equal(channelId, captured.Channel.Id);
		Assert.Equal(guildId, captured.Channel.GuildId);
		Assert.False(client.GuildsInternal.ContainsKey(guildId));
	}

	private static DiscordClient CreateClient(Action<DiscordConfiguration>? configure = null)
	{
		var configuration = new DiscordConfiguration
		{
			Token = "1",
			Gateway =
			{
				Advanced =
				{
					DispatchMode = Enums.GatewayDispatchMode.SequentialHandlers
				}
			}
		};
		configure?.Invoke(configuration);
		return new(configuration);
	}

	private sealed class TestDiagnosticsSink : ILibraryDiagnosticsSink
	{
		private readonly TaskCompletionSource<CapturedException> _exceptionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
		private readonly TaskCompletionSource<CapturedReport> _reportSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

		public bool IsEnabled => true;

		public void CaptureException(string source, Exception exception, IDictionary<string, object>? context = null, IDictionary<string, string>? tags = null)
			=> this._exceptionSource.TrySetResult(new(source, exception, context is null ? [] : new(context), tags is null ? [] : new(tags)));

		public void CaptureReport(DiagnosticReport report)
			=> this._reportSource.TrySetResult(new(report.Source, report.Severity, report.Logger, report.Message, report.Extra is null ? [] : new(report.Extra), report.Tags is null ? [] : new(report.Tags)));

		public void StartSession()
		{ }

		public void EndSession()
		{ }

		public void AddBreadcrumb(string source, string category, string message, DiagnosticSeverity level = DiagnosticSeverity.Info, IDictionary<string, string>? data = null)
		{ }

		public void EmitMetric(string source, string name, double value, string unit, IDictionary<string, string>? tags = null)
		{ }

		public IDisposable StartTiming(string source, string name, IDictionary<string, string>? tags = null)
			=> NoOpDisposable.Instance;

		public void Flush()
		{ }

		public Task<CapturedException> WaitForExceptionAsync()
			=> this._exceptionSource.Task.WaitAsync(TimeSpan.FromSeconds(15));

		public Task<CapturedReport> WaitForReportAsync()
			=> this._reportSource.Task.WaitAsync(TimeSpan.FromSeconds(15));
	}

	private sealed record CapturedException(string Source, Exception Exception, Dictionary<string, object> Context, Dictionary<string, string> Tags);

	private sealed record CapturedReport(string Source, DiagnosticSeverity Severity, string Logger, string Message, Dictionary<string, object> Extra, Dictionary<string, string> Tags);

	private sealed class TestWebSocketClient : IWebSocketClient
	{
		public IWebProxy Proxy => null!;

		public IReadOnlyDictionary<string, string> DefaultHeaders { get; } = new Dictionary<string, string>();

		public IServiceProvider ServiceProvider { get; set; } = null!;

		public string? LastTextMessage { get; private set; }

		public event AsyncEventHandler<IWebSocketClient, SocketEventArgs>? Connected
		{
			add { }
			remove { }
		}

		public event AsyncEventHandler<IWebSocketClient, SocketCloseEventArgs>? Disconnected
		{
			add { }
			remove { }
		}

		public event AsyncEventHandler<IWebSocketClient, SocketMessageEventArgs>? MessageReceived
		{
			add { }
			remove { }
		}

		public event AsyncEventHandler<IWebSocketClient, SocketErrorEventArgs>? ExceptionThrown
		{
			add { }
			remove { }
		}

		public Task ConnectAsync(Uri uri) => Task.CompletedTask;

		public Task DisconnectAsync(int code = 1000, string message = "") => Task.CompletedTask;

		public Task SendMessageAsync(string message)
		{
			this.LastTextMessage = message;
			return Task.CompletedTask;
		}

		public Task SendMessageAsync(byte[] data) => Task.CompletedTask;

		public bool AddDefaultHeader(string name, string value) => true;

		public bool RemoveDefaultHeader(string name) => true;

		public void Dispose()
		{ }
	}

	private sealed class NoOpDisposable : IDisposable
	{
		public static NoOpDisposable Instance { get; } = new();

		public void Dispose()
		{ }
	}
}
