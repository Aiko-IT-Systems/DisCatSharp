using System.Linq;

using DisCatSharp.Enums;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a change set for creating a sticker.
/// </summary>
public sealed class StickerCreateChangeSet : DiscordAuditLogEntry
{
	public StickerCreateChangeSet()
	{
		this.ValidFor = AuditLogActionType.StickerCreate;
	}

	public string Name => (string)this.Changes.FirstOrDefault(x => x.Key == "name")?.NewValue;
	public string Description => (string)this.Changes.FirstOrDefault(x => x.Key == "description")?.NewValue;
	public string Tags => (string)this.Changes.FirstOrDefault(x => x.Key == "tags")?.NewValue;
	public StickerType StickerType => (StickerType)this.Changes.FirstOrDefault(x => x.Key == "type")?.NewValue;
	public StickerFormat FormatType => (StickerFormat)this.Changes.FirstOrDefault(x => x.Key == "format_type")?.NewValue;
	public bool? IsAvailable => (bool?)this.Changes.FirstOrDefault(x => x.Key == "available")?.NewValue;
}
