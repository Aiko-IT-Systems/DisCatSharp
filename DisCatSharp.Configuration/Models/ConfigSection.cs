using Microsoft.Extensions.Configuration;

namespace DisCatSharp.Configuration.Models;

/// <summary>
/// Represents an object in <see cref="IConfiguration"/>
/// </summary>
internal readonly struct ConfigSection
{
	/// <summary>
	/// Key within <see cref="Config"/> which represents an object containing multiple values
	/// </summary>
	public string SectionName { get; }

	/// <summary>
	/// Optional used to indicate this section is nested within another
	/// </summary>
	public string? Root { get; }

	/// <summary>
	/// Reference to <see cref="IConfiguration"/> used within application
	/// </summary>
	public IConfiguration Config { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="ConfigSection"/> class.
	/// </summary>
	/// <param name="config">Reference to config</param>
	/// <param name="sectionName">Section of interest</param>
	/// <param name="rootName">(Optional) Indicates <paramref name="sectionName"/> is nested within this name. Default value is DisCatSharp</param>
	public ConfigSection(ref IConfiguration config, string sectionName, string? rootName = "DisCatSharp")
	{
		this.Config = config;
		this.SectionName = sectionName;
		this.Root = rootName;
	}

	/// <summary>
	/// Checks if key exists in <see cref="Config"/>
	/// </summary>
	/// <param name="name">Property / Key to search for in section</param>
	/// <returns>True if key exists, otherwise false. Outputs path to config regardless</returns>
	public bool ContainsKey(string name)
	{
		var path = string.IsNullOrEmpty(this.Root)
			? this.Config.ConfigPath(this.SectionName, name)
			: this.Config.ConfigPath(this.Root, this.SectionName, name);

		return !string.IsNullOrEmpty(this.Config[path]);
	}

	/// <summary>
	/// Attempts to get value associated to the config path. <br/> Should be used in unison with <see cref="ContainsKey"/>
	/// </summary>
	/// <param name="propName">Config path to value</param>
	/// <returns>Value found at <paramref name="propName"/></returns>
	public string GetValue(string propName)
		=> this.Config[this.GetPath(propName)];

	/// <summary>
	/// Gets the path.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <returns>A string.</returns>
	public string GetPath(string value) =>
		string.IsNullOrEmpty(this.Root)
			? this.Config.ConfigPath(this.SectionName, value)
			: this.Config.ConfigPath(this.Root, this.SectionName, value);
}
