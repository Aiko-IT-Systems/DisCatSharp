// This file is part of the DisCatSharp project, based off DSharpPlus.
//
// Copyright (c) 2021-2023 AITSYS
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Collections.Generic;

using DisCatSharp.Entities;
using DisCatSharp.Enums;

namespace DisCatSharp.Net.Models;

/// <summary>
/// Represents a forum channel edit model.
/// </summary>
public class ForumChannelEditModel : BaseEditModel
{
	/// <summary>
	/// Sets the channel's new name.
	/// </summary>
	public string Name { internal get; set; }

	/// <summary>
	/// Sets the channel's new position.
	/// </summary>
	public int? Position { internal get; set; }

	/// <summary>
	/// Sets the channel's new topic.
	/// </summary>
	public Optional<string> Topic { internal get; set; }

	/// <summary>
	/// Sets the channel's new template.
	/// <note type="warning">This is not yet released and won't be applied by library.</note>
	/// </summary>
	public Optional<string> Template { internal get; set; }

	/// <summary>
	/// Sets whether the channel is to be marked as NSFW.
	/// </summary>
	public bool? Nsfw { internal get; set; }

	/// <summary>
	/// Sets the available tags.
	/// </summary>
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
	public Optional<List<ForumPostTag>?> AvailableTags { internal get; set; }
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.

	/// <summary>
	/// Sets the default reaction emoji.
	/// </summary>
	public Optional<ForumReactionEmoji> DefaultReactionEmoji { internal get; set; }

	/// <summary>
	/// Sets the default forum post sort order
	/// </summary>
	public Optional<ForumPostSortOrder?> DefaultSortOrder { internal get; set; }

	/// <summary>
	/// <para>Sets the parent of this channel.</para>
	/// <para>This should be channel with <see cref="DisCatSharp.Entities.DiscordChannel.Type"/> set to <see cref="ChannelType.Category"/>.</para>
	/// </summary>
	public Optional<DiscordChannel> Parent { internal get; set; }

	/// <summary>
	/// <para>Sets the voice channel's new user limit.</para>
	/// <para>Setting this to 0 will disable the user limit.</para>
	/// </summary>
	public int? UserLimit { internal get; set; }

	/// <summary>
	/// <para>Sets the channel's new slow mode timeout.</para>
	/// <para>Setting this to null or 0 will disable slow mode.</para>
	/// </summary>
	public Optional<int?> PerUserRateLimit { internal get; set; }

	/// <summary>
	/// <para>Sets the channel's new post slow mode timeout.</para>
	/// <para>Setting this to null or 0 will disable slow mode.</para>
	/// </summary>
	public Optional<int?> PostCreateUserRateLimit { internal get; set; }

	/// <summary>
	/// Sets this channel's default duration for newly created threads, in minutes, to automatically archive the thread after recent activity.
	/// </summary>
	public Optional<ThreadAutoArchiveDuration?> DefaultAutoArchiveDuration { internal get; set; }

	/// <summary>
	/// Sets the channel's permission overwrites.
	/// </summary>
	public IEnumerable<DiscordOverwriteBuilder> PermissionOverwrites { internal get; set; }

	public Optional<ChannelFlags?> Flags { internal get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="ChannelEditModel"/> class.
	/// </summary>
	internal ForumChannelEditModel()
	{ }
}
