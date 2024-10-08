using System;

namespace DisCatSharp.Enums;

/// <summary>
///     Represents a application discovery flag extensions.
/// </summary>
public static class ApplicationDiscoveryEligibilityFlagExtensions
{
	/// <summary>
	///     Calculates whether these application discovery flags contain a specific flag.
	/// </summary>
	/// <param name="baseFlags">The existing flags.</param>
	/// <param name="flag">The flags to search for.</param>
	/// <returns></returns>
	public static bool HasApplicationDiscoveryFlag(this ApplicationDiscoveryEligibilityFlags baseFlags, ApplicationDiscoveryEligibilityFlags flag) => (baseFlags & flag) == flag;
}

/// <summary>
///     Represents the application discoverability eligibility flags.
/// </summary>
[Flags]
public enum ApplicationDiscoveryEligibilityFlags : long
{
	/// <summary>
	///     Application is verified.
	/// </summary>
	Verified = 1 << 0,

	/// <summary>
	///     Application has at least one tag set.
	/// </summary>
	Tag = 1 << 1,

	/// <summary>
	///     Application has a description.
	/// </summary>
	Description = 1 << 2,

	/// <summary>
	///     Applications has a terms of service.
	/// </summary>
	TermsOfService = 1 << 3,

	/// <summary>
	///     Application has a privacy policy.
	/// </summary>
	PrivacyPolicy = 1 << 4,

	/// <summary>
	///     Application has custom install url or install params.
	/// </summary>
	InstallParams = 1 << 5,

	/// <summary>
	///     Application's name is safe for work.
	/// </summary>
	SafeName = 1 << 6,

	/// <summary>
	///     Application's description is safe for work.
	/// </summary>
	SafeDescription = 1 << 7,

	/// <summary>
	///     Application has the message content approved or utilizes application commands.
	/// </summary>
	ApprovedCommandsOrMessageContent = 1 << 8,

	/// <summary>
	///     Application has a support guild set.
	/// </summary>
	SupportGuild = 1 << 9,

	/// <summary>
	///     Application's commands are safe for work.
	/// </summary>
	SafeCommands = 1 << 10,

	/// <summary>
	///     Application's owner has MFA enabled.
	/// </summary>
	Mfa = 1 << 11,

	/// <summary>
	///     Application's directory long description is safe for work.
	/// </summary>
	SafeDirectoryOverview = 1 << 12,

	/// <summary>
	///     Application has at least one supported locale set.
	/// </summary>
	SupportedLocales = 1 << 13,

	/// <summary>
	///     Application's directory short description is safe for work.
	/// </summary>
	SafeShortDescription = 1 << 14,

	/// <summary>
	///     Application's role connections metadata is safe for work.
	/// </summary>
	SafeRoleConnections = 1 << 15,

	/// <summary>
	///     Application has met all criteria and is eligible for discovery.
	/// </summary>
	Eligible = 1 << 16
}
