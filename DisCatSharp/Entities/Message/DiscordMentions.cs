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

using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Handles mentionables.
/// </summary>
internal class DiscordMentions
{
	/// <summary>
	/// Parse users.
	/// </summary>
	private const string PARSE_USERS = "users";

	/// <summary>
	/// Parse roles.
	/// </summary>
	private const string PARSE_ROLES = "roles";

	/// <summary>
	/// Parse everyone.
	/// </summary>
	private const string PARSE_EVERYONE = "everyone";

	/// <summary>
	/// Collection roles to serialize
	/// </summary>
	[JsonProperty("roles", NullValueHandling = NullValueHandling.Ignore)]
	public IReadOnlyList<ulong> Roles { get; }

	/// <summary>
	/// Collection of users to serialize
	/// </summary>
	[JsonProperty("users", NullValueHandling = NullValueHandling.Ignore)]
	public IReadOnlyList<ulong> Users { get; }

	/// <summary>
	/// The values to be parsed
	/// </summary>
	[JsonProperty("parse", NullValueHandling = NullValueHandling.Ignore)]
	public IReadOnlyList<string> Parse { get; }

	/// <summary>
	/// For replies, whether to mention the author of the message being replied to.
	/// </summary>
	[JsonProperty("replied_user", NullValueHandling = NullValueHandling.Ignore)]
	public bool? RepliedUser { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="DiscordMentions"/> class.
	/// </summary>
	/// <param name="mentions">The mentions.</param>
	/// <param name="mention">If true, mention.</param>
	/// <param name="repliedUser">If true, replied user.</param>
	internal DiscordMentions(IEnumerable<IMention> mentions, bool mention = false, bool repliedUser = false)
	{
		if (mentions == null)
			return;

		if (!mentions.Any())
		{
			this.Parse = Array.Empty<string>();
			this.RepliedUser = repliedUser;
			return;
		}

		if (mention)
		{
			this.RepliedUser = repliedUser;
		}

		var roles = new HashSet<ulong>();
		var users = new HashSet<ulong>();
		var parse = new HashSet<string>();

		foreach (var m in mentions)
		{
			switch (m)
			{
				default:
					throw new NotSupportedException("Type not supported in mentions.");
				case UserMention u:
					if (u.Id.HasValue)
						users.Add(u.Id.Value);
					else
						parse.Add(PARSE_USERS);

					break;

				case RoleMention r:
					if (r.Id.HasValue)
						roles.Add(r.Id.Value);
					else
						parse.Add(PARSE_ROLES);
					break;

				case EveryoneMention e:
					parse.Add(PARSE_EVERYONE);
					break;

				case RepliedUserMention _:
					this.RepliedUser = repliedUser;
					break;
			}
		}

		if (!parse.Contains(PARSE_USERS) && users.Count > 0)
			this.Users = users.ToArray();

		if (!parse.Contains(PARSE_ROLES) && roles.Count > 0)
			this.Roles = roles.ToArray();

		if (parse.Count > 0)
			this.Parse = parse.ToArray();
	}
}
