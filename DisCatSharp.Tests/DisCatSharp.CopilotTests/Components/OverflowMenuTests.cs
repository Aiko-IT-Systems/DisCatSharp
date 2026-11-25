using System;
using System.Linq;

using DisCatSharp.Entities;
using DisCatSharp.Enums;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Xunit;

namespace DisCatSharp.Copilot.Tests.Components;

/// <summary>
///     Covers overflow-menu payloads, validation, and builder traversal.
/// </summary>
public class OverflowMenuTests
{
	/// <summary>
	///     Ensures overflow menus use Discord's documented component discriminators and field names.
	/// </summary>
	[Fact]
	public void OverflowMenu_SerializesProtocolShape()
	{
		var menu = CreateMenu();
		var builder = new DiscordMessageBuilder()
			.WithV2Components()
			.AddComponents(new DiscordActionRowComponent([menu]));

		var payload = JArray.Parse(JsonConvert.SerializeObject(builder.Components));
		var serializedMenu = (JObject)payload[0]!["components"]![0]!;
		Assert.Equal(25, (int)serializedMenu["type"]!);
		Assert.Equal("Manage", (string)serializedMenu["groups"]![0]!["label"]!);
		Assert.Equal(26, (int)serializedMenu["groups"]![0]!["components"]![0]!["type"]!);
		Assert.Equal("edit", (string)serializedMenu["groups"]![0]!["components"]![0]!["custom_id"]!);
		Assert.Equal(3, (int)serializedMenu["groups"]![1]!["components"]![0]!["style"]!);
		Assert.Equal("https://example.com", (string)serializedMenu["groups"]![1]!["components"]![0]!["url"]!);
	}

	/// <summary>
	///     Ensures component payloads deserialize menu groups and section accessories to their typed models.
	/// </summary>
	[Fact]
	public void OverflowMenu_DeserializesTypedGroupsAndSectionAccessory()
	{
		const string json = """
		                    [{"type":17,"components":[{"type":9,"components":[{"type":10,"content":"Actions"}],"accessory":{"type":25,"groups":[{"label":"Manage","components":[{"type":26,"id":5,"custom_id":"edit","label":"Edit","style":1}]}]}}]}]
		                    """;

		var components = JsonConvert.DeserializeObject<DiscordComponent[]>(json)!;
		var section = Assert.IsType<DiscordSectionComponent>(Assert.IsType<DiscordContainerComponent>(components[0]).Components[0]);
		var menu = Assert.IsType<DiscordOverflowMenuComponent>(section.Accessory);
		var item = Assert.Single(menu.Groups).Components.Single();

		Assert.Equal("Manage", menu.Groups[0].Label);
		Assert.Equal("edit", item.CustomId);
		Assert.Equal(ComponentType.MenuActionItem, item.Type);
	}

	/// <summary>
	///     Ensures component interactions expose the menu item discriminator and selected custom ID.
	/// </summary>
	[Fact]
	public void MenuActionInteraction_DeserializesComponentTypeAndCustomId()
	{
		var interactionData = JsonConvert.DeserializeObject<DiscordInteractionData>("{\"component_type\":26,\"id\":2,\"custom_id\":\"edit\"}")!;

		Assert.Equal(ComponentType.MenuActionItem, interactionData.ComponentType);
		Assert.Equal("edit", interactionData.CustomId);
	}

	/// <summary>
	///     Ensures message, webhook, follow-up, and interaction response builders accept overflow menu components.
	/// </summary>
	[Fact]
	public void OverflowMenu_IsAcceptedByAllMessageBuilderTypes()
	{
		var row = new DiscordActionRowComponent([CreateMenu()]);
		Assert.Same(row, Assert.Single(new DiscordMessageBuilder().AddComponents(row).Components!));
		Assert.Same(row, Assert.Single(new DiscordWebhookBuilder().AddComponents(row).Components!));
		Assert.Same(row, Assert.Single(new DiscordFollowupMessageBuilder().AddComponents(row).Components!));
		Assert.Same(row, Assert.Single(new DiscordInteractionResponseBuilder().AddComponents(row).Components!));
	}

	/// <summary>
	///     Ensures constructors reject invalid item styles and protocol length/count bounds.
	/// </summary>
	[Fact]
	public void OverflowMenu_ConstructorsRejectInvalidStyleFieldsAndBounds()
	{
		Assert.Throws<ArgumentException>(() => new DiscordMenuActionItemComponent(MenuActionItemStyle.Default, "Edit"));
		Assert.Throws<ArgumentException>(() => new DiscordMenuActionItemComponent(MenuActionItemStyle.Link, "Open", customId: "open", url: "https://example.com"));
		Assert.Throws<ArgumentException>(() => new DiscordMenuActionItemComponent(MenuActionItemStyle.Default, new string('x', 81), customId: "edit"));
		Assert.Throws<ArgumentException>(() => new DiscordOverflowMenuGroup([new DiscordMenuActionItemComponent(MenuActionItemStyle.Default, "Edit", customId: "edit")], new string('x', 81)));
		Assert.Throws<ArgumentException>(() => new DiscordOverflowMenuComponent(Enumerable.Repeat(
			new DiscordOverflowMenuGroup([new DiscordMenuActionItemComponent(MenuActionItemStyle.Default, "Edit", customId: "edit")]), 9)));
	}

	/// <summary>
	///     Ensures component editing and disabled-state helpers reach grouped menu items.
	/// </summary>
	[Fact]
	public void ModifyComponentByCustomIdAndDisableComponents_ReachesMenuItems()
	{
		var menu = CreateMenu();
		var builder = new DiscordMessageBuilder()
			.WithV2Components()
			.AddComponents(new DiscordActionRowComponent([menu]));

		var modified = false;
		builder.ModifyComponentByCustomId("edit", _ => modified = true);
		builder.DisableComponentsByCustomId("edit");

		Assert.True(modified);
		Assert.True(menu.Groups[0].Components[0].Disabled);
	}

	/// <summary>
	///     Creates a menu fixture with one interactive item and one link item.
	/// </summary>
	private static DiscordOverflowMenuComponent CreateMenu()
		=> new(
		[
			new DiscordOverflowMenuGroup(
			[
				new DiscordMenuActionItemComponent(MenuActionItemStyle.Default, "Edit", customId: "edit")
			], "Manage"),
			new DiscordOverflowMenuGroup(
			[
				new DiscordMenuActionItemComponent(MenuActionItemStyle.Link, "Open", url: "https://example.com")
			])
		]);
}
