---
uid: topics_messagebuilder
title: Message Builder
author: DisCatSharp Team
hasDiscordComponents: true
---

# Message Builder

## Background

Before the message builder was put into place, we had one large method for sending messages along with 3 additional methods for sending files. This was becoming a major code smell and it was hard to maintain and add more parameters onto it. Now we support just sending a simple message, an embed, a simple message with an embed, or a message builder.

## Using the Message Builder

The API Documentation for the message builder can be found [there](xref:DisCatSharp.Entities.DiscordMessageBuilder),
but here we'll go over some of the concepts of using the message builder:

### Adding a File

For sending files, you'll have to use the MessageBuilder to construct your message, see example below:

```cs
 using (var fs = new FileStream("global_name.cs", FileMode.Open, FileAccess.Read))
 {
    var msg = await new DiscordMessageBuilder()
        .WithContent("Here's a code snippet for you!")
        .WithFiles(new Dictionary<string, Stream>() { { "global_name.cs", fs } })
        .SendAsync(ctx.Channel);
}
```

<discord-messages>
    <discord-message profile="dcs">
        Here's a code snippet for you!
        <discord-attachments slot="attachments">
            <discord-attachment type="file" alt="global_name.cs" size="1.2 MB"  url="/snippets/global_name.cs"></discord-attachment>
        </discord-attachments>
    </discord-message>
</discord-messages>

### Adding Mentions

For sending mentions, you'll have to use the MessageBuilder to construct your message, see example below:

```cs
var msg = await new DiscordMessageBuilder()
    .WithContent($"✔ UserMention(user): Hey, {user.Mention}! Listen!")
    .WithAllowedMentions(new IMention[] { new UserMention(user) })
    .SendAsync(ctx.Channel);
```

<discord-messages>
    <discord-message profile="dcs" highlight>
        ✔ UserMention(user): Hey, <discord-mention highlight profile="user">Discord User</discord-mention>! Listen!
    </discord-message>
</discord-messages>

### Sending TTS Messages

For sending a TTS message, you'll have to use the MessageBuilder to construct your message, see example below:

```cs
var msg = await new DiscordMessageBuilder()
    .WithContent($"This is a test message")
    .HasTTS(true)
    .SendAsync(ctx.Channel);
```

### Sending an Inline Reply

For sending an inline reply, you'll have to use the MessageBuilder to construct your message, see example below:

```cs
var msg = await new DiscordMessageBuilder()
    .WithContent($"I'm talking to *you*!")
    .WithReply(ctx.Message.Id)
    .SendAsync(ctx.Channel);
```

<discord-messages>
    <discord-message profile="user">Who you talking to?</discord-message>
    <discord-message profile="dcs">
        <discord-reply slot="reply" profile="user" mentions>Who you talking to?</discord-reply>
        I'm talking to <discord-bold>you</discord-bold>!
    </discord-message>
</discord-messages>

<br/>
By default, replies do not mention. To make a reply mention, simply pass true as the second parameter:

```cs
var msg = await new DiscordMessageBuilder()
    .WithContent($"I'm talking to *you*!")
    .WithReply(ctx.Message.Id, true)
    .SendAsync(ctx.Channel);
```

<discord-messages>
    <discord-message profile="user">Who you talking to?</discord-message>
    <discord-message profile="dcs" highlight>
        <discord-reply slot="reply" profile="user" mentions>Who you talking to?</discord-reply>
        I'm talking to <discord-bold>you</discord-bold>!
    </discord-message>
</discord-messages>
