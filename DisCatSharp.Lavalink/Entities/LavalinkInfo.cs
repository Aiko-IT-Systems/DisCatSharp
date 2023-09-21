
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
#pragma warning disable CS0649 // Field 'LavalinkInfo._buildTime' is never assigned to, and will always have its default value 0
	private readonly long _buildTime;
#pragma warning restore CS0649 // Field 'LavalinkInfo._buildTime' is never assigned to, and will always have its default value 0

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
#pragma warning disable CS0649 // Field 'Git._commitTime' is never assigned to, and will always have its default value 0
	private readonly long _commitTime;
#pragma warning restore CS0649 // Field 'Git._commitTime' is never assigned to, and will always have its default value 0
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

