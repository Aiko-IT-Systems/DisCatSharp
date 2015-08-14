using System;
using System.Collections.Generic;

using DisCatSharp.ApplicationCommands.Attributes;

namespace DisCatSharp.ApplicationCommands.Exceptions;

/// <summary>
/// Thrown when a pre-execution check for a context menu command fails.
/// </summary>
public sealed class ContextMenuExecutionChecksFailedException : Exception
{
	/// <summary>
	/// The list of failed checks.
	/// </summary>
	public IReadOnlyList<ApplicationCommandCheckBaseAttribute> FailedChecks;
}
