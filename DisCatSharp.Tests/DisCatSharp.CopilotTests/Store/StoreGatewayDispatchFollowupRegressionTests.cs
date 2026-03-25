using System.Threading.Tasks;

using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.EventArgs;
using DisCatSharp.Net.Abstractions;

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

	private static DiscordClient CreateClient()
		=> new(new DiscordConfiguration
		{
			Token = "1"
		});
}
