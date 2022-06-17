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
using System.Linq;
using System.Threading.Tasks;

using DisCatSharp.Entities;
using DisCatSharp.Net.Abstractions;
using DisCatSharp.Net.Serialization;

using Newtonsoft.Json.Linq;

namespace DisCatSharp.Net;

public sealed partial class DiscordApiClient
{
	/// <summary>
	/// Gets the guild stickers.
	/// </summary>
	/// <param name="guildId">The guild id.</param>
	internal async Task<IReadOnlyList<DiscordSticker>> GetGuildStickersAsync(ulong guildId)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.STICKERS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {guild_id = guildId}, out var path);
		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);

		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);
		var json = JArray.Parse(res.Response);
		var ret = json.ToDiscordObject<DiscordSticker[]>();

		for (var i = 0; i < ret.Length; i++)
		{
			var stkr = ret[i];
			stkr.Discord = this.Discord;

			if (json[i]["user"] is JObject obj) // Null = Missing stickers perm //
			{
				var tsr = obj.ToDiscordObject<TransportUser>();
				var usr = new DiscordUser(tsr) {Discord = this.Discord};
				usr = this.Discord.UserCache.AddOrUpdate(tsr.Id, usr, (id, old) =>
				{
					old.Username = usr.Username;
					old.Discriminator = usr.Discriminator;
					old.AvatarHash = usr.AvatarHash;
					return old;
				});
				stkr.User = usr;
			}
		}

		return ret.ToList();
	}

	/// <summary>
	/// Gets a guild sticker.
	/// </summary>
	/// <param name="guildId">The guild id.</param>
	/// <param name="stickerId">The sticker id.</param>
	internal async Task<DiscordSticker> GetGuildStickerAsync(ulong guildId, ulong stickerId)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.STICKERS}/:sticker_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {guild_id = guildId, sticker_id = stickerId}, out var path);
		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);

		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var json = JObject.Parse(res.Response);
		var ret = json.ToDiscordObject<DiscordSticker>();
		if (json["user"] is not null) // Null = Missing stickers perm //
		{
			var tsr = json["user"].ToDiscordObject<TransportUser>();
			var usr = new DiscordUser(tsr) {Discord = this.Discord};
			usr = this.Discord.UserCache.AddOrUpdate(tsr.Id, usr, (id, old) =>
			{
				old.Username = usr.Username;
				old.Discriminator = usr.Discriminator;
				old.AvatarHash = usr.AvatarHash;
				return old;
			});
			ret.User = usr;
		}
		ret.Discord = this.Discord;
		return ret;
	}

	/// <summary>
	/// Creates the guild sticker.
	/// </summary>
	/// <param name="guildId">The guild id.</param>
	/// <param name="name">The name.</param>
	/// <param name="description">The description.</param>
	/// <param name="tags">The tags.</param>
	/// <param name="file">The file.</param>
	/// <param name="reason">The reason.</param>
	internal async Task<DiscordSticker> CreateGuildStickerAsync(ulong guildId, string name, string description, string tags, DiscordMessageFile file, string reason)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.STICKERS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new {guild_id = guildId}, out var path);
		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);

		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var res = await this.ExecuteStickerMultipartRestRequest(this.Discord, bucket, url, RestRequestMethod.POST, route, headers, file, name, tags, description);

		var ret = JObject.Parse(res.Response).ToDiscordObject<DiscordSticker>();

		ret.Discord = this.Discord;

		return ret;
	}

	/// <summary>
	/// Modifies the guild sticker.
	/// </summary>
	/// <param name="guildId">The guild id.</param>
	/// <param name="stickerId">The sticker id.</param>
	/// <param name="name">The name.</param>
	/// <param name="description">The description.</param>
	/// <param name="tags">The tags.</param>
	/// <param name="reason">The reason.</param>
	internal async Task<DiscordSticker> ModifyGuildStickerAsync(ulong guildId, ulong stickerId, Optional<string> name, Optional<string> description, Optional<string> tags, string reason)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.STICKERS}/:sticker_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new {guild_id = guildId, sticker_id = stickerId}, out var path);
		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var pld = new RestStickerModifyPayload()
		{
			Name = name,
			Description = description,
			Tags = tags
		};

		var values = new Dictionary<string, string>
		{
			["payload_json"] = DiscordJson.SerializeObject(pld)
		};

		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.PATCH, route);
		var ret = JObject.Parse(res.Response).ToDiscordObject<DiscordSticker>();
		ret.Discord = this.Discord;

		return null;
	}

	/// <summary>
	/// Deletes the guild sticker async.
	/// </summary>
	/// <param name="guildId">The guild id.</param>
	/// <param name="stickerId">The sticker id.</param>
	/// <param name="reason">The reason.</param>
	internal async Task DeleteGuildStickerAsync(ulong guildId, ulong stickerId, string reason)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.STICKERS}/:sticker_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new {guild_id = guildId, sticker_id = stickerId }, out var path);
		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers);
	}
}
