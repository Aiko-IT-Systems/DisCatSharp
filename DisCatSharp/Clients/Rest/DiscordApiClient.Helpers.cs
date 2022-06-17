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
using System.Net;
using System.Text;
using System.Threading.Tasks;

using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Net.Abstractions;
using DisCatSharp.Net.Serialization;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DisCatSharp.Net;

/// <summary>
/// Represents a discord api client.
/// </summary>
public sealed partial class DiscordApiClient
{
	/// <summary>
	/// The audit log reason header name.
	/// </summary>
	private const string REASON_HEADER_NAME = "X-Audit-Log-Reason";

	/// <summary>
	/// Prepares the message.
	/// </summary>
	/// <param name="msgRaw">The msg_raw.</param>
	/// <returns>A DiscordMessage.</returns>
	private DiscordMessage PrepareMessage(JToken msgRaw)
	{
		var author = msgRaw["author"].ToObject<TransportUser>();
		var ret = msgRaw.ToDiscordObject<DiscordMessage>();
		ret.Discord = this.Discord;

		this.PopulateMessage(author, ret);

		var referencedMsg = msgRaw["referenced_message"];
		if (ret.MessageType == MessageType.Reply && !string.IsNullOrWhiteSpace(referencedMsg?.ToString()))
		{
			author = referencedMsg["author"].ToObject<TransportUser>();
			ret.ReferencedMessage.Discord = this.Discord;
			this.PopulateMessage(author, ret.ReferencedMessage);
		}

		if (ret.Channel != null)
			return ret;

		var channel = !ret.GuildId.HasValue
			? new DiscordDmChannel
			{
				Id = ret.ChannelId,
				Discord = this.Discord,
				Type = ChannelType.Private
			}
			: new DiscordChannel
			{
				Id = ret.ChannelId,
				GuildId = ret.GuildId,
				Discord = this.Discord
			};

		ret.Channel = channel;

		return ret;
	}

	/// <summary>
	/// Populates the message.
	/// </summary>
	/// <param name="author">The author.</param>
	/// <param name="ret">The message.</param>
	private void PopulateMessage(TransportUser author, DiscordMessage ret)
	{
		var guild = ret.Channel?.Guild;

		//If this is a webhook, it shouldn't be in the user cache.
		if (author.IsBot && int.Parse(author.Discriminator) == 0)
		{
			ret.Author = new DiscordUser(author) { Discord = this.Discord };
		}
		else
		{
			if (!this.Discord.UserCache.TryGetValue(author.Id, out var usr))
			{
				this.Discord.UserCache[author.Id] = usr = new DiscordUser(author) { Discord = this.Discord };
			}

			if (guild != null)
			{
				if (!guild.Members.TryGetValue(author.Id, out var mbr))
					mbr = new DiscordMember(usr) { Discord = this.Discord, GuildId = guild.Id };
				ret.Author = mbr;
			}
			else
			{
				ret.Author = usr;
			}
		}

		ret.PopulateMentions();

		if (ret.ReactionsInternal == null)
			ret.ReactionsInternal = new List<DiscordReaction>();
		foreach (var xr in ret.ReactionsInternal)
			xr.Emoji.Discord = this.Discord;
	}
}
