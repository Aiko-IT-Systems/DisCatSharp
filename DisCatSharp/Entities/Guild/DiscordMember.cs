// This file is part of the DisCatSharp project, a fork of DSharpPlus.
//
// Copyright (c) 2021 AITSYS
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
using System.Threading.Tasks;
using DisCatSharp.Enums;
using DisCatSharp.Net;
using DisCatSharp.Net.Abstractions;
using DisCatSharp.Net.Models;
using Newtonsoft.Json;

namespace DisCatSharp.Entities
{
    /// <summary>
    /// Represents a Discord guild member.
    /// </summary>
    public class DiscordMember : DiscordUser, IEquatable<DiscordMember>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordMember"/> class.
        /// </summary>
        internal DiscordMember()
        {
            this._roleIdsLazy = new Lazy<IReadOnlyList<ulong>>(() => new ReadOnlyCollection<ulong>(this._roleIds));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordMember"/> class.
        /// </summary>
        /// <param name="User">The user.</param>
        internal DiscordMember(DiscordUser User)
        {
            this.Discord = User.Discord;

            this.Id = User.Id;

            this._roleIds = new List<ulong>();
            this._roleIdsLazy = new Lazy<IReadOnlyList<ulong>>(() => new ReadOnlyCollection<ulong>(this._roleIds));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordMember"/> class.
        /// </summary>
        /// <param name="Mbr">The mbr.</param>
        internal DiscordMember(TransportMember Mbr)
        {
            this.Id = Mbr.User.Id;
            this.IsDeafened = Mbr.IsDeafened;
            this.IsMuted = Mbr.IsMuted;
            this.JoinedAt = Mbr.JoinedAt;
            this.Nickname = Mbr.Nickname;
            this.PremiumSince = Mbr.PremiumSince;
            this.IsPending = Mbr.IsPending;
            this.GuildAvatarHash = Mbr.GuildAvatarHash;
            this.GuildBannerHash = Mbr.GuildBannerHash;
            this.GuildBio = Mbr.GuildBio;
            this.CommunicationDisabledUntil = Mbr.CommunicationDisabledUntil;
            this._avatarHash = Mbr.AvatarHash;
            this._roleIds = Mbr.Roles ?? new List<ulong>();
            this._roleIdsLazy = new Lazy<IReadOnlyList<ulong>>(() => new ReadOnlyCollection<ulong>(this._roleIds));
        }

        /// <summary>
        /// Gets the members avatar hash.
        /// </summary>
        [JsonProperty("avatar", NullValueHandling = NullValueHandling.Ignore)]
        public virtual string GuildAvatarHash { get; internal set; }

        /// <summary>
        /// Gets the members avatar URL.
        /// </summary>
        [JsonIgnore]
        public string GuildAvatarUrl
            => string.IsNullOrWhiteSpace(this.GuildAvatarHash) ? this.User.AvatarUrl : $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Url}{Endpoints.Guilds}/{this._guildId.ToString(CultureInfo.InvariantCulture)}{Endpoints.Users}/{this.Id.ToString(CultureInfo.InvariantCulture)}{Endpoints.Avatars}/{this.GuildAvatarHash}.{(this.GuildAvatarHash.StartsWith("a_") ? "gif" : "png")}?size=1024";

        /// <summary>
        /// Gets this member's banner url.
        /// </summary>
        [JsonIgnore]
#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
        public string BannerUrl => this.User.BannerUrl;
#pragma warning restore CS0108 // Member hides inherited member; missing new keyword

        /// <summary>
        /// Gets the member's banner hash.
        /// </summary>
        [JsonIgnore]
        public override string BannerHash
        {
            get => this.User.BannerHash;
            internal set => this.User.BannerHash = value;
        }

        /// <summary>
        /// Gets the members banner hash.
        /// </summary>
        [JsonProperty("banner", NullValueHandling = NullValueHandling.Ignore)]
        public virtual string GuildBannerHash { get; internal set; }

        /// <summary>
        /// Gets the members banner URL.
        /// </summary>
        [JsonIgnore]
        public string GuildBannerUrl
            => string.IsNullOrWhiteSpace(this.GuildBannerHash) ? this.User.BannerUrl : $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Url}{Endpoints.Guilds}/{this._guildId.ToString(CultureInfo.InvariantCulture)}{Endpoints.Users}/{this.Id.ToString(CultureInfo.InvariantCulture)}{Endpoints.Banners}/{this.GuildBannerHash}.{(this.GuildBannerHash.StartsWith("a_") ? "gif" : "png")}?size=1024";

        /// <summary>
        /// The color of this member's banner. Mutually exclusive with <see cref="BannerHash"/>.
        /// </summary>
        [JsonIgnore]
        public override DiscordColor? BannerColor => this.User.BannerColor;

        /// <summary>
        /// Gets this member's nickname.
        /// </summary>
        [JsonProperty("nick", NullValueHandling = NullValueHandling.Ignore)]
        public string Nickname { get; internal set; }

        /// <summary>
        /// Gets the members guild bio.
        /// This is not available to bots tho.
        /// </summary>
        [JsonProperty("bio", NullValueHandling = NullValueHandling.Ignore)]
        public string GuildBio { get; internal set; }

        [JsonIgnore]
        internal string _avatarHash;

        /// <summary>
        /// Gets this member's display name.
        /// </summary>
        [JsonIgnore]
        public string DisplayName
            => this.Nickname ?? this.Username;

        /// <summary>
        /// List of role ids
        /// </summary>
        [JsonIgnore]
        internal IReadOnlyList<ulong> RoleIds
            => this._roleIdsLazy.Value;

        [JsonProperty("roles", NullValueHandling = NullValueHandling.Ignore)]
        internal List<ulong> _roleIds;
        [JsonIgnore]
        private readonly Lazy<IReadOnlyList<ulong>> _roleIdsLazy;

        /// <summary>
        /// Gets the list of roles associated with this member.
        /// </summary>
        [JsonIgnore]
        public IEnumerable<DiscordRole> Roles
            => this.RoleIds.Select(Id => this.Guild.GetRole(Id)).Where(X => X != null);

        /// <summary>
        /// Gets the color associated with this user's top color-giving role, otherwise 0 (no color).
        /// </summary>
        [JsonIgnore]
        public DiscordColor Color
        {
            get
            {
                var role = this.Roles.OrderByDescending(Xr => Xr.Position).FirstOrDefault(Xr => Xr.Color.Value != 0);
                return role != null ? role.Color : new DiscordColor();
            }
        }

        /// <summary>
        /// Date the user joined the guild
        /// </summary>
        [JsonProperty("joined_at", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset JoinedAt { get; internal set; }

        /// <summary>
        /// Date the user started boosting this server
        /// </summary>
        [JsonProperty("premium_since", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? PremiumSince { get; internal set; }

        /// <summary>
        /// Date until the can communicate again.
        /// </summary>
        [JsonProperty("communication_disabled_until", NullValueHandling = NullValueHandling.Include)]
        public DateTimeOffset? CommunicationDisabledUntil { get; internal set; }

        /// <summary>
        /// If the user is deafened
        /// </summary>
        [JsonProperty("is_deafened", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsDeafened { get; internal set; }

        /// <summary>
        /// If the user is muted
        /// </summary>
        [JsonProperty("is_muted", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsMuted { get; internal set; }

        /// <summary>
        /// Whether the user has not passed the guild's Membership Screening requirements yet.
        /// </summary>
        [JsonProperty("pending", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsPending { get; internal set; }

        /// <summary>
        /// Gets this member's voice state.
        /// </summary>
        [JsonIgnore]
        public DiscordVoiceState VoiceState
            => this.Discord.Guilds[this._guildId].VoiceStates.TryGetValue(this.Id, out var voiceState) ? voiceState : null;

        [JsonIgnore]
        internal ulong _guildId = 0;

        /// <summary>
        /// Gets the guild of which this member is a part of.
        /// </summary>
        [JsonIgnore]
        public DiscordGuild Guild
            => this.Discord.Guilds[this._guildId];

        /// <summary>
        /// Gets whether this member is the Guild owner.
        /// </summary>
        [JsonIgnore]
        public bool IsOwner
            => this.Id == this.Guild.OwnerId;

        /// <summary>
        /// Gets the member's position in the role hierarchy, which is the member's highest role's position. Returns <see cref="int.MaxValue"/> for the guild's owner.
        /// </summary>
        [JsonIgnore]
        public int Hierarchy
            => this.IsOwner ? int.MaxValue : this.RoleIds.Count == 0 ? 0 : this.Roles.Max(X => X.Position);

        /// <summary>
        /// Gets the permissions for the current member.
        /// </summary>
        [JsonIgnore]
        public Permissions Permissions => this.GetPermissions();

        #region Overridden user properties
        /// <summary>
        /// Gets the user.
        /// </summary>
        [JsonIgnore]
        internal DiscordUser User
            => this.Discord.UserCache[this.Id];

        /// <summary>
        /// Gets this member's username.
        /// </summary>
        public override string Username
        {
            get => this.User.Username;
            internal set => this.User.Username = value;
        }

        /// <summary>
        /// Gets the member's 4-digit discriminator.
        /// </summary>
        public override string Discriminator
        {
            get => this.User.Discriminator;
            internal set => this.User.Discriminator = value;
        }

        /// <summary>
        /// Gets the member's avatar hash.
        /// </summary>
        [JsonIgnore]
        public override string AvatarHash
        {
            get => this.User.AvatarHash;
            internal set => this.User.AvatarHash = value;
        }

        /// <summary>
        /// Gets whether the member is a bot.
        /// </summary>
        public override bool IsBot
        {
            get => this.User.IsBot;
            internal set => this.User.IsBot = value;
        }

        /// <summary>
        /// Gets the member's email address.
        /// <para>This is only present in OAuth.</para>
        /// </summary>
        public override string Email
        {
            get => this.User.Email;
            internal set => this.User.Email = value;
        }

        /// <summary>
        /// Gets whether the member has multi-factor authentication enabled.
        /// </summary>
        public override bool? MfaEnabled
        {
            get => this.User.MfaEnabled;
            internal set => this.User.MfaEnabled = value;
        }

        /// <summary>
        /// Gets whether the member is verified.
        /// <para>This is only present in OAuth.</para>
        /// </summary>
        public override bool? Verified
        {
            get => this.User.Verified;
            internal set => this.User.Verified = value;
        }

        /// <summary>
        /// Gets the member's chosen language
        /// </summary>
        public override string Locale
        {
            get => this.User.Locale;
            internal set => this.User.Locale = value;
        }

        /// <summary>
        /// Gets the user's flags.
        /// </summary>
        public override UserFlags? OAuthFlags
        {
            get => this.User.OAuthFlags;
            internal set => this.User.OAuthFlags = value;
        }

        /// <summary>
        /// Gets the member's flags for OAuth.
        /// </summary>
        public override UserFlags? Flags
        {
            get => this.User.Flags;
            internal set => this.User.Flags = value;
        }
        #endregion

        /// <summary>
        /// Creates a direct message channel to this member.
        /// </summary>
        /// <returns>Direct message channel to this member.</returns>
        /// <exception cref="Exceptions.UnauthorizedException">Thrown when the member has the bot blocked, the member is no longer in the guild, or if the member has Allow DM from server members off.</exception>
        /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
        /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task<DiscordDmChannel> CreateDmChannel()
            => this.Discord.ApiClient.CreateDmAsync(this.Id);

        /// <summary>
        /// Sends a direct message to this member. Creates a direct message channel if one does not exist already.
        /// </summary>
        /// <param name="Content">Content of the message to send.</param>
        /// <returns>The sent message.</returns>
        /// <exception cref="Exceptions.UnauthorizedException">Thrown when the member has the bot blocked, the member is no longer in the guild, or if the member has Allow DM from server members off.</exception>
        /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
        /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public async Task<DiscordMessage> SendMessageAsync(string Content)
        {
            if (this.IsBot && this.Discord.CurrentUser.IsBot)
                throw new ArgumentException("Bots cannot DM each other.");

            var chn = await this.CreateDmChannel().ConfigureAwait(false);
            return await chn.SendMessage(Content).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends a direct message to this member. Creates a direct message channel if one does not exist already.
        /// </summary>
        /// <param name="Embed">Embed to attach to the message.</param>
        /// <returns>The sent message.</returns>
        /// <exception cref="Exceptions.UnauthorizedException">Thrown when the member has the bot blocked, the member is no longer in the guild, or if the member has Allow DM from server members off.</exception>
        /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
        /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public async Task<DiscordMessage> SendMessageAsync(DiscordEmbed Embed)
        {
            if (this.IsBot && this.Discord.CurrentUser.IsBot)
                throw new ArgumentException("Bots cannot DM each other.");

            var chn = await this.CreateDmChannel().ConfigureAwait(false);
            return await chn.SendMessage(Embed).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends a direct message to this member. Creates a direct message channel if one does not exist already.
        /// </summary>
        /// <param name="Content">Content of the message to send.</param>
        /// <param name="Embed">Embed to attach to the message.</param>
        /// <returns>The sent message.</returns>
        /// <exception cref="Exceptions.UnauthorizedException">Thrown when the member has the bot blocked, the member is no longer in the guild, or if the member has Allow DM from server members off.</exception>
        /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
        /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public async Task<DiscordMessage> SendMessageAsync(string Content, DiscordEmbed Embed)
        {
            if (this.IsBot && this.Discord.CurrentUser.IsBot)
                throw new ArgumentException("Bots cannot DM each other.");

            var chn = await this.CreateDmChannel().ConfigureAwait(false);
            return await chn.SendMessage(Content, Embed).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends a direct message to this member. Creates a direct message channel if one does not exist already.
        /// </summary>
        /// <param name="Message">Builder to with the message.</param>
        /// <returns>The sent message.</returns>
        /// <exception cref="Exceptions.UnauthorizedException">Thrown when the member has the bot blocked, the member is no longer in the guild, or if the member has Allow DM from server members off.</exception>
        /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
        /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public async Task<DiscordMessage> SendMessageAsync(DiscordMessageBuilder Message)
        {
            if (this.IsBot && this.Discord.CurrentUser.IsBot)
                throw new ArgumentException("Bots cannot DM each other.");

            var chn = await this.CreateDmChannel().ConfigureAwait(false);
            return await chn.SendMessage(Message).ConfigureAwait(false);
        }

        /// <summary>
        /// Sets this member's voice mute status.
        /// </summary>
        /// <param name="Mute">Whether the member is to be muted.</param>
        /// <param name="Reason">Reason for audit logs.</param>
        /// <returns></returns>
        /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.MuteMembers"/> permission.</exception>
        /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
        /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task SetMute(bool Mute, string Reason = null)
            => this.Discord.ApiClient.ModifyGuildMember(this._guildId, this.Id, default, default, Mute, default, default, Reason);

        /// <summary>
        /// Sets this member's voice deaf status.
        /// </summary>
        /// <param name="Deaf">Whether the member is to be deafened.</param>
        /// <param name="Reason">Reason for audit logs.</param>
        /// <returns></returns>
        /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.DeafenMembers"/> permission.</exception>
        /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
        /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task SetDeaf(bool Deaf, string Reason = null)
            => this.Discord.ApiClient.ModifyGuildMember(this._guildId, this.Id, default, default, default, Deaf, default, Reason);

        /// <summary>
        /// Modifies this member.
        /// </summary>
        /// <param name="Action">Action to perform on this member.</param>
        /// <returns></returns>
        /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageNicknames"/> permission.</exception>
        /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
        /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public async Task ModifyAsync(Action<MemberEditModel> Action)
        {
            var mdl = new MemberEditModel();
            Action(mdl);

            if (mdl.VoiceChannel.HasValue && mdl.VoiceChannel.Value != null && mdl.VoiceChannel.Value.Type != ChannelType.Voice && mdl.VoiceChannel.Value.Type != ChannelType.Stage)
                throw new ArgumentException("Given channel is not a voice or stage channel.", nameof(mdl.VoiceChannel));

            if (mdl.Nickname.HasValue && this.Discord.CurrentUser.Id == this.Id)
            {
                await this.Discord.ApiClient.ModifyCurrentMemberNickname(this.Guild.Id, mdl.Nickname.Value,
                    mdl.AuditLogReason).ConfigureAwait(false);

                await this.Discord.ApiClient.ModifyGuildMember(this.Guild.Id, this.Id, Optional.FromNoValue<string>(),
                    mdl.Roles.IfPresent(E => E.Select(Xr => Xr.Id)), mdl.Muted, mdl.Deafened,
                    mdl.VoiceChannel.IfPresent(E => E?.Id), mdl.AuditLogReason).ConfigureAwait(false);
            }
            else
            {
                await this.Discord.ApiClient.ModifyGuildMember(this.Guild.Id, this.Id, mdl.Nickname,
                    mdl.Roles.IfPresent(E => E.Select(Xr => Xr.Id)), mdl.Muted, mdl.Deafened,
                    mdl.VoiceChannel.IfPresent(E => E?.Id), mdl.AuditLogReason).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Adds a timeout to a member.
        /// </summary>
        /// <param name="Until">The datetime offset to time out the user. Up to 28 days.</param>
        /// <param name="Reason">Reason for audit logs.</param>
        /// <returns></returns>
        /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ModerateMembers"/> permission.</exception>
        /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
        /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task Timeout(DateTimeOffset Until, string Reason = null)
            => Until.Subtract(DateTimeOffset.UtcNow).Days > 28 ? throw new ArgumentException("Timeout can not be longer than 28 days") : this.Discord.ApiClient.ModifyTimeout(this.Guild.Id, this.Id, Until, Reason);

        /// <summary>
        /// Adds a timeout to a member.
        /// </summary>
        /// <param name="Until">The timespan to time out the user. Up to 28 days.</param>
        /// <param name="Reason">Reason for audit logs.</param>
        /// <returns></returns>
        /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ModerateMembers"/> permission.</exception>
        /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
        /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task Timeout(TimeSpan Until, string Reason = null)
            => this.Timeout(DateTimeOffset.UtcNow + Until, Reason);

        /// <summary>
        /// Adds a timeout to a member.
        /// </summary>
        /// <param name="Until">The datetime to time out the user. Up to 28 days.</param>
        /// <param name="Reason">Reason for audit logs.</param>
        /// <returns></returns>
        /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ModerateMembers"/> permission.</exception>
        /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
        /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task Timeout(DateTime Until, string Reason = null)
            => this.Timeout(Until.ToUniversalTime() - DateTime.UtcNow, Reason);

        /// <summary>
        /// Removes the timeout from a member.
        /// </summary>
        /// <param name="Reason">Reason for audit logs.</param>
        /// <returns></returns>
        /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ModerateMembers"/> permission.</exception>
        /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
        /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task RemoveTimeout(string Reason = null) => this.Discord.ApiClient.ModifyTimeout(this.Guild.Id, this.Id, null, Reason);

        /// <summary>
        /// Grants a role to the member.
        /// </summary>
        /// <param name="Role">Role to grant.</param>
        /// <param name="Reason">Reason for audit logs.</param>
        /// <returns></returns>
        /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageRoles"/> permission.</exception>
        /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
        /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task GrantRole(DiscordRole Role, string Reason = null)
            => this.Discord.ApiClient.AddGuildMemberRole(this.Guild.Id, this.Id, Role.Id, Reason);

        /// <summary>
        /// Revokes a role from a member.
        /// </summary>
        /// <param name="Role">Role to revoke.</param>
        /// <param name="Reason">Reason for audit logs.</param>
        /// <returns></returns>
        /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageRoles"/> permission.</exception>
        /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
        /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task RevokeRole(DiscordRole Role, string Reason = null)
            => this.Discord.ApiClient.RemoveGuildMemberRole(this.Guild.Id, this.Id, Role.Id, Reason);

        /// <summary>
        /// Sets the member's roles to ones specified.
        /// </summary>
        /// <param name="Roles">Roles to set.</param>
        /// <param name="Reason">Reason for audit logs.</param>
        /// <returns></returns>
        /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageRoles"/> permission.</exception>
        /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
        /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task ReplaceRoles(IEnumerable<DiscordRole> Roles, string Reason = null)
            => this.Discord.ApiClient.ModifyGuildMember(this.Guild.Id, this.Id, default,
                new Optional<IEnumerable<ulong>>(Roles.Select(Xr => Xr.Id)), default, default, default, Reason);

        /// <summary>
        /// Bans this member from their guild.
        /// </summary>
        /// <param name="delete_message_days">How many days to remove messages from.</param>
        /// <param name="Reason">Reason for audit logs.</param>
        /// <returns></returns>
        /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.BanMembers"/> permission.</exception>
        /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
        /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task Ban(int DeleteMessageDays = 0, string Reason = null)
            => this.Guild.BanMember(this, DeleteMessageDays, Reason);

        /// <summary>
        /// Unbans this member from their guild.
        /// </summary>
        /// <exception cref = "Exceptions.UnauthorizedException" > Thrown when the client does not have the<see cref="Permissions.BanMembers"/> permission.</exception>
        /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
        /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task Unban(string Reason = null) => this.Guild.UnbanMember(this, Reason);

        /// <summary>
        /// Kicks this member from their guild.
        /// </summary>
        /// <param name="Reason">Reason for audit logs.</param>
        /// <returns></returns>
        /// <remarks>[alias="KickAsync"]</remarks>
        /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.KickMembers"/> permission.</exception>
        /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
        /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task Remove(string Reason = null)
            => this.Discord.ApiClient.RemoveGuildMember(this._guildId, this.Id, Reason);

        /// <summary>
        /// Moves this member to the specified voice channel
        /// </summary>
        /// <param name="Channel"></param>
        /// <returns></returns>
        /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.MoveMembers"/> permission.</exception>
        /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
        /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task PlaceIn(DiscordChannel Channel)
            => Channel.PlaceMemberAsync(this);

        /// <summary>
        /// Updates the member's suppress state in a stage channel.
        /// </summary>
        /// <param name="Channel">The channel the member is currently in.</param>
        /// <param name="Suppress">Toggles the member's suppress state.</param>
        /// <exception cref="System.ArgumentException">Thrown when the channel in not a voice channel.</exception>
        public async Task UpdateVoiceStateAsync(DiscordChannel Channel, bool? Suppress)
        {
            if (Channel.Type != ChannelType.Stage)
                throw new ArgumentException("Voice state can only be updated in a stage channel.");

            await this.Discord.ApiClient.UpdateUserVoiceStateAsync(this.Guild.Id, this.Id, Channel.Id, Suppress).ConfigureAwait(false);
        }

        /// <summary>
        /// Makes the user a speaker.
        /// </summary>
        /// <exception cref="System.ArgumentException">Thrown when the user is not inside an stage channel.</exception>
        /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.MuteMembers"/> permission.</exception>
        /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
        /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public async Task MakeSpeakerAsync()
        {
            var vs = this.VoiceState;
            if (vs == null || vs.Channel.Type != ChannelType.Stage)
                throw new ArgumentException("Voice state can only be updated when the user is inside an stage channel.");

            await this.Discord.ApiClient.UpdateUserVoiceStateAsync(this.Guild.Id, this.Id, vs.Channel.Id, false).ConfigureAwait(false);
        }

        /// <summary>
        /// Moves the user to audience.
        /// </summary>
        /// <exception cref="System.ArgumentException">Thrown when the user is not inside an stage channel.</exception>
        /// <exception cref="Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.MuteMembers"/> permission.</exception>
        /// <exception cref="Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
        /// <exception cref="Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public async Task MoveToAudienceAsync()
        {
            var vs = this.VoiceState;
            if (vs == null || vs.Channel.Type != ChannelType.Stage)
                throw new ArgumentException("Voice state can only be updated when the user is inside an stage channel.");

            await this.Discord.ApiClient.UpdateUserVoiceStateAsync(this.Guild.Id, this.Id, vs.Channel.Id, true).ConfigureAwait(false);
        }

        /// <summary>
        /// Calculates permissions in a given channel for this member.
        /// </summary>
        /// <param name="Channel">Channel to calculate permissions for.</param>
        /// <returns>Calculated permissions for this member in the channel.</returns>
        public Permissions PermissionsIn(DiscordChannel Channel)
            => Channel.PermissionsFor(this);

        /// <summary>
        /// Get's the current member's roles based on the sum of the permissions of their given roles.
        /// </summary>
        private Permissions GetPermissions()
        {
            if (this.Guild.OwnerId == this.Id)
                return PermissionMethods.FullPerms;

            Permissions perms;

            // assign @everyone permissions
            var everyoneRole = this.Guild.EveryoneRole;
            perms = everyoneRole.Permissions;

            // assign permissions from member's roles (in order)
            perms |= this.Roles.Aggregate(Permissions.None, (C, Role) => C | Role.Permissions);

            // Adminstrator grants all permissions and cannot be overridden
            return (perms & Permissions.Administrator) == Permissions.Administrator ? PermissionMethods.FullPerms : perms;
        }

        /// <summary>
        /// Returns a string representation of this member.
        /// </summary>
        /// <returns>String representation of this member.</returns>
        public override string ToString() => $"Member {this.Id}; {this.Username}#{this.Discriminator} ({this.DisplayName})";

        /// <summary>
        /// Checks whether this <see cref="DiscordMember"/> is equal to another object.
        /// </summary>
        /// <param name="Obj">Object to compare to.</param>
        /// <returns>Whether the object is equal to this <see cref="DiscordMember"/>.</returns>
        public override bool Equals(object Obj) => this.Equals(Obj as DiscordMember);

        /// <summary>
        /// Checks whether this <see cref="DiscordMember"/> is equal to another <see cref="DiscordMember"/>.
        /// </summary>
        /// <param name="E"><see cref="DiscordMember"/> to compare to.</param>
        /// <returns>Whether the <see cref="DiscordMember"/> is equal to this <see cref="DiscordMember"/>.</returns>
        public bool Equals(DiscordMember E) => E is not null && (ReferenceEquals(this, E) || (this.Id == E.Id && this._guildId == E._guildId));

        /// <summary>
        /// Gets the hash code for this <see cref="DiscordMember"/>.
        /// </summary>
        /// <returns>The hash code for this <see cref="DiscordMember"/>.</returns>
        public override int GetHashCode()
        {
            var hash = 13;

            hash = (hash * 7) + this.Id.GetHashCode();
            hash = (hash * 7) + this._guildId.GetHashCode();

            return hash;
        }

        /// <summary>
        /// Gets whether the two <see cref="DiscordMember"/> objects are equal.
        /// </summary>
        /// <param name="E1">First member to compare.</param>
        /// <param name="E2">Second member to compare.</param>
        /// <returns>Whether the two members are equal.</returns>
        public static bool operator ==(DiscordMember E1, DiscordMember E2)
        {
            var o1 = E1 as object;
            var o2 = E2 as object;

            return (o1 != null || o2 == null) && (o1 == null || o2 != null) && ((o1 == null && o2 == null) || (E1.Id == E2.Id && E1._guildId == E2._guildId));
        }

        /// <summary>
        /// Gets whether the two <see cref="DiscordMember"/> objects are not equal.
        /// </summary>
        /// <param name="E1">First member to compare.</param>
        /// <param name="E2">Second member to compare.</param>
        /// <returns>Whether the two members are not equal.</returns>
        public static bool operator !=(DiscordMember E1, DiscordMember E2)
            => !(E1 == E2);
    }
}
