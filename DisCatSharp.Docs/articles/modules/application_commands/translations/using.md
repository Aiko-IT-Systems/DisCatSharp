---
uid: modules_application_commands_translations_using
title: Using Translations
---

# Using Translations

## Why Do We Outsource Translation In External JSON Files

Pretty simple: It's common to have translations external stored.
This makes it easier to modify them, while keeping the code itself clean.

## Adding Translations

Translations are added the same way like permissions are added to Application Commands:

```cs
const string TRANSLATION_PATH = "translations/";

Client.GetApplicationCommands().RegisterGuildCommands<MyCommand>(1215484634894646844, translations =>
{
    string json = File.ReadAllText(TRANSLATION_PATH + "my_command.json");

    translations.AddGroupTranslation(json);
});

Client.GetApplicationCommands().RegisterGuildCommands<MySimpleCommands>(1215484634894646844, translations =>
{
    string json = File.ReadAllText(TRANSLATION_PATH + "my_simple_command.json");

    translations.AddSingleTranslation(json);
});
```

> [!WARNING]
> If you add a translation to a class, you have to supply translations for every command in this class. Otherwise it will fail.

## Creating The Translation JSON

We split the translation in two categories.
One for slash command groups and one for normal slash commands and context menu commands.
The `name` key in the JSON will be mapped to the command / option / choice names internally.

### Translation For Slash Command Groups

Imagine, your class look like the following example:

```cs
public class MyCommand : ApplicationCommandsModule
{
    [SlashCommandGroup("my_command", "This is description of the command group.")]
    public class MyCommandGroup : ApplicationCommandsModule
    {
        [SlashCommand("first", "This is description of the command.")]
        public async Task MySlashCommand(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
            {
                Content = "This is first subcommand."
            });
        }
        [SlashCommand("second", "This is description of the command.")]
        public async Task MySecondCommand(InteractionContext ctx, [Option("value", "Some string value.")] string value)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
            {
                Content = "This is second subcommand. The value was " + value
            });
        }
    }
}
```

The translation json is a object of [Command Group Objects](xref:modules_application_commands_translations_reference#command-group-object)
A correct translation json for english and german would look like that:

```json
[
	{
		"name": "my_command",
		"description": "This is description of the command group.",
		"type": 1,
		"name_translations": {
			"en-US": "my_command",
			"de": "mein_befehl"
		},
		"description_translations": {
			"en-US": "This is description of the command group.",
			"de": "Das ist die description der Befehl Gruppe."
		},
		"groups": [],
		"commands": [
			{
				"name": "first",
				"description": "First",
				"name_translations": {
					"en-US": "first",
					"de": "erste"
				},
				"description_translations": {
					"en-US": "This is description of the command.",
					"de": "Das ist die Beschreibung des Befehls."
				}
			},
			{
				"name": "second",
				"description": "Second",
				"name_translations": {
					"en-US": "second",
					"de": "zweite"
				},
				"description_translations": {
					"en-US": "This is description of the command.",
					"de": "Das ist die Beschreibung des Befehls."
				},
				"options": [
					{
						"name": "value",
						"description": "Some string value.",
						"type": 3,
						"name_translations": {
							"en-US": "value",
							"de": "wert"
						},
						"description_translations": {
							"en-US": "Some string value.",
							"de": "Ein string Wert."
						}
					}
				]
			}
		]
	}
]
```

### Translation For Slash Commands & Context Menu Commands

Now imagine, that your class look like this example:

```cs
public class MySimpleCommands : ApplicationCommandsModule
{
    [SlashCommand("my_command", "This is description of the command.")]
    public async Task MySlashCommand(InteractionContext ctx)
    {

    }

    [ContextMenu(ApplicationCommandType.User, "My Command")]
    public async Task MyContextMenuCommand(ContextMenuContext ctx)
    {

    }
}
```

The slash command is a simple [Command Object](xref:modules_application_commands_translations_reference#command-object).
Same goes for the context menu command, but note that it can't have a description.

Slash Commands has the [type](xref:modules_application_commands_translations_reference#application-command-type) `1` and context menu commands the [type](xref:modules_application_commands_translations_reference#application-command-type) `2` or `3`.
We use this to determine, where the translation belongs to.

Please note that the description field is optional. We suggest setting it for slash commands if you want to use our translation generator, which we're building right now.
Context menu commands can't have a description, so omit it.

A correct json for this example would look like that:

```json
[
	{
		"name": "my_command",
		"description": "This is description of the command.",
		"type": 1, // Type 1 for slash command
		"name_translations": {
			"en-US": "my_command",
			"de": "mein_befehl"
		},
		"description_translations": {
			"en-US": "This is description of the command.",
			"de": "Das ist die Beschreibung des Befehls."
		}
	},
	{
		"name": "My Command",
		"type": 2, // Type 2 for user context menu command
		"name_translations": {
			"en-US": "My Command",
			"de": "Mein Befehl"
		}
	}
]
```

## Available Locales

Discord has a limited choice of locales, in particular, the ones you can select in the client.
To see the available locales, visit [this](xref:modules_application_commands_translations_reference#valid-locales) page.

## Can We Get The User And Guild Locale?

Yes, you can!
Discord sends the user on all [interaction types](xref:DisCatSharp.Enums.InteractionType), except `Ping`.

We introduced two new properties `Locale` and `GuildLocale` on [InteractionContext](xref:DisCatSharp.ApplicationCommands.Context.InteractionContext), [ContextMenuContext](xref:DisCatSharp.ApplicationCommands.Context.ContextMenuContext), [AutoCompleteContext](xref:DisCatSharp.ApplicationCommands.Context.AutocompleteContext) and [DiscordInteraction](xref:DisCatSharp.Entities.DiscordInteraction).
`Locale` is the locale of the user and always represented.
`GuildLocale` is only represented, when the interaction is **not** in a DM.

Furthermore we cache known user locales on the [DiscordUser](xref:DisCatSharp.Entities.DiscordUser.Locale) object.
