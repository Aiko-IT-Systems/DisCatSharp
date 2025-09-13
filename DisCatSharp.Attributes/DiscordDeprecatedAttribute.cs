using System;

namespace DisCatSharp.Attributes
{
	/// <summary>
	///     Marks something as deprecated by discord.
	/// </summary>
	[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
	public sealed class DiscordDeprecatedAttribute : Attribute
	{
		public DiscordDeprecatedAttribute(string message)
		{
			this.Message = message;
		}

		public DiscordDeprecatedAttribute()
		{ }

		/// <summary>
		///     The additional information message.
		/// </summary>
		public string Message { get; set; }
	}
}
