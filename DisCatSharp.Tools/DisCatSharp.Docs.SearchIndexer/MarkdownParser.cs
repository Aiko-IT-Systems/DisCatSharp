using System.Text.RegularExpressions;
using YamlDotNet.Serialization;

namespace DisCatSharp.Docs.SearchIndexer;

internal static partial class MarkdownParser
{
	private static readonly IDeserializer FrontMatterDeserializer = new DeserializerBuilder().Build();

	public static ParsedMarkdown Parse(string markdown, string fallbackTitle)
	{
		var normalized = markdown.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
		var body = normalized;
		var metadata = new Dictionary<object, object?>();

		if (normalized.StartsWith("---\n", StringComparison.Ordinal))
		{
			var closing = normalized.IndexOf("\n---\n", 4, StringComparison.Ordinal);
			if (closing >= 0)
			{
				var yaml = normalized[4..closing];
				metadata = FrontMatterDeserializer.Deserialize<Dictionary<object, object?>>(yaml) ?? [];
				body = normalized[(closing + 5)..];
			}
		}

		var heading = HeadingRegex().Match(body);
		var title = GetMetadata(metadata, "title")
			?? (heading.Success ? heading.Groups[1].Value.Trim() : null);
		title = string.IsNullOrWhiteSpace(title) ? fallbackTitle : title;

		var description = GetMetadata(metadata, "description") ?? FirstParagraph(body);
		return new ParsedMarkdown(NormalizeInline(title), NormalizeInline(description), body.Trim());
	}

	private static string? GetMetadata(Dictionary<object, object?> metadata, string name)
	{
		foreach (var (key, value) in metadata)
		{
			if (string.Equals(Convert.ToString(key), name, StringComparison.OrdinalIgnoreCase))
				return Convert.ToString(value);
		}

		return null;
	}

	private static string FirstParagraph(string body)
	{
		var paragraphs = Regex.Split(body, "\\n\\s*\\n", RegexOptions.CultureInvariant);
		foreach (var paragraph in paragraphs)
		{
			var candidate = paragraph.Trim();
			if (candidate.Length == 0 || candidate.StartsWith('#') || candidate.StartsWith("<section", StringComparison.OrdinalIgnoreCase))
				continue;

			return NormalizeInline(candidate);
		}

		return string.Empty;
	}

	internal static string NormalizeInline(string? value)
	{
		if (string.IsNullOrWhiteSpace(value))
			return string.Empty;

		var text = XrefRegex().Replace(value, match => match.Groups[1].Value);
		text = HtmlRegex().Replace(text, " ");
		text = MarkdownLinkRegex().Replace(text, "$1");
		text = MarkdownPunctuationRegex().Replace(text, string.Empty);
		return WhitespaceRegex().Replace(text, " ").Trim();
	}

	[GeneratedRegex("(?m)^#\\s+(.+)$", RegexOptions.CultureInvariant)]
	private static partial Regex HeadingRegex();

	[GeneratedRegex("<xref[^>]*href=[\"']([^\"']+)[\"'][^>]*>(?:</xref>)?", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
	private static partial Regex XrefRegex();

	[GeneratedRegex("<[^>]+>", RegexOptions.CultureInvariant)]
	private static partial Regex HtmlRegex();

	[GeneratedRegex("\\[([^\\]]+)\\]\\([^\\)]+\\)", RegexOptions.CultureInvariant)]
	private static partial Regex MarkdownLinkRegex();

	[GeneratedRegex("[`*_~]", RegexOptions.CultureInvariant)]
	private static partial Regex MarkdownPunctuationRegex();

	[GeneratedRegex("\\s+", RegexOptions.CultureInvariant)]
	private static partial Regex WhitespaceRegex();
}
