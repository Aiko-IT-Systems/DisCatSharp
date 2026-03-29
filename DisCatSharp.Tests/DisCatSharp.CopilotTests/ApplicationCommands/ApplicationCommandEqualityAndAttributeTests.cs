using System;
using System.Collections.Generic;
using System.Linq;

using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Checks;
using DisCatSharp.Enums;

using Xunit;

namespace DisCatSharp.Copilot.Tests;

/// <summary>
///     Tests for the ApplicationCommands audit fixes (equality checks, attribute validation, etc.)
/// </summary>
public class ApplicationCommandEqualityAndAttributeTests
{
	// ===== C5: NullableSequenceEqual duplicate-distribution bug =====

	[Fact]
	public void NullableSequenceEqual_DifferentDistributions_ReturnsFalse()
	{
		var source = new List<ApplicationCommandIntegrationTypes>
		{
			ApplicationCommandIntegrationTypes.GuildInstall,
			ApplicationCommandIntegrationTypes.GuildInstall,
			ApplicationCommandIntegrationTypes.UserInstall
		};
		var target = new List<ApplicationCommandIntegrationTypes>
		{
			ApplicationCommandIntegrationTypes.GuildInstall,
			ApplicationCommandIntegrationTypes.UserInstall,
			ApplicationCommandIntegrationTypes.UserInstall
		};

		Assert.False(source.NullableSequenceEqual(target));
	}

	[Fact]
	public void NullableSequenceEqual_SameElements_ReturnsTrue()
	{
		var source = new List<ApplicationCommandIntegrationTypes>
		{
			ApplicationCommandIntegrationTypes.GuildInstall,
			ApplicationCommandIntegrationTypes.UserInstall
		};
		var target = new List<ApplicationCommandIntegrationTypes>
		{
			ApplicationCommandIntegrationTypes.UserInstall,
			ApplicationCommandIntegrationTypes.GuildInstall
		};

		Assert.True(source.NullableSequenceEqual(target));
	}

	[Fact]
	public void NullableSequenceEqual_BothNull_ReturnsTrue()
	{
		List<int>? source = null;
		List<int>? target = null;

		Assert.True(source.NullableSequenceEqual(target));
	}

	[Fact]
	public void NullableSequenceEqual_OneNull_ReturnsFalse()
	{
		List<int>? source = [1, 2, 3];
		List<int>? target = null;

		Assert.False(source.NullableSequenceEqual(target));
	}

	[Fact]
	public void NullableSequenceEqual_DifferentCounts_ReturnsFalse()
	{
		List<int> source = [1, 2];
		List<int> target = [1, 2, 3];

		Assert.False(source.NullableSequenceEqual(target));
	}

	// ===== C7/M5: MinimumLength/MaximumLength validation =====

	[Fact]
	public void MinimumLengthAttribute_ValueOver6000_Throws()
	{
		Assert.Throws<ArgumentException>(() => new MinimumLengthAttribute(6001));
	}

	[Fact]
	public void MinimumLengthAttribute_Value6000_DoesNotThrow()
	{
		var attr = new MinimumLengthAttribute(6000);
		Assert.Equal(6000, attr.Value);
	}

	[Fact]
	public void MinimumLengthAttribute_Value601_DoesNotThrow()
	{
		var attr = new MinimumLengthAttribute(601);
		Assert.Equal(601, attr.Value);
	}

	[Fact]
	public void MinimumLengthAttribute_NegativeValue_Throws()
	{
		Assert.Throws<ArgumentException>(() => new MinimumLengthAttribute(-1));
	}

	[Fact]
	public void MinimumLengthAttribute_ZeroValue_DoesNotThrow()
	{
		var attr = new MinimumLengthAttribute(0);
		Assert.Equal(0, attr.Value);
	}

	[Fact]
	public void MaximumLengthAttribute_ValueOver6000_Throws()
	{
		Assert.Throws<ArgumentException>(() => new MaximumLengthAttribute(6001));
	}

	[Fact]
	public void MaximumLengthAttribute_Value6000_DoesNotThrow()
	{
		var attr = new MaximumLengthAttribute(6000);
		Assert.Equal(6000, attr.Value);
	}

	[Fact]
	public void MaximumLengthAttribute_Value601_DoesNotThrow()
	{
		var attr = new MaximumLengthAttribute(601);
		Assert.Equal(601, attr.Value);
	}

	[Fact]
	public void MaximumLengthAttribute_ValueZero_Throws()
	{
		Assert.Throws<ArgumentException>(() => new MaximumLengthAttribute(0));
	}

	// ===== H6: ChoiceProvider/Autocomplete type validation =====

	[Fact]
	public void ChoiceProviderAttribute_InvalidType_Throws()
	{
		Assert.Throws<ArgumentException>(() => new ChoiceProviderAttribute(typeof(string)));
	}

	[Fact]
	public void AutocompleteAttribute_InvalidType_Throws()
	{
		Assert.Throws<ArgumentException>(() => new AutocompleteAttribute(typeof(string)));
	}

	// ===== M1: SlashCommand name/description validation =====

	[Fact]
	public void SlashCommandAttribute_NameTooLong_Throws()
	{
		var longName = new string('a', 33);
		Assert.Throws<ArgumentException>(() => new SlashCommandAttribute(longName, "description"));
	}

	[Fact]
	public void SlashCommandAttribute_EmptyName_Throws()
	{
		Assert.Throws<ArgumentException>(() => new SlashCommandAttribute("", "description"));
	}

	[Fact]
	public void SlashCommandAttribute_DescriptionTooLong_Throws()
	{
		var longDesc = new string('a', 101);
		Assert.Throws<ArgumentException>(() => new SlashCommandAttribute("name", longDesc));
	}

	[Fact]
	public void SlashCommandAttribute_EmptyDescription_Throws()
	{
		Assert.Throws<ArgumentException>(() => new SlashCommandAttribute("name", ""));
	}

	[Fact]
	public void SlashCommandAttribute_ValidArgs_DoesNotThrow()
	{
		var attr = new SlashCommandAttribute("test", "A description");
		Assert.Equal("test", attr.Name);
		Assert.Equal("A description", attr.Description);
	}

	[Fact]
	public void SlashCommandGroupAttribute_NameTooLong_Throws()
	{
		var longName = new string('a', 33);
		Assert.Throws<ArgumentException>(() => new SlashCommandGroupAttribute(longName, "description"));
	}

	[Fact]
	public void SlashCommandGroupAttribute_EmptyName_Throws()
	{
		Assert.Throws<ArgumentException>(() => new SlashCommandGroupAttribute("", "description"));
	}

	// ===== L2: Empty ChannelTypes validation =====

	[Fact]
	public void ChannelTypesAttribute_EmptyArray_Throws()
	{
		Assert.Throws<ArgumentException>(() => new ChannelTypesAttribute());
	}

	[Fact]
	public void ChannelTypesAttribute_WithTypes_DoesNotThrow()
	{
		var attr = new ChannelTypesAttribute(ChannelType.Text, ChannelType.Voice);
		Assert.Equal(2, attr.ChannelTypes.Count);
	}

	// ===== L5: ContextMenuAttribute setter consistency =====

	[Fact]
	public void ContextMenuAttribute_PropertiesHaveInternalSetters()
	{
		var type = typeof(ContextMenuAttribute);

		// Verify AllowedContexts, IntegrationTypes, IsNsfw have non-public setters
		var allowedContextsProp = type.GetProperty("AllowedContexts")!;
		Assert.NotNull(allowedContextsProp.SetMethod);
		Assert.False(allowedContextsProp.SetMethod!.IsPublic);

		var integrationTypesProp = type.GetProperty("IntegrationTypes")!;
		Assert.NotNull(integrationTypesProp.SetMethod);
		Assert.False(integrationTypesProp.SetMethod!.IsPublic);

		var isNsfwProp = type.GetProperty("IsNsfw")!;
		Assert.NotNull(isNsfwProp.SetMethod);
		Assert.False(isNsfwProp.SetMethod!.IsPublic);
	}

	// ===== AreDictionariesEqual tests =====

	[Fact]
	public void AreDictionariesEqual_BothNull_ReturnsTrue()
	{
		Dictionary<string, string>? a = null;
		Dictionary<string, string>? b = null;
		Assert.True(a.AreDictionariesEqual(b));
	}

	[Fact]
	public void AreDictionariesEqual_SameContent_ReturnsTrue()
	{
		var a = new Dictionary<string, string> { ["en-US"] = "hello", ["fr"] = "bonjour" };
		var b = new Dictionary<string, string> { ["en-US"] = "hello", ["fr"] = "bonjour" };
		Assert.True(a.AreDictionariesEqual(b));
	}

	[Fact]
	public void AreDictionariesEqual_DifferentContent_ReturnsFalse()
	{
		var a = new Dictionary<string, string> { ["en-US"] = "hello" };
		var b = new Dictionary<string, string> { ["en-US"] = "world" };
		Assert.False(a.AreDictionariesEqual(b));
	}

	[Fact]
	public void AreDictionariesEqual_DifferentKeys_ReturnsFalse()
	{
		var a = new Dictionary<string, string> { ["en-US"] = "hello" };
		var b = new Dictionary<string, string> { ["fr"] = "hello" };
		Assert.False(a.AreDictionariesEqual(b));
	}
}
