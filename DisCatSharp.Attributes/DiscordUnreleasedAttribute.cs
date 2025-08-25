using System;

namespace DisCatSharp.Attributes
{
	/// <summary>
	///     Marks something as unreleased by discord.
	/// </summary>
	[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
	public sealed class DiscordUnreleasedAttribute : Attribute
	{
		public DiscordUnreleasedAttribute(string message)
		{
			this.Message = message;
		}

		public DiscordUnreleasedAttribute()
		{ }

		/// <summary>
		///     The additional information message.
		/// </summary>
		public string Message { get; set; }
	}
}
