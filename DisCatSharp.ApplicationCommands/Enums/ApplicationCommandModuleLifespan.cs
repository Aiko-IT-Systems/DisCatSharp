namespace DisCatSharp.ApplicationCommands.Enums;

/// <summary>
/// Represents a application command module lifespan.
/// </summary>
public enum ApplicationCommandModuleLifespan
{
	/// <summary>
	/// Whether this module should be initiated every time a command is run, with dependencies injected from a scope.
	/// </summary>
	Scoped,

	/// <summary>
	/// Whether this module should be initiated every time a command is run.
	/// </summary>
	Transient,

	/// <summary>
	/// Whether this module should be initiated at startup.
	/// </summary>
	Singleton
}
