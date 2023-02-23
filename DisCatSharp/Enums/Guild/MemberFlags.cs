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

using DisCatSharp.Attributes;

namespace DisCatSharp.Enums;

/// <summary>
/// Represents additional details of a member account.
/// </summary>
[Flags]
public enum MemberFlags : long
{
	/// <summary>
	/// Member has no flags.
	/// </summary>
	None = 0,

	/// <summary>
	/// Member has left and rejoined the guild.
	/// </summary>
	DidRejoin = 1 << 0,

	/// <summary>
	/// Member has completed onboarding.
	/// </summary>
	[DiscordInExperiment]
	CompletedOnboarding = 1 << 1,

	/// <summary>
	/// Member bypasses guild verification requirements.
	/// </summary>
	[DiscordInExperiment]
	BypassesVerification = 1 << 2,
	[DiscordInExperiment]
	Verified = BypassesVerification,

	/// <summary>
	/// Member has started onboarding.
	/// </summary>
	[DiscordInExperiment]
	StartedOnboarding = 1 << 3,

	/// <summary>
	/// Member has started home actions.
	/// </summary>
	[DiscordInExperiment]
	StartedHomeActions = 1 << 5,

	/// <summary>
	/// Member has completed home actions.
	/// </summary>
	[DiscordInExperiment]
	CompletedHomeActions = 1 << 6,
}

