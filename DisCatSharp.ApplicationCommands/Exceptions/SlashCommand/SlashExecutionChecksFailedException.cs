using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;

namespace DisCatSharp.ApplicationCommands.Exceptions;

/// <summary>
///     Thrown when a pre-execution check for a slash command fails.
/// </summary>
public class SlashExecutionChecksFailedException : Exception
{
	/// <summary>
	///     Creates a new <see cref="SlashExecutionChecksFailedException" />.
	/// </summary>
	public SlashExecutionChecksFailedException()
		: base("One or more pre-execution checks failed.")
	{ }

	/// <summary>
	///     Creates a new <see cref="SlashExecutionChecksFailedException" />.
	/// </summary>
	/// <param name="failedChecks">The checks that failed.</param>
	public SlashExecutionChecksFailedException(IEnumerable<ApplicationCommandCheckBaseAttribute> failedChecks)
		: base("One or more pre-execution checks failed.")
	{
		this.FailedChecks = new ReadOnlyCollection<ApplicationCommandCheckBaseAttribute>([.. failedChecks]);
	}

	/// <summary>
	///     Creates a new <see cref="SlashExecutionChecksFailedException" />.
	/// </summary>
	/// <param name="context">The interaction context in which the checks failed.</param>
	/// <param name="failedChecks">The checks that failed.</param>
	public SlashExecutionChecksFailedException(InteractionContext context, IEnumerable<ApplicationCommandCheckBaseAttribute> failedChecks)
		: this(failedChecks)
	{
		this.Context = context;
	}

	/// <summary>
	///     The interaction context in which the checks failed.
	/// </summary>
	public InteractionContext Context { get; private set; }

	/// <summary>
	///     The list of failed checks.
	/// </summary>
	public IReadOnlyList<ApplicationCommandCheckBaseAttribute> FailedChecks = [];
}
