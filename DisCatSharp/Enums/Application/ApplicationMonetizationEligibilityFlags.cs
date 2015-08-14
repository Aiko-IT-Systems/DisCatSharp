using System;

namespace DisCatSharp.Enums;

/// <summary>
/// Represents a application monetization eligibility flags extension.
/// </summary>
public static class ApplicationMonetizationEligibilityFlagExtensions
{
	/// <summary>
	/// Calculates whether these application discovery flags contain a specific flag.
	/// </summary>
	/// <param name="baseFlags">The existing flags.</param>
	/// <param name="flag">The flags to search for.</param>
	/// <returns></returns>
	public static bool HasApplicationMonetizationEligibilityFlag(this ApplicationMonetizationEligibilityFlags baseFlags, ApplicationMonetizationEligibilityFlags flag) => (baseFlags & flag) == flag;
}

/// <summary>
/// Represents the application monetization eligibility flags.
/// </summary>
[Flags]
public enum ApplicationMonetizationEligibilityFlags : long
{
	/// <summary>
	/// This application is verified
	/// </summary>
	Verified = 1 << 0,

	/// <summary>
	/// This application is owned by a team
	/// </summary>
	HasTeam = 1 << 1,

	/// <summary>
	/// This application has the message content intent approved or utilizes application commands
	/// </summary>
	ApprovedCommandsOrMessageContent = 1 << 2,

	/// <summary>
	/// This application has terms of service set
	/// </summary>
	TermsOfService = 1 << 3,

	/// <summary>
	/// This application has a privacy policy set
	/// </summary>
	PrivacyPolicy = 1 << 4,

	/// <summary>
	/// This application's name is safe for work
	/// </summary>
	SafeName = 1 << 5,

	/// <summary>
	/// This application's description is safe for work
	/// </summary>
	SafeDescription = 1 << 6,

	/// <summary>
	/// This application's role connections metadata is safe for work
	/// </summary>
	HasSafeRoleConnections = 1 << 7,

	/// <summary>
	/// The user is the owner of the team that owns the application
	/// </summary>
	UserIsTeamOwner = 1 << 8,

	/// <summary>
	/// This application is not quarantined
	/// </summary>
	NotQuarantined = 1 << 9,

	/// <summary>
	/// The user's locale is supported by monetization
	/// </summary>
	UserLocaleSupported = 1 << 10,

	/// <summary>
	/// The user is old enough to use monetization
	/// </summary>
	UserAgeSupported = 1 << 11,

	/// <summary>
	/// The user has a date of birth defined on their account
	/// </summary>
	UserDateOfBirthDefined = 1 << 12,

	/// <summary>
	/// The user has MFA enabled
	/// </summary>
	UserMfaEnabled = 1 << 13,

	/// <summary>
	/// The user's email is verified
	/// </summary>
	UserEmailVerified = 1 << 14,

	/// <summary>
	/// All members of the team that owns the application have verified emails
	/// </summary>
	TeamMembersEmailVerified = 1 << 15,

	/// <summary>
	/// All members of the team that owns the application have MFA enabled
	/// </summary>
	TeamMembersMfaEnabled = 1 << 16,

	/// <summary>
	/// This application has no issues blocking monetization
	/// </summary>
	NoBlockingIssues = 1 << 17,

	/// <summary>
	/// The team has a valid payout status
	/// </summary>
	ValidPayoutStatus = 1 << 18
}
