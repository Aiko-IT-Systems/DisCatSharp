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

using System.Collections.Generic;
using System.Collections.ObjectModel;

using DisCatSharp.Attributes;
using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities
{
	/// <summary>
	/// Represents the rule's meta data.
	/// </summary>
	public class AutomodTriggerMetadata
	{
		/// <summary>
		/// <para>The substrings which will be searched for in content. Maximum of 1000.</para>
		/// <para>A keyword can be a phrase which contains multiple words. Wildcard symbols (not available to allow lists) can be used to customize how each keyword will be matched.</para>
		/// <para>See <see href="https://discord.com/developers/docs/resources/auto-moderation#auto-moderation-rule-object-keyword-matching-strategies">keyword matching strategies</see>. Each keyword must be 30 characters or less.</para>
		/// </summary>
		[JsonProperty("keyword_filter", NullValueHandling = NullValueHandling.Ignore)]
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
		public ReadOnlyCollection<string>? KeywordFilter { get; set; }
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.

		/// <summary>
		/// The internally predefined word-sets which will be searched for in content.
		/// </summary>
		[JsonProperty("presets", NullValueHandling = NullValueHandling.Ignore)]
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
		public IReadOnlyList<AutomodKeywordPresetType>? Presets { get; internal set; }
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.

		/// <summary>
		/// <para>The regex patterns the content will be checked against. Maximum of 10.</para>
		/// <para>Only Rust flavored regex is currently supported, which can be tested in online editors such as <see href="https://rustexp.lpil.uk/">Rustexp</see>.</para>
		/// <para> Each regex pattern must be 75 characters or less.</para>
		/// </summary>
		[JsonProperty("regex_patterns", NullValueHandling = NullValueHandling.Ignore)]
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
		public List<string>? RegexPatterns { get; set; }
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.

		/// <summary>
		/// The substrings which will be exempt from triggering the preset type. Maximum of 1000.
		/// </summary>
		[JsonProperty("allow_list", NullValueHandling = NullValueHandling.Ignore)]
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
		public List<string>? AllowList { get; set; }
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.

		/// <summary>
		/// The total number of unique role and user mentions allowed per message. Maximum of 50.
		/// </summary>
		[JsonProperty("mention_total_limit", NullValueHandling = NullValueHandling.Ignore)]
		public int? MentionTotalLimit { get; set; } = null;

		/// <summary>
		/// Whether to automatically detect mention raids.
		/// </summary>
		[JsonProperty("mention_raid_protection_enabled", NullValueHandling = NullValueHandling.Ignore), DiscordInExperiment]
		public bool? MentionRaidProtectionEnabled { get; set; } = null;
	}
}
