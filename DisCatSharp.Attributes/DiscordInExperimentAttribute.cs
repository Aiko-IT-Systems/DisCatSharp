using System;

namespace DisCatSharp.Attributes;

/// <summary>
/// Marks something as in experiment by discord.
/// </summary>
[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
public sealed class DiscordInExperimentAttribute : Attribute
{
	/// <summary>
	/// The additional information message.
	/// </summary>
	public string Message { get; set; }

	public DiscordInExperimentAttribute(string message)
	{
		this.Message = message;
	}

	public DiscordInExperimentAttribute()
	{ }
}
