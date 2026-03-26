using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace DisCatSharp.Analyzer;

/// <summary>
///     Base class for code-fix providers that serve a single DisCatSharp diagnostic.
/// </summary>
public abstract class SingleDiagnosticCodeFixProvider : CodeFixProvider
{
	/// <summary>
	///     Gets the diagnostic ID that this fixer handles.
	/// </summary>
	protected abstract string FixableDiagnosticId { get; }

	/// <inheritdoc />
	public sealed override ImmutableArray<string> FixableDiagnosticIds
		=> [this.FixableDiagnosticId];

	/// <inheritdoc />
	public sealed override FixAllProvider GetFixAllProvider()
		=> WellKnownFixAllProviders.BatchFixer;

	/// <inheritdoc />
	public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var diagnostics = context.Diagnostics
			.Where(x => x.Id == this.FixableDiagnosticId)
			.ToImmutableArray();

		if (diagnostics.IsDefaultOrEmpty)
			return Task.CompletedTask;

		return this.RegisterCodeFixesAsync(context, diagnostics);
	}

	/// <summary>
	///     Registers fixes for the diagnostics already filtered to <see cref="FixableDiagnosticId" />.
	/// </summary>
	protected abstract Task RegisterCodeFixesAsync(CodeFixContext context, ImmutableArray<Diagnostic> diagnostics);
}
