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
using System.Globalization;
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
	/// Gets the current user async.
	/// </summary>

	internal Task<DiscordUser> GetCurrentUserAsync()
		=> this.GetUserAsync("@me");

	/// <summary>
	/// Gets the user async.
	/// </summary>
	/// <param name="userId">The user_id.</param>

	internal Task<DiscordUser> GetUserAsync(ulong userId)
		=> this.GetUserAsync(userId.ToString(CultureInfo.InvariantCulture));

	/// <summary>
	/// Gets the user async.
	/// </summary>
	/// <param name="userId">The user_id.</param>

	internal async Task<DiscordUser> GetUserAsync(string userId)
	{
		var route = $"{Endpoints.USERS}/:user_id";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new {user_id = userId }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var userRaw = JsonConvert.DeserializeObject<TransportUser>(res.Response);
		var duser = new DiscordUser(userRaw) { Discord = this.Discord };

		return duser;
	}


	/// <summary>
	/// Modifies the current user async.
	/// </summary>
	/// <param name="username">The username.</param>
	/// <param name="base64Avatar">The base64_avatar.</param>

	internal async Task<TransportUser> ModifyCurrentUserAsync(string username, Optional<string> base64Avatar)
	{
		var pld = new RestUserUpdateCurrentPayload
		{
			Username = username,
			AvatarBase64 = base64Avatar.ValueOrDefault(),
			AvatarSet = base64Avatar.HasValue
		};

		var route = $"{Endpoints.USERS}{Endpoints.ME}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.PATCH, route, new { }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.PATCH, route, payload: DiscordJson.SerializeObject(pld)).ConfigureAwait(false);

		var userRaw = JsonConvert.DeserializeObject<TransportUser>(res.Response);

		return userRaw;
	}
	/// <summary>
	/// Gets the current user guilds async.
	/// </summary>
	/// <param name="limit">The limit.</param>
	/// <param name="before">The before.</param>
	/// <param name="after">The after.</param>

	internal async Task<IReadOnlyList<DiscordGuild>> GetCurrentUserGuildsAsync(int limit = 100, ulong? before = null, ulong? after = null)
	{
		var route = $"{Endpoints.USERS}{Endpoints.ME}{Endpoints.GUILDS}";

		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { }, out var path);

		var url = Utilities.GetApiUriBuilderFor(path, this.Discord.Configuration)
		.AddParameter($"limit", limit.ToString(CultureInfo.InvariantCulture));

		if (before != null)
			url.AddParameter("before", before.Value.ToString(CultureInfo.InvariantCulture));
		if (after != null)
			url.AddParameter("after", after.Value.ToString(CultureInfo.InvariantCulture));

		var res = await this.ExecuteRestRequest(this.Discord, bucket, url.Build(), RestRequestMethod.GET, route).ConfigureAwait(false);

		if (this.Discord is DiscordClient)
		{
			var guildsRaw = JsonConvert.DeserializeObject<IEnumerable<RestUserGuild>>(res.Response);
			var glds = guildsRaw.Select(xug => (this.Discord as DiscordClient)?.GuildsInternal[xug.Id]);
			return new ReadOnlyCollection<DiscordGuild>(new List<DiscordGuild>(glds));
		}
		else
		{
			return new ReadOnlyCollection<DiscordGuild>(JsonConvert.DeserializeObject<List<DiscordGuild>>(res.Response));
		}
	}

	/// <summary>
	/// Gets the users connections async.
	/// </summary>
	internal async Task<IReadOnlyList<DiscordConnection>> GetCurrentUserConnectionsAsync()
	{
		var route = $"{Endpoints.USERS}{Endpoints.ME}{Endpoints.CONNECTIONS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var connectionsRaw = JsonConvert.DeserializeObject<IEnumerable<DiscordConnection>>(res.Response).Select(xc => { xc.Discord = this.Discord; return xc; });

		return new ReadOnlyCollection<DiscordConnection>(new List<DiscordConnection>(connectionsRaw));
	}
	
	/// <summary>
	/// Gets the users connections async.
	/// </summary>
	internal async Task<IReadOnlyList<DiscordConnection>> GetUserConnectionsAsync(ulong user_id)
	{
		var route = $"{Endpoints.USERS}/:user_id{Endpoints.CONNECTIONS}";
		var bucket = this.Rest.GetBucket(RestRequestMethod.GET, route, new { user_id = user_id.ToString() }, out var path);

		var url = Utilities.GetApiUriFor(path, this.Discord.Configuration);
		var res = await this.ExecuteRestRequest(this.Discord, bucket, url, RestRequestMethod.GET, route).ConfigureAwait(false);

		var connectionsRaw = JsonConvert.DeserializeObject<IEnumerable<DiscordConnection>>(res.Response).Select(xc => { xc.Discord = this.Discord; return xc; });

		return new ReadOnlyCollection<DiscordConnection>(new List<DiscordConnection>(connectionsRaw));
	}
}
