using System;
using System.Collections.Generic;
using System.Linq;

using DisCatSharp.Entities;
using DisCatSharp.Interactivity.Entities;

namespace DisCatSharp.Interactivity;

/// <summary>
///     Provides helper methods for interactivity features.
/// </summary>
public static class InteractivityHelpers
{
	/// <summary>
	///     Recalculates the pages to include a footer with the current page number and total page count. Only works for embed pages.
	/// </summary>
	/// <param name="pages">The list of pages to recalculate.</param>
	/// <returns>A new list of pages with updated footers.</returns>
	public static List<Page> Recalculate(this List<Page> pages)
	{
		if (pages.All(p => p.Embed is null))
			throw new InvalidOperationException("Recalculate can only be used on pages that contain at least one embed page.");

		List<Page> recalculatedPages = new(pages.Count);
		var pageCount = 1;
		foreach (var page in pages)
		{
			if (page.Embed is not null)
			{
				var replaceEmbed = new DiscordEmbedBuilder(page.Embed).WithFooter($"Page {pageCount}/{pages.Count}");
				recalculatedPages.Add(new(page.Content, replaceEmbed));
			}
			else
			{
				recalculatedPages.Add(page);
			}

			pageCount++;
		}

		return recalculatedPages;
	}
}
