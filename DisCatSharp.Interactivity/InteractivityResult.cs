namespace DisCatSharp.Interactivity;

/// <summary>
/// Interactivity result
/// </summary>
/// <typeparam name="T">Type of result</typeparam>
public readonly struct InteractivityResult<T>
{
	/// <summary>
	/// Whether interactivity was timed out
	/// </summary>
	public bool TimedOut { get; }

	/// <summary>
	/// Result
	/// </summary>
	public T Result { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="InteractivityResult{T}"/> class.
	/// </summary>
	/// <param name="timedOut">If true, timed out.</param>
	/// <param name="result">The result.</param>
	internal InteractivityResult(bool timedOut, T result)
	{
		this.TimedOut = timedOut;
		this.Result = result;
	}
}
