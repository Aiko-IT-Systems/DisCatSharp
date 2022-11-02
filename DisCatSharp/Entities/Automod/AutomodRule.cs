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

using System.Collections.ObjectModel;

using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities
{
	/// <summary>
	/// Represents an auto mod rule.
	/// </summary>
	public class AutomodRule : SnowflakeObject
	{
		public AutomodRule(ulong? ruleId, ulong? guildId, string ruleName, ulong? creatorId, AutomodEventType eventType, AutomodTriggerType triggerType, AutomodTriggerMetadata triggerMetadata,
			ReadOnlyCollection<AutomodAction> actions, bool enabled, ReadOnlyCollection<ulong> exemptRoles, ReadOnlyCollection<ulong> exemptChannels)
		{
			this.RuleId = ruleId;
			this.GuildId = guildId;
			this.RuleName = ruleName;
			this.CreatorId = creatorId;
			this.EventType = eventType;
			this.TriggerType = triggerType;
			this.TriggerMetadata = triggerMetadata;
			this.Actions = actions;
			this.Enabled = enabled;
			this.ExemptRoles = exemptRoles;
			this.ExemptChannels = exemptChannels;
		}

		/// <summary>
		/// Gets the base client.
		/// </summary>
		internal BaseDiscordClient Discord { get; set; }

		/// <summary>
		/// Gets the id of this rule.
		/// </summary>
		[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
		public ulong? RuleId { get; internal set; }

		/// <summary>
		/// Gets the id of the guild this rule belongs to.
		/// </summary>
		[JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
		public ulong? GuildId { get; internal set; }

		/// <summary>
		/// Gets the name of this rule.
		/// </summary>
		[JsonProperty("name")]
		public string RuleName { get; internal set; }

		/// <summary>
		/// The id of the user who first created this rule.
		/// </summary>
		[JsonProperty("creator_id", NullValueHandling = NullValueHandling.Ignore)]
		public ulong? CreatorId { get; internal set; }

		/// <summary>
		/// Gets the type of this rule.
		/// </summary>
		[JsonProperty("event_type")]
		public AutomodEventType EventType { get; internal set; }

		/// <summary>
		/// Gets the trigger type of this rule.
		/// </summary>
		[JsonProperty("trigger_type")]
		public AutomodTriggerType TriggerType { get; internal set; }

		/// <summary>
		/// Gets the rule trigger meta data.
		/// </summary>
		[JsonProperty("trigger_metadata", NullValueHandling = NullValueHandling.Ignore)]
		public AutomodTriggerMetadata TriggerMetadata { get; internal set; }

		/// <summary>
		/// The actions which will execute when the rule is triggered.
		/// </summary>
		[JsonProperty("actions")]
		public ReadOnlyCollection<AutomodAction> Actions { get; internal set; }

		/// <summary>
		/// Whether the rule is enabled.
		/// </summary>
		[JsonProperty("enabled")]
		public bool Enabled { get; internal set; }

		/// <summary>
		/// The role ids that should not be affected by the rule.
		/// Maximum of 20.
		/// </summary>
		[JsonProperty("exempt_roles", NullValueHandling = NullValueHandling.Ignore)]
		public ReadOnlyCollection<ulong>? ExemptRoles { get; internal set; }

		/// <summary>
		/// The channel ids that should not be affected by the rule.
		/// Maximum of 50.
		/// </summary>
		[JsonProperty("exempt_channels", NullValueHandling = NullValueHandling.Ignore)]
		public ReadOnlyCollection<ulong>? ExemptChannels { get; internal set; }

		[JsonIgnore]
		public DiscordGuild Guild
			=> this.Discord.Guilds.TryGetValue(this.GuildId, out var guild) ? guild : null;
	}
}
