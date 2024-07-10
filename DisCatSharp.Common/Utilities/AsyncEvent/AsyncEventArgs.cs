using System;

namespace DisCatSharp.Common.Utilities;

/// <summary>
/// Contains arguments passed to an asynchronous event.
/// </summary>
public class AsyncEventArgs : EventArgs
{
	/// <summary>
	/// <para>Gets or sets whether this event was handled.</para>
	/// <para>Setting this to true will prevent other handlers from running.</para>
	/// </summary>
	public bool Handled { get; set; } = false;
}
