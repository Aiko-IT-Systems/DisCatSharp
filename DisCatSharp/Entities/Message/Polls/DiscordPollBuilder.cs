using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DisCatSharp.Enums;

namespace DisCatSharp.Entities;

/// <summary>
/// Constructs a poll to be send.
/// </summary>
public sealed class DiscordPollBuilder
{
	/// <summary>
	/// Gets or sets the question.
	/// </summary>
	public string Question
	{
		get => this._question.Text;
		set
		{
			if (value is { Length: > 300 })
				throw new ArgumentException("Question cannot exceed 300 characters.", nameof(value));

			this._question = new(value);
		}
	}

	private DiscordPollMedia _question;

	/// <summary>
	/// Gets the answers.
	/// </summary>
	public IReadOnlyList<DiscordPollAnswer> Answers => this._answers;

	private readonly List<DiscordPollAnswer> _answers = [];

	/// <summary>
	/// Gets or sets the layout type. Defaults to <see cref="PollLayoutType.Default"/>.
	/// </summary>
	public PollLayoutType LayoutType { get; internal set; } = PollLayoutType.Default;

	/// <summary>
	/// Gets or sets the number of hours the poll should be open for, up to 32 days.
	/// Defaults to <c>24</c> hours.
	/// </summary>
	public int Duration { get; set; } = 24;

	/// <summary>
	/// Gets or sets whether a user can select multiple answers.
	/// Defaults to <see langword="false"/>.
	/// </summary>
	public bool AllowMultiselect { get; set; } = false;

	/// <summary>
	/// Sets the duration for the poll.
	/// </summary>
	/// <param name="hours">How long the poll should be open for. Max 7 days.</param>
	/// <returns>The current builder to be chained.</returns>
	public DiscordPollBuilder SetDuration(int hours)
	{
		this.Duration = hours;
		return this;
	}

	/// <summary>
	/// ALlows users to select multiple answers or not.
	/// </summary>
	/// <param name="allow">Whether a user can select multiple answers.</param>
	/// <returns>The current builder to be chained.</returns>
	public DiscordPollBuilder WithAllowMultiselect(bool allow = true)
	{
		this.AllowMultiselect = allow;
		return this;
	}

	/// <summary>
	/// Sets the question of a poll.
	/// </summary>
	/// <param name="question">The question to be set.</param>
	/// <returns>The current builder to be chained.</returns>
	public DiscordPollBuilder WithQuestion(string question)
	{
		this.Question = question;
		return this;
	}

	/// <summary>
	/// Adds answers to a poll, up to 10 answers.
	/// </summary>
	/// <param name="answers">The answers to add to the poll.</param>
	/// <returns>The current builder to be chained.</returns>
	/// <exception cref="ArgumentOutOfRangeException">No answers were passed.</exception>
	public DiscordPollBuilder AddAnswers(params DiscordPollAnswer[] answers)
		=> this.AddAnswers((IEnumerable<DiscordPollAnswer>)answers);

	/// <summary>
	/// Adds answers without emoji to a poll, up to 10 answers.
	/// </summary>
	/// <param name="answers">The answers to add to the poll.</param>
	/// <returns>The current builder to be chained.</returns>
	public DiscordPollBuilder AddAnswers(params string[] answers)
		=> this.AddAnswers(answers.Select(x => new DiscordPollAnswer(x)));

	/// <summary>
	/// Adds answers to a poll, up to 10 answers.
	/// </summary>
	/// <param name="answers">The answers to add to the message.</param>
	/// <returns>The current builder to be chained.</returns>
	/// <exception cref="ArgumentOutOfRangeException">No answers were passed.</exception>
	public DiscordPollBuilder AddAnswers(IEnumerable<DiscordPollAnswer> answers)
	{
		var cmpArr = answers.ToArray();
		var count = cmpArr.Length + this._answers.Count;

		switch (count)
		{
			case 0:
				throw new ArgumentOutOfRangeException(nameof(answers), "You must provide at least one answer");
			case > 10:
				throw new ArgumentException("Cannot add more than 10 answers to a poll!");
		}

		var answerId = 1;
		foreach (var answer in answers)
		{
			if (answer.PollMedia.Text.Length > 55)
				throw new ArgumentException($"Answers text cannot exceed 55 characters. Thrown in answer {answerId}");

			answerId++;
		}

		this._answers.AddRange(cmpArr);

		return this;
	}

	/// <summary>
	/// Sends the poll to a specific channel
	/// </summary>
	/// <param name="channel">The channel the poll should be sent to.</param>
	/// <returns>The current builder to be chained.</returns>
	public Task<DiscordMessage> SendAsync(DiscordChannel channel)
		=> channel.SendMessageAsync(new DiscordMessageBuilder().WithPoll(this));

	/// <summary>
	/// Clears all answers on this builder.
	/// </summary>
	public void ClearAnswers()
		=> this._answers.Clear();

	/// <summary>
	/// Allows for clearing the poll builder so that it can be used again to build a new poll.
	/// </summary>
	public void Clear()
	{
		this._question = null!;
		this._answers.Clear();
		this.AllowMultiselect = false;
		this.Duration = 24;
		this.LayoutType = PollLayoutType.Default;
	}

	/// <summary>
	/// Does the validation before we send a the create request.
	/// </summary>
	internal void Validate()
	{
		if (this._answers.Count > 10)
			throw new ArgumentException("A poll can only have up to 10 answers.");

		if (this._question is null)
			throw new ArgumentException("You must specify a question.");

		if (this.Duration > 168)
			throw new ArgumentException("Polls can only be open for up to 32 days or 768 hours.");
	}

	/// <summary>
	/// Builds the poll request.
	/// </summary>
	/// <returns>The constructed poll request.</returns>
	internal DiscordPollRequest Build()
		=> new(this._question, this._answers, this.Duration, this.LayoutType, this.AllowMultiselect);
}
