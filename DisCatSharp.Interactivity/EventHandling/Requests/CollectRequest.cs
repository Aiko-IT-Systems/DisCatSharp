using System;
using System.Threading;
using System.Threading.Tasks;

using ConcurrentCollections;

using DisCatSharp.Common.Utilities;

namespace DisCatSharp.Interactivity.EventHandling;

/// <summary>
/// CollectRequest is a class that serves as a representation of
/// EventArgs that are being collected within a specific time frame.
/// </summary>
/// <typeparam name="T"></typeparam>
internal class CollectRequest<T> : IDisposable where T : AsyncEventArgs
{
	internal TaskCompletionSource<bool> Tcs;
	internal CancellationTokenSource Ct;
	internal Func<T, bool> Predicate;
	internal TimeSpan Timeout;
	internal ConcurrentHashSet<T> Collected;

	/// <summary>
	/// Creates a new CollectRequest object.
	/// </summary>
	/// <param name="predicate">Predicate to match</param>
	/// <param name="timeout">Timeout time</param>
	public CollectRequest(Func<T, bool> predicate, TimeSpan timeout)
	{
		this.Tcs = new();
		this.Ct = new(timeout);
		this.Predicate = predicate;
		this.Ct.Token.Register(() => this.Tcs.TrySetResult(true));
		this.Timeout = timeout;
		this.Collected = [];
	}

	~CollectRequest()
	{
		this.Dispose();
	}

	/// <summary>
	/// Disposes this CollectRequest.
	/// </summary>
	public void Dispose()
	{
		this.Ct.Dispose();
		this.Tcs = null;
		this.Predicate = null;

		if (this.Collected != null)
		{
			this.Collected.Clear();
			this.Collected = null;
		}

		GC.SuppressFinalize(this);
	}
}

/*
              ^  ^
( Quack! )> (ﾐචᆽචﾐ)


(somewhere on twitter I read amazon had a duck
that said meow so I had to add a cat that says quack)

*/
