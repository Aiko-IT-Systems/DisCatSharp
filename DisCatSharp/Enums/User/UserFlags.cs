using System;

namespace DisCatSharp.Enums;

/// <summary>
/// Represents additional details of a users account.
/// </summary>
[Flags]
public enum UserFlags : long
{
	/// <summary>
	/// The user has no flags.
	/// </summary>
	None = 0,

	/// <summary>
	/// The user is a Discord employee.
	/// </summary>
	Staff = 1L << 0,

	/// <summary>
	/// The user is a Discord partner.
	/// </summary>
	Partner = 1L << 1,

	/// <summary>
	/// The user has the HypeSquad badge.
	/// </summary>
	HypeSquad = 1L << 2,

	/// <summary>
	/// The user reached the first bug hunter tier.
	/// </summary>
	BugHunterLevelOne = 1L << 3,

	/// <summary>
	/// The user has SMS recovery for 2FA enabled.
	/// </summary>
	MfaSms = 1L << 4,

	/// <summary>
	/// The user is marked as dismissed Nitro promotion
	/// </summary>
	PremiumPromoDismissed = 1L << 5,

	/// <summary>
	/// The user is a member of house bravery.
	/// </summary>
	HouseBravery = 1L << 6,

	/// <summary>
	/// The user is a member of house brilliance.
	/// </summary>
	HouseBrilliance = 1L << 7,

	/// <summary>
	/// The user is a member of house balance.
	/// </summary>
	HouseBalance = 1L << 8,

	/// <summary>
	/// The user has the early supporter badge.
	/// </summary>
	PremiumEarlySupporter = 1L << 9,

	/// <summary>
	/// User is a <see cref="Entities.DiscordTeam"/>.
	/// </summary>
	TeamPseudoUser = 1L << 10,

	/// <summary>
	/// User previously requested verification and/or partnership for a guild.
	/// </summary>
	InternalApplication = 1L << 11,

	/// <summary>
	/// Whether the user is an official system user.
	/// </summary>
	System = 1L << 12,

	/// <summary>
	/// Whether the user has unread system messages.
	/// </summary>
	HasUnreadUrgentMessages = 1L << 13,

	/// <summary>
	/// The user reached the second bug hunter tier.
	/// </summary>
	BugHunterLevelTwo = 1L << 14,

	/// <summary>
	/// The user has a pending deletion for being underage in DOB prompt.
	/// </summary>
	UnderageDeleted = 1L << 15,

	/// <summary>
	/// The user is a verified bot.
	/// </summary>
	VerifiedBot = 1L << 16,

	/// <summary>
	/// The user is a verified bot developer.
	/// </summary>
	VerifiedDeveloper = 1L << 17,

	/// <summary>
	/// The user is a discord certified moderator.
	/// </summary>
	CertifiedModerator = 1L << 18,

	/// <summary>
	/// The user is a bot and has set an interactions endpoint url.
	/// </summary>
	BotHttpInteractions = 1L << 19,

	/// <summary>
	/// The user is disabled for being a spammer.
	/// </summary>
	Spammer = 1L << 20,

	/// <summary>
	/// Nitro is disabled for user.
	/// Used by discord staff instead of forcedNonPremium.
	/// </summary>
	DisablePremium = 1L << 21,

	/// <summary>
	/// User is an active developer.
	/// Read more here: https://support-dev.discord.com/hc/articles/10113997751447.
	/// </summary>
	ActiveDeveloper = 1L << 22,

	/// <summary>
	/// Account has a high global ratelimit.
	/// </summary>
	HighGlobalRateLimit = 1L << 33,

	/// <summary>
	/// Account has been deleted.
	/// </summary>
	Deleted = 1L << 34,

	/// <summary>
	/// Account has been disabled for suspicious activity.
	/// </summary>
	DisabledSuspiciousActivity = 1L << 35,

	/// <summary>
	/// Account was deleted by the user.
	/// </summary>
	SelfDeleted = 1L << 36,

	/// <summary>
	/// The user has a premium discriminator.
	/// </summary>
	PremiumDiscriminator = 1L << 37,

	/// <summary>
	/// The user has used the desktop client.
	/// </summary>
	UsedDesktopClient = 1L << 38,

	/// <summary>
	/// The user has used the web client.
	/// </summary>
	UsedWebClient = 1L << 39,

	/// <summary>
	/// The user has used the mobile client.
	/// </summary>
	UsedMobileClient = 1L << 40,

	/// <summary>
	/// The user is currently temporarily or permanently disabled.
	/// </summary>
	Disabled = 1L << 41,

	/// <summary>
	/// The user has a verified email.
	/// </summary>
	VerifiedEmail = 1L << 43,

	/// <summary>
	/// The user is currently quarantined.
	/// The user can't start new dms and join servers.
	/// The user has to appeal via https://dis.gd/appeal.
	/// </summary>
	Quarantined = 1L << 44,

	/// <summary>
	/// User is a collaborator and has staff permissions.
	/// </summary>
	Collaborator = 1L << 50,

	/// <summary>
	/// User is a restricted collaborator and has staff permissions.
	/// </summary>
	RestrictedCollaborator = 1L << 51
}
