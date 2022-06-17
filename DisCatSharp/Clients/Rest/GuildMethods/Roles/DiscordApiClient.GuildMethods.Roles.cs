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

namespace DisCatSharp.Net;

public sealed partial class DiscordApiClient
{
	/// <summary>
	/// Gets the guild roles async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>

	internal async Task<IReadOnlyList<DiscordRole>> GetGuildRolesAsync(ulong guildId)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.ROLES}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {guild_id = guildId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var rolesRaw = JsonConvert.DeserializeObject<IEnumerable<DiscordRole>>(res.Response).Select(xr => { xr.Discord = this.Discord; xr.GuildId = guildId; return xr; });

		return new ReadOnlyCollection<DiscordRole>(new List<DiscordRole>(rolesRaw));
	}

	/// <summary>
	/// Modifies the guild role position async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="pld">The pld.</param>
	/// <param name="reason">The reason.</param>

	internal Task ModifyGuildRolePositionAsync(ulong guildId, IEnumerable<RestGuildRoleReorderPayload> pld, string reason)
	{
		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.ROLES}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new {guild_id = guildId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld));
	}

	/// <summary>
	/// Modifies the guild role async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="roleId">The role_id.</param>
	/// <param name="name">The name.</param>
	/// <param name="permissions">The permissions.</param>
	/// <param name="color">The color.</param>
	/// <param name="hoist">If true, hoist.</param>
	/// <param name="mentionable">If true, mentionable.</param>
	/// <param name="iconb64">The icon.</param>
	/// <param name="emoji">The unicode emoji icon.</param>
	/// <param name="reason">The reason.</param>
	internal async Task<DiscordRole> ModifyGuildRoleAsync(ulong guildId, ulong roleId, string name, Permissions? permissions, int? color, bool? hoist, bool? mentionable, Optional<string> iconb64, Optional<string> emoji, string reason)
	{
		var pld = new RestGuildRolePayload
		{
			Name = name,
			Permissions = permissions & PermissionMethods.FullPerms,
			Color = color,
			Hoist = hoist,
			Mentionable = mentionable,
		};

		if (emoji.HasValue && !iconb64.HasValue)
			pld.UnicodeEmoji = emoji;

		if (emoji.HasValue && iconb64.HasValue)
		{
			pld.IconBase64 = null;
			pld.UnicodeEmoji = emoji;
		}

		if (iconb64.HasValue)
			pld.IconBase64 = iconb64;

		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers[REASON_HEADER_NAME] = reason;

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.ROLES}/:role_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new {guild_id = guildId, role_id = roleId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var ret = JsonConvert.DeserializeObject<DiscordRole>(res.Response);
		ret.Discord = this.Discord;
		ret.GuildId = guildId;

		return ret;
	}

	/// <summary>
	/// Deletes the role async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="roleId">The role_id.</param>
	/// <param name="reason">The reason.</param>

	internal Task DeleteRoleAsync(ulong guildId, ulong roleId, string reason)
	{
		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers[REASON_HEADER_NAME] = reason;

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.ROLES}/:role_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new {guild_id = guildId, role_id = roleId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers);
	}

	/// <summary>
	/// Creates the guild role async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="name">The name.</param>
	/// <param name="permissions">The permissions.</param>
	/// <param name="color">The color.</param>
	/// <param name="hoist">If true, hoist.</param>
	/// <param name="mentionable">If true, mentionable.</param>
	/// <param name="reason">The reason.</param>

	internal async Task<DiscordRole> CreateGuildRoleAsync(ulong guildId, string name, Permissions? permissions, int? color, bool? hoist, bool? mentionable, string reason)
	{
		var pld = new RestGuildRolePayload
		{
			Name = name,
			Permissions = permissions & PermissionMethods.FullPerms,
			Color = color,
			Hoist = hoist,
			Mentionable = mentionable
		};

		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers[REASON_HEADER_NAME] = reason;

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.ROLES}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new {guild_id = guildId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.DoRequestAsync(this.Discord, bucket, url, RestRequestMethod.POST, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var ret = JsonConvert.DeserializeObject<DiscordRole>(res.Response);
		ret.Discord = this.Discord;
		ret.GuildId = guildId;

		return ret;
	}
}
