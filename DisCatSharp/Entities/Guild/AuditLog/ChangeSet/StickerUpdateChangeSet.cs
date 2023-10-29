using System.Linq;

using DisCatSharp.Enums;

namespace DisCatSharp.Entities;

/// <summary>
/// Represents a change set for updating sticker details.
/// </summary>
public sealed class StickerUpdateChangeSet : DiscordAuditLogEntry
{
	public StickerUpdateChangeSet()
	{
		this.ValidFor = AuditLogActionType.StickerUpdate;
	}

	public bool NameChanged => this.NameBefore is not null || this.NameAfter is not null;
	public string? NameBefore => (string)this.Changes.FirstOrDefault(x => x.Key == "name")?.OldValue;
	public string? NameAfter => (string)this.Changes.FirstOrDefault(x => x.Key == "name")?.NewValue;

	public bool DescriptionChanged => this.DescriptionBefore is not null || this.DescriptionAfter is not null;
	public string? DescriptionBefore => (string)this.Changes.FirstOrDefault(x => x.Key == "description")?.OldValue;
	public string? DescriptionAfter => (string)this.Changes.FirstOrDefault(x => x.Key == "description")?.NewValue;

	public bool TagsChanged => this.TagsBefore is not null || this.TagsAfter is not null;
	public string? TagsBefore => (string)this.Changes.FirstOrDefault(x => x.Key == "tags")?.OldValue;
	public string? TagsAfter => (string)this.Changes.FirstOrDefault(x => x.Key == "tags")?.NewValue;

	public bool StickerTypeChanged => this.StickerTypeBefore is not null || this.StickerTypeAfter is not null;
	public StickerType? StickerTypeBefore => (StickerType)this.Changes.FirstOrDefault(x => x.Key == "type")?.OldValue;
	public StickerType? StickerTypeAfter => (StickerType)this.Changes.FirstOrDefault(x => x.Key == "type")?.NewValue;

	public bool FormatTypeChanged => this.FormatTypeBefore is not null || this.FormatTypeAfter is not null;
	public StickerFormat? FormatTypeBefore => (StickerFormat)this.Changes.FirstOrDefault(x => x.Key == "format_type")?.OldValue;
	public StickerFormat? FormatTypeAfter => (StickerFormat)this.Changes.FirstOrDefault(x => x.Key == "format_type")?.NewValue;

	public bool IsAvailableChanged => this.IsAvailableBefore is not null || this.IsAvailableAfter is not null;
	public bool? IsAvailableBefore => (bool?)this.Changes.FirstOrDefault(x => x.Key == "available")?.OldValue;
	public bool? IsAvailableAfter => (bool?)this.Changes.FirstOrDefault(x => x.Key == "available")?.NewValue;
}
