using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a welcome message for the server guide.
/// </summary>
public sealed class WelcomeMessage : ObservableApiObject
{
	/// <summary>
	/// Gets the welcome message author ids.
	/// Can only be <c>1</c> and target must have write permission.
	/// </summary>
	[JsonProperty("author_ids", NullValueHandling = NullValueHandling.Ignore)]
	internal List<ulong> AuthorIds { get; set; } = [];

	/// <summary>
	/// Gets the author id.
	/// </summary>
	[JsonIgnore]
	public ulong AuthorId
		=> this.AuthorIds.First();

	/// <summary>
	/// Gets the welcome message.
	/// <para> <c>[@username]</c> is used to mention the new member.</para>
	/// </summary>
	[JsonProperty("message", NullValueHandling = NullValueHandling.Ignore)]
	public string Message { get; internal set; }

	/// <summary>
	/// Constructs a new welcome message for the server guide.
	/// </summary>
	/// <param name="authorId">The author id.</param>
	/// <param name="message">The message. Use <c>[@username]</c> to mention the new member. Required.</param>
	public WelcomeMessage(ulong authorId, string message)
	{
		this.AuthorIds = [authorId];
		this.Message = message;
	}

	/// <summary>
	/// Constructs a welcome message for the server guide.
	/// </summary>
	internal WelcomeMessage()
	{ }
}
