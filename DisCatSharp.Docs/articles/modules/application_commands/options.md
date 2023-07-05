---
uid: modules_application_commands_options
title: Application Commands Options
---

# Slash Commands options
For slash commands, you can create options. They allow users to submit additional information to commands.

Command options can be of the following types:
- string
- int
- long
- double
- bool
- [DiscordUser](xref:DisCatSharp.Entities.DiscordUser)
- [DiscordRole](xref:DisCatSharp.Entities.DiscordRole)
- [DiscordChannel](xref:DisCatSharp.Entities.DiscordChannel)
- [DiscordAttachment](xref:DisCatSharp.Entities.DiscordAttachment)
- mentionable (ulong)
- Enum

## Basic usage

>[!NOTE]
>Options can only be added in the slash commands. Context menus do not support this!

All of options must contain the [Option](xref:DisCatSharp.ApplicationCommands.Attributes.OptionAttribute) attribute.
They should be after [InteractionContext](xref:DisCatSharp.ApplicationCommands.Context.InteractionContext).

```cs
public class MyCommand : ApplicationCommandsModule
{
    [SlashCommand("my_command", "This is description of the command.")]
    public static async Task MySlashCommand(InteractionContext ctx, [Option("argument", "This is description of the option.")] string firstParam)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
        {
            Content = firstParam
        });
    }
}
```

## Choices
Sometimes, we need to allow users to choose from several pre-created options.
We can of course add a string or long parameter and let users guess the options, but why when we can make things more convenient?

We have 3 ways to make choices:
- Enums
- [Choice Attribute](xref:DisCatSharp.ApplicationCommands.Attributes.ChoiceAttribute)
- [Choice Providers](xref:DisCatSharp.ApplicationCommands.Attributes.IChoiceProvider)

### Enums

This is the easiest option. We just need to specify the required Enum as a command parameter.

```cs
public class MyCommand : ApplicationCommandsModule
{
    [SlashCommand("my_command", "This is description of the command.")]
    public static async Task MySlashCommand(InteractionContext ctx, [Option("enum_param", "Description")] MyEnum enumParameter)
    {

    }
}

public enum MyEnum
{
    FirstOption,
    SecondOption
}
```

In this case, the user will be shown this as options: `FirstOption` and `SecondOption`.
Therefore, if you want to define different names for options without changing the Enum, you can add a special attribute:
```cs
public enum MyEnum
{
    [ChoiceName("First option")]
    FirstOption,
    [ChoiceName("Second option")]
    SecondOption
}
```

### Choice Attribute

With this way, you can get rid of unnecessary conversions within the command.
To do this, you need to add one or more [Choice Attributes](xref:DisCatSharp.ApplicationCommands.Attributes.ChoiceAttribute) before the [Option](xref:DisCatSharp.ApplicationCommands.Attributes.OptionAttribute) attribute
```cs
[SlashCommand("my_command", "This is description of the command.")]
public static async Task MySlashCommand(InteractionContext ctx, [Choice("First option", 1)] [Choice("Second option", 2)] [Option("option", "Description")] long firstParam)
{

}
```

As the first parameter, [Choice](xref:DisCatSharp.ApplicationCommands.Attributes.ChoiceAttribute) takes a name that will be visible to the user, and the second - a value that will be passed to the command.
You can also use strings.

### Choice Provider

Perhaps the most difficult way. It consists in writing a method that will generate a list of options when registering commands.
This way we don't have to list all of them in the code when there are many of them.

To create your own provider, you need to create a class that inherits [IChoiceProvider](xref:DisCatSharp.ApplicationCommands.Attributes.IChoiceProvider) and contains the `Provider()` method.
```cs
public class MyChoiceProvider : IChoiceProvider
{
    public Task<IEnumerable<DiscordApplicationCommandOptionChoice>> Provider()
    {

    }
}
```

As seen above, the method should return a list of [DiscordApplicationCommandOptionChoice](xref:DisCatSharp.Entities.DiscordApplicationCommandOptionChoice).

Now we need to create a list and add items to it:
```cs
var options = new List<DiscordApplicationCommandOptionChoice>
{
    new DiscordApplicationCommandOptionChoice("First option", 1),
    new DiscordApplicationCommandOptionChoice("Second option", 2)
};

return Task.FromResult(options.AsEnumerable());
```

Of course you can generate this list as you like. The main thing is that the method should return this list.

Now let's add our new provider to the command.


```cs
[SlashCommand("my_command", "This is description of the command.")]
public static async Task MySlashCommand(InteractionContext ctx, [ChoiceProvider(typeof(MyChoiceProvider))] [Option("option", "Description")] long option)
{

}
```

All the code that we got:
```cs
public class MyCommand : ApplicationCommandsModule
{
    [SlashCommand("my_command", "This is description of the command.")]
    public static async Task MySlashCommand(InteractionContext ctx, [ChoiceProvider(typeof(MyChoiceProvider))] [Option("option", "Description")] long option)
    {

    }
}

public class MyChoiceProvider : IChoiceProvider
{
    public Task<IEnumerable<DiscordApplicationCommandOptionChoice>> Provider()
    {
        var options = new List<DiscordApplicationCommandOptionChoice>
        {
            new DiscordApplicationCommandOptionChoice("First option", 1),
            new DiscordApplicationCommandOptionChoice("Second option", 2)
        };

        return Task.FromResult(options.AsEnumerable());
    }
}
```

That's all, for a better example for [ChoiceProvider](xref:DisCatSharp.ApplicationCommands.Attributes.IChoiceProvider) refer to the examples.

## Autocomplete
Autocomplete works in the same way as ChoiceProvider, with one difference:
the method that creates the list of choices is triggered not once when the commands are registered, but whenever the user types a command.
It is advisable to use this method exactly when you have a list that will be updated while the bot is running.
In other cases, when the choices will not change, it is advisable to use the previous methods.

Creating an autocomplete is similar to creating a ChoiceProvider with a few changes:
```cs
public class MyAutocompleteProvider : IAutocompleteProvider
{
    public async Task<IEnumerable<DiscordApplicationCommandAutocompleteChoice>> Provider(AutocompleteContext ctx)
    {
        var options = new List<DiscordApplicationCommandAutocompleteChoice>
        {
            new DiscordApplicationCommandAutocompleteChoice("First option", 1),
            new DiscordApplicationCommandAutocompleteChoice("Second option", 2)
        };

        return Task.FromResult(options.AsEnumerable());
    }
}
```

The changes are that instead of [IChoiceProvider](xref:DisCatSharp.ApplicationCommands.Attributes.IChoiceProvider), the class inherits [IAutocompleteProvider](xref:DisCatSharp.ApplicationCommands.Attributes.IAutocompleteProvider), and the Provider method should return a list with [DiscordApplicationCommandAutocompleteChoice](xref:DisCatSharp.Entities.DiscordApplicationCommandAutocompleteChoice).

Now we add it to the command:
```cs
[SlashCommand("my_command", "This is description of the command.")]
public static async Task MySlashCommand(InteractionContext ctx, [Autocomplete(typeof(MyAutocompleteProvider))] [Option("option", "Description", true)] long option)
{

}
```

Note that we have not only replaced [ChoiceProvider](xref:DisCatSharp.ApplicationCommands.Attributes.ChoiceProviderAttribute) with [Autocomplete](xref:DisCatSharp.ApplicationCommands.Attributes.AutocompleteAttribute), but also added `true` to [Option](xref:DisCatSharp.ApplicationCommands.Attributes.OptionAttribute).

## Channel types

Sometimes we may need to give users the ability to select only a certain type of channels, for example, only text, or voice channels.

This can be done by adding the [ChannelTypes](xref:DisCatSharp.ApplicationCommands.Attributes.ChannelTypesAttribute) attribute to the option with the [DiscordChannel](xref:DisCatSharp.Entities.DiscordChannel) type.

```cs
[SlashCommand("my_command", "This is description of the command.")]
public static async Task MySlashCommand(InteractionContext ctx, [Option("channel", "You can select only text channels."), ChannelTypes(ChannelType.Text)] DiscordChannel channel)
{

}
```

This will make it possible to select only text channels.

## MinimumValue / MaximumValue Attribute

Sometimes we may need to give users the ability to select only a certain range of values.

This can be done by adding the [MinimumValue](xref:DisCatSharp.ApplicationCommands.Attributes.MinimumValueAttribute) and [MaximumValue](xref:DisCatSharp.ApplicationCommands.Attributes.MaximumValueAttribute) attribute to the option.
It can be used only for the types `double`, `int` and `long` tho.

```cs
[SlashCommand("my_command", "This is description of the command.")]
public static async Task MySlashCommand(InteractionContext ctx, [Option("number", "You can select only a certain range."), MinimumValue(50), MaximumValue(100)] int numbers)
{

}
```

## MinimumLength / MaximumLength Attribute

Sometimes we may need to limit the user to a certain string length.

This can be done by adding the [MinimumLength](xref:DisCatSharp.ApplicationCommands.Attributes.MinimumLengthAttribute) and [MaximumLength](xref:DisCatSharp.ApplicationCommands.Attributes.MaximumLengthAttribute) attribute to the option.
It can be used only for the type `string`.

```cs
[SlashCommand("my_command", "This is description of the command.")]
public static async Task MySlashCommand(InteractionContext ctx, [Option("text", "You can only send text with a length between 10 and 50 characters."), MinimumLength(10), MaximumLength(50)] string text)
{

}
```
