using System;

namespace DisCatSharp.CommandsNext.Attributes;

/// <summary>
/// Indicates that the command argument takes the rest of the input without parsing.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public class RemainingTextAttribute : Attribute
{ }
