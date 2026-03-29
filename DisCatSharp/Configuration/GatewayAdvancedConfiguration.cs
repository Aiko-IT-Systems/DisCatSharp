namespace DisCatSharp;

/// <summary>
///     Advanced configuration for Discord gateway transport tuning.
/// </summary>
/// <remarks>
///     Properties in this class control low-level transport behavior and should rarely need adjustment.
///     Additional timeout and tuning knobs will be added here in later phases.
/// </remarks>
public sealed class GatewayAdvancedConfiguration
{
	/// <summary>
	///     Creates a new gateway advanced configuration with default values.
	/// </summary>
	public GatewayAdvancedConfiguration()
	{ }

	/// <summary>
	///     Creates a clone of another gateway advanced configuration.
	/// </summary>
	/// <param name="other">Configuration to clone.</param>
	public GatewayAdvancedConfiguration(GatewayAdvancedConfiguration other)
	{ }
}
