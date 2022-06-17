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
	/// Gets the guild integrations async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>

	internal async Task<IReadOnlyList<DiscordIntegration>> GetGuildIntegrationsAsync(ulong guildId)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.INTEGRATIONS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {guild_id = guildId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var integrationsRaw = JsonConvert.DeserializeObject<IEnumerable<DiscordIntegration>>(res.Response).Select(xi => { xi.Discord = this.Discord; return xi; });

		return new ReadOnlyCollection<DiscordIntegration>(new List<DiscordIntegration>(integrationsRaw));
	}


	/// <summary>
	/// Creates the guild integration async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="type">The type.</param>
	/// <param name="id">The id.</param>

	internal async Task<DiscordIntegration> CreateGuildIntegrationAsync(ulong guildId, string type, ulong id)
	{
		var pld = new RestGuildIntegrationAttachPayload
		{
			Type = type,
			Id = id
		};

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.INTEGRATIONS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new {guild_id = guildId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.POST, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var ret = JsonConvert.DeserializeObject<DiscordIntegration>(res.Response);
		ret.Discord = this.Discord;

		return ret;
	}

	/// <summary>
	/// Modifies the guild integration async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="integrationId">The integration_id.</param>
	/// <param name="expireBehaviour">The expire_behaviour.</param>
	/// <param name="expireGracePeriod">The expire_grace_period.</param>
	/// <param name="enableEmoticons">If true, enable_emoticons.</param>

	internal async Task<DiscordIntegration> ModifyGuildIntegrationAsync(ulong guildId, ulong integrationId, int expireBehaviour, int expireGracePeriod, bool enableEmoticons)
	{
		var pld = new RestGuildIntegrationModifyPayload
		{
			ExpireBehavior = expireBehaviour,
			ExpireGracePeriod = expireGracePeriod,
			EnableEmoticons = enableEmoticons
		};

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.INTEGRATIONS}/:integration_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new {guild_id = guildId, integration_id = integrationId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.PATCH, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var ret = JsonConvert.DeserializeObject<DiscordIntegration>(res.Response);
		ret.Discord = this.Discord;

		return ret;
	}

	/// <summary>
	/// Deletes the guild integration async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="integration">The integration.</param>

	internal Task DeleteGuildIntegrationAsync(ulong guildId, DiscordIntegration integration)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.INTEGRATIONS}/:integration_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new {guild_id = guildId, integration_id = integration.Id }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.DELETE, route, payload: DiscordJson.SerializeObject(integration));
	}

	/// <summary>
	/// Syncs the guild integration async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="integrationId">The integration_id.</param>

	internal Task SyncGuildIntegrationAsync(ulong guildId, ulong integrationId)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.INTEGRATIONS}/:integration_id{Endpoints.SYNC}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new {guild_id = guildId, integration_id = integrationId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.POST, route);
	}
}
