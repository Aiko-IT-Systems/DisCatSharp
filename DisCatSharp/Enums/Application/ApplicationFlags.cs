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

using System;

namespace DisCatSharp.Enums;

/// <summary>
/// Represents additional details of an application.
/// </summary>
[Flags]
public enum ApplicationFlags : long
{
	/// <summary>
	/// The application is embedded and can be used by users.
	/// This was introduced to avoid users using in-dev apps.
	/// </summary>
	EmbeddedReleased = 1L << 1,

	/// <summary>
	/// The application is a managed emoji.
	/// </summary>
	ManagedEmoji = 1L << 2,

	/// <summary>
	/// Unknown, relates to in app purchase.
	/// </summary>
	EmbeddedIap = 1L << 3,

	/// <summary>
	/// The application can create group dms.
	/// </summary>
	GroupDmCreate = 1L << 4,

	/// <summary>
	/// Allows the application to access the local RPC server.
	/// </summary>
	RpcPrivateBeta = 1L << 6,

	/// <summary>
	/// Allows the application to create activity assets.
	/// </summary>
	AllowAssets = 1L<<8,

	/// <summary>
	/// Allows the application to enable activity spectating.
	/// </summary>
	AllowActivityActionSpectate = 1L<<9,

	/// <summary>
	/// Allows the application to enable join requests for activities.
	/// </summary>
	AllowActivityActionJoinRequest = 1L<<10,

	/// <summary>
	/// The application has connected to RPC.
	/// </summary>
	RpcHasConnected = 1L << 11,

	/// <summary>
	/// The application can track presence data.
	/// </summary>
	GatewayPresence = 1L << 12,

	/// <summary>
	/// The application can track presence data (limited).
	/// </summary>
	GatewayPresenceLimited = 1L << 13,

	/// <summary>
	/// The application can track guild members.
	/// </summary>
	GatewayGuildMembers = 1L << 14,

	/// <summary>
	/// The application can track guild members (limited).
	/// </summary>
	GatewayGuildMembersLimited = 1L << 15,

	/// <summary>
	/// The application can track pending guild member verifications (limited).
	/// </summary>
	VerificationPendingGuildLimit = 1L << 16,

	/// <summary>
	/// The application is embedded.
	/// </summary>
	Embedded = 1L << 17,

	/// <summary>
	/// The application can track message content.
	/// </summary>
	GatewayMessageContent = 1L << 18,

	/// <summary>
	/// The application can track message content (limited).
	/// </summary>
	GatewayMessageContentLimited = 1L << 19,

	/// <summary>
	/// Related to embedded applications.
	/// </summary>
	EmbeddedFirstParty = 1L << 20,

	/// <summary>
	/// To be datamined.
	/// </summary>
	UnknownFlag = 1L << 21,

	/// <summary>
	/// The application has registered global application commands.
	/// </summary>
	ApplicationCommandBadge = 1L << 23,

	/// <summary>
	/// Indicates if an app is considered active. This means that it has had any global command executed in the past 30 days.
	/// </summary>
	Active = 1L << 24
}
