using System.Collections.Generic;
using System.Threading.Tasks;

using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;

namespace DisCatSharp.ApplicationCommands.Attributes;

/// <summary>
/// The autocomplete provider.
/// </summary>
public interface IAutocompleteProvider
{
	/// <summary>
	/// Provider the autocompletion.
	/// </summary>
	/// <param name="context">The context.</param>
	Task<IEnumerable<DiscordApplicationCommandAutocompleteChoice>> Provider(AutocompleteContext context);
}
