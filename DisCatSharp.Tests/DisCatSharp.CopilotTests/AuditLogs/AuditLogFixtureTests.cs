using System;
using System.IO;

using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Net.AuditLogs;

using Newtonsoft.Json;

using Xunit;

namespace DisCatSharp.Copilot.Tests.AuditLogs;

/// <summary>
///     Verifies that the sanitized audit log fixture files remain parseable and representative.
/// </summary>
public sealed class AuditLogFixtureTests
{
	/// <summary>
	///     Gets the sanitized fixture cases to validate.
	/// </summary>
	public static TheoryData<string, Type, AuditLogActionCategory> FixtureCases => new()
	{
		{ "guild-update.json", typeof(DiscordGuildAuditLogEntry), AuditLogActionCategory.Update },
		{ "voice-channel-status-create.json", typeof(DiscordVoiceChannelStatusAuditLogEntry), AuditLogActionCategory.Create },
		{ "guild-profile-update.json", typeof(DiscordGuildProfileAuditLogEntry), AuditLogActionCategory.Update },
		{ "member-role-update.json", typeof(DiscordMemberAuditLogEntry), AuditLogActionCategory.Update },
		{ "auto-moderation-rule-update.json", typeof(DiscordAutoModerationRuleAuditLogEntry), AuditLogActionCategory.Update },
		{ "overwrite-update.json", typeof(DiscordOverwriteAuditLogEntry), AuditLogActionCategory.Update },
		{ "thread-update.json", typeof(DiscordThreadAuditLogEntry), AuditLogActionCategory.Update }
	};

	/// <summary>
	///     Ensures that each sanitized fixture can be deserialized and parsed into the expected entry family.
	/// </summary>
	/// <param name="fixtureName">The fixture file name.</param>
	/// <param name="expectedEntryType">The expected parsed entry type.</param>
	/// <param name="expectedCategory">The expected broad action category.</param>
	[Theory]
	[MemberData(nameof(FixtureCases))]
	public void SanitizedFixture_DeserializesAndParses(string fixtureName, Type expectedEntryType, AuditLogActionCategory expectedCategory)
	{
		using var harness = new AuditLogTestHarness();
		var rawAuditLog = LoadFixture(fixtureName);

		var page = AuditLogEntryParserRegistry.Instance.ParsePage(harness.Guild, rawAuditLog, isAscending: false);
		var entry = Assert.Single(page.Entries);

		Assert.IsType(expectedEntryType, entry);
		Assert.Equal(expectedCategory, entry.ActionCategory);
		Assert.NotNull(entry.Actor);
	}

	/// <summary>
	///     Loads a sanitized raw audit log fixture from disk.
	/// </summary>
	/// <param name="fixtureName">The fixture file name.</param>
	/// <returns>The deserialized raw audit log payload.</returns>
	private static RawAuditLog LoadFixture(string fixtureName)
	{
		var fixturePath = Path.Combine(AppContext.BaseDirectory, "AuditLogs", "Fixtures", fixtureName);
		var json = File.ReadAllText(fixturePath);
		return JsonConvert.DeserializeObject<RawAuditLog>(json) ?? throw new InvalidOperationException($"Failed to deserialize fixture '{fixtureName}'.");
	}

	/// <summary>
	///     Provides a minimal guild/client fixture for fixture parsing tests.
	/// </summary>
	private sealed class AuditLogTestHarness : IDisposable
	{
		/// <summary>
		///     Gets the client backing the fixture.
		/// </summary>
		public DiscordClient Client { get; }

		/// <summary>
		///     Gets the guild under test.
		/// </summary>
		public DiscordGuild Guild { get; }

		/// <summary>
		///     Initializes a new instance of the <see cref="AuditLogTestHarness"/> class.
		/// </summary>
		public AuditLogTestHarness()
		{
			this.Client = new(new DiscordConfiguration
			{
				Token = "copilot-tests-token",
				MessageCacheSize = 0,
				AutoReconnect = false
			});

			this.Guild = new DiscordGuild
			{
				Discord = this.Client,
				Id = 123456789012345678UL,
				Name = "Audit Fixture Guild"
			};
		}

		/// <summary>
		///     Disposes the underlying client resources.
		/// </summary>
		public void Dispose()
			=> this.Client.Dispose();
	}
}
