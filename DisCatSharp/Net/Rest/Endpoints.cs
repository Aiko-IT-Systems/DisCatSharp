using DisCatSharp.Attributes;

namespace DisCatSharp.Net;

/// <summary>
///     The discord endpoints.
/// </summary>
public static class Endpoints
{
	/// <summary>
	///     The base discord api uri.
	/// </summary>
	public const string BASE_URI = "https://discord.com/api/v";

	/// <summary>
	///     The base discord canary api uri.
	/// </summary>
	public const string CANARY_URI = "https://canary.discord.com/api/v";

	/// <summary>
	///     The base discord ptb api uri.
	/// </summary>
	public const string PTB_URI = "https://ptb.discord.com/api/v";

	/// <summary>
	///     The base discord ptb api uri.
	/// </summary>
	public const string STAGING_URI = "https://staging.discord.co/api/v";

	/// <summary>
	///     The oauth2 endpoint.
	/// </summary>
	public const string OAUTH2 = "/oauth2";

	/// <summary>
	///     The oauth2 token endpoint.
	/// </summary>
	public const string TOKEN = "/token";

	/// <summary>
	///     The oauth2 revoke endpoint.
	/// </summary>
	public const string REVOKE = "/revoke";

	/// <summary>
	///     The oauth2 authorize endpoint.
	/// </summary>
	public const string AUTHORIZE = "/authorize";

	/// <summary>
	///     The applications endpoint.
	/// </summary>
	public const string APPLICATIONS = "/applications";

	/// <summary>
	///     The message reactions endpoint.
	/// </summary>
	public const string REACTIONS = "/reactions";

	/// <summary>
	///     The self (@me) endpoint.
	/// </summary>
	public const string ME = "/@me";

	/// <summary>
	///     The @original endpoint.
	/// </summary>
	public const string ORIGINAL = "@original";

	/// <summary>
	///     The permissions endpoint.
	/// </summary>
	public const string PERMISSIONS = "/permissions";

	/// <summary>
	///     The recipients endpoint.
	/// </summary>
	public const string RECIPIENTS = "/recipients";

	/// <summary>
	///     The bulk-delete endpoint.
	/// </summary>
	public const string BULK_DELETE = "/bulk-delete";

	/// <summary>
	///     The integrations endpoint.
	/// </summary>
	public const string INTEGRATIONS = "/integrations";

	/// <summary>
	///     The sync endpoint.
	/// </summary>
	public const string SYNC = "/sync";

	/// <summary>
	///     The prune endpoint.
	///     Used for user removal.
	/// </summary>
	public const string PRUNE = "/prune";

	/// <summary>
	///     The regions endpoint.
	/// </summary>
	public const string REGIONS = "/regions";

	/// <summary>
	///     The connections endpoint.
	/// </summary>
	public const string CONNECTIONS = "/connections";

	/// <summary>
	///     The icons endpoint.
	/// </summary>
	public const string ICONS = "/icons";

	/// <summary>
	///     The gateway endpoint.
	/// </summary>
	public const string GATEWAY = "/gateway";

	/// <summary>
	///     The oauth2 auth endpoint.
	/// </summary>
	public const string AUTH = "/auth";

	/// <summary>
	///     The oauth2 login endpoint.
	/// </summary>
	public const string LOGIN = "/login";

	/// <summary>
	///     The channels endpoint.
	/// </summary>
	public const string CHANNELS = "/channels";

	/// <summary>
	///     The messages endpoint.
	/// </summary>
	public const string MESSAGES = "/messages";

	/// <summary>
	///     The pinned messages endpoint.
	/// </summary>
	public const string PINS = "/pins";

	/// <summary>
	///     The users endpoint.
	/// </summary>
	public const string USERS = "/users";

	/// <summary>
	///     The guilds endpoint.
	/// </summary>
	public const string GUILDS = "/guilds";

	/// <summary>
	///     The guild discovery splash endpoint.
	/// </summary>
	public const string GUILD_DISCOVERY_SPLASHES = "/discovery-splashes";

	/// <summary>
	///     The guild splash endpoint.
	/// </summary>
	public const string SPLASHES = "/splashes";

	/// <summary>
	///     The search endpoint.
	/// </summary>
	public const string SEARCH = "/search";

	/// <summary>
	///     The invites endpoint.
	/// </summary>
	public const string INVITES = "/invites";

	/// <summary>
	///     The roles endpoint.
	/// </summary>
	public const string ROLES = "/roles";

	/// <summary>
	///     The members endpoint.
	/// </summary>
	public const string MEMBERS = "/members";

	/// <summary>
	///     The member endpoint.
	/// </summary>
	public const string MEMBER = "/member";

	/// <summary>
	///     The typing endpoint.
	///     Triggers a typing indicator inside a channel.
	/// </summary>
	public const string TYPING = "/typing";

	/// <summary>
	///     The avatars endpoint.
	/// </summary>
	public const string AVATARS = "/avatars";

	/// <summary>
	///     The avatar decorations endpoint.
	/// </summary>
	public const string AVATARS_DECORATION_PRESETS = "/avatar-decoration-presets";

	/// <summary>
	///     The collectibles endpoint.
	/// </summary>
	public const string COLLECTIBLES = "collectibles";

	/// <summary>
	///     The bans endpoint.
	/// </summary>
	public const string BANS = "/bans";

	/// <summary>
	///     The bulk ban endpoint.
	/// </summary>
	public const string BULK_BAN = "/bulk-ban";

	/// <summary>
	///     The webhook endpoint.
	/// </summary>
	public const string WEBHOOKS = "/webhooks";

	/// <summary>
	///     The slack endpoint.
	///     Used for <see cref="Entities.DiscordWebhook" />.
	/// </summary>
	public const string SLACK = "/slack";

	/// <summary>
	///     The github endpoint.
	///     Used for <see cref="Entities.DiscordWebhook" />.
	/// </summary>
	public const string GITHUB = "/github";

	/// <summary>
	///     The guilds mfa endpoint.
	/// </summary>
	public const string MFA = "/mfa";

	/// <summary>
	///     The bot endpoint.
	/// </summary>
	public const string BOT = "/bot";

	/// <summary>
	///     The voice endpoint.
	/// </summary>
	public const string VOICE = "/voice";

	/// <summary>
	///     The audit logs endpoint.
	/// </summary>
	public const string AUDIT_LOGS = "/audit-logs";

	/// <summary>
	///     The acknowledge endpoint.
	///     Indicates that a message is read.
	/// </summary>
	public const string ACK = "/ack";

	/// <summary>
	///     The nickname endpoint.
	/// </summary>
	public const string NICK = "/nick";

	/// <summary>
	///     The assets endpoint.
	/// </summary>
	public const string ASSETS = "/assets";

	/// <summary>
	///     The embed endpoint.
	/// </summary>
	public const string EMBED = "/embed";

	/// <summary>
	///     The emojis endpoint.
	/// </summary>
	public const string EMOJIS = "/emojis";

	/// <summary>
	///     The vanity url endpoint.
	/// </summary>
	public const string VANITY_URL = "/vanity-url";

	/// <summary>
	///     The guild preview endpoint.
	/// </summary>
	public const string PREVIEW = "/preview";

	/// <summary>
	///     The followers endpoint.
	/// </summary>
	public const string FOLLOWERS = "/followers";

	/// <summary>
	///     The crosspost endpoint.
	/// </summary>
	public const string CROSSPOST = "/crosspost";

	/// <summary>
	///     The guild widget endpoint.
	/// </summary>
	public const string WIDGET = "/widget";

	/// <summary>
	///     The guild widget json endpoint.
	/// </summary>
	public const string WIDGET_JSON = "/widget.json";

	/// <summary>
	///     The guild widget png endpoint.
	/// </summary>
	public const string WIDGET_PNG = "/widget.png";

	/// <summary>
	///     The templates endpoint.
	/// </summary>
	public const string TEMPLATES = "/templates";

	/// <summary>
	///     The member verification gate endpoint.
	/// </summary>
	public const string MEMBER_VERIFICATION = "/member-verification";

	/// <summary>
	///     The slash commands endpoint.
	/// </summary>
	public const string COMMANDS = "/commands";

	/// <summary>
	///     The interactions endpoint.
	/// </summary>
	public const string INTERACTIONS = "/interactions";

	/// <summary>
	///     The interaction/command callback endpoint.
	/// </summary>
	public const string CALLBACK = "/callback";

	/// <summary>
	///     The welcome screen endpoint.
	/// </summary>
	public const string WELCOME_SCREEN = "/welcome-screen";

	/// <summary>
	///     The voice states endpoint.
	/// </summary>
	public const string VOICE_STATES = "/voice-states";

	/// <summary>
	///     The stage instances endpoint.
	/// </summary>
	public const string STAGE_INSTANCES = "/stage-instances";

	/// <summary>
	///     The threads endpoint.
	/// </summary>
	public const string THREADS = "/threads";

	/// <summary>
	///     The public threads endpoint.
	/// </summary>
	public const string THREAD_PUBLIC = "/public";

	/// <summary>
	///     The private threads endpoint.
	/// </summary>
	public const string THREAD_PRIVATE = "/private";

	/// <summary>
	///     The active threads endpoint.
	/// </summary>
	public const string THREAD_ACTIVE = "/active";

	/// <summary>
	///     The archived threads endpoint.
	/// </summary>
	public const string THREAD_ARCHIVED = "/archived";

	/// <summary>
	///     The thread members endpoint.
	/// </summary>
	public const string THREAD_MEMBERS = "/thread-members";

	/// <summary>
	///     The tags endpoint.
	/// </summary>
	public const string TAGS = "/tags";

	/// <summary>
	///     The guild scheduled events endpoint.
	/// </summary>
	public const string SCHEDULED_EVENTS = "/scheduled-events";

	/// <summary>
	///     The guild scheduled events cover image endpoint.
	/// </summary>
	public const string GUILD_EVENTS = "guild-events";

	/// <summary>
	///     The stickers endpoint.
	/// </summary>
	public const string STICKERS = "/stickers";

	/// <summary>
	///     The sticker packs endpoint.
	///     Global nitro sticker packs.
	/// </summary>
	public const string STICKERPACKS = "/sticker-packs";

	/// <summary>
	///     The store endpoint.
	/// </summary>
	public const string STORE = "/store";

	/// <summary>
	///     The entitlements endpoint.
	/// </summary>
	public const string ENTITLEMENTS = "/entitlements";

	/// <summary>
	///     The subscriptions endpoint.
	/// </summary>
	public const string SUBSCRIPTIONS = "/subscriptions";

	/// <summary>
	///     The app assets endpoint.
	/// </summary>
	public const string APP_ASSETS = "/app-assets";

	/// <summary>
	///     The app icons endpoint.
	/// </summary>
	public const string APP_ICONS = "/app-icons";

	/// <summary>
	///     The team icons endpoint.
	/// </summary>
	public const string TEAM_ICONS = "/team-icons";

	/// <summary>
	///     The channel icons endpoint.
	/// </summary>
	public const string CHANNEL_ICONS = "/channel-icons";

	/// <summary>
	///     The banners endpoint.
	/// </summary>
	public const string BANNERS = "/banners";

	/// <summary>
	///     The custom banners endpoint.
	/// </summary>
	public const string CUSTOM_BANNERS = "/custom-banners";

	/// <summary>
	///     The sticker endpoint.
	///     This endpoint is the static nitro sticker application.
	/// </summary>
	public const string STICKER_APPLICATION = "/710982414301790216";

	/// <summary>
	///     The role subscription endpoint.
	/// </summary>
	public const string ROLE_SUBSCRIPTIONS = "/role-subscriptions";

	/// <summary>
	///     The group listings endpoint.
	/// </summary>
	public const string GROUP_LISTINGS = "/group-listings";

	/// <summary>
	///     The subscription listings endpoint.
	/// </summary>
	public const string SUBSCRIPTION_LISTINGS = "/subscription-listings";

	/// <summary>
	///     The directory entries endpoint.
	/// </summary>
	public const string DIRECTORY_ENTRIES = "/directory-entries";

	/// <summary>
	///     The counts endpoint.
	/// </summary>
	public const string COUNTS = "/counts";

	/// <summary>
	///     The list endpoint.
	/// </summary>
	public const string LIST = "/list";

	/// <summary>
	///     The role icons endpoint.
	/// </summary>
	public const string ROLE_ICONS = "/role-icons";

	/// <summary>
	///     The activities endpoint.
	/// </summary>
	public const string ACTIVITIES = "/activities";

	/// <summary>
	///     The config endpoint.
	/// </summary>
	public const string CONFIG = "/config";

	/// <summary>
	///     The ephemeral attachments endpoint.
	/// </summary>
	public const string EPHEMERAL_ATTACHMENTS = "/ephemeral-attachments";

	/// <summary>
	///     The attachments endpoint.
	/// </summary>
	public const string ATTACHMENTS = "/attachments";

	/// <summary>
	///     The rpc endpoint.
	/// </summary>
	public const string RPC = "/rpc";

	/// <summary>
	///     The role connections endpoint.
	/// </summary>
	public const string ROLE_CONNECTIONS = "/role-connections";

	/// <summary>
	///     The role connection endpoint.
	/// </summary>
	public const string ROLE_CONNECTION = "/role-connection";

	/// <summary>
	///     The metadata endpoint.
	/// </summary>
	public const string METADATA = "/metadata";

	/// <summary>
	///     The voice status endpoint.
	/// </summary>
	public const string VOICE_STATUS = "/voice-status";

	/// <summary>
	///     The incident actions endpoint.
	/// </summary>
	public const string INCIDENT_ACTIONS = "/incident-actions";

	/// <summary>
	///     The new member welcome endpoint.
	/// </summary>
	public const string NEW_MEMBER_WELCOME = "/new-member-welcome";

	/// <summary>
	///     The new member actions endpoint.
	/// </summary>
	[DiscordDeprecated]
	public const string NEW_MEMBER_ACTIONS = "/new-member-actions";

	/// <summary>
	///     The new member action endpoint.
	/// </summary>
	[DiscordDeprecated]
	public const string NEW_MEMBER_ACTION = "/new-member-action";

	/// <summary>
	///     The application directory endpoint.
	/// </summary>
	public const string APPLICATION_DIRECTORY = "/application-directory";

	/// <summary>
	///     The shared canvas endpoint.
	/// </summary>
	[DiscordUnreleased]
	public const string SHARED_CANVAS = "/shared-canvas";

	/// <summary>
	///     The lines endpoint.
	/// </summary>
	[DiscordUnreleased]
	public const string LINES = "/lines";

	/// <summary>
	///     The emoji hose endpoint.
	/// </summary>
	[DiscordUnreleased]
	public const string EMOJI_HOSE = "/emoji-hose";

	/// <summary>
	///     The pincode endpoint.
	/// </summary>
	public const string PINCODE = "/pincode";

	/// <summary>
	///     The soundboard default sounds endpoint.
	/// </summary>
	public const string SOUNDBOARD_DEFAULT_SOUNDS = "/soundboard-default-sounds";

	/// <summary>
	///     The soundboard sounds endpoint.
	/// </summary>
	public const string SOUNDBOARD_SOUNDS = "/soundboard-sounds";

	/// <summary>
	///     The changelogs endpoint.
	/// </summary>
	public const string CHANGELOGS = "/changelogs";

	/// <summary>
	///     The changelogs desktop JSON endpoint.
	/// </summary>
	public const string CHANGELOGS_DESKTOP_JSON = "/config_0.json";

	/// <summary>
	///     The changelogs mobile JSON endpoint.
	/// </summary>
	public const string CHANGELOGS_MOBILE_JSON = "/config_1.json";

	/// <summary>
	///     The ownership transfer endpoint.
	/// </summary>
	public const string OWNERSHIP_TRANSFER = "/ownership-transfer";

	/// <summary>
	///     The onboarding endpoint.
	/// </summary>
	public const string ONBOARDING = "/onboarding";

	/// <summary>
	///     The creator monetization endpoint.
	/// </summary>
	public const string CREATOR_MONETIZATION = "/creator-monetization";

	/// <summary>
	///     The auto moderation endpoint.
	/// </summary>
	public const string AUTO_MODERATION = "/auto-moderation";

	/// <summary>
	///     The clear mention raid endpoint.
	/// </summary>
	public const string CLEAR_MENTION_RAID = "/clear-mention-raid";

	/// <summary>
	///     The published listings endpoint.
	/// </summary>
	public const string PUBLISHED_LISTINGS = "/published-listings";

	/// <summary>
	///     The SKUs endpoint.
	/// </summary>
	public const string SKUS = "/skus";

	/// <summary>
	///     The guild home headers endpoint.
	/// </summary>
	[DiscordDeprecated]
	public const string GUILD_HOME_HEADERS = "/home-headers";

	/// <summary>
	///     The alert action endpoint.
	/// </summary>
	public const string ALERT_ACTION = "/alert-action";

	/// <summary>
	///     The consent endpoint.
	/// </summary>
	public const string CONSENT = "/consent";

	/// <summary>
	///     The settings endpoint.
	/// </summary>
	public const string SETTINGS = "/settings";

	/// <summary>
	///     The inventory endpoint.
	/// </summary>
	[DiscordDeprecated]
	public const string INVENTORY = "/inventory";

	/// <summary>
	///     The Clyde settings endpoint.
	/// </summary>
	[DiscordDeprecated]
	public const string CLYDE_SETTINGS = "/clyde-settings";

	/// <summary>
	///     The Clyde profiles endpoint.
	/// </summary>
	[DiscordDeprecated]
	public const string CLYDE_PROFILES = "/clyde-profiles";

	/// <summary>
	///     The generate personality endpoint.
	/// </summary>
	[DiscordDeprecated]
	public const string GENERATE_PERSONALITY = "/generate-personality";

	/// <summary>
	///     The refresh URLs endpoint.
	/// </summary>
	public const string REFRESH_URLS = "/refresh-urls";

	/// <summary>
	///     The polls endpoint.
	/// </summary>
	public const string POLLS = "/polls";

	/// <summary>
	///     The answers endpoint.
	/// </summary>
	public const string ANSWERS = "/answers";

	/// <summary>
	///     The expire endpoint.
	/// </summary>
	public const string EXPIRE = "/expire";

	/// <summary>
	///     The clan badges endpoint.
	/// </summary>
	public const string CLAN_BADGES = "/clan-badges";

	/// <summary>
	///     The clan banners endpoint.
	/// </summary>
	public const string CLAN_BANNERS = "/clan-banners";

	/// <summary>
	///     The members search endpoint.
	/// </summary>
	public const string MEMBERS_SEARCH = "/members-search";

	/// <summary>
	///     The send soundboard sound endpoint.
	/// </summary>
	public const string SEND_SOUNDBOARD_SOUND = "/send-soundboard-sound";

	/// <summary>
	///     The requests endpoint.
	/// </summary>
	public const string REQUESTS = "/requests";

	/// <summary>
	///     The clans endpoint.
	/// </summary>
	public const string CLANS = "/clans";

	/// <summary>
	///     The consume endpoint.
	/// </summary>
	public const string CONSUME = "/consume";

	public const string BADGES = "/badges";

	public const string MEMBER_COUNTS = "/member-counts";
}
