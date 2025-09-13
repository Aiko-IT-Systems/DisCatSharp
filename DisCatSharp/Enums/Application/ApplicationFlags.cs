using System;

using DisCatSharp.Attributes;

namespace DisCatSharp.Enums;

/// <summary>
///     Represents additional details of an application.
/// </summary>
[Flags]
public enum ApplicationFlags : long
{
	/// <summary>
	///     The application has no flags.
	/// </summary>
	None = 0,

	/// <summary>
	///     The application is embedded and can be used by users.
	///     This was introduced to avoid users using in-dev apps.
	/// </summary>
	EmbeddedReleased = 1L << 1,

	/// <summary>
	///     The application is a managed emoji.
	/// </summary>
	ManagedEmoji = 1L << 2,

	/// <summary>
	///     Unknown, relates to in app purchase.
	/// </summary>
	EmbeddedIap = 1L << 3,

	/// <summary>
	///     The application can create group dms.
	/// </summary>
	GroupDmCreate = 1L << 4,

	/// <summary>
	///     Allows the application to access the local RPC server.
	/// </summary>
	RpcPrivateBeta = 1L << 5,

	/// <summary>
	///     The application has created multiple auto moderation rules.
	/// </summary>
	AutoModerationRuleCreateBadge = 1L << 6,

	/// <summary>
	///     The application's game profile is disabled.
	/// </summary>
	GameProfileDisabled = 1L << 7,

	/// <summary>
	///     The application's OAuth2 credentials are public.
	/// </summary>
	PublicOAuth2Client = 1L << 8,

	/// <summary>
	///     The application's activity can be launched without a context.
	/// </summary>
	ContextlessActivity = 1L << 9,

	/// <summary>
	///     The application has limited access to the social layer SDK.
	/// </summary>
	SocialLayerIntegrationLimited = 1L << 10,

	/// <summary>
	///     The application has connected to RPC.
	/// </summary>
	[DiscordDeprecated]
	RpcHasConnected = 1L << 11,

	/// <summary>
	///     The application can track presence data.
	/// </summary>
	GatewayPresence = 1L << 12,

	/// <summary>
	///     The application can track presence data (limited).
	/// </summary>
	GatewayPresenceLimited = 1L << 13,

	/// <summary>
	///     The application can track guild members.
	/// </summary>
	GatewayGuildMembers = 1L << 14,

	/// <summary>
	///     The application can track guild members (limited).
	/// </summary>
	GatewayGuildMembersLimited = 1L << 15,

	/// <summary>
	///     The application can track pending guild member verifications (limited).
	/// </summary>
	VerificationPendingGuildLimit = 1L << 16,

	/// <summary>
	///     The application is embedded.
	/// </summary>
	Embedded = 1L << 17,

	/// <summary>
	///     The application can track message content.
	/// </summary>
	GatewayMessageContent = 1L << 18,

	/// <summary>
	///     The application can track message content (limited).
	/// </summary>
	GatewayMessageContentLimited = 1L << 19,

	/// <summary>
	///     Related to embedded applications.
	/// </summary>
	EmbeddedFirstParty = 1L << 20,

	/// <summary>
	///     The application has been migrated to the new application command system.
	/// </summary>
	ApplicationCommandMigrated = 1L << 21,

	/// <summary>
	///     The application has registered global application commands.
	/// </summary>
	ApplicationCommandBadge = 1L << 23,

	/// <summary>
	///     Indicates if an app is considered active.
	///     This means that it has had any global command executed in the past 30 days.
	/// </summary>
	Active = 1L << 24,

	/// <summary>
	///     Indicates if an application has not had any global application commands used in the last 30 days and has lost the <see cref="Active"/> flag.
	/// </summary>
	AcitiveGracePeriod = 1L << 25,

	/// <summary>
	///     Allows the app to use Iframe modals.
	/// </summary>
	IframeModal = 1L << 26,

	/// <summary>
	///     Indicates if an app can use the social layer SDK.
	/// </summary>
	SocialLayerIntegration = 1L << 27,

	/// <summary>
	///     Indicates if an app is promoted by discord.
	/// </summary>
	Promoted = 1L << 29,

	/// <summary>
	///     Indicates if an app is partnered with discord.
	/// </summary>
	Partner = 1L << 30,

	/// <summary>
	///      The flags are unknown.
	/// </summary>
	Unknown = long.MaxValue
}
