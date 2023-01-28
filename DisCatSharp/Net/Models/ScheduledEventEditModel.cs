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

using System;
using System.IO;

using DisCatSharp.Entities;
using DisCatSharp.Enums;

namespace DisCatSharp.Net.Models;

/// <summary>
/// Represents a scheduled event edit model.
/// </summary>
public class ScheduledEventEditModel : BaseEditModel
{
	/// <summary>
	/// Gets or sets the channel.
	/// </summary>
	public Optional<DiscordChannel> Channel { get; set; }

	/// <summary>
	/// Gets or sets the location.
	/// </summary>
	public Optional<string> Location { get; set; }

	/// <summary>
	/// Gets or sets the name.
	/// </summary>
	public Optional<string> Name { get; set; }

	/// <summary>
	/// Gets or sets the description.
	/// </summary>
	public Optional<string> Description { get; set; }

	/// <summary>
	/// Gets or sets the time to schedule the scheduled event.
	/// </summary>
	public Optional<DateTimeOffset> ScheduledStartTime { get; internal set; }

	/// <summary>
	/// Gets or sets the time when the scheduled event is scheduled to end.
	/// </summary>
	public Optional<DateTimeOffset> ScheduledEndTime { get; internal set; }

	/// <summary>
	/// Gets or sets the entity type of the scheduled event.
	/// </summary>
	public Optional<ScheduledEventEntityType> EntityType { get; set; }

	/// <summary>
	/// Gets or sets the cover image as base64.
	/// </summary>
	public Optional<Stream> CoverImage { get; set; }

	/// <summary>
	/// Gets or sets the status of the scheduled event.
	/// </summary>
	public Optional<ScheduledEventStatus> Status { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="ScheduledEventEditModel"/> class.
	/// </summary>
	internal ScheduledEventEditModel() { }
}
