using System;
using System.Collections.Generic;
using System.Linq;

using DisCatSharp.Entities;

namespace DisCatSharp.Interactivity;

/// <summary>
///     Provides helper methods for interactivity features.
/// </summary>
public static class InteractivityHelpers
{
	/// <summary>
	///     Recalculates the pages to include a footer with the current page number and total page count.
	/// </summary>
	/// <param name="pages">The list of pages to recalculate.</param>
	/// <returns>A new list of pages with updated footers.</returns>
	public static List<Page> Recalculate(this List<Page> pages)
	{
		List<Page> recalulatedPages = new(pages.Count);
		var pageCount = 1;
		foreach (var page in pages.Where(p => p.Embed is not null))
		{
			var tempPage = new Page();
			ArgumentNullException.ThrowIfNull(page.Embed);
			var replaceEmbed = new DiscordEmbedBuilder(page.Embed).WithFooter($"Page {pageCount}/{pages.Count}");
			tempPage.Embed = replaceEmbed.Build();
			recalulatedPages.Add(tempPage);
			pageCount++;
		}

		return recalulatedPages;
	}
}
