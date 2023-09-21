using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a new-member action.
/// </summary>
public sealed class NewMemberAction : ObservableApiObject
{
	/// <summary>
	/// Gets the new-member action channel id.
	/// </summary>
	[JsonProperty("channel_id", NullValueHandling = NullValueHandling.Ignore)]
	public ulong ChannelId { get; internal set; }

	/// <summary>
	/// Gets the new-member action type.
	/// </summary>
	[JsonProperty("action_type", NullValueHandling = NullValueHandling.Ignore)]
	public NewMemberActionType ActionType { get; internal set; }

	/// <summary>
	/// Gets the new-member action title.
	/// </summary>
	[JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
	public string Title { get; internal set; }

	/// <summary>
	/// Gets the new-member action description.
	/// </summary>
	[JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
	public string Description { get; internal set; }

	/// <summary>
	/// Gets the new-member action emoji.
	/// </summary>
	[JsonProperty("emoji", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordEmoji Emoji { get; internal set; }

	/// <summary>
	/// Constructs a new new-member action.
	/// </summary>
	/// <param name="channelId">The action's target channel id.</param>
	/// <param name="actionType">The action's type.</param>
	/// <param name="title">Te action's title.</param>
	/// <param name="description">The action's description.</param>
	/// <param name="emoji">The action's emoji.</param>
	public NewMemberAction(ulong channelId, NewMemberActionType actionType, string title, string description, DiscordEmoji emoji)
	{
		this.ChannelId = channelId;
		this.ActionType = actionType;
		this.Title = title;
		this.Description = description;
		this.Emoji = emoji;
	}

	/// <summary>
	/// Constructs a new new-member action.
	/// </summary>
	internal NewMemberAction()
	{ }
}
