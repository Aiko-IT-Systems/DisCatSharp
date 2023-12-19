using System;

using DisCatSharp.Entities;

namespace DisCatSharp.EventArgs;

/// <summary>
/// Represents arguments for <see cref="DiscordClient.AutomodRuleCreated"/> event.
/// </summary>
public class AutomodRuleCreateEventArgs : DiscordEventArgs
{
	/// <summary>
	/// Gets the rule that has been created.
	/// </summary>
	public AutomodRule Rule { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="AutomodRuleCreateEventArgs"/> class.
	/// </summary>
	public AutomodRuleCreateEventArgs(IServiceProvider provider)
		: base(provider)
	{ }
}
