using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using DisCatSharp.Enums;
using DisCatSharp.Exceptions;
using DisCatSharp.Net.Models;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents an auto mod rule.
/// </summary>
public class AutomodRule : SnowflakeObject
{
	/// <summary>
	/// Gets the id of the guild this rule belongs to.
	/// </summary>
	[JsonProperty("guild_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong GuildId { get; internal set; }

	/// <summary>
	/// Gets the name of this rule.
	/// </summary>
	[JsonProperty("name")]
	public string Name { get; internal set; }

	/// <summary>
	/// The id of the user who first created this rule.
	/// </summary>
	[JsonProperty("creator_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong CreatorId { get; internal set; }

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
	public IReadOnlyList<AutomodAction> Actions { get; internal set; }

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
	public IReadOnlyList<ulong>? ExemptRoles { get; internal set; }

	/// <summary>
	/// The channel ids that should not be affected by the rule.
	/// Maximum of 50.
	/// </summary>
	[JsonProperty("exempt_channels", NullValueHandling = NullValueHandling.Ignore)]
	public IReadOnlyList<ulong>? ExemptChannels { get; internal set; }

	[JsonIgnore]
	public DiscordGuild Guild
		=> this.Discord.Guilds.TryGetValue(this.GuildId, out var guild) ? guild : null;

	/// <summary>
	/// Modifies this auto mod rule.
	/// </summary>
	/// <param name="action">Action to perform on this rule.</param>
	/// <returns>The modified rule object.</returns>
	/// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageGuild"/> permission.</exception>
	/// <exception cref="NotFoundException">Thrown when the rule does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task<AutomodRule> ModifyAsync(Action<AutomodRuleEditModel> action)
	{
		var mdl = new AutomodRuleEditModel();
		action(mdl);

		if (mdl.TriggerMetadata.HasValue)
		{
			if ((mdl.TriggerMetadata.Value.KeywordFilter != null || mdl.TriggerMetadata.Value.RegexPatterns != null) && this.TriggerType != AutomodTriggerType.Keyword)
				throw new ArgumentException($"Cannot use KeywordFilter and RegexPattern for a {this.TriggerType} rule. Only {AutomodTriggerType.Keyword} is valid in this context.");
			else if (mdl.TriggerMetadata.Value.AllowList != null && this.TriggerType != AutomodTriggerType.KeywordPreset)
				throw new ArgumentException($"Cannot use AllowList for a {this.TriggerType} rule. Only {AutomodTriggerType.KeywordPreset} is valid in this context.");
			else if (mdl.TriggerMetadata.Value.MentionTotalLimit != null && this.TriggerType != AutomodTriggerType.MentionSpam)
				throw new ArgumentException($"Cannot use MentionTotalLimit for a {this.TriggerType} rule. Only {AutomodTriggerType.MentionSpam} is valid in this context.");

			if (mdl.TriggerMetadata.Value.MentionRaidProtectionEnabled != null && this.TriggerType != AutomodTriggerType.MentionSpam)
				throw new ArgumentException($"Cannot use MentionRaidProtectionEnabled for a {this.TriggerType} rule. Only {AutomodTriggerType.MentionSpam} is valid in this context.");
		}

		return await this.Discord.ApiClient.ModifyAutomodRuleAsync(this.GuildId, this.Id, mdl.Name, mdl.EventType, mdl.TriggerMetadata, mdl.Actions, mdl.Enabled, mdl.ExemptRoles, mdl.ExemptChannels, mdl.AuditLogReason).ConfigureAwait(false);
	}

	/// <summary>
	/// Deletes this auto mod rule.
	/// </summary>
	/// <param name="reason">The reason for this deletion.</param>
	/// <exception cref="UnauthorizedException">Thrown when the client does not have the <see cref="Permissions.ManageGuild"/> permission.</exception>
	/// <exception cref="NotFoundException">Thrown when the rule does not exist.</exception>
	/// <exception cref="BadRequestException">Thrown when an invalid parameter was provided.</exception>
	/// <exception cref="ServerErrorException">Thrown when Discord is unable to process the request.</exception>
	public async Task DeleteAsync(string reason = null)
		=> await this.Discord.ApiClient.DeleteAutomodRuleAsync(this.GuildId, this.Id, reason).ConfigureAwait(false);
}
