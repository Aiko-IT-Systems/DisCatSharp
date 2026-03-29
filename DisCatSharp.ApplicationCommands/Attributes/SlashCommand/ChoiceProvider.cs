using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using DisCatSharp.Entities;

namespace DisCatSharp.ApplicationCommands.Attributes;

/// <summary>
///     Implementation of <see cref="IChoiceProvider" /> with access to service collection.
/// </summary>
public abstract class ChoiceProvider : IChoiceProvider
{
	/// <summary>
	///     Gets or sets the service provider. This property is injected by the framework
	///     during command registration and will be available when <see cref="Provider" /> is called.
	/// </summary>
	public IServiceProvider Services { get; set; } = null!;

	/// <summary>
	///     The optional ID of the Guild the command got registered for.
	/// </summary>
	public ulong? GuildId { get; set; }

	/// <summary>
	///     Sets the choices for the slash command.
	/// </summary>
	public abstract Task<IEnumerable<DiscordApplicationCommandOptionChoice>> Provider();
}
