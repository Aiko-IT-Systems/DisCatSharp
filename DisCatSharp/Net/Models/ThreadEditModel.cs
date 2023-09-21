using System.Collections.Generic;

using DisCatSharp.Entities;
using DisCatSharp.Enums;

namespace DisCatSharp.Net.Models;

/// <summary>
/// Represents a thread edit model.
/// </summary>
public class ThreadEditModel : BaseEditModel
{
	/// <summary>
	/// Sets the thread's new name.
	/// </summary>
	public string Name { internal get; set; }

	/// <summary>
	/// Sets the thread's locked state.
	/// </summary>
	public Optional<bool?> Locked { internal get; set; }

	/// <summary>
	/// Sets the thread's archived state.
	/// </summary>
	public Optional<bool?> Archived { internal get; set; }

	/// <summary>
	/// Sets the thread's auto archive duration.
	/// </summary>
	public Optional<ThreadAutoArchiveDuration?> AutoArchiveDuration { internal get; set; }

	/// <summary>
	/// Sets the thread's new user rate limit.
	/// </summary>
	public Optional<int?> PerUserRateLimit { internal get; set; }

	/// <summary>
	/// Sets the thread's invitable state.
	/// </summary>
	public Optional<bool?> Invitable { internal get; set; }

	/// <summary>
	/// Whether the thread is pinned within the <see cref="ChannelType.Forum">forum</see> channel.
	/// </summary>
	public Optional<bool?> Pinned { internal get; set; }

	/// <summary>
	/// Sets the thread's applied tags.
	/// </summary>
	public Optional<IEnumerable<ForumPostTag>?> AppliedTags { internal get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="ThreadEditModel"/> class.
	/// </summary>
	internal ThreadEditModel()
	{ }
}
