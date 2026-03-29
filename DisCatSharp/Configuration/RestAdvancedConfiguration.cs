namespace DisCatSharp;

/// <summary>
///     Advanced configuration for Discord REST client tuning.
/// </summary>
/// <remarks>
///     Properties in this class control low-level REST behavior such as queue timeouts, retry limits,
///     and rate-limit buffers. Additional knobs will be added here in later phases.
/// </remarks>
public sealed class RestAdvancedConfiguration
{
	/// <summary>
	///     Creates a new REST advanced configuration with default values.
	/// </summary>
	public RestAdvancedConfiguration()
	{ }

	/// <summary>
	///     Creates a clone of another REST advanced configuration.
	/// </summary>
	/// <param name="other">Configuration to clone.</param>
	public RestAdvancedConfiguration(RestAdvancedConfiguration other)
	{ }
}
