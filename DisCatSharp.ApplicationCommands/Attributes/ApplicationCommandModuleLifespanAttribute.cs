using System;

using DisCatSharp.ApplicationCommands.Enums;

namespace DisCatSharp.ApplicationCommands.Attributes;

/// <summary>
/// Defines this application command module's lifespan. Module lifespans are transient by default.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class ApplicationCommandModuleLifespanAttribute : Attribute
{
	/// <summary>
	/// Gets the lifespan.
	/// </summary>
	public ApplicationCommandModuleLifespan Lifespan { get; }

	/// <summary>
	/// Defines this application command module's lifespan.
	/// </summary>
	/// <param name="lifespan">The lifespan of the module. Module lifespans are transient by default.</param>
	public ApplicationCommandModuleLifespanAttribute(ApplicationCommandModuleLifespan lifespan)
	{
		this.Lifespan = lifespan;
	}
}
