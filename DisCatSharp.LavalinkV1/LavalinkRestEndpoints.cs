
namespace DisCatSharp.Lavalink;

/// <summary>
/// Represents the lavalink endpoints.
/// </summary>
internal static class Endpoints
{
	/// <summary>
	/// The version endpoint.
	/// </summary>
	internal const string VERSION = "/version";

	//Track loading
	/// <summary>
	/// The load tracks endpoint.
	/// </summary>
	internal const string LOAD_TRACKS = "/loadtracks";
	/// <summary>
	/// The decode track endpoint.
	/// </summary>
	internal const string DECODE_TRACK = "/decodetrack";
	/// <summary>
	/// The decode tracks endpoint.
	/// </summary>
	internal const string DECODE_TRACKS = "/decodetracks";

	//Route Planner
	/// <summary>
	/// The route planner endpoint.
	/// </summary>
	internal const string ROUTE_PLANNER = "/routeplanner";
	/// <summary>
	/// The status endpoint.
	/// </summary>
	internal const string STATUS = "/status";
	/// <summary>
	/// The free address endpoint.
	/// </summary>
	internal const string FREE_ADDRESS = "/free/address";
	/// <summary>
	/// The free all endpoint.
	/// </summary>
	internal const string FREE_ALL = "/free/all";
}
