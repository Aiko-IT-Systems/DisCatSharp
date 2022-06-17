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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Net.Abstractions;
using DisCatSharp.Net.Serialization;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DisCatSharp.Net;

public sealed partial class DiscordApiClient
{

	/// <summary>
	/// Creates the guild from template async.
	/// </summary>
	/// <param name="templateCode">The template_code.</param>
	/// <param name="name">The name.</param>
	/// <param name="iconb64">The iconb64.</param>
	internal async Task<DiscordGuild> CreateGuildFromTemplateAsync(string templateCode, string name, Optional<string> iconb64)
	{
		var pld = new RestGuildCreateFromTemplatePayload
		{
			Name = name,
			IconBase64 = iconb64
		};

		var route = $"{Endpoints.GUILDS}{Endpoints.TEMPLATES}/:template_code";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new {template_code = templateCode }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var json = JObject.Parse(res.Response);
		var rawMembers = (JArray)json["members"];
		var guild = json.ToDiscordObject<DiscordGuild>();

		if (this.Discord is DiscordClient dc)
			await dc.OnGuildCreateEventAsync(guild, rawMembers, null).ConfigureAwait(false);
		return guild;
	}

	/// <summary>
	/// Gets the template async.
	/// </summary>
	/// <param name="code">The code.</param>

	internal async Task<DiscordGuildTemplate> GetTemplateAsync(string code)
	{
		var route = $"{Endpoints.GUILDS}{Endpoints.TEMPLATES}/:code";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { code }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var templatesRaw = JsonConvert.DeserializeObject<DiscordGuildTemplate>(res.Response);

		return templatesRaw;
	}



	/// <summary>
	/// Gets the guild templates async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>

	internal async Task<IReadOnlyList<DiscordGuildTemplate>> GetGuildTemplatesAsync(ulong guildId)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.TEMPLATES}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {guild_id = guildId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var templatesRaw = JsonConvert.DeserializeObject<IEnumerable<DiscordGuildTemplate>>(res.Response);

		return new ReadOnlyCollection<DiscordGuildTemplate>(new List<DiscordGuildTemplate>(templatesRaw));
	}

	/// <summary>
	/// Creates the guild template async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="name">The name.</param>
	/// <param name="description">The description.</param>

	internal async Task<DiscordGuildTemplate> CreateGuildTemplateAsync(ulong guildId, string name, string description)
	{
		var pld = new RestGuildTemplateCreateOrModifyPayload
		{
			Name = name,
			Description = description
		};

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.TEMPLATES}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new {guild_id = guildId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var ret = JsonConvert.DeserializeObject<DiscordGuildTemplate>(res.Response);

		return ret;
	}

	/// <summary>
	/// Syncs the guild template async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="templateCode">The template_code.</param>

	internal async Task<DiscordGuildTemplate> SyncGuildTemplateAsync(ulong guildId, string templateCode)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.TEMPLATES}/:template_code";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PUT, route, new {guild_id = guildId, template_code = templateCode }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.PUT, route).ConfigureAwait(false);

		var templateRaw = JsonConvert.DeserializeObject<DiscordGuildTemplate>(res.Response);

		return templateRaw;
	}

	/// <summary>
	/// Modifies the guild template async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="templateCode">The template_code.</param>
	/// <param name="name">The name.</param>
	/// <param name="description">The description.</param>

	internal async Task<DiscordGuildTemplate> ModifyGuildTemplateAsync(ulong guildId, string templateCode, string name, string description)
	{
		var pld = new RestGuildTemplateCreateOrModifyPayload
		{
			Name = name,
			Description = description
		};

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.TEMPLATES}/:template_code";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new {guild_id = guildId, template_code = templateCode }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.PATCH, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var templateRaw = JsonConvert.DeserializeObject<DiscordGuildTemplate>(res.Response);

		return templateRaw;
	}

	/// <summary>
	/// Deletes the guild template async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="templateCode">The template_code.</param>

	internal async Task<DiscordGuildTemplate> DeleteGuildTemplateAsync(ulong guildId, string templateCode)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.TEMPLATES}/:template_code";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new {guild_id = guildId, template_code = templateCode }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.DELETE, route).ConfigureAwait(false);

		var templateRaw = JsonConvert.DeserializeObject<DiscordGuildTemplate>(res.Response);

		return templateRaw;
	}
}
