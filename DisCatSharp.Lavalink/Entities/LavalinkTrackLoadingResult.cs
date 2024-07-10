using System.Collections.Generic;

using DisCatSharp.Lavalink.Enums;

using Newtonsoft.Json;

namespace DisCatSharp.Lavalink.Entities;

/// <summary>
/// Represents a loading result when looking up tracks with <see cref="LavalinkSession.LoadTracksAsync(LavalinkSearchType, string)"/> or <see cref="LavalinkGuildPlayer.LoadTracksAsync(string)"/>.
/// </summary>
public sealed class LavalinkTrackLoadingResult
{
	/// <summary>
	/// Gets the load result type.
	/// </summary>
	[JsonProperty("loadType")]
	public LavalinkLoadResultType LoadType { get; internal set; }

	/// <summary>
	/// Gets the raw result string.
	/// </summary>
	[JsonProperty("data")]
	internal string RawResult { get; set; }

	/// <summary>
	/// Gets the load result.
	/// <para>You need to convert the <see langword="object"/> to the corresponding type based on <see cref="LoadType"/>.</para>
	/// <example>
	/// <code>var track = (LavalinkTrack)LavalinkTrackLoadingResult.Result;</code>
	/// </example>
	/// <list type="table">
	/// <listheader>
	///		<term><see cref="LoadType"/></term>
	///		<description>Convert to</description>
	/// </listheader>
	/// <item>
	///		<term><see cref="LavalinkLoadResultType.Track"/></term>
	///		<description><see cref="LavalinkTrack"/></description>
	/// </item>
	/// <item>
	///		<term><see cref="LavalinkLoadResultType.Playlist"/></term>
	///		<description><see cref="LavalinkPlaylist"/></description>
	/// </item>
	/// <item>
	///		<term><see cref="LavalinkLoadResultType.Search"/></term>
	///		<description><see cref="List{T}"/> of <see cref="LavalinkTrack"/></description>
	/// </item>
	/// <item>
	///		<term><see cref="LavalinkLoadResultType.Empty"/></term>
	///		<description><see langword="null"/></description>
	/// </item>
	/// <item>
	///		<term><see cref="LavalinkLoadResultType.Error"/></term>
	///		<description><see cref="LavalinkException"/></description>
	/// </item>
	/// </list>
	/// </summary>
	[JsonIgnore]
	public object Result =>
		this.LoadType switch
		{
			LavalinkLoadResultType.Track => LavalinkJson.DeserializeObject<LavalinkTrack>(this.RawResult!)!,
			LavalinkLoadResultType.Playlist => LavalinkJson.DeserializeObject<LavalinkPlaylist>(this.RawResult!)!,
			LavalinkLoadResultType.Search => LavalinkJson.DeserializeObject<List<LavalinkTrack>>(this.RawResult!)!,
			LavalinkLoadResultType.Empty => null!,
			LavalinkLoadResultType.Error => LavalinkJson.DeserializeObject<LavalinkException>(this.RawResult!)!,
			_ => this.RawResult!
		};

	/// <summary>
	/// Gets the result as a specific type.
	/// </summary>
	/// <typeparam name="T">Type to convert to.</typeparam>
	/// <returns>Result as the specified type.</returns>
	public T GetResultAs<T>()
		=> (T)this.Result;
}
