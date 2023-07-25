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

public sealed class DiscordOnboardingPrompt : SnowflakeObject
{
	[JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
	public string Title { get; internal set; }

	[JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
	public List<DiscordOnboardingPromptOption> Options { get; internal set; } = new();

	[JsonProperty("single_select", NullValueHandling = NullValueHandling.Ignore)]
	public bool SingleSelect { get; internal set; }

	[JsonProperty("required", NullValueHandling = NullValueHandling.Ignore)]
	public bool Required { get; internal set; }

	[JsonProperty("in_onboarding", NullValueHandling = NullValueHandling.Ignore)]
	public bool InOnboarding { get; internal set; }

	[JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
	public PromptType? Type { get; internal set; }

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

	internal DiscordOnboardingPrompt()
	{ }
}