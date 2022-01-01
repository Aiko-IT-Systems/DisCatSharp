// This file is part of the DisCatSharp project, based off DSharpPlus.
//
// Copyright (c) 2021-2022 AITSYS
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Threading;
using System.Threading.Tasks;

using ConcurrentCollections;

using DisCatSharp.Common.Utilities;

namespace DisCatSharp.Interactivity.EventHandling
{
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
			this.Tcs = new TaskCompletionSource<bool>();
			this.Ct = new CancellationTokenSource(timeout);
			this.Predicate = predicate;
			this.Ct.Token.Register(() => this.Tcs.TrySetResult(true));
			this.Timeout = timeout;
			this.Collected = new ConcurrentHashSet<T>();
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
		}
	}
}


/*
              ^  ^
( Quack! )> (ﾐචᆽචﾐ)


(somewhere on twitter I read amazon had a duck
that said meow so I had to add a cat that says quack)

*/
