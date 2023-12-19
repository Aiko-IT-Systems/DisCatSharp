using System;

namespace DisCatSharp.Attributes;

/// <summary>
/// Marks something as unreleased by discord.
/// </summary>
[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
public sealed class DiscordUnreleasedAttribute : Attribute
{
	/// <summary>
	/// The additional information message.
	/// </summary>
	public string Message { get; set; }

	public DiscordUnreleasedAttribute(string message)
	{
		this.Message = message;
	}

	public DiscordUnreleasedAttribute()
	{ }
}
