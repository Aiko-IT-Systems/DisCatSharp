using System;

namespace DisCatSharp.Attributes;

/// <summary>
/// Informs that something requires a certain feature to use it.
/// </summary>
[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
public sealed class RequiresFeatureAttribute : Attribute
{
	/// <summary>
	/// The additional information message.
	/// </summary>
	public string Message { get; set; }

	public Features Features { get; set; }

	public RequiresFeatureAttribute(Features features, string message)
	{
		this.Features = features;
		this.Message = message;
	}

	public RequiresFeatureAttribute(Features features)
	{
		this.Features = features;
	}
}
