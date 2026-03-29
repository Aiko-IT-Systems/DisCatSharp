using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using Sentry;

namespace DisCatSharp.Telemetry;

/// <summary>
///     Maintains a bounded per-client cache of report fingerprints so noisy diagnostics can be emitted once per shape.
/// </summary>
internal sealed class ReportFingerprintCache
{
	private const int MaximumEntries = 256;
	private readonly ConcurrentDictionary<string, byte> _entries = new(StringComparer.Ordinal);
	private readonly ConcurrentQueue<string> _entryOrder = new();

	/// <summary>
	///     Attempts to reserve the report's computed fingerprint for capture.
	///     Returns <see langword="false" /> when the report has already been seen by this client instance.
	/// </summary>
	internal bool TryReserve(SentryEvent sentryEvent, DiagnosticReport report)
	{
		if (!report.DeduplicateByFingerprint)
			return true;

		var key = CreateKey(sentryEvent, report);
		if (!this._entries.TryAdd(key, 0))
			return false;

		this._entryOrder.Enqueue(key);
		this.Trim();
		return true;
	}

	/// <summary>
	///     Creates the stable cache key used for per-client report deduplication.
	/// </summary>
	internal static string CreateKey(SentryEvent sentryEvent, DiagnosticReport report)
	{
		var components = new List<string>(SentryDiagnosticsSink.GenerateFingerprint(sentryEvent, report));

		if (report.Tags is not null)
			components.AddRange(report.Tags
				.OrderBy(static kvp => kvp.Key, StringComparer.Ordinal)
				.Select(static kvp => $"tag:{kvp.Key}={kvp.Value}"));

		if (report.Extra?.TryGetValue("Found Fields", out var foundFields) is true && foundFields is not null)
			components.Add($"fields:{foundFields}");

		return string.Join("\u001F", components);
	}

	private void Trim()
	{
		// Minor TOCTOU under contention is acceptable: the cache is a soft deduplication aid,
		// not a hard bound — transiently exceeding MaximumEntries is fine.
		while (this._entries.Count > MaximumEntries && this._entryOrder.TryDequeue(out var staleKey))
			_ = this._entries.TryRemove(staleKey, out _);
	}
}
