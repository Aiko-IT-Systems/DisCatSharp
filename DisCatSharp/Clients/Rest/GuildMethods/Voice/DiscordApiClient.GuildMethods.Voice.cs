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
using System.Threading.Tasks;

using DisCatSharp.Entities;
using DisCatSharp.Net.Abstractions;
using DisCatSharp.Net.Serialization;

using Newtonsoft.Json;

namespace DisCatSharp.Net;

public sealed partial class DiscordApiClient
{
	/// <summary>
	/// Updates the current user voice state async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="channelId">The channel id.</param>
	/// <param name="suppress">If true, suppress.</param>
	/// <param name="requestToSpeakTimestamp">The request to speak timestamp.</param>
	internal async Task UpdateCurrentUserVoiceStateAsync(ulong guildId, ulong channelId, bool? suppress, DateTimeOffset? requestToSpeakTimestamp)
	{
		var pld = new RestGuildUpdateCurrentUserVoiceStatePayload
		{
			ChannelId = channelId,
			Suppress = suppress,
			RequestToSpeakTimestamp = requestToSpeakTimestamp
		};

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.VOICE_STATES}/@me";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new {guild_id = guildId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.PATCH, route, payload: DiscordJson.SerializeObject(pld));
	}

	/// <summary>
	/// Updates the user voice state async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>
	/// <param name="userId">The user_id.</param>
	/// <param name="channelId">The channel id.</param>
	/// <param name="suppress">If true, suppress.</param>

	internal async Task UpdateUserVoiceStateAsync(ulong guildId, ulong userId, ulong channelId, bool? suppress)
	{
		var pld = new RestGuildUpdateUserVoiceStatePayload
		{
			ChannelId = channelId,
			Suppress = suppress
		};

		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.VOICE_STATES}/:user_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new {guild_id = guildId, user_id = userId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.PATCH, route, payload: DiscordJson.SerializeObject(pld));
	}

	/// <summary>
	/// Gets the guild voice regions async.
	/// </summary>
	/// <param name="guildId">The guild_id.</param>

	internal async Task<IReadOnlyList<DiscordVoiceRegion>> GetGuildVoiceRegionsAsync(ulong guildId)
	{
		var route = $"{Endpoints.GUILDS}/:guild_id{Endpoints.REGIONS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {guild_id = guildId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var regionsRaw = JsonConvert.DeserializeObject<IEnumerable<DiscordVoiceRegion>>(res.Response);

		return new ReadOnlyCollection<DiscordVoiceRegion>(new List<DiscordVoiceRegion>(regionsRaw));
	}

	/// <summary>
	/// Lists the voice regions async.
	/// </summary>
	internal async Task<IReadOnlyList<DiscordVoiceRegion>> ListVoiceRegionsAsync()
	{
		var route = $"{Endpoints.VOICE}{Endpoints.REGIONS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var regions = JsonConvert.DeserializeObject<IEnumerable<DiscordVoiceRegion>>(res.Response);

		return new ReadOnlyCollection<DiscordVoiceRegion>(new List<DiscordVoiceRegion>(regions));
	}
}
