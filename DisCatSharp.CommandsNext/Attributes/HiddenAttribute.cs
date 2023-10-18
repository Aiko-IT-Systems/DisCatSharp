using System;

namespace DisCatSharp.CommandsNext.Attributes;

/// <summary>
/// Marks this command or group as hidden.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class HiddenAttribute : Attribute
{ }
