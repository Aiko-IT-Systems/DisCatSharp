using System;

namespace DisCatSharp.Attributes;

/// <summary>
/// Marks something as deprecated by DisCatSharp.
/// </summary>
[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
public sealed class DeprecatedAttribute : Attribute
{
	/// <summary>
	/// The additional information message.
	/// </summary>
	public string Message { get; set; }

	public DeprecatedAttribute(string message)
	{
		this.Message = message;
	}

	public DeprecatedAttribute()
	{ }
}
