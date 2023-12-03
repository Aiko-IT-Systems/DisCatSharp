using System;

namespace DisCatSharp.Attributes;

/// <summary>
/// Marks something as deprecated by discord.
/// </summary>
[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
public sealed class DiscordDeprecatedAttribute : Attribute
{
	/// <summary>
	/// The additional information message.
	/// </summary>
	public string Message { get; set; }

	public DiscordDeprecatedAttribute(string message)
	{
		this.Message = message;
	}

	public DiscordDeprecatedAttribute()
	{ }
}
