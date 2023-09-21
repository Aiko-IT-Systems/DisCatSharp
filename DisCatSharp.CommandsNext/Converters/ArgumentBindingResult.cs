using System;
using System.Collections.Generic;

namespace DisCatSharp.CommandsNext.Converters;

/// <summary>
/// Represents a argument binding result.
/// </summary>
public readonly struct ArgumentBindingResult
{
	/// <summary>
	/// Gets a value indicating whether the binding is successful.
	/// </summary>
	public bool IsSuccessful { get; }

	/// <summary>
	/// Gets the converted.
	/// </summary>
	public object[] Converted { get; }

	/// <summary>
	/// Gets the raw.
	/// </summary>
	public IReadOnlyList<string> Raw { get; }

	/// <summary>
	/// Gets the reason.
	/// </summary>
	public Exception Reason { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="ArgumentBindingResult"/> class.
	/// </summary>
	/// <param name="converted">The converted.</param>
	/// <param name="raw">The raw.</param>
	public ArgumentBindingResult(object[] converted, IReadOnlyList<string> raw)
	{
		this.IsSuccessful = true;
		this.Reason = null;
		this.Converted = converted;
		this.Raw = raw;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="ArgumentBindingResult"/> class.
	/// </summary>
	/// <param name="ex">The ex.</param>
	public ArgumentBindingResult(Exception ex)
	{
		this.IsSuccessful = false;
		this.Reason = ex;
		this.Converted = null;
		this.Raw = null;
	}
}
