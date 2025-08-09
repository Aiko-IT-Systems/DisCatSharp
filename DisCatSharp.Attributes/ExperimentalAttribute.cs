using System;

namespace DisCatSharp.Attributes
{
	/// <summary>
	///     Marks something as experimental by DisCatSharp.
	/// </summary>
	[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
	public sealed class ExperimentalAttribute : Attribute
	{
		public ExperimentalAttribute(string message)
		{
			this.Message = message;
		}

		public ExperimentalAttribute()
		{ }

		/// <summary>
		///     The additional information message.
		/// </summary>
		public string Message { get; set; }
	}
}
