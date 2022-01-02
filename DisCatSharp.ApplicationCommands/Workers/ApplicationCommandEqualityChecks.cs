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
using System.Collections.Generic;
using System.Text;

using DisCatSharp.Entities;

namespace DisCatSharp.ApplicationCommands
{
	internal static class ApplicationCommandEqualityChecks
	{
		public static bool IsEqualTo(this DiscordApplicationCommand ac1, DiscordApplicationCommand targetApplicationCommand)
		{
			if (targetApplicationCommand == null || ac1 == null)
				return false;

			DiscordApplicationCommand sourceApplicationCommand = new(
				ac1.Name, ac1.Description, ac1.Options,
				ac1.DefaultPermission, ac1.Type,
				ac1.NameLocalizations, ac1.DescriptionLocalizations
			);

			return sourceApplicationCommand.SoftEqual(targetApplicationCommand);
		}

		/// <summary>
		/// Checks if two <see cref="DisCatSharp.Entities.DiscordApplicationCommand"/>s are the same.
		/// Excluding id, application id and version here.
		/// </summary>
		/// <param name="source">Source application command.</param>
		/// <param name="other">Application command to check against.</param>
		private static bool SoftEqual(this DiscordApplicationCommand source, DiscordApplicationCommand other)
			=> (source.Name == other.Name) && (source.Description == other.Description)
			&& (source.Options == other.Options) && (source.DefaultPermission == other.DefaultPermission)
			&& (source.NameLocalizations == other.NameLocalizations) && (source.DescriptionLocalizations == other.DescriptionLocalizations)
			&& (source.Type == other.Type);
		// && (source.Permission == other.Permission) && (source.DmPermission == other.DmPermission)

	}
}
