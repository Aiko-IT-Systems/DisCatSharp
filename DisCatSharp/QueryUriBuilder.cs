// This file is part of the DisCatSharp project, based off DSharpPlus.
//
// Copyright (c) 2021-2023 AITSYS
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

		this.SourceUri = new Uri(uri);
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
		this._queryParams.Add(new KeyValuePair<string, string>(key, value));
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
