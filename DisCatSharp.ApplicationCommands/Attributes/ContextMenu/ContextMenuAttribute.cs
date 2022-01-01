// This file is part of the DisCatSharp project, based off DSharpPlus.
//
// Copyright (c) 2021-2022 AITSYS
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

using DisCatSharp.Enums;

namespace DisCatSharp.ApplicationCommands
{

	/// <summary>
	/// Marks this method as a context menu.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	public sealed class ContextMenuAttribute : Attribute
	{
		/// <summary>
		/// Gets the name of this context menu.
		/// </summary>
		public string Name { get; internal set; }

		/// <summary>
		/// Gets the type of this context menu.
		/// </summary>
		public ApplicationCommandType Type { get; internal set; }

		/// <summary>
		/// Gets whether this command is enabled by default.
		/// </summary>
		public bool DefaultPermission { get; internal set; }

		/// <summary>
		/// Marks this method as a context menu.
		/// </summary>
		/// <param name="type">The type of the context menu.</param>
		/// <param name="name">The name of the context menu.</param>
		/// <param name="defaultPermission">The default permission of the context menu.</param>
		public ContextMenuAttribute(ApplicationCommandType type, string name, bool defaultPermission = true)
		{
			if (type == ApplicationCommandType.ChatInput)
				throw new ArgumentException("Context menus cannot be of type ChatInput (Slash).");

			this.Type = type;
			this.Name = name;
			this.DefaultPermission = defaultPermission;
		}
	}
}
