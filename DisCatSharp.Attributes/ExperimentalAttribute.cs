using System;

namespace DisCatSharp.Attributes;

/// <summary>
/// Marks something as experimental by DisCatSharp.
/// </summary>
[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
public sealed class ExperimentalAttribute : Attribute
{
	/// <summary>
	/// The additional information message.
	/// </summary>
	public string Message { get; set; }

	public ExperimentalAttribute(string message)
	{
		this.Message = message;
	}

	public ExperimentalAttribute()
	{ }
}
