using System.Collections.Generic;
using System.Threading.Tasks;

using DisCatSharp.Entities;

namespace DisCatSharp.ApplicationCommands.Attributes;

/// <summary>
/// All choice providers must inherit from this interface
/// </summary>
public interface IChoiceProvider
{
	/// <summary>
	/// Sets the choices for the slash command
	/// </summary>
	Task<IEnumerable<DiscordApplicationCommandOptionChoice>> Provider();
}
