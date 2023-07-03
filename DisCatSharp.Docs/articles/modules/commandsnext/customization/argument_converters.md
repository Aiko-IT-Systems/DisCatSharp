---
uid: modules_commandsnext_customization_argument_converters
title: Argument Converter
author: DisCatSharp Team
hasDiscordComponents: true
---

## Custom Argument Converter

Writing your own argument converter will enable you to convert custom types and replace the functionality of existing converters.
Like many things in DisCatSharp, doing this is straightforward and simple.

First, create a new class which implements `IArgumentConverter<T>` and its method `ConvertAsync`.
Our example will be a boolean converter, so we'll also pass `bool` as the type parameter for `IArgumentConverter`.

```cs
public class CustomArgumentConverter : IArgumentConverter<bool>
{
    public Task<Optional<bool>> ConvertAsync(string value, CommandContext ctx)
    {
        if (bool.TryParse(value, out var boolean))
        {
            return Task.FromResult(Optional.Some<bool>(boolean));
        }

        switch (value.ToLower())
        {
            case "yes":
            case "y":
            case "t":
                return Task.FromResult(Optional.Some<bool>(true));

            case "no":
            case "n":
            case "f":
                return Task.FromResult(Optional.Some<bool>(false));

            default:
                return Task.FromResult(Optional.None);
        }
    }
}
```

Then register the argument converter with CommandContext.

```cs
var discord = new DiscordClient();
var commands = discord.UseCommandsNext();

commands.RegisterConverter(new CustomArgumentConverter());
```

Once the argument converter is written and registered, we'll be able to use it:

```cs
[Command("boolean")]
public async Task BooleanCommand(CommandContext ctx, bool boolean)
{
    await ctx.RespondAsync($"Converted to {boolean} :3");
}
```

<discord-messages>
    <discord-message profile="user">
        !boolean yes
    </discord-message>
    <discord-message profile="dcs">
        Converted to True :3
    </discord-message>
</discord-messages>

