namespace DisCatSharp;

/// <summary>
/// Represents base for all DisCatSharp extensions. To implement your own extension, extend this class, and implement its abstract members.
/// </summary>
public abstract class BaseExtension
{
	/// <summary>
	/// Gets the instance of <see cref="DiscordClient"/> this extension is attached to.
	/// </summary>
	public DiscordClient Client { get; protected set; }

	/// <summary>
	/// Gets the string representing the version of bot lib extension.
	/// </summary>
	public string VersionString { get; set; } = string.Empty;

	/// <summary>
	/// Gets whether this lib extension supports the built-in version checl.
	/// </summary>
	public bool HasVersionCheckSupport { get; set; } = false;

	/// <summary>
	/// Gets the repository owner of this lib extension.
	/// </summary>
	public string RepositoryOwner { get; set; } = string.Empty;

	/// <summary>
	/// Gets the repository of this lib extension.
	/// </summary>
	public string Repository { get; set; } = string.Empty;

	/// <summary>
	/// Gets the package id of this lib extension.
	/// </summary>
	public string PackageId { get; set; } = string.Empty;

	/// <summary>
	/// Initializes this extension for given <see cref="DiscordClient"/> instance.
	/// </summary>
	/// <param name="client">Discord client to initialize for.</param>
	protected internal abstract void Setup(DiscordClient client);
}
