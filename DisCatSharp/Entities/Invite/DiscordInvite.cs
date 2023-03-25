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
using System.Threading.Tasks;

using DisCatSharp.Enums;
using DisCatSharp.Exceptions;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a Discord invite.
/// </summary>
public class DiscordInvite : ObservableApiObject
{
	/// <summary>
	/// Gets the invite's code.
	/// </summary>
	[JsonProperty("code", NullValueHandling = NullValueHandling.Ignore)]
	public string Code { get; internal set; }

	/// <summary>
	/// Gets the invite's url.
	/// </summary>
	[JsonIgnore]
	public string Url => DiscordDomain.GetDomain(CoreDomain.DiscordShortlink).Url + "/" + this.Code;

	/// <summary>
	/// Gets the invite's url as Uri.
	/// </summary>
	[JsonIgnore]
	public Uri Uri => new(this.Url);

	/// <summary>
	/// Gets the guild this invite is for.
	/// </summary>
	[JsonProperty("guild", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordInviteGuild Guild { get; internal set; }

	/// <summary>
	/// Gets the channel this invite is for.
	/// </summary>
	[JsonProperty("channel", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordInviteChannel Channel { get; internal set; }

	/// <summary>
	/// Gets the target type for the voice channel this invite is for.
	/// </summary>
	[JsonProperty("target_type", NullValueHandling = NullValueHandling.Ignore)]
	public TargetType? TargetType { get; internal set; }

	/// <summary>
	/// Gets the type of this invite.
	/// </summary>
	[JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
	public InviteType Type { get; internal set; }

	/// <summary>
	/// Gets the user that is currently livestreaming.
	/// </summary>
	[JsonProperty("target_user", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordUser TargetUser { get; internal set; }

	/// <summary>
	/// Gets the embedded partial application to open for this voice channel.
	/// </summary>
	[JsonProperty("target_application", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordApplication TargetApplication { get; internal set; }

	/// <summary>
	/// Gets the approximate guild online member count for the invite.
	/// </summary>
	[JsonProperty("approximate_presence_count", NullValueHandling = NullValueHandling.Ignore)]
	public int? ApproximatePresenceCount { get; internal set; }

	/// <summary>
	/// Gets the approximate guild total member count for the invite.
	/// </summary>
	[JsonProperty("approximate_member_count", NullValueHandling = NullValueHandling.Ignore)]
	public int? ApproximateMemberCount { get; internal set; }

	/// <summary>
	/// Gets the user who created the invite.
	/// </summary>
	[JsonProperty("inviter", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordUser Inviter { get; internal set; }

	/// <summary>
	/// Gets the number of times this invite has been used.
	/// </summary>
	[JsonProperty("uses", NullValueHandling = NullValueHandling.Ignore)]
	public int Uses { get; internal set; }

	/// <summary>
	/// Gets the max number of times this invite can be used.
	/// </summary>
	[JsonProperty("max_uses", NullValueHandling = NullValueHandling.Ignore)]
	public int MaxUses { get; internal set; }

	/// <summary>
	/// Gets duration in seconds after which the invite expires.
	/// </summary>
	[JsonProperty("max_age", NullValueHandling = NullValueHandling.Ignore)]
	public int MaxAge { get; internal set; }

	/// <summary>
	/// Gets whether this invite only grants temporary membership.
	/// </summary>
	[JsonProperty("temporary", NullValueHandling = NullValueHandling.Ignore)]
	public bool IsTemporary { get; internal set; }

	/// <summary>
	/// Gets the date and time this invite was created.
	/// </summary>
	[JsonProperty("created_at", NullValueHandling = NullValueHandling.Ignore)]
	public DateTimeOffset CreatedAt { get; internal set; }

	/// <summary>
	/// Gets the date and time when this invite expires.
	/// </summary>
	[JsonProperty("expires_at", NullValueHandling = NullValueHandling.Ignore)]
	public DateTimeOffset ExpiresAt { get; internal set; }

	/// <summary>
	/// Gets the date and time when this invite got expired.
	/// </summary>
	[JsonProperty("expired_at", NullValueHandling = NullValueHandling.Ignore)]
	public DateTimeOffset ExpiredAt { get; internal set; }

	/// <summary>
	/// Gets whether this invite is revoked.
	/// </summary>
	[JsonProperty("revoked", NullValueHandling = NullValueHandling.Ignore)]
	public bool IsRevoked { get; internal set; }

	/// <summary>
	/// Gets the stage instance this invite is for.
	/// </summary>
	[JsonProperty("stage_instance", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordInviteStage Stage { get; internal set; }

	/// <summary>
	/// Gets the guild scheduled event data for the invite.
	/// </summary>
	[JsonProperty("guild_scheduled_event", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordScheduledEvent GuildScheduledEvent { get; internal set; }

	/// <summary>
	/// Gets the invites flags.
	/// </summary>
	[JsonProperty("flags", NullValueHandling = NullValueHandling.Ignore)]
	public InviteFlags Flags { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="DiscordInvite"/> class.
	/// </summary>
	internal DiscordInvite()
	{ }

	/// <summary>
	/// Deletes the invite.
	/// </summary>
	/// <param name="reason">Reason for audit logs.</param>
	/// <returns></returns>
	/// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageChannels"/> permission or the <see cref="Permissions.ManageGuild"/> permission.</exception>
	/// <exception cref="NotFoundException">Thrown when the emoji does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordInvite> DeleteAsync(string reason = null)
		=> this.Discord.ApiClient.DeleteInviteAsync(this.Code, reason);

	/// <summary>
	/// Converts this invite into an invite link.
	/// </summary>
	/// <returns>A discord.gg invite link.</returns>
	public override string ToString()
		=> $"{DiscordDomain.GetDomain(CoreDomain.DiscordShortlink).Url}/{this.Code}";
}
