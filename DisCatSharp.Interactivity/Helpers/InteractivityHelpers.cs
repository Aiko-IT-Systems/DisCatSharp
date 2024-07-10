using System.Collections.Generic;

using DisCatSharp.Entities;

namespace DisCatSharp.Interactivity;

public static class InteractivityHelpers
{
	public static List<Page> Recalculate(this List<Page> pages)
	{
		List<Page> recalulatedPages = new(pages.Count);
		var pageCount = 1;
		foreach (var page in pages)
		{
			var tempPage = new Page();
			var replaceEmbed = new DiscordEmbedBuilder(page.Embed).WithFooter($"Page {pageCount}/{pages.Count}");
			tempPage.Embed = replaceEmbed.Build();
			recalulatedPages.Add(tempPage);
			pageCount++;
		}

		return recalulatedPages;
	}
}
