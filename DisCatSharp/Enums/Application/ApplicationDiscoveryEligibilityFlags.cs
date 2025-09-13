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
	Verified = 1L << 0,

	/// <summary>
	///     Application has at least one tag set.
	/// </summary>
	Tag = 1L << 1,

	/// <summary>
	///     Application has a description.
	/// </summary>
	Description = 1L << 2,

	/// <summary>
	///     Applications has a terms of service.
	/// </summary>
	TermsOfService = 1L << 3,

	/// <summary>
	///     Application has a privacy policy.
	/// </summary>
	PrivacyPolicy = 1L << 4,

	/// <summary>
	///     Application has custom install url or install params.
	/// </summary>
	InstallParams = 1L << 5,

	/// <summary>
	///     Application's name is safe for work.
	/// </summary>
	SafeName = 1L << 6,

	/// <summary>
	///     Application's description is safe for work.
	/// </summary>
	SafeDescription = 1L << 7,

	/// <summary>
	///     Application has the message content approved or utilizes application commands.
	/// </summary>
	ApprovedCommandsOrMessageContent = 1L << 8,

	/// <summary>
	///     Application has a support guild set.
	/// </summary>
	SupportGuild = 1L << 9,

	/// <summary>
	///     Application's commands are safe for work.
	/// </summary>
	SafeCommands = 1L << 10,

	/// <summary>
	///     Application's owner has MFA enabled.
	/// </summary>
	Mfa = 1L << 11,

	/// <summary>
	///     Application's directory long description is safe for work.
	/// </summary>
	SafeDirectoryOverview = 1L << 12,

	/// <summary>
	///     Application has at least one supported locale set.
	/// </summary>
	SupportedLocales = 1L << 13,

	/// <summary>
	///     Application's directory short description is safe for work.
	/// </summary>
	SafeShortDescription = 1L << 14,

	/// <summary>
	///     Application's role connections metadata is safe for work.
	/// </summary>
	SafeRoleConnections = 1L << 15,

	/// <summary>
	///     Application has met all criteria and is eligible for discovery.
	/// </summary>
	Eligible = 1L << 16,

	/// <summary>
	///      The flags are unknown.
	/// </summary>
	Unknown = long.MaxValue
}
