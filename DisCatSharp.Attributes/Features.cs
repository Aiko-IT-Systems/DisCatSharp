using System;

namespace DisCatSharp.Attributes;

/// <summary>
/// Features enum.
/// </summary>
[Flags]
public enum Features : long
{
	[FeatureDescription("Requires that you specify an override in the DiscordConfiguration.")]
	Override = 1 << 0,

	[FeatureDescription("Requires that the guild has onboarding enabled.")]
	Onboarding = 1 << 1,

	[FeatureDescription("Requires that the guild is partnered.")]
	Partnered = 1 << 2,

	[FeatureDescription("Requires that the guild is verified.")]
	Verified = 1 << 3,

	[FeatureDescription("Requires that the guild has discovery enabled.")]
	Discoverable = 1 << 4,

	[FeatureDescription("Requires that the guild has community enabled.")]
	Community = 1 << 5,

	[FeatureDescription("Requires that the guild has monetization enabled.")]
	ServerSubscription = 1 << 6,

	[FeatureDescription("Requires that the application has monetization enabled.")]
	MonetizedApplication = 1 << 7,

	[FeatureDescription("Requires that the user and/or guild has a specific experiment and/or treatment.")]
	Experiment = 1 << 8
}

/// <summary>
/// Defines a readable name for this feature requirement.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public sealed class FeatureDescriptionAttribute : Attribute
{
	/// <summary>
	/// Gets the readable name for this feature requirement.
	/// </summary>
	public string Description { get; }

	/// <summary>
	/// Defines a readable name for this feature requirement.
	/// </summary>
	/// <param name="description">Readable name for this feature requirement.</param>
	public FeatureDescriptionAttribute(string description)
	{
		this.Description = description;
	}
}
