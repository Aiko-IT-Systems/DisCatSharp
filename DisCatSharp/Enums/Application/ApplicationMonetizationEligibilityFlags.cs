using System;

namespace DisCatSharp.Enums;

/// <summary>
///     Represents a application monetization eligibility flags extension.
/// </summary>
public static class ApplicationMonetizationEligibilityFlagExtensions
{
	/// <summary>
	///     Calculates whether these application discovery flags contain a specific flag.
	/// </summary>
	/// <param name="baseFlags">The existing flags.</param>
	/// <param name="flag">The flags to search for.</param>
	/// <returns></returns>
	public static bool HasApplicationMonetizationEligibilityFlag(this ApplicationMonetizationEligibilityFlags baseFlags, ApplicationMonetizationEligibilityFlags flag) => (baseFlags & flag) == flag;
}

/// <summary>
///     Represents the application monetization eligibility flags.
/// </summary>
[Flags]
public enum ApplicationMonetizationEligibilityFlags : long
{
	/// <summary>
	///     This application is verified
	/// </summary>
	Verified = 1L << 0,

	/// <summary>
	///     This application is owned by a team
	/// </summary>
	HasTeam = 1L << 1,

	/// <summary>
	///     This application has the message content intent approved or utilizes application commands
	/// </summary>
	ApprovedCommandsOrMessageContent = 1L << 2,

	/// <summary>
	///     This application has terms of service set
	/// </summary>
	TermsOfService = 1L << 3,

	/// <summary>
	///     This application has a privacy policy set
	/// </summary>
	PrivacyPolicy = 1L << 4,

	/// <summary>
	///     This application's name is safe for work
	/// </summary>
	SafeName = 1L << 5,

	/// <summary>
	///     This application's description is safe for work
	/// </summary>
	SafeDescription = 1L << 6,

	/// <summary>
	///     This application's role connections metadata is safe for work
	/// </summary>
	HasSafeRoleConnections = 1L << 7,

	/// <summary>
	///     The user is the owner of the team that owns the application
	/// </summary>
	UserIsTeamOwner = 1L << 8,

	/// <summary>
	///     This application is not quarantined
	/// </summary>
	NotQuarantined = 1L << 9,

	/// <summary>
	///     The user's locale is supported by monetization
	/// </summary>
	UserLocaleSupported = 1L << 10,

	/// <summary>
	///     The user is old enough to use monetization
	/// </summary>
	UserAgeSupported = 1L << 11,

	/// <summary>
	///     The user has a date of birth defined on their account
	/// </summary>
	UserDateOfBirthDefined = 1L << 12,

	/// <summary>
	///     The user has MFA enabled
	/// </summary>
	UserMfaEnabled = 1L << 13,

	/// <summary>
	///     The user's email is verified
	/// </summary>
	UserEmailVerified = 1L << 14,

	/// <summary>
	///     All members of the team that owns the application have verified emails
	/// </summary>
	TeamMembersEmailVerified = 1L << 15,

	/// <summary>
	///     All members of the team that owns the application have MFA enabled
	/// </summary>
	TeamMembersMfaEnabled = 1L << 16,

	/// <summary>
	///     This application has no issues blocking monetization
	/// </summary>
	NoBlockingIssues = 1L << 17,

	/// <summary>
	///     The team has a valid payout status
	/// </summary>
	ValidPayoutStatus = 1L << 18,

	/// <summary>
	///      The flags are unknown.
	/// </summary>
	Unknown = long.MaxValue
}
