using System;

namespace DisCatSharp.ApplicationCommands.Attributes;

/// <summary>
/// The autocomplete attribute.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public class AutocompleteAttribute : Attribute
{
	/// <summary>
	/// The type of the provider.
	/// </summary>
	public Type ProviderType { get; }

	/// <summary>
	/// Adds an autocomplete provider to this command option.
	/// </summary>
	/// <param name="providerType">The type of the provider.</param>
	public AutocompleteAttribute(Type providerType)
	{
		this.ProviderType = providerType;
	}
}
