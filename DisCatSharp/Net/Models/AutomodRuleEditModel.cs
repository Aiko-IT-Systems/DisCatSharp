using System.Collections.Generic;

using DisCatSharp.Entities;
using DisCatSharp.Enums;

namespace DisCatSharp.Net.Models;

/// <summary>
///     Represents an automod rule edit model.
/// </summary>
public class AutomodRuleEditModel : BaseEditModel
{
	/// <summary>
	///     Sets the rule's new name.
	/// </summary>
	public Optional<string> Name { internal get; set; }

	/// <summary>
	///     Sets the rule's event type.
	/// </summary>
	public Optional<AutomodEventType> EventType { internal get; set; }

	/// <summary>
	///     Sets the rule's trigger metadata.
	/// </summary>
	public Optional<AutomodTriggerMetadata> TriggerMetadata { internal get; set; }

	/// <summary>
	///     Sets the rule's actions.
	/// </summary>
	public Optional<List<AutomodAction>> Actions { internal get; set; }

	/// <summary>
	///     Sets whether the rule is enabled.
	/// </summary>
	public Optional<bool> Enabled { internal get; set; }

	/// <summary>
	///     Sets the roles exempt from the rule.
	/// </summary>
	public Optional<List<ulong>> ExemptRoles { internal get; set; }

	/// <summary>
	///     Sets the channels exempt from the rule.
	/// </summary>
	public Optional<List<ulong>> ExemptChannels { internal get; set; }
}
