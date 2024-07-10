using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;

using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a poll for a message.
/// </summary>
public sealed class DiscordPoll : ObservableApiObject
{
	/// <summary>
	/// Gets the poll question.
	/// </summary>
	[JsonProperty("question")]
	public DiscordPollMedia Question { get; internal set; }

	/// <summary>
	/// Gets the poll answers to choose from.
	/// </summary>
	[JsonProperty("answers")]
	public List<DiscordPollAnswer> Answers { get; internal set; }

	/// <summary>
	/// Gets the poll's end time as raw string.
	/// </summary>
	[JsonProperty("expiry", NullValueHandling = NullValueHandling.Ignore)]
	internal string? ExpiryRaw { get; set; } = null;

	/// <summary>
	/// Gets the poll's end time.
	/// </summary>
	[JsonIgnore]
	public DateTimeOffset? Expiry
		=> !string.IsNullOrWhiteSpace(this.ExpiryRaw) && DateTimeOffset.TryParse(this.ExpiryRaw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dto) ? dto : null;

	/// <summary>
	/// Gets the poll layout type.
	/// </summary>
	[JsonProperty("layout_type")]
	public PollLayoutType LayoutType { get; internal set; }

	/// <summary>
	/// Gets whether the poll allows multiselect.
	/// </summary>
	[JsonProperty("allow_multiselect")]
	public bool AllowMultiselect { get; internal set; }

	/// <summary>
	/// Gets whether the poll allows multiselect.
	/// </summary>
	[JsonProperty("results", NullValueHandling = NullValueHandling.Ignore)]
	public DiscordPollResult? Results { get; internal set; } = null;

	/// <summary>
	/// Gets the id of the channel this poll was send in.
	/// </summary>
	[JsonIgnore]
	public ulong ChannelId { get; internal set; }

	/// <summary>
	/// Gets the id of the message this poll belongs to.
	/// </summary>
	[JsonIgnore]
	public ulong MessageId { get; internal set; }

	/// <summary>
	/// Gets the id of the author this poll belongs to.
	/// </summary>
	[JsonIgnore]
	public ulong AuthorId { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="DiscordPoll"/> class.
	/// </summary>
	internal DiscordPoll()
	{ }

	/// <summary>
	/// Gets the answer voters for an answer.
	/// </summary>
	/// <param name="answerId">The id of the answer to get voters for.</param>
	/// <param name="limit">The max number of users to return (<c>1</c>-<c>100</c>). Defaults to <c>25</c>.</param>
	/// <param name="after">Get users after this user ID.</param>
	/// <returns>A read-only collection of users who voted for given answer.</returns>
	public async Task<ReadOnlyCollection<DiscordUser>> GetAnswerVotersAsync(int answerId, int? limit = null, ulong? after = null)
		=> await this.Discord.ApiClient.GetAnswerVotersAsync(this.ChannelId, this.MessageId, answerId, limit, after);

	/// <summary>
	/// <para>Ends the poll.</para>
	/// <para>Works only for own polls and if they are not expired yet. </para>
	/// </summary>
	/// <returns>The fresh discord message.</returns>
	/// <exception cref="InvalidOperationException">Thrown when the author is not us, or the poll has been already ended.</exception>
	public async Task<DiscordMessage> EndAsync()
		=> this.AuthorId != this.Discord.CurrentUser.Id
			? throw new InvalidOperationException("Can only end own polls.")
			: this.Results?.IsFinalized ?? false
				? throw new InvalidOperationException("The poll was already ended.")
				: await this.Discord.ApiClient.EndPollAsync(this.ChannelId, this.MessageId);
}
