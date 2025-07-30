using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using DisCatSharp.Attributes;
using DisCatSharp.Enums;
using DisCatSharp.Exceptions;
using DisCatSharp.Net;
using DisCatSharp.Net.Abstractions;
using DisCatSharp.Net.Models;
using DisCatSharp.Net.Serialization;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
///     Represents a Discord guild.
/// </summary>
public partial class DiscordGuild : SnowflakeObject, IEquatable<DiscordGuild>
{
	[JsonIgnore]
	private readonly Lazy<DiscordMember?> _currentMemberLazy;

	[JsonProperty("channels", NullValueHandling = NullValueHandling.Ignore), JsonConverter(typeof(SnowflakeArrayAsDictionaryJsonConverter))]
	internal ConcurrentDictionary<ulong, DiscordChannel> ChannelsInternal = [];

	[JsonProperty("emojis", NullValueHandling = NullValueHandling.Ignore), JsonConverter(typeof(SnowflakeArrayAsDictionaryJsonConverter))]
	internal ConcurrentDictionary<ulong, DiscordEmoji> EmojisInternal = [];

	[JsonIgnore]
	internal ConcurrentDictionary<string, DiscordInvite> Invites = [];

	[JsonIgnore]
	internal bool IsOwnerInternal;

	[JsonProperty("members", NullValueHandling = NullValueHandling.Ignore), JsonConverter(typeof(SnowflakeArrayAsDictionaryJsonConverter))]
	internal ConcurrentDictionary<ulong, DiscordMember> MembersInternal = [];

	[JsonProperty("roles", NullValueHandling = NullValueHandling.Ignore), JsonConverter(typeof(SnowflakeArrayAsDictionaryJsonConverter))]
	internal ConcurrentDictionary<ulong, DiscordRole> RolesInternal = [];

	[JsonProperty("guild_scheduled_events", NullValueHandling = NullValueHandling.Ignore), JsonConverter(typeof(SnowflakeArrayAsDictionaryJsonConverter))]
	internal ConcurrentDictionary<ulong, DiscordScheduledEvent> ScheduledEventsInternal = [];

	[JsonProperty("soundboard_sounds", NullValueHandling = NullValueHandling.Ignore), JsonConverter(typeof(SnowflakeArrayAsDictionaryJsonConverter))]
	internal ConcurrentDictionary<ulong, DiscordSoundboardSound> SoundboardSoundsInternal = [];

	[JsonProperty("stage_instances", NullValueHandling = NullValueHandling.Ignore), JsonConverter(typeof(SnowflakeArrayAsDictionaryJsonConverter))]
	internal ConcurrentDictionary<ulong, DiscordStageInstance> StageInstancesInternal = [];

	[JsonProperty("stickers", NullValueHandling = NullValueHandling.Ignore), JsonConverter(typeof(SnowflakeArrayAsDictionaryJsonConverter))]
	internal ConcurrentDictionary<ulong, DiscordSticker> StickersInternal = [];

	[JsonProperty("threads", NullValueHandling = NullValueHandling.Ignore), JsonConverter(typeof(SnowflakeArrayAsDictionaryJsonConverter))]
	internal ConcurrentDictionary<ulong, DiscordThreadChannel> ThreadsInternal = [];

	[JsonProperty("voice_states", NullValueHandling = NullValueHandling.Ignore), JsonConverter(typeof(SnowflakeArrayAsDictionaryJsonConverter))]
	internal ConcurrentDictionary<ulong, DiscordVoiceState> VoiceStatesInternal = [];

	/// <summary>
	///     Initializes a new instance of the <see cref="DiscordGuild" /> class.
	/// </summary>
	internal DiscordGuild()
	{
		this._currentMemberLazy = new(() => this.MembersInternal.GetValueOrDefault(this.Discord.CurrentUser.Id));
		this.Invites = new();
		this.Threads = new ReadOnlyConcurrentDictionary<ulong, DiscordThreadChannel>(this.ThreadsInternal);
		this.StageInstances = new ReadOnlyConcurrentDictionary<ulong, DiscordStageInstance>(this.StageInstancesInternal);
		this.ScheduledEvents = new ReadOnlyConcurrentDictionary<ulong, DiscordScheduledEvent>(this.ScheduledEventsInternal);
		this.SoundboardSounds = new ReadOnlyConcurrentDictionary<ulong, DiscordSoundboardSound>(this.SoundboardSoundsInternal);
	}

	/// <summary>
	///     Gets the guild's name.
	/// </summary>
	[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
	public string Name { get; internal set; }

	/// <summary>
	///     Gets the guild icon's hash.
	/// </summary>
	[JsonProperty("icon", NullValueHandling = NullValueHandling.Ignore)]
	public string? IconHash { get; internal set; }

	/// <summary>
	///     Gets the guild icon's url.
	/// </summary>
	[JsonIgnore]
	public string? IconUrl
		=> !string.IsNullOrWhiteSpace(this.IconHash) ? $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Url}{Endpoints.ICONS}/{this.Id.ToString(CultureInfo.InvariantCulture)}/{this.IconHash}.{(this.IconHash.StartsWith("a_", StringComparison.Ordinal) ? "gif" : "png")}?size=1024" : null;

	/// <summary>
	///     Gets the guild splash's hash.
	/// </summary>
	[JsonProperty("splash", NullValueHandling = NullValueHandling.Ignore)]
	public string? SplashHash { get; internal set; }

	/// <summary>
	///     Gets the guild splash's url.
	/// </summary>
	[JsonIgnore]
	public string? SplashUrl
		=> !string.IsNullOrWhiteSpace(this.SplashHash) ? $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Url}{Endpoints.SPLASHES}/{this.Id.ToString(CultureInfo.InvariantCulture)}/{this.SplashHash}.png?size=1024" : null;

	/// <summary>
	///     Gets the guild discovery splash's hash.
	/// </summary>
	[JsonProperty("discovery_splash", NullValueHandling = NullValueHandling.Ignore)]
	public string? DiscoverySplashHash { get; internal set; }

	/// <summary>
	///     Gets the guild discovery splash's url.
	/// </summary>
	[JsonIgnore, RequiresFeature(Attributes.Features.Discoverable)]
	public string? DiscoverySplashUrl
		=> !string.IsNullOrWhiteSpace(this.DiscoverySplashHash) ? $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Url}{Endpoints.GUILD_DISCOVERY_SPLASHES}/{this.Id.ToString(CultureInfo.InvariantCulture)}/{this.DiscoverySplashHash}.png?size=1024" : null;

	/// <summary>
	///     Gets the guild home header's hash.
	/// </summary>
	[JsonProperty("home_header", NullValueHandling = NullValueHandling.Ignore)]
	public string? HomeHeaderHash { get; internal set; }

	/// <summary>
	///     Gets the guild home header's url.
	/// </summary>
	[JsonIgnore, RequiresFeature(Attributes.Features.Onboarding, "Requires to have guide enabled.")]
	public string? HomeHeaderUrl
		=> !string.IsNullOrWhiteSpace(this.HomeHeaderHash) ? $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Url}{Endpoints.GUILD_HOME_HEADERS}/{this.Id.ToString(CultureInfo.InvariantCulture)}/{this.HomeHeaderHash}.jpg?size=1280" : null;

	/// <summary>
	///     Gets the preferred locale of this guild.
	///     <para>This is used for server discovery, interactions and notices from Discord. Defaults to en-US.</para>
	/// </summary>
	[JsonProperty("preferred_locale", NullValueHandling = NullValueHandling.Ignore)]
	public string? PreferredLocale { get; internal set; }

	/// <summary>
	///     Gets the ID of the guild's owner.
	/// </summary>
	[JsonProperty("owner_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? OwnerId { get; internal set; }

	/// <summary>
	///     Gets the guild's owner.
	/// </summary>
	[JsonIgnore]
	public DiscordMember? Owner
		=> this.OwnerId.HasValue
			? this.Members.TryGetValue(this.OwnerId.Value, out var owner)
				? owner
				: this.Discord.ApiClient.GetGuildMemberAsync(this.Id, this.OwnerId.Value).ConfigureAwait(false).GetAwaiter().GetResult()
			: null;

	/// <summary>
	///     Gets permissions for the user in the guild (does not include channel overrides)
	/// </summary>
	[JsonProperty("permissions", NullValueHandling = NullValueHandling.Ignore)]
	public Permissions? Permissions { get; set; }

	/// <summary>
	///     Gets the guild's voice region ID.
	/// </summary>
	[JsonProperty("region", NullValueHandling = NullValueHandling.Ignore)]
	internal string? VoiceRegionId { get; set; }

	/// <summary>
	///     Gets the guild's voice region.
	/// </summary>
	[JsonIgnore]
	public DiscordVoiceRegion? VoiceRegion
		=> this.VoiceRegionId is not null ? this.Discord.VoiceRegions[this.VoiceRegionId] : null;

	/// <summary>
	///     Gets the guild's AFK voice channel ID.
	/// </summary>
	[JsonProperty("afk_channel_id", NullValueHandling = NullValueHandling.Ignore)]
	internal ulong? AfkChannelId { get; set; }

	/// <summary>
	///     Gets the guild's AFK voice channel.
	/// </summary>
	[JsonIgnore]
	public DiscordChannel? AfkChannel
		=> this.AfkChannelId.HasValue
			? this.GetChannel(this.AfkChannelId.Value)
			: null;

	/// <summary>
	///     List of <see cref="DisCatSharp.Entities.DiscordApplicationCommand" />.
	///     Null if DisCatSharp.ApplicationCommands is not used or no guild commands are registered.
	/// </summary>
	[JsonIgnore]
	public ReadOnlyCollection<DiscordApplicationCommand> RegisteredApplicationCommands
		=> new(this.InternalRegisteredApplicationCommands);

	[JsonIgnore]
	internal List<DiscordApplicationCommand> InternalRegisteredApplicationCommands { get; set; } = [];

	/// <summary>
	///     Gets the guild's AFK timeout.
	/// </summary>
	[JsonProperty("afk_timeout", NullValueHandling = NullValueHandling.Ignore)]
	public int? AfkTimeout { get; internal set; }

	/// <summary>
	///     Gets the guild's verification level.
	/// </summary>
	[JsonProperty("verification_level", NullValueHandling = NullValueHandling.Ignore)]
	public VerificationLevel? VerificationLevel { get; internal set; }

	/// <summary>
	///     Gets the guild's default notification settings.
	/// </summary>
	[JsonProperty("default_message_notifications", NullValueHandling = NullValueHandling.Ignore)]
	public DefaultMessageNotifications? DefaultMessageNotifications { get; internal set; }

	/// <summary>
	///     Gets the guild's explicit content filter settings.
	/// </summary>
	[JsonProperty("explicit_content_filter", NullValueHandling = NullValueHandling.Ignore)]
	public ExplicitContentFilter? ExplicitContentFilter { get; internal set; }

	/// <summary>
	///     Gets the guild's nsfw level.
	/// </summary>
	[JsonProperty("nsfw_level", NullValueHandling = NullValueHandling.Ignore)]
	public NsfwLevel? NsfwLevel { get; internal set; }

	/// <summary>
	///     Gets the system channel id.
	/// </summary>
	[JsonProperty("system_channel_id", NullValueHandling = NullValueHandling.Ignore)]
	internal ulong? SystemChannelId { get; set; }

	/// <summary>
	///     Gets the channel where system messages (such as boost and welcome messages) are sent.
	/// </summary>
	[JsonIgnore]
	public DiscordChannel? SystemChannel
		=> this.SystemChannelId.HasValue
			? this.GetChannel(this.SystemChannelId.Value)
			: null;

	/// <summary>
	///     Gets the settings for this guild's system channel.
	/// </summary>
	[JsonProperty("system_channel_flags", NullValueHandling = NullValueHandling.Ignore)]
	public SystemChannelFlags? SystemChannelFlags { get; internal set; }

	/// <summary>
	///     Gets whether this guild's widget is enabled.
	/// </summary>
	[JsonProperty("widget_enabled", NullValueHandling = NullValueHandling.Ignore)]
	public bool? WidgetEnabled { get; internal set; }

	/// <summary>
	///     Gets the widget channel id.
	/// </summary>
	[JsonProperty("widget_channel_id", NullValueHandling = NullValueHandling.Ignore)]
	internal ulong? WidgetChannelId { get; set; }

	/// <summary>
	///     Gets the widget channel for this guild.
	/// </summary>
	[JsonIgnore]
	public DiscordChannel? WidgetChannel
		=> this.WidgetChannelId.HasValue
			? this.GetChannel(this.WidgetChannelId.Value)
			: null;

	/// <summary>
	///     Gets the safety alerts channel id.
	/// </summary>
	[JsonProperty("safety_alerts_channel_id", NullValueHandling = NullValueHandling.Ignore)]
	internal ulong? SafetyAlertsChannelId { get; set; }

	/// <summary>
	///     Gets the safety alert channel for this guild.
	/// </summary>
	[JsonIgnore, RequiresFeature(Attributes.Features.Community)]
	public DiscordChannel? SafetyAltersChannel
		=> this.SafetyAlertsChannelId.HasValue
			? this.GetChannel(this.SafetyAlertsChannelId.Value)
			: null;

	/// <summary>
	///     Gets the rules channel id.
	/// </summary>
	[JsonProperty("rules_channel_id", NullValueHandling = NullValueHandling.Ignore)]
	internal ulong? RulesChannelId { get; set; }

	/// <summary>
	///     Gets the rules channel for this guild.
	///     <para>This is only available if the guild is considered "discoverable".</para>
	/// </summary>
	[JsonIgnore, RequiresFeature(Attributes.Features.Community)]
	public DiscordChannel? RulesChannel
		=> this.RulesChannelId.HasValue
			? this.GetChannel(this.RulesChannelId.Value)
			: null;

	/// <summary>
	///     Gets the public updates channel id.
	/// </summary>
	[JsonProperty("public_updates_channel_id", NullValueHandling = NullValueHandling.Ignore)]
	internal ulong? PublicUpdatesChannelId { get; set; }

	/// <summary>
	///     Gets the public updates channel (where admins and moderators receive messages from Discord) for this guild.
	///     <para>This is only available if the guild is considered "discoverable".</para>
	/// </summary>
	[JsonIgnore, RequiresFeature(Attributes.Features.Community)]
	public DiscordChannel? PublicUpdatesChannel
		=> this.PublicUpdatesChannelId.HasValue
			? this.GetChannel(this.PublicUpdatesChannelId.Value)
			: null;

	/// <summary>
	///     Gets the application id of this guild if it is bot created.
	/// </summary>
	[JsonProperty("application_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? ApplicationId { get; internal set; }

	/// <summary>
	///     Gets a collection of this guild's roles.
	/// </summary>
	[JsonIgnore]
	public IReadOnlyDictionary<ulong, DiscordRole> Roles
		=> new ReadOnlyConcurrentDictionary<ulong, DiscordRole>(this.RolesInternal);

	/// <summary>
	///     Gets a collection of this guild's stickers.
	/// </summary>
	[JsonIgnore]
	public IReadOnlyDictionary<ulong, DiscordSticker> Stickers
		=> new ReadOnlyConcurrentDictionary<ulong, DiscordSticker>(this.StickersInternal);

	/// <summary>
	///     Gets a collection of this guild's emojis.
	/// </summary>
	[JsonIgnore]
	public IReadOnlyDictionary<ulong, DiscordEmoji> Emojis
		=> new ReadOnlyConcurrentDictionary<ulong, DiscordEmoji>(this.EmojisInternal);

	/// <summary>
	///     Gets a collection of this guild's features.
	/// </summary>
	[JsonProperty("features", NullValueHandling = NullValueHandling.Ignore)]
	public IReadOnlyList<string>? RawFeatures { get; internal set; }

	/// <summary>
	///     Gets the guild's features.
	/// </summary>
	[JsonIgnore]
	public GuildFeatures Features => new(this);

	/// <summary>
	///     Gets the required multi-factor authentication level for this guild.
	/// </summary>
	[JsonProperty("mfa_level", NullValueHandling = NullValueHandling.Ignore)]
	public MfaLevel? MfaLevel { get; internal set; }

	/// <summary>
	///     Gets this guild's join date.
	/// </summary>
	[JsonProperty("joined_at", NullValueHandling = NullValueHandling.Ignore)]
	public DateTimeOffset? JoinedAt { get; internal set; }

	/// <summary>
	///     Gets whether this guild is considered to be a large guild.
	/// </summary>
	[JsonProperty("large", NullValueHandling = NullValueHandling.Ignore)]
	public bool IsLarge { get; internal set; }

	/// <summary>
	///     Gets whether this guild is unavailable.
	/// </summary>
	[JsonProperty("unavailable", NullValueHandling = NullValueHandling.Ignore)]
	public bool IsUnavailable { get; internal set; }

	/// <summary>
	///     Gets whether the guild ready event has been thrown.
	/// </summary>
	[JsonIgnore]
	internal bool GuildReadyThrown { get; set; } = false;

	/// <summary>
	///     Gets the total number of members in this guild.
	/// </summary>
	[JsonProperty("member_count", NullValueHandling = NullValueHandling.Ignore)]
	public int? MemberCount { get; internal set; }

	/// <summary>
	///     Gets the maximum amount of members allowed for this guild.
	/// </summary>
	[JsonProperty("max_members", NullValueHandling = NullValueHandling.Ignore)]
	public int? MaxMembers { get; internal set; }

	/// <summary>
	///     Gets the maximum amount of presences allowed for this guild.
	/// </summary>
	[JsonProperty("max_presences", NullValueHandling = NullValueHandling.Ignore)]
	public int? MaxPresences { get; internal set; }

	/// <summary>
	///     Gets the approximate number of members in this guild, when using
	///     <see cref="DiscordClient.GetGuildAsync(ulong, bool?, bool)" /> and having withCounts set to true.
	/// </summary>
	[JsonProperty("approximate_member_count", NullValueHandling = NullValueHandling.Ignore)]
	public int? ApproximateMemberCount { get; internal set; }

	/// <summary>
	///     Gets the approximate number of presences in this guild, when using
	///     <see cref="DiscordClient.GetGuildAsync(ulong, bool?, bool)" /> and having withCounts set to true.
	/// </summary>
	[JsonProperty("approximate_presence_count", NullValueHandling = NullValueHandling.Ignore)]
	public int? ApproximatePresenceCount { get; internal set; }

	/// <summary>
	///     Gets the maximum amount of users allowed per video channel.
	/// </summary>
	[JsonProperty("max_video_channel_users", NullValueHandling = NullValueHandling.Ignore)]
	public int? MaxVideoChannelUsers { get; internal set; }

	/// <summary>
	///     Gets the maximum amount of users allowed per video stage channel.
	/// </summary>
	[JsonProperty("max_stage_video_channel_users", NullValueHandling = NullValueHandling.Ignore)]
	public int? MaxStageVideoChannelUsers { get; internal set; }

	/// <summary>
	///     Gets a dictionary of all the voice states for this guilds. The key for this dictionary is the ID of the user
	///     the voice state corresponds to.
	/// </summary>
	[JsonIgnore]
	public IReadOnlyDictionary<ulong, DiscordVoiceState> VoiceStates
		=> new ReadOnlyConcurrentDictionary<ulong, DiscordVoiceState>(this.VoiceStatesInternal);

	/// <summary>
	///     Gets a dictionary of all the members that belong to this guild. The dictionary's key is the member ID.
	/// </summary>
	[JsonIgnore]
	public IReadOnlyDictionary<ulong, DiscordMember> Members
		=> new ReadOnlyConcurrentDictionary<ulong, DiscordMember>(this.MembersInternal);

	/// <summary>
	///     Gets a dictionary of all the channels associated with this guild. The dictionary's key is the channel ID.
	/// </summary>
	[JsonIgnore]
	public IReadOnlyDictionary<ulong, DiscordChannel> Channels
		=> new ReadOnlyConcurrentDictionary<ulong, DiscordChannel>(this.ChannelsInternal);

	/// <summary>
	///     Gets a dictionary of all the active threads associated with this guild the user has permission to view. The
	///     dictionary's key is the channel ID.
	/// </summary>
	[JsonIgnore]
	public IReadOnlyDictionary<ulong, DiscordThreadChannel> Threads { get; internal set; }

	/// <summary>
	///     Gets a dictionary of all active stage instances. The dictionary's key is the stage ID.
	/// </summary>
	[JsonIgnore]
	public IReadOnlyDictionary<ulong, DiscordStageInstance> StageInstances { get; internal set; }

	/// <summary>
	///     Gets a dictionary of all scheduled events.
	/// </summary>
	[JsonIgnore]
	public IReadOnlyDictionary<ulong, DiscordScheduledEvent> ScheduledEvents { get; internal set; }

	/// <summary>
	///     Gets a dictionary of all soundboard sounds.
	/// </summary>
	[JsonIgnore]
	public IReadOnlyDictionary<ulong, DiscordSoundboardSound> SoundboardSounds { get; internal set; }

	/// <summary>
	///     Gets the guild member for current user.
	/// </summary>
	[JsonIgnore]
	public DiscordMember? CurrentMember
		=> this._currentMemberLazy.Value;

	/// <summary>
	///     Gets the @everyone role for this guild.
	/// </summary>
	[JsonIgnore]
	public DiscordRole? EveryoneRole
		=> this.GetRole(this.Id);

	/// <summary>
	///     Gets whether the current user is the guild's owner.
	/// </summary>
	[JsonProperty("owner", NullValueHandling = NullValueHandling.Ignore)]
	public bool IsOwner
	{
		get => this.IsOwnerInternal || this.OwnerId == this.Discord.CurrentUser.Id;
		internal set => this.IsOwnerInternal = value;
	}

	/// <summary>
	///     Gets the vanity URL code for this guild, when applicable.
	/// </summary>
	[JsonProperty("vanity_url_code", NullValueHandling = NullValueHandling.Ignore)]
	public string? VanityUrlCode { get; internal set; }

	/// <summary>
	///     Gets the guild description, when applicable.
	/// </summary>
	[JsonProperty("description", NullValueHandling = NullValueHandling.Ignore), RequiresFeature(Attributes.Features.Community)]
	public string? Description { get; internal set; }

	/// <summary>
	///     Gets this guild's banner hash, when applicable.
	/// </summary>
	[JsonProperty("banner", NullValueHandling = NullValueHandling.Ignore)]
	public string? BannerHash { get; internal set; }

	/// <summary>
	///     Gets this guild's banner in url form.
	/// </summary>
	[JsonIgnore]
	public string? BannerUrl
		=> !string.IsNullOrWhiteSpace(this.BannerHash) ? $"{DiscordDomain.GetDomain(CoreDomain.DiscordCdn).Uri}{Endpoints.BANNERS}/{this.Id.ToString(CultureInfo.InvariantCulture)}/{this.BannerHash}.{(this.BannerHash.StartsWith("a_", StringComparison.Ordinal) ? "gif" : "png")}" : null;

	/// <summary>
	///     Whether this guild has the community feature enabled.
	/// </summary>
	[JsonIgnore]
	public bool IsCommunity
		=> this.Features.HasFeature(GuildFeaturesEnum.HasCommunityEnabled);

	/// <summary>
	///     Whether this guild has enabled the welcome screen.
	/// </summary>
	[JsonIgnore]
	public bool HasWelcomeScreen
		=> this.Features.HasFeature(GuildFeaturesEnum.HasWelcomeScreenEnabled);

	/// <summary>
	///     Whether this guild has enabled membership screening.
	/// </summary>
	[JsonIgnore]
	public bool HasMemberVerificationGate
		=> this.Features.HasFeature(GuildFeaturesEnum.HasMembershipScreeningEnabled);

	/// <summary>
	///     Gets this guild's premium tier (Nitro boosting).
	/// </summary>
	[JsonProperty("premium_tier", NullValueHandling = NullValueHandling.Ignore)]
	public PremiumTier? PremiumTier { get; internal set; }

	/// <summary>
	///     Gets the amount of members that boosted this guild.
	/// </summary>
	[JsonProperty("premium_subscription_count", NullValueHandling = NullValueHandling.Ignore)]
	public int? PremiumSubscriptionCount { get; internal set; }

	/// <summary>
	///     Whether the premium progress bar is enabled.
	/// </summary>
	[JsonProperty("premium_progress_bar_enabled", NullValueHandling = NullValueHandling.Ignore)]
	public bool? PremiumProgressBarEnabled { get; internal set; }

	/// <summary>
	///     Gets whether this guild is designated as NSFW.
	/// </summary>
	[JsonProperty("nsfw", NullValueHandling = NullValueHandling.Ignore)]
	public bool? IsNsfw { get; internal set; }

	/// <summary>
	///     Gets this guild's hub type, if applicable.
	/// </summary>
	[JsonProperty("hub_type", NullValueHandling = NullValueHandling.Ignore)]
	public HubType? HubType { get; internal set; }

	/// <summary>
	///     Gets the latest onboarding question id.
	/// </summary>
	[JsonProperty("latest_onboarding_question_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? LatestOnboardingQuestionId { get; internal set; }

	/// <summary>
	///     Gets the guild incidents data.
	/// </summary>
	[JsonProperty("incidents_data", NullValueHandling = NullValueHandling.Ignore), DiscordInExperiment]
	public IncidentsData? IncidentsData { get; internal set; }

	/// <summary>
	///     Gets the guild inventory settings.
	/// </summary>
	[JsonProperty("inventory_settings", NullValueHandling = NullValueHandling.Ignore), DiscordDeprecated]
	public DiscordGuildInventorySettings? InventorySettings { get; internal set; }

	[JsonProperty("embed_enabled", NullValueHandling = NullValueHandling.Ignore)]
	public bool? EmbedEnabled { get; internal set; }

	[JsonProperty("embed_channel_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong? EmbedChannelId { get; internal set; }

	[JsonIgnore]
	public DiscordChannel? EmbedChannel
		=> this.EmbedChannelId.HasValue
			? this.GetChannel(this.EmbedChannelId.Value)
			: null;

	/// <summary>
	///     Gets a dictionary of all by position ordered channels associated with this guild. The dictionary's key is the
	///     channel ID.
	/// </summary>
	[JsonIgnore]
	public IReadOnlyDictionary<ulong, DiscordChannel> OrderedChannels
		=> new ReadOnlyDictionary<ulong, DiscordChannel>(this.InternalSortChannels());

	/// <summary>
	///     Whether it is synced.
	/// </summary>
	[JsonIgnore]
	internal bool IsSynced { get; set; }

	/// <summary>
	///     Checks whether this <see cref="DiscordGuild" /> is equal to another <see cref="DiscordGuild" />.
	/// </summary>
	/// <param name="e"><see cref="DiscordGuild" /> to compare to.</param>
	/// <returns>Whether the <see cref="DiscordGuild" /> is equal to this <see cref="DiscordGuild" />.</returns>
	public bool Equals(DiscordGuild e)
		=> e is not null && (ReferenceEquals(this, e) || this.Id == e.Id);

	/// <summary>
	///     Gets the current guild member's voice state.
	/// </summary>
	/// <returns></returns>
	public async Task<DiscordVoiceState?> GetCurrentMemberVoiceStateAsync()
		=> await this.Discord.ApiClient.GetCurrentUserVoiceStateAsync(this.Id);

	/// <summary>
	///     Gets the current voice state for a member.
	/// </summary>
	/// <returns></returns>
	public async Task<DiscordVoiceState?> GetMemberVoiceStateAsync(ulong memberId)
		=> await this.Discord.ApiClient.GetMemberVoiceStateAsync(this.Id, memberId);

	/// <summary>
	///     Sorts the channels.
	/// </summary>
	private Dictionary<ulong, DiscordChannel> InternalSortChannels()
	{
		Dictionary<ulong, DiscordChannel> keyValuePairs = [];
		var orderedChannels = this.GetOrderedChannels();
		foreach (var orderedChannel in orderedChannels)
		{
			if (orderedChannel.Key != 0)
				keyValuePairs.Add(orderedChannel.Key, this.GetChannel(orderedChannel.Key));
			foreach (var chan in orderedChannel.Value)
				keyValuePairs.Add(chan.Id, chan);
		}

		return keyValuePairs;
	}

	/// <summary>
	///     Gets an ordered <see cref="DiscordChannel" /> list out of the channel cache.
	///     Returns a Dictionary where the key is an ulong and can be mapped to <see cref="ChannelType.Category" />
	///     <see cref="DiscordChannel" />s.
	///     Ignore the 0 key here, because that indicates that this is the "has no category" list.
	///     Each value contains a ordered list of text/news and voice/stage channels as <see cref="DiscordChannel" />.
	/// </summary>
	/// <returns>A ordered list of categories with its channels</returns>
	public Dictionary<ulong, List<DiscordChannel>> GetOrderedChannels()
	{
		IReadOnlyList<DiscordChannel> rawChannels = [.. this.ChannelsInternal.Values];

		Dictionary<ulong, List<DiscordChannel>> orderedChannels = new()
		{
			{ 0, [] }
		};

		foreach (var channel in rawChannels.Where(c => c.Type == ChannelType.Category).OrderBy(c => c.Position))
			orderedChannels.Add(channel.Id, []);

		foreach (var channel in rawChannels.Where(c => c is { ParentId: not null, Type: ChannelType.Text or ChannelType.News or ChannelType.Forum }).OrderBy(c => c.Position))
			orderedChannels[channel.ParentId.Value!].Add(channel);
		foreach (var channel in rawChannels.Where(c => c is { ParentId: not null, Type: ChannelType.Voice or ChannelType.Stage }).OrderBy(c => c.Position))
			orderedChannels[channel.ParentId.Value!].Add(channel);

		foreach (var channel in rawChannels.Where(c => !c.ParentId.HasValue && c.Type != ChannelType.Category && c.Type is ChannelType.Text or ChannelType.News or ChannelType.Forum).OrderBy(c => c.Position))
			orderedChannels[0].Add(channel);
		foreach (var channel in rawChannels.Where(c => !c.ParentId.HasValue && c.Type != ChannelType.Category && c.Type is ChannelType.Voice or ChannelType.Stage).OrderBy(c => c.Position))
			orderedChannels[0].Add(channel);

		return orderedChannels;
	}

	/// <summary>
	///     Gets an ordered <see cref="DiscordChannel" /> list.
	///     Returns a Dictionary where the key is an ulong and can be mapped to <see cref="ChannelType.Category" />
	///     <see cref="DiscordChannel" />s.
	///     Ignore the 0 key here, because that indicates that this is the "has no category" list.
	///     Each value contains a ordered list of text/news and voice/stage channels as <see cref="DiscordChannel" />.
	/// </summary>
	/// <returns>A ordered list of categories with its channels</returns>
	public async Task<Dictionary<ulong, List<DiscordChannel>>> GetOrderedChannelsAsync()
	{
		var rawChannels = await this.Discord.ApiClient.GetGuildChannelsAsync(this.Id).ConfigureAwait(false);

		Dictionary<ulong, List<DiscordChannel>> orderedChannels = new()
		{
			{ 0, [] }
		};

		foreach (var channel in rawChannels.Where(c => c.Type == ChannelType.Category).OrderBy(c => c.Position))
			orderedChannels.Add(channel.Id, []);

		foreach (var channel in rawChannels.Where(c => c is { ParentId: not null, Type: ChannelType.Text or ChannelType.News or ChannelType.Forum }).OrderBy(c => c.Position))
			orderedChannels[channel.ParentId.Value!].Add(channel);
		foreach (var channel in rawChannels.Where(c => c is { ParentId: not null, Type: ChannelType.Voice or ChannelType.Stage }).OrderBy(c => c.Position))
			orderedChannels[channel.ParentId.Value!].Add(channel);

		foreach (var channel in rawChannels.Where(c => !c.ParentId.HasValue && c.Type != ChannelType.Category && c.Type is ChannelType.Text or ChannelType.News or ChannelType.Forum).OrderBy(c => c.Position))
			orderedChannels[0].Add(channel);
		foreach (var channel in rawChannels.Where(c => !c.ParentId.HasValue && c.Type != ChannelType.Category && c.Type is ChannelType.Voice or ChannelType.Stage).OrderBy(c => c.Position))
			orderedChannels[0].Add(channel);

		return orderedChannels;
	}

	/// <summary>
	///     Returns a string representation of this guild.
	/// </summary>
	/// <returns>String representation of this guild.</returns>
	public override string ToString()
		=> $"Guild {this.Id}; {this.Name}";

	/// <summary>
	///     Checks whether this <see cref="DiscordGuild" /> is equal to another object.
	/// </summary>
	/// <param name="obj">Object to compare to.</param>
	/// <returns>Whether the object is equal to this <see cref="DiscordGuild" />.</returns>
	public override bool Equals(object obj)
		=> this.Equals(obj as DiscordGuild);

	/// <summary>
	///     Gets the hash code for this <see cref="DiscordGuild" />.
	/// </summary>
	/// <returns>The hash code for this <see cref="DiscordGuild" />.</returns>
	public override int GetHashCode()
		=> this.Id.GetHashCode();

	/// <summary>
	///     Gets whether the two <see cref="DiscordGuild" /> objects are equal.
	/// </summary>
	/// <param name="e1">First guild to compare.</param>
	/// <param name="e2">Second guild to compare.</param>
	/// <returns>Whether the two guilds are equal.</returns>
	public static bool operator ==(DiscordGuild e1, DiscordGuild e2)
	{
		var o1 = e1 as object;
		var o2 = e2 as object;

		return (o1 != null || o2 == null) && (o1 == null || o2 != null) && ((o1 == null && o2 == null) || e1.Id == e2.Id);
	}

	/// <summary>
	///     Gets whether the two <see cref="DiscordGuild" /> objects are not equal.
	/// </summary>
	/// <param name="e1">First guild to compare.</param>
	/// <param name="e2">Second guild to compare.</param>
	/// <returns>Whether the two guilds are not equal.</returns>
	public static bool operator !=(DiscordGuild e1, DiscordGuild e2)
		=> !(e1 == e2);

#region Guild Methods

	/// <summary>
	///     Gets this guilds onboarding configuration.
	/// </summary>
	/// <exception cref="NotFoundException">Thrown when onboarding does not exist for a reason.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	[RequiresFeature(Attributes.Features.Onboarding)]
	public Task<DiscordOnboarding> GetOnboardingAsync()
		=> this.Discord.ApiClient.GetGuildOnboardingAsync(this.Id);

	/// <summary>
	///     Modifies this guilds onboarding configuration.
	/// </summary>
	/// <param name="prompts">The onboarding prompts</param>
	/// <param name="defaultChannelIds">The default channel ids.</param>
	/// <param name="enabled">Whether onboarding is enabled.</param>
	/// <param name="mode">The onboarding mode.</param>
	/// <param name="reason">The reason.</param>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the <see cref="Permissions.ManageGuild" />
	///     permission.
	/// </exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	[RequiresFeature(Attributes.Features.Community)]
	public Task<DiscordOnboarding> ModifyOnboardingAsync(
		Optional<List<DiscordOnboardingPrompt>> prompts = default,
		Optional<List<ulong>> defaultChannelIds = default,
		Optional<bool> enabled = default,
		Optional<OnboardingMode> mode = default,
		string? reason = null
	)
		=> this.Discord.ApiClient.ModifyGuildOnboardingAsync(this.Id, prompts, defaultChannelIds, enabled, mode,
			reason);

	/// <summary>
	///     Gets this guilds server guide configuration.
	/// </summary>
	/// <exception cref="NotFoundException">Thrown when server guide does not exist for a reason.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	[RequiresFeature(Attributes.Features.Onboarding, "Additionally needs to have server guide enabled.")]
	public Task<DiscordServerGuide> GetServerGuideAsync()
		=> this.Discord.ApiClient.GetGuildServerGuideAsync(this.Id);

	/// <summary>
	///     Modifies this guilds server guide configuration.
	/// </summary>
	/// <param name="enabled">Whether the server guide is enabled.</param>
	/// <param name="welcomeMessage">The server guide welcome message.</param>
	/// <param name="newMemberActions">The new member actions.</param>
	/// <param name="resourceChannels">The resource channels.</param>
	/// <param name="reason">The reason.</param>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the <see cref="Permissions.ManageGuild" />
	///     permission.
	/// </exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	[RequiresFeature(Attributes.Features.Onboarding)]
	public Task<DiscordServerGuide> ModifyServerGuideAsync(Optional<bool> enabled = default, Optional<WelcomeMessage> welcomeMessage = default, Optional<List<NewMemberAction>> newMemberActions = default, Optional<List<ResourceChannel>> resourceChannels = default, string? reason = null)
		=> this.Discord.ApiClient.ModifyGuildServerGuideAsync(this.Id, enabled, welcomeMessage, newMemberActions, resourceChannels, reason);

	/// <summary>
	///     Searches the current guild for members who's display name start with the specified name.
	/// </summary>
	/// <param name="name">The name to search for.</param>
	/// <param name="limit">The maximum amount of members to return. Max 1000. Defaults to 1.</param>
	/// <returns>The members found, if any.</returns>
	public Task<IReadOnlyList<DiscordMember>> SearchMembersAsync(string name, int? limit = 1)
		=> this.Discord.ApiClient.SearchGuildMembersAsync(this.Id, name, limit);

	/// <summary>
	///     Adds a new member to this guild
	/// </summary>
	/// <param name="user">The user to add.</param>
	/// <param name="accessToken">The user's access token (OAuth2).</param>
	/// <param name="nickname">The new nickname.</param>
	/// <param name="roles">The new roles.</param>
	/// <param name="muted">Whether this user has to be muted.</param>
	/// <param name="deaf">Whether this user has to be deafened.</param>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the
	///     <see cref="Permissions.CreateInstantInvite" /> permission.
	/// </exception>
	/// <exception cref="NotFoundException">
	///     Thrown when the <paramref name="user" /> or <paramref name="accessToken" /> is not
	///     found.
	/// </exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task AddMemberAsync(
		DiscordUser user,
		string accessToken,
		string? nickname = null,
		IEnumerable<DiscordRole>? roles = null,
		bool? muted = null,
		bool? deaf = null
	)
		=> this.Discord.ApiClient.AddGuildMemberAsync(this.Id, user.Id, accessToken, nickname, roles, muted, deaf);


	/// <summary>
	///     Enables the mfa requirement for this guild.
	/// </summary>
	/// <param name="reason">The audit log reason.</param>
	/// <exception cref="UnauthorizedException">Thrown when the current user is not the guilds owner.</exception>
	/// <exception cref="NotFoundException">Thrown when the guild does not exist.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task EnableMfaAsync(string? reason = null)
		=> this.IsOwner ? this.Discord.ApiClient.EnableGuildMfaAsync(this.Id, reason) : throw new("The current user does not own the guild.");

	/// <summary>
	///     Disables the mfa requirement for this guild.
	/// </summary>
	/// <param name="reason">The audit log reason.</param>
	/// <exception cref="UnauthorizedException">Thrown when the current user is not the guilds owner.</exception>
	/// <exception cref="NotFoundException">Thrown when the guild does not exist.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task DisableMfaAsync(string? reason = null)
		=> this.IsOwner ? this.Discord.ApiClient.DisableGuildMfaAsync(this.Id, reason) : throw new("The current user does not own the guild.");

	/// <summary>
	///     Modifies this guild.
	/// </summary>
	/// <param name="action">Action to perform on this guild.</param>
	/// <returns>The modified guild object.</returns>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the <see cref="Permissions.ManageGuild" />
	///     permission.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the guild does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task<DiscordGuild> ModifyAsync(Action<GuildEditModel> action)
	{
		var mdl = new GuildEditModel();
		action(mdl);

		var afkChannelId = mdl.PublicUpdatesChannel
			.MapOrNull<ulong?>(c => c.Type != ChannelType.Voice
				? throw new ArgumentException("AFK channel needs to be a text channel.")
				: c.Id);

		static Optional<ulong?> ChannelToId(Optional<DiscordChannel> ch, string name)
			=> ch.MapOrNull<ulong?>(c => c.Type != ChannelType.Text && c.Type != ChannelType.News
				? throw new ArgumentException($"{name} channel needs to be a text channel.")
				: c.Id);

		var rulesChannelId = ChannelToId(mdl.RulesChannel, "Rules");
		var publicUpdatesChannelId = ChannelToId(mdl.PublicUpdatesChannel, "Public updates");
		var systemChannelId = ChannelToId(mdl.SystemChannel, "System");

		var iconb64 = MediaTool.Base64FromStream(mdl.Icon);
		var splashb64 = MediaTool.Base64FromStream(mdl.Splash);
		var bannerb64 = MediaTool.Base64FromStream(mdl.Banner);
		var discoverySplashb64 = MediaTool.Base64FromStream(mdl.DiscoverySplash);
		var homeHeaderb64 = MediaTool.Base64FromStream(mdl.HomeHeader);

	       return await this.Discord.ApiClient.ModifyGuildAsync(this.Id, mdl.Name,
		       mdl.VerificationLevel, mdl.DefaultMessageNotifications, mdl.MfaLevel, mdl.ExplicitContentFilter,
		       afkChannelId, mdl.AfkTimeout, iconb64, splashb64,
		       systemChannelId, mdl.SystemChannelFlags, publicUpdatesChannelId, rulesChannelId,
		       mdl.Description, bannerb64, discoverySplashb64, homeHeaderb64, mdl.PreferredLocale, mdl.PremiumProgressBarEnabled, mdl.AuditLogReason).ConfigureAwait(false);
	}

	/// <summary>
	///     Modifies the community settings async.
	///     This sets <see cref="VerificationLevel.High" /> if not highest and <see cref="ExplicitContentFilter.AllMembers" />.
	/// </summary>
	/// <param name="enabled">If true, enables <see cref="GuildFeaturesEnum.HasCommunityEnabled" />.</param>
	/// <param name="rulesChannel">The rules channel.</param>
	/// <param name="publicUpdatesChannel">The public updates channel.</param>
	/// <param name="preferredLocale">The preferred locale. Defaults to en-US.</param>
	/// <param name="description">The description.</param>
	/// <param name="defaultMessageNotifications">
	///     The default message notifications. Defaults to
	///     <see cref="DefaultMessageNotifications.MentionsOnly" />
	/// </param>
	/// <param name="reason">The audit log reason.</param>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the
	///     <see cref="Permissions.Administrator" /> permission.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the guild does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task<DiscordGuild> ModifyCommunitySettingsAsync(bool enabled, DiscordChannel rulesChannel, DiscordChannel publicUpdatesChannel, string preferredLocale = "en-US", string description = null, DefaultMessageNotifications defaultMessageNotifications = Enums.DefaultMessageNotifications.MentionsOnly, string? reason = null)
	{
		var verificationLevel = this.VerificationLevel;
		if (this.VerificationLevel != Enums.VerificationLevel.Highest)
			verificationLevel = Enums.VerificationLevel.High;

		var explicitContentFilter = Enums.ExplicitContentFilter.AllMembers;

		var rulesChannelId = ChannelToId(rulesChannel, "Rules");
		var publicUpdatesChannelId = ChannelToId(publicUpdatesChannel, "Public updates");

		var rfeatures = this.RawFeatures?.ToList() ?? [];
		if (!rfeatures.Contains("COMMUNITY") && enabled)
			rfeatures.Add("COMMUNITY");
		else if (rfeatures.Contains("COMMUNITY") && !enabled)
			rfeatures.Remove("COMMUNITY");

		return await this.Discord.ApiClient.ModifyGuildCommunitySettingsAsync(this.Id, rfeatures, rulesChannelId, publicUpdatesChannelId, preferredLocale, description, defaultMessageNotifications, explicitContentFilter, verificationLevel ?? Optional<VerificationLevel>.None, reason).ConfigureAwait(false);

		static Optional<ulong?> ChannelToId(DiscordChannel? ch, string name)
			=> ch is null
				? null
				: ch.Type is not ChannelType.Text && ch.Type is not ChannelType.News
					? throw new ArgumentException($"{name} channel needs to be a text channel.")
					: ch.Id;
	}

	/// <summary>
	///     Modifies the guild's inventory settings.
	/// </summary>
	/// <param name="enabled">Whether to allow emoji packs to be collected.</param>
	/// <param name="reason">The audit log reason, currently not supported.</param>
	[DiscordDeprecated, RequiresFeature(Attributes.Features.Community | Attributes.Features.Override)]
	public Task<DiscordGuild> ModifyInventorySettingsAsync(bool enabled, string? reason = null)
		=> this.Discord.ApiClient.ModifyGuildInventorySettingsAsync(this.Id, enabled, reason);

	/// <summary>
	///     Modifies the safety alerts settings async.
	/// </summary>
	/// <param name="enabled">If true, enables <see cref="GuildFeaturesEnum.HasCommunityEnabled" />.</param>
	/// <param name="safetyAlertsChannel">The safety alerts channel.</param>
	/// <param name="reason">The audit log reason.</param>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the <see cref="Permissions.ManageGuild" />
	///     permission.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the guild does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	[RequiresFeature(Attributes.Features.Community)]
	public async Task<DiscordGuild> ModifySafetyAlertsSettingsAsync(bool enabled, DiscordChannel safetyAlertsChannel, string? reason = null)
	{
		var safetyAlertsChannelId = ChannelToId(safetyAlertsChannel, "Safety Alerts");

		var rfeatures = this.RawFeatures?.ToList() ?? [];
		if (!rfeatures.Contains("RAID_ALERTS_ENABLED") && enabled)
			rfeatures.Add("RAID_ALERTS_ENABLED");
		else if (rfeatures.Contains("RAID_ALERTS_ENABLED") && !enabled)
			rfeatures.Remove("RAID_ALERTS_ENABLED");

		return await this.Discord.ApiClient.ModifyGuildSafetyAlertsSettingsAsync(this.Id, rfeatures, safetyAlertsChannelId, reason).ConfigureAwait(false);

		static Optional<ulong?> ChannelToId(DiscordChannel? ch, string name)
			=> ch is null
				? null
				: ch.Type != ChannelType.Text && ch.Type != ChannelType.News
					? throw new ArgumentException($"{name} channel needs to be a text channel.")
					: ch.Id;
	}

	/// <summary>
	///     Enables invites for the guild.
	/// </summary>
	/// <param name="reason">The audit log reason.</param>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the <see cref="Permissions.ManageGuild" />
	///     permission.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the guild does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task<DiscordGuild> EnableInvitesAsync(string? reason = null)
	{
		var rfeatures = this.RawFeatures?.ToList() ?? [];
		if (this.Features.HasFeature(GuildFeaturesEnum.InvitesDisabled))
			rfeatures.Remove("INVITES_DISABLED");

		return await this.Discord.ApiClient.ModifyGuildFeaturesAsync(this.Id, rfeatures, reason).ConfigureAwait(false);
	}

	/// <summary>
	///     Disables invites for the guild.
	/// </summary>
	/// <param name="reason">The audit log reason.</param>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the <see cref="Permissions.ManageGuild" />
	///     permission.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the guild does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task<DiscordGuild> DisableInvitesAsync(string? reason = null)
	{
		var rfeatures = this.RawFeatures?.ToList() ?? [];
		if (!this.Features.HasFeature(GuildFeaturesEnum.InvitesDisabled))
			rfeatures.Add("INVITES_DISABLED");

		return await this.Discord.ApiClient.ModifyGuildFeaturesAsync(this.Id, rfeatures, reason).ConfigureAwait(false);
	}

	/// <summary>
	///     Disables invites for the guild.
	/// </summary>
	/// <param name="invitesDisabledUntil">Until when invites are disabled. Set <see langword="null" /> to disable.</param>
	/// <param name="dmsDisabledUntil">Until when direct messages are disabled. Set <see langword="null" /> to disable.</param>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the <see cref="Permissions.ManageGuild" />
	///     permission.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the guild does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task<IncidentsData> ModifyIncidentActionsAsync(DateTimeOffset? invitesDisabledUntil = null, DateTimeOffset? dmsDisabledUntil = null)
		=> invitesDisabledUntil.HasValue &&
		   invitesDisabledUntil.Value.UtcDateTime > DateTimeOffset.UtcNow.UtcDateTime.AddHours(24)
			? throw new InvalidOperationException("Cannot disable invites for more than 24 hours.")
			: dmsDisabledUntil.HasValue &&
			  dmsDisabledUntil.Value.UtcDateTime > DateTimeOffset.UtcNow.UtcDateTime.AddHours(24)
				? throw new InvalidOperationException("Cannot disable direct messages for more than 24 hours.")
				: await this.Discord.ApiClient.ModifyGuildIncidentActionsAsync(this.Id, invitesDisabledUntil, dmsDisabledUntil).ConfigureAwait(false);

	/// <summary>
	///     Timeout a specified member in this guild.
	/// </summary>
	/// <param name="memberId">Member to timeout.</param>
	/// <param name="until">The datetime offset to time out the user. Up to 28 days.</param>
	/// <param name="reason">Reason for audit logs.</param>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the
	///     <see cref="Permissions.ModerateMembers" /> permission.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the member does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task TimeoutAsync(ulong memberId, DateTimeOffset until, string? reason = null)
		=> until.Subtract(DateTimeOffset.UtcNow).Days > 28
			? throw new ArgumentException("Timeout can not be longer than 28 days")
			: this.Discord.ApiClient.ModifyTimeoutAsync(this.Id, memberId, until, reason);

	/// <summary>
	///     Timeout a specified member in this guild.
	/// </summary>
	/// <param name="memberId">Member to timeout.</param>
	/// <param name="until">The timespan to time out the user. Up to 28 days.</param>
	/// <param name="reason">Reason for audit logs.</param>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the
	///     <see cref="Permissions.ModerateMembers" /> permission.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the member does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task TimeoutAsync(ulong memberId, TimeSpan until, string? reason = null)
		=> this.TimeoutAsync(memberId, DateTimeOffset.UtcNow + until, reason);

	/// <summary>
	///     Timeout a specified member in this guild.
	/// </summary>
	/// <param name="memberId">Member to timeout.</param>
	/// <param name="until">The datetime to time out the user. Up to 28 days.</param>
	/// <param name="reason">Reason for audit logs.</param>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the
	///     <see cref="Permissions.ModerateMembers" /> permission.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the member does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task TimeoutAsync(ulong memberId, DateTime until, string? reason = null)
		=> this.TimeoutAsync(memberId, until.ToUniversalTime() - DateTime.UtcNow, reason);

	/// <summary>
	///     Removes the timeout from a specified member in this guild.
	/// </summary>
	/// <param name="memberId">Member to remove the timeout from.</param>
	/// <param name="reason">Reason for audit logs.</param>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the
	///     <see cref="Permissions.ModerateMembers" /> permission.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the member does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task RemoveTimeoutAsync(ulong memberId, string? reason = null)
		=> this.Discord.ApiClient.ModifyTimeoutAsync(this.Id, memberId, null, reason);

	/// <summary>
	///     Bans a specified <see cref="DiscordMember" /> from this guild.
	/// </summary>
	/// <param name="member">Member to ban.</param>
	/// <param name="deleteMessageSeconds">
	///     How many seconds to remove messages from the users. Minimum <c>0</c> seconds and
	///     maximum <c>604800 </c> seconds (7 days).
	/// </param>
	/// <param name="reason">Reason for audit logs.</param>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the <see cref="Permissions.BanMembers" />
	///     permission.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the member does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task BanMemberAsync(DiscordMember member, [DiscordDeprecated("This is now in seconds, we convert it until the next minor release.")] int deleteMessageSeconds = 0, string? reason = null)
		=> this.Discord.ApiClient.CreateGuildBanAsync(this.Id, member.Id, deleteMessageSeconds is < 8 and > 0 ? this.DaysToSeconds(deleteMessageSeconds) : deleteMessageSeconds, reason);

	/// <summary>
	///     Bans a specified <see cref="DiscordUser" />. This doesn't require the user to be in this guild.
	/// </summary>
	/// <param name="user">The user to ban.</param>
	/// <param name="deleteMessageSeconds">
	///     How many seconds to remove messages from the users. Minimum <c>0</c> seconds and
	///     maximum <c>604800 </c> seconds (7 days).
	/// </param>
	/// <param name="reason">Reason for audit logs.</param>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the <see cref="Permissions.BanMembers" />
	///     permission.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the member does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task BanMemberAsync(DiscordUser user, [DiscordDeprecated("This is now in seconds, we convert it until the next minor release.")] int deleteMessageSeconds = 0, string? reason = null)
		=> this.Discord.ApiClient.CreateGuildBanAsync(this.Id, user.Id, deleteMessageSeconds is < 8 and > 0 ? this.DaysToSeconds(deleteMessageSeconds) : deleteMessageSeconds, reason);

	/// <summary>
	///     Bans a specified user ID from this guild. This doesn't require the user to be in this guild.
	/// </summary>
	/// <param name="userId">ID of the user to ban.</param>
	/// <param name="deleteMessageSeconds">
	///     How many seconds to remove messages from the users. Minimum <c>0</c> seconds and
	///     maximum <c>604800 </c> seconds (7 days).
	/// </param>
	/// <param name="reason">Reason for audit logs.</param>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the <see cref="Permissions.BanMembers" />
	///     permission.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the member does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task BanMemberAsync(ulong userId, [DiscordDeprecated("This is now in seconds, we convert it until the next minor release.")] int deleteMessageSeconds = 0, string reason = null)
		=> this.Discord.ApiClient.CreateGuildBanAsync(this.Id, userId, deleteMessageSeconds is < 8 and > 0 ? this.DaysToSeconds(deleteMessageSeconds) : deleteMessageSeconds, reason);

	/// <summary>
	///     Converts days to seconds to help users transition from <c>deleteMessageDays</c> to <c>deleteMessageSeconds</c>.
	/// </summary>
	/// <param name="days">The days to convert to seconds.</param>
	/// <returns>The days in seconds.</returns>
	private int DaysToSeconds(int days)
		=> days * 24 * 60 * 60;

	/// <summary>
	///     Bulk bans a list of <see cref="DiscordMember" />s from this guild.
	/// </summary>
	/// <param name="members">The members to ban.</param>
	/// <param name="deleteMessageSeconds">
	///     How many seconds to remove messages from the users. Minimum <c>0</c> seconds and
	///     maximum <c>604800 </c> seconds (7 days).
	/// </param>
	/// <param name="reason">Reason for audit logs.</param>
	/// <exception cref="ArgumentException">
	///     Thrown when <paramref name="deleteMessageSeconds" /> was too low or too high, or
	///     when <paramref name="members" /> contains more than <c>200</c> members.
	/// </exception>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the <see cref="Permissions.BanMembers" />
	///     or <see cref="Permissions.ManageGuild" /> permission.
	/// </exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordBulkBanResponse> BulkBanMembersAsync(List<DiscordMember> members, int deleteMessageSeconds = 0, string? reason = null)
		=> this.Discord.ApiClient.CreateGuildBulkBanAsync(this.Id, members.Select(x => x.Id).ToList(), deleteMessageSeconds, reason);

	/// <summary>
	///     Bulk bans a list of <see cref="DiscordUser" />s from this guild. This doesn't require the users to be in this
	///     guild.
	/// </summary>
	/// <param name="users">The users to ban.</param>
	/// <param name="deleteMessageSeconds">
	///     How many seconds to remove messages from the users. Minimum <c>0</c> seconds and
	///     maximum <c>604800 </c> seconds (7 days).
	/// </param>
	/// <param name="reason">Reason for audit logs.</param>
	/// <exception cref="ArgumentException">
	///     Thrown when <paramref name="deleteMessageSeconds" /> was too low or too high, or
	///     when <paramref name="users" /> contains more than <c>200</c> users.
	/// </exception>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the <see cref="Permissions.BanMembers" />
	///     or <see cref="Permissions.ManageGuild" /> permission.
	/// </exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordBulkBanResponse> BulkBanMembersAsync(List<DiscordUser> users, int deleteMessageSeconds = 0, string? reason = null)
		=> this.Discord.ApiClient.CreateGuildBulkBanAsync(this.Id, users.Select(x => x.Id).ToList(), deleteMessageSeconds, reason);

	/// <summary>
	///     Bans a list of user IDs from this guild. This doesn't require the users to be in this guild.
	/// </summary>
	/// <param name="userIds">The user IDs to ban.</param>
	/// <param name="deleteMessageSeconds">
	///     How many seconds to remove messages from the users. Minimum <c>0</c> seconds and
	///     maximum <c>604800 </c> seconds (7 days).
	/// </param>
	/// <param name="reason">Reason for audit logs.</param>
	/// <exception cref="ArgumentException">
	///     Thrown when <paramref name="deleteMessageSeconds" /> was too low or too high, or
	///     when <paramref name="userIds" /> contains more than <c>200</c> user ids.
	/// </exception>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the <see cref="Permissions.BanMembers" />
	///     or <see cref="Permissions.ManageGuild" /> permission.
	/// </exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordBulkBanResponse> BulkBanMembersAsync(List<ulong> userIds, int deleteMessageSeconds = 0, string reason = null)
		=> this.Discord.ApiClient.CreateGuildBulkBanAsync(this.Id, userIds, deleteMessageSeconds, reason);

	/// <summary>
	///     Unbans a user from this guild.
	/// </summary>
	/// <param name="user">User to unban.</param>
	/// <param name="reason">Reason for audit logs.</param>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the <see cref="Permissions.BanMembers" />
	///     permission.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the user does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task UnbanMemberAsync(DiscordUser user, string? reason = null)
		=> this.Discord.ApiClient.RemoveGuildBanAsync(this.Id, user.Id, reason);

	/// <summary>
	///     Unbans a user by ID.
	/// </summary>
	/// <param name="userId">ID of the user to unban.</param>
	/// <param name="reason">Reason for audit logs.</param>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the <see cref="Permissions.BanMembers" />
	///     permission.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the user does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task UnbanMemberAsync(ulong userId, string? reason = null)
		=> this.Discord.ApiClient.RemoveGuildBanAsync(this.Id, userId, reason);

	/// <summary>
	///     Leaves this guild.
	/// </summary>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task LeaveAsync()
		=> this.Discord.ApiClient.LeaveGuildAsync(this.Id);

	/// <summary>
	///     Gets the bans for this guild, allowing for pagination.
	/// </summary>
	/// <param name="limit">Maximum number of bans to fetch. Max 1000. Defaults to 1000.</param>
	/// <param name="before">
	///     The Id of the user before which to fetch the bans. Overrides <paramref name="after" /> if both are
	///     present.
	/// </param>
	/// <param name="after">The Id of the user after which to fetch the bans.</param>
	/// <returns>Collection of bans in this guild in ascending order by user id.</returns>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the <see cref="Permissions.BanMembers" />
	///     permission.
	/// </exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<IReadOnlyList<DiscordBan>> GetBansAsync(int? limit = null, ulong? before = null, ulong? after = null)
		=> this.Discord.ApiClient.GetGuildBansAsync(this.Id, limit, before, after);

	/// <summary>
	///     Gets a ban for a specific user.
	/// </summary>
	/// <param name="userId">The Id of the user to get the ban for.</param>
	/// <returns>The requested ban object.</returns>
	/// <exception cref="NotFoundException">Thrown when the specified user is not banned.</exception>
	public Task<DiscordBan> GetBanAsync(ulong userId)
		=> this.Discord.ApiClient.GetGuildBanAsync(this.Id, userId);

	/// <summary>
	///     Tries to get a ban for a specific user.
	/// </summary>
	/// <param name="userId">The Id of the user to get the ban for.</param>
	/// <returns>The requested ban object or null if not found.</returns>
	public async Task<DiscordBan?> TryGetBanAsync(ulong userId)
	{
		try
		{
			return await this.GetBanAsync(userId).ConfigureAwait(false);
		}
		catch (NotFoundException)
		{
			return null;
		}
	}

	/// <summary>
	///     Gets a ban for a specific user.
	/// </summary>
	/// <param name="user">The user to get the ban for.</param>
	/// <returns>The requested ban object.</returns>
	/// <exception cref="NotFoundException">Thrown when the specified user is not banned.</exception>
	public Task<DiscordBan> GetBanAsync(DiscordUser user)
		=> this.GetBanAsync(user.Id);

	/// <summary>
	///     Tries to get a ban for a specific user.
	/// </summary>
	/// <param name="user">The user to get the ban for.</param>
	/// <returns>The requested ban object or null if not found.</returns>
	public async Task<DiscordBan?> TryGetBanAsync(DiscordUser user)
	{
		try
		{
			return await this.GetBanAsync(user).ConfigureAwait(false);
		}
		catch (NotFoundException)
		{
			return null;
		}
	}

	/// <summary>
	///     Gets all auto mod rules for a guild.
	/// </summary>
	/// <returns>A collection of all rules in the guild.</returns>
	[RequiresFeature(Attributes.Features.Community)]
	public Task<ReadOnlyCollection<AutomodRule>> GetAutomodRulesAsync()
		=> this.Discord.ApiClient.GetAutomodRulesAsync(this.Id);

	/// <summary>
	///     Gets a specific auto mod rule.
	/// </summary>
	/// <param name="ruleId">The rule id to get.</param>
	/// <returns>The auto mod rule.</returns>
	[RequiresFeature(Attributes.Features.Community)]
	public Task<AutomodRule> GetAutomodRuleAsync(ulong ruleId)
		=> this.Discord.ApiClient.GetAutomodRuleAsync(this.Id, ruleId);

	/// <summary>
	///     Creates a new auto mod rule in a guild.
	/// </summary>
	/// <param name="name">The name of the rule.</param>
	/// <param name="eventType">The event type of the rule.</param>
	/// <param name="triggerType">The trigger type of the rule.</param>
	/// <param name="actions">The actions of the rule.</param>
	/// <param name="triggerMetadata">The meta data of the rule.</param>
	/// <param name="enabled">Whether this rule is enabled.</param>
	/// <param name="exemptRoles">The exempt roles of the rule.</param>
	/// <param name="exemptChannels">The exempt channels of the rule.</param>
	/// <param name="reason">The reason for this addition</param>
	/// <returns>The created rule.</returns>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the <see cref="Permissions.ManageGuild" />
	///     permission.
	/// </exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	[RequiresFeature(Attributes.Features.Community)]
	public async Task<AutomodRule> CreateAutomodRuleAsync(
		string name,
		AutomodEventType eventType,
		AutomodTriggerType triggerType,
		IEnumerable<AutomodAction> actions,
		AutomodTriggerMetadata triggerMetadata = null,
		bool enabled = false,
		IEnumerable<ulong> exemptRoles = null,
		IEnumerable<ulong> exemptChannels = null,
		string reason = null
	)
		=> await this.Discord.ApiClient.CreateAutomodRuleAsync(this.Id, name, eventType, triggerType, actions, triggerMetadata, enabled, exemptRoles, exemptChannels, reason).ConfigureAwait(false);

#region Scheduled Events

	/// <summary>
	///     Creates a scheduled event.
	/// </summary>
	/// <param name="name">The name.</param>
	/// <param name="scheduledStartTime">The scheduled start time.</param>
	/// <param name="scheduledEndTime">The scheduled end time.</param>
	/// <param name="channel">The channel.</param>
	/// <param name="metadata">The metadata.</param>
	/// <param name="description">The description.</param>
	/// <param name="type">The type.</param>
	/// <param name="coverImage">The cover image.</param>
	/// <param name="recurrenceRule">The recurrence rule.</param>
	/// <param name="reason">The reason.</param>
	/// <returns>A scheduled event.</returns>
	/// <exception cref="ValidationException">Thrown if the user gave an invalid input.</exception>
	/// <exception cref="NotFoundException">Thrown when the guild does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task<DiscordScheduledEvent> CreateScheduledEventAsync(string name, DateTimeOffset scheduledStartTime, DateTimeOffset? scheduledEndTime = null, DiscordChannel channel = null, DiscordScheduledEventEntityMetadata metadata = null, string description = null, ScheduledEventEntityType type = ScheduledEventEntityType.StageInstance, Optional<Stream> coverImage = default, DiscordScheduledEventRecurrenceRule? recurrenceRule = null, string reason = null)
	{
		var coverb64 = MediaTool.Base64FromStream(coverImage);
		return await this.Discord.ApiClient.CreateGuildScheduledEventAsync(this.Id, type is ScheduledEventEntityType.External ? null : channel?.Id, type is ScheduledEventEntityType.External ? metadata : null, name, scheduledStartTime, scheduledEndTime.HasValue && type is ScheduledEventEntityType.External ? scheduledEndTime.Value : null, description, type, coverb64, recurrenceRule, reason).ConfigureAwait(false);
	}

	/// <summary>
	///     Creates a scheduled event with type <see cref="ScheduledEventEntityType.External" />.
	/// </summary>
	/// <param name="name">The name.</param>
	/// <param name="scheduledStartTime">The scheduled start time.</param>
	/// <param name="scheduledEndTime">The scheduled end time.</param>
	/// <param name="location">The location of the external event.</param>
	/// <param name="description">The description.</param>
	/// <param name="coverImage">The cover image.</param>
	/// <param name="recurrenceRule">The recurrence rule.</param>
	/// <param name="reason">The reason.</param>
	/// <returns>A scheduled event.</returns>
	/// <exception cref="ValidationException">Thrown if the user gave an invalid input.</exception>
	/// <exception cref="NotFoundException">Thrown when the guild does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task<DiscordScheduledEvent> CreateExternalScheduledEventAsync(string name, DateTimeOffset scheduledStartTime, DateTimeOffset scheduledEndTime, string location, string description = null, Optional<Stream> coverImage = default, DiscordScheduledEventRecurrenceRule? recurrenceRule = null, string reason = null)
	{
		var coverb64 = MediaTool.Base64FromStream(coverImage);
		return await this.Discord.ApiClient.CreateGuildScheduledEventAsync(this.Id, null, new(location), name, scheduledStartTime, scheduledEndTime, description, ScheduledEventEntityType.External, coverb64, recurrenceRule, reason).ConfigureAwait(false);
	}

	/// <summary>
	///     Gets a specific scheduled events.
	/// </summary>
	/// <param name="scheduledEventId">The Id of the event to get.</param>
	/// <param name="withUserCount">Whether to include user count.</param>
	/// <returns>A scheduled event.</returns>
	/// <exception cref="NotFoundException">Thrown when the guild does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task<DiscordScheduledEvent> GetScheduledEventAsync(ulong scheduledEventId, bool? withUserCount = null)
		=> this.ScheduledEventsInternal.TryGetValue(scheduledEventId, out var ev) ? ev : await this.Discord.ApiClient.GetGuildScheduledEventAsync(this.Id, scheduledEventId, withUserCount).ConfigureAwait(false);

	/// <summary>
	///     Tries to get a specific scheduled events.
	/// </summary>
	/// <param name="scheduledEventId">The Id of the event to get.</param>
	/// <param name="withUserCount">Whether to include user count.</param>
	/// <returns>A scheduled event or null if not found.</returns>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task<DiscordScheduledEvent?> TryGetScheduledEventAsync(ulong scheduledEventId, bool? withUserCount = null)
	{
		try
		{
			return await this.GetScheduledEventAsync(scheduledEventId, withUserCount).ConfigureAwait(false);
		}
		catch (NotFoundException)
		{
			return null;
		}
	}

	/// <summary>
	///     Gets a specific scheduled events.
	/// </summary>
	/// <param name="scheduledEvent">The event to get.</param>
	/// <param name="withUserCount">Whether to include user count.</param>
	/// <returns>A scheduled event.</returns>
	/// <exception cref="NotFoundException">Thrown when the guild does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task<DiscordScheduledEvent> GetScheduledEventAsync(DiscordScheduledEvent scheduledEvent, bool? withUserCount = null)
		=> await this.GetScheduledEventAsync(scheduledEvent.Id, withUserCount).ConfigureAwait(false);

	/// <summary>
	///     Tries to get a specific scheduled events.
	/// </summary>
	/// <param name="scheduledEvent">The event to get.</param>
	/// <param name="withUserCount">Whether to include user count.</param>
	/// <returns>A scheduled event or null if not found.</returns>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task<DiscordScheduledEvent?> TryGetScheduledEventAsync(DiscordScheduledEvent scheduledEvent, bool? withUserCount = null)
	{
		try
		{
			return await this.GetScheduledEventAsync(scheduledEvent, withUserCount).ConfigureAwait(false);
		}
		catch (NotFoundException)
		{
			return null;
		}
	}

	/// <summary>
	///     Gets the guilds scheduled events.
	/// </summary>
	/// <param name="withUserCount">Whether to include user count.</param>
	/// <returns>A list of the guilds scheduled events.</returns>
	/// <exception cref="NotFoundException">Thrown when the guild does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task<IReadOnlyDictionary<ulong, DiscordScheduledEvent>> GetScheduledEventsAsync(bool? withUserCount = null)
		=> await this.Discord.ApiClient.ListGuildScheduledEventsAsync(this.Id, withUserCount).ConfigureAwait(false);

#endregion

	/// <summary>
	///     Creates a new text channel in this guild.
	/// </summary>
	/// <param name="name">Name of the new channel.</param>
	/// <param name="parent">Category to put this channel in.</param>
	/// <param name="topic">Topic of the channel.</param>
	/// <param name="overwrites">Permission overwrites for this channel.</param>
	/// <param name="nsfw">Whether the channel is to be flagged as not safe for work.</param>
	/// <param name="perUserRateLimit">Slow mode timeout for users.</param>
	/// <param name="defaultAutoArchiveDuration">The default auto archive duration for new threads.</param>
	/// <param name="flags">The flags of the new channel.</param>
	/// <param name="reason">Reason for audit logs.</param>
	/// <returns>The newly-created channel.</returns>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the
	///     <see cref="Permissions.ManageChannels" /> permission.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the guild does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordChannel> CreateTextChannelAsync(string name, DiscordChannel? parent = null, Optional<string> topic = default, IEnumerable<DiscordOverwriteBuilder>? overwrites = null, bool? nsfw = null, Optional<int?> perUserRateLimit = default, ThreadAutoArchiveDuration defaultAutoArchiveDuration = ThreadAutoArchiveDuration.OneDay, Optional<ChannelFlags?> flags = default, string? reason = null)
		=> this.CreateChannelAsync(name, ChannelType.Text, parent, topic, null, null, overwrites, nsfw, perUserRateLimit, null, defaultAutoArchiveDuration, flags, reason);

	/// <summary>
	///     Creates a new forum channel in this guild.
	///     <note type="note">The field template is not yet released, so it won't applied.</note>
	/// </summary>
	/// <param name="name">Name of the new channel.</param>
	/// <param name="parent">Category to put this channel in.</param>
	/// <param name="topic">Topic of the channel.</param>
	/// <param name="overwrites">Permission overwrites for this channel.</param>
	/// <param name="nsfw">Whether the channel is to be flagged as not safe for work.</param>
	/// <param name="defaultReactionEmoji">The default reaction emoji for posts.</param>
	/// <param name="perUserRateLimit">Slow mode timeout for users.</param>
	/// <param name="postCreateUserRateLimit">Slow mode timeout for user post creations.</param>
	/// <param name="defaultAutoArchiveDuration">The default auto archive duration for new threads.</param>
	/// <param name="defaultSortOrder">The default sort order for posts in the new channel.</param>
	/// <param name="defaultLayout">The default forum layout for this channel</param>
	/// <param name="flags">The flags of the new channel.</param>
	/// <param name="reason">Reason for audit logs.</param>
	/// <returns>The newly-created channel.</returns>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the
	///     <see cref="Permissions.ManageChannels" /> permission or the guild does not have the forum channel feature.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the guild does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordChannel> CreateForumChannelAsync(
		string name,
		DiscordChannel? parent = null,
		Optional<string> topic = default,
		IEnumerable<DiscordOverwriteBuilder>? overwrites = null,
		bool? nsfw = null,
		Optional<ForumReactionEmoji> defaultReactionEmoji = default,
		Optional<int?> perUserRateLimit = default,
		Optional<int?> postCreateUserRateLimit = default,
		ThreadAutoArchiveDuration defaultAutoArchiveDuration = ThreadAutoArchiveDuration.OneDay,
		Optional<ForumPostSortOrder> defaultSortOrder = default,
		Optional<ForumLayout?> defaultLayout = default,
		Optional<ChannelFlags?> flags = default,
		string? reason = null
	)
		=> this.Discord.ApiClient.CreateGuildForumChannelAsync(this.Id, name, parent?.Id, topic, null, nsfw, defaultReactionEmoji, perUserRateLimit, postCreateUserRateLimit, defaultSortOrder, defaultLayout, defaultAutoArchiveDuration, overwrites, flags, reason);

	/// <summary>
	///     Creates a new channel category in this guild.
	/// </summary>
	/// <param name="name">Name of the new category.</param>
	/// <param name="overwrites">Permission overwrites for this category.</param>
	/// <param name="reason">Reason for audit logs.</param>
	/// <returns>The newly-created channel category.</returns>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the
	///     <see cref="Permissions.ManageChannels" /> permission.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the guild does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordChannel> CreateChannelCategoryAsync(string name, IEnumerable<DiscordOverwriteBuilder> overwrites = null, string reason = null)
		=> this.CreateChannelAsync(name, ChannelType.Category, null, Optional.None, null, null, overwrites, null, Optional.None, null, null, Optional.None, reason);

	/// <summary>
	///     Creates a new stage channel in this guild.
	/// </summary>
	/// <param name="name">Name of the new stage channel.</param>
	/// <param name="overwrites">Permission overwrites for this stage channel.</param>
	/// <param name="reason">Reason for audit logs.</param>
	/// <returns>The newly-created stage channel.</returns>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the
	///     <see cref="Permissions.ManageChannels" />.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the guild does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	/// <exception cref="NotSupportedException">Thrown when the guilds has not enabled community.</exception>
	[RequiresFeature(Attributes.Features.Community)]
	public Task<DiscordChannel> CreateStageChannelAsync(string name, IEnumerable<DiscordOverwriteBuilder> overwrites = null, string reason = null)
		=> this.Features.HasFeature(GuildFeaturesEnum.HasCommunityEnabled) ? this.CreateChannelAsync(name, ChannelType.Stage, null, Optional.None, null, null, overwrites, null, Optional.None, null, null, Optional.None, reason) : throw new NotSupportedException("Guild has not enabled community. Can not create a stage channel.");

	/// <summary>
	///     Creates a new news channel in this guild.
	/// </summary>
	/// <param name="name">Name of the new news channel.</param>
	/// <param name="overwrites">Permission overwrites for this news channel.</param>
	/// <param name="defaultAutoArchiveDuration">The default auto archive duration for new threads.</param>
	/// <param name="flags">The flags of the new channel.</param>
	/// <param name="reason">Reason for audit logs.</param>
	/// <returns>The newly-created news channel.</returns>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the
	///     <see cref="Permissions.ManageChannels" />.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the guild does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	/// <exception cref="NotSupportedException">Thrown when the guilds has not enabled community.</exception>
	[RequiresFeature(Attributes.Features.Community)]
	public Task<DiscordChannel> CreateNewsChannelAsync(string name, IEnumerable<DiscordOverwriteBuilder>? overwrites = null, string? reason = null, ThreadAutoArchiveDuration defaultAutoArchiveDuration = ThreadAutoArchiveDuration.OneDay, Optional<ChannelFlags?> flags = default)
		=> this.Features.HasFeature(GuildFeaturesEnum.HasCommunityEnabled) ? this.CreateChannelAsync(name, ChannelType.News, null, Optional.None, null, null, overwrites, null, Optional.None, null, defaultAutoArchiveDuration, flags, reason) : throw new NotSupportedException("Guild has not enabled community. Can not create a news channel.");

	/// <summary>
	///     Creates a new voice channel in this guild.
	/// </summary>
	/// <param name="name">Name of the new channel.</param>
	/// <param name="parent">Category to put this channel in.</param>
	/// <param name="bitrate">Bitrate of the channel.</param>
	/// <param name="userLimit">Maximum number of users in the channel.</param>
	/// <param name="overwrites">Permission overwrites for this channel.</param>
	/// <param name="flags">The flags of the new channel.</param>
	/// <param name="qualityMode">Video quality mode of the channel.</param>
	/// <param name="reason">Reason for audit logs.</param>
	/// <returns>The newly-created channel.</returns>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the
	///     <see cref="Permissions.ManageChannels" /> permission.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the guild does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordChannel> CreateVoiceChannelAsync(string name, DiscordChannel? parent = null, int? bitrate = null, int? userLimit = null, IEnumerable<DiscordOverwriteBuilder>? overwrites = null, VideoQualityMode? qualityMode = null, Optional<ChannelFlags?> flags = default, string? reason = null)
		=> this.CreateChannelAsync(name, ChannelType.Voice, parent, Optional.None, bitrate, userLimit, overwrites, null, Optional.None, qualityMode, null, flags, reason);

	/// <summary>
	///     Creates a new channel in this guild.
	/// </summary>
	/// <param name="name">Name of the new channel.</param>
	/// <param name="type">Type of the new channel.</param>
	/// <param name="parent">Category to put this channel in.</param>
	/// <param name="topic">Topic of the channel.</param>
	/// <param name="bitrate">Bitrate of the channel. Applies to voice only.</param>
	/// <param name="userLimit">Maximum number of users in the channel. Applies to voice only.</param>
	/// <param name="overwrites">Permission overwrites for this channel.</param>
	/// <param name="nsfw">Whether the channel is to be flagged as not safe for work. Applies to text only.</param>
	/// <param name="perUserRateLimit">Slow mode timeout for users.</param>
	/// <param name="qualityMode">Video quality mode of the channel. Applies to voice only.</param>
	/// <param name="defaultAutoArchiveDuration">The default auto archive duration for new threads.</param>
	/// <param name="flags">The flags of the new channel.</param>
	/// <param name="reason">Reason for audit logs.</param>
	/// <returns>The newly-created channel.</returns>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the
	///     <see cref="Permissions.ManageChannels" /> permission.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the guild does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordChannel> CreateChannelAsync(
		string name,
		ChannelType type,
		DiscordChannel? parent = null,
		Optional<string> topic = default,
		int? bitrate = null,
		int? userLimit = null,
		IEnumerable<DiscordOverwriteBuilder>? overwrites = null,
		bool? nsfw = null,
		Optional<int?> perUserRateLimit = default,
		VideoQualityMode? qualityMode = null,
		ThreadAutoArchiveDuration? defaultAutoArchiveDuration = null,
		Optional<ChannelFlags?> flags = default,
		string? reason = null
	) =>
		// technically you can create news/store channels but not always
		type != ChannelType.Text && type != ChannelType.Voice && type != ChannelType.Category && type != ChannelType.News && type != ChannelType.Store && type != ChannelType.Stage
			? throw new ArgumentException("Channel type must be text, voice, stage, or category.", nameof(type))
			: type == ChannelType.Category && parent is not null
				? throw new ArgumentException("Cannot specify parent of a channel category.", nameof(parent))
				: this.Discord.ApiClient.CreateGuildChannelAsync(this.Id, name, type, parent?.Id, topic, bitrate, userLimit, overwrites, nsfw, perUserRateLimit, qualityMode, defaultAutoArchiveDuration, flags, reason);

	/// <summary>
	///     Gets active threads. Can contain more threads.
	///     If the result's value 'HasMore' is true, you need to recall this function to get older threads.
	/// </summary>
	/// <exception cref="NotFoundException">Thrown when the thread does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordThreadResult> GetActiveThreadsAsync()
		=> this.Discord.ApiClient.GetActiveThreadsAsync(this.Id);

	/// <summary>
	///     <para>Deletes all channels in this guild.</para>
	///     <para>Note that this is irreversible. Use carefully!</para>
	/// </summary>
	/// <returns></returns>
	public Task DeleteAllChannelsAsync()
	{
		var tasks = this.Channels.Values.Select(xc => xc.DeleteAsync());
		return Task.WhenAll(tasks);
	}

	/// <summary>
	///     Estimates the number of users to be pruned.
	/// </summary>
	/// <param name="days">Minimum number of inactivity days required for users to be pruned. Defaults to 7.</param>
	/// <param name="includedRoles">The roles to be included in the prune.</param>
	/// <returns>Number of users that will be pruned.</returns>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the <see cref="Permissions.KickMembers" />
	///     permission.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the guild does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<int> GetPruneCountAsync(int days = 7, IEnumerable<DiscordRole>? includedRoles = null)
	{
		if (includedRoles != null)
		{
			includedRoles = includedRoles.Where(r => r != null);
			var rawRoleIds = includedRoles
				.Where(x => this.RolesInternal.ContainsKey(x.Id))
				.Select(x => x.Id);

			return this.Discord.ApiClient.GetGuildPruneCountAsync(this.Id, days, rawRoleIds);
		}

		return this.Discord.ApiClient.GetGuildPruneCountAsync(this.Id, days, null);
	}

	/// <summary>
	///     Prunes inactive users from this guild.
	/// </summary>
	/// <param name="days">Minimum number of inactivity days required for users to be pruned. Defaults to 7.</param>
	/// <param name="computePruneCount">
	///     Whether to return the prune count after this method completes. This is discouraged for
	///     larger guilds.
	/// </param>
	/// <param name="includedRoles">The roles to be included in the prune.</param>
	/// <param name="reason">Reason for audit logs.</param>
	/// <returns>Number of users pruned.</returns>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the
	///     <see cref="Permissions.ManageChannels" /> permission.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the guild does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<int?> PruneAsync(int days = 7, bool computePruneCount = true, IEnumerable<DiscordRole>? includedRoles = null, string? reason = null)
	{
		if (includedRoles != null)
		{
			includedRoles = includedRoles.Where(r => r != null);
			var rawRoleIds = includedRoles
				.Where(x => this.RolesInternal.ContainsKey(x.Id))
				.Select(x => x.Id);

			return this.Discord.ApiClient.BeginGuildPruneAsync(this.Id, days, computePruneCount, rawRoleIds, reason);
		}

		return this.Discord.ApiClient.BeginGuildPruneAsync(this.Id, days, computePruneCount, null, reason);
	}

	/// <summary>
	///     Gets integrations attached to this guild.
	/// </summary>
	/// <returns>Collection of integrations attached to this guild.</returns>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the <see cref="Permissions.ManageGuild" />
	///     permission.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the guild does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<IReadOnlyList<DiscordIntegration>> GetIntegrationsAsync()
		=> this.Discord.ApiClient.GetGuildIntegrationsAsync(this.Id);

	/// <summary>
	///     Attaches an integration from current user to this guild.
	/// </summary>
	/// <param name="integration">Integration to attach.</param>
	/// <returns>The integration after being attached to the guild.</returns>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the <see cref="Permissions.ManageGuild" />
	///     permission.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the guild does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordIntegration> AttachUserIntegrationAsync(DiscordIntegration integration)
		=> this.Discord.ApiClient.CreateGuildIntegrationAsync(this.Id, integration.Type, integration.Id);

	/// <summary>
	///     Modifies an integration in this guild.
	/// </summary>
	/// <param name="integration">Integration to modify.</param>
	/// <param name="expireBehaviour">Number of days after which the integration expires.</param>
	/// <param name="expireGracePeriod">Length of grace period which allows for renewing the integration.</param>
	/// <param name="enableEmoticons">Whether emotes should be synced from this integration.</param>
	/// <returns>The modified integration.</returns>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the <see cref="Permissions.ManageGuild" />
	///     permission.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the guild does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordIntegration> ModifyIntegrationAsync(DiscordIntegration integration, int expireBehaviour, int expireGracePeriod, bool enableEmoticons)
		=> this.Discord.ApiClient.ModifyGuildIntegrationAsync(this.Id, integration.Id, expireBehaviour, expireGracePeriod, enableEmoticons);

	/// <summary>
	///     Removes an integration from this guild.
	/// </summary>
	/// <param name="integration">Integration to remove.</param>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the <see cref="Permissions.ManageGuild" />
	///     permission.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the guild does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task DeleteIntegrationAsync(DiscordIntegration integration)
		=> this.Discord.ApiClient.DeleteGuildIntegrationAsync(this.Id, integration);

	/// <summary>
	///     Forces re-synchronization of an integration for this guild.
	/// </summary>
	/// <param name="integration">Integration to synchronize.</param>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the <see cref="Permissions.ManageGuild" />
	///     permission.
	/// </exception>
	/// <exception cref="NotFoundException">Thrown when the guild does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task SyncIntegrationAsync(DiscordIntegration integration)
		=> this.Discord.ApiClient.SyncGuildIntegrationAsync(this.Id, integration.Id);

	/// <summary>
	///     Gets the voice regions for this guild.
	/// </summary>
	/// <returns>Voice regions available for this guild.</returns>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task<IReadOnlyList<DiscordVoiceRegion>> ListVoiceRegionsAsync()
	{
		var vrs = await this.Discord.ApiClient.GetGuildVoiceRegionsAsync(this.Id).ConfigureAwait(false);
		foreach (var xvr in vrs)
			this.Discord.InternalVoiceRegions.TryAdd(xvr.Id, xvr);

		return vrs;
	}

	/// <summary>
	///     Gets an invite from this guild from an invite code.
	/// </summary>
	/// <param name="code">The invite code</param>
	/// <returns>An invite, or null if not in cache.</returns>
	public DiscordInvite? GetInvite(string code)
		=> this.Invites.GetValueOrDefault(code);

	/// <summary>
	///     Gets all the invites created for all the channels in this guild.
	/// </summary>
	/// <returns>A collection of invites.</returns>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task<IReadOnlyList<DiscordInvite>> GetInvitesAsync()
	{
		var res = await this.Discord.ApiClient.GetGuildInvitesAsync(this.Id).ConfigureAwait(false);

		var intents = this.Discord.Configuration.Intents;

		if (!intents.HasIntent(DiscordIntents.GuildInvites))
			foreach (var r in res)
				this.Invites[r.Code] = r;

		return res;
	}

	/// <summary>
	///     Gets the vanity invite for this guild.
	/// </summary>
	/// <returns>A partial vanity invite.</returns>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the <see cref="Permissions.ManageGuild" />
	///     permission.
	/// </exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordInvite> GetVanityInviteAsync()
		=> this.Discord.ApiClient.GetGuildVanityUrlAsync(this.Id);

	/// <summary>
	///     Gets all the webhooks created for all the channels in this guild.
	/// </summary>
	/// <returns>A collection of webhooks this guild has.</returns>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the
	///     <see cref="Permissions.ManageWebhooks" /> permission.
	/// </exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<IReadOnlyList<DiscordWebhook>> GetWebhooksAsync()
		=> this.Discord.ApiClient.GetGuildWebhooksAsync(this.Id);

	/// <summary>
	///     Gets this guild's widget image.
	/// </summary>
	/// <param name="bannerType">The format of the widget.</param>
	/// <returns>The URL of the widget image.</returns>
	public string GetWidgetImage(WidgetType bannerType = WidgetType.Shield)
	{
		var param = bannerType switch
		{
			WidgetType.Banner1 => "banner1",
			WidgetType.Banner2 => "banner2",
			WidgetType.Banner3 => "banner3",
			WidgetType.Banner4 => "banner4",
			_ => "shield"
		};
		return $"{Utilities.GetApiBaseUri(this.Discord.Configuration)}{Endpoints.GUILDS}/{this.Id}{Endpoints.WIDGET_PNG}?style={param}";
	}

	/// <summary>
	///     Gets a member of this guild by their user ID.
	/// </summary>
	/// <param name="userId">ID of the member to get.</param>
	/// <param name="fetch">Whether to fetch the member from the api prior to cache.</param>
	/// <returns>The requested member.</returns>
	/// <exception cref="NotFoundException">Thrown when the member does not exist in this guild.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task<DiscordMember> GetMemberAsync(ulong userId, bool fetch = false)
	{
		if (!fetch && this.MembersInternal.TryGetValue(userId, out var mbr))
			return mbr;

		mbr = await this.Discord.ApiClient.GetGuildMemberAsync(this.Id, userId).ConfigureAwait(false);

		var intents = this.Discord.Configuration.Intents;

		if (intents.HasIntent(DiscordIntents.GuildMembers))
			this.MembersInternal[userId] = mbr;

		return mbr;
	}

	/// <summary>
	///     Gets a member of this guild by their user ID.
	/// </summary>
	/// <param name="userId">ID of the member to get.</param>
	/// <param name="fetch">Whether to fetch the member from the api prior to cache.</param>
	/// <returns>The requested <see cref="DiscordMember" /> if the member was found, otherwise <see langword="null" />.</returns>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task<DiscordMember?> TryGetMemberAsync(ulong userId, bool fetch = false)
	{
		if (!fetch && this.MembersInternal.TryGetValue(userId, out var mbr))
			return mbr;

		try
		{
			mbr = await this.Discord.ApiClient.GetGuildMemberAsync(this.Id, userId).ConfigureAwait(false);
		}
		catch (NotFoundException)
		{
			return null;
		}

		var intents = this.Discord.Configuration.Intents;

		if (intents.HasIntent(DiscordIntents.GuildMembers))
			this.MembersInternal[userId] = mbr;

		return mbr;
	}

	/// <summary>
	///     Gets a member of this guild by their user ID.
	/// </summary>
	/// <param name="userId">ID of the member to get.</param>
	/// <param name="member">The requested member if found, otherwise <see langword="null" />.</param>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	/// <returns>Whether the member was found.</returns>
	public bool TryGetMember(ulong userId, [NotNullWhen(true)] out DiscordMember? member)
	{
		member = this.TryGetMemberAsync(userId).Result;
		return member is not null;
	}

	/// <summary>
	///     Retrieves a full list of members from Discord. This method will bypass cache.
	/// </summary>
	/// <returns>A collection of all members in this guild.</returns>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task<IReadOnlyCollection<DiscordMember>> GetAllMembersAsync()
	{
		var recmbr = new HashSet<DiscordMember>();

		var recd = 1000;
		var last = 0ul;

		var intents = this.Discord.Configuration.Intents;
		var hasIntent = intents.HasIntent(DiscordIntents.GuildMembers);
		while (recd > 0)
		{
			var tms = await this.Discord.ApiClient.ListGuildMembersAsync(this.Id, 1000, last == 0 ? null : last).ConfigureAwait(false);
			recd = tms.Count;

			foreach (var xtm in tms)
			{
				var usr = new DiscordUser(xtm.User)
				{
					Discord = this.Discord
				};

				usr = this.Discord.UserCache.AddOrUpdate(xtm.User.Id, usr, (id, old) =>
				{
					old.Username = usr.Username;
					old.Discriminator = usr.Discriminator;
					old.AvatarHash = usr.AvatarHash;
					old.BannerHash = usr.BannerHash;
					old.BannerColorInternal = usr.BannerColorInternal;
					old.AvatarDecorationData = usr.AvatarDecorationData;
					old.Collectibles = usr.Collectibles;
					old.IsSystem = usr.IsSystem;
					old.IsBot = usr.IsBot;
					old.ThemeColorsInternal = usr.ThemeColorsInternal;
					old.Pronouns = usr.Pronouns;
					old.Locale = usr.Locale;
					old.GlobalName = usr.GlobalName;
					old.PrimaryGuild = usr.PrimaryGuild;
					return old;
				});
				var mbr = new DiscordMember(xtm)
				{
					Discord = this.Discord,
					GuildId = this.Id
				};
				recmbr.Add(mbr);
				if (hasIntent)
					this.MembersInternal[usr.Id] = mbr;
			}

			var tm = tms.LastOrDefault();
			last = tm?.User.Id ?? 0;
		}

		return new ReadOnlySet<DiscordMember>(recmbr);
	}

	/// <summary>
	///     Requests that Discord send a list of guild members based on the specified arguments. This method will fire the
	///     <see cref="DiscordClient.GuildMembersChunked" /> event.
	///     <para>
	///         If no arguments aside from <paramref name="presences" /> and <paramref name="nonce" /> are specified, this
	///         will request all guild members.
	///     </para>
	/// </summary>
	/// <param name="query">
	///     Filters the returned members based on what the username starts with. Either this or <paramref name="userIds" />
	///     must not be null.
	///     The <paramref name="limit" /> must also be greater than 0 if this is specified.
	/// </param>
	/// <param name="limit">
	///     Total number of members to request. This must be greater than 0 if <paramref name="query" /> is
	///     specified.
	/// </param>
	/// <param name="presences">
	///     Whether to include the
	///     <see cref="DisCatSharp.EventArgs.GuildMembersChunkEventArgs.Presences" /> associated with the fetched members.
	/// </param>
	/// <param name="userIds">
	///     Whether to limit the request to the specified user ids. Either this or <paramref name="query" />
	///     must not be null.
	/// </param>
	/// <param name="nonce">The unique string to identify the response.</param>
	public async Task RequestMembersAsync(string? query = null, int limit = 0, bool? presences = null, IEnumerable<ulong>? userIds = null, string? nonce = null)
	{
		if (this.Discord is not DiscordClient client)
			throw new InvalidOperationException("This operation is only valid for regular Discord clients.");

		if (query == null && userIds == null)
			throw new ArgumentException("The query and user IDs cannot both be null.");

		if (query != null && userIds != null)
			query = null;

		var grgm = new GatewayRequestGuildMembers(this)
		{
			Query = query,
			Limit = limit >= 0 ? limit : 0,
			Presences = presences,
			UserIds = userIds,
			Nonce = nonce
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
	///     Gets all the channels this guild has.
	/// </summary>
	/// <returns>A collection of this guild's channels.</returns>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<IReadOnlyList<DiscordChannel>> GetChannelsAsync()
		=> this.Discord.ApiClient.GetGuildChannelsAsync(this.Id);

	/// <summary>
	///     Creates a new role in this guild.
	/// </summary>
	/// <param name="name">Name of the role.</param>
	/// <param name="permissions">Permissions for the role.</param>
	/// <param name="color">Color for the role.</param>
	/// <param name="hoist">Whether the role is to be hoisted.</param>
	/// <param name="mentionable">Whether the role is to be mentionable.</param>
	/// <param name="reason">Reason for audit logs.</param>
	/// <returns>The newly-created role.</returns>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the <see cref="Permissions.ManageRoles" />
	///     permission.
	/// </exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordRole> CreateRoleAsync(string name = null, Permissions? permissions = null, DiscordColor? color = null, bool? hoist = null, bool? mentionable = null, string reason = null)
		=> this.Discord.ApiClient.CreateGuildRoleAsync(this.Id, name, permissions, color?.Value, hoist, mentionable, reason);

	/// <summary>
	///     Gets a role from this guild's cache by its ID.
	/// </summary>
	/// <param name="id">ID of the role to get.</param>
	/// <returns>Requested role.</returns>
	public DiscordRole? GetRole(ulong id)
		=> this.RolesInternal.GetValueOrDefault(id);

	/// <summary>
	///     Gets a role from this guild from the api by its ID.
	/// </summary>
	/// <param name="id">ID of the role to get.</param>
	/// <returns>Requested role.</returns>
	public async Task<DiscordRole> GetRoleAsync(ulong id)
		=> await this.Discord.ApiClient.GetGuildRoleAsync(this.Id, id);

	/// <summary>
	///     Gets a channel from this guild by its ID.
	/// </summary>
	/// <param name="id">ID of the channel to get.</param>
	/// <returns>Requested channel.</returns>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public DiscordChannel? GetChannel(ulong id)
		=> this.ChannelsInternal.GetValueOrDefault(id);

	/// <summary>
	///     Gets a thread from this guild by its ID.
	/// </summary>
	/// <param name="id">ID of the thread to get.</param>
	/// <returns>Requested thread.</returns>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public DiscordThreadChannel? GetThread(ulong id)
		=> this.ThreadsInternal.GetValueOrDefault(id);

	/// <summary>
	///     Gets all of this guild's custom emojis.
	/// </summary>
	/// <returns>All of this guild's custom emojis.</returns>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<IReadOnlyList<DiscordGuildEmoji>> GetEmojisAsync()
		=> this.Discord.ApiClient.GetGuildEmojisAsync(this.Id);

	/// <summary>
	///     Gets this guild's specified custom emoji.
	/// </summary>
	/// <param name="id">ID of the emoji to get.</param>
	/// <returns>The requested custom emoji.</returns>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordGuildEmoji> GetEmojiAsync(ulong id)
		=> this.Discord.ApiClient.GetGuildEmojiAsync(this.Id, id);

	/// <summary>
	///     Creates a new custom emoji for this guild.
	/// </summary>
	/// <param name="name">Name of the new emoji.</param>
	/// <param name="image">Image to use as the emoji.</param>
	/// <param name="roles">
	///     Roles for which the emoji will be available. This works only if your application is whitelisted as
	///     integration.
	/// </param>
	/// <param name="reason">Reason for audit log.</param>
	/// <returns>The newly-created emoji.</returns>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the
	///     <see cref="Permissions.ManageGuildExpressions" /> permission.
	/// </exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordGuildEmoji> CreateEmojiAsync(string name, Stream image, IEnumerable<DiscordRole> roles = null, string reason = null)
	{
		if (string.IsNullOrWhiteSpace(name))
			throw new ArgumentNullException(nameof(name));

		name = name.Trim();
		if (name.Length is < 2 or > 50)
			throw new ArgumentException("Emoji name needs to be between 2 and 50 characters long.");

		ArgumentNullException.ThrowIfNull(image);

		var image64 = MediaTool.Base64FromStream(image);

		return this.Discord.ApiClient.CreateGuildEmojiAsync(this.Id, name, image64, roles?.Select(xr => xr.Id), reason);
	}

	/// <summary>
	///     Modifies a this guild's custom emoji.
	/// </summary>
	/// <param name="emoji">Emoji to modify.</param>
	/// <param name="name">New name for the emoji.</param>
	/// <param name="roles">
	///     Roles for which the emoji will be available. This works only if your application is whitelisted as
	///     integration.
	/// </param>
	/// <param name="reason">Reason for audit log.</param>
	/// <returns>The modified emoji.</returns>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the
	///     <see cref="Permissions.ManageGuildExpressions" /> permission.
	/// </exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordGuildEmoji> ModifyEmojiAsync(DiscordGuildEmoji emoji, string name, IEnumerable<DiscordRole> roles = null, string reason = null)
	{
		ArgumentNullException.ThrowIfNull(emoji);

		if (emoji.Guild.Id != this.Id)
			throw new ArgumentException("This emoji does not belong to this guild.");

		if (string.IsNullOrWhiteSpace(name))
			throw new ArgumentNullException(nameof(name));

		name = name.Trim();
		return name.Length is < 2 or > 50
			? throw new ArgumentException("Emoji name needs to be between 2 and 50 characters long.")
			: this.Discord.ApiClient.ModifyGuildEmojiAsync(this.Id, emoji.Id, name, roles?.Select(xr => xr.Id), reason);
	}

	/// <summary>
	///     Deletes this guild's custom emoji.
	/// </summary>
	/// <param name="emoji">Emoji to delete.</param>
	/// <param name="reason">Reason for audit log.</param>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the
	///     <see cref="Permissions.ManageGuildExpressions" /> permission.
	/// </exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task DeleteEmojiAsync(DiscordGuildEmoji emoji, string? reason = null)
		=> emoji == null!
			? throw new ArgumentNullException(nameof(emoji))
			: emoji.Guild.Id != this.Id
				? throw new ArgumentException("This emoji does not belong to this guild.")
				: this.Discord.ApiClient.DeleteGuildEmojiAsync(this.Id, emoji.Id, reason);

	/// <summary>
	///     Gets all of this guild's custom stickers.
	/// </summary>
	/// <returns>All of this guild's custom stickers.</returns>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task<IReadOnlyList<DiscordSticker>> GetStickersAsync()
	{
		var stickers = await this.Discord.ApiClient.GetGuildStickersAsync(this.Id).ConfigureAwait(false);

		foreach (var xstr in stickers)
			this.StickersInternal.AddOrUpdate(xstr.Id, xstr, (id, old) =>
			{
				old.Name = xstr.Name;
				old.Description = xstr.Description;
				old.InternalTags = xstr.InternalTags;
				return old;
			});

		return stickers;
	}

	/// <summary>
	///     Gets a sticker
	/// </summary>
	/// <exception cref="UnauthorizedException">Thrown when the sticker could not be found.</exception>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the
	///     <see cref="Permissions.ManageGuildExpressions" /> permission.
	/// </exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	/// <exception cref="ArgumentException">Sticker does not belong to a guild.</exception>
	public Task<DiscordSticker> GetStickerAsync(ulong stickerId)
		=> this.Discord.ApiClient.GetGuildStickerAsync(this.Id, stickerId);

	/// <summary>
	///     Creates a sticker
	/// </summary>
	/// <param name="name">The name of the sticker.</param>
	/// <param name="description">The optional description of the sticker.</param>
	/// <param name="emoji">The emoji to associate the sticker with.</param>
	/// <param name="format">The file format the sticker is written in.</param>
	/// <param name="file">The sticker.</param>
	/// <param name="reason">Audit log reason</param>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the
	///     <see cref="Permissions.ManageGuildExpressions" /> permission.
	/// </exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordSticker> CreateStickerAsync(string name, string description, DiscordEmoji emoji, Stream file, StickerFormat format, string? reason = null)
	{
		var fileExt = format switch
		{
			StickerFormat.Png => "png",
			StickerFormat.Apng => "png",
			StickerFormat.Gif => "gif",
			StickerFormat.Lottie => "json",
			_ => throw new InvalidOperationException("This format is not supported.")
		};

		var contentType = format switch
		{
			StickerFormat.Png => "image/png",
			StickerFormat.Apng => "image/png",
			StickerFormat.Gif => "image/gif",
			StickerFormat.Lottie => "application/json",
			_ => throw new InvalidOperationException("This format is not supported.")
		};

		return emoji.Id is not 0
			? throw new InvalidOperationException("Only unicode emoji can be used for stickers.")
			: name.Length is < 2 or > 30
				? throw new ArgumentOutOfRangeException(nameof(name), "Sticker name needs to be between 2 and 30 characters long.")
				: description.Length is < 1 or > 100
					? throw new ArgumentOutOfRangeException(nameof(description), "Sticker description needs to be between 1 and 100 characters long.")
					: this.Discord.ApiClient.CreateGuildStickerAsync(this.Id, name, description, emoji.GetDiscordName().Replace(":", ""), new("sticker", file, null, fileExt, contentType), reason);
	}

	/// <summary>
	///     Modifies a sticker
	/// </summary>
	/// <param name="sticker">The id of the sticker to modify</param>
	/// <param name="name">The name of the sticker</param>
	/// <param name="description">The description of the sticker</param>
	/// <param name="emoji">The emoji to associate with this sticker.</param>
	/// <param name="reason">Audit log reason</param>
	/// <returns>A sticker object</returns>
	/// <exception cref="UnauthorizedException">Thrown when the sticker could not be found.</exception>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the
	///     <see cref="Permissions.ManageGuildExpressions" /> permission.
	/// </exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	/// <exception cref="ArgumentException">Sticker does not belong to a guild.</exception>
	public async Task<DiscordSticker> ModifyStickerAsync(ulong sticker, Optional<string> name, Optional<string> description, Optional<DiscordEmoji> emoji, string? reason = null)
	{
		if (!this.StickersInternal.TryGetValue(sticker, out var stickerobj) || stickerobj.Guild.Id != this.Id)
			throw new ArgumentException("This sticker does not belong to this guild.");
		if (name is { HasValue: true, Value.Length: < 2 or > 30 })
			throw new ArgumentException("Sticker name needs to be between 2 and 30 characters long.");
		if (description is { HasValue: true, Value.Length: < 1 or > 100 })
			throw new ArgumentException("Sticker description needs to be between 1 and 100 characters long.");
		if (emoji is { HasValue: true, Value.Id: > 0 })
			throw new ArgumentException("Only unicode emojis can be used with stickers.");

		string? uemoji = null;
		if (emoji.HasValue)
			uemoji = emoji.Value.GetDiscordName().Replace(":", "");

		var usticker = await this.Discord.ApiClient.ModifyGuildStickerAsync(this.Id, sticker, name, description, uemoji, reason).ConfigureAwait(false);

		if (this.StickersInternal.TryGetValue(usticker.Id, out var old))
			this.StickersInternal.TryUpdate(usticker.Id, usticker, old);

		return usticker;
	}

	/// <summary>
	///     Modifies a sticker
	/// </summary>
	/// <param name="sticker">The sticker to modify</param>
	/// <param name="name">The name of the sticker</param>
	/// <param name="description">The description of the sticker</param>
	/// <param name="emoji">The emoji to associate with this sticker.</param>
	/// <param name="reason">Audit log reason</param>
	/// <returns>A sticker object</returns>
	/// <exception cref="UnauthorizedException">Thrown when the sticker could not be found.</exception>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the
	///     <see cref="Permissions.ManageGuildExpressions" /> permission.
	/// </exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	/// <exception cref="ArgumentException">Sticker does not belong to a guild.</exception>
	public Task<DiscordSticker> ModifyStickerAsync(DiscordSticker sticker, Optional<string> name, Optional<string> description, Optional<DiscordEmoji> emoji, string? reason = null)
		=> this.ModifyStickerAsync(sticker.Id, name, description, emoji, reason);

	/// <summary>
	///     Deletes a sticker
	/// </summary>
	/// <param name="sticker">Id of sticker to delete</param>
	/// <param name="reason">Audit log reason</param>
	/// <exception cref="UnauthorizedException">Thrown when the sticker could not be found.</exception>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the
	///     <see cref="Permissions.ManageGuildExpressions" /> permission.
	/// </exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	/// <exception cref="ArgumentException">Sticker does not belong to a guild.</exception>
	public Task DeleteStickerAsync(ulong sticker, string? reason = null) =>
		!this.StickersInternal.TryGetValue(sticker, out var stickerobj)
			? throw new ArgumentNullException(nameof(sticker))
			: stickerobj.Guild.Id != this.Id
				? throw new ArgumentException("This sticker does not belong to this guild.")
				: this.Discord.ApiClient.DeleteGuildStickerAsync(this.Id, sticker, reason);

	/// <summary>
	///     Deletes a sticker
	/// </summary>
	/// <param name="sticker">Sticker to delete</param>
	/// <param name="reason">Audit log reason</param>
	/// <exception cref="UnauthorizedException">Thrown when the sticker could not be found.</exception>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the
	///     <see cref="Permissions.ManageGuildExpressions" /> permission.
	/// </exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	/// <exception cref="ArgumentException">Sticker does not belong to a guild.</exception>
	public Task DeleteStickerAsync(DiscordSticker sticker, string? reason = null)
		=> this.DeleteStickerAsync(sticker.Id, reason);

	/// <summary>
	///     <para>Gets the default channel for this guild.</para>
	///     <para>Default channel is the first channel current member can see.</para>
	/// </summary>
	/// <returns>This member's default guild.</returns>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public DiscordChannel? GetDefaultChannel() =>
		this.ChannelsInternal.Values.Where(xc => xc.Type == ChannelType.Text)
			.OrderBy(xc => xc.Position)
			.FirstOrDefault(xc => (xc.PermissionsFor(this.CurrentMember) & Enums.Permissions.AccessChannels) == Enums.Permissions.AccessChannels);

	/// <summary>
	///     Gets the guild's widget
	/// </summary>
	/// <returns>The guild's widget</returns>
	public Task<DiscordWidget> GetWidgetAsync()
		=> this.Discord.ApiClient.GetGuildWidgetAsync(this.Id);

	/// <summary>
	///     Gets the guild's widget settings
	/// </summary>
	/// <returns>The guild's widget settings</returns>
	public Task<DiscordWidgetSettings> GetWidgetSettingsAsync()
		=> this.Discord.ApiClient.GetGuildWidgetSettingsAsync(this.Id);

	/// <summary>
	///     Modifies the guild's widget settings
	/// </summary>
	/// <param name="isEnabled">If the widget is enabled or not</param>
	/// <param name="channel">Widget channel</param>
	/// <param name="reason">Reason the widget settings were modified</param>
	/// <returns>The newly modified widget settings</returns>
	public Task<DiscordWidgetSettings> ModifyWidgetSettingsAsync(bool? isEnabled = null, DiscordChannel? channel = null, string? reason = null)
		=> this.Discord.ApiClient.ModifyGuildWidgetSettingsAsync(this.Id, isEnabled, channel?.Id, reason);

	/// <summary>
	///     Gets all of this guild's templates.
	/// </summary>
	/// <returns>All of the guild's templates.</returns>
	/// <exception cref="UnauthorizedException">
	///     Throws when the client does not have the <see cref="Permissions.ManageGuild" />
	///     permission.
	/// </exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<IReadOnlyList<DiscordGuildTemplate>> GetTemplatesAsync()
		=> this.Discord.ApiClient.GetGuildTemplatesAsync(this.Id);

	/// <summary>
	///     Creates a guild template.
	/// </summary>
	/// <param name="name">Name of the template.</param>
	/// <param name="description">Description of the template.</param>
	/// <returns>The template created.</returns>
	/// <exception cref="BadRequestException">
	///     Throws when a template already exists for the guild or a null parameter is
	///     provided for the name.
	/// </exception>
	/// <exception cref="UnauthorizedException">
	///     Throws when the client does not have the <see cref="Permissions.ManageGuild" />
	///     permission.
	/// </exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordGuildTemplate> CreateTemplateAsync(string name, string? description = null)
		=> this.Discord.ApiClient.CreateGuildTemplateAsync(this.Id, name, description);

	/// <summary>
	///     Syncs the template to the current guild's state.
	/// </summary>
	/// <param name="code">The code of the template to sync.</param>
	/// <returns>The template synced.</returns>
	/// <exception cref="NotFoundException">Throws when the template for the code cannot be found</exception>
	/// <exception cref="UnauthorizedException">
	///     Throws when the client does not have the <see cref="Permissions.ManageGuild" />
	///     permission.
	/// </exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordGuildTemplate> SyncTemplateAsync(string code)
		=> this.Discord.ApiClient.SyncGuildTemplateAsync(this.Id, code);

	/// <summary>
	///     Modifies the template's metadata.
	/// </summary>
	/// <param name="code">The template's code.</param>
	/// <param name="name">Name of the template.</param>
	/// <param name="description">Description of the template.</param>
	/// <returns>The template modified.</returns>
	/// <exception cref="NotFoundException">Throws when the template for the code cannot be found</exception>
	/// <exception cref="UnauthorizedException">
	///     Throws when the client does not have the <see cref="Permissions.ManageGuild" />
	///     permission.
	/// </exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordGuildTemplate> ModifyTemplateAsync(string code, string? name = null, string? description = null)
		=> this.Discord.ApiClient.ModifyGuildTemplateAsync(this.Id, code, name, description);

	/// <summary>
	///     Deletes the template.
	/// </summary>
	/// <param name="code">The code of the template to delete.</param>
	/// <returns>The deleted template.</returns>
	/// <exception cref="NotFoundException">Throws when the template for the code cannot be found</exception>
	/// <exception cref="UnauthorizedException">
	///     Throws when the client does not have the <see cref="Permissions.ManageGuild" />
	///     permission.
	/// </exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordGuildTemplate> DeleteTemplateAsync(string code)
		=> this.Discord.ApiClient.DeleteGuildTemplateAsync(this.Id, code);

	/// <summary>
	///     Gets this guild's membership screening form.
	/// </summary>
	/// <returns>This guild's membership screening form.</returns>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordGuildMembershipScreening> GetMembershipScreeningFormAsync()
		=> this.Discord.ApiClient.GetGuildMembershipScreeningFormAsync(this.Id);

	/// <summary>
	///     Modifies this guild's membership screening form.
	/// </summary>
	/// <param name="action">Action to perform</param>
	/// <returns>The modified screening form.</returns>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client doesn't have the <see cref="Permissions.ManageGuild" />
	///     permission, or community is not enabled on this guild.
	/// </exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task<DiscordGuildMembershipScreening> ModifyMembershipScreeningFormAsync(Action<MembershipScreeningEditModel> action)
	{
		var mdl = new MembershipScreeningEditModel();
		action(mdl);
		return await this.Discord.ApiClient.ModifyGuildMembershipScreeningFormAsync(this.Id, mdl.Enabled, mdl.Fields, mdl.Description).ConfigureAwait(false);
	}

	/// <summary>
	///     Gets all the application commands in this guild.
	/// </summary>
	/// <returns>A list of application commands in this guild.</returns>
	public Task<IReadOnlyList<DiscordApplicationCommand>> GetApplicationCommandsAsync() =>
		this.Discord.ApiClient.GetGuildApplicationCommandsAsync(this.Discord.CurrentApplication.Id, this.Id);

	/// <summary>
	///     Overwrites the existing application commands in this guild. New commands are automatically created and missing
	///     commands are automatically delete
	/// </summary>
	/// <param name="commands">The list of commands to overwrite with.</param>
	/// <returns>The list of guild commands</returns>
	public Task<IReadOnlyList<DiscordApplicationCommand>> BulkOverwriteApplicationCommandsAsync(IEnumerable<DiscordApplicationCommand> commands) =>
		this.Discord.ApiClient.BulkOverwriteGuildApplicationCommandsAsync(this.Discord.CurrentApplication.Id, this.Id, commands);

	/// <summary>
	///     Creates or overwrites a application command in this guild.
	/// </summary>
	/// <param name="command">The command to create.</param>
	/// <returns>The created command.</returns>
	public Task<DiscordApplicationCommand> CreateApplicationCommandAsync(DiscordApplicationCommand command) =>
		this.Discord.ApiClient.CreateGuildApplicationCommandAsync(this.Discord.CurrentApplication.Id, this.Id, command);

	/// <summary>
	///     Edits a application command in this guild.
	/// </summary>
	/// <param name="commandId">The id of the command to edit.</param>
	/// <param name="action">Action to perform.</param>
	/// <returns>The edit command.</returns>
	public async Task<DiscordApplicationCommand> EditApplicationCommandAsync(ulong commandId, Action<ApplicationCommandEditModel> action)
	{
		var mdl = new ApplicationCommandEditModel();
		action(mdl);
		return await this.Discord.ApiClient.EditGuildApplicationCommandAsync(this.Discord.CurrentApplication.Id, this.Id, commandId, mdl.Name, mdl.Description!, mdl.Options, mdl.NameLocalizations, mdl.DescriptionLocalizations, mdl.DefaultMemberPermissions, mdl.IsNsfw, mdl.AllowedContexts, mdl.IntegrationTypes).ConfigureAwait(false);
	}

	/// <summary>
	///     Gets this guild's welcome screen.
	/// </summary>
	/// <returns>This guild's welcome screen object.</returns>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordGuildWelcomeScreen> GetWelcomeScreenAsync() =>
		this.Discord.ApiClient.GetGuildWelcomeScreenAsync(this.Id);

	/// <summary>
	///     Modifies this guild's welcome screen.
	/// </summary>
	/// <param name="action">Action to perform.</param>
	/// <returns>The modified welcome screen.</returns>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client doesn't have the <see cref="Permissions.ManageGuild" />
	///     permission, or community is not enabled on this guild.
	/// </exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task<DiscordGuildWelcomeScreen> ModifyWelcomeScreenAsync(Action<WelcomeScreenEditModel> action)
	{
		var mdl = new WelcomeScreenEditModel();
		action(mdl);
		return await this.Discord.ApiClient.ModifyGuildWelcomeScreenAsync(this.Id, mdl.Enabled, mdl.WelcomeChannels, mdl.Description).ConfigureAwait(false);
	}

	/// <summary>
	///     Creates a new soundboard sound in the guild.
	/// </summary>
	/// <param name="name">The name of the sound.</param>
	/// <param name="sound">The sound file stream. Can be MP3 or OGG, and must be base64 encoded.</param>
	/// <param name="volume">The volume of the sound. Optional.</param>
	/// <param name="emojiId">The ID of the emoji associated with the sound. Optional.</param>
	/// <param name="emojiName">The name of the emoji associated with the sound. Optional.</param>
	/// <param name="reason">The reason for creating the sound, to be logged in the audit log. Optional.</param>
	/// <returns>The created <see cref="DiscordSoundboardSound" />.</returns>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the <see cref="Permissions.CreateGuildExpressions" /> permission.
	/// </exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task<DiscordSoundboardSound> CreateSoundboardSoundAsync(string name, Stream sound, double? volume = null, ulong? emojiId = null, string? emojiName = null, string? reason = null)
	{
		var sound64 = MediaTool.Base64FromStream(sound);
		return await this.Discord.ApiClient.CreateGuildSoundboardSoundAsync(this.Id, name, sound64, volume, emojiId, emojiName, reason);
	}

	/// <summary>
	///     Modifies an existing soundboard sound.
	/// </summary>
	/// <param name="soundId">The ID of the sound to modify.</param>
	/// <param name="action">The action to configure the soundboard sound edit model.</param>
	/// <returns>The updated <see cref="DiscordSoundboardSound" />.</returns>
	/// <exception cref="NotFoundException">Thrown when the soundboard sound cannot be found.</exception>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the <see cref="Permissions.ManageGuildExpressions" /> permission.
	/// </exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task<DiscordSoundboardSound> ModifySoundboardSoundAsync(ulong soundId, Action<SoundboardSoundEditModel> action)
	{
		var mdl = new SoundboardSoundEditModel();
		action(mdl);

		return await this.Discord.ApiClient.ModifyGuildSoundboardSoundAsync(
			this.Id,
			soundId,
			mdl.Name,
			mdl.Volume,
			mdl.EmojiId,
			mdl.EmojiName
		).ConfigureAwait(false);
	}

	/// <summary>
	///     Deletes a soundboard sound from the guild.
	/// </summary>
	/// <param name="soundId">The ID of the sound to delete.</param>
	/// <param name="reason">The reason for deleting the sound, to be logged in the audit log. Optional.</param>
	/// <returns>A task representing the deletion operation.</returns>
	/// <exception cref="NotFoundException">Thrown when the soundboard sound cannot be found.</exception>
	/// <exception cref="UnauthorizedException">
	///     Thrown when the client does not have the <see cref="Permissions.ManageGuildExpressions" /> permission.
	/// </exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task DeleteSoundboardSoundAsync(ulong soundId, string? reason = null)
		=> this.Discord.ApiClient.DeleteGuildSoundboardSoundAsync(this.Id, soundId, reason);

	/// <summary>
	///     Gets a soundboard sound by its ID.
	/// </summary>
	/// <param name="soundId">The ID of the sound to retrieve.</param>
	/// <returns>The requested <see cref="DiscordSoundboardSound" />.</returns>
	/// <exception cref="NotFoundException">Thrown when the soundboard sound cannot be found.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<DiscordSoundboardSound> GetSoundboardSoundAsync(ulong soundId)
		=> this.Discord.ApiClient.GetGuildSoundboardSoundAsync(this.Id, soundId);

	/// <summary>
	///     Lists all soundboard sounds in the guild.
	/// </summary>
	/// <returns>A collection of <see cref="DiscordSoundboardSound" /> objects representing all soundboard sounds in the guild.</returns>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public Task<IReadOnlyList<DiscordSoundboardSound>> ListSoundboardSoundsAsync()
		=> this.Discord.ApiClient.ListGuildSoundboardSoundsAsync(this.Id);

	/// <summary>
	///     Gets the join requests.
	/// </summary>
	/// <param name="limit">The maximum number of join requests to return. Defaults to 100.</param>
	/// <param name="statusType">The status type to filter join requests by. Can be Submitted, Approved, or Rejected.</param>
	/// <param name="before">Retrieve join requests before this ID.</param>
	/// <param name="after">Retrieve join requests after this ID.</param>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when the status type is not supported.</exception>
	[DiscordUnreleased("This feature is not available for bots at the current time"), Obsolete("This feature is not available for bots at the current time", true)]
	public async Task<DiscordGuildJoinRequestSearchResult> GetJoinRequestsAsync(int limit = 100, JoinRequestStatusType? statusType = null, ulong? before = null, ulong? after = null)
		=> await this.Discord.ApiClient.GetGuildJoinRequestsAsync(this.Id, limit, statusType, before, after);

	/// <summary>
	///     Gets a specific join request.
	/// </summary>
	/// <param name="joinRequestId">The ID of the join request.</param>
	[DiscordUnreleased("This feature is not available for bots at the current time"), Obsolete("This feature is not available for bots at the current time", true)]
	public async Task<DiscordGuildJoinRequest> GetJoinRequestAsync(ulong joinRequestId)
		=> await this.Discord.ApiClient.GetGuildJoinRequestAsync(this.Id, joinRequestId);

	/// <summary>
	///     Modifies a join request.
	/// </summary>
	/// <param name="joinRequestId">The ID of the join request.</param>
	/// <param name="approve">Whether to approve or deny the request.</param>
	/// <param name="rejectionReason">The optional rejection reason.</param>
	[DiscordUnreleased("This feature is not available for bots at the current time"), Obsolete("This feature is not available for bots at the current time", true)]
	public async Task<DiscordGuildJoinRequest> ModifyJoinRequestsAsync(ulong joinRequestId, bool approve, string? rejectionReason)
		=> await this.Discord.ApiClient.ModifyGuildJoinRequestsAsync(this.Id, joinRequestId, approve ? JoinRequestStatusType.Approved : JoinRequestStatusType.Rejected, rejectionReason);

#endregion
}
