using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents an action which will execute when a rule is triggered.
/// </summary>
public class AutomodAction : ObservableApiObject
{
	/// <summary>
	/// The type of action.
	/// </summary>
	[JsonProperty("type")]
	public AutomodActionType ActionType { get; internal set; }

	/// <summary>
	/// The additional meta data needed during execution for this specific action type.
	/// </summary>
	[JsonProperty("metadata", NullValueHandling = NullValueHandling.Ignore)]
	public AutomodActionMetadata Metadata { get; internal set; }

	/// <summary>
	/// Creates a new empty automod action.
	/// </summary>
	internal AutomodAction()
	{ }

	/// <summary>
	/// Creates a new automod action.
	/// </summary>
	/// <param name="actionType">The type of action.</param>
	/// <param name="metadata">The additional metadata for this action.</param>
	public AutomodAction(AutomodActionType actionType, AutomodActionMetadata metadata = null)
	{
		this.ActionType = actionType;
		this.Metadata = metadata;
	}
}
