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

using System.Threading.Tasks;

using DisCatSharp.Entities;
using DisCatSharp.Net.Abstractions;
using DisCatSharp.Net.Serialization;

using Newtonsoft.Json;

namespace DisCatSharp.Net;

public sealed partial class DiscordApiClient
{
	/// <summary>
	/// Creates the stage instance async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	/// <param name="topic">The topic.</param>
	/// <param name="sendStartNotification">Whether everyone should be notified about the stage.</param>
	/// <param name="privacyLevel">The privacy_level.</param>
	/// <param name="reason">The reason.</param>
	internal async Task<DiscordStageInstance> CreateStageInstanceAsync(ulong channelId, string topic, bool sendStartNotification, StagePrivacyLevel privacyLevel, string reason)
	{
		var pld = new RestStageInstanceCreatePayload
		{
			ChannelId = channelId,
			Topic = topic,
			PrivacyLevel = privacyLevel,
			SendStartNotification = sendStartNotification
		};

		var route = $"{Endpoints.STAGE_INSTANCES}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.POST, route, new { }, out var path);
		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.POST, route, headers, DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var stageInstance = JsonConvert.DeserializeObject<DiscordStageInstance>(res.Response);

		return stageInstance;
	}

	/// <summary>
	/// Gets the stage instance async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	internal async Task<DiscordStageInstance> GetStageInstanceAsync(ulong channelId)
	{
		var route = $"{Endpoints.STAGE_INSTANCES}/:channel_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {channel_id = channelId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var stageInstance = JsonConvert.DeserializeObject<DiscordStageInstance>(res.Response);

		return stageInstance;
	}

	/// <summary>
	/// Modifies the stage instance async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	/// <param name="topic">The topic.</param>
	/// <param name="privacyLevel">The privacy_level.</param>
	/// <param name="reason">The reason.</param>
	internal Task ModifyStageInstanceAsync(ulong channelId, Optional<string> topic, Optional<StagePrivacyLevel> privacyLevel, string reason)
	{
		var pld = new RestStageInstanceModifyPayload
		{
			Topic = topic,
			PrivacyLevel = privacyLevel
		};

		var route = $"{Endpoints.STAGE_INSTANCES}/:channel_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new {channel_id = channelId }, out var path);
		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.PATCH, route, headers, DiscordJson.SerializeObject(pld));
	}

	/// <summary>
	/// Deletes the stage instance async.
	/// </summary>
	/// <param name="channelId">The channel_id.</param>
	/// <param name="reason">The reason.</param>
	internal Task DeleteStageInstanceAsync(ulong channelId, string reason)
	{
		var route = $"{Endpoints.STAGE_INSTANCES}/:channel_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.DELETE, route, new {channel_id = channelId }, out var path);
		var headers = Utilities.GetBaseHeaders();
		if (!string.IsNullOrWhiteSpace(reason))
			headers.Add(REASON_HEADER_NAME, reason);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		return this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.DELETE, route, headers);
	}
}
