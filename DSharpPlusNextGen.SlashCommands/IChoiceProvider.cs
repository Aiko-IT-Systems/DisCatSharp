using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlusNextGen.Entities;

namespace DSharpPlusNextGen.SlashCommands
{
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
}
