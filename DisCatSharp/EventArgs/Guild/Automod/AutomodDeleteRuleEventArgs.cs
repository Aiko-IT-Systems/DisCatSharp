using System;

using DisCatSharp.Entities;

namespace DisCatSharp.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.AutomodRuleDeleted"/> event.
/// </summary>
public class AutomodRuleDeleteEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets the rule that has been deleted.
	/// </summary>
	public AutomodRule Rule { get; internal set; }

	public AutomodRuleDeleteEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
