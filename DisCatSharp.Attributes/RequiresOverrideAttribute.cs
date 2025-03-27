using System;

namespace DisCatSharp.Attributes
{
	/// <summary>
	///     Informs that something requires a certain override to use it.
	/// </summary>
	[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
	public sealed class RequiresOverrideAttribute : Attribute
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="RequiresOverrideAttribute" /> class with specified override
		///     information.
		/// </summary>
		/// <param name="lastKnownOverride">Specifies the last known working override value.</param>
		/// <param name="overrideDate">Specifies the date of the override.</param>
		/// <param name="message">Provides a message associated with the attribute for additional context.</param>
		public RequiresOverrideAttribute(string lastKnownOverride, string overrideDate, string message)
		{
			this.LastKnownOverride = lastKnownOverride;
			this.OverrideDate = overrideDate;
			this.Message = message;
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="RequiresOverrideAttribute" /> class with a specified override value.
		/// </summary>
		/// <param name="lastKnownOverride">Specifies the last known working override value.</param>
		/// <param name="overrideDate">Specifies the date of the override.</param>
		public RequiresOverrideAttribute(string lastKnownOverride, string overrideDate)
		{
			this.LastKnownOverride = lastKnownOverride;
			this.OverrideDate = overrideDate;
		}

		/// <summary>
		///     The additional information message.
		/// </summary>
		public string Message { get; set; }

		/// <summary>
		///     The last known working override.
		/// </summary>
		public string LastKnownOverride { get; set; }

		/// <summary>
		///     The date of the override.
		/// </summary>
		public string OverrideDate { get; set; }
	}
}
