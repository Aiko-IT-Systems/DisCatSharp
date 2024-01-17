---
uid: modules_commandsnext_cooldowns
title: Cooldowns
author: DisCatSharp Team
hasDiscordComponents: true
---

## Cooldown Attribute

You can apply cooldowns to your text commands with the @DisCatSharp.CommandsNext.Attributes.CooldownAttribute.

The cooldown attribute consists of three required parameters and one optional parameter.

-   `int maxUses` - The number of times a command can be used during the definied time before being put on cooldown.
-   `double resetAfter` - After how many seconds the cooldown resets.
-   `CooldownBucketType bucketType` - The [type](xref:DisCatSharp.Enums.Core.CooldownBucketType) of bucket to use. Can be combined.

## Usage

```cs
// We create a text command which sends a meow to the channel. This command can be executed twice every 40 seconds per channel.
[Command("meow"), Description("Meow at chat"), Cooldown(2, 40, CooldownBucketType.Channel)]
public async Task MeowAsync(CommandContext ctx)
{
    await context.RespondAsync("Meow!");
}
```

### Visual Example

<discord-messages>
    <discord-message profile="dcs_owner">
        !meow
    </discord-message>
    <discord-message profile="dcs">
        <discord-reply slot="reply" profile="dcs_owner" mentions>!meow</discord-reply>
        Meow!
    </discord-message>
    <discord-message profile="user">
        !meow
    </discord-message>
    <discord-message profile="dcs">
        <discord-reply slot="reply" profile="user" mentions>!meow</discord-reply>
        Meow!
    </discord-message>
    <discord-message profile="dcs_user">
        !meow
        <discord-reactions slot="reactions">
            <discord-reaction interactive="false" name=":x:" emoji="https://cdn.aitsys.dev/file/data/z5xj4bzraybavtu72in7/PHID-FILE-3rkjxmtwinuysdnwumm7/d22e8c77c1aa9e5a69a2.png" count="1"></discord-reaction>
        </discord-reactions>
    </discord-message>
</discord-messages>

## Customizing the cooldown hit response

You can customize the response that is sent when a user hits a cooldown by creating a custom class which inherits from [ICooldownResponder](xref:DisCatSharp.CommandsNext.Entities.ICooldownResponder).

```cs
public sealed class CooldownResponse : ICooldownResponder
{
    /// <inheritdoc />
    public async Task Responder(BaseContext context)
    {
        await context.Message.CreateReactionAsync(DiscordEmoji.FromGuildEmote(context.Client, 972904417005293609));
        // or send a message
        // await context.Member.SendMessageAsync("You hit a cooldown! Try again later.");

    }
}
```

You can then apply this to your command by using the `cooldownResponderType` property on the attribute.

```cs
[Command("meow"), Description("Meow at chat"), Cooldown(2, 40, CooldownBucketType.Channel, typeof(CooldownResponse)))]
public async Task MeowAsync(CommandContext ctx)
{
    await context.RespondAsync("Meow!");
}
```

### Visual Example

<discord-messages>
    <discord-message profile="dcs_owner">
        !meow
    </discord-message>
    <discord-message profile="dcs">
        <discord-reply slot="reply" profile="dcs_owner" mentions>!meow</discord-reply>
        Meow!
    </discord-message>
    <discord-message profile="user">
        !meow
    </discord-message>
    <discord-message profile="dcs">
        <discord-reply slot="reply" profile="user" mentions>!meow</discord-reply>
        Meow!
    </discord-message>
    <discord-message profile="dcs_user">
        !meow
        <discord-reactions slot="reactions">
            <discord-reaction interactive="false" name=":hasiscared:" emoji="https://cdn.discordapp.com/emojis/972904417005293609.png" count="1"></discord-reaction>
        </discord-reactions>
    </discord-message>
</discord-messages>
