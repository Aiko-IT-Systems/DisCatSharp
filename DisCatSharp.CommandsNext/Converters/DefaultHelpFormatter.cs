// This file is part of the DisCatSharp project, a fork of DSharpPlus.
//
// Copyright (c) 2021 AITSYS
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using DisCatSharp.CommandsNext.Entities;
using DisCatSharp.Entities;

namespace DisCatSharp.CommandsNext.Converters
{
    /// <summary>
    /// Default CommandsNext help formatter.
    /// </summary>
    public class DefaultHelpFormatter : BaseHelpFormatter
    {
        /// <summary>
        /// Gets the embed builder.
        /// </summary>
        public DiscordEmbedBuilder EmbedBuilder { get; }
        /// <summary>
        /// Gets or sets the command.
        /// </summary>
        private Command Command { get; set; }

        /// <summary>
        /// Creates a new default help formatter.
        /// </summary>
        /// <param name="Ctx">Context in which this formatter is being invoked.</param>
        public DefaultHelpFormatter(CommandContext Ctx)
            : base(Ctx)
        {
            this.EmbedBuilder = new DiscordEmbedBuilder()
                .WithTitle("Help")
                .WithColor(0x007FFF);
        }

        /// <summary>
        /// Sets the command this help message will be for.
        /// </summary>
        /// <param name="Command">Command for which the help message is being produced.</param>
        /// <returns>This help formatter.</returns>
        public override BaseHelpFormatter WithCommand(Command Command)
        {
            this.Command = Command;

            this.EmbedBuilder.WithDescription($"{Formatter.InlineCode(Command.Name)}: {Command.Description ?? "No description provided."}");

            if (Command is CommandGroup cgroup && cgroup.IsExecutableWithoutSubcommands)
                this.EmbedBuilder.WithDescription($"{this.EmbedBuilder.Description}\n\nThis group can be executed as a standalone command.");

            if (Command.Aliases?.Any() == true)
                this.EmbedBuilder.AddField("Aliases", string.Join(", ", Command.Aliases.Select(Formatter.InlineCode)), false);

            if (Command.Overloads?.Any() == true)
            {
                var sb = new StringBuilder();

                foreach (var ovl in Command.Overloads.OrderByDescending(X => X.Priority))
                {
                    sb.Append('`').Append(Command.QualifiedName);

                    foreach (var arg in ovl.Arguments)
                        sb.Append(arg.IsOptional || arg.IsCatchAll ? " [" : " <").Append(arg.Name).Append(arg.IsCatchAll ? "..." : "").Append(arg.IsOptional || arg.IsCatchAll ? ']' : '>');

                    sb.Append("`\n");

                    foreach (var arg in ovl.Arguments)
                        sb.Append('`').Append(arg.Name).Append(" (").Append(this.CommandsNext.GetUserFriendlyTypeName(arg.Type)).Append(")`: ").Append(arg.Description ?? "No description provided.").Append('\n');

                    sb.Append('\n');
                }

                this.EmbedBuilder.AddField("Arguments", sb.ToString().Trim(), false);
            }

            return this;
        }

        /// <summary>
        /// Sets the subcommands for this command, if applicable. This method will be called with filtered data.
        /// </summary>
        /// <param name="Subcommands">Subcommands for this command group.</param>
        /// <returns>This help formatter.</returns>
        public override BaseHelpFormatter WithSubcommands(IEnumerable<Command> Subcommands)
        {
            this.EmbedBuilder.AddField(this.Command != null ? "Subcommands" : "Commands", string.Join(", ", Subcommands.Select(X => Formatter.InlineCode(X.Name))), false);

            return this;
        }

        /// <summary>
        /// Construct the help message.
        /// </summary>
        /// <returns>Data for the help message.</returns>
        public override CommandHelpMessage Build()
        {
            if (this.Command == null)
                this.EmbedBuilder.WithDescription("Listing all top-level commands and groups. Specify a command to see more information.");

            return new CommandHelpMessage(Embed: this.EmbedBuilder.Build());
        }
    }
}
