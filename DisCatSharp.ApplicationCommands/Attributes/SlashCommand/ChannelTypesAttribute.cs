using System;
using System.Collections.Generic;

using DisCatSharp.Enums;

namespace DisCatSharp.ApplicationCommands.Attributes;

/// <summary>
///     Defines allowed channel types for a channel parameter.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public class ChannelTypesAttribute : Attribute
{
	/// <summary>
	///     Defines allowed channel types for a channel parameter.
	/// </summary>
	/// <param name="channelTypes">The channel types to allow.</param>
	public ChannelTypesAttribute(params ChannelType[] channelTypes)
	{
		this.ChannelTypes = [.. channelTypes];
	}

	/// <summary>
	///     Allowed channel types.
	/// </summary>
	public List<ChannelType> ChannelTypes { get; }
}
