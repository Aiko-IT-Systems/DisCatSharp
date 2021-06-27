using DSharpPlusNextGen.Common.Utilities;

namespace DSharpPlusNextGen.SlashCommands.EventArgs
{
    /// <summary>
    /// Represents the arguments for a <see cref="SlashCommandsExtension.SlashCommandExecuted"/> event
    /// </summary>
    public class SlashCommandExecutedEventArgs : AsyncEventArgs
    {
        /// <summary>
        /// The context of the command.
        /// </summary>
        public InteractionContext Context { get; internal set; }
    }
}
