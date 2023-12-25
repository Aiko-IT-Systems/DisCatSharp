using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace DisCatSharp.Net.Udp;

/// <summary>
/// The default, native-based UDP client implementation.
/// </summary>
internal class DcsUdpClient : BaseUdpClient
{
	/// <summary>
	/// Gets the client.
	/// </summary>
	private UdpClient _client;

	/// <summary>
	/// Gets the end point.
	/// </summary>
	private ConnectionEndpoint _endPoint;

	/// <summary>
	/// Gets the packet queue.
	/// </summary>
	private readonly BlockingCollection<byte[]> _packetQueue;

	/// <summary>
	/// Gets the receiver task.
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members",
		Justification = "<Pending>")]
	private Task _receiverTask;

	/// <summary>
	/// Gets the cancellation token source.
	/// </summary>
	private readonly CancellationTokenSource _tokenSource;

	/// <summary>
	/// Gets the cancellation token.
	/// </summary>
	private CancellationToken TOKEN => this._tokenSource.Token;

	/// <summary>
	/// Creates a new UDP client instance.
	/// </summary>
	public DcsUdpClient()
	{
		this._packetQueue = [];
		this._tokenSource = new();
	}

	/// <summary>
	/// Configures the UDP client.
	/// </summary>
	/// <param name="endpoint">Endpoint that the client will be communicating with.</param>
	public override void Setup(ConnectionEndpoint endpoint)
	{
		this._endPoint = endpoint;
		this._client = new();
		this._receiverTask = Task.Run(this.ReceiverLoopAsync, this.TOKEN);
	}

	/// <summary>
	/// Sends a datagram.
	/// </summary>
	/// <param name="data">Datagram.</param>
	/// <param name="dataLength">Length of the datagram.</param>
	/// <returns></returns>
	public override Task SendAsync(byte[] data, int dataLength)
		=> this._client.SendAsync(data, dataLength, this._endPoint.Hostname, this._endPoint.Port);

	/// <summary>
	/// Receives a datagram.
	/// </summary>
	/// <returns>The received bytes.</returns>
	public override Task<byte[]> ReceiveAsync() => Task.FromResult(this._packetQueue.Take(this.TOKEN));

	/// <summary>
	/// Closes and disposes the client.
	/// </summary>
	public override void Close()
	{
		this._tokenSource.Cancel();
		try
		{
			this._client.Close();
		}
		catch (Exception)
		{
			// Nothing
		}

		// dequeue all the packets
		this._packetQueue.Dispose();
	}

	/// <summary>
	/// Receivers the loop.
	/// </summary>
	private async Task ReceiverLoopAsync()
	{
		while (!this.TOKEN.IsCancellationRequested)
			try
			{
				var packet = await this._client.ReceiveAsync(this.TOKEN).ConfigureAwait(false);
				this._packetQueue.Add(packet.Buffer, this.TOKEN);
			}
			catch (Exception)
			{ }
	}

	/// <summary>
	/// Creates a new instance of <see cref="BaseUdpClient"/>.
	/// </summary>
	public static BaseUdpClient CreateNew()
		=> new DcsUdpClient();
}
