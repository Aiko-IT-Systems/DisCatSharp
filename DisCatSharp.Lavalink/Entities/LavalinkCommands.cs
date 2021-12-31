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
using Newtonsoft.Json;

namespace DisCatSharp.Lavalink.Entities
{
    /// <summary>
    /// The lavalink configure resume.
    /// </summary>
    internal sealed class LavalinkConfigureResume : LavalinkPayload
    {
        /// <summary>
        /// Gets the key.
        /// </summary>
        [JsonProperty("key")]
        public string Key { get; }

        /// <summary>
        /// Gets the timeout.
        /// </summary>
        [JsonProperty("timeout")]
        public int Timeout { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LavalinkConfigureResume"/> class.
        /// </summary>
        /// <param name="Key">The key.</param>
        /// <param name="Timeout">The timeout.</param>
        public LavalinkConfigureResume(string Key, int Timeout)
            : base("configureResuming")
        {
            this.Key = Key;
            this.Timeout = Timeout;
        }
    }

    /// <summary>
    /// The lavalink destroy.
    /// </summary>
    internal sealed class LavalinkDestroy : LavalinkPayload
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LavalinkDestroy"/> class.
        /// </summary>
        /// <param name="Lvl">The lvl.</param>
        public LavalinkDestroy(LavalinkGuildConnection Lvl)
            : base("destroy", Lvl.GuildIdString)
        { }
    }

    /// <summary>
    /// The lavalink play.
    /// </summary>
    internal sealed class LavalinkPlay : LavalinkPayload
    {
        /// <summary>
        /// Gets the track.
        /// </summary>
        [JsonProperty("track")]
        public string Track { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LavalinkPlay"/> class.
        /// </summary>
        /// <param name="Lvl">The lvl.</param>
        /// <param name="Track">The track.</param>
        public LavalinkPlay(LavalinkGuildConnection Lvl, LavalinkTrack Track)
            : base("play", Lvl.GuildIdString)
        {
            this.Track = Track.TrackString;
        }
    }

    /// <summary>
    /// The lavalink play partial.
    /// </summary>
    internal sealed class LavalinkPlayPartial : LavalinkPayload
    {
        /// <summary>
        /// Gets the track.
        /// </summary>
        [JsonProperty("track")]
        public string Track { get; }

        /// <summary>
        /// Gets the start time.
        /// </summary>
        [JsonProperty("startTime")]
        public long StartTime { get; }

        /// <summary>
        /// Gets the stop time.
        /// </summary>
        [JsonProperty("endTime")]
        public long StopTime { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LavalinkPlayPartial"/> class.
        /// </summary>
        /// <param name="Lvl">The lvl.</param>
        /// <param name="Track">The track.</param>
        /// <param name="Start">The start.</param>
        /// <param name="Stop">The stop.</param>
        public LavalinkPlayPartial(LavalinkGuildConnection Lvl, LavalinkTrack Track, TimeSpan Start, TimeSpan Stop)
            : base("play", Lvl.GuildIdString)
        {
            this.Track = Track.TrackString;
            this.StartTime = (long)Start.TotalMilliseconds;
            this.StopTime = (long)Stop.TotalMilliseconds;
        }
    }

    /// <summary>
    /// The lavalink pause.
    /// </summary>
    internal sealed class LavalinkPause : LavalinkPayload
    {
        /// <summary>
        /// Gets a value indicating whether pause.
        /// </summary>
        [JsonProperty("pause")]
        public bool Pause { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LavalinkPause"/> class.
        /// </summary>
        /// <param name="Lvl">The lvl.</param>
        /// <param name="Pause">If true, pause.</param>
        public LavalinkPause(LavalinkGuildConnection Lvl, bool Pause)
            : base("pause", Lvl.GuildIdString)
        {
            this.Pause = Pause;
        }
    }

    /// <summary>
    /// The lavalink stop.
    /// </summary>
    internal sealed class LavalinkStop : LavalinkPayload
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LavalinkStop"/> class.
        /// </summary>
        /// <param name="Lvl">The lvl.</param>
        public LavalinkStop(LavalinkGuildConnection Lvl)
            : base("stop", Lvl.GuildIdString)
        { }
    }

    /// <summary>
    /// The lavalink seek.
    /// </summary>
    internal sealed class LavalinkSeek : LavalinkPayload
    {
        /// <summary>
        /// Gets the position.
        /// </summary>
        [JsonProperty("position")]
        public long Position { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LavalinkSeek"/> class.
        /// </summary>
        /// <param name="Lvl">The lvl.</param>
        /// <param name="Position">The position.</param>
        public LavalinkSeek(LavalinkGuildConnection Lvl, TimeSpan Position)
            : base("seek", Lvl.GuildIdString)
        {
            this.Position = (long)Position.TotalMilliseconds;
        }
    }

    /// <summary>
    /// The lavalink volume.
    /// </summary>
    internal sealed class LavalinkVolume : LavalinkPayload
    {
        /// <summary>
        /// Gets the volume.
        /// </summary>
        [JsonProperty("volume")]
        public int Volume { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LavalinkVolume"/> class.
        /// </summary>
        /// <param name="Lvl">The lvl.</param>
        /// <param name="Volume">The volume.</param>
        public LavalinkVolume(LavalinkGuildConnection Lvl, int Volume)
            : base("volume", Lvl.GuildIdString)
        {
            this.Volume = Volume;
        }
    }

    /// <summary>
    /// The lavalink equalizer.
    /// </summary>
    internal sealed class LavalinkEqualizer : LavalinkPayload
    {
        /// <summary>
        /// Gets the bands.
        /// </summary>
        [JsonProperty("bands")]
        public IEnumerable<LavalinkBandAdjustment> Bands { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LavalinkEqualizer"/> class.
        /// </summary>
        /// <param name="Lvl">The lvl.</param>
        /// <param name="Bands">The bands.</param>
        public LavalinkEqualizer(LavalinkGuildConnection Lvl, IEnumerable<LavalinkBandAdjustment> Bands)
            : base("equalizer", Lvl.GuildIdString)
        {
            this.Bands = Bands;
        }
    }
}
