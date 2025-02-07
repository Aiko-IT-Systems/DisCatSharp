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
	///     Unknown.
	/// </summary>
	AutoModerationRuleCreateBadge = 1L << 6,

	GameProfileDisabled = 1L << 7,

	/// <summary>
	///     Allows the application to create activity assets.
	/// </summary>
	AllowAssets = 1L << 8,

	/// <summary>
	///     Indicates the the application is an contextless activity.
	/// </summary>
	ContextlessActivity = 1L << 9,

	/// <summary>
	///     Allows the application to enable activity spectating.
	/// </summary>
	[DiscordDeprecated("Replaced by ContextlessActivity")]
	AllowActivityActionSpectate = ContextlessActivity,

	/// <summary>
	///     Allows the application to enable join requests for activities.
	/// </summary>
	AllowActivityActionJoinRequest = 1L << 10,

	/// <summary>
	///     The application has connected to RPC.
	/// </summary>
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
	///     To be datamined.
	/// </summary>
	UnknownFlag = 1L << 21,

	/// <summary>
	///     The application has registered global application commands.
	/// </summary>
	ApplicationCommandBadge = 1L << 23,

	/// <summary>
	///     Indicates if an app is considered active. This means that it has had any global command executed in the past 30
	///     days.
	/// </summary>
	Active = 1L << 24,

	/// <summary>
	///     Allows the app to use Iframe modals.
	/// </summary>
	IframeModal = 1L << 26,

	/// <summary>
	///     Indicates if an app is a social layer integration.
	///     days.
	/// </summary>
	SocialLayerIntegration = 1L << 27,

	/// <summary>
	///     Indicates if an app is promoted by discord.
	///     days.
	/// </summary>
	Promoted = 1L << 29,

	/// <summary>
	///     Indicates if an app is partnered with discord.
	///     days.
	/// </summary>
	Partner = 1L << 30
}
