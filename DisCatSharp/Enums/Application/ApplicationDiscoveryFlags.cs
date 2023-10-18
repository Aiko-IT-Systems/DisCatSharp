using System;

namespace DisCatSharp.Enums;

/// <summary>
/// Represents a application discovery flag extensions.
/// </summary>
public static class ApplicationDiscoveryFlagExtensions
{
	/// <summary>
	/// Calculates whether these application discovery flags contain a specific flag.
	/// </summary>
	/// <param name="baseFlags">The existing flags.</param>
	/// <param name="flag">The flags to search for.</param>
	/// <returns></returns>
	public static bool HasApplicationDiscoveryFlag(this ApplicationDiscoveryFlags baseFlags, ApplicationDiscoveryFlags flag) => (baseFlags & flag) == flag;
}

[Flags]
public enum ApplicationDiscoveryFlags : long
{
	/// <summary>
	/// Application is verified.
	/// </summary>
	IsVerified = 1<<0,

	/// <summary>
	/// Application has at least one tag set.
	/// </summary>
	HasAtLeastOneTag = 1<<1,

	/// <summary>
	/// Application has a description.
	/// </summary>
	HasDescription = 1<<2,

	/// <summary>
	/// Applications has a terms of service.
	/// </summary>
	HasTermsOfService = 1<<3,

	/// <summary>
	/// Application has a privacy policy.
	/// </summary>
	HasPrivacyPolicy = 1<<4,

	/// <summary>
	/// Application has custom install url or install params.
	/// </summary>
	HasCustomInstallUrlOrInstallParams = 1<<5,

	/// <summary>
	/// Application's name is safe for work.
	/// </summary>
	HasSafeName = 1<<6,

	/// <summary>
	/// Application's description is safe for work.
	/// </summary>
	HasSafeDescription = 1<<7,

	/// <summary>
	/// Application has the message content approved or utilizes application commands.
	/// </summary>
	HasApprovedCommandsOrMessageContent = 1<<8,

	/// <summary>
	/// Application has a support guild set.
	/// </summary>
	HasSupportGuild = 1<<9,

	/// <summary>
	/// Application's commands are safe for work.
	/// </summary>
	HasSafeCommands = 1<<10,

	/// <summary>
	/// Application's owner has MFA enabled.
	/// </summary>
	OwnerHasMfa = 1<<11,

	/// <summary>
	/// Application's directory long description is safe for work.
	/// </summary>
	HasSafeDirectoryLongDescription = 1<<12,

	/// <summary>
	/// Application has at least one supported locale set.
	/// </summary>
	HasAtLeastOneSupportedLocale = 1<<13,

	/// <summary>
	/// Application's directory short description is safe for work.
	/// </summary>
	HasSafeDirectoryShortDescription = 1<<14,

	/// <summary>
	/// Application's role connections metadata is safe for work.
	/// </summary>
	HasSafeRoleConnections = 1<<15,

	/// <summary>
	/// Application has met all criteria and is eligible for discovery.
	/// </summary>
	IsEligible = 1<<16
}
