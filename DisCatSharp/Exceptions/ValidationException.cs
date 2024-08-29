using System;
using System.Collections.Generic;
using System.Linq;

namespace DisCatSharp.Exceptions;

/// <summary>
///     Represents a validation result exception that collects multiple validation errors.
/// </summary>
public sealed class ValidationException : DisCatSharpUserException
{
	/// <summary>
	///     Constructs a new <see cref="ValidationException" />.
	/// </summary>
	/// <param name="validatedType">The validated type.</param>
	/// <param name="origin">The validation origin.</param>
	/// <param name="validationErrors">The list of validation errors.</param>
	internal ValidationException(Type validatedType, string origin, List<ValidationResult> validationErrors)
		: base($"DisCatSharp validation errors: {string.Join("; ", validationErrors.Select(e => e.ErrorMessage))}")
	{
		this.ValidatedType = validatedType;
		this.Origin = origin;
		this.ValidationErrors = validationErrors.AsReadOnly();
	}

	/// <summary>
	///     Gets the validated type.
	/// </summary>
	public Type ValidatedType { get; }

	/// <summary>
	///     Gets the validation origin.
	/// </summary>
	public string Origin { get; }

	/// <summary>
	///     Gets the list of validation errors.
	/// </summary>
	public IReadOnlyList<ValidationResult> ValidationErrors { get; }
}

/// <summary>
///     Represents a single validation result.
/// </summary>
public class ValidationResult(int? index, string name, string errorMessage)
{
	/// <summary>
	///     Gets or sets the index of the item that failed validation.
	/// </summary>
	public int? Index { get; set; } = index;

	/// <summary>
	///     Gets or sets the name of the item that failed validation.
	/// </summary>
	public string Name { get; set; } = name;

	/// <summary>
	///     Gets or sets the error message.
	/// </summary>
	public string ErrorMessage { get; set; } = errorMessage;

	/// <inheritdoc />
	public override string ToString()
	{
		var indexPart = this.Index.HasValue ? $"Index: {this.Index.Value}, " : string.Empty;
		return $"{indexPart}Name: {this.Name}, Error: {this.ErrorMessage}";
	}
}
