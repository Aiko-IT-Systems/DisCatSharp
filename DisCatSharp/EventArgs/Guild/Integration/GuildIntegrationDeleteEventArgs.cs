// This file is part of the DisCatSharp project, based of DSharpPlus.
//
// Copyright (c) 2021 AITSYS
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
using DisCatSharp.Entities;

namespace DisCatSharp.EventArgs
{
	/// <summary>
	/// Represents arguments for <see cref="DiscordClient.GuildIntegrationDeleted"/> event.
	/// </summary>
	public class GuildIntegrationDeleteEventArgs : DiscordEventArgs
	{
		/// <summary>
		/// Gets the integration id which where deleted.
		/// </summary>
		///
		public ulong IntegrationId { get; internal set; }

		/// <summary>
		/// Gets the guild where the integration which where deleted.
		/// </summary>
		public DiscordGuild Guild { get; internal set; }

		/// <summary>
		/// Gets the application id of the integration which where deleted.
		/// </summary>
		public ulong? ApplicationId { get; internal set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="GuildIntegrationDeleteEventArgs"/> class.
		/// </summary>
		internal GuildIntegrationDeleteEventArgs(IServiceProvider provider) : base(provider) { }
	}
}
