using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using DisCatSharp.Entities;

namespace DisCatSharp.Net;

/// <summary>
/// Represents a multipart HTTP request.
/// </summary>
internal sealed class MultipartWebRequest : BaseRestRequest
{
	/// <summary>
	/// Gets the dictionary of values attached to this request.
	/// </summary>
	public IReadOnlyDictionary<string, string>? Values { get; }

	/// <summary>
	/// Gets the dictionary of files attached to this request.
	/// </summary>
	public IReadOnlyDictionary<string, Stream>? Files { get; }

	/// <summary>
	/// Overwrites the file id start.
	/// </summary>
	public int? OverwriteFileIdStart { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="MultipartWebRequest"/> class.
	/// </summary>
	/// <param name="client">The client.</param>
	/// <param name="bucket">The bucket.</param>
	/// <param name="url">The url.</param>
	/// <param name="method">The method.</param>
	/// <param name="route">The route.</param>
	/// <param name="headers">The headers.</param>
	/// <param name="values">The values.</param>
	/// <param name="files">The files.</param>
	/// <param name="ratelimitWaitOverride">The ratelimit_wait_override.</param>
	/// <param name="overwriteFileIdStart">The file id start.</param>
	internal MultipartWebRequest(
		BaseDiscordClient client,
		RateLimitBucket bucket,
		Uri url,
		RestRequestMethod method,
		string route,
		IReadOnlyDictionary<string, string>? headers = null,
		IReadOnlyDictionary<string, string>? values = null,
		IEnumerable<DiscordMessageFile>? files = null,
		double? ratelimitWaitOverride = null,
		int? overwriteFileIdStart = null
	)
		: base(client, bucket, url, method, route, headers, ratelimitWaitOverride)
	{
		this.Values = values;
		this.OverwriteFileIdStart = overwriteFileIdStart;
		this.Files = files?.ToDictionary(x => x.Filename, x => x.Stream);
	}
}

/// <summary>
/// Represents a multipart HTTP request for stickers.
/// </summary>
internal sealed class MultipartStickerWebRequest : BaseRestRequest
{
	/// <summary>
	/// Gets the file.
	/// </summary>
	public DiscordMessageFile? File { get; }

	/// <summary>
	/// Gets the name.
	/// </summary>
	public string Name { get; }

	/// <summary>
	/// Gets the description.
	/// </summary>
	public string? Description { get; }

	/// <summary>
	/// Gets the tags.
	/// </summary>
	public string Tags { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="MultipartStickerWebRequest"/> class.
	/// </summary>
	/// <param name="client">The client.</param>
	/// <param name="bucket">The bucket.</param>
	/// <param name="url">The url.</param>
	/// <param name="method">The method.</param>
	/// <param name="route">The route.</param>
	/// <param name="name">The sticker name.</param>
	/// <param name="tags">The sticker tag.</param>
	/// <param name="description">The sticker description.</param>
	/// <param name="headers">The headers.</param>
	/// <param name="file">The file.</param>
	/// <param name="ratelimitWaitOverride">The ratelimit_wait_override.</param>
	internal MultipartStickerWebRequest(
		BaseDiscordClient client,
		RateLimitBucket bucket,
		Uri url,
		RestRequestMethod method,
		string route,
		string name,
		string tags,
		string? description = null,
		IReadOnlyDictionary<string, string>? headers = null,
		DiscordMessageFile? file = null,
		double? ratelimitWaitOverride = null
	)
		: base(client, bucket, url, method, route, headers, ratelimitWaitOverride)
	{
		this.File = file;
		this.Name = name;
		this.Description = description;
		this.Tags = tags;
	}
}
