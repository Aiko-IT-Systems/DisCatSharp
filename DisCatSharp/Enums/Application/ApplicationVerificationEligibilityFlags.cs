using System;

namespace DisCatSharp.Enums;

/// <summary>
///     Represents an extension for checking application verification eligibility flags.
/// </summary>
public static class ApplicationVerificationEligibilityFlagExtensions
{
	/// <summary>
	///     Calculates whether these verification eligibility flags contain a specific flag.
	/// </summary>
	/// <param name="baseFlags">The existing flags.</param>
	/// <param name="flag">The flags to search for.</param>
	/// <returns></returns>
	public static bool HasVerificationEligibilityFlag(this ApplicationVerificationEligibilityFlags baseFlags, ApplicationVerificationEligibilityFlags flag) => (baseFlags & flag) == flag;
}

/// <summary>
///     Represents the verification eligibility flags.
/// </summary>
[Flags]
public enum ApplicationVerificationEligibilityFlags : long
{
	/// <summary>
	///     The application has a team.
	/// </summary>
	HasTeam = 1 << 1,

	/// <summary>
	///     The application's name is considered safe.
	/// </summary>
	SafeApplicationName = 1 << 2,

	/// <summary>
	///     The application's description is considered safe.
	/// </summary>
	SafeApplicationDescription = 1 << 3,

	/// <summary>
	///     The application's role connections metadata is considered safe.
	/// </summary>
	SafeApplicationRoleConnectionsMetadata = 1 << 4,

	/// <summary>
	///     The application has a terms of service URL set.
	/// </summary>
	TermsOfService = 1 << 5,

	/// <summary>
	///     The application has a privacy policy URL set.
	/// </summary>
	PrivacyPolicy = 1 << 6,

	/// <summary>
	///     The application has an invite link set.
	/// </summary>
	InviteLink = 1 << 7,

	/// <summary>
	///     The application is not embedded.
	/// </summary>
	NotEmbedded = 1 << 8,

	/// <summary>
	///     The application has no unapproved intents.
	/// </summary>
	NoUnapprovedIntents = 1 << 9,

	/// <summary>
	///     The user is the owner of the team that owns the application.
	/// </summary>
	UserIsTeamOwner = 1 << 10,

	/// <summary>
	///     The team owner is verified.
	/// </summary>
	TeamOwnerIsVerified = 1 << 11,

	/// <summary>
	///     The user has two-factor authentication enabled.
	/// </summary>
	User2FaEnabled = 1 << 12,

	/// <summary>
	///     The user's email is verified.
	/// </summary>
	UserEmailVerified = 1 << 13,

	/// <summary>
	///     All team members' emails are verified.
	/// </summary>
	TeamMembersEmailVerified = 1 << 14,

	/// <summary>
	///     All team members have two-factor authentication enabled.
	/// </summary>
	TeamMembers2FaEnabled = 1 << 15,

	/// <summary>
	///     The application has no blocking issues.
	/// </summary>
	NoBlockingIssues = 1 << 16,

	/// <summary>
	///     The application is eligible for verification.
	/// </summary>
	EligibleForVerification = HasTeam | SafeApplicationName | SafeApplicationDescription | SafeApplicationRoleConnectionsMetadata |
	                          TermsOfService | PrivacyPolicy | InviteLink | NotEmbedded | NoUnapprovedIntents | UserIsTeamOwner |
	                          TeamOwnerIsVerified | User2FaEnabled | UserEmailVerified | TeamMembersEmailVerified | TeamMembers2FaEnabled |
	                          NoBlockingIssues
}
