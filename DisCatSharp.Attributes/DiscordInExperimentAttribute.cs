using System;

namespace DisCatSharp.Attributes
{
	/// <summary>
	///     Marks something as in experiment by discord.
	/// </summary>
	[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
	public sealed class DiscordInExperimentAttribute : Attribute
	{
		public DiscordInExperimentAttribute(string message)
		{
			this.Message = message;
		}

		public DiscordInExperimentAttribute()
		{ }

		/// <summary>
		///     The additional information message.
		/// </summary>
		public string Message { get; set; }
	}
}
