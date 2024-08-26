using System;

using DisCatSharp.Entities;

namespace DisCatSharp.EventArgs;

/// <summary>
///     Represents arguments for <see cref="DiscordClient.AutomodRuleUpdated" /> event.
/// </summary>
public class AutomodRuleUpdateEventArgs : DiscordEventArgs
{
	public AutomodRuleUpdateEventArgs(IServiceProvider provider)
		: base(provider)
	{ }

	/// <summary>
	///     Gets the rule that has been updated.
	/// </summary>
	public AutomodRule Rule { get; internal set; }
}
