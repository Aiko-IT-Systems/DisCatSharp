using System;

namespace DisCatSharp.Attributes
{
	/// <summary>
	///     Informs that something requires a certain feature to use it.
	/// </summary>
	[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
	public sealed class RequiresFeatureAttribute : Attribute
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="RequiresFeatureAttribute" /> class with specified features and a
		///     message.
		/// </summary>
		/// <param name="features">Specifies the required features.</param>
		/// <param name="message">Provides a message that describes the requirement or purpose of the attribute.</param>
		public RequiresFeatureAttribute(Features features, string message)
		{
			this.Features = features;
			this.Message = message;
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="RequiresFeatureAttribute" /> class.
		/// </summary>
		/// <param name="features">Specifies the required features.</param>
		public RequiresFeatureAttribute(Features features)
		{
			this.Features = features;
		}

		/// <summary>
		///     The additional information message.
		/// </summary>
		public string Message { get; set; }

		/// <summary>
		///     The required features.
		/// </summary>
		public Features Features { get; set; }
	}
}
