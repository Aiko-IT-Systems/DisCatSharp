using System.Net;

namespace DisCatSharp.Net;

/// <summary>
/// Represents a network connection IP endpoint.
/// </summary>
public readonly struct IpEndpoint
{
	/// <summary>
	/// Gets or sets the hostname associated with this endpoint.
	/// </summary>
	public IPAddress Address { get; init; }

	/// <summary>
	/// Gets or sets the port associated with this endpoint.
	/// </summary>
	public int Port { get; init; }

	/// <summary>
	/// Creates a new IP endpoint structure.
	/// </summary>
	/// <param name="address">IP address to connect to.</param>
	/// <param name="port">Port to use for connection.</param>
	public IpEndpoint(IPAddress address, int port)
	{
		this.Address = address;
		this.Port = port;
	}
}
