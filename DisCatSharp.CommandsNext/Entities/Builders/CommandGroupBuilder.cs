using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using DisCatSharp.CommandsNext.Entities;

namespace DisCatSharp.CommandsNext.Builders;

/// <summary>
/// Represents an interface to build a command group.
/// </summary>
public sealed class CommandGroupBuilder : CommandBuilder
{
	/// <summary>
	/// Gets the list of child commands registered for this group.
	/// </summary>
	public IReadOnlyList<CommandBuilder> Children { get; }

	/// <summary>
	/// Gets the children list.
	/// </summary>
	private readonly List<CommandBuilder> _childrenList;

	/// <summary>
	/// Creates a new module-less command group builder.
	/// </summary>
	public CommandGroupBuilder()
		: this(null)
	{ }

	/// <summary>
	/// Creates a new command group builder.
	/// </summary>
	/// <param name="module">Module on which this group is to be defined.</param>
	public CommandGroupBuilder(ICommandModule module)
		: base(module)
	{
		this._childrenList = [];
		this.Children = new ReadOnlyCollection<CommandBuilder>(this._childrenList);
	}

	/// <summary>
	/// Adds a command to the collection of child commands for this group.
	/// </summary>
	/// <param name="child">Command to add to the collection of child commands for this group.</param>
	/// <returns>This builder.</returns>
	public CommandGroupBuilder WithChild(CommandBuilder child)
	{
		this._childrenList.Add(child);
		return this;
	}

	/// <summary>
	/// Builds the command group.
	/// </summary>
	/// <param name="parent">The parent command group.</param>
	internal override Command Build(CommandGroup parent)
	{
		var cmd = new CommandGroup
		{
			Name = this.Name,
			Description = this.Description,
			Aliases = this.Aliases,
			ExecutionChecks = this.ExecutionChecks,
			IsHidden = this.IsHidden,
			Parent = parent,
			Overloads = new ReadOnlyCollection<CommandOverload>(this.Overloads.Select(xo => xo.Build()).ToList()),
			Module = this.Module,
			CustomAttributes = this.CustomAttributes
		};

		var cs = new List<Command>();
		foreach (var xc in this.Children)
			cs.Add(xc.Build(cmd));

		cmd.Children = new ReadOnlyCollection<Command>(cs);
		return cmd;
	}
}
