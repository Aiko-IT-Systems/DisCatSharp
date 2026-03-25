using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;

namespace DisCatSharp.ApplicationCommands.Exceptions;

/// <summary>
///     Thrown when a pre-execution check for a context menu command fails.
/// </summary>
public sealed class ContextMenuExecutionChecksFailedException : Exception
{
	/// <summary>
	///     Creates a new <see cref="ContextMenuExecutionChecksFailedException" />.
	/// </summary>
	public ContextMenuExecutionChecksFailedException()
		: base("One or more pre-execution checks failed.")
	{ }

	/// <summary>
	///     Creates a new <see cref="ContextMenuExecutionChecksFailedException" />.
	/// </summary>
	/// <param name="failedChecks">The checks that failed.</param>
	public ContextMenuExecutionChecksFailedException(IEnumerable<ApplicationCommandCheckBaseAttribute> failedChecks)
		: base("One or more pre-execution checks failed.")
	{
		this.FailedChecks = new ReadOnlyCollection<ApplicationCommandCheckBaseAttribute>([.. failedChecks]);
	}

	/// <summary>
	///     Creates a new <see cref="ContextMenuExecutionChecksFailedException" />.
	/// </summary>
	/// <param name="context">The context menu context in which the checks failed.</param>
	/// <param name="failedChecks">The checks that failed.</param>
	public ContextMenuExecutionChecksFailedException(ContextMenuContext context, IEnumerable<ApplicationCommandCheckBaseAttribute> failedChecks)
		: this(failedChecks)
	{
		this.Context = context;
	}

	/// <summary>
	///     The context menu context in which the checks failed.
	/// </summary>
	public ContextMenuContext Context { get; private set; }

	/// <summary>
	///     The list of failed checks.
	/// </summary>
	public IReadOnlyList<ApplicationCommandCheckBaseAttribute> FailedChecks = [];
}
