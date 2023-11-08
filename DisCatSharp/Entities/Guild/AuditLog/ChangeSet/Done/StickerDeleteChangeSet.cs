using System.Linq;

using DisCatSharp.Enums;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a change set for deleting a sticker.
/// </summary>
public sealed class StickerDeleteChangeSet : DiscordAuditLogEntry
{
	public StickerDeleteChangeSet()
	{
		this.ValidFor = AuditLogActionType.StickerDelete;
	}

	public string Name => (string)this.Changes.FirstOrDefault(x => x.Key == "name")?.OldValue;
	public string Description => (string)this.Changes.FirstOrDefault(x => x.Key == "description")?.OldValue;
	public string Tags => (string)this.Changes.FirstOrDefault(x => x.Key == "tags")?.OldValue;
	public StickerType StickerType => (StickerType)this.Changes.FirstOrDefault(x => x.Key == "type")?.OldValue;
	public StickerFormat FormatType => (StickerFormat)this.Changes.FirstOrDefault(x => x.Key == "format_type")?.OldValue;
	public bool? IsAvailable => (bool?)this.Changes.FirstOrDefault(x => x.Key == "available")?.OldValue;
}
