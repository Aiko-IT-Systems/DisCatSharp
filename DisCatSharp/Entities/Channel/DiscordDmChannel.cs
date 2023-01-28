// This file is part of the DisCatSharp project, based off DSharpPlus.
//
// Copyright (c) 2021-2023 AITSYS
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
using System.Globalization;
using System.Threading.Tasks;

using DisCatSharp.Enums;
using DisCatSharp.Exceptions;
using DisCatSharp.Net;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a direct message channel.
/// </summary>
public class DiscordDmChannel : DiscordChannel
{
	/// <summary>
	/// Gets the recipients of this direct message.
	/// </summary>
	[JsonProperty("recipients", NullValueHandling = NullValueHandling.Ignore)]
	public IReadOnlyList<DiscordUser> Recipients { get; internal set; }

	/// <summary>
	/// Gets the hash of this channel's icon.
	/// </summary>
	[JsonProperty("icon", NullValueHandling = NullValueHandling.Ignore)]
	public string IconHash { get; internal set; }

	/// <summary>
	/// Gets the id of this direct message's creator.
	/// </summary>
	[JsonProperty("owner_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong OwnerId { get; internal set; }

	/// <summary>
	/// Gets the application id of the direct message's creator if it a bot.
	/// </summary>
	[JsonProperty("application_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? ApplicationId { get; internal set; }

	/// <summary>
	/// Gets whether the channel is managed by an application via the `gdm.join` OAuth2 scope.
	/// </summary>
	[JsonProperty("managed", NullValueHandling = NullValueHandling.Ignore)]
	public bool? Managed { get; internal set; }

	/// <summary>
	/// Gets the URL of this channel's icon.
	/// </summary>
	[JsonIgnore]
	public string IconUrl
		=> !string.IsNullOrWhiteSpace(this.IconHash) ? $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Url}{Endpoints.CHANNEL_ICONS}/{this.Id.ToString(CultureInfo.InvariantCulture)}/{this.IconHash}.png" : null;

	/// <summary>
	/// Only use for Group DMs! Whitelisted bots only. Requires user's oauth2 access token.
	/// </summary>
	/// <param name="userId">The id of the user to add.</param>
	/// <param name="accessToken">The OAuth2 access token.</param>
	/// <param name="nickname">The nickname to give to the user.</param>
	/// <exception cref="NotFoundException">Thrown when the channel does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task AddDmRecipientAsync(ulong userId, string accessToken, string nickname)
		=> this.Discord.ApiClient.AddGroupDmRecipientAsync(this.Id, userId, accessToken, nickname);

	/// <summary>
	/// Only use for Group DMs! Whitelisted bots only. Requires user's oauth2 access token.
	/// </summary>
	/// <param name="userId">The id of the User to remove.</param>
	/// <param name="accessToken">The OAuth2 access token.</param>
	/// <exception cref="NotFoundException">Thrown when the channel does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task RemoveDmRecipientAsync(ulong userId, string accessToken)
		=> this.Discord.ApiClient.RemoveGroupDmRecipientAsync(this.Id, userId);
}
