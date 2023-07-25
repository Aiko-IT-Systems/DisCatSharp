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
// FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Collections.Generic;

using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents an onboarding prompt.
/// </summary>
public sealed class DiscordOnboardingPrompt : SnowflakeObject
{
	/// <summary>
	/// Gets the title.
	/// </summary>
	[JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
	public string Title { get; internal set; }

	/// <summary>
	/// Gets the prompt options.
	/// </summary>
	[JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
	public List<DiscordOnboardingPromptOption> Options { get; internal set; } = new();

	/// <summary>
	/// Gets whether the prompt is single select.
	/// </summary>
	[JsonProperty("single_select", NullValueHandling = NullValueHandling.Ignore)]
	public bool SingleSelect { get; internal set; }

	/// <summary>
	/// Gets whether the prompt is required.
	/// </summary>
	[JsonProperty("required", NullValueHandling = NullValueHandling.Ignore)]
	public bool Required { get; internal set; }

	/// <summary>
	/// Gets whether the prompt is shown on join in the onboarding.
	/// </summary>
	[JsonProperty("in_onboarding", NullValueHandling = NullValueHandling.Ignore)]
	public bool InOnboarding { get; internal set; }

	/// <summary>
	/// Gets the prompt time.
	/// </summary>
	[JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
	public PromptType? Type { get; internal set; }

	/// <summary>
	/// Constructs a new onboarding prompt.
	/// </summary>
	/// <param name="title">The prompt title.</param>
	/// <param name="options">The prompt options.</param>
	/// <param name="singleSelect">Whether the prompt is single select. Defaults to <see langword="false"/>.</param>
	/// <param name="required">Whether the prompt is required. Defaults to <see langword="true"/>.</param>
	/// <param name="inOnboarding">Whether the prompt is shown in onboarding. Defaults to <see langword="true"/>.</param>
	/// <param name="type">The prompt type. Defaults to <see cref="PromptType.MultipleChoice"/>.</param>
	public DiscordOnboardingPrompt(string title, List<DiscordOnboardingPromptOption> options,
		bool singleSelect = false, bool required = true,
		bool inOnboarding = true, PromptType type = PromptType.MultipleChoice)
	{
		this.Title = title;
		this.Options = options;
		this.SingleSelect = singleSelect;
		this.Required = required;
		this.InOnboarding = inOnboarding;
		this.Type = type;
	}

	/// <summary>
	/// Constructs a new onboarding prompt.
	/// </summary>
	internal DiscordOnboardingPrompt()
	{ }
}
