using System;

using DisCatSharp.CommandsNext.Attributes;
using DisCatSharp.CommandsNext.Entities;

namespace DisCatSharp.CommandsNext.Builders;

/// <summary>
///     Represents an interface to build a command module.
/// </summary>
public sealed class CommandModuleBuilder
{
	/// <summary>
	///     Creates a new command module builder.
	/// </summary>
	public CommandModuleBuilder()
	{ }

	/// <summary>
	///     Gets the type this build will construct a module out of.
	/// </summary>
	public Type Type { get; private set; }

	/// <summary>
	///     Gets the lifespan for the built module.
	/// </summary>
	public ModuleLifespan Lifespan { get; private set; }

	/// <summary>
	///     Sets the type this builder will construct a module out of.
	/// </summary>
	/// <param name="t">Type to build a module out of. It has to derive from <see cref="BaseCommandModule" />.</param>
	/// <returns>This builder.</returns>
	public CommandModuleBuilder WithType(Type t)
	{
		if (!t.IsModuleCandidateType())
			throw new ArgumentException("Specified type is not a valid module type.", nameof(t));

		this.Type = t;
		return this;
	}

	/// <summary>
	///     Lifespan to give this module.
	/// </summary>
	/// <param name="lifespan">Lifespan for this module.</param>
	/// <returns>This builder.</returns>
	public CommandModuleBuilder WithLifespan(ModuleLifespan lifespan)
	{
		this.Lifespan = lifespan;
		return this;
	}

	/// <summary>
	///     Builds the command module.
	/// </summary>
	/// <param name="services">The services.</param>
	internal ICommandModule Build(IServiceProvider services) =>
		this.Lifespan switch
		{
			ModuleLifespan.Singleton => new SingletonCommandModule(this.Type, services),
			ModuleLifespan.Transient => new TransientCommandModule(this.Type),
			_ => throw new NotSupportedException("Module lifespans other than transient and singleton are not supported.")
		};
}
