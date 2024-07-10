using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

using DisCatSharp.Common.Utilities;
using DisCatSharp.EventArgs;

namespace DisCatSharp.Net.WebSocket;

// weebsocket
// not even sure whether emzi or I posted this. much love, naam.
/// <summary>
/// The default, native-based WebSocket client implementation.
/// </summary>
public class WebSocketClient : IWebSocketClient
{
	/// <summary>
	/// The outgoing chunk size.
	/// </summary>
	private const int OUTGOING_CHUNK_SIZE = 8192; // 8 KiB

	/// <summary>
	/// The incoming chunk size.
	/// </summary>
	private const int INCOMING_CHUNK_SIZE = 32768; // 32 KiB

	/// <summary>
	/// Gets the proxy settings for this client.
	/// </summary>
	public IWebProxy Proxy { get; }

	/// <summary>
	/// Gets the collection of default headers to send when connecting to the remote endpoint.
	/// </summary>
	public IReadOnlyDictionary<string, string> DefaultHeaders { get; }

	/// <summary>
	/// Gets or sets the service provider.
	/// </summary>
	IServiceProvider IWebSocketClient.ServiceProvider
	{
		get => this._serviceProvider;
		set => this._serviceProvider = value;
	}

	/// <summary>
	/// Gets the default headers.
	/// </summary>
	private readonly Dictionary<string, string> _defaultHeaders;

	/// <summary>
	/// Gets the receiver task.
	/// </summary>
	private Task? _receiverTask;

	/// <summary>
	/// Gets the receiver token source, if any.
	/// </summary>
	private CancellationTokenSource? _receiverTokenSource;

	/// <summary>
	/// Gets the receiver token.
	/// </summary>
	private CancellationToken _receiverToken;

	/// <summary>
	/// Gets the sender lock.
	/// </summary>
	private readonly SemaphoreSlim _senderLock;

	/// <summary>
	/// Gets the socket token source, if any.
	/// </summary>
	private CancellationTokenSource? _socketTokenSource;

	/// <summary>
	/// Gets the socket token.
	/// </summary>
	private CancellationToken _socketToken;

	/// <summary>
	/// Gets the WebSocket instance.
	/// </summary>
	private ClientWebSocket? _ws;

	/// <summary>
	/// Gets a value indicating whether this client is closed.
	/// </summary>
	private volatile bool _isClientClose;

	/// <summary>
	/// Gets a value indicating whether this client is connected.
	/// </summary>
	private volatile bool _isConnected;

	/// <summary>
	/// Gets a value indicating whether this client is disposed.
	/// </summary>
	private bool _isDisposed;

	/// <summary>
	/// Instantiates a new WebSocket client with specified proxy settings.
	/// </summary>
	/// <param name="proxy">Proxy settings for the client.</param>
	/// <param name="provider">Service provider.</param>
	private WebSocketClient(IWebProxy proxy, IServiceProvider provider)
	{
		this._connected = new("WS_CONNECT", TimeSpan.Zero, this.EventErrorHandler);
		this._disconnected = new("WS_DISCONNECT", TimeSpan.Zero, this.EventErrorHandler);
		this._messageReceived = new("WS_MESSAGE", TimeSpan.Zero, this.EventErrorHandler);
		this._exceptionThrown = new("WS_ERROR", TimeSpan.Zero, null);

		this.Proxy = proxy;
		this._defaultHeaders = [];
		this.DefaultHeaders = new ReadOnlyDictionary<string, string>(this._defaultHeaders);

		this._receiverTokenSource = null;
		this._receiverToken = CancellationToken.None;
		this._senderLock = new(1);

		this._socketTokenSource = null;
		this._socketToken = CancellationToken.None;

		this._serviceProvider = provider;
	}

	/// <summary>
	/// Connects to a specified remote WebSocket endpoint.
	/// </summary>
	/// <param name="uri">The URI of the WebSocket endpoint.</param>
	public async Task ConnectAsync(Uri uri)
	{
		// Disconnect first
		try
		{
			await this.DisconnectAsync().ConfigureAwait(false);
		}
		catch
		{ }

		// Disallow sending messages
		await this._senderLock.WaitAsync(CancellationToken.None).ConfigureAwait(false);

		try
		{
			// This can be null at this point
			this._receiverTokenSource?.Dispose();
			this._socketTokenSource?.Dispose();

			this._ws?.Dispose();
			this._ws = new();
			this._ws.Options.Proxy = this.Proxy;
			this._ws.Options.KeepAliveInterval = TimeSpan.Zero;
			if (this._defaultHeaders.Count is not 0)
				foreach (var (k, v) in this._defaultHeaders)
					this._ws.Options.SetRequestHeader(k, v);

			this._receiverTokenSource = new();
			this._receiverToken = this._receiverTokenSource.Token;

			this._socketTokenSource = new();
			this._socketToken = this._socketTokenSource.Token;

			this._isClientClose = false;
			this._isDisposed = false;
			await this._ws.ConnectAsync(uri, this._socketToken).ConfigureAwait(false);
			this._receiverTask = Task.Run(this.ReceiverLoopAsync, this._receiverToken);
		}
		finally
		{
			this._senderLock.Release();
		}
	}

	/// <summary>
	/// Disconnects the WebSocket connection.
	/// </summary>
	/// <param name="code">The code</param>
	/// <param name="message">The message</param>
	public async Task DisconnectAsync(int code = 1000, string? message = null)
	{
		// Ensure that messages cannot be sent
		await this._senderLock.WaitAsync(CancellationToken.None).ConfigureAwait(false);

		try
		{
			this._isClientClose = true;
			if (this._ws is { State: WebSocketState.Open or WebSocketState.CloseReceived })
				await this._ws.CloseOutputAsync((WebSocketCloseStatus)code, message, CancellationToken.None).ConfigureAwait(false);

			if (this._receiverTask != null)
				await this._receiverTask.ConfigureAwait(false); // Ensure that receiving completed

			if (this._isConnected)
				this._isConnected = false;

			if (!this._isDisposed)
			{
				// Cancel all running tasks
				if (this._socketToken.CanBeCanceled && this._socketTokenSource is not null)
					this._socketTokenSource.Cancel();
				this._socketTokenSource?.Dispose();

				if (this._receiverToken.CanBeCanceled && this._receiverTokenSource is not null)
					this._receiverTokenSource.Cancel();
				this._receiverTokenSource?.Dispose();

				this._isDisposed = true;
			}
		}
		catch
		{ }
		finally
		{
			this._senderLock.Release();
		}
	}

	/// <summary>
	/// Send a message to the WebSocket server.
	/// </summary>
	/// <param name="message">The message to send.</param>
	public async Task SendMessageAsync(string message)
	{
		if (this._ws is null)
			return;

		if (this._ws.State is not WebSocketState.Open && this._ws.State is not WebSocketState.CloseReceived)
			return;

		var bytes = Utilities.UTF8.GetBytes(message);
		await this._senderLock.WaitAsync(CancellationToken.None).ConfigureAwait(false);
		try
		{
			var len = bytes.Length;
			var segCount = len / OUTGOING_CHUNK_SIZE;
			if (len % OUTGOING_CHUNK_SIZE != 0)
				segCount++;

			for (var i = 0; i < segCount; i++)
			{
				var segStart = OUTGOING_CHUNK_SIZE * i;
				var segLen = Math.Min(OUTGOING_CHUNK_SIZE, len - segStart);

				await this._ws.SendAsync(new(bytes, segStart, segLen), WebSocketMessageType.Text, i == segCount - 1, CancellationToken.None).ConfigureAwait(false);
			}
		}
		finally
		{
			this._senderLock.Release();
		}
	}

	/// <summary>
	/// Adds a header to the default header collection.
	/// </summary>
	/// <param name="name">Name of the header to add.</param>
	/// <param name="value">Value of the header to add.</param>
	/// <returns>Whether the operation succeeded.</returns>
	public bool AddDefaultHeader(string name, string value)
	{
		this._defaultHeaders[name] = value;
		return true;
	}

	/// <summary>
	/// Removes a header from the default header collection.
	/// </summary>
	/// <param name="name">Name of the header to remove.</param>
	/// <returns>Whether the operation succeeded.</returns>
	public bool RemoveDefaultHeader(string name)
		=> this._defaultHeaders.Remove(name);

	/// <summary>
	/// Disposes of resources used by this WebSocket client instance.
	/// </summary>
	public void Dispose()
	{
		if (this._isDisposed)
			return;

		this._isDisposed = true;

		this.DisconnectAsync().ConfigureAwait(false).GetAwaiter().GetResult();

		this._receiverTokenSource?.Dispose();
		this._socketTokenSource?.Dispose();
		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// Receivers the loop.
	/// </summary>
	internal async Task ReceiverLoopAsync()
	{
		await Task.Yield();

		var token = this._receiverToken;
		var buffer = new ArraySegment<byte>(new byte[INCOMING_CHUNK_SIZE]);

		try
		{
			using var bs = new MemoryStream();
			while (!token.IsCancellationRequested)
			{
				// See https://github.com/RogueException/Discord.Net/commit/ac389f5f6823e3a720aedd81b7805adbdd78b66d
				// for explanation on the cancellation token

				WebSocketReceiveResult result;
				if (this._ws is null)
					throw new NullReferenceException();

				do
				{
					result = await this._ws.ReceiveAsync(buffer, CancellationToken.None).ConfigureAwait(false);

					if (result.MessageType is WebSocketMessageType.Close)
						break;

					await bs.WriteAsync(buffer.Array!, 0, result.Count, this._receiverToken);
				}
				while (!result.EndOfMessage);

				var resultBytes = new byte[bs.Length];
				bs.Position = 0;
				_ = await bs.ReadAsync(resultBytes, 0, resultBytes.Length, this._receiverToken);
				bs.Position = 0;
				bs.SetLength(0);

				if (!this._isConnected && result.MessageType is not WebSocketMessageType.Close)
				{
					this._isConnected = true;
					await this._connected.InvokeAsync(this, new(this._serviceProvider)).ConfigureAwait(false);
				}

				if (result.MessageType is WebSocketMessageType.Binary)
					await this._messageReceived.InvokeAsync(this, new SocketBinaryMessageEventArgs(resultBytes)).ConfigureAwait(false);
				else if (result.MessageType is WebSocketMessageType.Text)
					await this._messageReceived.InvokeAsync(this, new SocketTextMessageEventArgs(Utilities.UTF8.GetString(resultBytes))).ConfigureAwait(false);
				else // close
				{
					if (!this._isClientClose)
					{
						var code = result.CloseStatus!.Value;
						code = code is WebSocketCloseStatus.NormalClosure or WebSocketCloseStatus.EndpointUnavailable
							? (WebSocketCloseStatus)4000
							: code;

						await this._ws.CloseOutputAsync(code, result.CloseStatusDescription, CancellationToken.None).ConfigureAwait(false);
					}

					await this._disconnected.InvokeAsync(this, new(this._serviceProvider)
					{
						CloseCode = (int)result.CloseStatus!,
						CloseMessage = result.CloseStatusDescription
					}).ConfigureAwait(false);
					break;
				}
			}
		}
		catch (Exception ex)
		{
			await this._exceptionThrown.InvokeAsync(this, new(this._serviceProvider)
			{
				Exception = ex
			}).ConfigureAwait(false);
			await this._disconnected.InvokeAsync(this, new(this._serviceProvider)
			{
				CloseCode = -1,
				CloseMessage = ""
			}).ConfigureAwait(false);
		}

		// Don't await or you deadlock
		// DisconnectAsync waits for this method
		_ = this.DisconnectAsync().ConfigureAwait(false);
	}

	/// <summary>
	/// Creates a new instance of <see cref="WebSocketClient"/>.
	/// </summary>
	/// <param name="proxy">Proxy to use for this client instance.</param>
	/// <param name="provider">Service provider.</param>
	/// <returns>An instance of <see cref="WebSocketClient"/>.</returns>
	public static IWebSocketClient CreateNew(IWebProxy proxy, IServiceProvider provider)
		=> new WebSocketClient(proxy, provider);

#region Events

	/// <summary>
	/// Triggered when the client connects successfully.
	/// </summary>
	public event AsyncEventHandler<IWebSocketClient, SocketEventArgs> Connected
	{
		add => this._connected.Register(value);
		remove => this._connected.Unregister(value);
	}

	private readonly AsyncEvent<WebSocketClient, SocketEventArgs> _connected;

	/// <summary>
	/// Triggered when the client is disconnected.
	/// </summary>
	public event AsyncEventHandler<IWebSocketClient, SocketCloseEventArgs> Disconnected
	{
		add => this._disconnected.Register(value);
		remove => this._disconnected.Unregister(value);
	}

	private readonly AsyncEvent<WebSocketClient, SocketCloseEventArgs> _disconnected;

	/// <summary>
	/// Triggered when the client receives a message from the remote party.
	/// </summary>
	public event AsyncEventHandler<IWebSocketClient, SocketMessageEventArgs> MessageReceived
	{
		add => this._messageReceived.Register(value);
		remove => this._messageReceived.Unregister(value);
	}

	private readonly AsyncEvent<WebSocketClient, SocketMessageEventArgs> _messageReceived;

	/// <summary>
	/// Triggered when an error occurs in the client.
	/// </summary>
	public event AsyncEventHandler<IWebSocketClient, SocketErrorEventArgs> ExceptionThrown
	{
		add => this._exceptionThrown.Register(value);
		remove => this._exceptionThrown.Unregister(value);
	}

	private readonly AsyncEvent<WebSocketClient, SocketErrorEventArgs> _exceptionThrown;
	private IServiceProvider _serviceProvider;

	/// <summary>
	/// Events the error handler.
	/// </summary>
	/// <param name="asyncEvent">The event.</param>
	/// <param name="ex">The exception.</param>
	/// <param name="handler">The handler.</param>
	/// <param name="sender">The sender.</param>
	/// <param name="eventArgs">The event args.</param>
	private void EventErrorHandler<TArgs>(AsyncEvent<WebSocketClient, TArgs> asyncEvent, Exception ex, AsyncEventHandler<WebSocketClient, TArgs> handler, WebSocketClient sender, TArgs eventArgs)
		where TArgs : AsyncEventArgs
		=> this._exceptionThrown.InvokeAsync(this, new(this._serviceProvider)
		{
			Exception = ex
		}).ConfigureAwait(false).GetAwaiter().GetResult();

#endregion
}
