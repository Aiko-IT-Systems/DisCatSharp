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
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a guild emoji.
/// </summary>
public sealed class DiscordGuildEmoji : DiscordEmoji
{
	/// <summary>
	/// Gets the user that created this emoji.
	/// </summary>
	[JsonIgnore]
	public DiscordUser User { get; internal set; }

	/// <summary>
	/// Gets the guild to which this emoji belongs.
	/// </summary>
	[JsonIgnore]
	public DiscordGuild Guild { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="DiscordGuildEmoji"/> class.
	/// </summary>
	internal DiscordGuildEmoji()
	{ }


#pragma warning disable CS1574 // XML comment has cref attribute that could not be resolved
#pragma warning disable CS1574 // XML comment has cref attribute that could not be resolved
#pragma warning disable CS1574 // XML comment has cref attribute that could not be resolved
#pragma warning disable CS1574 // XML comment has cref attribute that could not be resolved
#pragma warning disable CS1574 // XML comment has cref attribute that could not be resolved
	/// <summary>
	/// Modifies this emoji.
	/// </summary>
	/// <param name="name">New name for this emoji.</param>
	/// <param name="roles">Roles for which this emoji will be available. This works only if your application is whitelisted as integration.</param>
	/// <param name="reason">Reason for audit log.</param>
	/// <returns>The modified emoji.</returns>
	/// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageEmojisAndStickers"/> permission.</exception>
	/// <exception cref="NotFoundException">Thrown when the emoji does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordGuildEmoji> ModifyAsync(string name, IEnumerable<DiscordRole> roles = null, string reason = null)
#pragma warning restore CS1574 // XML comment has cref attribute that could not be resolved
#pragma warning restore CS1574 // XML comment has cref attribute that could not be resolved
#pragma warning restore CS1574 // XML comment has cref attribute that could not be resolved
#pragma warning restore CS1574 // XML comment has cref attribute that could not be resolved
#pragma warning restore CS1574 // XML comment has cref attribute that could not be resolved
		=> this.Guild.ModifyEmojiAsync(this, name, roles, reason);


#pragma warning disable CS1574 // XML comment has cref attribute that could not be resolved
#pragma warning disable CS1574 // XML comment has cref attribute that could not be resolved
#pragma warning disable CS1574 // XML comment has cref attribute that could not be resolved
#pragma warning disable CS1574 // XML comment has cref attribute that could not be resolved
#pragma warning disable CS1574 // XML comment has cref attribute that could not be resolved
	/// <summary>
	/// Deletes this emoji.
	/// </summary>
	/// <param name="reason">Reason for audit log.</param>
	/// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageEmojisAndStickers"/> permission.</exception>
	/// <exception cref="NotFoundException">Thrown when the emoji does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task DeleteAsync(string reason = null)
#pragma warning restore CS1574 // XML comment has cref attribute that could not be resolved
#pragma warning restore CS1574 // XML comment has cref attribute that could not be resolved
#pragma warning restore CS1574 // XML comment has cref attribute that could not be resolved
#pragma warning restore CS1574 // XML comment has cref attribute that could not be resolved
#pragma warning restore CS1574 // XML comment has cref attribute that could not be resolved
		=> this.Guild.DeleteEmojiAsync(this, reason);
}
