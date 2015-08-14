using System.Collections.Generic;

using DisCatSharp.Entities;
using DisCatSharp.Enums;

namespace DisCatSharp.Net.Models;

/// <summary>
/// Represents an automod rule edit model.
/// </summary>
public class AutomodRuleEditModel : BaseEditModel
{
	public Optional<string> Name { internal get; set; }

	public Optional<AutomodEventType> EventType { internal get; set; }

	public Optional<AutomodTriggerMetadata> TriggerMetadata { internal get; set; }

	public Optional<List<AutomodAction>> Actions { internal get; set; }

	public Optional<bool> Enabled { internal get; set; }

	public Optional<List<ulong>> ExemptRoles { internal get; set; }

	public Optional<List<ulong>> ExemptChannels { internal get; set; }
}
