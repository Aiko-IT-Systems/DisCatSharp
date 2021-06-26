using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlusNextGen.Entities;

namespace DSharpPlusNextGen.SlashCommands
{
    public interface IChoiceProvider
    {
        Task<IEnumerable<DiscordApplicationCommandOptionChoice>> Provider();
    }
}
