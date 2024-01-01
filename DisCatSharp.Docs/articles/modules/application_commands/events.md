---
uid: modules_application_commands_events
title: Application Commands Events
---

# Application Commands events

Sometimes we need to add a variety of actions and checks before and after executing a command.
We can do this in the commands itself, or we can use special events for this.

## Before execution

The simplest example in this case: checking if the command was executed within the guild.
Suppose we have a certain class with commands that must be executed ONLY in the guilds:

```cs
public class MyGuildCommands : ApplicationCommandsModule
{
    [SlashCommand("mute", "Mute user.")]
    public static async Task Mute(InteractionContext ctx)
    {

    }
    [SlashCommand("kick", "Kick user.")]
    public static async Task Kick(InteractionContext ctx)
    {

    }
    [SlashCommand("ban", "Ban user.")]
    public static async Task Ban(InteractionContext ctx)
    {

    }
}
```

In this case, the easiest way would be to override the method from [ApplicationCommandsModule](xref:DisCatSharp.ApplicationCommands.ApplicationCommandsModule).

```cs
public class MyGuildCommands : ApplicationCommandsModule
{
    public override async Task<bool> BeforeSlashExecutionAsync(InteractionContext ctx)
    {
        if (ctx.Guild == null)
            return false;
    }

    [SlashCommand("mute", "Mute user.")]
    public static async Task Mute(InteractionContext ctx)
    {

    }
    [SlashCommand("kick", "Kick user.")]
    public static async Task Kick(InteractionContext ctx)
    {

    }
    [SlashCommand("ban", "Ban user.")]
    public static async Task Ban(InteractionContext ctx)
    {

    }
}
```

Now, before executing any of these commands, the `BeforeSlashExecutionAsync` method will be executed. You can do anything in it, for example, special logging.
If you return `true`, then the command method will be executed after that, otherwise the execution will end there.

## After execution

If you want to create actions after executing the command, then you need to do the same, but override a different method:

```cs
public override async Task AfterSlashExecutionAsync(InteractionContext ctx)
{
    // some actions
}
```

## Context menus

You can also add similar actions for the context menus. But this time, you need to override the other methods:

```cs
public class MyGuildCommands : ApplicationCommandsModule
{
    public override async Task<bool> BeforeContextMenuExecutionAsync(ContextMenuContext ctx)
    {
        if (ctx.Guild == null)
            return false;
    }
    public override async Task AfterContextMenuExecutionAsync(ContextMenuContext ctx)
    {
        // some actions
    }
}
```

## Error handling

If you want to handle errors for slash commands, subscribe to the [SlashCommandErrored](xref:DisCatSharp.ApplicationCommands.ApplicationCommandsExtension.SlashCommandErrored) event.

A separate event exists for context menus as [ContextMenuErrored](xref:DisCatSharp.ApplicationCommands.ApplicationCommandsExtension.ContextMenuErrored) event.

It contains a castable field `Exception` with the exception that was thrown during the execution of the command.

As example it can be a type of [SlashExecutionChecksFailedException](xref:DisCatSharp.ApplicationCommands.Exceptions.SlashExecutionChecksFailedException) or [ContextMenuExecutionChecksFailedException](xref:DisCatSharp.ApplicationCommands.Exceptions.ContextMenuExecutionChecksFailedException) which contains a list of failed checks.
