using System;

using DisCatSharp.Entities;

namespace DisCatSharp.EventArgs;

/// <summary>
///     Represents arguments for <see cref="DiscordClient.AutomodRuleDeleted" /> event.
/// </summary>
public class AutomodRuleDeleteEventArgs : DiscordEventArgs
{
	public AutomodRuleDeleteEventArgs(IServiceProvider provider)
		: base(provider)
	{ }

	/// <summary>
	///     Gets the rule that has been deleted.
	/// </summary>
	public AutomodRule Rule { get; internal set; }
}
