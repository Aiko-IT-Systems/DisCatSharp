// This file is part of the DisCatSharp project, based off DSharpPlus.
//
// Copyright (c) 2021-2023 AITSYS
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

namespace DisCatSharp.Enums;

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
