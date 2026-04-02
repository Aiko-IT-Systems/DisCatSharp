using DisCatSharp.Enums;

namespace DisCatSharp;

/// <summary>
///     Configuration for library update checking behavior.
/// </summary>
public sealed class UpdateCheckConfiguration
{
	/// <summary>
	///     Creates a new update check configuration with default values.
	/// </summary>
	public UpdateCheckConfiguration()
	{ }

	/// <summary>
	///     Creates a clone of another update check configuration.
	/// </summary>
	/// <param name="other">Configuration to clone.</param>
	public UpdateCheckConfiguration(UpdateCheckConfiguration other)
	{
		this.Disabled = other.Disabled;
		this.Mode = other.Mode;
		this.IncludePrerelease = other.IncludePrerelease;
		this.GitHubToken = other.GitHubToken;
		this.ShowReleaseNotes = other.ShowReleaseNotes;
	}

	/// <summary>
	///     Whether to disable the update check.
	/// </summary>
	public bool Disabled { internal get; set; } = false;

	/// <summary>
	///     Against which channel to check for updates.
	///     <para>Defaults to <see cref="VersionCheckMode.NuGet" />.</para>
	/// </summary>
	public VersionCheckMode Mode { internal get; set; } = VersionCheckMode.NuGet;

	/// <summary>
	///     Whether to include prerelease versions in the update check.
	/// </summary>
	public bool IncludePrerelease { internal get; set; } = true;

	/// <summary>
	///     Sets the GitHub token to use for the update check.
	///     <para>
	///         Only useful if extensions are private and <see cref="Mode" /> is
	///         <see cref="VersionCheckMode.GitHub" />.
	///     </para>
	/// </summary>
	public string? GitHubToken { internal get; set; } = null;

	/// <summary>
	///     Whether to show release notes in the update check.
	///     <para>Defaults to <see langword="false" />.</para>
	/// </summary>
	public bool ShowReleaseNotes { internal get; set; } = false;
}
