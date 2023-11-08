namespace DisCatSharp.Hosting;

/// <summary>
/// The constants.
/// </summary>
internal static class Constants
{
	/// <summary>
	/// Gets the lib name.
	/// </summary>
	public static string LibName => Configuration.ConfigurationExtensions.DEFAULT_ROOT_LIB;

	/// <summary>
	/// Gets the config suffix.
	/// </summary>
	public static string ConfigSuffix => "Configuration";

	/// <summary>
	/// Gets the extension suffix.
	/// </summary>
	public static string ExtensionSuffix => "Extension";
}
