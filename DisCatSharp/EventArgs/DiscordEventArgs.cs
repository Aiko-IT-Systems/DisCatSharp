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

using DisCatSharp.Common.Utilities;

using Microsoft.Extensions.DependencyInjection;

namespace DisCatSharp.EventArgs;

// Note: this might seem useless, but should we ever need to add a common property or method to all event arg
// classes, it would be useful to already have a base for all of it.

/// <summary>
/// Common base for all other <see cref="DiscordClient"/>-related event argument classes.
/// </summary>
public abstract class DiscordEventArgs : AsyncEventArgs
{
	/// <summary>
	/// <para>Gets the service provider.</para>
	/// <para>This allows passing data around without resorting to static members.</para>
	/// <para>Defaults to an empty service provider.</para>
	/// </summary>
	public IServiceProvider ServiceProvider { get; internal set; } = new ServiceCollection().BuildServiceProvider(true);

	/// <summary>
	/// Initializes a new instance of the <see cref="DiscordEventArgs"/> class.
	/// </summary>
	protected DiscordEventArgs(IServiceProvider provider)
	{
		if (provider != null)
			this.ServiceProvider = provider.CreateScope().ServiceProvider;
	}
}
