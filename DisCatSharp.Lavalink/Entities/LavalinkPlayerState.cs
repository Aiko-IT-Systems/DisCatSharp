using System;

using Newtonsoft.Json;

namespace DisCatSharp.Lavalink.Entities;

/// <summary>
/// Represents a lavalink player state.
/// </summary>
public sealed class LavalinkPlayerState
{
	/// <summary>
	/// Gets the current datetime offset.
	/// </summary>
	[JsonIgnore]
	public DateTimeOffset Time => Utilities.GetDateTimeOffsetFromMilliseconds(this._time);

	/// <summary>
	/// Gets the unix timestamp in milliseconds.
	/// </summary>
	[JsonProperty("time")]
#pragma warning disable CS0649 // Field 'LavalinkPlayerState._time' is never assigned to, and will always have its default value 0
	private readonly long _time;
#pragma warning restore CS0649 // Field 'LavalinkPlayerState._time' is never assigned to, and will always have its default value 0

	/// <summary>
	/// Gets the position of the track as <see cref="TimeSpan"/>.
	/// </summary>
	[JsonIgnore]
	public TimeSpan Position => TimeSpan.FromMilliseconds(this._position);

	/// <summary>
	/// Gets the position of the track in milliseconds.
	/// </summary>
	[JsonProperty("position")]
#pragma warning disable CS0649 // Field 'LavalinkPlayerState._position' is never assigned to, and will always have its default value 0
	private readonly long _position;
#pragma warning restore CS0649 // Field 'LavalinkPlayerState._position' is never assigned to, and will always have its default value 0

	/// <summary>
	/// Gets whether Lavalink is connected to the voice gateway.
	/// </summary>
	[JsonProperty("connected")]
	public bool IsConnected { get; internal set; }

	/// <summary>
	/// Gets the ping of the node to the Discord voice server in milliseconds (<c>-1</c> if not connected).
	/// </summary>
	[JsonProperty("ping")]
	public int Ping { get; internal set; }
}
