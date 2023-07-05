// This file is part of the DisCatSharp project.
//
// Copyright (c) 2023 AITSYS
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
// FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace DisCatSharp.Lavalink.Entities;

/// <summary>
/// Represents lavalink server information.
/// </summary>
public sealed class LavalinkInfo
{
	/// <summary>
	/// Gets the lavalink version object.
	/// </summary>
	[JsonProperty("version")]
	public Version Version { get; internal set; }

	/// <summary>
	/// Gets the datetime offset when this version was built.
	/// </summary>
	[JsonIgnore]
	public DateTimeOffset BuildTime => Utilities.GetDateTimeOffsetFromMilliseconds(this._buildTime);
	/// <summary>
	/// Gets the timestamp when this version was built.
	/// </summary>
	[JsonProperty("commitTime")]
	private readonly long _buildTime;

	/// <summary>
	/// Gets the git information.
	/// </summary>
	[JsonProperty("git")]
	public Git Git { get; internal set; }

	/// <summary>
	/// Gets the jvm version.
	/// </summary>
	[JsonProperty("jvm")]
	public string Jvm { get; internal set; }

	/// <summary>
	/// Gets the lavaplayer version.
	/// </summary>
	[JsonProperty("lavaplayer")]
	public string Lavaplayer { get; internal set; }

	/// <summary>
	/// Gets a <see cref="List{T}"/> of available source managers.
	/// </summary>
	[JsonProperty("sourceManagers")]
	public List<string> SourceManagers { get; internal set; } = new();

	/// <summary>
	/// Gets a <see cref="List{T}"/> of available folters.
	/// </summary>
	[JsonProperty("filters")]
	public List<string> Filters { get; internal set; } = new();

	/// <summary>
	/// Gets a <see cref="List{T}"/> of available plugins.
	/// </summary>
	[JsonProperty("plugins")]
	public List<Plugin> Plugins { get; internal set; } = new();
}

/// <summary>
/// Represents git information
/// </summary>
public sealed class Git
{
	/// <summary>
	/// Gets the branch this Lavalink server was build on.
	/// </summary>
	[JsonProperty("branch")]
	public string Branch { get; internal set; }

	/// <summary>
	/// Gets the commit hash this Lavalink server was build on.
	/// </summary>
	[JsonProperty("commit")]
	public string Commit { get; internal set; }

	/// <summary>
	/// Gets the commit time as datetime offset when this Lavalink server was build.
	/// </summary>
	[JsonIgnore]
	public DateTimeOffset CommitTime => Utilities.GetDateTimeOffsetFromMilliseconds(this._commitTime);

	/// <summary>
	/// Gets the millisecond unix timestamp for when the commit was created.
	/// </summary>
	[JsonProperty("commitTime")]
	private readonly long _commitTime;
}

/// <summary>
/// Represents a plugin.
/// </summary>
public sealed class Plugin
{
	/// <summary>
	/// Gets the plugin name.
	/// </summary>
	[JsonProperty("name")]
	public string Name { get; internal set; }

	/// <summary>
	/// Gets the plugin version.
	/// </summary>
	[JsonProperty("version")]
	public string Version { get; internal set; }
}

/// <summary>
/// Represents a version object.
/// </summary>
public sealed class Version
{
	/// <summary>
	/// Gets the full version string of the Lavalink server.
	/// </summary>
	[JsonProperty("semver")]
	public string Semver { get; internal set; }

	/// <summary>
	/// Gets the major version of the Lavalink server.
	/// </summary>
	[JsonProperty("major")]
	public int Major { get; internal set; }

	/// <summary>
	/// Gets the minor version of the Lavalink server.
	/// </summary>
	[JsonProperty("minor")]
	public int Minor { get; internal set; }

	/// <summary>
	/// Gets the patch version of the Lavalink server.
	/// </summary>
	[JsonProperty("patch")]
	public int Patch { get; internal set; }

	/// <summary>
	/// Gets the pre-release version according to semver as a <c>.</c> separated list of identifiers.
	/// </summary>
	[JsonProperty("preRelease")]
	public string? PreRelease { get; internal set; }

	/// <summary>
	/// The build metadata according to semver as a <c>.</c> separated list of identifiers
	/// </summary>
	[JsonProperty("build")]
	public string? Build { get; internal set; }
}

