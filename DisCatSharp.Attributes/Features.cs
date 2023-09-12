// This file is part of the DisCatSharp project.
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

namespace DisCatSharp.Attributes;

/// <summary>
/// Features enum.
/// </summary>
[Flags]
public enum Features : long
{
	[FeatureDescription("Requires that you specify an override in the DiscordConfiguration.")]
	Override = 1 << 0,

	[FeatureDescription("Requires that the guild has onboarding enabled.")]
	Onboarding = 1 << 1,

	[FeatureDescription("Requires that the guild is partnered.")]
	Partnered = 1 << 2,

	[FeatureDescription("Requires that the guild is verified.")]
	Verified = 1 << 3,

	[FeatureDescription("Requires that the guild has discovery enabled.")]
	Discoverable = 1 << 4,

	[FeatureDescription("Requires that the guild has community enabled.")]
	Community = 1 << 5,

	[FeatureDescription("Requires that the guild has monetization enabled.")]
	ServerSubscription = 1 << 6
}

/// <summary>
/// Defines a readable name for this feature requirement.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public sealed class FeatureDescriptionAttribute : Attribute
{
	/// <summary>
	/// Gets the readable name for this feature requirement.
	/// </summary>
	public string Description { get; }

	/// <summary>
	/// Defines a readable name for this feature requirement.
	/// </summary>
	/// <param name="description">Readable name for this feature requirement.</param>
	public FeatureDescriptionAttribute(string description)
	{
		this.Description = description;
	}
}
