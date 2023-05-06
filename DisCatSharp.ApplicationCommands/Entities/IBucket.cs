// This file is part of the DisCatSharp project.
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

namespace DisCatSharp.ApplicationCommands.Entities;

/// <summary>
/// Defines the standard contract for bucket feature
/// </summary>
public interface IBucket
{
	/// <summary>
	/// Gets the ID of the user whom this cooldown is associated
	/// </summary>
	ulong UserId { get; }

	/// <summary>
	/// Gets the ID of the channel with which this cooldown is associated
	/// </summary>
	ulong ChannelId { get; }

	/// <summary>
	/// Gets the ID of the guild with which this cooldown is associated
	/// </summary>
	ulong GuildId { get; }

	/// <summary>
	/// Gets the ID of the bucket. This is used to distinguish between cooldown buckets
	/// </summary>
	string BucketId { get; }

	/// <summary>
	/// Gets the remaining number of uses before the cooldown is triggered
	/// </summary>
	int RemainingUses { get; }

	/// <summary>
	/// Gets the maximum number of times this command can be used in a given timespan
	/// </summary>
	int MaxUses { get; }

	/// <summary>
	/// Gets the date and time at which the cooldown resets
	/// </summary>
	DateTimeOffset ResetsAt { get; }

	/// <summary>
	/// Get the time after which this cooldown resets
	/// </summary>
	TimeSpan Reset { get; }
}
