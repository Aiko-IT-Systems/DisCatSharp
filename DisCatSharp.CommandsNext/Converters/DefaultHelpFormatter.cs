using System.Collections.Generic;
using System.Linq;
using System.Text;

using DisCatSharp.CommandsNext.Entities;
using DisCatSharp.Entities;

namespace DisCatSharp.CommandsNext.Converters;

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
	private Command _command;

	/// <summary>
	/// Creates a new default help formatter.
	/// </summary>
	/// <param name="ctx">Context in which this formatter is being invoked.</param>
	public DefaultHelpFormatter(CommandContext ctx)
		: base(ctx)
	{
		this.EmbedBuilder = new DiscordEmbedBuilder()
			.WithTitle("Help")
			.WithColor(0x007FFF);
	}

	/// <summary>
	/// Sets the command this help message will be for.
	/// </summary>
	/// <param name="command">Command for which the help message is being produced.</param>
	/// <returns>This help formatter.</returns>
	public override BaseHelpFormatter WithCommand(Command command)
	{
		this._command = command;

		this.EmbedBuilder.WithDescription($"{command.Name.InlineCode()}: {command.Description ?? "No description provided."}");

		if (command is CommandGroup { IsExecutableWithoutSubcommands: true })
			this.EmbedBuilder.WithDescription($"{this.EmbedBuilder.Description}\n\nThis group can be executed as a standalone command.");

		if (command.Aliases?.Any() == true)
			this.EmbedBuilder.AddField(new("Aliases", string.Join(", ", command.Aliases.Select(Formatter.InlineCode))));

		if (command.Overloads?.Any() == true)
		{
			var sb = new StringBuilder();

			foreach (var ovl in command.Overloads.OrderByDescending(x => x.Priority))
			{
				sb.Append('`').Append(command.QualifiedName);

				foreach (var arg in ovl.Arguments)
					sb.Append(arg.IsOptional || arg.IsCatchAll ? " [" : " <").Append(arg.Name).Append(arg.IsCatchAll ? "..." : "").Append(arg.IsOptional || arg.IsCatchAll ? ']' : '>');

				sb.Append("`\n");

				foreach (var arg in ovl.Arguments)
					sb.Append('`').Append(arg.Name).Append(" (").Append(this.CommandsNext.GetUserFriendlyTypeName(arg.Type)).Append(")`: ").Append(arg.Description ?? "No description provided.").Append('\n');

				sb.Append('\n');
			}

			this.EmbedBuilder.AddField(new("Arguments", sb.ToString().Trim()));
		}

		return this;
	}

	/// <summary>
	/// Sets the subcommands for this command, if applicable. This method will be called with filtered data.
	/// </summary>
	/// <param name="subcommands">Subcommands for this command group.</param>
	/// <returns>This help formatter.</returns>
	public override BaseHelpFormatter WithSubcommands(IEnumerable<Command> subcommands)
	{
		this.EmbedBuilder.AddField(new(this._command != null ? "Subcommands" : "Commands", string.Join(", ", subcommands.Select(x => x.Name.InlineCode()))));

		return this;
	}

	/// <summary>
	/// Construct the help message.
	/// </summary>
	/// <returns>Data for the help message.</returns>
	public override CommandHelpMessage Build()
	{
		if (this._command == null)
			this.EmbedBuilder.WithDescription("Listing all top-level commands and groups. Specify a command to see more information.");

		return new(embed: this.EmbedBuilder.Build());
	}
}
