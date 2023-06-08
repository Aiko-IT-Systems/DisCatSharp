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
// FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;

using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.ApplicationCommands.Enums;

namespace DisCatSharp.ApplicationCommands.Entities;

/// <summary>
/// Cooldown feature contract
/// </summary>
/// <typeparam name="TContextType">Type of <see cref="BaseContext"/> in which this cooldown handles</typeparam>
/// <typeparam name="TBucketType">Type of Cooldown bucket</typeparam>
public interface ICooldown<in TContextType, out TBucketType>
	where TContextType : BaseContext
	where TBucketType : CooldownBucket
{
	/// <summary>
	/// Gets the maximum number of uses before this command triggers a cooldown for its bucket.
	/// </summary>
	int MaxUses { get; }

	/// <summary>
	/// Gets the time after which the cooldown is reset.
	/// </summary>
	TimeSpan Reset { get; }

	/// <summary>
	/// Gets the type of the cooldown bucket. This determines how a cooldown is applied.
	/// </summary>
	CooldownBucketType BucketType { get; }

	/// <summary>
	/// Calculates the cooldown remaining for given context.
	/// </summary>
	/// <param name="ctx">Context for which to calculate the cooldown.</param>
	/// <returns>Remaining cooldown, or zero if no cooldown is active</returns>
	TimeSpan GetRemainingCooldown(TContextType ctx);

	/// <summary>
	/// Gets a cooldown bucket for given context
	/// </summary>
	/// <param name="ctx">Command context to get cooldown bucket for.</param>
	/// <returns>Requested cooldown bucket, or null if one wasn't present</returns>
	TBucketType GetBucket(TContextType ctx);
}
