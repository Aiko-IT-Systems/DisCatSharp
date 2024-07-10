using System.Collections.Generic;

using DisCatSharp.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents an onboarding prompt.
/// </summary>
public sealed class DiscordOnboardingPrompt : SnowflakeObject
{
	/// <summary>
	/// Gets the title.
	/// </summary>
	[JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
	public string Title { get; internal set; }

	/// <summary>
	/// Gets the prompt options.
	/// </summary>
	[JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
	public List<DiscordOnboardingPromptOption> Options { get; internal set; } = [];

	/// <summary>
	/// Gets whether the prompt is single select.
	/// </summary>
	[JsonProperty("single_select", NullValueHandling = NullValueHandling.Ignore)]
	public bool SingleSelect { get; internal set; }

	/// <summary>
	/// Gets whether the prompt is required.
	/// </summary>
	[JsonProperty("required", NullValueHandling = NullValueHandling.Ignore)]
	public bool Required { get; internal set; }

	/// <summary>
	/// Gets whether the prompt is shown on join in the onboarding.
	/// </summary>
	[JsonProperty("in_onboarding", NullValueHandling = NullValueHandling.Ignore)]
	public bool InOnboarding { get; internal set; }

	/// <summary>
	/// Gets the prompt time.
	/// </summary>
	[JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
	public PromptType? Type { get; internal set; }

	/// <summary>
	/// Constructs a new onboarding prompt.
	/// </summary>
	/// <param name="title">The prompt title.</param>
	/// <param name="options">The prompt options.</param>
	/// <param name="singleSelect">Whether the prompt is single select. Defaults to <see langword="false"/>.</param>
	/// <param name="required">Whether the prompt is required. Defaults to <see langword="true"/>.</param>
	/// <param name="inOnboarding">Whether the prompt is shown in onboarding. Defaults to <see langword="true"/>.</param>
	/// <param name="type">The prompt type. Defaults to <see cref="PromptType.MultipleChoice"/>.</param>
	public DiscordOnboardingPrompt(
		string title,
		List<DiscordOnboardingPromptOption> options,
		bool singleSelect = false,
		bool required = true,
		bool inOnboarding = true,
		PromptType type = PromptType.MultipleChoice
	)
	{
		this.Title = title;
		this.Options = options;
		this.SingleSelect = singleSelect;
		this.Required = required;
		this.InOnboarding = inOnboarding;
		this.Type = type;
	}

	/// <summary>
	/// Constructs a new onboarding prompt.
	/// </summary>
	internal DiscordOnboardingPrompt()
	{ }
}
