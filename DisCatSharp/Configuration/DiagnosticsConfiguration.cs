namespace DisCatSharp;

/// <summary>
///     Configuration for Discord client diagnostics and debugging.
/// </summary>
public sealed class DiagnosticsConfiguration
{
	/// <summary>
	///     Creates a new diagnostics configuration with default values.
	/// </summary>
	public DiagnosticsConfiguration()
	{ }

	/// <summary>
	///     Creates a clone of another diagnostics configuration.
	/// </summary>
	/// <param name="other">Configuration to clone.</param>
	public DiagnosticsConfiguration(DiagnosticsConfiguration other)
	{
		this.EnablePayloadReceivedEvent = other.EnablePayloadReceivedEvent;
		this.UpdateChecks = new(other.UpdateChecks);
	}

	/// <summary>
	///     <para>Causes the <see cref="DiscordClient.PayloadReceived" /> event to be fired.</para>
	///     <para>Useful if you want to work with raw events.</para>
	///     <para>Defaults to <see langword="false" />.</para>
	/// </summary>
	public bool EnablePayloadReceivedEvent { internal get; set; } = false;

	/// <summary>
	///     <para>Library update check configuration.</para>
	/// </summary>
	public UpdateCheckConfiguration UpdateChecks { internal get; set; } = new();
}
