// This file is part of the DisCatSharp project, based off DSharpPlus.
//
// Copyright (c) 2021-2022 AITSYS
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;

namespace DisCatSharp;

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
	/// Relates to partner/verification applications.
	/// </summary>
	PartnerOrVerificationApplication = 1L << 11,

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
	/// The user has a premium discriminator.
	/// </summary>
	PremiumDiscriminator = 1L << 37,

	/// <summary>
	/// The user has used the desktop client
	/// </summary>
	UsedDesktopClient = 1L << 38,

	/// <summary>
	/// The user has used the web client
	/// </summary>
	UsedWebClient = 1L << 39,

	/// <summary>
	/// The user has used the mobile client
	/// </summary>
	UsedMobileClient = 1L << 40,

	/// <summary>
	/// The user is currently temporarily or permanently disabled.
	/// </summary>
	Disabled = 1L << 42,

	/// <summary>
	/// The user has a verified email.
	/// </summary>
	VerifiedEmail = 1L << 43,

	/// <summary>
	/// The user is currently quarantined.
	/// The user can't start new dms and join servers.
	/// The user has to appeal via https://dis.gd/appeal.
	/// </summary>
	Quarantined = 1L << 44
}
