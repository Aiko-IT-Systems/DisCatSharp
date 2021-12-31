// This file is part of the DisCatSharp project, based off DSharpPlus.
//
// Copyright (c) 2021 AITSYS
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
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace DisCatSharp.Net.Udp
{
	/// <summary>
	/// The default, native-based UDP client implementation.
	/// </summary>
	internal class DCSUdpClient : BaseUdpClient
	{
		/// <summary>
		/// Gets the client.
		/// </summary>
		private UdpClient Client { get; set; }

		/// <summary>
		/// Gets the end point.
		/// </summary>
		private ConnectionEndpoint EndPoint { get; set; }

		/// <summary>
		/// Gets the packet queue.
		/// </summary>
		private BlockingCollection<byte[]> PacketQueue { get; }


		/// <summary>
		/// Gets the receiver task.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "<Pending>")]
		private Task ReceiverTask { get; set; }

		/// <summary>
		/// Gets the cancellation token source.
		/// </summary>
		private CancellationTokenSource TokenSource { get; }

		/// <summary>
		/// Gets the cancellation token.
		/// </summary>
		private CancellationToken Token => this.TokenSource.Token;

		/// <summary>
		/// Creates a new UDP client instance.
		/// </summary>
		public DCSUdpClient()
		{
			this.PacketQueue = new BlockingCollection<byte[]>();
			this.TokenSource = new CancellationTokenSource();
		}

		/// <summary>
		/// Configures the UDP client.
		/// </summary>
		/// <param name="endpoint">Endpoint that the client will be communicating with.</param>
		public override void Setup(ConnectionEndpoint endpoint)
		{
			this.EndPoint = endpoint;
			this.Client = new UdpClient();
			this.ReceiverTask = Task.Run(this.ReceiverLoopAsync, this.Token);
		}

		/// <summary>
		/// Sends a datagram.
		/// </summary>
		/// <param name="data">Datagram.</param>
		/// <param name="dataLength">Length of the datagram.</param>
		/// <returns></returns>
		public override Task SendAsync(byte[] data, int dataLength)
			=> this.Client.SendAsync(data, dataLength, this.EndPoint.Hostname, this.EndPoint.Port);

		/// <summary>
		/// Receives a datagram.
		/// </summary>
		/// <returns>The received bytes.</returns>
		public override Task<byte[]> ReceiveAsync() => Task.FromResult(this.PacketQueue.Take(this.Token));

		/// <summary>
		/// Closes and disposes the client.
		/// </summary>
		public override void Close()
		{
			this.TokenSource.Cancel();
#if !NETSTANDARD1_3
			try
			{ this.Client.Close(); }
			catch (Exception) { }
#endif

			// dequeue all the packets
			this.PacketQueue.Dispose();
		}

		/// <summary>
		/// Receivers the loop.
		/// </summary>
		private async Task ReceiverLoopAsync()
		{
			while (!this.Token.IsCancellationRequested)
			{
				try
				{
					var packet = await this.Client.ReceiveAsync().ConfigureAwait(false);
					this.PacketQueue.Add(packet.Buffer);
				}
				catch (Exception) { }
			}
		}

		/// <summary>
		/// Creates a new instance of <see cref="BaseUdpClient"/>.
		/// </summary>
		public static BaseUdpClient CreateNew()
			=> new DCSUdpClient();
	}
}
