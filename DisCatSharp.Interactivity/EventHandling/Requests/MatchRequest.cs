using System;
using System.Threading;
using System.Threading.Tasks;

using DisCatSharp.Common.Utilities;

namespace DisCatSharp.Interactivity.EventHandling;

/// <summary>
/// MatchRequest is a class that serves as a representation of a
/// match that is being waited for.
/// </summary>
/// <typeparam name="T"></typeparam>
internal class MatchRequest<T> : IDisposable where T : AsyncEventArgs
{
	internal TaskCompletionSource<T> Tcs;
	internal CancellationTokenSource Ct;
	internal Func<T, bool> Predicate;
	internal TimeSpan Timeout;

	/// <summary>
	/// Creates a new MatchRequest object.
	/// </summary>
	/// <param name="predicate">Predicate to match</param>
	/// <param name="timeout">Timeout time</param>
	public MatchRequest(Func<T, bool> predicate, TimeSpan timeout)
	{
		this.Tcs = new();
		this.Ct = new(timeout);
		this.Predicate = predicate;
		this.Ct.Token.Register(() => this.Tcs.TrySetResult(null));
		this.Timeout = timeout;
	}

	~MatchRequest()
	{
		this.Dispose();
	}

	/// <summary>
	/// Disposes this MatchRequest.
	/// </summary>
	public void Dispose()
	{
		this.Ct.Dispose();
		this.Tcs = null;
		this.Predicate = null;
		GC.SuppressFinalize(this);
	}
}
