// This file is part of the DisCatSharp project, a fork of DSharpPlus.
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using DisCatSharp.Lavalink.Entities;
using DisCatSharp.Net;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DisCatSharp.Lavalink
{
    /// <summary>
    /// Represents a class for Lavalink REST calls.
    /// </summary>
    public sealed class LavalinkRestClient
    {
        /// <summary>
        /// Gets the REST connection endpoint for this client.
        /// </summary>
        public ConnectionEndpoint RestEndpoint { get; private set; }

        private HttpClient _http;

        private readonly ILogger _logger;

        private readonly Lazy<string> _dcsVersionString = new(() =>
        {
            var a = typeof(DiscordClient).GetTypeInfo().Assembly;

            var iv = a.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            if (iv != null)
                return iv.InformationalVersion;

            var v = a.GetName().Version;
            var vs = v.ToString(3);

            if (v.Revision > 0)
                vs = $"{vs}, CI build {v.Revision}";

            return vs;
        });

        /// <summary>
        /// Creates a new Lavalink REST client.
        /// </summary>
        /// <param name="RestEndpoint">The REST server endpoint to connect to.</param>
        /// <param name="Password">The password for the remote server.</param>
        public LavalinkRestClient(ConnectionEndpoint RestEndpoint, string Password)
        {
            this.RestEndpoint = RestEndpoint;
            this.ConfigureHttpHandling(Password);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LavalinkRestClient"/> class.
        /// </summary>
        /// <param name="Config">The config.</param>
        /// <param name="Client">The client.</param>
        internal LavalinkRestClient(LavalinkConfiguration Config, BaseDiscordClient Client)
        {
            this.RestEndpoint = Config.RestEndpoint;
            this._logger = Client.Logger;
            this.ConfigureHttpHandling(Config.Password, Client);
        }

        /// <summary>
        /// Gets the version of the Lavalink server.
        /// </summary>
        /// <returns></returns>
        public Task<string> GetVersion()
        {
            var versionUri = new Uri($"{this.RestEndpoint.ToHttpString()}{Endpoints.Version}");
            return this.InternalGetVersionAsync(versionUri);
        }

        #region Track_Loading

        /// <summary>
        /// Searches for specified terms.
        /// </summary>
        /// <param name="SearchQuery">What to search for.</param>
        /// <param name="Type">What platform will search for.</param>
        /// <returns>A collection of tracks matching the criteria.</returns>
        public Task<LavalinkLoadResult> GetTracks(string SearchQuery, LavalinkSearchType Type = LavalinkSearchType.Youtube)
        {
            var prefix = Type switch
            {
                LavalinkSearchType.Youtube => "ytsearch:",
                LavalinkSearchType.SoundCloud => "scsearch:",
                LavalinkSearchType.Plain => "",
                _ => throw new ArgumentOutOfRangeException(nameof(Type), Type, null)
            };
            var str = WebUtility.UrlEncode(prefix + SearchQuery);
            var tracksUri = new Uri($"{this.RestEndpoint.ToHttpString()}{Endpoints.LoadTracks}?identifier={str}");
            return this.InternalResolveTracksAsync(tracksUri);
        }

        /// <summary>
        /// Loads tracks from specified URL.
        /// </summary>
        /// <param name="Uri">URL to load tracks from.</param>
        /// <returns>A collection of tracks from the URL.</returns>
        public Task<LavalinkLoadResult> GetTracks(Uri Uri)
        {
            var str = WebUtility.UrlEncode(Uri.ToString());
            var tracksUri = new Uri($"{this.RestEndpoint.ToHttpString()}{Endpoints.LoadTracks}?identifier={str}");
            return this.InternalResolveTracksAsync(tracksUri);
        }

        /// <summary>
        /// Loads tracks from a local file.
        /// </summary>
        /// <param name="File">File to load tracks from.</param>
        /// <returns>A collection of tracks from the file.</returns>
        public Task<LavalinkLoadResult> GetTracks(FileInfo File)
        {
            var str = WebUtility.UrlEncode(File.FullName);
            var tracksUri = new Uri($"{this.RestEndpoint.ToHttpString()}{Endpoints.LoadTracks}?identifier={str}");
            return this.InternalResolveTracksAsync(tracksUri);
        }

        /// <summary>
        /// Decodes a base64 track string into a Lavalink track object.
        /// </summary>
        /// <param name="TrackString">The base64 track string.</param>
        /// <returns></returns>
        public Task<LavalinkTrack> DecodeTrack(string TrackString)
        {
            var str = WebUtility.UrlEncode(TrackString);
            var decodeTrackUri = new Uri($"{this.RestEndpoint.ToHttpString()}{Endpoints.DecodeTrack}?track={str}");
            return this.InternalDecodeTrackAsync(decodeTrackUri);
        }

        /// <summary>
        /// Decodes an array of base64 track strings into Lavalink track objects.
        /// </summary>
        /// <param name="TrackStrings">The array of base64 track strings.</param>
        /// <returns></returns>
        public Task<IEnumerable<LavalinkTrack>> DecodeTracks(string[] TrackStrings)
        {
            var decodeTracksUri = new Uri($"{this.RestEndpoint.ToHttpString()}{Endpoints.DecodeTracks}");
            return this.InternalDecodeTracksAsync(decodeTracksUri, TrackStrings);
        }

        /// <summary>
        /// Decodes a list of base64 track strings into Lavalink track objects.
        /// </summary>
        /// <param name="TrackStrings">The list of base64 track strings.</param>
        /// <returns></returns>
        public Task<IEnumerable<LavalinkTrack>> DecodeTracks(List<string> TrackStrings)
        {
            var decodeTracksUri = new Uri($"{this.RestEndpoint.ToHttpString()}{Endpoints.DecodeTracks}");
            return this.InternalDecodeTracksAsync(decodeTracksUri, TrackStrings.ToArray());
        }

        #endregion

        #region Route_Planner

        /// <summary>
        /// Retrieves statistics from the route planner.
        /// </summary>
        /// <returns>The status (<see cref="DisCatSharp.Lavalink.Entities.LavalinkRouteStatus"/>) details.</returns>
        public Task<LavalinkRouteStatus> GetRoutePlannerStatus()
        {
            var routeStatusUri = new Uri($"{this.RestEndpoint.ToHttpString()}{Endpoints.RoutePlanner}{Endpoints.Status}");
            return this.InternalGetRoutePlannerStatusAsync(routeStatusUri);
        }

        /// <summary>
        /// Unmarks a failed route planner IP Address.
        /// </summary>
        /// <param name="Address">The IP address name to unmark.</param>
        /// <returns></returns>
        public Task FreeAddress(string Address)
        {
            var routeFreeAddressUri = new Uri($"{this.RestEndpoint.ToHttpString()}{Endpoints.RoutePlanner}{Endpoints.FreeAddress}");
            return this.InternalFreeAddressAsync(routeFreeAddressUri, Address);
        }

        /// <summary>
        /// Unmarks all failed route planner IP Addresses.
        /// </summary>
        /// <returns></returns>
        public Task FreeAllAddresses()
        {
            var routeFreeAllAddressesUri = new Uri($"{this.RestEndpoint.ToHttpString()}{Endpoints.RoutePlanner}{Endpoints.FreeAll}");
            return this.InternalFreeAllAddressesAsync(routeFreeAllAddressesUri);
        }

        #endregion

        /// <summary>
        /// get version async.
        /// </summary>
        /// <param name="Uri">The uri.</param>
        /// <returns>A Task.</returns>
        internal async Task<string> InternalGetVersionAsync(Uri Uri)
        {
            using var req = await this._http.GetAsync(Uri).ConfigureAwait(false);
            using var res = await req.Content.ReadAsStreamAsync().ConfigureAwait(false);
            using var sr = new StreamReader(res, Utilities.Utf8);
            var json = await sr.ReadToEndAsync().ConfigureAwait(false);
            return json;
        }

        #region Internal_Track_Loading

        /// <summary>
        /// resolve tracks async.
        /// </summary>
        /// <param name="Uri">The uri.</param>
        /// <returns>A Task.</returns>
        internal async Task<LavalinkLoadResult> InternalResolveTracksAsync(Uri Uri)
        {
            // this function returns a Lavalink 3-like dataset regardless of input data version

            var json = "[]";
            using (var req = await this._http.GetAsync(Uri).ConfigureAwait(false))
            using (var res = await req.Content.ReadAsStreamAsync().ConfigureAwait(false))
            using (var sr = new StreamReader(res, Utilities.Utf8))
                json = await sr.ReadToEndAsync().ConfigureAwait(false);

            var jdata = JToken.Parse(json);
            if (jdata is JArray jarr)
            {
                // Lavalink 2.x

                var tracks = new List<LavalinkTrack>(jarr.Count);
                foreach (var jt in jarr)
                {
                    var track = jt["info"].ToObject<LavalinkTrack>();
                    track.TrackString = jt["track"].ToString();

                    tracks.Add(track);
                }

                return new LavalinkLoadResult
                {
                    PlaylistInfo = default,
                    LoadResultType = tracks.Count == 0 ? LavalinkLoadResultType.LoadFailed : LavalinkLoadResultType.TrackLoaded,
                    Tracks = tracks
                };
            }
            else if (jdata is JObject jo)
            {
                // Lavalink 3.x

                jarr = jo["tracks"] as JArray;
                var loadInfo = jo.ToObject<LavalinkLoadResult>();
                var tracks = new List<LavalinkTrack>(jarr.Count);
                foreach (var jt in jarr)
                {
                    var track = jt["info"].ToObject<LavalinkTrack>();
                    track.TrackString = jt["track"].ToString();

                    tracks.Add(track);
                }

                loadInfo.Tracks = new ReadOnlyCollection<LavalinkTrack>(tracks);

                return loadInfo;
            }
            else
                return null;
        }

        /// <summary>
        /// decode track async.
        /// </summary>
        /// <param name="Uri">The uri.</param>
        /// <returns>A Task.</returns>
        internal async Task<LavalinkTrack> InternalDecodeTrackAsync(Uri Uri)
        {
            using var req = await this._http.GetAsync(Uri).ConfigureAwait(false);
            using var res = await req.Content.ReadAsStreamAsync().ConfigureAwait(false);
            using var sr = new StreamReader(res, Utilities.Utf8);
            var json = await sr.ReadToEndAsync().ConfigureAwait(false);
            if (!req.IsSuccessStatusCode)
            {
                var jsonError = JObject.Parse(json);
                this._logger?.LogError(LavalinkEvents.LavalinkDecodeError, "Unable to decode track strings: {0}", jsonError["message"]);

                return null;
            }
            var track = JsonConvert.DeserializeObject<LavalinkTrack>(json);
            return track;
        }

        /// <summary>
        /// decode tracks async.
        /// </summary>
        /// <param name="Uri">The uri.</param>
        /// <param name="Ids">The ids.</param>
        /// <returns>A Task.</returns>
        internal async Task<IEnumerable<LavalinkTrack>> InternalDecodeTracksAsync(Uri Uri, string[] Ids)
        {
            var jsonOut = JsonConvert.SerializeObject(Ids);
            var content = new StringContent(jsonOut, Utilities.Utf8, "application/json");
            using var req = await this._http.PostAsync(Uri, content).ConfigureAwait(false);
            using var res = await req.Content.ReadAsStreamAsync().ConfigureAwait(false);
            using var sr = new StreamReader(res, Utilities.Utf8);
            var jsonIn = await sr.ReadToEndAsync().ConfigureAwait(false);
            if (!req.IsSuccessStatusCode)
            {
                var jsonError = JObject.Parse(jsonIn);
                this._logger?.LogError(LavalinkEvents.LavalinkDecodeError, "Unable to decode track strings", jsonError["message"]);
                return null;
            }

            var jarr = JToken.Parse(jsonIn) as JArray;
            var decodedTracks = new LavalinkTrack[jarr.Count];

            for (var i = 0; i < decodedTracks.Length; i++)
            {
                decodedTracks[i] = JsonConvert.DeserializeObject<LavalinkTrack>(jarr[i]["info"].ToString());
                decodedTracks[i].TrackString = jarr[i]["track"].ToString();
            }

            var decodedTrackList = new ReadOnlyCollection<LavalinkTrack>(decodedTracks);

            return decodedTrackList;
        }

        #endregion

        #region Internal_Route_Planner

        /// <summary>
        /// get route planner status async.
        /// </summary>
        /// <param name="Uri">The uri.</param>
        /// <returns>A Task.</returns>
        internal async Task<LavalinkRouteStatus> InternalGetRoutePlannerStatusAsync(Uri Uri)
        {
            using var req = await this._http.GetAsync(Uri).ConfigureAwait(false);
            using var res = await req.Content.ReadAsStreamAsync().ConfigureAwait(false);
            using var sr = new StreamReader(res, Utilities.Utf8);
            var json = await sr.ReadToEndAsync().ConfigureAwait(false);
            var status = JsonConvert.DeserializeObject<LavalinkRouteStatus>(json);
            return status;
        }

        /// <summary>
        /// free address async.
        /// </summary>
        /// <param name="Uri">The uri.</param>
        /// <param name="Address">The address.</param>
        /// <returns>A Task.</returns>
        internal async Task InternalFreeAddressAsync(Uri Uri, string Address)
        {
            var payload = new StringContent(Address, Utilities.Utf8, "application/json");
            using var req = await this._http.PostAsync(Uri, payload).ConfigureAwait(false);
            if (req.StatusCode == HttpStatusCode.InternalServerError)
                this._logger?.LogWarning(LavalinkEvents.LavalinkRestError, "Request to {0} returned an internal server error - your server route planner configuration is likely incorrect", Uri);

        }

        /// <summary>
        /// free all addresses async.
        /// </summary>
        /// <param name="Uri">The uri.</param>
        /// <returns>A Task.</returns>
        internal async Task InternalFreeAllAddressesAsync(Uri Uri)
        {
            var httpReq = new HttpRequestMessage(HttpMethod.Post, Uri);
            using var req = await this._http.SendAsync(httpReq).ConfigureAwait(false);
            if (req.StatusCode == HttpStatusCode.InternalServerError)
                this._logger?.LogWarning(LavalinkEvents.LavalinkRestError, "Request to {0} returned an internal server error - your server route planner configuration is likely incorrect", Uri);
        }

        #endregion

        /// <summary>
        /// Configures the http handling.
        /// </summary>
        /// <param name="Password">The password.</param>
        /// <param name="Client">The client.</param>
        private void ConfigureHttpHandling(string Password, BaseDiscordClient Client = null)
        {
            var httphandler = new HttpClientHandler
            {
                UseCookies = false,
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
                UseProxy = Client != null && Client.Configuration.Proxy != null
            };
            if (httphandler.UseProxy) // because mono doesn't implement this properly
                httphandler.Proxy = Client.Configuration.Proxy;

            this._http = new HttpClient(httphandler);

            this._http.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", $"DisCatSharp.LavaLink/{this._dcsVersionString}");
            this._http.DefaultRequestHeaders.TryAddWithoutValidation("Client-Name", $"DisCatSharp");
            this._http.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", Password);
        }
    }
}
