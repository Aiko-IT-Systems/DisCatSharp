using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

using Sentry;

namespace DisCatSharp.Telemetry;

/// <summary>
///     Built-in Sentry-backed diagnostics sink.
///     Uses a per-client <see cref="SentryClient" /> instance — does not touch global <see cref="SentrySdk" /> state.
///     Maintains a per-client <see cref="Scope" /> for breadcrumb accumulation.
/// </summary>
internal sealed class SentryDiagnosticsSink : ILibraryDiagnosticsSink
{
	private readonly SentryClient _client;
	private readonly Scope _scope;
	private readonly DiscordConfiguration _config;
	private readonly ReportFingerprintCache _reportFingerprintCache = new();

	/// <summary>
	///     Initializes a new Sentry diagnostics sink with the given options and configuration.
	/// </summary>
	/// <param name="options">The pre-configured Sentry options.</param>
	/// <param name="config">The Discord configuration for filtering/enrichment decisions.</param>
	public SentryDiagnosticsSink(SentryOptions options, DiscordConfiguration config)
	{
		this._client = new(options);
		this._scope = new(options);
		this._config = config;
	}

	/// <inheritdoc />
	public bool IsEnabled => true;

	/// <summary>
	///     Gets the underlying Sentry client for advanced scenarios.
	/// </summary>
	internal SentryClient Client => this._client;

	/// <inheritdoc />
	public void CaptureException(string source, Exception exception, IDictionary<string, object>? context = null, IDictionary<string, string>? tags = null)
	{
		if (context is not null && exception is SentryCapturableException sce)
			sce.AddSentryContext("Request", context);

		var evt = new SentryEvent(exception);
		if (context is not null && exception is not SentryCapturableException)
			ApplyContext(evt, "Context", context);

		ApplyExceptionContexts(evt, exception);
		this.ApplyDefaultTags(evt, source);
		ApplyTags(evt, tags);

		this._client.CaptureEvent(evt, this._scope, new());
		this.Flush();
	}

	/// <inheritdoc />
	public void CaptureReport(DiagnosticReport report)
	{
		SentryEvent sentryEvent = report.Exception is not null
			? new(report.Exception)
			: new();

		sentryEvent.Level = MapSeverity(report.Severity);
		sentryEvent.Logger = report.Logger;
		sentryEvent.Message = report.Message;

		this.ApplyDefaultTags(sentryEvent, report.Source);
		ApplyTags(sentryEvent, report.Tags);

		if (report.Extra is not null)
			foreach (var kvp in report.Extra)
				sentryEvent.SetExtra(kvp.Key, kvp.Value);

		if (report.UserInfo is not null)
			sentryEvent.User = new()
			{
				Id = report.UserInfo.Id,
				Username = report.UserInfo.Username,
				Other = new Dictionary<string, string>
				{
					{ "developer", report.UserInfo.DeveloperUserId ?? "not_given" },
					{ "email", report.UserInfo.FeedbackEmail ?? "not_given" }
				}
			};

		sentryEvent.Contexts["library"] = new Dictionary<string, object>
		{
			["api_version"] = this._config.Api.Version,
			["shard_id"] = this._config.Gateway.ShardId,
			["shard_count"] = this._config.Gateway.ShardCount,
			["intents"] = this._config.Intents.ToString()
		};

		if (!this._reportFingerprintCache.TryReserve(sentryEvent, report))
			return;

		var hasFoundFields = report.Extra?.ContainsKey("Found Fields") ?? false;
		if (!hasFoundFields)
			sentryEvent.SetFingerprint(GenerateFingerprint(sentryEvent, report));

		// File payload support: large scrubbed payloads sent as Sentry attachments
		var hint = new SentryHint();
		if (report.FilePayload is not null && report.FilePayloadName is not null)
			hint.Attachments.Add(new(AttachmentType.Default, new ByteAttachmentContent(report.FilePayload), report.FilePayloadName, "application/json"));

		this._client.CaptureEvent(sentryEvent, this._scope, hint);
		this.Flush();
	}

	/// <inheritdoc />
	public void StartSession()
	{
		var evt = new SentryEvent
		{
			Level = SentryLevel.Info,
			Logger = "DisCatSharp.Session",
			Message = "Gateway session started"
		};

		this.ApplyDefaultTags(evt, "DisCatSharp");
		evt.SetTag(DiagnosticTags.SessionEvent, "start");
		evt.Contexts["library"] = new Dictionary<string, object>
		{
			["api_version"] = this._config.Api.Version,
			["shard_id"] = this._config.Gateway.ShardId,
			["shard_count"] = this._config.Gateway.ShardCount,
			["intents"] = this._config.Intents.ToString()
		};

		this._client.CaptureEvent(evt, this._scope, new());
		this.Flush();
	}

	/// <inheritdoc />
	public void EndSession()
	{
		var evt = new SentryEvent
		{
			Level = SentryLevel.Info,
			Logger = "DisCatSharp.Session",
			Message = "Gateway session ended"
		};

		this.ApplyDefaultTags(evt, "DisCatSharp");
		evt.SetTag(DiagnosticTags.SessionEvent, "end");

		this._client.CaptureEvent(evt, this._scope, new());
		this.Flush();
	}

	/// <inheritdoc />
	public void AddBreadcrumb(string source, string category, string message, DiagnosticSeverity level = DiagnosticSeverity.Info, IDictionary<string, string>? data = null)
	{
		IReadOnlyDictionary<string, string>? readOnlyData = data is not null
			? new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(data))
			: null;

		this._scope.AddBreadcrumb(new(
			message: message,
			type: "default",
			data: readOnlyData,
			category: $"dcs.{source}.{category}",
			level: MapBreadcrumbLevel(level)));
	}

	/// <inheritdoc />
	public void EmitMetric(string source, string name, double value, string unit, IDictionary<string, string>? tags = null)
	{
		var evt = new SentryEvent
		{
			Level = SentryLevel.Debug,
			Logger = "DisCatSharp.Metrics",
			Message = $"dcs.metric.{name}"
		};

		this.ApplyDefaultTags(evt, source);
		evt.SetTag(DiagnosticTags.MetricName, name);
		evt.SetTag(DiagnosticTags.MetricUnit, unit);
		ApplyTags(evt, tags);

		evt.SetExtra("metric.name", name);
		evt.SetExtra("metric.value", value);
		evt.SetExtra("metric.unit", unit);

		this._client.CaptureEvent(evt, this._scope, new());
	}

	/// <inheritdoc />
	public IDisposable StartTiming(string source, string name, IDictionary<string, string>? tags = null)
		=> new DiagnosticTimer(this, source, name, tags);

	/// <inheritdoc />
	public void Flush()
		=> _ = this._client.FlushAsync(TimeSpan.FromSeconds(2));

	/// <summary>
	///     Applies default library tags to every Sentry event for consistent filtering.
	/// </summary>
	private void ApplyDefaultTags(SentryEvent evt, string source)
	{
		evt.SetTag(DiagnosticTags.Source, source);
		evt.SetTag(DiagnosticTags.ApiVersion, this._config.Api.Version);
		evt.SetTag(DiagnosticTags.ShardId, this._config.Gateway.ShardId.ToString());
		evt.SetTag(DiagnosticTags.ShardCount, this._config.Gateway.ShardCount.ToString());
	}

	/// <summary>
	///     Applies additional caller-provided tags to a Sentry event.
	/// </summary>
	private static void ApplyTags(SentryEvent evt, IDictionary<string, string>? tags)
	{
		if (tags is null)
			return;

		foreach (var (key, value) in tags)
			evt.SetTag(key, value);
	}

	/// <summary>
	///     Copies structured contexts from capturable exceptions onto the event payload.
	/// </summary>
	internal static void ApplyExceptionContexts(SentryEvent evt, Exception exception)
	{
		if (exception is not SentryCapturableException sce)
			return;

		foreach (var (key, value) in sce.Contexts)
			ApplyContext(evt, key, value);
	}

	internal static void ApplyContext(SentryEvent evt, string key, IDictionary<string, object> context)
		=> evt.Contexts[key] = new Dictionary<string, object>(context);

	/// <summary>
	///     Maps a <see cref="DiagnosticSeverity" /> to a Sentry <see cref="SentryLevel" />.
	/// </summary>
	private static SentryLevel MapSeverity(DiagnosticSeverity severity)
		=> severity switch
		{
			DiagnosticSeverity.Debug => SentryLevel.Debug,
			DiagnosticSeverity.Info => SentryLevel.Info,
			DiagnosticSeverity.Warning => SentryLevel.Warning,
			DiagnosticSeverity.Error => SentryLevel.Error,
			DiagnosticSeverity.Fatal => SentryLevel.Fatal,
			_ => SentryLevel.Info
		};

	/// <summary>
	///     Maps a <see cref="DiagnosticSeverity" /> to a Sentry <see cref="BreadcrumbLevel" />.
	/// </summary>
	private static BreadcrumbLevel MapBreadcrumbLevel(DiagnosticSeverity severity)
		=> severity switch
		{
			DiagnosticSeverity.Debug => BreadcrumbLevel.Debug,
			DiagnosticSeverity.Info => BreadcrumbLevel.Info,
			DiagnosticSeverity.Warning => BreadcrumbLevel.Warning,
			DiagnosticSeverity.Error => BreadcrumbLevel.Error,
			DiagnosticSeverity.Fatal => BreadcrumbLevel.Fatal,
			_ => BreadcrumbLevel.Info
		};

	/// <summary>
	///     Generates a fingerprint for Sentry event deduplication.
	///     Uses entity type, route, and close code tags for more precise grouping.
	/// </summary>
	internal static IEnumerable<string> GenerateFingerprint(SentryEvent ev, DiagnosticReport? report = null)
	{
		var fingerPrint = new List<string>
		{
			ev.Level.ToString()
		};

		if (!string.IsNullOrWhiteSpace(ev.Logger))
			fingerPrint.Add(ev.Logger);

		if (ev.Message?.Message is not null)
			fingerPrint.Add(ev.Message.Message);

		if (report?.AdditionalFingerprint is not null)
			fingerPrint.Add(report.AdditionalFingerprint);

		// Include entity type for missing-field grouping
		if (report?.Tags?.TryGetValue(DiagnosticTags.EntityType, out var entityType) is true)
			fingerPrint.Add($"entity:{entityType}");

		// Include REST route for REST error grouping
		if (report?.Tags?.TryGetValue(DiagnosticTags.RestRoute, out var route) is true)
			fingerPrint.Add($"route:{route}");

		var ex = ev.Exception;

		if (ex is null)
			return fingerPrint;

		fingerPrint.Add(ex.GetType().FullName!);
		if (!string.IsNullOrEmpty(ex.Message))
			fingerPrint.Add(ex.Message);

		if (ex.TargetSite is not null)
			fingerPrint.Add(ex.TargetSite.ToString()!);

		if (ex.InnerException is null)
			return fingerPrint;

		fingerPrint.Add(ex.InnerException.GetType().FullName!);
		if (!string.IsNullOrEmpty(ex.InnerException.Message))
			fingerPrint.Add(ex.InnerException.Message);

		return fingerPrint;
	}
}
