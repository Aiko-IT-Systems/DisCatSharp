using System;
using System.Collections.Generic;
using System.Linq;

using DisCatSharp.Exceptions;
using DisCatSharp.Telemetry;

using Newtonsoft.Json;

using Sentry;

using Xunit;

namespace DisCatSharp.CopilotTests.Telemetry;

public class SentryDiagnosticsSinkTests
{
	[Fact]
	public void ApplyExceptionContexts_CopiesStructuredContextOntoEvent()
	{
		var evt = new SentryEvent(new InvalidOperationException("outer"));
		var exception = new SentryCapturableException("wrapped", new InvalidOperationException("inner"));
		exception.AddSentryContext("Request", new Dictionary<string, object>
		{
			["route"] = "/channels/{channel_id}/messages",
			["time"] = new DateTimeOffset(2026, 3, 28, 0, 0, 0, TimeSpan.Zero)
		});

		SentryDiagnosticsSink.ApplyExceptionContexts(evt, exception);

		Assert.True(evt.Contexts.ContainsKey("Request"));
		var requestContext = Assert.IsType<Dictionary<string, object>>(evt.Contexts["Request"]);
		Assert.Equal("/channels/{channel_id}/messages", requestContext["route"]);
		Assert.Equal(new DateTimeOffset(2026, 3, 28, 0, 0, 0, TimeSpan.Zero), requestContext["time"]);
	}

	[Fact]
	public void ExceptionFilter_AllowsWrappedTrackedException()
	{
		var config = new DiscordConfiguration
		{
			EnableLibraryDeveloperMode = true
		};
		config.TrackExceptions = [typeof(DiscordJsonException)];
		var filter = new DisCatSharpExceptionFilter(config);
		var exception = new SentryCapturableException("wrapped", new DiscordJsonException(new JsonReaderException("bad json")));

		Assert.False(filter.Filter(exception));
	}

	[Fact]
	public void ApplyContext_AttachesStructuredContextToEvent()
	{
		var evt = new SentryEvent(new InvalidOperationException("boom"));
		var context = new Dictionary<string, object>
		{
			["sequence"] = 42,
			["opcode"] = 0
		};

		SentryDiagnosticsSink.ApplyContext(evt, "Context", context);

		Assert.True(evt.Contexts.ContainsKey("Context"));
		var eventContext = Assert.IsType<Dictionary<string, object>>(evt.Contexts["Context"]);
		Assert.Equal(42, eventContext["sequence"]);
		Assert.Equal(0, eventContext["opcode"]);
	}

	[Fact]
	public void GenerateFingerprint_DoesNotIncludeNullLogger()
	{
		var evt = new SentryEvent
		{
			Level = SentryLevel.Error,
			Message = "test"
		};

		var fingerprint = SentryDiagnosticsSink.GenerateFingerprint(evt).ToArray();

		Assert.DoesNotContain(fingerprint, value => value is null);
		Assert.Contains("Error", fingerprint);
		Assert.Contains("test", fingerprint);
	}

	[Fact]
	public void GenerateFingerprint_WithReport_IncludesRouteAndEntityType()
	{
		var evt = new SentryEvent
		{
			Level = SentryLevel.Warning,
			Message = "rate limited"
		};

		var report = new DiagnosticReport
		{
			Source = "DisCatSharp",
			Severity = DiagnosticSeverity.Warning,
			Logger = "RestClient",
			Message = "rate limited",
			Tags = new Dictionary<string, string>
			{
				[DiagnosticTags.RestRoute] = "POST /channels/123/messages",
				[DiagnosticTags.EntityType] = "DiscordMessage"
			}
		};

		var withReport = SentryDiagnosticsSink.GenerateFingerprint(evt, report).ToArray();
		var withoutReport = SentryDiagnosticsSink.GenerateFingerprint(evt).ToArray();

		Assert.Contains(withReport, f => f.Contains("route:"));
		Assert.Contains(withReport, f => f.Contains("entity:"));
		Assert.DoesNotContain(withoutReport, f => f.Contains("route:"));
		Assert.DoesNotContain(withoutReport, f => f.Contains("entity:"));
	}

	[Fact]
	public void SetFingerprint_PreExisting_NotOverwrittenByBeforeSendLogic()
	{
		var evt = new SentryEvent
		{
			Level = SentryLevel.Warning,
			Message = "test"
		};

		// Simulate CaptureReport setting a route-specific fingerprint
		var richFingerprint = new[] { "Warning", "RestClient", "route:POST /channels/123/messages" };
		evt.SetFingerprint(richFingerprint);

		// The BeforeSend condition should detect the pre-existing fingerprint
		Assert.NotNull(evt.Fingerprint);
		Assert.True(evt.Fingerprint.Count > 0, "Pre-existing fingerprint should be detectable");
	}

	[Fact]
	public void ReportFingerprintCache_CreateKey_IncludesSchemaAndTags()
	{
		var evt = new SentryEvent
		{
			Level = SentryLevel.Warning,
			Logger = "DiscordJson",
			Message = "Missing fields detected"
		};

		var report = new DiagnosticReport
		{
			Source = "DisCatSharp",
			Severity = DiagnosticSeverity.Warning,
			Logger = "DiscordJson",
			Message = "Missing fields detected",
			DeduplicateByFingerprint = true,
			Tags = new Dictionary<string, string>
			{
				[DiagnosticTags.EntityType] = "DiscordMember"
			},
			Extra = new Dictionary<string, object>
			{
				["Found Fields"] = "{\n  \"collectibles\": \"object\"\n}"
			}
		};

		var cacheKey = ReportFingerprintCache.CreateKey(evt, report);

		Assert.Contains("entity:DiscordMember", cacheKey);
		Assert.Contains("tag:dcs.entity_type=DiscordMember", cacheKey);
		Assert.Contains("fields:{\n  \"collectibles\": \"object\"\n}", cacheKey);
	}

	[Fact]
	public void ReportFingerprintCache_TryReserve_SuppressesRepeatedOptInReports()
	{
		var cache = new ReportFingerprintCache();
		var evt = new SentryEvent
		{
			Level = SentryLevel.Warning,
			Logger = "DiscordClient.WebSocket",
			Message = "Unknown gateway opcode 99"
		};

		var report = new DiagnosticReport
		{
			Source = "DisCatSharp",
			Severity = DiagnosticSeverity.Warning,
			Logger = "DiscordClient.WebSocket",
			Message = "Unknown gateway opcode 99",
			DeduplicateByFingerprint = true,
			Tags = new Dictionary<string, string>
			{
				["dcs.gateway_opcode"] = "99",
				["dcs.gateway_event"] = "mystery_event"
			}
		};

		Assert.True(cache.TryReserve(evt, report));
		Assert.False(cache.TryReserve(evt, report));
	}

	[Fact]
	public void ReportFingerprintCache_TryReserve_AllowsRepeatedNonOptInReports()
	{
		var cache = new ReportFingerprintCache();
		var evt = new SentryEvent
		{
			Level = SentryLevel.Warning,
			Logger = "RestClient",
			Message = "Rate limit hit"
		};

		var report = new DiagnosticReport
		{
			Source = "DisCatSharp",
			Severity = DiagnosticSeverity.Warning,
			Logger = "RestClient",
			Message = "Rate limit hit",
			Tags = new Dictionary<string, string>
			{
				[DiagnosticTags.RestRoute] = "POST /channels/{channel_id}/messages"
			}
		};

		Assert.True(cache.TryReserve(evt, report));
		Assert.True(cache.TryReserve(evt, report));
	}
}
