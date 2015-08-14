---
uid: modules_application_commands_cooldowns
title: Cooldowns
author: DisCatSharp Team
hasDiscordComponents: true
---

## Cooldown Attribute

You can apply cooldowns to your application commands with the @DisCatSharp.ApplicationCommands.Attributes.SlashCommandCooldownAttribute & @DisCatSharp.ApplicationCommands.Attributes.ContextMenuCooldownAttribute.

The cooldown attributes consists of three required parameters and one optional parameter.

- `int maxUses` - The number of times a command can be used during the definied time before being put on cooldown.
- `double resetAfter` - After how many seconds the cooldown resets.
- `CooldownBucketType bucketType` - The [type](xref:DisCatSharp.Enums.Core.CooldownBucketType) of bucket to use. Can be combined.

## Usage

```cs
// We create a slash command which sends a meow to the channel. This command can be executed twice every 40 seconds per channel.
[SlashCommand("meow", "Meow at chat"), SlashCommandCooldown(2, 40, CooldownBucketType.Channel)]
public async Task MeowAsync(InteractionContext ctx)
{
    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Meow!"));
}
```

### Visual Example

Please note that the timestamp would show the actual time of the cooldown reset.
We can't dynamically change the timestamp in the example, so we just use a static one.

<discord-messages>
    <discord-message profile="dcs">
        <discord-command slot="reply" profile="dcs_owner" command="/meow"></discord-command>
        Meow!
    </discord-message>
    <discord-message profile="dcs">
        <discord-command slot="reply" profile="user" command="/meow"></discord-command>
        Meow!
    </discord-message>
    <discord-message profile="dcs" ephemeral>
        <discord-command slot="reply" profile="dcs_user" command="/meow"></discord-command>
        Error: Ratelimit hit<br/>Try again <discord-time timestamp="1894912612" format="R"></discord-time>
    </discord-message>
</discord-messages>

## Customizing the cooldown hit response

You can customize the response that is sent when a user hits a cooldown by creating a custom class which inherits from [ICooldownResponder](xref:DisCatSharp.ApplicationCommands.Entities.ICooldownResponder).

```cs
public sealed class CooldownResponse : ICooldownResponder
{
    /// <inheritdoc />
    public async Task Responder(BaseContext context)
    {
        await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("You hit a cooldown! Try again later.").AsEphemeral());
    }
}
```

You can then apply this to your command by using the `cooldownResponderType` property on the attribute.

```cs
[SlashCommand("meow", "Meow at chat"), SlashCommandCooldown(2, 40, CooldownBucketType.Channel, typeof(CooldownResponse))]
public async Task CooldownTestAsync(InteractionContext ctx)
{
    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Meow!"));
}
```

### Visual Example

<discord-messages>
    <discord-message profile="dcs">
        <discord-command slot="reply" profile="dcs_owner" command="/meow"></discord-command>
        Meow!
    </discord-message>
    <discord-message profile="dcs">
        <discord-command slot="reply" profile="user" command="/meow"></discord-command>
        Meow!
    </discord-message>
    <discord-message profile="dcs" ephemeral>
        <discord-command slot="reply" profile="dcs_user" command="/meow"></discord-command>
        You hit a cooldown! Try again later.
    </discord-message>
</discord-messages>
