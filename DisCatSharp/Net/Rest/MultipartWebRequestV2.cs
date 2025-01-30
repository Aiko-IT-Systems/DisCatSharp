// MultipartWebRequestV2.cs

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using DisCatSharp.Entities;

namespace DisCatSharp.Net.V2;

public sealed class MultipartWebRequestV2 : BaseRestRequestV2
{
	public MultipartWebRequestV2(
		BaseDiscordClient client,
		RateLimitBucketV2 bucket,
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

	public IReadOnlyDictionary<string, string>? Values { get; }
	public IReadOnlyDictionary<string, Stream>? Files { get; }
	public int? OverwriteFileIdStart { get; }
}

public sealed class MultipartStickerWebRequestV2 : BaseRestRequestV2
{
	public MultipartStickerWebRequestV2(
		BaseDiscordClient client,
		RateLimitBucketV2 bucket,
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

	public DiscordMessageFile? File { get; }
	public string Name { get; }
	public string? Description { get; }
	public string Tags { get; }
}
