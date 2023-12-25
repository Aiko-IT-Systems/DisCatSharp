namespace DisCatSharp.Net;

/// <summary>
/// Represents a network connection endpoint.
/// </summary>
public readonly struct ConnectionEndpoint
{
	/// <summary>
	/// Gets or sets the hostname associated with this endpoint.
	/// </summary>
	public string Hostname { get; init; }

	/// <summary>
	/// Gets or sets the port associated with this endpoint.
	/// </summary>
	public int Port { get; init; }

	/// <summary>
	/// Gets or sets the secured status of this connection.
	/// </summary>
	public bool Secured { get; init; }

	/// <summary>
	/// Creates a new endpoint structure.
	/// </summary>
	/// <param name="hostname">Hostname to connect to.</param>
	/// <param name="port">Port to use for connection.</param>
	/// <param name="secured">Whether the connection should be secured (https/wss).</param>
	public ConnectionEndpoint(string hostname, int port, bool secured = false)
	{
		this.Hostname = hostname;
		this.Port = port;
		this.Secured = secured;
	}

	/// <summary>
	/// Gets the hash code of this endpoint.
	/// </summary>
	/// <returns>Hash code of this endpoint.</returns>
	public readonly override int GetHashCode()
		=> 13 + (7 * this.Hostname.GetHashCode()) + (7 * this.Port);

	/// <summary>
	/// Gets the string representation of this connection endpoint.
	/// </summary>
	/// <returns>String representation of this endpoint.</returns>
	public readonly override string ToString()
		=> $"{this.Hostname}:{this.Port}";

	/// <summary>
	/// Returns a http string.
	/// </summary>
	internal readonly string ToHttpString()
	{
		var secure = this.Secured ? "s" : "";
		return $"http{secure}://{this}";
	}

	/// <summary>
	/// Returns a web socket string.
	/// </summary>
	internal readonly string ToWebSocketString()
	{
		var secure = this.Secured ? "s" : "";
		return $"ws{secure}://{this}";
	}
}
