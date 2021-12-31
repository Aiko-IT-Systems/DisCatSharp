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

#pragma warning disable CS0618
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DisCatSharp.Enums;
using DisCatSharp.Net;
using DisCatSharp.Net.Abstractions;
using DisCatSharp.Net.Models;
using DisCatSharp.Net.Serialization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DisCatSharp.Entities
{
    /// <summary>
    /// Represents a Discord guild.
    /// </summary>
    public class DiscordGuild : SnowflakeObject, IEquatable<DiscordGuild>
    {
        /// <summary>
        /// Gets the guild's name.
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; internal set; }

        /// <summary>
        /// Gets the guild icon's hash.
        /// </summary>
        [JsonProperty("icon", NullValueHandling = NullValueHandling.Ignore)]
        public string IconHash { get; internal set; }

        /// <summary>
        /// Gets the guild icon's url.
        /// </summary>
        [JsonIgnore]
        public string IconUrl
            => !string.IsNullOrWhiteSpace(this.IconHash) ? $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Url}{Endpoints.Icons}/{this.Id.ToString(CultureInfo.InvariantCulture)}/{this.IconHash}.{(this.IconHash.StartsWith("a_") ? "gif" : "png")}?size=1024" : null;

        /// <summary>
        /// Gets the guild splash's hash.
        /// </summary>
        [JsonProperty("splash", NullValueHandling = NullValueHandling.Ignore)]
        public string SplashHash { get; internal set; }

        /// <summary>
        /// Gets the guild splash's url.
        /// </summary>
        [JsonIgnore]
        public string SplashUrl
         => !string.IsNullOrWhiteSpace(this.SplashHash) ? $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Url}{Endpoints.Splashes}/{this.Id.ToString(CultureInfo.InvariantCulture)}/{this.SplashHash}.png?size=1024" : null;

        /// <summary>
        /// Gets the guild discovery splash's hash.
        /// </summary>
        [JsonProperty("discovery_splash", NullValueHandling = NullValueHandling.Ignore)]
        public string DiscoverySplashHash { get; internal set; }

        /// <summary>
        /// Gets the guild discovery splash's url.
        /// </summary>
        [JsonIgnore]
        public string DiscoverySplashUrl
            => !string.IsNullOrWhiteSpace(this.DiscoverySplashHash) ? $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Url}{Endpoints.GuildDiscoverySplashes}/{this.Id.ToString(CultureInfo.InvariantCulture)}/{this.DiscoverySplashHash}.png?size=1024" : null;

        /// <summary>
        /// Gets the preferred locale of this guild.
        /// <para>This is used for server discovery and notices from Discord. Defaults to en-US.</para>
        /// </summary>
        [JsonProperty("preferred_locale", NullValueHandling = NullValueHandling.Ignore)]
        public string PreferredLocale { get; internal set; }

        /// <summary>
        /// Gets the ID of the guild's owner.
        /// </summary>
        [JsonProperty("owner_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong OwnerId { get; internal set; }

        /// <summary>
        /// Gets the guild's owner.
        /// </summary>
        [JsonIgnore]
        public DiscordMember Owner
            => this.Members.TryGetValue(this.OwnerId, out var owner)
                ? owner
                : this.Discord.ApiClient.GetGuildMemberAsync(this.Id, this.OwnerId).ConfigureAwait(false).GetAwaiter().GetResult();

        /// <summary>
        /// Gets permissions for the user in the guild (does not include channel overrides)
        /// </summary>
        [JsonProperty("permissions", NullValueHandling = NullValueHandling.Ignore)]
        public Permissions? Permissions { get; set; }

        /// <summary>
        /// Gets the guild's voice region ID.
        /// </summary>
        [JsonProperty("region", NullValueHandling = NullValueHandling.Ignore)]
        internal string VoiceRegionId { get; set; }

        /// <summary>
        /// Gets the guild's voice region.
        /// </summary>
        [JsonIgnore]
        public DiscordVoiceRegion VoiceRegion
            => this.Discord.VoiceRegions[this.VoiceRegionId];

        /// <summary>
        /// Gets the guild's AFK voice channel ID.
        /// </summary>
        [JsonProperty("afk_channel_id", NullValueHandling = NullValueHandling.Ignore)]
        internal ulong AfkChannelId { get; set; } = 0;

        /// <summary>
        /// Gets the guild's AFK voice channel.
        /// </summary>
        [JsonIgnore]
        public DiscordChannel AfkChannel
            => this.GetChannel(this.AfkChannelId);

        /// <summary>
        /// List of <see cref="DisCatSharp.Entities.DiscordApplicationCommand"/>.
        /// Null if DisCatSharp.ApplicationCommands is not used or no guild commands are registered.
        /// </summary>
        [JsonIgnore]
        public ReadOnlyCollection<DiscordApplicationCommand> RegisteredApplicationCommands
            => new(this.InternalRegisteredApplicationCommands);
        [JsonIgnore]
        internal List<DiscordApplicationCommand> InternalRegisteredApplicationCommands { get; set; } = null;

        /// <summary>
        /// Gets the guild's AFK timeout.
        /// </summary>
        [JsonProperty("afk_timeout", NullValueHandling = NullValueHandling.Ignore)]
        public int AfkTimeout { get; internal set; }

        /// <summary>
        /// Gets the guild's verification level.
        /// </summary>
        [JsonProperty("verification_level", NullValueHandling = NullValueHandling.Ignore)]
        public VerificationLevel VerificationLevel { get; internal set; }

        /// <summary>
        /// Gets the guild's default notification settings.
        /// </summary>
        [JsonProperty("default_message_notifications", NullValueHandling = NullValueHandling.Ignore)]
        public DefaultMessageNotifications DefaultMessageNotifications { get; internal set; }

        /// <summary>
        /// Gets the guild's explicit content filter settings.
        /// </summary>
        [JsonProperty("explicit_content_filter")]
        public ExplicitContentFilter ExplicitContentFilter { get; internal set; }

        /// <summary>
        /// Gets the guild's nsfw level.
        /// </summary>
        [JsonProperty("nsfw_level")]
        public NsfwLevel NsfwLevel { get; internal set; }

        /// <summary>
        /// Gets the system channel id.
        /// </summary>
        [JsonProperty("system_channel_id", NullValueHandling = NullValueHandling.Include)]
        internal ulong? SystemChannelId { get; set; }

        /// <summary>
        /// Gets the channel where system messages (such as boost and welcome messages) are sent.
        /// </summary>
        [JsonIgnore]
        public DiscordChannel SystemChannel => this.SystemChannelId.HasValue
            ? this.GetChannel(this.SystemChannelId.Value)
            : null;

        /// <summary>
        /// Gets the settings for this guild's system channel.
        /// </summary>
        [JsonProperty("system_channel_flags")]
        public SystemChannelFlags SystemChannelFlags { get; internal set; }

        /// <summary>
        /// Gets whether this guild's widget is enabled.
        /// </summary>
        [JsonProperty("widget_enabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool? WidgetEnabled { get; internal set; }

        /// <summary>
        /// Gets the widget channel id.
        /// </summary>
        [JsonProperty("widget_channel_id", NullValueHandling = NullValueHandling.Ignore)]
        internal ulong? WidgetChannelId { get; set; }

        /// <summary>
        /// Gets the widget channel for this guild.
        /// </summary>
        [JsonIgnore]
        public DiscordChannel WidgetChannel => this.WidgetChannelId.HasValue
            ? this.GetChannel(this.WidgetChannelId.Value)
            : null;

        /// <summary>
        /// Gets the rules channel id.
        /// </summary>
        [JsonProperty("rules_channel_id")]
        internal ulong? RulesChannelId { get; set; }

        /// <summary>
        /// Gets the rules channel for this guild.
        /// <para>This is only available if the guild is considered "discoverable".</para>
        /// </summary>
        [JsonIgnore]
        public DiscordChannel RulesChannel => this.RulesChannelId.HasValue
            ? this.GetChannel(this.RulesChannelId.Value)
            : null;

        /// <summary>
        /// Gets the public updates channel id.
        /// </summary>
        [JsonProperty("public_updates_channel_id")]
        internal ulong? PublicUpdatesChannelId { get; set; }

        /// <summary>
        /// Gets the public updates channel (where admins and moderators receive messages from Discord) for this guild.
        /// <para>This is only available if the guild is considered "discoverable".</para>
        /// </summary>
        [JsonIgnore]
        public DiscordChannel PublicUpdatesChannel => this.PublicUpdatesChannelId.HasValue
            ? this.GetChannel(this.PublicUpdatesChannelId.Value)
            : null;

        /// <summary>
        /// Gets the application id of this guild if it is bot created.
        /// </summary>
        [JsonProperty("application_id")]
        public ulong? ApplicationId { get; internal set; }

        /// <summary>
        /// Gets a collection of this guild's roles.
        /// </summary>
        [JsonIgnore]
        public IReadOnlyDictionary<ulong, DiscordRole> Roles => new ReadOnlyConcurrentDictionary<ulong, DiscordRole>(this._roles);

        [JsonProperty("roles", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(SnowflakeArrayAsDictionaryJsonConverter))]
        internal ConcurrentDictionary<ulong, DiscordRole> _roles;

        /// <summary>
        /// Gets a collection of this guild's stickers.
        /// </summary>
        [JsonIgnore]
        public IReadOnlyDictionary<ulong, DiscordSticker> Stickers => new ReadOnlyConcurrentDictionary<ulong, DiscordSticker>(this._stickers);

        [JsonProperty("stickers", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(SnowflakeArrayAsDictionaryJsonConverter))]
        internal ConcurrentDictionary<ulong, DiscordSticker> _stickers;

        /// <summary>
        /// Gets a collection of this guild's emojis.
        /// </summary>
        [JsonIgnore]
        public IReadOnlyDictionary<ulong, DiscordEmoji> Emojis => new ReadOnlyConcurrentDictionary<ulong, DiscordEmoji>(this._emojis);

        [JsonProperty("emojis", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(SnowflakeArrayAsDictionaryJsonConverter))]
        internal ConcurrentDictionary<ulong, DiscordEmoji> _emojis;

        /// <summary>
        /// Gets a collection of this guild's features.
        /// </summary>
        [JsonProperty("features", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<string> RawFeatures { get; internal set; }

        /// <summary>
        /// Gets the guild's features.
        /// </summary>
        [JsonIgnore]
        public GuildFeatures Features => new(this);

        /// <summary>
        /// Gets the required multi-factor authentication level for this guild.
        /// </summary>
        [JsonProperty("mfa_level", NullValueHandling = NullValueHandling.Ignore)]
        public MfaLevel MfaLevel { get; internal set; }

        /// <summary>
        /// Gets this guild's join date.
        /// </summary>
        [JsonProperty("joined_at", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset JoinedAt { get; internal set; }

        /// <summary>
        /// Gets whether this guild is considered to be a large guild.
        /// </summary>
        [JsonProperty("large", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsLarge { get; internal set; }

        /// <summary>
        /// Gets whether this guild is unavailable.
        /// </summary>
        [JsonProperty("unavailable", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsUnavailable { get; internal set; }

        /// <summary>
        /// Gets the total number of members in this guild.
        /// </summary>
        [JsonProperty("member_count", NullValueHandling = NullValueHandling.Ignore)]
        public int MemberCount { get; internal set; }

        /// <summary>
        /// Gets the maximum amount of members allowed for this guild.
        /// </summary>
        [JsonProperty("max_members")]
        public int? MaxMembers { get; internal set; }

        /// <summary>
        /// Gets the maximum amount of presences allowed for this guild.
        /// </summary>
        [JsonProperty("max_presences")]
        public int? MaxPresences { get; internal set; }

#pragma warning disable CS1734
        /// <summary>
        /// Gets the approximate number of members in this guild, when using <see cref="DiscordClient.GetGuildAsync(ulong, bool?)"/> and having <paramref name = "withCounts"></paramref> set to true.
        /// </summary>
        [JsonProperty("approximate_member_count", NullValueHandling = NullValueHandling.Ignore)]
        public int? ApproximateMemberCount { get; internal set; }

        /// <summary>
        /// Gets the approximate number of presences in this guild, when using <see cref="DiscordClient.GetGuildAsync(ulong, bool?)"/> and having <paramref name = "withCounts"></paramref> set to true.
        /// </summary>
        [JsonProperty("approximate_presence_count", NullValueHandling = NullValueHandling.Ignore)]
        public int? ApproximatePresenceCount { get; internal set; }

#pragma warning restore CS1734
        /// <summary>
        /// Gets the maximum amount of users allowed per video channel.
        /// </summary>
        [JsonProperty("max_video_channel_users", NullValueHandling = NullValueHandling.Ignore)]
        public int? MaxVideoChannelUsers { get; internal set; }

        /// <summary>
        /// Gets a dictionary of all the voice states for this guilds. The key for this dictionary is the ID of the user
        /// the voice state corresponds to.
        /// </summary>
        [JsonIgnore]
        public IReadOnlyDictionary<ulong, DiscordVoiceState> VoiceStates => new ReadOnlyConcurrentDictionary<ulong, DiscordVoiceState>(this._voiceStates);

        [JsonProperty("voice_states", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(SnowflakeArrayAsDictionaryJsonConverter))]
        internal ConcurrentDictionary<ulong, DiscordVoiceState> _voiceStates;

        /// <summary>
        /// Gets a dictionary of all the members that belong to this guild. The dictionary's key is the member ID.
        /// </summary>
        [JsonIgnore] // TODO overhead of => vs Lazy? it's a struct
        public IReadOnlyDictionary<ulong, DiscordMember> Members => new ReadOnlyConcurrentDictionary<ulong, DiscordMember>(this._members);

        [JsonProperty("members", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(SnowflakeArrayAsDictionaryJsonConverter))]
        internal ConcurrentDictionary<ulong, DiscordMember> _members;

        /// <summary>
        /// Gets a dictionary of all the channels associated with this guild. The dictionary's key is the channel ID.
        /// </summary>
        [JsonIgnore]
        public IReadOnlyDictionary<ulong, DiscordChannel> Channels => new ReadOnlyConcurrentDictionary<ulong, DiscordChannel>(this._channels);

        [JsonProperty("channels", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(SnowflakeArrayAsDictionaryJsonConverter))]
        internal ConcurrentDictionary<ulong, DiscordChannel> _channels;

        internal ConcurrentDictionary<string, DiscordInvite> _invites;

        /// <summary>
        /// Gets a dictionary of all the active threads associated with this guild the user has permission to view. The dictionary's key is the channel ID.
        /// </summary>
        [JsonIgnore]
        public IReadOnlyDictionary<ulong, DiscordThreadChannel> Threads { get; internal set; }

        [JsonProperty("threads", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(SnowflakeArrayAsDictionaryJsonConverter))]
        internal ConcurrentDictionary<ulong, DiscordThreadChannel> _threads = new();

        /// <summary>
        /// Gets a dictionary of all active stage instances. The dictionary's key is the stage ID.
        /// </summary>
        [JsonIgnore]
        public IReadOnlyDictionary<ulong, DiscordStageInstance> StageInstances { get; internal set; }

        [JsonProperty("stage_instances", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(SnowflakeArrayAsDictionaryJsonConverter))]
        internal ConcurrentDictionary<ulong, DiscordStageInstance> _stageInstances = new();

        /// <summary>
        /// Gets a dictionary of all scheduled events.
        /// </summary>
        [JsonIgnore]
        public IReadOnlyDictionary<ulong, DiscordScheduledEvent> ScheduledEvents { get; internal set; }

        [JsonProperty("guild_scheduled_events", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(SnowflakeArrayAsDictionaryJsonConverter))]
        internal ConcurrentDictionary<ulong, DiscordScheduledEvent> _scheduledEvents = new();

        /// <summary>
        /// Gets the guild member for current user.
        /// </summary>
        [JsonIgnore]
        public DiscordMember CurrentMember
            => this._currentMemberLazy.Value;

        [JsonIgnore]
        private readonly Lazy<DiscordMember> _currentMemberLazy;

        /// <summary>
        /// Gets the @everyone role for this guild.
        /// </summary>
        [JsonIgnore]
        public DiscordRole EveryoneRole
            => this.GetRole(this.Id);

        [JsonIgnore]
        internal bool _isOwner;

        /// <summary>
        /// Gets whether the current user is the guild's owner.
        /// </summary>
        [JsonProperty("owner", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsOwner
        {
            get => this._isOwner || this.OwnerId == this.Discord.CurrentUser.Id;
            internal set => this._isOwner = value;
        }

        /// <summary>
        /// Gets the vanity URL code for this guild, when applicable.
        /// </summary>
        [JsonProperty("vanity_url_code")]
        public string VanityUrlCode { get; internal set; }

        /// <summary>
        /// Gets the guild description, when applicable.
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; internal set; }

        /// <summary>
        /// Gets this guild's banner hash, when applicable.
        /// </summary>
        [JsonProperty("banner")]
        public string BannerHash { get; internal set; }

        /// <summary>
        /// Gets this guild's banner in url form.
        /// </summary>
        [JsonIgnore]
        public string BannerUrl
            => !string.IsNullOrWhiteSpace(this.BannerHash) ? $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Uri}{Endpoints.Banners}/{this.Id.ToString(CultureInfo.InvariantCulture)}/{this.BannerHash}.{(this.BannerHash.StartsWith("a_") ? "gif" : "png")}" : null;

        /// <summary>
        /// Whether this guild has the community feature enabled.
        /// </summary>
        [JsonIgnore]
        public bool IsCommunity => this.Features.HasCommunityEnabled;

        /// <summary>
        /// Whether this guild has enabled the welcome screen.
        /// </summary>
        [JsonIgnore]
        public bool HasWelcomeScreen => this.Features.HasWelcomeScreenEnabled;

        /// <summary>
        /// Whether this guild has enabled membership screening.
        /// </summary>
        [JsonIgnore]
        public bool HasMemberVerificationGate => this.Features.HasMembershipScreeningEnabled;

        /// <summary>
        /// Gets this guild's premium tier (Nitro boosting).
        /// </summary>
        [JsonProperty("premium_tier")]
        public PremiumTier PremiumTier { get; internal set; }

        /// <summary>
        /// Gets the amount of members that boosted this guild.
        /// </summary>
        [JsonProperty("premium_subscription_count", NullValueHandling = NullValueHandling.Ignore)]
        public int? PremiumSubscriptionCount { get; internal set; }

        /// <summary>
        /// Whether the premium progress bar is enabled.
        /// </summary>
        [JsonProperty("premium_progress_bar_enabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool PremiumProgressBarEnabled { get; internal set; }

        /// <summary>
        /// Gets whether this guild is designated as NSFW.
        /// </summary>
        [JsonProperty("nsfw", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsNsfw { get; internal set; }

        /// <summary>
        /// Gets this guild's hub type, if applicable.
        /// </summary>
        [JsonProperty("hub_type", NullValueHandling = NullValueHandling.Ignore)]
        public HubType HubType { get; internal set; }

        /// <summary>
        /// Gets a dictionary of all by position ordered channels associated with this guild. The dictionary's key is the channel ID.
        /// </summary>
        [JsonIgnore]
        public IReadOnlyDictionary<ulong, DiscordChannel> OrderedChannels => new ReadOnlyDictionary<ulong, DiscordChannel>(this.InternalSortChannels());

        /// <summary>
        /// Sorts the channels.
        /// </summary>
        private Dictionary<ulong, DiscordChannel> InternalSortChannels()
        {
            Dictionary<ulong, DiscordChannel> keyValuePairs = new();
            var ochannels = this.GetOrderedChannels();
            foreach (var ochan in ochannels)
            {
                if (ochan.Key != 0)
                    keyValuePairs.Add(ochan.Key, this.GetChannel(ochan.Key));
                foreach (var chan in ochan.Value)
                    keyValuePairs.Add(chan.Id, chan);
            }
            return keyValuePairs;
        }

        /// <summary>
        /// Gets an ordered <see cref="DiscordChannel"/> list out of the channel cache.
        /// Returns a Dictionary where the key is an ulong and can be mapped to <see cref="ChannelType.Category"/> <see cref="DiscordChannel"/>s.
        /// Ignore the 0 key here, because that indicates that this is the "has no category" list.
        /// Each value contains a ordered list of text/news and voice/stage channels as <see cref="DiscordChannel"/>.
        /// </summary>
        /// <returns>A ordered list of categories with its channels</returns>
        public Dictionary<ulong, List<DiscordChannel>> GetOrderedChannels()
        {
            IReadOnlyList<DiscordChannel> rawChannels = this._channels.Values.ToList();

            Dictionary<ulong, List<DiscordChannel>> orderedChannels = new();

            orderedChannels.Add(0, new List<DiscordChannel>());

            foreach (var channel in rawChannels.Where(C => C.Type == ChannelType.Category).OrderBy(C => C.Position))
            {
                orderedChannels.Add(channel.Id, new List<DiscordChannel>());
            }

            foreach (var channel in rawChannels.Where(C => C.ParentId.HasValue && (C.Type == ChannelType.Text || C.Type == ChannelType.News)).OrderBy(C => C.Position))
            {
                orderedChannels[channel.ParentId.Value].Add(channel);
            }
            foreach (var channel in rawChannels.Where(C => C.ParentId.HasValue && (C.Type == ChannelType.Voice || C.Type == ChannelType.Stage)).OrderBy(C => C.Position))
            {
                orderedChannels[channel.ParentId.Value].Add(channel);
            }

            foreach (var channel in rawChannels.Where(C => !C.ParentId.HasValue && C.Type != ChannelType.Category && (C.Type == ChannelType.Text || C.Type == ChannelType.News)).OrderBy(C => C.Position))
            {
                orderedChannels[0].Add(channel);
            }
            foreach (var channel in rawChannels.Where(C => !C.ParentId.HasValue && C.Type != ChannelType.Category && (C.Type == ChannelType.Voice || C.Type == ChannelType.Stage)).OrderBy(C => C.Position))
            {
                orderedChannels[0].Add(channel);
            }

            return orderedChannels;
        }

        /// <summary>
        /// Gets an ordered <see cref="DiscordChannel"/> list.
        /// Returns a Dictionary where the key is an ulong and can be mapped to <see cref="ChannelType.Category"/> <see cref="DiscordChannel"/>s.
        /// Ignore the 0 key here, because that indicates that this is the "has no category" list.
        /// Each value contains a ordered list of text/news and voice/stage channels as <see cref="DiscordChannel"/>.
        /// </summary>
        /// <returns>A ordered list of categories with its channels</returns>
        public async Task<Dictionary<ulong, List<DiscordChannel>>> GetOrderedChannelsAsync()
        {
            var rawChannels = await this.Discord.ApiClient.GetGuildChannelsAsync(this.Id);

            Dictionary<ulong, List<DiscordChannel>> orderedChannels = new();

            orderedChannels.Add(0, new List<DiscordChannel>());

            foreach (var channel in rawChannels.Where(C => C.Type == ChannelType.Category).OrderBy(C => C.Position))
            {
                orderedChannels.Add(channel.Id, new List<DiscordChannel>());
            }

            foreach (var channel in rawChannels.Where(C => C.ParentId.HasValue && (C.Type == ChannelType.Text || C.Type == ChannelType.News)).OrderBy(C => C.Position))
            {
                orderedChannels[channel.ParentId.Value].Add(channel);
            }
            foreach (var channel in rawChannels.Where(C => C.ParentId.HasValue && (C.Type == ChannelType.Voice || C.Type == ChannelType.Stage)).OrderBy(C => C.Position))
            {
                orderedChannels[channel.ParentId.Value].Add(channel);
            }

            foreach (var channel in rawChannels.Where(C => !C.ParentId.HasValue && C.Type != ChannelType.Category && (C.Type == ChannelType.Text || C.Type == ChannelType.News)).OrderBy(C => C.Position))
            {
                orderedChannels[0].Add(channel);
            }
            foreach (var channel in rawChannels.Where(C => !C.ParentId.HasValue && C.Type != ChannelType.Category && (C.Type == ChannelType.Voice || C.Type == ChannelType.Stage)).OrderBy(C => C.Position))
            {
                orderedChannels[0].Add(channel);
            }

            return orderedChannels;
        }

        /// <summary>
        /// Whether it is synced.
        /// </summary>
        [JsonIgnore]
        internal bool IsSynced { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordGuild"/> class.
        /// </summary>
        internal DiscordGuild()
        {
            this._currentMemberLazy = new Lazy<DiscordMember>(() => (this._members != null && this._members.TryGetValue(this.Discord.CurrentUser.Id, out var member)) ? member : null);
            this._invites = new ConcurrentDictionary<string, DiscordInvite>();
            this.Threads = new ReadOnlyConcurrentDictionary<ulong, DiscordThreadChannel>(this._threads);
            this.StageInstances = new ReadOnlyConcurrentDictionary<ulong, DiscordStageInstance>(this._stageInstances);
            this.ScheduledEvents = new ReadOnlyConcurrentDictionary<ulong, DiscordScheduledEvent>(this._scheduledEvents);
        }

        #region Guild Methods

        /// <summary>
        /// Searches the current guild for members who's display name start with the specified name.
        /// </summary>
        /// <param name="Name">The name to search for.</param>
        /// <param name="Limit">The maximum amount of members to return. Max 1000. Defaults to 1.</param>
        /// <returns>The members found, if any.</returns>
        public Task<IReadOnlyList<DiscordMember>> SearchMembers(string Name, int? Limit = 1)
            => this.Discord.ApiClient.SearchMembersAsync(this.Id, Name, Limit);

        /// <summary>
        /// Adds a new member to this guild
        /// </summary>
        /// <param name="User">User to add</param>
        /// <param name="access_token">User's access token (OAuth2)</param>
        /// <param name="Nickname">new nickname</param>
        /// <param name="Roles">new roles</param>
        /// <param name="Muted">whether this user has to be muted</param>
        /// <param name="Deaf">whether this user has to be deafened</param>
        /// <returns></returns>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.CreateInstantInvite" /> permission.</exception>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the <paramref name="User"/> or <paramref name="access_token"/> is not found.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task AddMember(DiscordUser User, string AccessToken, string Nickname = null, IEnumerable<DiscordRole> Roles = null,
            bool Muted = false, bool Deaf = false)
            => this.Discord.ApiClient.AddGuildMemberAsync(this.Id, User.Id, AccessToken, Nickname, Roles, Muted, Deaf);

        /// <summary>
        /// Deletes this guild. Requires the caller to be the owner of the guild.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client is not the owner of the guild.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task Delete()
            => this.Discord.ApiClient.DeleteGuildAsync(this.Id);

        /// <summary>
        /// Modifies this guild.
        /// </summary>
        /// <param name="Action">Action to perform on this guild..</param>
        /// <returns>The modified guild object.</returns>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageGuild"/> permission.</exception>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the guild does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public async Task<DiscordGuild> ModifyAsync(Action<GuildEditModel> Action)
        {
            var mdl = new GuildEditModel();
            Action(mdl);

            var afkChannelId = Optional.FromNoValue<ulong?>();
            if (mdl.AfkChannel.HasValue && mdl.AfkChannel.Value.Type != ChannelType.Voice && mdl.AfkChannel.Value != null)
                throw new ArgumentException("AFK channel needs to be a voice channel.");
            else if (mdl.AfkChannel.HasValue && mdl.AfkChannel.Value != null)
                afkChannelId = mdl.AfkChannel.Value.Id;
            else if (mdl.AfkChannel.HasValue)
                afkChannelId = null;

            var rulesChannelId = Optional.FromNoValue<ulong?>();
            if (mdl.RulesChannel.HasValue && mdl.RulesChannel.Value != null && mdl.RulesChannel.Value.Type != ChannelType.Text && mdl.RulesChannel.Value.Type != ChannelType.News)
                throw new ArgumentException("Rules channel needs to be a text channel.");
            else if (mdl.RulesChannel.HasValue && mdl.RulesChannel.Value != null)
                rulesChannelId = mdl.RulesChannel.Value.Id;
            else if (mdl.RulesChannel.HasValue)
                rulesChannelId = null;

            var publicUpdatesChannelId = Optional.FromNoValue<ulong?>();
            if (mdl.PublicUpdatesChannel.HasValue && mdl.PublicUpdatesChannel.Value != null && mdl.PublicUpdatesChannel.Value.Type != ChannelType.Text && mdl.PublicUpdatesChannel.Value.Type != ChannelType.News)
                throw new ArgumentException("Public updates channel needs to be a text channel.");
            else if (mdl.PublicUpdatesChannel.HasValue && mdl.PublicUpdatesChannel.Value != null)
                publicUpdatesChannelId = mdl.PublicUpdatesChannel.Value.Id;
            else if (mdl.PublicUpdatesChannel.HasValue)
                publicUpdatesChannelId = null;

            var systemChannelId = Optional.FromNoValue<ulong?>();
            if (mdl.SystemChannel.HasValue && mdl.SystemChannel.Value != null && mdl.SystemChannel.Value.Type != ChannelType.Text && mdl.SystemChannel.Value.Type != ChannelType.News)
                throw new ArgumentException("Public updates channel needs to be a text channel.");
            else if (mdl.SystemChannel.HasValue && mdl.SystemChannel.Value != null)
                systemChannelId = mdl.SystemChannel.Value.Id;
            else if (mdl.SystemChannel.HasValue)
                systemChannelId = null;

            var iconb64 = Optional.FromNoValue<string>();
            if (mdl.Icon.HasValue && mdl.Icon.Value != null)
                using (var imgtool = new ImageTool(mdl.Icon.Value))
                    iconb64 = imgtool.GetBase64();
            else if (mdl.Icon.HasValue)
                iconb64 = null;

            var splashb64 = Optional.FromNoValue<string>();
            if (mdl.Splash.HasValue && mdl.Splash.Value != null)
                using (var imgtool = new ImageTool(mdl.Splash.Value))
                    splashb64 = imgtool.GetBase64();
            else if (mdl.Splash.HasValue)
                splashb64 = null;

            var bannerb64 = Optional.FromNoValue<string>();
            if (mdl.Banner.HasValue && mdl.Banner.Value != null)
                using (var imgtool = new ImageTool(mdl.Banner.Value))
                    bannerb64 = imgtool.GetBase64();
            else if (mdl.Banner.HasValue)
                bannerb64 = null;

            var discoverySplash64 = Optional.FromNoValue<string>();
            if (mdl.DiscoverySplash.HasValue && mdl.DiscoverySplash.Value != null)
                using (var imgtool = new ImageTool(mdl.DiscoverySplash.Value))
                    discoverySplash64 = imgtool.GetBase64();
            else if (mdl.DiscoverySplash.HasValue)
                discoverySplash64 = null;

            var description = Optional.FromNoValue<string>();
            if (mdl.Description.HasValue && mdl.Description.Value != null)
                description = mdl.Description;
            else if (mdl.Description.HasValue)
                description = null;

            return await this.Discord.ApiClient.ModifyGuildAsync(this.Id, mdl.Name,
                mdl.VerificationLevel, mdl.DefaultMessageNotifications, mdl.MfaLevel, mdl.ExplicitContentFilter,
                afkChannelId, mdl.AfkTimeout, iconb64, mdl.Owner.IfPresent(E => E.Id), splashb64,
                systemChannelId, mdl.SystemChannelFlags, publicUpdatesChannelId, rulesChannelId,
                description, bannerb64, discoverySplash64, mdl.PreferredLocale, mdl.PremiumProgressBarEnabled, mdl.AuditLogReason).ConfigureAwait(false);
        }

        /// <summary>
        /// Modifies the community settings async.
        /// This sets <see cref="VerificationLevel.High"/> if not highest and <see cref="ExplicitContentFilter.AllMembers"/>.
        /// </summary>
        /// <param name="Enabled">If true, enable <see cref="GuildFeatures.HasCommunityEnabled"/>.</param>
        /// <param name="RulesChannel">The rules channel.</param>
        /// <param name="PublicUpdatesChannel">The public updates channel.</param>
        /// <param name="PreferredLocale">The preferred locale. Defaults to en-US.</param>
        /// <param name="Description">The description.</param>
        /// <param name="DefaultMessageNotifications">The default message notifications. Defaults to <see cref="DefaultMessageNotifications.MentionsOnly"/></param>
        /// <param name="Reason">The auditlog reason.</param>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageGuild"/> permission.</exception>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the guild does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public async Task<DiscordGuild> ModifyCommunitySettingsAsync(bool Enabled, DiscordChannel RulesChannel = null, DiscordChannel PublicUpdatesChannel = null, string PreferredLocale = "en-US", string Description = null, DefaultMessageNotifications DefaultMessageNotifications = DefaultMessageNotifications.MentionsOnly, string Reason = null)
        {
            var verificationLevel = this.VerificationLevel;
            if (this.VerificationLevel != VerificationLevel.Highest)
            {
                verificationLevel = VerificationLevel.High;
            }

            var explicitContentFilter = ExplicitContentFilter.AllMembers;

            var rulesChannelId = Optional.FromNoValue<ulong?>();
            if (RulesChannel != null && RulesChannel.Type != ChannelType.Text && RulesChannel.Type != ChannelType.News)
                throw new ArgumentException("Rules channel needs to be a text channel.");
            else if (RulesChannel != null)
                rulesChannelId = RulesChannel.Id;
            else if (RulesChannel == null)
                rulesChannelId = null;

            var publicUpdatesChannelId = Optional.FromNoValue<ulong?>();
            if (PublicUpdatesChannel != null && PublicUpdatesChannel.Type != ChannelType.Text && PublicUpdatesChannel.Type != ChannelType.News)
                throw new ArgumentException("Public updates channel needs to be a text channel.");
            else if (PublicUpdatesChannel != null)
                publicUpdatesChannelId = PublicUpdatesChannel.Id;
            else if (PublicUpdatesChannel == null)
                publicUpdatesChannelId = null;

            List<string> features = new();
            var rfeatures = this.RawFeatures.ToList();
            if (this.RawFeatures.Contains("COMMUNITY") && Enabled)
            {
                features = rfeatures;
            }
            else if (!this.RawFeatures.Contains("COMMUNITY") && Enabled)
            {
                rfeatures.Add("COMMUNITY");
                features = rfeatures;
            }
            else if (this.RawFeatures.Contains("COMMUNITY") && !Enabled)
            {
                rfeatures.Remove("COMMUNITY");
                features = rfeatures;
            }
            else if (!this.RawFeatures.Contains("COMMUNITY") && !Enabled)
            {
                features = rfeatures;
            }

            return await this.Discord.ApiClient.ModifyGuildCommunitySettingsAsync(this.Id, features, rulesChannelId, publicUpdatesChannelId, PreferredLocale, Description, DefaultMessageNotifications, explicitContentFilter, verificationLevel, Reason).ConfigureAwait(false);
        }

        /// <summary>
        /// Timeout a specified member in this guild.
        /// </summary>
        /// <param name="member_id">Member to timeout.</param>
        /// <param name="Until">The datetime offset to time out the user. Up to 28 days.</param>
        /// <param name="Reason">Reason for audit logs.</param>
        /// <returns></returns>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ModerateMembers"/> permission.</exception>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task Timeout(ulong MemberId, DateTimeOffset Until, string Reason = null)
            => Until.Subtract(DateTimeOffset.UtcNow).Days > 28 ? throw new ArgumentException("Timeout can not be longer than 28 days") : this.Discord.ApiClient.ModifyTimeout(this.Id, MemberId, Until, Reason);

        /// <summary>
        /// Timeout a specified member in this guild.
        /// </summary>
        /// <param name="member_id">Member to timeout.</param>
        /// <param name="Until">The timespan to time out the user. Up to 28 days.</param>
        /// <param name="Reason">Reason for audit logs.</param>
        /// <returns></returns>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ModerateMembers"/> permission.</exception>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task Timeout(ulong MemberId, TimeSpan Until, string Reason = null)
            => this.Timeout(MemberId, DateTimeOffset.UtcNow + Until, Reason);

        /// <summary>
        /// Timeout a specified member in this guild.
        /// </summary>
        /// <param name="member_id">Member to timeout.</param>
        /// <param name="Until">The datetime to time out the user. Up to 28 days.</param>
        /// <param name="Reason">Reason for audit logs.</param>
        /// <returns></returns>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ModerateMembers"/> permission.</exception>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task Timeout(ulong MemberId, DateTime Until, string Reason = null)
            => this.Timeout(MemberId, Until.ToUniversalTime() - DateTime.UtcNow, Reason);

        /// <summary>
        /// Removes the timeout from a specified member in this guild.
        /// </summary>
        /// <param name="member_id">Member to remove the timeout from.</param>
        /// <param name="Reason">Reason for audit logs.</param>
        /// <returns></returns>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ModerateMembers"/> permission.</exception>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task RemoveTimeout(ulong MemberId, string Reason = null) => this.Discord.ApiClient.ModifyTimeout(this.Id, MemberId, null, Reason);

        /// <summary>
        /// Bans a specified member from this guild.
        /// </summary>
        /// <param name="Member">Member to ban.</param>
        /// <param name="delete_message_days">How many days to remove messages from.</param>
        /// <param name="Reason">Reason for audit logs.</param>
        /// <returns></returns>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.BanMembers"/> permission.</exception>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task BanMember(DiscordMember Member, int DeleteMessageDays = 0, string Reason = null)
            => this.Discord.ApiClient.CreateGuildBan(this.Id, Member.Id, DeleteMessageDays, Reason);

        /// <summary>
        /// Bans a specified user by ID. This doesn't require the user to be in this guild.
        /// </summary>
        /// <param name="user_id">ID of the user to ban.</param>
        /// <param name="delete_message_days">How many days to remove messages from.</param>
        /// <param name="Reason">Reason for audit logs.</param>
        /// <returns></returns>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.BanMembers"/> permission.</exception>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the member does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task BanMember(ulong UserId, int DeleteMessageDays = 0, string Reason = null)
            => this.Discord.ApiClient.CreateGuildBan(this.Id, UserId, DeleteMessageDays, Reason);

        /// <summary>
        /// Unbans a user from this guild.
        /// </summary>
        /// <param name="User">User to unban.</param>
        /// <param name="Reason">Reason for audit logs.</param>
        /// <returns></returns>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.BanMembers"/> permission.</exception>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the user does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task UnbanMember(DiscordUser User, string Reason = null)
            => this.Discord.ApiClient.RemoveGuildBan(this.Id, User.Id, Reason);

        /// <summary>
        /// Unbans a user by ID.
        /// </summary>
        /// <param name="user_id">ID of the user to unban.</param>
        /// <param name="Reason">Reason for audit logs.</param>
        /// <returns></returns>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.BanMembers"/> permission.</exception>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the user does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task UnbanMember(ulong UserId, string Reason = null)
            => this.Discord.ApiClient.RemoveGuildBan(this.Id, UserId, Reason);

        /// <summary>
        /// Leaves this guild.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task Leave()
            => this.Discord.ApiClient.LeaveGuild(this.Id);

        /// <summary>
        /// Gets the bans for this guild.
        /// </summary>
        /// <returns>Collection of bans in this guild.</returns>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.BanMembers"/> permission.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task<IReadOnlyList<DiscordBan>> GetBans()
            => this.Discord.ApiClient.GetGuildBansAsync(this.Id);

        /// <summary>
        /// Gets a ban for a specific user.
        /// </summary>
        /// <param name="UserId">The Id of the user to get the ban for.</param>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the specified user is not banned.</exception>
        /// <returns>The requested ban object.</returns>
        public Task<DiscordBan> GetBan(ulong UserId)
            => this.Discord.ApiClient.GetGuildBanAsync(this.Id, UserId);

        /// <summary>
        /// Gets a ban for a specific user.
        /// </summary>
        /// <param name="User">The user to get the ban for.</param>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the specified user is not banned.</exception>
        /// <returns>The requested ban object.</returns>
        public Task<DiscordBan> GetBan(DiscordUser User)
            => this.GetBan(User.Id);

        #region Sheduled Events

        /// <summary>
        /// Creates a scheduled event.
        /// </summary>
        /// <param name="Name">The name.</param>
        /// <param name="ScheduledStartTime">The scheduled start time.</param>
        /// <param name="ScheduledEndTime">The scheduled end time.</param>
        /// <param name="Channel">The channel.</param>
        /// <param name="Metadata">The metadata.</param>
        /// <param name="Description">The description.</param>
        /// <param name="Type">The type.</param>
        /// <param name="Reason">The reason.</param>
        /// <returns>A scheduled event.</returns>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the guild does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public async Task<DiscordScheduledEvent> CreateScheduledEventAsync(string Name, DateTimeOffset ScheduledStartTime, DateTimeOffset? ScheduledEndTime = null, DiscordChannel Channel = null, DiscordScheduledEventEntityMetadata Metadata = null, string Description = null, ScheduledEventEntityType Type = ScheduledEventEntityType.StageInstance, string Reason = null)
            => await this.Discord.ApiClient.CreateGuildScheduledEventAsync(this.Id, Type == ScheduledEventEntityType.External ? null : Channel?.Id, Type == ScheduledEventEntityType.External ? Metadata : null, Name, ScheduledStartTime, ScheduledEndTime.HasValue && Type == ScheduledEventEntityType.External ? ScheduledEndTime.Value : null, Description, Type, Reason);

        /// <summary>
        /// Creates a scheduled event with type <see cref="ScheduledEventEntityType.External"/>.
        /// </summary>
        /// <param name="Name">The name.</param>
        /// <param name="ScheduledStartTime">The scheduled start time.</param>
        /// <param name="ScheduledEndTime">The scheduled end time.</param>
        /// <param name="Location">The location of the external event.</param>
        /// <param name="Description">The description.</param>
        /// <param name="Reason">The reason.</param>
        /// <returns>A scheduled event.</returns>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the guild does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public async Task<DiscordScheduledEvent> CreateExternalScheduledEventAsync(string Name, DateTimeOffset ScheduledStartTime, DateTimeOffset ScheduledEndTime, string Location, string Description = null, string Reason = null)
            => await this.Discord.ApiClient.CreateGuildScheduledEventAsync(this.Id, null, new DiscordScheduledEventEntityMetadata(Location), Name, ScheduledStartTime, ScheduledEndTime, Description, ScheduledEventEntityType.External, Reason);


        /// <summary>
        /// Gets a specific scheduled events.
        /// </summary>
        /// <param name="ScheduledEventId">The Id of the event to get.</param>
        /// <param name="WithUserCount">Whether to include user count.</param>
        /// <returns>A scheduled event.</returns>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the guild does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public async Task<DiscordScheduledEvent> GetScheduledEventAsync(ulong ScheduledEventId, bool? WithUserCount = null)
            => this._scheduledEvents.TryGetValue(ScheduledEventId, out var ev) ? ev : await this.Discord.ApiClient.GetGuildScheduledEventAsync(this.Id, ScheduledEventId, WithUserCount);

        /// <summary>
        /// Gets a specific scheduled events.
        /// </summary>
        /// <param name="ScheduledEvent">The event to get.</param>
        /// <param name="WithUserCount">Whether to include user count.</param>
        /// <returns>A sheduled event.</returns>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the guild does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public async Task<DiscordScheduledEvent> GetScheduledEventAsync(DiscordScheduledEvent ScheduledEvent, bool? WithUserCount = null)
            => await this.GetScheduledEventAsync(ScheduledEvent.Id, WithUserCount);

        /// <summary>
        /// Gets the guilds scheduled events.
        /// </summary>
        /// <param name="WithUserCount">Whether to include user count.</param>
        /// <returns>A list of the guilds scheduled events.</returns>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the guild does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public async Task<IReadOnlyDictionary<ulong, DiscordScheduledEvent>> GetScheduledEventsAsync(bool? WithUserCount = null)
            => await this.Discord.ApiClient.ListGuildScheduledEventsAsync(this.Id, WithUserCount);
        #endregion

        /// <summary>
        /// Creates a new text channel in this guild.
        /// </summary>
        /// <param name="Name">Name of the new channel.</param>
        /// <param name="Parent">Category to put this channel in.</param>
        /// <param name="Topic">Topic of the channel.</param>
        /// <param name="Overwrites">Permission overwrites for this channel.</param>
        /// <param name="Nsfw">Whether the channel is to be flagged as not safe for work.</param>
        /// <param name="Reason">Reason for audit logs.</param>
        /// <param name="PerUserRateLimit">Slow mode timeout for users.</param>
        /// <returns>The newly-created channel.</returns>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageChannels"/> permission.</exception>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the guild does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task<DiscordChannel> CreateTextChannel(string Name, DiscordChannel Parent = null, Optional<string> Topic = default, IEnumerable<DiscordOverwriteBuilder> Overwrites = null, bool? Nsfw = null, Optional<int?> PerUserRateLimit = default, string Reason = null)
            => this.CreateChannel(Name, ChannelType.Text, Parent, Topic, null, null, Overwrites, Nsfw, PerUserRateLimit, null, Reason);

        /// <summary>
        /// Creates a new channel category in this guild.
        /// </summary>
        /// <param name="Name">Name of the new category.</param>
        /// <param name="Overwrites">Permission overwrites for this category.</param>
        /// <param name="Reason">Reason for audit logs.</param>
        /// <returns>The newly-created channel category.</returns>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageChannels"/> permission.</exception>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the guild does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task<DiscordChannel> CreateChannelCategory(string Name, IEnumerable<DiscordOverwriteBuilder> Overwrites = null, string Reason = null)
            => this.CreateChannel(Name, ChannelType.Category, null, Optional.FromNoValue<string>(), null, null, Overwrites, null, Optional.FromNoValue<int?>(), null, Reason);

        /// <summary>
        /// Creates a new stage channel in this guild.
        /// </summary>
        /// <param name="Name">Name of the new stage channel.</param>
        /// <param name="Overwrites">Permission overwrites for this stage channel.</param>
        /// <param name="Reason">Reason for audit logs.</param>
        /// <returns>The newly-created stage channel.</returns>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageChannels"/>.</exception>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the guild does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        /// <exception cref="System.NotSupportedException">Thrown when the guilds has not enabled community.</exception>
        public Task<DiscordChannel> CreateStageChannel(string Name, IEnumerable<DiscordOverwriteBuilder> Overwrites = null, string Reason = null)
            => this.Features.HasCommunityEnabled ? this.CreateChannel(Name, ChannelType.Stage, null, Optional.FromNoValue<string>(), null, null, Overwrites, null, Optional.FromNoValue<int?>(), null, Reason) : throw new NotSupportedException("Guild has not enabled community. Can not create a stage channel.");

        /// <summary>
        /// Creates a new news channel in this guild.
        /// </summary>
        /// <param name="Name">Name of the new stage channel.</param>
        /// <param name="Overwrites">Permission overwrites for this news channel.</param>
        /// <param name="Reason">Reason for audit logs.</param>
        /// <returns>The newly-created news channel.</returns>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageChannels"/>.</exception>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the guild does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        /// <exception cref="System.NotSupportedException">Thrown when the guilds has not enabled community.</exception>
        public Task<DiscordChannel> CreateNewsChannel(string Name, IEnumerable<DiscordOverwriteBuilder> Overwrites = null, string Reason = null)
            => this.Features.HasCommunityEnabled ? this.CreateChannel(Name, ChannelType.News, null, Optional.FromNoValue<string>(), null, null, Overwrites, null, Optional.FromNoValue<int?>(), null, Reason) : throw new NotSupportedException("Guild has not enabled community. Can not create a news channel.");

        /// <summary>
        /// Creates a new voice channel in this guild.
        /// </summary>
        /// <param name="Name">Name of the new channel.</param>
        /// <param name="Parent">Category to put this channel in.</param>
        /// <param name="Bitrate">Bitrate of the channel.</param>
        /// <param name="user_limit">Maximum number of users in the channel.</param>
        /// <param name="Overwrites">Permission overwrites for this channel.</param>
        /// <param name="QualityMode">Video quality mode of the channel.</param>
        /// <param name="Reason">Reason for audit logs.</param>
        /// <returns>The newly-created channel.</returns>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageChannels"/> permission.</exception>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the guild does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task<DiscordChannel> CreateVoiceChannel(string Name, DiscordChannel Parent = null, int? Bitrate = null, int? UserLimit = null, IEnumerable<DiscordOverwriteBuilder> Overwrites = null, VideoQualityMode? QualityMode = null, string Reason = null)
            => this.CreateChannel(Name, ChannelType.Voice, Parent, Optional.FromNoValue<string>(), Bitrate, UserLimit, Overwrites, null, Optional.FromNoValue<int?>(), QualityMode, Reason);

        /// <summary>
        /// Creates a new channel in this guild.
        /// </summary>
        /// <param name="Name">Name of the new channel.</param>
        /// <param name="Type">Type of the new channel.</param>
        /// <param name="Parent">Category to put this channel in.</param>
        /// <param name="Topic">Topic of the channel.</param>
        /// <param name="Bitrate">Bitrate of the channel. Applies to voice only.</param>
        /// <param name="UserLimit">Maximum number of users in the channel. Applies to voice only.</param>
        /// <param name="Overwrites">Permission overwrites for this channel.</param>
        /// <param name="Nsfw">Whether the channel is to be flagged as not safe for work. Applies to text only.</param>
        /// <param name="PerUserRateLimit">Slow mode timeout for users.</param>
        /// <param name="QualityMode">Video quality mode of the channel. Applies to voice only.</param>
        /// <param name="Reason">Reason for audit logs.</param>
        /// <returns>The newly-created channel.</returns>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageChannels"/> permission.</exception>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the guild does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task<DiscordChannel> CreateChannel(string Name, ChannelType Type, DiscordChannel Parent = null, Optional<string> Topic = default, int? Bitrate = null, int? UserLimit = null, IEnumerable<DiscordOverwriteBuilder> Overwrites = null, bool? Nsfw = null, Optional<int?> PerUserRateLimit = default, VideoQualityMode? QualityMode = null, string Reason = null)
        {
            // technically you can create news/store channels but not always
            return Type != ChannelType.Text && Type != ChannelType.Voice && Type != ChannelType.Category && Type != ChannelType.News && Type != ChannelType.Store && Type != ChannelType.Stage
                ? throw new ArgumentException("Channel type must be text, voice, stage, or category.", nameof(Type))
                : Type == ChannelType.Category && Parent != null
                ? throw new ArgumentException("Cannot specify parent of a channel category.", nameof(Parent))
                : this.Discord.ApiClient.CreateGuildChannelAsync(this.Id, Name, Type, Parent?.Id, Topic, Bitrate, UserLimit, Overwrites, Nsfw, PerUserRateLimit, QualityMode, Reason);
        }

        /// <summary>
        /// Gets active threads. Can contain more threads.
        /// If the result's value 'HasMore' is true, you need to recall this function to get older threads.
        /// </summary>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the thread does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task<DiscordThreadResult> GetActiveThreads()
            => this.Discord.ApiClient.GetActiveThreadsAsync(this.Id);

        // this is to commemorate the Great DAPI Channel Massacre of 2017-11-19.
        /// <summary>
        /// <para>Deletes all channels in this guild.</para>
        /// <para>Note that this is irreversible. Use carefully!</para>
        /// </summary>
        /// <returns></returns>
        public Task DeleteAllChannels()
        {
            var tasks = this.Channels.Values.Select(Xc => Xc.Delete());
            return Task.WhenAll(tasks);
        }

        /// <summary>
        /// Estimates the number of users to be pruned.
        /// </summary>
        /// <param name="Days">Minimum number of inactivity days required for users to be pruned. Defaults to 7.</param>
        /// <param name="IncludedRoles">The roles to be included in the prune.</param>
        /// <returns>Number of users that will be pruned.</returns>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.KickMembers"/> permission.</exception>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the guild does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task<int> GetPruneCount(int Days = 7, IEnumerable<DiscordRole> IncludedRoles = null)
        {
            if (IncludedRoles != null)
            {
                IncludedRoles = IncludedRoles.Where(R => R != null);
                var roleCount = IncludedRoles.Count();
                var roleArr = IncludedRoles.ToArray();
                var rawRoleIds = new List<ulong>();

                for (var i = 0; i < roleCount; i++)
                {
                    if (this._roles.ContainsKey(roleArr[i].Id))
                        rawRoleIds.Add(roleArr[i].Id);
                }

                return this.Discord.ApiClient.GetGuildPruneCountAsync(this.Id, Days, rawRoleIds);
            }

            return this.Discord.ApiClient.GetGuildPruneCountAsync(this.Id, Days, null);
        }

        /// <summary>
        /// Prunes inactive users from this guild.
        /// </summary>
        /// <param name="Days">Minimum number of inactivity days required for users to be pruned. Defaults to 7.</param>
        /// <param name="ComputePruneCount">Whether to return the prune count after this method completes. This is discouraged for larger guilds.</param>
        /// <param name="IncludedRoles">The roles to be included in the prune.</param>
        /// <param name="Reason">Reason for audit logs.</param>
        /// <returns>Number of users pruned.</returns>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageChannels"/> permission.</exception>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the guild does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task<int?> Prune(int Days = 7, bool ComputePruneCount = true, IEnumerable<DiscordRole> IncludedRoles = null, string Reason = null)
        {
            if (IncludedRoles != null)
            {
                IncludedRoles = IncludedRoles.Where(R => R != null);
                var roleCount = IncludedRoles.Count();
                var roleArr = IncludedRoles.ToArray();
                var rawRoleIds = new List<ulong>();

                for (var i = 0; i < roleCount; i++)
                {
                    if (this._roles.ContainsKey(roleArr[i].Id))
                        rawRoleIds.Add(roleArr[i].Id);
                }

                return this.Discord.ApiClient.BeginGuildPruneAsync(this.Id, Days, ComputePruneCount, rawRoleIds, Reason);
            }

            return this.Discord.ApiClient.BeginGuildPruneAsync(this.Id, Days, ComputePruneCount, null, Reason);
        }

        /// <summary>
        /// Gets integrations attached to this guild.
        /// </summary>
        /// <returns>Collection of integrations attached to this guild.</returns>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageGuild"/> permission.</exception>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the guild does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task<IReadOnlyList<DiscordIntegration>> GetIntegrations()
            => this.Discord.ApiClient.GetGuildIntegrationsAsync(this.Id);

        /// <summary>
        /// Attaches an integration from current user to this guild.
        /// </summary>
        /// <param name="Integration">Integration to attach.</param>
        /// <returns>The integration after being attached to the guild.</returns>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageGuild"/> permission.</exception>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the guild does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task<DiscordIntegration> AttachUserIntegration(DiscordIntegration Integration)
            => this.Discord.ApiClient.CreateGuildIntegrationAsync(this.Id, Integration.Type, Integration.Id);

        /// <summary>
        /// Modifies an integration in this guild.
        /// </summary>
        /// <param name="Integration">Integration to modify.</param>
        /// <param name="expire_behaviour">Number of days after which the integration expires.</param>
        /// <param name="expire_grace_period">Length of grace period which allows for renewing the integration.</param>
        /// <param name="enable_emoticons">Whether emotes should be synced from this integration.</param>
        /// <returns>The modified integration.</returns>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageGuild"/> permission.</exception>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the guild does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task<DiscordIntegration> ModifyIntegration(DiscordIntegration Integration, int ExpireBehaviour, int ExpireGracePeriod, bool EnableEmoticons)
            => this.Discord.ApiClient.ModifyGuildIntegrationAsync(this.Id, Integration.Id, ExpireBehaviour, ExpireGracePeriod, EnableEmoticons);

        /// <summary>
        /// Removes an integration from this guild.
        /// </summary>
        /// <param name="Integration">Integration to remove.</param>
        /// <returns></returns>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageGuild"/> permission.</exception>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the guild does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task DeleteIntegration(DiscordIntegration Integration)
            => this.Discord.ApiClient.DeleteGuildIntegration(this.Id, Integration);

        /// <summary>
        /// Forces re-synchronization of an integration for this guild.
        /// </summary>
        /// <param name="Integration">Integration to synchronize.</param>
        /// <returns></returns>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageGuild"/> permission.</exception>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Thrown when the guild does not exist.</exception>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Thrown when an invalid parameter was provided.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task SyncIntegration(DiscordIntegration Integration)
            => this.Discord.ApiClient.SyncGuildIntegration(this.Id, Integration.Id);

        /// <summary>
        /// Gets the voice regions for this guild.
        /// </summary>
        /// <returns>Voice regions available for this guild.</returns>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public async Task<IReadOnlyList<DiscordVoiceRegion>> ListVoiceRegionsAsync()
        {
            var vrs = await this.Discord.ApiClient.GetGuildVoiceRegionsAsync(this.Id).ConfigureAwait(false);
            foreach (var xvr in vrs)
                this.Discord.InternalVoiceRegions.TryAdd(xvr.Id, xvr);

            return vrs;
        }

        /// <summary>
        /// Gets an invite from this guild from an invite code.
        /// </summary>
        /// <param name="Code">The invite code</param>
        /// <returns>An invite, or null if not in cache.</returns>
        public DiscordInvite GetInvite(string Code)
            => this._invites.TryGetValue(Code, out var invite) ? invite : null;

        /// <summary>
        /// Gets all the invites created for all the channels in this guild.
        /// </summary>
        /// <returns>A collection of invites.</returns>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public async Task<IReadOnlyList<DiscordInvite>> GetInvitesAsync()
        {
            var res = await this.Discord.ApiClient.GetGuildInvitesAsync(this.Id).ConfigureAwait(false);

            var intents = this.Discord.Configuration.Intents;

            if (!intents.HasIntent(DiscordIntents.GuildInvites))
            {
                for (var i = 0; i < res.Count; i++)
                    this._invites[res[i].Code] = res[i];
            }

            return res;
        }

        /// <summary>
        /// Gets the vanity invite for this guild.
        /// </summary>
        /// <returns>A partial vanity invite.</returns>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageGuild"/> permission.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task<DiscordInvite> GetVanityInvite()
            => this.Discord.ApiClient.GetGuildVanityUrlAsync(this.Id);

        /// <summary>
        /// Gets all the webhooks created for all the channels in this guild.
        /// </summary>
        /// <returns>A collection of webhooks this guild has.</returns>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageWebhooks"/> permission.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task<IReadOnlyList<DiscordWebhook>> GetWebhooks()
            => this.Discord.ApiClient.GetGuildWebhooksAsync(this.Id);

        /// <summary>
        /// Gets this guild's widget image.
        /// </summary>
        /// <param name="BannerType">The format of the widget.</param>
        /// <returns>The URL of the widget image.</returns>
        public string GetWidgetImage(WidgetType BannerType = WidgetType.Shield)
        {
            var param = BannerType switch
            {
                WidgetType.Banner1 => "banner1",
                WidgetType.Banner2 => "banner2",
                WidgetType.Banner3 => "banner3",
                WidgetType.Banner4 => "banner4",
                _ => "shield",
            };
            return $"{Endpoints.BaseUri}{Endpoints.Guilds}/{this.Id}{Endpoints.WidgetPng}?style={param}";
        }

        /// <summary>
        /// Gets a member of this guild by their user ID.
        /// </summary>
        /// <param name="UserId">ID of the member to get.</param>
        /// <returns>The requested member.</returns>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public async Task<DiscordMember> GetMemberAsync(ulong UserId)
        {
            if (this._members != null && this._members.TryGetValue(UserId, out var mbr))
                return mbr;

            mbr = await this.Discord.ApiClient.GetGuildMemberAsync(this.Id, UserId).ConfigureAwait(false);

            var intents = this.Discord.Configuration.Intents;

            if (intents.HasIntent(DiscordIntents.GuildMembers))
            {
                if (this._members != null)
                {
                    this._members[UserId] = mbr;
                }
            }

            return mbr;
        }

        /// <summary>
        /// Retrieves a full list of members from Discord. This method will bypass cache.
        /// </summary>
        /// <returns>A collection of all members in this guild.</returns>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public async Task<IReadOnlyCollection<DiscordMember>> GetAllMembersAsync()
        {
            var recmbr = new HashSet<DiscordMember>();

            var recd = 1000;
            var last = 0ul;
            while (recd > 0)
            {
                var tms = await this.Discord.ApiClient.ListGuildMembersAsync(this.Id, 1000, last == 0 ? null : (ulong?)last).ConfigureAwait(false);
                recd = tms.Count;

                foreach (var xtm in tms)
                {
                    var usr = new DiscordUser(xtm.User) { Discord = this.Discord };

                    usr = this.Discord.UserCache.AddOrUpdate(xtm.User.Id, usr, (Id, Old) =>
                    {
                        Old.Username = usr.Username;
                        Old.Discord = usr.Discord;
                        Old.AvatarHash = usr.AvatarHash;

                        return Old;
                    });

                    recmbr.Add(new DiscordMember(xtm) { Discord = this.Discord, _guildId = this.Id });
                }

                var tm = tms.LastOrDefault();
                last = tm?.User.Id ?? 0;
            }

            return new ReadOnlySet<DiscordMember>(recmbr);
        }

        /// <summary>
        /// Requests that Discord send a list of guild members based on the specified arguments. This method will fire the <see cref="DiscordClient.GuildMembersChunked"/> event.
        /// <para>If no arguments aside from <paramref name="Presences"/> and <paramref name="Nonce"/> are specified, this will request all guild members.</para>
        /// </summary>
        /// <param name="Query">Filters the returned members based on what the username starts with. Either this or <paramref name="UserIds"/> must not be null.
        /// The <paramref name="Limit"/> must also be greater than 0 if this is specified.</param>
        /// <param name="Limit">Total number of members to request. This must be greater than 0 if <paramref name="Query"/> is specified.</param>
        /// <param name="Presences">Whether to include the <see cref="DisCatSharp.EventArgs.GuildMembersChunkEventArgs.Presences"/> associated with the fetched members.</param>
        /// <param name="UserIds">Whether to limit the request to the specified user ids. Either this or <paramref name="Query"/> must not be null.</param>
        /// <param name="Nonce">The unique string to identify the response.</param>
        public async Task RequestMembersAsync(string Query = "", int Limit = 0, bool? Presences = null, IEnumerable<ulong> UserIds = null, string Nonce = null)
        {
            if (this.Discord is not DiscordClient client)
                throw new InvalidOperationException("This operation is only valid for regular Discord clients.");

            if (Query == null && UserIds == null)
                throw new ArgumentException("The query and user IDs cannot both be null.");

            if (Query != null && UserIds != null)
                Query = null;

            var grgm = new GatewayRequestGuildMembers(this)
            {
                Query = Query,
                Limit = Limit >= 0 ? Limit : 0,
                Presences = Presences,
                UserIds = UserIds,
                Nonce = Nonce
            };

            var payload = new GatewayPayload
            {
                OpCode = GatewayOpCode.RequestGuildMembers,
                Data = grgm
            };

            var payloadStr = JsonConvert.SerializeObject(payload, Formatting.None);
            await client.WsSendAsync(payloadStr).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets all the channels this guild has.
        /// </summary>
        /// <returns>A collection of this guild's channels.</returns>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task<IReadOnlyList<DiscordChannel>> GetChannels()
            => this.Discord.ApiClient.GetGuildChannelsAsync(this.Id);

        /// <summary>
        /// Creates a new role in this guild.
        /// </summary>
        /// <param name="Name">Name of the role.</param>
        /// <param name="Permissions">Permissions for the role.</param>
        /// <param name="Color">Color for the role.</param>
        /// <param name="Hoist">Whether the role is to be hoisted.</param>
        /// <param name="Mentionable">Whether the role is to be mentionable.</param>
        /// <param name="Reason">Reason for audit logs.</param>
        /// <returns>The newly-created role.</returns>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageRoles"/> permission.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task<DiscordRole> CreateRole(string Name = null, Permissions? Permissions = null, DiscordColor? Color = null, bool? Hoist = null, bool? Mentionable = null, string Reason = null)
            => this.Discord.ApiClient.CreateGuildRoleAsync(this.Id, Name, Permissions, Color?.Value, Hoist, Mentionable, Reason);

        /// <summary>
        /// Gets a role from this guild by its ID.
        /// </summary>
        /// <param name="Id">ID of the role to get.</param>
        /// <returns>Requested role.</returns>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public DiscordRole GetRole(ulong Id)
            => this._roles.TryGetValue(Id, out var role) ? role : null;

        /// <summary>
        /// Gets a channel from this guild by its ID.
        /// </summary>
        /// <param name="Id">ID of the channel to get.</param>
        /// <returns>Requested channel.</returns>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public DiscordChannel GetChannel(ulong Id)
            => (this._channels != null && this._channels.TryGetValue(Id, out var channel)) ? channel : null;

        /// <summary>
        /// Gets a thread from this guild by its ID.
        /// </summary>
        /// <param name="Id">ID of the thread to get.</param>
        /// <returns>Requested thread.</returns>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public DiscordThreadChannel GetThread(ulong Id)
            => (this._threads != null && this._threads.TryGetValue(Id, out var thread)) ? thread : null;

        /// <summary>
        /// Gets audit log entries for this guild.
        /// </summary>
        /// <param name="Limit">Maximum number of entries to fetch.</param>
        /// <param name="by_member">Filter by member responsible.</param>
        /// <param name="action_type">Filter by action type.</param>
        /// <returns>A collection of requested audit log entries.</returns>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ViewAuditLog"/> permission.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public async Task<IReadOnlyList<DiscordAuditLogEntry>> GetAuditLogsAsync(int? Limit = null, DiscordMember ByMember = null, AuditLogActionType? ActionType = null)
        {
            var alrs = new List<AuditLog>();
            int ac = 1, tc = 0, rmn = 100;
            var last = 0ul;
            while (ac > 0)
            {
                rmn = Limit != null ? Limit.Value - tc : 100;
                rmn = Math.Min(100, rmn);
                if (rmn <= 0) break;

                var alr = await this.Discord.ApiClient.GetAuditLogsAsync(this.Id, rmn, null, last == 0 ? null : (ulong?)last, ByMember?.Id, (int?)ActionType).ConfigureAwait(false);
                ac = alr.Entries.Count();
                tc += ac;
                if (ac > 0)
                {
                    last = alr.Entries.Last().Id;
                    alrs.Add(alr);
                }
            }

            var amr = alrs.SelectMany(Xa => Xa.Users)
                .GroupBy(Xu => Xu.Id)
                .Select(Xgu => Xgu.First());

            foreach (var xau in amr)
            {
                if (this.Discord.UserCache.ContainsKey(xau.Id))
                    continue;

                var xtu = new TransportUser
                {
                    Id = xau.Id,
                    Username = xau.Username,
                    Discriminator = xau.Discriminator,
                    AvatarHash = xau.AvatarHash
                };
                var xu = new DiscordUser(xtu) { Discord = this.Discord };
                xu = this.Discord.UserCache.AddOrUpdate(xu.Id, xu, (Id, Old) =>
                {
                    Old.Username = xu.Username;
                    Old.Discriminator = xu.Discriminator;
                    Old.AvatarHash = xu.AvatarHash;
                    return Old;
                });
            }

            var atgse = alrs.SelectMany(Xa => Xa.ScheduledEvents)
                .GroupBy(Xse => Xse.Id)
                .Select(Xgse => Xgse.First());

            var ath = alrs.SelectMany(Xa => Xa.Threads)
                .GroupBy(Xt => Xt.Id)
                .Select(Xgt => Xgt.First());

            var aig = alrs.SelectMany(Xa => Xa.Integrations)
                .GroupBy(Xi => Xi.Id)
                .Select(Xgi => Xgi.First());

            var ahr = alrs.SelectMany(Xa => Xa.Webhooks)
                .GroupBy(Xh => Xh.Id)
                .Select(Xgh => Xgh.First());

            var ams = amr.Select(Xau => (this._members != null && this._members.TryGetValue(Xau.Id, out var member)) ? member : new DiscordMember { Discord = this.Discord, Id = Xau.Id, _guildId = this.Id });
            var amd = ams.ToDictionary(Xm => Xm.Id, Xm => Xm);

#pragma warning disable CS0219
            Dictionary<ulong, DiscordThreadChannel> dtc = null;
            Dictionary<ulong, DiscordIntegration> di = null;
            Dictionary<ulong, DiscordScheduledEvent> dse = null;
#pragma warning restore

            Dictionary<ulong, DiscordWebhook> ahd = null;
            if (ahr.Any())
            {
                var whr = await this.GetWebhooks().ConfigureAwait(false);
                var whs = whr.ToDictionary(Xh => Xh.Id, Xh => Xh);

                var amh = ahr.Select(Xah => whs.TryGetValue(Xah.Id, out var webhook) ? webhook : new DiscordWebhook { Discord = this.Discord, Name = Xah.Name, Id = Xah.Id, AvatarHash = Xah.AvatarHash, ChannelId = Xah.ChannelId, GuildId = Xah.GuildId, Token = Xah.Token });
                ahd = amh.ToDictionary(Xh => Xh.Id, Xh => Xh);
            }

            var acs = alrs.SelectMany(Xa => Xa.Entries).OrderByDescending(Xa => Xa.Id);
            var entries = new List<DiscordAuditLogEntry>();
            foreach (var xac in acs)
            {
                DiscordAuditLogEntry entry = null;
                ulong t1, t2;
                int t3, t4;
                long t5, t6;
                bool p1, p2;
                switch (xac.ActionType)
                {
                    case AuditLogActionType.GuildUpdate:
                        entry = new DiscordAuditLogGuildEntry
                        {
                            Target = this
                        };

                        var entrygld = entry as DiscordAuditLogGuildEntry;
                        foreach (var xc in xac.Changes)
                        {
                            switch (xc.Key.ToLowerInvariant())
                            {
                                case "name":
                                    entrygld.NameChange = new PropertyChange<string>
                                    {
                                        Before = xc.OldValueString,
                                        After = xc.NewValueString
                                    };
                                    break;

                                case "owner_id":
                                    entrygld.OwnerChange = new PropertyChange<DiscordMember>
                                    {
                                        Before = (this._members != null && this._members.TryGetValue(xc.OldValueUlong, out var oldMember)) ? oldMember : await this.GetMemberAsync(xc.OldValueUlong).ConfigureAwait(false),
                                        After = (this._members != null && this._members.TryGetValue(xc.NewValueUlong, out var newMember)) ? newMember : await this.GetMemberAsync(xc.NewValueUlong).ConfigureAwait(false)
                                    };
                                    break;

                                case "icon_hash":
                                    entrygld.IconChange = new PropertyChange<string>
                                    {
                                        Before = xc.OldValueString != null ? $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Url}{Endpoints.Icons}/{this.Id}/{xc.OldValueString}.webp" : null,
                                        After = xc.OldValueString != null ? $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Url}{Endpoints.Icons}/{this.Id}/{xc.NewValueString}.webp" : null
                                    };
                                    break;

                                case "verification_level":
                                    entrygld.VerificationLevelChange = new PropertyChange<VerificationLevel>
                                    {
                                        Before = (VerificationLevel)(long)xc.OldValue,
                                        After = (VerificationLevel)(long)xc.NewValue
                                    };
                                    break;

                                case "afk_channel_id":
                                    ulong.TryParse(xc.NewValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t1);
                                    ulong.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t2);

                                    entrygld.AfkChannelChange = new PropertyChange<DiscordChannel>
                                    {
                                        Before = this.GetChannel(t1) ?? new DiscordChannel { Id = t1, Discord = this.Discord, GuildId = this.Id },
                                        After = this.GetChannel(t2) ?? new DiscordChannel { Id = t1, Discord = this.Discord, GuildId = this.Id }
                                    };
                                    break;

                                case "widget_channel_id":
                                    ulong.TryParse(xc.NewValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t1);
                                    ulong.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t2);

                                    entrygld.EmbedChannelChange = new PropertyChange<DiscordChannel>
                                    {
                                        Before = this.GetChannel(t1) ?? new DiscordChannel { Id = t1, Discord = this.Discord, GuildId = this.Id },
                                        After = this.GetChannel(t2) ?? new DiscordChannel { Id = t1, Discord = this.Discord, GuildId = this.Id }
                                    };
                                    break;

                                case "splash_hash":
                                    entrygld.SplashChange = new PropertyChange<string>
                                    {
                                        Before = xc.OldValueString != null ? $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Url}{Endpoints.Splashes}/{this.Id}/{xc.OldValueString}.webp?size=2048" : null,
                                        After = xc.NewValueString != null ? $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Url}{Endpoints.Splashes}/{this.Id}/{xc.NewValueString}.webp?size=2048" : null
                                    };
                                    break;

                                case "default_message_notifications":
                                    entrygld.NotificationSettingsChange = new PropertyChange<DefaultMessageNotifications>
                                    {
                                        Before = (DefaultMessageNotifications)(long)xc.OldValue,
                                        After = (DefaultMessageNotifications)(long)xc.NewValue
                                    };
                                    break;

                                case "system_channel_id":
                                    ulong.TryParse(xc.NewValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t1);
                                    ulong.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t2);

                                    entrygld.SystemChannelChange = new PropertyChange<DiscordChannel>
                                    {
                                        Before = this.GetChannel(t1) ?? new DiscordChannel { Id = t1, Discord = this.Discord, GuildId = this.Id },
                                        After = this.GetChannel(t2) ?? new DiscordChannel { Id = t1, Discord = this.Discord, GuildId = this.Id }
                                    };
                                    break;

                                case "explicit_content_filter":
                                    entrygld.ExplicitContentFilterChange = new PropertyChange<ExplicitContentFilter>
                                    {
                                        Before = (ExplicitContentFilter)(long)xc.OldValue,
                                        After = (ExplicitContentFilter)(long)xc.NewValue
                                    };
                                    break;

                                case "mfa_level":
                                    entrygld.MfaLevelChange = new PropertyChange<MfaLevel>
                                    {
                                        Before = (MfaLevel)(long)xc.OldValue,
                                        After = (MfaLevel)(long)xc.NewValue
                                    };
                                    break;

                                case "region":
                                    entrygld.RegionChange = new PropertyChange<string>
                                    {
                                        Before = xc.OldValueString,
                                        After = xc.NewValueString
                                    };
                                    break;

                                case "premium_progress_bar_enabled":
                                    entrygld.PremiumProgressBarChange = new PropertyChange<bool>
                                    {
                                        Before = (bool)xc.OldValue,
                                        After = (bool)xc.NewValue
                                    };
                                    break;

                                default:
                                    this.Discord.Logger.LogWarning(LoggerEvents.AuditLog, "Unknown key in guild update: {0} - this should be reported to library developers", xc.Key);
                                    break;
                            }
                        }
                        break;

                    case AuditLogActionType.ChannelCreate:
                    case AuditLogActionType.ChannelDelete:
                    case AuditLogActionType.ChannelUpdate:
                        entry = new DiscordAuditLogChannelEntry
                        {
                            Target = this.GetChannel(xac.TargetId.Value) ?? new DiscordChannel { Id = xac.TargetId.Value, Discord = this.Discord, GuildId = this.Id }
                        };

                        var entrychn = entry as DiscordAuditLogChannelEntry;
                        foreach (var xc in xac.Changes)
                        {
                            switch (xc.Key.ToLowerInvariant())
                            {
                                case "name":
                                    entrychn.NameChange = new PropertyChange<string>
                                    {
                                        Before = xc.OldValue != null ? xc.OldValueString : null,
                                        After = xc.NewValue != null ? xc.NewValueString : null
                                    };
                                    break;

                                case "type":
                                    p1 = ulong.TryParse(xc.NewValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t1);
                                    p2 = ulong.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t2);

                                    entrychn.TypeChange = new PropertyChange<ChannelType?>
                                    {
                                        Before = p1 ? (ChannelType?)t1 : null,
                                        After = p2 ? (ChannelType?)t2 : null
                                    };
                                    break;

                                case "permission_overwrites":
                                    var olds = xc.OldValues?.OfType<JObject>()
                                        ?.Select(Xjo => Xjo.ToObject<DiscordOverwrite>())
                                        ?.Select(Xo => { Xo.Discord = this.Discord; return Xo; });

                                    var news = xc.NewValues?.OfType<JObject>()
                                        ?.Select(Xjo => Xjo.ToObject<DiscordOverwrite>())
                                        ?.Select(Xo => { Xo.Discord = this.Discord; return Xo; });

                                    entrychn.OverwriteChange = new PropertyChange<IReadOnlyList<DiscordOverwrite>>
                                    {
                                        Before = olds != null ? new ReadOnlyCollection<DiscordOverwrite>(new List<DiscordOverwrite>(olds)) : null,
                                        After = news != null ? new ReadOnlyCollection<DiscordOverwrite>(new List<DiscordOverwrite>(news)) : null
                                    };
                                    break;

                                case "topic":
                                    entrychn.TopicChange = new PropertyChange<string>
                                    {
                                        Before = xc.OldValueString,
                                        After = xc.NewValueString
                                    };
                                    break;

                                case "nsfw":
                                    entrychn.NsfwChange = new PropertyChange<bool?>
                                    {
                                        Before = (bool?)xc.OldValue,
                                        After = (bool?)xc.NewValue
                                    };
                                    break;

                                case "bitrate":
                                    entrychn.BitrateChange = new PropertyChange<int?>
                                    {
                                        Before = (int?)(long?)xc.OldValue,
                                        After = (int?)(long?)xc.NewValue
                                    };
                                    break;

                                case "rate_limit_per_user":
                                    entrychn.PerUserRateLimitChange = new PropertyChange<int?>
                                    {
                                        Before = (int?)(long?)xc.OldValue,
                                        After = (int?)(long?)xc.NewValue
                                    };
                                    break;

                                default:
                                    this.Discord.Logger.LogWarning(LoggerEvents.AuditLog, "Unknown key in channel update: {0} - this should be reported to library developers", xc.Key);
                                    break;
                            }
                        }
                        break;

                    case AuditLogActionType.OverwriteCreate:
                    case AuditLogActionType.OverwriteDelete:
                    case AuditLogActionType.OverwriteUpdate:
                        entry = new DiscordAuditLogOverwriteEntry
                        {
                            Target = this.GetChannel(xac.TargetId.Value)?.PermissionOverwrites.FirstOrDefault(Xo => Xo.Id == xac.Options.Id),
                            Channel = this.GetChannel(xac.TargetId.Value)
                        };

                        var entryovr = entry as DiscordAuditLogOverwriteEntry;
                        foreach (var xc in xac.Changes)
                        {
                            switch (xc.Key.ToLowerInvariant())
                            {
                                case "deny":
                                    p1 = ulong.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t1);
                                    p2 = ulong.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t2);

                                    entryovr.DenyChange = new PropertyChange<Permissions?>
                                    {
                                        Before = p1 ? (Permissions?)t1 : null,
                                        After = p2 ? (Permissions?)t2 : null
                                    };
                                    break;

                                case "allow":
                                    p1 = ulong.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t1);
                                    p2 = ulong.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t2);

                                    entryovr.AllowChange = new PropertyChange<Permissions?>
                                    {
                                        Before = p1 ? (Permissions?)t1 : null,
                                        After = p2 ? (Permissions?)t2 : null
                                    };
                                    break;

                                case "type":
                                    entryovr.TypeChange = new PropertyChange<string>
                                    {
                                        Before = xc.OldValueString,
                                        After = xc.NewValueString
                                    };
                                    break;

                                case "id":
                                    p1 = ulong.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t1);
                                    p2 = ulong.TryParse(xc.NewValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t2);

                                    entryovr.TargetIdChange = new PropertyChange<ulong?>
                                    {
                                        Before = p1 ? (ulong?)t1 : null,
                                        After = p2 ? (ulong?)t2 : null
                                    };
                                    break;

                                default:
                                    this.Discord.Logger.LogWarning(LoggerEvents.AuditLog, "Unknown key in overwrite update: {0} - this should be reported to library developers", xc.Key);
                                    break;
                            }
                        }
                        break;

                    case AuditLogActionType.Kick:
                        entry = new DiscordAuditLogKickEntry
                        {
                            Target = amd.TryGetValue(xac.TargetId.Value, out var kickMember) ? kickMember : new DiscordMember { Id = xac.TargetId.Value, Discord = this.Discord, _guildId = this.Id }
                        };
                        break;

                    case AuditLogActionType.Prune:
                        entry = new DiscordAuditLogPruneEntry
                        {
                            Days = xac.Options.DeleteMemberDays,
                            Toll = xac.Options.MembersRemoved
                        };
                        break;

                    case AuditLogActionType.Ban:
                    case AuditLogActionType.Unban:
                        entry = new DiscordAuditLogBanEntry
                        {
                            Target = amd.TryGetValue(xac.TargetId.Value, out var unbanMember) ? unbanMember : new DiscordMember { Id = xac.TargetId.Value, Discord = this.Discord, _guildId = this.Id }
                        };
                        break;

                    case AuditLogActionType.MemberUpdate:
                    case AuditLogActionType.MemberRoleUpdate:
                        entry = new DiscordAuditLogMemberUpdateEntry
                        {
                            Target = amd.TryGetValue(xac.TargetId.Value, out var roleUpdMember) ? roleUpdMember : new DiscordMember { Id = xac.TargetId.Value, Discord = this.Discord, _guildId = this.Id }
                        };

                        var entrymbu = entry as DiscordAuditLogMemberUpdateEntry;
                        foreach (var xc in xac.Changes)
                        {
                            switch (xc.Key.ToLowerInvariant())
                            {
                                case "nick":
                                    entrymbu.NicknameChange = new PropertyChange<string>
                                    {
                                        Before = xc.OldValueString,
                                        After = xc.NewValueString
                                    };
                                    break;

                                case "deaf":
                                    entrymbu.DeafenChange = new PropertyChange<bool?>
                                    {
                                        Before = (bool?)xc.OldValue,
                                        After = (bool?)xc.NewValue
                                    };
                                    break;

                                case "mute":
                                    entrymbu.MuteChange = new PropertyChange<bool?>
                                    {
                                        Before = (bool?)xc.OldValue,
                                        After = (bool?)xc.NewValue
                                    };
                                    break;
                                case "communication_disabled_until":
                                    entrymbu.CommunicationDisabledUntilChange = new PropertyChange<DateTime?>
                                    {
                                        Before = (DateTime?)xc.OldValue,
                                        After = (DateTime?)xc.NewValue
                                    };
                                    break;

                                case "$add":
                                    entrymbu.AddedRoles = new ReadOnlyCollection<DiscordRole>(xc.NewValues.Select(Xo => (ulong)Xo["id"]).Select(this.GetRole).ToList());
                                    break;

                                case "$remove":
                                    entrymbu.RemovedRoles = new ReadOnlyCollection<DiscordRole>(xc.NewValues.Select(Xo => (ulong)Xo["id"]).Select(this.GetRole).ToList());
                                    break;

                                default:
                                    this.Discord.Logger.LogWarning(LoggerEvents.AuditLog, "Unknown key in member update: {0} - this should be reported to library developers", xc.Key);
                                    break;
                            }
                        }
                        break;

                    case AuditLogActionType.RoleCreate:
                    case AuditLogActionType.RoleDelete:
                    case AuditLogActionType.RoleUpdate:
                        entry = new DiscordAuditLogRoleUpdateEntry
                        {
                            Target = this.GetRole(xac.TargetId.Value) ?? new DiscordRole { Id = xac.TargetId.Value, Discord = this.Discord }
                        };

                        var entryrol = entry as DiscordAuditLogRoleUpdateEntry;
                        foreach (var xc in xac.Changes)
                        {
                            switch (xc.Key.ToLowerInvariant())
                            {
                                case "name":
                                    entryrol.NameChange = new PropertyChange<string>
                                    {
                                        Before = xc.OldValueString,
                                        After = xc.NewValueString
                                    };
                                    break;

                                case "color":
                                    p1 = int.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t3);
                                    p2 = int.TryParse(xc.NewValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t4);

                                    entryrol.ColorChange = new PropertyChange<int?>
                                    {
                                        Before = p1 ? (int?)t3 : null,
                                        After = p2 ? (int?)t4 : null
                                    };
                                    break;

                                case "permissions":
                                    entryrol.PermissionChange = new PropertyChange<Permissions?>
                                    {
                                        Before = xc.OldValue != null ? (Permissions?)long.Parse((string)xc.OldValue) : null,
                                        After = xc.NewValue != null ? (Permissions?)long.Parse((string)xc.NewValue) : null
                                    };
                                    break;

                                case "position":
                                    entryrol.PositionChange = new PropertyChange<int?>
                                    {
                                        Before = xc.OldValue != null ? (int?)(long)xc.OldValue : null,
                                        After = xc.NewValue != null ? (int?)(long)xc.NewValue : null,
                                    };
                                    break;

                                case "mentionable":
                                    entryrol.MentionableChange = new PropertyChange<bool?>
                                    {
                                        Before = xc.OldValue != null ? (bool?)xc.OldValue : null,
                                        After = xc.NewValue != null ? (bool?)xc.NewValue : null
                                    };
                                    break;

                                case "hoist":
                                    entryrol.HoistChange = new PropertyChange<bool?>
                                    {
                                        Before = (bool?)xc.OldValue,
                                        After = (bool?)xc.NewValue
                                    };
                                    break;

                                default:
                                    this.Discord.Logger.LogWarning(LoggerEvents.AuditLog, "Unknown key in role update: {0} - this should be reported to library developers", xc.Key);
                                    break;
                            }
                        }
                        break;

                    case AuditLogActionType.InviteCreate:
                    case AuditLogActionType.InviteDelete:
                    case AuditLogActionType.InviteUpdate:
                        entry = new DiscordAuditLogInviteEntry();

                        var inv = new DiscordInvite
                        {
                            Discord = this.Discord,
                            Guild = new DiscordInviteGuild
                            {
                                Discord = this.Discord,
                                Id = this.Id,
                                Name = this.Name,
                                SplashHash = this.SplashHash
                            }
                        };

                        var entryinv = entry as DiscordAuditLogInviteEntry;
                        foreach (var xc in xac.Changes)
                        {
                            switch (xc.Key.ToLowerInvariant())
                            {
                                case "max_age":
                                    p1 = int.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t3);
                                    p2 = int.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t4);

                                    entryinv.MaxAgeChange = new PropertyChange<int?>
                                    {
                                        Before = p1 ? (int?)t3 : null,
                                        After = p2 ? (int?)t4 : null
                                    };
                                    break;

                                case "code":
                                    inv.Code = xc.OldValueString ?? xc.NewValueString;

                                    entryinv.CodeChange = new PropertyChange<string>
                                    {
                                        Before = xc.OldValueString,
                                        After = xc.NewValueString
                                    };
                                    break;

                                case "temporary":
                                    entryinv.TemporaryChange = new PropertyChange<bool?>
                                    {
                                        Before = xc.OldValue != null ? (bool?)xc.OldValue : null,
                                        After = xc.NewValue != null ? (bool?)xc.NewValue : null
                                    };
                                    break;

                                case "inviter_id":
                                    p1 = ulong.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t1);
                                    p2 = ulong.TryParse(xc.NewValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t2);

                                    entryinv.InviterChange = new PropertyChange<DiscordMember>
                                    {
                                        Before = amd.TryGetValue(t1, out var propBeforeMember) ? propBeforeMember : new DiscordMember { Id = t1, Discord = this.Discord, _guildId = this.Id },
                                        After = amd.TryGetValue(t2, out var propAfterMember) ? propAfterMember : new DiscordMember { Id = t1, Discord = this.Discord, _guildId = this.Id },
                                    };
                                    break;

                                case "channel_id":
                                    p1 = ulong.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t1);
                                    p2 = ulong.TryParse(xc.NewValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t2);

                                    entryinv.ChannelChange = new PropertyChange<DiscordChannel>
                                    {
                                        Before = p1 ? this.GetChannel(t1) ?? new DiscordChannel { Id = t1, Discord = this.Discord, GuildId = this.Id } : null,
                                        After = p2 ? this.GetChannel(t2) ?? new DiscordChannel { Id = t1, Discord = this.Discord, GuildId = this.Id } : null
                                    };

                                    var ch = entryinv.ChannelChange.Before ?? entryinv.ChannelChange.After;
                                    var cht = ch?.Type;
                                    inv.Channel = new DiscordInviteChannel
                                    {
                                        Discord = this.Discord,
                                        Id = p1 ? t1 : t2,
                                        Name = ch?.Name,
                                        Type = cht != null ? cht.Value : ChannelType.Unknown
                                    };
                                    break;

                                case "uses":
                                    p1 = int.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t3);
                                    p2 = int.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t4);

                                    entryinv.UsesChange = new PropertyChange<int?>
                                    {
                                        Before = p1 ? (int?)t3 : null,
                                        After = p2 ? (int?)t4 : null
                                    };
                                    break;

                                case "max_uses":
                                    p1 = int.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t3);
                                    p2 = int.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t4);

                                    entryinv.MaxUsesChange = new PropertyChange<int?>
                                    {
                                        Before = p1 ? (int?)t3 : null,
                                        After = p2 ? (int?)t4 : null
                                    };
                                    break;

                                // TODO: Add changes for target application

                                default:
                                    this.Discord.Logger.LogWarning(LoggerEvents.AuditLog, "Unknown key in invite update: {0} - this should be reported to library developers", xc.Key);
                                    break;
                            }
                        }

                        entryinv.Target = inv;
                        break;

                    case AuditLogActionType.WebhookCreate:
                    case AuditLogActionType.WebhookDelete:
                    case AuditLogActionType.WebhookUpdate:
                        entry = new DiscordAuditLogWebhookEntry
                        {
                            Target = ahd.TryGetValue(xac.TargetId.Value, out var webhook) ? webhook : new DiscordWebhook { Id = xac.TargetId.Value, Discord = this.Discord }
                        };

                        var entrywhk = entry as DiscordAuditLogWebhookEntry;
                        foreach (var xc in xac.Changes)
                        {
                            switch (xc.Key.ToLowerInvariant())
                            {
                                case "name":
                                    entrywhk.NameChange = new PropertyChange<string>
                                    {
                                        Before = xc.OldValueString,
                                        After = xc.NewValueString
                                    };
                                    break;

                                case "channel_id":
                                    p1 = ulong.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t1);
                                    p2 = ulong.TryParse(xc.NewValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t2);

                                    entrywhk.ChannelChange = new PropertyChange<DiscordChannel>
                                    {
                                        Before = p1 ? this.GetChannel(t1) ?? new DiscordChannel { Id = t1, Discord = this.Discord, GuildId = this.Id } : null,
                                        After = p2 ? this.GetChannel(t2) ?? new DiscordChannel { Id = t1, Discord = this.Discord, GuildId = this.Id } : null
                                    };
                                    break;

                                case "type": // ???
                                    p1 = int.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t3);
                                    p2 = int.TryParse(xc.NewValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t4);

                                    entrywhk.TypeChange = new PropertyChange<int?>
                                    {
                                        Before = p1 ? (int?)t3 : null,
                                        After = p2 ? (int?)t4 : null
                                    };
                                    break;

                                case "avatar_hash":
                                    entrywhk.AvatarHashChange = new PropertyChange<string>
                                    {
                                        Before = xc.OldValueString,
                                        After = xc.NewValueString
                                    };
                                    break;

                                default:
                                    this.Discord.Logger.LogWarning(LoggerEvents.AuditLog, "Unknown key in webhook update: {0} - this should be reported to library developers", xc.Key);
                                    break;
                            }
                        }
                        break;

                    case AuditLogActionType.EmojiCreate:
                    case AuditLogActionType.EmojiDelete:
                    case AuditLogActionType.EmojiUpdate:
                        entry = new DiscordAuditLogEmojiEntry
                        {
                            Target = this._emojis.TryGetValue(xac.TargetId.Value, out var target) ? target : new DiscordEmoji { Id = xac.TargetId.Value, Discord = this.Discord }
                        };

                        var entryemo = entry as DiscordAuditLogEmojiEntry;
                        foreach (var xc in xac.Changes)
                        {
                            switch (xc.Key.ToLowerInvariant())
                            {
                                case "name":
                                    entryemo.NameChange = new PropertyChange<string>
                                    {
                                        Before = xc.OldValueString,
                                        After = xc.NewValueString
                                    };
                                    break;

                                default:
                                    this.Discord.Logger.LogWarning(LoggerEvents.AuditLog, "Unknown key in emote update: {0} - this should be reported to library developers", xc.Key);
                                    break;
                            }
                        }
                        break;

                    case AuditLogActionType.StageInstanceCreate:
                    case AuditLogActionType.StageInstanceDelete:
                    case AuditLogActionType.StageInstanceUpdate:
                        entry = new DiscordAuditLogStageEntry
                        {
                            Target = this._stageInstances.TryGetValue(xac.TargetId.Value, out var stage) ? stage : new DiscordStageInstance { Id = xac.TargetId.Value, Discord = this.Discord }
                        };

                        var entrysta = entry as DiscordAuditLogStageEntry;
                        foreach (var xc in xac.Changes)
                        {
                            switch (xc.Key.ToLowerInvariant())
                            {
                                case "topic":
                                    entrysta.TopicChange = new PropertyChange<string>
                                    {
                                        Before = xc.OldValueString,
                                        After = xc.NewValueString
                                    };
                                    break;
                                case "privacy_level":
                                    entrysta.PrivacyLevelChange = new PropertyChange<StagePrivacyLevel?>
                                    {
                                        Before = long.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t5) ? (StagePrivacyLevel?)t5 : null,
                                        After = long.TryParse(xc.NewValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t6) ? (StagePrivacyLevel?)t6 : null,
                                    };
                                    break;

                                default:
                                    this.Discord.Logger.LogWarning(LoggerEvents.AuditLog, "Unknown key in stage instance update: {0} - this should be reported to library developers", xc.Key);
                                    break;
                            }
                        }
                        break;

                    case AuditLogActionType.StickerCreate:
                    case AuditLogActionType.StickerDelete:
                    case AuditLogActionType.StickerUpdate:
                        entry = new DiscordAuditLogStickerEntry
                        {
                            Target = this._stickers.TryGetValue(xac.TargetId.Value, out var sticker) ? sticker : new DiscordSticker { Id = xac.TargetId.Value, Discord = this.Discord }
                        };

                        var entrysti = entry as DiscordAuditLogStickerEntry;
                        foreach (var xc in xac.Changes)
                        {
                            switch (xc.Key.ToLowerInvariant())
                            {
                                case "name":
                                    entrysti.NameChange = new PropertyChange<string>
                                    {
                                        Before = xc.OldValueString,
                                        After = xc.NewValueString
                                    };
                                    break;
                                case "description":
                                    entrysti.DescriptionChange = new PropertyChange<string>
                                    {
                                        Before = xc.OldValueString,
                                        After = xc.NewValueString
                                    };
                                    break;
                                case "tags":
                                    entrysti.TagsChange = new PropertyChange<string>
                                    {
                                        Before = xc.OldValueString,
                                        After = xc.NewValueString
                                    };
                                    break;
                                case "guild_id":
                                    entrysti.GuildIdChange = new PropertyChange<ulong?>
                                    {
                                        Before = ulong.TryParse(xc.OldValueString, out var ogid) ? ogid : null,
                                        After = ulong.TryParse(xc.NewValueString, out var ngid) ? ngid : null
                                    };
                                    break;
                                case "available":
                                    entrysti.AvailabilityChange = new PropertyChange<bool?>
                                    {
                                        Before = (bool?)xc.OldValue,
                                        After = (bool?)xc.NewValue,
                                    };
                                    break;
                                case "asset":
                                    entrysti.AssetChange = new PropertyChange<string>
                                    {
                                        Before = xc.OldValueString,
                                        After = xc.NewValueString
                                    };
                                    break;
                                case "id":
                                    entrysti.IdChange = new PropertyChange<ulong?>
                                    {
                                        Before = ulong.TryParse(xc.OldValueString, out var oid) ? oid : null,
                                        After = ulong.TryParse(xc.NewValueString, out var nid) ? nid : null
                                    };
                                    break;
                                case "type":
                                    p1 = long.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t5);
                                    p2 = long.TryParse(xc.NewValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t6);
                                    entrysti.TypeChange = new PropertyChange<StickerType?>
                                    {
                                        Before = p1 ? (StickerType?)t5 : null,
                                        After = p2 ? (StickerType?)t6 : null
                                    };
                                    break;
                                case "format_type":
                                    p1 = long.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t5);
                                    p2 = long.TryParse(xc.NewValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t6);
                                    entrysti.FormatChange = new PropertyChange<StickerFormat?>
                                    {
                                        Before = p1 ? (StickerFormat?)t5 : null,
                                        After = p2 ? (StickerFormat?)t6 : null
                                    };
                                    break;

                                default:
                                    this.Discord.Logger.LogWarning(LoggerEvents.AuditLog, "Unknown key in sticker update: {0} - this should be reported to library developers", xc.Key);
                                    break;
                            }
                        }
                        break;



                    case AuditLogActionType.MessageDelete:
                    case AuditLogActionType.MessageBulkDelete:
                    {
                        entry = new DiscordAuditLogMessageEntry();

                        var entrymsg = entry as DiscordAuditLogMessageEntry;

                        if (xac.Options != null)
                        {
                            entrymsg.Channel = this.GetChannel(xac.Options.ChannelId) ?? new DiscordChannel { Id = xac.Options.ChannelId, Discord = this.Discord, GuildId = this.Id };
                            entrymsg.MessageCount = xac.Options.Count;
                        }

                        if (entrymsg.Channel != null)
                        {
                            entrymsg.Target = this.Discord is DiscordClient dc
                                && dc.MessageCache != null
                                && dc.MessageCache.TryGet(Xm => Xm.Id == xac.TargetId.Value && Xm.ChannelId == entrymsg.Channel.Id, out var msg)
                                ? msg
                                : new DiscordMessage { Discord = this.Discord, Id = xac.TargetId.Value };
                        }
                        break;
                    }

                    case AuditLogActionType.MessagePin:
                    case AuditLogActionType.MessageUnpin:
                    {
                        entry = new DiscordAuditLogMessagePinEntry();

                        var entrypin = entry as DiscordAuditLogMessagePinEntry;

                        if (this.Discord is not DiscordClient dc)
                        {
                            break;
                        }

                        if (xac.Options != null)
                        {
                            DiscordMessage message = default;
                            dc.MessageCache?.TryGet(X => X.Id == xac.Options.MessageId && X.ChannelId == xac.Options.ChannelId, out message);

                            entrypin.Channel = this.GetChannel(xac.Options.ChannelId) ?? new DiscordChannel { Id = xac.Options.ChannelId, Discord = this.Discord, GuildId = this.Id };
                            entrypin.Message = message ?? new DiscordMessage { Id = xac.Options.MessageId, Discord = this.Discord };
                        }

                        if (xac.TargetId.HasValue)
                        {
                            dc.UserCache.TryGetValue(xac.TargetId.Value, out var user);
                            entrypin.Target = user ?? new DiscordUser { Id = user.Id, Discord = this.Discord };
                        }

                        break;
                    }

                    case AuditLogActionType.BotAdd:
                    {
                        entry = new DiscordAuditLogBotAddEntry();

                        if (!(this.Discord is DiscordClient dc && xac.TargetId.HasValue))
                        {
                            break;
                        }

                        dc.UserCache.TryGetValue(xac.TargetId.Value, out var bot);
                        (entry as DiscordAuditLogBotAddEntry).TargetBot = bot ?? new DiscordUser { Id = xac.TargetId.Value, Discord = this.Discord };

                        break;
                    }

                    case AuditLogActionType.MemberMove:
                        entry = new DiscordAuditLogMemberMoveEntry();

                        if (xac.Options == null)
                        {
                            break;
                        }

                        var moveentry = entry as DiscordAuditLogMemberMoveEntry;

                        moveentry.UserCount = xac.Options.Count;
                        moveentry.Channel = this.GetChannel(xac.Options.ChannelId) ?? new DiscordChannel { Id = xac.Options.ChannelId, Discord = this.Discord, GuildId = this.Id };
                        break;

                    case AuditLogActionType.MemberDisconnect:
                        entry = new DiscordAuditLogMemberDisconnectEntry
                        {
                            UserCount = xac.Options?.Count ?? 0
                        };
                        break;

                    case AuditLogActionType.IntegrationCreate:
                    case AuditLogActionType.IntegrationDelete:
                    case AuditLogActionType.IntegrationUpdate:
                        entry = new DiscordAuditLogIntegrationEntry();

                        var integentry = entry as DiscordAuditLogIntegrationEntry;
                        foreach (var xc in xac.Changes)
                        {
                            switch (xc.Key.ToLowerInvariant())
                            {
                                case "enable_emoticons":
                                    integentry.EnableEmoticons = new PropertyChange<bool?>
                                    {
                                        Before = (bool?)xc.OldValue,
                                        After = (bool?)xc.NewValue
                                    };
                                    break;
                                case "expire_behavior":
                                    integentry.ExpireBehavior = new PropertyChange<int?>
                                    {
                                        Before = (int?)xc.OldValue,
                                        After = (int?)xc.NewValue
                                    };
                                    break;
                                case "expire_grace_period":
                                    integentry.ExpireBehavior = new PropertyChange<int?>
                                    {
                                        Before = (int?)xc.OldValue,
                                        After = (int?)xc.NewValue
                                    };
                                    break;

                                default:
                                    this.Discord.Logger.LogWarning(LoggerEvents.AuditLog, "Unknown key in integration update: {0} - this should be reported to library developers", xc.Key);
                                    break;
                            }
                        }
                        break;

                    case AuditLogActionType.ThreadCreate:
                    case AuditLogActionType.ThreadDelete:
                    case AuditLogActionType.ThreadUpdate:
                        entry = new DiscordAuditLogThreadEntry
                        {
                            Target = this._threads.TryGetValue(xac.TargetId.Value, out var thread) ? thread : new DiscordThreadChannel { Id = xac.TargetId.Value, Discord = this.Discord }
                        };

                        var entrythr = entry as DiscordAuditLogThreadEntry;
                        foreach (var xc in xac.Changes)
                        {
                            switch (xc.Key.ToLowerInvariant())
                            {
                                case "name":
                                    entrythr.NameChange = new PropertyChange<string>
                                    {
                                        Before = xc.OldValue != null ? xc.OldValueString : null,
                                        After = xc.NewValue != null ? xc.NewValueString : null
                                    };
                                    break;

                                case "type":
                                    p1 = ulong.TryParse(xc.NewValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t1);
                                    p2 = ulong.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t2);

                                    entrythr.TypeChange = new PropertyChange<ChannelType?>
                                    {
                                        Before = p1 ? (ChannelType?)t1 : null,
                                        After = p2 ? (ChannelType?)t2 : null
                                    };
                                    break;

                                case "archived":
                                    entrythr.ArchivedChange = new PropertyChange<bool?>
                                    {
                                        Before = xc.OldValue != null ? (bool?)xc.OldValue : null,
                                        After = xc.NewValue != null ? (bool?)xc.NewValue : null
                                    };
                                    break;

                                case "locked":
                                    entrythr.LockedChange = new PropertyChange<bool?>
                                    {
                                        Before = xc.OldValue != null ? (bool?)xc.OldValue : null,
                                        After = xc.NewValue != null ? (bool?)xc.NewValue : null
                                    };
                                    break;

                                case "auto_archive_duration":
                                    p1 = long.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t5);
                                    p2 = long.TryParse(xc.NewValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t6);

                                    entrythr.AutoArchiveDurationChange = new PropertyChange<ThreadAutoArchiveDuration?>
                                    {
                                        Before = p1 ? (ThreadAutoArchiveDuration?)t5 : null,
                                        After = p2 ? (ThreadAutoArchiveDuration?)t6 : null
                                    };
                                    break;

                                case "rate_limit_per_user":
                                    entrythr.PerUserRateLimitChange = new PropertyChange<int?>
                                    {
                                        Before = (int?)(long?)xc.OldValue,
                                        After = (int?)(long?)xc.NewValue
                                    };
                                    break;

                                default:
                                    this.Discord.Logger.LogWarning(LoggerEvents.AuditLog, "Unknown key in thread update: {0} - this should be reported to library developers", xc.Key);
                                    break;
                            }
                        }
                        break;


                    case AuditLogActionType.GuildScheduledEventCreate:
                    case AuditLogActionType.GuildScheduledEventDelete:
                    case AuditLogActionType.GuildScheduledEventUpdate:
                        entry = new DiscordAuditLogGuildScheduledEventEntry
                        {
                            Target = this._scheduledEvents.TryGetValue(xac.TargetId.Value, out var scheduledEvent) ? scheduledEvent : new DiscordScheduledEvent { Id = xac.TargetId.Value, Discord = this.Discord }
                        };

                        var entryse = entry as DiscordAuditLogGuildScheduledEventEntry;
                        foreach (var xc in xac.Changes)
                        {
                            switch (xc.Key.ToLowerInvariant())
                            {
                                case "channel_id":
                                    entryse.ChannelIdChange = new PropertyChange<ulong?>
                                    {
                                        Before = ulong.TryParse(xc.OldValueString, out var ogid) ? ogid : null,
                                        After = ulong.TryParse(xc.NewValueString, out var ngid) ? ngid : null
                                    };
                                    break;

                                case "description":
                                    entryse.DescriptionChange = new PropertyChange<string>
                                    {
                                        Before = xc.OldValue != null ? xc.OldValueString : null,
                                        After = xc.NewValue != null ? xc.NewValueString : null
                                    };
                                    break;

                                case "location":
                                    entryse.LocationChange = new PropertyChange<string>
                                    {
                                        Before = xc.OldValue != null ? xc.OldValueString : null,
                                        After = xc.NewValue != null ? xc.NewValueString : null
                                    };
                                    break;

                                case "privacy_level":
                                    p1 = long.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t5);
                                    p2 = long.TryParse(xc.NewValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t6);

                                    entryse.PrivacyLevelChange = new PropertyChange<ScheduledEventPrivacyLevel?>
                                    {
                                        Before = p1 ? (ScheduledEventPrivacyLevel?)t5 : null,
                                        After = p2 ? (ScheduledEventPrivacyLevel?)t6 : null
                                    };
                                    break;

                                case "entity_type":
                                    p1 = long.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t5);
                                    p2 = long.TryParse(xc.NewValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t6);

                                    entryse.EntityTypeChange = new PropertyChange<ScheduledEventEntityType?>
                                    {
                                        Before = p1 ? (ScheduledEventEntityType?)t5 : null,
                                        After = p2 ? (ScheduledEventEntityType?)t6 : null
                                    };
                                    break;

                                case "status":
                                    p1 = long.TryParse(xc.OldValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t5);
                                    p2 = long.TryParse(xc.NewValue as string, NumberStyles.Integer, CultureInfo.InvariantCulture, out t6);

                                    entryse.StatusChange = new PropertyChange<ScheduledEventStatus?>
                                    {
                                        Before = p1 ? (ScheduledEventStatus?)t5 : null,
                                        After = p2 ? (ScheduledEventStatus?)t6 : null
                                    };
                                    break;

                                default:
                                    this.Discord.Logger.LogWarning(LoggerEvents.AuditLog, "Unknown key in scheduled event update: {0} - this should be reported to library developers", xc.Key);
                                    break;
                            }
                        }
                        break;

                    default:
                        this.Discord.Logger.LogWarning(LoggerEvents.AuditLog, "Unknown audit log action type: {0} - this should be reported to library developers", (int)xac.ActionType);
                        break;
                }

                if (entry == null)
                    continue;

                entry.ActionCategory = xac.ActionType switch
                {
                    AuditLogActionType.ChannelCreate or AuditLogActionType.EmojiCreate or AuditLogActionType.InviteCreate or AuditLogActionType.OverwriteCreate or AuditLogActionType.RoleCreate or AuditLogActionType.WebhookCreate or AuditLogActionType.IntegrationCreate or AuditLogActionType.StickerCreate or AuditLogActionType.StageInstanceCreate or AuditLogActionType.ThreadCreate or AuditLogActionType.GuildScheduledEventCreate => AuditLogActionCategory.Create,
                    AuditLogActionType.ChannelDelete or AuditLogActionType.EmojiDelete or AuditLogActionType.InviteDelete or AuditLogActionType.MessageDelete or AuditLogActionType.MessageBulkDelete or AuditLogActionType.OverwriteDelete or AuditLogActionType.RoleDelete or AuditLogActionType.WebhookDelete or AuditLogActionType.IntegrationDelete or AuditLogActionType.StickerDelete or AuditLogActionType.StageInstanceDelete or AuditLogActionType.ThreadDelete or AuditLogActionType.GuildScheduledEventDelete => AuditLogActionCategory.Delete,
                    AuditLogActionType.ChannelUpdate or AuditLogActionType.EmojiUpdate or AuditLogActionType.InviteUpdate or AuditLogActionType.MemberRoleUpdate or AuditLogActionType.MemberUpdate or AuditLogActionType.OverwriteUpdate or AuditLogActionType.RoleUpdate or AuditLogActionType.WebhookUpdate or AuditLogActionType.IntegrationUpdate or AuditLogActionType.StickerUpdate or AuditLogActionType.StageInstanceUpdate or AuditLogActionType.ThreadUpdate or AuditLogActionType.GuildScheduledEventUpdate => AuditLogActionCategory.Update,
                    _ => AuditLogActionCategory.Other,
                };
                entry.Discord = this.Discord;
                entry.ActionType = xac.ActionType;
                entry.Id = xac.Id;
                entry.Reason = xac.Reason;
                entry.UserResponsible = amd[xac.UserId];
                entries.Add(entry);
            }

            return new ReadOnlyCollection<DiscordAuditLogEntry>(entries);
        }

        /// <summary>
        /// Gets all of this guild's custom emojis.
        /// </summary>
        /// <returns>All of this guild's custom emojis.</returns>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task<IReadOnlyList<DiscordGuildEmoji>> GetEmojis()
            => this.Discord.ApiClient.GetGuildEmojisAsync(this.Id);

        /// <summary>
        /// Gets this guild's specified custom emoji.
        /// </summary>
        /// <param name="Id">ID of the emoji to get.</param>
        /// <returns>The requested custom emoji.</returns>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task<DiscordGuildEmoji> GetEmoji(ulong Id)
            => this.Discord.ApiClient.GetGuildEmojiAsync(this.Id, Id);

        /// <summary>
        /// Creates a new custom emoji for this guild.
        /// </summary>
        /// <param name="Name">Name of the new emoji.</param>
        /// <param name="Image">Image to use as the emoji.</param>
        /// <param name="Roles">Roles for which the emoji will be available. This works only if your application is whitelisted as integration.</param>
        /// <param name="Reason">Reason for audit log.</param>
        /// <returns>The newly-created emoji.</returns>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageEmojisAndStickers"/> permission.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task<DiscordGuildEmoji> CreateEmoji(string Name, Stream Image, IEnumerable<DiscordRole> Roles = null, string Reason = null)
        {
            if (string.IsNullOrWhiteSpace(Name))
                throw new ArgumentNullException(nameof(Name));

            Name = Name.Trim();
            if (Name.Length < 2 || Name.Length > 50)
                throw new ArgumentException("Emoji name needs to be between 2 and 50 characters long.");

            if (Image == null)
                throw new ArgumentNullException(nameof(Image));

            string image64 = null;
            using (var imgtool = new ImageTool(Image))
                image64 = imgtool.GetBase64();

            return this.Discord.ApiClient.CreateGuildEmojiAsync(this.Id, Name, image64, Roles?.Select(Xr => Xr.Id), Reason);
        }

        /// <summary>
        /// Modifies a this guild's custom emoji.
        /// </summary>
        /// <param name="Emoji">Emoji to modify.</param>
        /// <param name="Name">New name for the emoji.</param>
        /// <param name="Roles">Roles for which the emoji will be available. This works only if your application is whitelisted as integration.</param>
        /// <param name="Reason">Reason for audit log.</param>
        /// <returns>The modified emoji.</returns>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageEmojisAndStickers"/> permission.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task<DiscordGuildEmoji> ModifyEmoji(DiscordGuildEmoji Emoji, string Name, IEnumerable<DiscordRole> Roles = null, string Reason = null)
        {
            if (Emoji == null)
                throw new ArgumentNullException(nameof(Emoji));

            if (Emoji.Guild.Id != this.Id)
                throw new ArgumentException("This emoji does not belong to this guild.");

            if (string.IsNullOrWhiteSpace(Name))
                throw new ArgumentNullException(nameof(Name));

            Name = Name.Trim();
            return Name.Length < 2 || Name.Length > 50
                ? throw new ArgumentException("Emoji name needs to be between 2 and 50 characters long.")
                : this.Discord.ApiClient.ModifyGuildEmojiAsync(this.Id, Emoji.Id, Name, Roles?.Select(Xr => Xr.Id), Reason);
        }

        /// <summary>
        /// Deletes this guild's custom emoji.
        /// </summary>
        /// <param name="Emoji">Emoji to delete.</param>
        /// <param name="Reason">Reason for audit log.</param>
        /// <returns></returns>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageEmojisAndStickers"/> permission.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task DeleteEmoji(DiscordGuildEmoji Emoji, string Reason = null)
        {
            return Emoji == null
                ? throw new ArgumentNullException(nameof(Emoji))
                : Emoji.Guild.Id != this.Id
                ? throw new ArgumentException("This emoji does not belong to this guild.")
                : this.Discord.ApiClient.DeleteGuildEmoji(this.Id, Emoji.Id, Reason);
        }

        /// <summary>
        /// Gets all of this guild's custom stickers.
        /// </summary>
        /// <returns>All of this guild's custom stickers.</returns>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public async Task<IReadOnlyList<DiscordSticker>> GetStickersAsync()
        {
            var stickers = await this.Discord.ApiClient.GetGuildStickersAsync(this.Id);

            foreach (var xstr in stickers)
            {
                this._stickers.AddOrUpdate(xstr.Id, xstr, (Id, Old) =>
                {
                    Old.Name = xstr.Name;
                    Old.Description = xstr.Description;
                    Old.InternalTags = xstr.InternalTags;
                    return Old;
                });
            }

            return stickers;
        }

        /// <summary>
        /// Gets a sticker
        /// </summary>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the sticker could not be found.</exception>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageEmojisAndStickers"/> permission.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        /// <exception cref="System.ArgumentException">Sticker does not belong to a guild.</exception>
        public Task<DiscordSticker> GetSticker(ulong StickerId)
            => this.Discord.ApiClient.GetGuildStickerAsync(this.Id, StickerId);

        /// <summary>
        /// Creates a sticker
        /// </summary>
        /// <param name="Name">The name of the sticker.</param>
        /// <param name="Description">The optional description of the sticker.</param>
        /// <param name="Emoji">The emoji to associate the sticker with.</param>
        /// <param name="Format">The file format the sticker is written in.</param>
        /// <param name="File">The sticker.</param>
        /// <param name="Reason">Audit log reason</param>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageEmojisAndStickers"/> permission.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task<DiscordSticker> CreateSticker(string Name, string Description, DiscordEmoji Emoji, Stream File, StickerFormat Format, string Reason = null)
        {
            var fileExt = Format switch
            {
                StickerFormat.Png => "png",
                StickerFormat.Apng => "png",
                StickerFormat.Lottie => "json",
                _ => throw new InvalidOperationException("This format is not supported.")
            };

            var contentType = Format switch
            {
                StickerFormat.Png => "image/png",
                StickerFormat.Apng => "image/png",
                StickerFormat.Lottie => "application/json",
                _ => throw new InvalidOperationException("This format is not supported.")
            };

            return Emoji.Id is not 0
                ? throw new InvalidOperationException("Only unicode emoji can be used for stickers.")
                : Name.Length < 2 || Name.Length > 30
                ? throw new ArgumentOutOfRangeException(nameof(Name), "Sticker name needs to be between 2 and 30 characters long.")
                : Description.Length < 1 || Description.Length > 100
                ? throw new ArgumentOutOfRangeException(nameof(Description), "Sticker description needs to be between 1 and 100 characters long.")
                : this.Discord.ApiClient.CreateGuildStickerAsync(this.Id, Name, Description, Emoji.GetDiscordName().Replace(":", ""), new("sticker", File, null, fileExt, contentType), Reason);
        }

        /// <summary>
        /// Modifies a sticker
        /// </summary>
        /// <param name="Sticker">The id of the sticker to modify</param>
        /// <param name="Name">The name of the sticker</param>
        /// <param name="Description">The description of the sticker</param>
        /// <param name="Emoji">The emoji to associate with this sticker.</param>
        /// <param name="Reason">Audit log reason</param>
        /// <returns>A sticker object</returns>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the sticker could not be found.</exception>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageEmojisAndStickers"/> permission.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        /// <exception cref="System.ArgumentException">Sticker does not belong to a guild.</exception>
        public async Task<DiscordSticker> ModifyStickerAsync(ulong Sticker, Optional<string> Name, Optional<string> Description, Optional<DiscordEmoji> Emoji, string Reason = null)
        {

            string uemoji = null;

            if (!this._stickers.TryGetValue(Sticker, out var stickerobj) || stickerobj.Guild.Id != this.Id)
                throw new ArgumentException("This sticker does not belong to this guild.");
            if (Name.HasValue && (Name.Value.Length < 2 || Name.Value.Length > 30))
                throw new ArgumentException("Sticker name needs to be between 2 and 30 characters long.");
            if (Description.HasValue && (Description.Value.Length < 1 || Description.Value.Length > 100))
                throw new ArgumentException("Sticker description needs to be between 1 and 100 characters long.");
            if (Emoji.HasValue && Emoji.Value.Id > 0)
                throw new ArgumentException("Only unicode emojis can be used with stickers.");
            else if (Emoji.HasValue)
                uemoji = Emoji.Value.GetDiscordName().Replace(":", "");

            var usticker = await this.Discord.ApiClient.ModifyGuildStickerAsync(this.Id, Sticker, Name, Description, uemoji, Reason).ConfigureAwait(false);


            if (this._stickers.TryGetValue(usticker.Id, out var old))
                this._stickers.TryUpdate(usticker.Id, usticker, old);

            return usticker;
        }

        /// <summary>
        /// Modifies a sticker
        /// </summary>
        /// <param name="Sticker">The sticker to modify</param>
        /// <param name="Name">The name of the sticker</param>
        /// <param name="Description">The description of the sticker</param>
        /// <param name="Emoji">The emoji to associate with this sticker.</param>
        /// <param name="Reason">Audit log reason</param>
        /// <returns>A sticker object</returns>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the sticker could not be found.</exception>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageEmojisAndStickers"/> permission.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        /// <exception cref="System.ArgumentException">Sticker does not belong to a guild.</exception>
        public Task<DiscordSticker> ModifySticker(DiscordSticker Sticker, Optional<string> Name, Optional<string> Description, Optional<DiscordEmoji> Emoji, string Reason = null)
            => this.ModifyStickerAsync(Sticker.Id, Name, Description, Emoji, Reason);

        /// <summary>
        /// Deletes a sticker
        /// </summary>
        /// <param name="Sticker">Id of sticker to delete</param>
        /// <param name="Reason">Audit log reason</param>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the sticker could not be found.</exception>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageEmojisAndStickers"/> permission.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        /// <exception cref="System.ArgumentException">Sticker does not belong to a guild.</exception>
        public Task DeleteSticker(ulong Sticker, string Reason = null)
        {
            return !this._stickers.TryGetValue(Sticker, out var stickerobj)
                ? throw new ArgumentNullException(nameof(Sticker))
                : stickerobj.Guild.Id != this.Id
                ? throw new ArgumentException("This sticker does not belong to this guild.")
                : this.Discord.ApiClient.DeleteGuildStickerAsync(this.Id, Sticker, Reason);
        }

        /// <summary>
        /// Deletes a sticker
        /// </summary>
        /// <param name="Sticker">Sticker to delete</param>
        /// <param name="Reason">Audit log reason</param>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the sticker could not be found.</exception>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageEmojisAndStickers"/> permission.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        /// <exception cref="System.ArgumentException">Sticker does not belong to a guild.</exception>
        public Task DeleteSticker(DiscordSticker Sticker, string Reason = null)
            => this.DeleteSticker(Sticker.Id, Reason);

        /// <summary>
        /// <para>Gets the default channel for this guild.</para>
        /// <para>Default channel is the first channel current member can see.</para>
        /// </summary>
        /// <returns>This member's default guild.</returns>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public DiscordChannel GetDefaultChannel()
        {
            return this._channels?.Values.Where(Xc => Xc.Type == ChannelType.Text)
                    .OrderBy(Xc => Xc.Position)
                    .FirstOrDefault(Xc => (Xc.PermissionsFor(this.CurrentMember) & DisCatSharp.Permissions.AccessChannels) == DisCatSharp.Permissions.AccessChannels);
        }

        /// <summary>
        /// Gets the guild's widget
        /// </summary>
        /// <returns>The guild's widget</returns>
        public Task<DiscordWidget> GetWidget()
            => this.Discord.ApiClient.GetGuildWidgetAsync(this.Id);

        /// <summary>
        /// Gets the guild's widget settings
        /// </summary>
        /// <returns>The guild's widget settings</returns>
        public Task<DiscordWidgetSettings> GetWidgetSettings()
            => this.Discord.ApiClient.GetGuildWidgetSettingsAsync(this.Id);

        /// <summary>
        /// Modifies the guild's widget settings
        /// </summary>
        /// <param name="IsEnabled">If the widget is enabled or not</param>
        /// <param name="Channel">Widget channel</param>
        /// <param name="Reason">Reason the widget settings were modified</param>
        /// <returns>The newly modified widget settings</returns>
        public Task<DiscordWidgetSettings> ModifyWidgetSettings(bool? IsEnabled = null, DiscordChannel Channel = null, string Reason = null)
            => this.Discord.ApiClient.ModifyGuildWidgetSettingsAsync(this.Id, IsEnabled, Channel?.Id, Reason);

        /// <summary>
        /// Gets all of this guild's templates.
        /// </summary>
        /// <returns>All of the guild's templates.</returns>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Throws when the client does not have the <see cref="Permissions.ManageGuild"/> permission.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task<IReadOnlyList<DiscordGuildTemplate>> GetTemplates()
            => this.Discord.ApiClient.GetGuildTemplatesAsync(this.Id);

        /// <summary>
        /// Creates a guild template.
        /// </summary>
        /// <param name="Name">Name of the template.</param>
        /// <param name="Description">Description of the template.</param>
        /// <returns>The template created.</returns>
        /// <exception cref="DisCatSharp.Exceptions.BadRequestException">Throws when a template already exists for the guild or a null parameter is provided for the name.</exception>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Throws when the client does not have the <see cref="Permissions.ManageGuild"/> permission.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task<DiscordGuildTemplate> CreateTemplate(string Name, string Description = null)
            => this.Discord.ApiClient.CreateGuildTemplateAsync(this.Id, Name, Description);

        /// <summary>
        /// Syncs the template to the current guild's state.
        /// </summary>
        /// <param name="Code">The code of the template to sync.</param>
        /// <returns>The template synced.</returns>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Throws when the template for the code cannot be found</exception>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Throws when the client does not have the <see cref="Permissions.ManageGuild"/> permission.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task<DiscordGuildTemplate> SyncTemplate(string Code)
            => this.Discord.ApiClient.SyncGuildTemplateAsync(this.Id, Code);

        /// <summary>
        /// Modifies the template's metadata.
        /// </summary>
        /// <param name="Code">The template's code.</param>
        /// <param name="Name">Name of the template.</param>
        /// <param name="Description">Description of the template.</param>
        /// <returns>The template modified.</returns>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Throws when the template for the code cannot be found</exception>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Throws when the client does not have the <see cref="Permissions.ManageGuild"/> permission.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task<DiscordGuildTemplate> ModifyTemplate(string Code, string Name = null, string Description = null)
            => this.Discord.ApiClient.ModifyGuildTemplateAsync(this.Id, Code, Name, Description);

        /// <summary>
        /// Deletes the template.
        /// </summary>
        /// <param name="Code">The code of the template to delete.</param>
        /// <returns>The deleted template.</returns>
        /// <exception cref="DisCatSharp.Exceptions.NotFoundException">Throws when the template for the code cannot be found</exception>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Throws when the client does not have the <see cref="Permissions.ManageGuild"/> permission.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task<DiscordGuildTemplate> DeleteTemplate(string Code)
            => this.Discord.ApiClient.DeleteGuildTemplateAsync(this.Id, Code);

        /// <summary>
        /// Gets this guild's membership screening form.
        /// </summary>
        /// <returns>This guild's membership screening form.</returns>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task<DiscordGuildMembershipScreening> GetMembershipScreeningForm()
            => this.Discord.ApiClient.GetGuildMembershipScreeningFormAsync(this.Id);

        /// <summary>
        /// Modifies this guild's membership screening form.
        /// </summary>
        /// <param name="Action">Action to perform</param>
        /// <returns>The modified screening form.</returns>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client doesn't have the <see cref="Permissions.ManageGuild"/> permission, or community is not enabled on this guild.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public async Task<DiscordGuildMembershipScreening> ModifyMembershipScreeningFormAsync(Action<MembershipScreeningEditModel> Action)
        {
            var mdl = new MembershipScreeningEditModel();
            Action(mdl);
            return await this.Discord.ApiClient.ModifyGuildMembershipScreeningFormAsync(this.Id, mdl.Enabled, mdl.Fields, mdl.Description);
        }

        /// <summary>
        /// Gets all the application commands in this guild.
        /// </summary>
        /// <returns>A list of application commands in this guild.</returns>
        public Task<IReadOnlyList<DiscordApplicationCommand>> GetApplicationCommands() =>
            this.Discord.ApiClient.GetGuildApplicationCommandsAsync(this.Discord.CurrentApplication.Id, this.Id);

        /// <summary>
        /// Overwrites the existing application commands in this guild. New commands are automatically created and missing commands are automatically delete
        /// </summary>
        /// <param name="Commands">The list of commands to overwrite with.</param>
        /// <returns>The list of guild commands</returns>
        public Task<IReadOnlyList<DiscordApplicationCommand>> BulkOverwriteApplicationCommands(IEnumerable<DiscordApplicationCommand> Commands) =>
            this.Discord.ApiClient.BulkOverwriteGuildApplicationCommandsAsync(this.Discord.CurrentApplication.Id, this.Id, Commands);

        /// <summary>
        /// Creates or overwrites a application command in this guild.
        /// </summary>
        /// <param name="Command">The command to create.</param>
        /// <returns>The created command.</returns>
        public Task<DiscordApplicationCommand> CreateApplicationCommand(DiscordApplicationCommand Command) =>
            this.Discord.ApiClient.CreateGuildApplicationCommandAsync(this.Discord.CurrentApplication.Id, this.Id, Command);

        /// <summary>
        /// Edits a application command in this guild.
        /// </summary>
        /// <param name="CommandId">The id of the command to edit.</param>
        /// <param name="Action">Action to perform.</param>
        /// <returns>The edit command.</returns>
        public async Task<DiscordApplicationCommand> EditApplicationCommandAsync(ulong CommandId, Action<ApplicationCommandEditModel> Action)
        {
            var mdl = new ApplicationCommandEditModel();
            Action(mdl);
            return await this.Discord.ApiClient.EditGuildApplicationCommandAsync(this.Discord.CurrentApplication.Id, this.Id, CommandId, mdl.Name, mdl.Description, mdl.Options, mdl.DefaultPermission, mdl.NameLocalizations, mdl.DescriptionLocalizations).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets this guild's welcome screen.
        /// </summary>
        /// <returns>This guild's welcome screen object.</returns>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public Task<DiscordGuildWelcomeScreen> GetWelcomeScreen() =>
            this.Discord.ApiClient.GetGuildWelcomeScreenAsync(this.Id);

        /// <summary>
        /// Modifies this guild's welcome screen.
        /// </summary>
        /// <param name="Action">Action to perform.</param>
        /// <returns>The modified welcome screen.</returns>
        /// <exception cref="DisCatSharp.Exceptions.UnauthorizedException">Thrown when the client doesn't have the <see cref="Permissions.ManageGuild"/> permission, or community is not enabled on this guild.</exception>
        /// <exception cref="DisCatSharp.Exceptions.ServerErrorException">Thrown when Discord is unable to process the request.</exception>
        public async Task<DiscordGuildWelcomeScreen> ModifyWelcomeScreenAsync(Action<WelcomeScreenEditModel> Action)
        {
            var mdl = new WelcomeScreenEditModel();
            Action(mdl);
            return await this.Discord.ApiClient.ModifyGuildWelcomeScreenAsync(this.Id, mdl.Enabled, mdl.WelcomeChannels, mdl.Description).ConfigureAwait(false);
        }
        #endregion

        /// <summary>
        /// Returns a string representation of this guild.
        /// </summary>
        /// <returns>String representation of this guild.</returns>
        public override string ToString() => $"Guild {this.Id}; {this.Name}";

        /// <summary>
        /// Checks whether this <see cref="DiscordGuild"/> is equal to another object.
        /// </summary>
        /// <param name="Obj">Object to compare to.</param>
        /// <returns>Whether the object is equal to this <see cref="DiscordGuild"/>.</returns>
        public override bool Equals(object Obj) => this.Equals(Obj as DiscordGuild);

        /// <summary>
        /// Checks whether this <see cref="DiscordGuild"/> is equal to another <see cref="DiscordGuild"/>.
        /// </summary>
        /// <param name="E"><see cref="DiscordGuild"/> to compare to.</param>
        /// <returns>Whether the <see cref="DiscordGuild"/> is equal to this <see cref="DiscordGuild"/>.</returns>
        public bool Equals(DiscordGuild E) => E is not null && (ReferenceEquals(this, E) || this.Id == E.Id);

        /// <summary>
        /// Gets the hash code for this <see cref="DiscordGuild"/>.
        /// </summary>
        /// <returns>The hash code for this <see cref="DiscordGuild"/>.</returns>
        public override int GetHashCode() => this.Id.GetHashCode();

        /// <summary>
        /// Gets whether the two <see cref="DiscordGuild"/> objects are equal.
        /// </summary>
        /// <param name="E1">First guild to compare.</param>
        /// <param name="E2">Second guild to compare.</param>
        /// <returns>Whether the two guilds are equal.</returns>
        public static bool operator ==(DiscordGuild E1, DiscordGuild E2)
        {
            var o1 = E1 as object;
            var o2 = E2 as object;

            return (o1 != null || o2 == null) && (o1 == null || o2 != null) && ((o1 == null && o2 == null) || E1.Id == E2.Id);
        }

        /// <summary>
        /// Gets whether the two <see cref="DiscordGuild"/> objects are not equal.
        /// </summary>
        /// <param name="E1">First guild to compare.</param>
        /// <param name="E2">Second guild to compare.</param>
        /// <returns>Whether the two guilds are not equal.</returns>
        public static bool operator !=(DiscordGuild E1, DiscordGuild E2)
            => !(E1 == E2);

    }

    /// <summary>
    /// Represents guild verification level.
    /// </summary>
    public enum VerificationLevel : int
    {
        /// <summary>
        /// No verification. Anyone can join and chat right away.
        /// </summary>
        None = 0,

        /// <summary>
        /// Low verification level. Users are required to have a verified email attached to their account in order to be able to chat.
        /// </summary>
        Low = 1,

        /// <summary>
        /// Medium verification level. Users are required to have a verified email attached to their account, and account age need to be at least 5 minutes in order to be able to chat.
        /// </summary>
        Medium = 2,

        /// <summary>
        /// High verification level. Users are required to have a verified email attached to their account, account age need to be at least 5 minutes, and they need to be in the server for at least 10 minutes in order to be able to chat.
        /// </summary>
        High = 3,

        /// <summary>
        /// Highest verification level. Users are required to have a verified phone number attached to their account.
        /// </summary>
        Highest = 4
    }

    /// <summary>
    /// Represents default notification level for a guild.
    /// </summary>
    public enum DefaultMessageNotifications : int
    {
        /// <summary>
        /// All messages will trigger push notifications.
        /// </summary>
        AllMessages = 0,

        /// <summary>
        /// Only messages that mention the user (or a role he's in) will trigger push notifications.
        /// </summary>
        MentionsOnly = 1
    }

    /// <summary>
    /// Represents multi-factor authentication level required by a guild to use administrator functionality.
    /// </summary>
    public enum MfaLevel : int
    {
        /// <summary>
        /// Multi-factor authentication is not required to use administrator functionality.
        /// </summary>
        Disabled = 0,

        /// <summary>
        /// Multi-factor authentication is required to use administrator functionality.
        /// </summary>
        Enabled = 1
    }

    /// <summary>
    /// Represents the value of explicit content filter in a guild.
    /// </summary>
    public enum ExplicitContentFilter : int
    {
        /// <summary>
        /// Explicit content filter is disabled.
        /// </summary>
        Disabled = 0,

        /// <summary>
        /// Only messages from members without any roles are scanned.
        /// </summary>
        MembersWithoutRoles = 1,

        /// <summary>
        /// Messages from all members are scanned.
        /// </summary>
        AllMembers = 2
    }

    /// <summary>
    /// Represents the formats for a guild widget.
    /// </summary>
    public enum WidgetType : int
    {
        /// <summary>
        /// The widget is represented in shield format.
        /// <para>This is the default widget type.</para>
        /// </summary>
        Shield = 0,

        /// <summary>
        /// The widget is represented as the first banner type.
        /// </summary>
        Banner1 = 1,

        /// <summary>
        /// The widget is represented as the second banner type.
        /// </summary>
        Banner2 = 2,

        /// <summary>
        /// The widget is represented as the third banner type.
        /// </summary>
        Banner3 = 3,

        /// <summary>
        /// The widget is represented in the fourth banner type.
        /// </summary>
        Banner4 = 4
    }

    /// <summary>
    /// Represents the guild features.
    /// </summary>
    public class GuildFeatures
    {
        /// <summary>
        /// Guild has access to set an animated guild icon.
        /// </summary>
        public bool CanSetAnimatedIcon { get; }

        /// <summary>
        /// Guild has access to set a guild banner image.
        /// </summary>
        public bool CanSetBanner { get; }

        /// <summary>
        /// Guild has access to use commerce features (i.e. create store channels)
        /// </summary>
        public bool CanCreateStoreChannels { get; }

        /// <summary>
        /// Guild can enable Welcome Screen, Membership Screening, Stage Channels, News Channels and receives community updates.
        /// Furthermore the guild can apply as a partner and for the discovery (if the prerequisites are given).
        /// <see cref="ChannelType.Stage"/> and <see cref="ChannelType.News"/> is usable.
        /// </summary>
        public bool HasCommunityEnabled { get; }

        /// <summary>
        /// Guild is able to be discovered in the discovery.
        /// </summary>
        public bool IsDiscoverable { get; }

        /// <summary>
        /// Guild is able to be featured in the discovery.
        /// </summary>
        public bool IsFeatureable { get; }

        /// <summary>
        /// Guild has access to set an invite splash background.
        /// </summary>
        public bool CanSetInviteSplash { get; }

        /// <summary>
        /// Guild has enabled Membership Screening.
        /// </summary>
        public bool HasMembershipScreeningEnabled { get; }

        /// <summary>
        /// Guild has access to create news channels.
        /// <see cref="ChannelType.News"/> is usable.
        /// </summary>
        public bool CanCreateNewsChannels { get; }

        /// <summary>
        /// Guild is partnered.
        /// </summary>
        public bool IsPartnered { get; }

        /// <summary>
        /// Guild has increased custom emoji slots.
        /// </summary>
        public bool CanUploadMoreEmojis { get; }

        /// <summary>
        /// Guild can be previewed before joining via Membership Screening or the discovery.
        /// </summary>
        public bool HasPreviewEnabled { get; }

        /// <summary>
        /// Guild has access to set a vanity URL.
        /// </summary>
        public bool CanSetVanityUrl { get; }

        /// <summary>
        /// Guild is verified.
        /// </summary>
        public bool IsVerified { get; }

        /// <summary>
        /// Guild has access to set 384kbps bitrate in voice (previously VIP voice servers).
        /// </summary>
        public bool CanAccessVipRegions { get; }

        /// <summary>
        /// Guild has enabled the welcome screen.
        /// </summary>
        public bool HasWelcomeScreenEnabled { get; }

        /// <summary>
        /// Guild has enabled ticketed events.
        /// </summary>
        public bool HasTicketedEventsEnabled { get; }

        /// <summary>
        /// Guild has enabled monetization.
        /// </summary>
        public bool HasMonetizationEnabled { get; }

        /// <summary>
        /// Guild has increased custom sticker slots.
        /// </summary>
        public bool CanUploadMoreStickers { get; }

        /// <summary>
        /// Guild has access to the three day archive time for threads.
        /// Needs Premium Tier 1 (<see cref="PremiumTier.TierOne"/>).
        /// </summary>
        public bool CanSetThreadArchiveDurationThreeDays { get; }

        /// <summary>
        /// Guild has access to the seven day archive time for threads.
        /// Needs Premium Tier 2 (<see cref="PremiumTier.TierTwo"/>).
        /// </summary>
        public bool CanSetThreadArchiveDurationSevenDays { get; }

        /// <summary>
        /// Guild has access to create private threads.
        /// Needs Premium Tier 2 (<see cref="PremiumTier.TierTwo"/>).
        /// </summary>
        public bool CanCreatePrivateThreads { get; }

        /// <summary>
        /// Guild is a hub.
        /// <see cref="ChannelType.GuildDirectory"/> is usable.
        /// </summary>
        public bool IsHub { get; }

        /// <summary>
        /// Guild is in a hub.
        /// https://github.com/discord/discord-api-docs/pull/3757/commits/4932d92c9d0c783861bc715bf7ebbabb15114e34
        /// </summary>
        public bool HasDirectoryEntry { get; }

        /// <summary>
        /// Guild is linked to a hub.
        /// </summary>
        public bool IsLinkedToHub { get; }

        /// <summary>
        /// Guild has full access to threads.
        /// Old Feature.
        /// </summary>
        public bool HasThreadTestingEnabled { get; }

        /// <summary>
        /// Guild has access to threads.
        /// </summary>
        public bool HasThreadsEnabled { get; }

        /// <summary>
        /// Guild can set role icons.
        /// </summary>
        public bool CanSetRoleIcons { get; }

        /// <summary>
        /// Guild has the new thread permissions.
        /// Old Feature.
        /// </summary>
        public bool HasNewThreadPermissions { get; }

        /// <summary>
        /// Guild can set thread default auto archive duration.
        /// Old Feature.
        /// </summary>
        public bool CanSetThreadDefaultAutoArchiveDuration { get; }

        /// <summary>
        /// Guild has enabled role subsriptions.
        /// </summary>
        public bool HasRoleSubscriptionsEnabled { get; }

        /// <summary>
        /// Guild role subsriptions as purchaseable.
        /// </summary>
        public bool RoleSubscriptionsIsAvaiableForPurchase { get; }

        /// <summary>
        /// Guild has premium tier 3 override.
        /// </summary>
        public bool PremiumTierThreeOverride { get; }

        /// <summary>
        /// Guild has access to text in voice.
        /// Restricted to <see cref="IsStaffOnly"/>.
        /// </summary>
        public bool TextInVoiceEnabled { get; }

        /// <summary>
        /// Guild can set an animated banner.
        /// Needs Premium Tier 3 (<see cref="PremiumTier.TierThree"/>).
        /// </summary>
        public bool CanSetAnimatedBanner { get; }

        /// <summary>
        /// Guild can set an animated banner.
        /// Needs Premium Tier 3 (<see cref="PremiumTier.TierThree"/>).
        /// </summary>
        public bool CanSetChannelBanner { get; }

        /// <summary>
        /// Allows members to customize their avatar, banner and bio for that server.
        /// </summary>
        public bool HasMemberProfiles { get; }

        /// <summary>
        /// Guild is restricted to users with the <see cref="UserFlags.Staff"/> badge.
        /// </summary>
        public bool IsStaffOnly { get; }

        /// <summary>
        /// String of guild features.
        /// </summary>
        public string FeatureString { get; }

        /// <summary>
        /// Checks the guild features and constructs a new <see cref="GuildFeatures"/> object.
        /// </summary>
        /// <param name="Guild">Guild to check</param>
        public GuildFeatures(DiscordGuild Guild)
        {
            this.CanSetAnimatedIcon = Guild.RawFeatures.Contains("ANIMATED_ICON");
            this.CanSetAnimatedBanner = Guild.RawFeatures.Contains("ANIMATED_BANNER");
            this.CanSetBanner = Guild.RawFeatures.Contains("BANNER");
            this.CanSetChannelBanner = Guild.RawFeatures.Contains("CHANNEL_BANNER");
            this.CanCreateStoreChannels = Guild.RawFeatures.Contains("COMMERCE");
            this.HasCommunityEnabled = Guild.RawFeatures.Contains("COMMUNITY");
            this.IsDiscoverable = !Guild.RawFeatures.Contains("DISCOVERABLE_DISABLED") && Guild.RawFeatures.Contains("DISCOVERABLE");
            this.IsFeatureable = Guild.RawFeatures.Contains("FEATURABLE");
            this.CanSetInviteSplash = Guild.RawFeatures.Contains("INVITE_SPLASH");
            this.HasMembershipScreeningEnabled = Guild.RawFeatures.Contains("MEMBER_VERIFICATION_GATE_ENABLED");
            this.CanCreateNewsChannels = Guild.RawFeatures.Contains("NEWS");
            this.IsPartnered = Guild.RawFeatures.Contains("PARTNERED");
            this.CanUploadMoreEmojis = Guild.RawFeatures.Contains("MORE_EMOJI");
            this.HasPreviewEnabled = Guild.RawFeatures.Contains("PREVIEW_ENABLED");
            this.CanSetVanityUrl = Guild.RawFeatures.Contains("VANITY_URL");
            this.IsVerified = Guild.RawFeatures.Contains("VERIFIED");
            this.CanAccessVipRegions = Guild.RawFeatures.Contains("VIP_REGIONS");
            this.HasWelcomeScreenEnabled = Guild.RawFeatures.Contains("WELCOME_SCREEN_ENABLED");
            this.HasTicketedEventsEnabled = Guild.RawFeatures.Contains("TICKETED_EVENTS_ENABLED");
            this.HasMonetizationEnabled = Guild.RawFeatures.Contains("MONETIZATION_ENABLED");
            this.CanUploadMoreStickers = Guild.RawFeatures.Contains("MORE_STICKERS");
            this.CanSetThreadArchiveDurationThreeDays = Guild.RawFeatures.Contains("THREE_DAY_THREAD_ARCHIVE");
            this.CanSetThreadArchiveDurationSevenDays = Guild.RawFeatures.Contains("SEVEN_DAY_THREAD_ARCHIVE");
            this.CanCreatePrivateThreads = Guild.RawFeatures.Contains("PRIVATE_THREADS");
            this.IsHub = Guild.RawFeatures.Contains("HUB");
            this.HasThreadTestingEnabled = Guild.RawFeatures.Contains("THREADS_ENABLED_TESTING");
            this.HasThreadsEnabled = Guild.RawFeatures.Contains("THREADS_ENABLED");
            this.CanSetRoleIcons = Guild.RawFeatures.Contains("ROLE_ICONS");
            this.HasNewThreadPermissions = Guild.RawFeatures.Contains("NEW_THREAD_PERMISSIONS");
            this.HasRoleSubscriptionsEnabled = Guild.RawFeatures.Contains("ROLE_SUBSCRIPTIONS_ENABLED");
            this.PremiumTierThreeOverride = Guild.RawFeatures.Contains("PREMIUM_TIER_3_OVERRIDE");
            this.CanSetThreadDefaultAutoArchiveDuration = Guild.RawFeatures.Contains("THREAD_DEFAULT_AUTO_ARCHIVE_DURATION");
            this.TextInVoiceEnabled = Guild.RawFeatures.Contains("TEXT_IN_VOICE_ENABLED");
            this.HasDirectoryEntry = Guild.RawFeatures.Contains("HAS_DIRECTORY_ENTRY");
            this.IsLinkedToHub = Guild.RawFeatures.Contains("LINKED_TO_HUB");
            this.HasMemberProfiles = Guild.RawFeatures.Contains("MEMBER_PROFILES");
            this.IsStaffOnly = Guild.RawFeatures.Contains("INTERNAL_EMPLOYEE_ONLY");
            this.RoleSubscriptionsIsAvaiableForPurchase = Guild.RawFeatures.Contains("ROLE_SUBSCRIPTIONS_AVAILABLE_FOR_PURCHASE");

            var features = Guild.RawFeatures.Any() ? "" : "None";
            foreach (var feature in Guild.RawFeatures)
            {
                features += feature + " ";
            }
            this.FeatureString = features;

        }
    }
}
