
namespace DisCatSharp;

/// <summary>
/// Represents base for all DisCatSharp extensions. To implement your own extension, extend this class, and implement its abstract members.
/// </summary>
public abstract class BaseExtension
{
	/// <summary>
	/// Gets the instance of <see cref="DiscordClient"/> this extension is attached to.
	/// </summary>
	// ReSharper disable once MemberCanBeProtected.Global
	// ReSharper disable once NotNullOrRequiredMemberIsNotInitialized
	public DiscordClient Client { get; protected set; }

	/// <summary>
	/// Initializes this extension for given <see cref="DiscordClient"/> instance.
	/// </summary>
	/// <param name="client">Discord client to initialize for.</param>
	protected internal abstract void Setup(DiscordClient client);
}
