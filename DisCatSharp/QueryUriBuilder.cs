using System;
using System.Collections.Generic;
using System.Linq;

namespace DisCatSharp;

/// <summary>
/// Represents a query uri builder.
/// </summary>
internal class QueryUriBuilder
{
	/// <summary>
	/// Gets the source uri.
	/// </summary>
	public Uri SourceUri { get; }

	/// <summary>
	/// Gets the query parameters.
	/// </summary>
	public IReadOnlyList<KeyValuePair<string, string>> QueryParameters => this._queryParams;
	private readonly List<KeyValuePair<string, string>> _queryParams = new();

	/// <summary>
	/// Initializes a new instance of the <see cref="QueryUriBuilder"/> class.
	/// </summary>
	/// <param name="uri">The uri.</param>
	public QueryUriBuilder(string uri)
	{
		if (uri == null)
			throw new ArgumentNullException(nameof(uri));

		this.SourceUri = new(uri);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="QueryUriBuilder"/> class.
	/// </summary>
	/// <param name="uri">The uri.</param>
	public QueryUriBuilder(Uri uri)
	{
		if (uri == null)
			throw new ArgumentNullException(nameof(uri));

		this.SourceUri = uri;
	}

	/// <summary>
	/// Adds a parameter.
	/// </summary>
	/// <param name="key">The key to be added.</param>
	/// <param name="value">The value to be added.</param>
	public QueryUriBuilder AddParameter(string key, string value)
	{
		this._queryParams.Add(new(key, value));
		return this;
	}

	/// <summary>
	/// Builds the uri.
	/// </summary>
	public Uri Build() =>
		new UriBuilder(this.SourceUri)
		{
			Query = string.Join("&", this._queryParams.Select(e => Uri.EscapeDataString(e.Key) + '=' + Uri.EscapeDataString(e.Value)))
		}.Uri;

	/// <summary>
	/// Returns a readable string.
	/// </summary>
	public override string ToString() => this.Build().ToString();
}
