using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using DisCatSharp.Common.RegularExpressions;
using DisCatSharp.Entities;
using DisCatSharp.Lavalink.Entities;
using DisCatSharp.Lavalink.Entities.Filters;
using DisCatSharp.Lavalink.Enums;
using DisCatSharp.Lavalink.Exceptions;
using DisCatSharp.Lavalink.Payloads;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json.Linq;

namespace DisCatSharp.Lavalink;

/// <summary>
/// Provides various rest methods to interact with the Lavalink API.
/// </summary>
internal sealed class LavalinkRestClient
{
	/// <summary>
	/// Gets the discord client.
	/// </summary>
	internal DiscordClient Discord { get; }

	/// <summary>
	/// Gets the http client.
	/// </summary>
	internal HttpClient HttpClient { get; }

	/// <summary>
	/// Gets whether trace is enabled.
	/// </summary>
	private bool TRACE_ENABLED { get; } = false;

	/// <summary>
	/// Constructs a new <see cref="LavalinkRestClient"/>.
	/// </summary>
	/// <param name="configuration">The lavalink configuration to use.</param>
	/// <param name="client">The discord client this rest client is attached to.</param>
	internal LavalinkRestClient(LavalinkConfiguration configuration, DiscordClient client)
	{
		this.Discord = client;

		var httpHandler = new HttpClientHandler
		{
			UseCookies = false,
			AutomaticDecompression = DecompressionMethods.All,
			UseProxy = configuration.Proxy != null!,
			Proxy = configuration.Proxy
		};

		this.HttpClient = new(httpHandler)
		{
			BaseAddress = new($"{configuration.RestEndpoint.ToHttpString()}"),
			Timeout = configuration.HttpTimeout
		};

		this.TRACE_ENABLED = configuration.EnableTrace;
		this.HttpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", $"DisCatSharp.Lavalink/{this.Discord.VersionString}");
		this.HttpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", configuration.Password);
		if (this.Discord.CurrentUser != null!)
			this.HttpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Id", this.Discord.CurrentUser.Id.ToString(CultureInfo.InvariantCulture));
	}

	/// <summary>
	/// Builds the query string.
	/// </summary>
	/// <param name="values">The values.</param>
	/// <param name="post">Whether this query will be transmitted via POST.</param>
	private static string BuildQueryString(IDictionary<string, string> values, bool post = false)
	{
		if (values == null! || values.Count == 0)
			return string.Empty;

		var valuesCollection = values.Select(xkvp =>
			$"{WebUtility.UrlEncode(xkvp.Key)}={WebUtility.UrlEncode(xkvp.Value)}");
		var valuesString = string.Join("&", valuesCollection);

		return !post ? $"?{valuesString}" : valuesString;
	}

	/// <summary>
	/// Gets the path with its <paramref name="routeParams"/> applied.
	/// </summary>
	/// <param name="route">The route.</param>
	/// <param name="routeParams">The route params.</param>
	/// <returns>The generated route.</returns>
	private static string GetPath(string route, object routeParams)
	{
		if (routeParams == null!)
			return route;

		var routeParamsProperties = routeParams.GetType()
			.GetTypeInfo()
			.DeclaredProperties;
		var extractedRouteParams = new Dictionary<string, string>();
		foreach (var xp in routeParamsProperties)
		{
			var val = xp.GetValue(routeParams);
			extractedRouteParams[xp.Name] = val is string xs
				? xs
				: val is DateTime dt
					? dt.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture)
					: val is DateTimeOffset dto
						? dto.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture)
						: val is IFormattable xf
							? xf.ToString(null, CultureInfo.InvariantCulture)
							: val.ToString();
		}

		return CommonRegEx.HttpRouteRegex().Replace(route, xm => extractedRouteParams[xm.Groups[1].Value]);
	}

	private Dictionary<string, string> GetDefaultParams()
		=> new()
		{
			["trace"] = this.TRACE_ENABLED.ToString().ToLower()
		};

	/// <summary>
	/// Executes a rest request.
	/// </summary>
	/// <param name="method">The method.</param>
	/// <param name="path">The path.</param>
	/// <param name="headers">The headers.</param>
	/// <param name="payload">The payload.</param>
	private async Task<LavalinkRestResponse> DoRequestAsync(HttpMethod method, string path, IReadOnlyDictionary<string, string>? headers = null, string? payload = null)
	{
		HttpRequestMessage request = new();
		if (headers != null)
			foreach (var header in headers)
				request.Headers.TryAddWithoutValidation(header.Key, header.Value);
		if (payload != null)
		{
			request.Content = new StringContent(payload);
			request.Content.Headers.ContentType = new("application/json");
		}

		request.Method = method;
		request.RequestUri = new(this.HttpClient.BaseAddress!, path);
		var response = await this.HttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead).ConfigureAwait(false);

		if (!response.IsSuccessStatusCode)
		{
			var data = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
			var ex = LavalinkJson.DeserializeObject<LavalinkRestException>(data)!;
			ex.Headers = response.Headers;
			ex.Json = data;
			this.Discord.Logger.LogError(LavalinkEvents.LavalinkRestError, ex, "Lavalink rest encountered an exception: {data}", data);
			throw ex;
		}

		var res = new LavalinkRestResponse()
		{
			ResponseCode = response.StatusCode,
			Response = response.StatusCode != HttpStatusCode.NoContent ? await response.Content.ReadAsStringAsync().ConfigureAwait(false) : null,
			Headers = response.Headers
		};

		return res;
	}

	/// <summary>
	/// Requests Lavalink server version.
	/// </summary>
	/// <returns>The version <see cref="string"/>.</returns>
	internal async Task<LavalinkRestResponse> GetVersionAsync()
	{
		var queryDict = this.GetDefaultParams();
		var route = $"{Endpoints.VERSION}";
		var path = GetPath(route, new
			{ });
		return await this.DoRequestAsync(HttpMethod.Get, $"{path}{BuildQueryString(queryDict)}").ConfigureAwait(false);
	}

	/// <summary>
	/// Requests Lavalink server information.
	/// </summary>
	/// <returns>A <see cref="LavalinkInfo"/> object.</returns>
	internal async Task<LavalinkInfo> GetInfoAsync()
	{
		var queryDict = this.GetDefaultParams();
		var route = $"{Endpoints.V4}{Endpoints.INFO}";
		var path = GetPath(route, new
			{ });
		var res = await this.DoRequestAsync(HttpMethod.Get, $"{path}{BuildQueryString(queryDict)}").ConfigureAwait(false);
		return LavalinkJson.DeserializeObject<LavalinkInfo>(res.Response!)!;
	}

	/// <summary>
	/// Requests Lavalink server statistics.
	/// </summary>
	/// <returns>A <see cref="LavalinkStats"/> object.</returns>
	internal async Task<LavalinkStats> GetStatsAsync()
	{
		var queryDict = this.GetDefaultParams();
		var route = $"{Endpoints.V4}{Endpoints.STATS}";
		var path = GetPath(route, new
			{ });
		var res = await this.DoRequestAsync(HttpMethod.Get, $"{path}{BuildQueryString(queryDict)}").ConfigureAwait(false);
		return LavalinkJson.DeserializeObject<LavalinkStats>(res.Response!)!;
	}

	/// <summary>
	/// Updates a session with the resuming state and timeout.
	/// </summary>
	/// <param name="sessionId">The session id.</param>
	/// <param name="config">The session config.</param>
	/// <returns>The updated <see cref="LavalinkSessionConfiguration"/>.</returns>
	internal async Task<LavalinkSessionConfiguration> UpdateSessionAsync(string sessionId, LavalinkSessionConfiguration config)
	{
		var queryDict = this.GetDefaultParams();
		var route = $"{Endpoints.V4}{Endpoints.SESSIONS}/:session_id";
		var path = GetPath(route, new
		{
			session_id = sessionId
		});
		var res = await this.DoRequestAsync(HttpMethod.Patch, $"{path}{BuildQueryString(queryDict)}", payload: LavalinkJson.SerializeObject(config)).ConfigureAwait(false);
		return LavalinkJson.DeserializeObject<LavalinkSessionConfiguration>(res.Response!)!;
	}

	/// <summary>
	/// Returns a list of players for the given session id.
	/// </summary>
	/// <param name="sessionId">The session id.</param>
	/// <returns>A <see cref="IReadOnlyList{T}"/> of <see cref="LavalinkPlayer"/>s.</returns>
	internal async Task<IReadOnlyList<LavalinkPlayer>> GetPlayersAsync(string sessionId)
	{
		var queryDict = this.GetDefaultParams();
		var route = $"{Endpoints.V4}{Endpoints.SESSIONS}/:session_id{Endpoints.PLAYERS}";
		var path = GetPath(route, new
		{
			session_id = sessionId
		});
		var res = await this.DoRequestAsync(HttpMethod.Get, $"{path}{BuildQueryString(queryDict)}").ConfigureAwait(false);
		return LavalinkJson.DeserializeObject<List<LavalinkPlayer>>(res.Response!)!;
	}

	/// <summary>
	/// Creates a player.
	/// </summary>
	/// <param name="sessionId">The session id.</param>
	/// <param name="guildId">The guild id this player should be created for.</param>
	/// <param name="defaultVolume">The default volume level which should be used.</param>
	internal async Task CreatePlayerAsync(string sessionId, ulong guildId, int defaultVolume)
	{
		var queryDict = this.GetDefaultParams();
		queryDict.Add("noReplace", "false");
		var pld = new LavalinkRestPlayerCreatePayload(guildId.ToString(), defaultVolume);
		var route = $"{Endpoints.V4}{Endpoints.SESSIONS}/:session_id{Endpoints.PLAYERS}/:guild_id";
		var path = GetPath(route, new
		{
			session_id = sessionId,
			guild_id = guildId
		});
		await this.DoRequestAsync(HttpMethod.Patch, $"{path}{BuildQueryString(queryDict)}", payload: LavalinkJson.SerializeObject(pld)).ConfigureAwait(false);
	}

	/// <summary>
	/// Returns the player for a guild for the given session id.
	/// </summary>
	/// <param name="sessionId">The session id.</param>
	/// <param name="guildId">The guild id this player should be created/updated for.</param>
	/// <returns>The <see cref="LavalinkPlayer"/> for <paramref name="guildId"/>.</returns>
	internal async Task<LavalinkPlayer> GetPlayerAsync(string sessionId, ulong guildId)
	{
		var queryDict = this.GetDefaultParams();
		var route = $"{Endpoints.V4}{Endpoints.SESSIONS}/:session_id{Endpoints.PLAYERS}/:guild_id";
		var path = GetPath(route, new
		{
			session_id = sessionId,
			guild_id = guildId
		});
		var res = await this.DoRequestAsync(HttpMethod.Get, $"{path}{BuildQueryString(queryDict)}").ConfigureAwait(false);
		return LavalinkJson.DeserializeObject<LavalinkPlayer>(res.Response!)!;
	}

	/// <summary>
	/// Updates or creates the player for a guild if it doesn't already exist for the given session id.
	/// </summary>
	/// <param name="sessionId">The session id.</param>
	/// <param name="guildId">The guild id this player should be created/updated for.</param>
	/// <param name="noReplace">Whether to replace the current track with the new track. Defaults to <see langword="false"/>.</param>
	/// <param name="encodedTrack">The encoded track to play.</param>
	/// <param name="identifier">The identifier to play</param>
	/// <param name="position">The position to jump to or start from.</param>
	/// <param name="endTime">The time when the track should automatically end.</param>
	/// <param name="volume">The volume.</param>
	/// <param name="paused">Whether to pause the track.</param>
	/// <param name="filters">The filters.</param>
	/// <returns>The updated <see cref="LavalinkPlayer"/> object.</returns>
	internal async Task<LavalinkPlayer> UpdatePlayerAsync(
		string sessionId,
		ulong guildId,
		bool noReplace = false,
		Optional<string?> encodedTrack = default,
		Optional<string> identifier = default,
		Optional<int> position = default,
		Optional<int?> endTime = default,
		Optional<int> volume = default,
		Optional<bool> paused = default,
		Optional<LavalinkFilters> filters = default
	)
	{
		var queryDict = this.GetDefaultParams();
		queryDict.Add("noReplace", noReplace.ToString().ToLower());
		var pld = new LavalinkRestPlayerUpdatePayload(guildId.ToString())
		{
			EncodedTrack = encodedTrack,
			Identifier = identifier,
			Position = position,
			EndTime = endTime,
			Volume = volume,
			Paused = paused,
			Filters = filters
		};
		var route = $"{Endpoints.V4}{Endpoints.SESSIONS}/:session_id{Endpoints.PLAYERS}/:guild_id";
		var path = GetPath(route, new
		{
			session_id = sessionId,
			guild_id = guildId
		});
		var res = await this.DoRequestAsync(HttpMethod.Patch, $"{path}{BuildQueryString(queryDict)}", payload: LavalinkJson.SerializeObject(pld)).ConfigureAwait(false);
		return LavalinkJson.DeserializeObject<LavalinkPlayer>(res.Response!)!;
	}

	/// <summary>
	/// Updates the players voice state.
	/// </summary>
	/// <param name="sessionId">The session id.</param>
	/// <param name="guildId">The guild id this player voice state should be updated for.</param>
	/// <param name="state">The state to update with.</param>
	/// <returns></returns>
	internal async Task UpdatePlayerVoiceStateAsync(string sessionId, ulong guildId, LavalinkVoiceState state)
	{
		var queryDict = this.GetDefaultParams();
		queryDict.Add("noReplace", "true");
		var pld = new LavalinkRestVoiceStateUpdatePayload(state, guildId.ToString());

		var route = $"{Endpoints.V4}{Endpoints.SESSIONS}/:session_id{Endpoints.PLAYERS}/:guild_id";
		var path = GetPath(route, new
		{
			session_id = sessionId,
			guild_id = guildId
		});
		await this.DoRequestAsync(HttpMethod.Patch, $"{path}{BuildQueryString(queryDict)}", payload: LavalinkJson.SerializeObject(pld)).ConfigureAwait(false);
	}

	/// <summary>
	/// Destroys the player for a guild for the given session id
	/// </summary>
	/// <param name="sessionId">The session id.</param>
	/// <param name="guildId">The guild id this player should be created/updated for.</param>
	/// <returns></returns>
	internal async Task DestroyPlayerAsync(string sessionId, ulong guildId)
	{
		var queryDict = this.GetDefaultParams();
		var route = $"{Endpoints.V4}{Endpoints.SESSIONS}/:session_id{Endpoints.PLAYERS}/:guild_id";
		var path = GetPath(route, new
		{
			session_id = sessionId,
			guild_id = guildId
		});
		await this.DoRequestAsync(HttpMethod.Delete, $"{path}{BuildQueryString(queryDict)}").ConfigureAwait(false);
	}

	/// <summary>
	/// Resolves audio tracks for use with the <see cref="UpdatePlayerAsync"/> method.
	/// </summary>
	/// <param name="identifier">The identifier to resolve tracks with.</param>
	/// <returns>A <see cref="LavalinkTrackLoadingResult"/> where <see cref="LavalinkTrackLoadingResult.Result"/> is dynamic based on <see cref="LavalinkTrackLoadingResult.LoadType"/>.</returns>
	internal async Task<LavalinkTrackLoadingResult> LoadTracksAsync(string identifier)
	{
		var queryDict = this.GetDefaultParams();
		queryDict.Add("identifier", identifier);
		var route = $"{Endpoints.V4}{Endpoints.LOAD_TRACKS}";
		var path = GetPath(route, new
			{ });
		var res = await this.DoRequestAsync(HttpMethod.Get, $"{path}{BuildQueryString(queryDict)}").ConfigureAwait(false);
		var obj = JObject.Parse(res.Response!);
		return new()
		{
			LoadType = obj.GetValue("loadType")!.ToObject<LavalinkLoadResultType>(),
			RawResult = LavalinkJson.SerializeObject(obj.GetValue("data"))
		};
	}

	/// <summary>
	/// Decode a single track into its info, where <paramref name="base64Track"/> is the encoded base64 data.
	/// </summary>
	/// <param name="base64Track">The encoded track <see cref="string"/>.</param>
	/// <returns>The decoded <see cref="LavalinkTrack"/>.</returns>
	internal async Task<LavalinkTrack> DecodeTrackAsync(string base64Track)
	{
		var queryDict = this.GetDefaultParams();
		queryDict.Add("encodedTrack", base64Track);
		var route = $"{Endpoints.V4}{Endpoints.DECODE_TRACK}";
		var path = GetPath(route, new
			{ });
		var res = await this.DoRequestAsync(HttpMethod.Get, $"{path}{BuildQueryString(queryDict)}").ConfigureAwait(false);
		return LavalinkJson.DeserializeObject<LavalinkTrack>(res.Response!)!;
	}

	/// <summary>
	/// Decodes multiple tracks into their info.
	/// </summary>
	/// <param name="base64Tracks"><see cref="List{T}"/> of encoded track <see cref="string"/>s.</param>
	/// <returns>A <see cref="IReadOnlyList{T}"/> of decoded <see cref="LavalinkTrack"/>s.</returns>
	internal async Task<IReadOnlyList<LavalinkTrack>> DecodeTracksAsync(IEnumerable<string> base64Tracks)
	{
		var queryDict = this.GetDefaultParams();
		var route = $"{Endpoints.V4}{Endpoints.DECODE_TRACKS}";
		var path = GetPath(route, new
			{ });
		var res = await this.DoRequestAsync(HttpMethod.Post, $"{path}{BuildQueryString(queryDict)}", payload: LavalinkJson.SerializeObject(base64Tracks)).ConfigureAwait(false);
		return LavalinkJson.DeserializeObject<List<LavalinkTrack>>(res.Response!)!;
	}
}
