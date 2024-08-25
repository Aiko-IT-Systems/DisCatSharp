using System;

namespace DisCatSharp.Exceptions;

/// <summary>
/// Represents a validation exception thrown by DisCatSharp because of an invalid user input.
/// </summary>
public sealed class ValidationException : DisCatSharpUserException
{
	/// <summary>
	/// Gets the validated type.
	/// </summary>
	public Type ValidatedType { get; internal set; }

	/// <summary>
	/// Gets i.e. the method name, where the exception was thrown for.
	/// </summary>
	public string Origin { get; internal set; }

	/// <summary>
	/// Gets the error message.
	/// </summary>
	public string ErrorMessage { get; internal set; }

	/// <summary>
	/// Constructs a new <see cref="ValidationException"/>.
	/// </summary>
	/// <param name="validatedType">The validated type.</param>
	/// <param name="origin">The validation origin.</param>
	/// <param name="errorMessage">The error message.</param>
	internal ValidationException(Type validatedType, string origin, string errorMessage)
		: base("DisCatSharp validation error: " + errorMessage)
	{
		this.ValidatedType = validatedType;
		this.Origin = origin;
		this.ErrorMessage = errorMessage;
	}
}
