// This file is part of the DisCatSharp project, based off DSharpPlus.
//
// Copyright (c) 2021-2022 AITSYS
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using DisCatSharp.Entities;
using DisCatSharp.Net.Abstractions;
using DisCatSharp.Net.Serialization;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DisCatSharp.Net;

public sealed partial class DiscordApiClient
{
	/// <summary>
	/// Gets the guild emojis async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	internal async Task<IReadOnlyList<DiscordGuildEmoji>> GetGuildEmojisAsync(ulong guildId)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.EMOJIS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {guild_id = guildId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var emojisRaw = JsonConvert.DeserializeObject<IEnumerable<JObject>>(res.Response);

		this.Discord.Guilds.TryGetValue(guildId, out var gld);
		var users = new Dictionary<ulong, DiscordUser>();
		var emojis = new List<DiscordGuildEmoji>();
		foreach (var rawEmoji in emojisRaw)
		{
			var xge = rawEmoji.ToObject<DiscordGuildEmoji>();
			xge.Guild = gld;

			var xtu = rawEmoji["user"]?.ToObject<TransportUser>();
			if (xtu != null)
			{
				if (!users.ContainsKey(xtu.Id))
				{
					var user = gld != null && gld.Members.TryGetValue(xtu.Id, out var member) ? member : new DiscordUser(xtu);
					users[user.Id] = user;
				}

				xge.User = users[xtu.Id];
			}

			emojis.Add(xge);
		}

		return new ReadOnlyCollection<DiscordGuildEmoji>(emojis);
	}

	/// <summary>
	/// Gets the guild emoji async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="emojiId">The emoji_id.</param>

	internal async Task<DiscordGuildEmoji> GetGuildEmojiAsync(ulong guildId, ulong emojiId)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.EMOJIS}/:emoji_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {guild_id = guildId, emoji_id = emojiId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		this.Discord.Guilds.TryGetValue(guildId, out var gld);

		var emojiRaw = JObject.Parse(res.Response);
		var emoji = emojiRaw.ToObject<DiscordGuildEmoji>();
		emoji.Guild = gld;

		var xtu = emojiRaw["user"]?.ToObject<TransportUser>();
		if (xtu != null)
			emoji.User = gld != null && gld.Members.TryGetValue(xtu.Id, out var member) ? member : new DiscordUser(xtu);

		return emoji;
	}

	/// <summary>
	/// Creates the guild emoji async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="name">The name.</param>
	/// <param name="imageb64">The imageb64.</param>
	/// <param name="roles">The roles.</param>
	/// <param name="reason">The reason.</param>

	internal async Task<DiscordGuildEmoji> CreateGuildEmojiAsync(ulong guildId, string name, string imageb64, IEnumerable<ulong> roles, string reason)
	{
		var pld = new RestGuildEmojiCreatePayload
		{
			Name = name,
			ImageB64 = imageb64,
			Roles = roles?.ToArray()
		};

		var headers = new Dictionary<string, string>();
		if (!string.IsNullOrWhiteSpace(reason))
			headers[REASON_HEADER_NAME] = reason;

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.EMOJIS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new {guild_id = guildId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.POST, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		this.Discord.Guilds.TryGetValue(guildId, out var gld);

		var emojiRaw = JObject.Parse(res.Response);
		var emoji = emojiRaw.ToObject<DiscordGuildEmoji>();
		emoji.Guild = gld;

		var xtu = emojiRaw["user"]?.ToObject<TransportUser>();
		emoji.User = xtu != null
			? gld != null && gld.Members.TryGetValue(xtu.Id, out var member) ? member : new DiscordUser(xtu)
			: this.Discord.CurrentUser;

		return emoji;
	}

	/// <summary>
	/// Modifies the guild emoji async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="emojiId">The emoji_id.</param>
	/// <param name="name">The name.</param>
	/// <param name="roles">The roles.</param>
	/// <param name="reason">The reason.</param>

	internal async Task<DiscordGuildEmoji> ModifyGuildEmojiAsync(ulong guildId, ulong emojiId, string name, IEnumerable<ulong> roles, string reason)
	{
		var pld = new RestGuildEmojiModifyPayload
		{
			Name = name,
			Roles = roles?.ToArray()
		};

		var headers = new Dictionary<string, string>();
		if (!string.IsNullOrWhiteSpace(reason))
			headers[REASON_HEADER_NAME] = reason;

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.EMOJIS}/:emoji_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new {guild_id = guildId, emoji_id = emojiId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		this.Discord.Guilds.TryGetValue(guildId, out var gld);

		var emojiRaw = JObject.Parse(res.Response);
		var emoji = emojiRaw.ToObject<DiscordGuildEmoji>();
		emoji.Guild = gld;

		var xtu = emojiRaw["user"]?.ToObject<TransportUser>();
		if (xtu != null)
			emoji.User = gld != null && gld.Members.TryGetValue(xtu.Id, out var member) ? member : new DiscordUser(xtu);

		return emoji;
	}

	/// <summary>
	/// Deletes the guild emoji async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="emojiId">The emoji_id.</param>
	/// <param name="reason">The reason.</param>

	internal Task DeleteGuildEmojiAsync(ulong guildId, ulong emojiId, string reason)
	{
		var headers = new Dictionary<string, string>();
		if (!string.IsNullOrWhiteSpace(reason))
			headers[REASON_HEADER_NAME] = reason;

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.EMOJIS}/:emoji_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new {guild_id = guildId, emoji_id = emojiId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers);
	}
}
