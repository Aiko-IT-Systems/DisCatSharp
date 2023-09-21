using System;

namespace DisCatSharp.ApplicationCommands.Attributes;

/// <summary>
/// Prevents this field or property from having its value injected by dependency injection.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class DontInjectAttribute : Attribute
{ }
