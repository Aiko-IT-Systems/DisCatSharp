---
uid: modules_commandsnext_intro
title: CommandsNext Introduction
author: DisCatSharp Team
hasDiscordComponents: true
---

> [!NOTE]
> This article assumes you've recently read the article on _[writing your first bot](xref:getting_started_first_bot)_.

# Introduction to CommandsNext

This article will introduce you to some basic concepts of our native command framework: _CommandsNext_.<br/>
Be sure to install the `DisCatSharp.CommandsNext` package from NuGet before continuing.

![CommandsNext NuGet Package](/images/commands_intro_01.png)

## Writing a Basic Command

### Create a Command Module

A command module is simply a class which acts as a container for your command methods. Instead of registering individual commands,
you'd register a single command module which contains multiple commands. There's no limit to the amount of modules you can have,
and no limit to the amount of commands each module can contain. For example: you could have a module for moderation commands and
a separate module for image commands. This will help you keep your commands organized and reduce the clutter in your project.

Our first demonstration will be simple, consisting of one command module with a simple command.<br/>
We'll start by creating a new folder named `Commands` which contains a new class named `MyFirstModule`.

![Solution Explorer](/images/commands_intro_02.png)

Give this new class `public` access and have it inherit from `BaseCommandModule`.

```cs
public class MyFirstModule : BaseCommandModule { }
```

### Create a Command Method

Within our new module, create a method named `GreetCommand` marked as `async` with a `Task` return type.
The first parameter of your method _must_ be of type `CommandContext`, as required by CommandsNext.

```cs
public async Task GreetCommand(CommandContext ctx) { }
```

In the body of our new method, we'll use `CommandContext#RespondAsync` to send a simple message.

```cs
await ctx.RespondAsync("Greetings! UwU");
```

Finally, mark your command method with the `Command` attribute so CommandsNext will know to treat our method as a command method.
This attribute takes a single parameter: the name of the command.

We'll name our command _greet_ to match the name of the method.

```cs
[Command("greet")]
public async Task GreetCommand(CommandContext ctx)
{
    await ctx.RespondAsync("Greetings! UwU");
}
```

Your command module should now resemble this:

```cs
using System.Threading.Tasks;
using DisCatSharp.CommandsNext;
using DisCatSharp.CommandsNext.Attributes;

public class MyFirstModule : BaseCommandModule
{
    [Command("greet")]
    public async Task GreetCommand(CommandContext ctx)
    {
        await ctx.RespondAsync("Greetings! UwU");
    }
}
```

### Cleanup and Configuration

Before we can run our new command, we'll need modify our main method.<br/>
Start by removing the event handler we created [previously](xref:getting_started_first_bot#spicing-up-your-bot).

```cs
var discord = new DiscordClient();

discord.MessageCreated += async (s, e) =>               // REMOVE
{                                                       // ALL
    if (e.Message.Content.ToLower().StartsWith("ping")) // OF
        await e.Message.RespondAsync("pong!");          // THESE
};                                                      // LINES

await discord.ConnectAsync();
```

Next, call the `UseCommandsNext` extension method on your `DiscordClient` instance and pass it a new `CommandsNextConfiguration` instance.
Assign the resulting `CommandsNextExtension` instance to a new variable named _commands_. This important step will enable CommandsNext for your Discord client.

```cs
var discord = new DiscordClient();
var commands = discord.UseCommandsNext(new CommandsNextConfiguration());
```

Create an object initializer for `CommandsNextConfiguration` and assign the `StringPrefixes` property a new `string` array containing your desired prefixes.
Our example below will only define a single prefix: `!`.

```cs
new CommandsNextConfiguration()
{
    StringPrefixes = new List<string>() { "!" }
}
```

Now we'll register our command module.
Call the `RegisterCommands` method on our `CommandsNextExtension` instance and provide it with your command module.

```cs
var discord = new DiscordClient();
var commands = discord.UseCommandsNext();

commands.RegisterCommands<MyFirstModule>();

await discord.ConnectAsync();
```

Alternatively, you can pass in your assembly to register commands from all modules in your program.

```cs
commands.RegisterCommands(Assembly.GetExecutingAssembly());
```

Your main method should look similar to the following:

```cs
internal static async Task MainAsync()
{
    var discord = new DiscordClient(new DiscordConfiguration());
    var commands = discord.UseCommandsNext(new CommandsNextConfiguration()
    {
        StringPrefixes = new List<string>() { "!" }
    });

    commands.RegisterCommands<MyFirstModule>();

    await discord.ConnectAsync();
    await Task.Delay(-1);
}
```

### Running Your Command

It's now the moment of truth; all your blood, sweat, and tears have lead to this moment.
Hit `F5` on your keyboard to compile and run your bot, then execute your command in any channel that your bot account has access to.

<discord-messages>
    <discord-message profile="user">
        !greet
    </discord-message>
    <discord-message profile="dcs">
        Greetings! UwU
    </discord-message>
</discord-messages>

## Taking User Input

### Command Arguments

Now that we have a basic command down, let's spice it up a bit by defining _arguments_ to accept user input.

Defining an argument is simple; just add additional parameters to your signature of your command method.
CommandsNext will automatically parse user input and populate the parameters of your command method with those arguments.
To demonstrate, we'll modify our _greet_ command to greet a user with a given name.

Head back to `MyFirstModule` and add a parameter of type `string` to the `GreetCommand` method.

```cs
[Command("greet")]
public async Task GreetCommand(CommandContext ctx, string name)
```

CommandsNext will now interpret this as a command named _greet_ that takes one argument.

Next, replace our original response message with an [interpolated string](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/tokens/interpolated)
which uses our new parameter.

```cs
public async Task GreetCommand(CommandContext ctx, string name)
{
    await ctx.RespondAsync($"Greetings, {name}! You're cute OwO!");
}
```

That's all there is to it. Smack `F5` and test it out in a channel your bot account has access to.

<discord-messages>
    <discord-message profile="user">
        !greet Drac
    </discord-message>
    <discord-message profile="dcs">
        Greetings, Drac! You're cute OwO!
    </discord-message>
</discord-messages>

Now, you may have noticed that providing more than one word simply does not work.
For example, `!greet Luke Smith` will result in no response from your bot.

This fails because a valid [overload](#command-overloads) could not be found for your command.

CommandsNext will split arguments by whitespace. This means `Luke Smith` is counted as two separate arguments; `Luke` and `Smith`.
In addition to this, CommandsNext will attempt to find and execute an overload of your command that has the _same number_ of provided arguments.
Together, this means that any additional arguments will prevent CommandsNext from finding a valid overload to execute.

The simplest way to get around this would be to wrap your input with double quotes.
CommandsNext will parse this as one argument, allowing your command to be executed.

```
!greet "Lala Sabathil"
```

If you would prefer not to use quotes, you can use the `RemainingText` attribute on your parameter.
This attribute will instruct CommandsNext to parse all remaining arguments into that parameter.

```cs
public async Task GreetCommand(CommandContext ctx, [RemainingText] string name)
```

Alternatively, you can use the `params` keyword to have all remaining arguments parsed into an array.

```cs
public async Task GreetCommand(CommandContext ctx, params string[] names)
```

A more obvious solution is to add additional parameters to the method signature of your command method.<br/>

```cs
public async Task GreetCommand(CommandContext ctx, string firstName, string lastName)
```

Each of these has their own caveats; it'll be up to you to choose the best solution for your commands.

### Argument Converters

CommandsNext can convert arguments, which are natively `string`, to the type specified by a command method parameter.
This functionality is powered by _argument converters_, and it'll help to eliminate the boilerplate code needed to parse and convert `string` arguments.

CommandsNext has built-in argument converters for the following types:

|    Category    | Types                                                                                                                                 |
| :------------: | :------------------------------------------------------------------------------------------------------------------------------------ |
|    Discord     | `DiscordGuild`, `DiscordChannel`, `DiscordMember`, `DiscordUser`,<br/>`DiscordRole`, `DiscordMessage`, `DiscordEmoji`, `DiscordColor` |
|    Integral    | `byte`, `short`, `int`, `long`, `sbyte`, `ushort`, `uint`, `ulong`                                                                    |
| Floating-Point | `float`, `double`, `decimal`                                                                                                          |
|      Date      | `DateTime`, `DateTimeOffset`, `TimeSpan`                                                                                              |
|   Character    | `string`, `char`                                                                                                                      |
|    Boolean     | `bool`                                                                                                                                |

You're also able to create and provide your own [custom argument converters](xref:modules_commandsnext_customization_argument_converters), if desired.

Let's do a quick demonstration of the built-in converters.

Create a new command method above our `GreetCommand` method named `RandomCommand` and have it take two integer arguments.
As the method name suggests, this command will be named _random_.

```cs
[Command("random")]
public async Task RandomCommand(CommandContext ctx, int min, int max) { }
```

Make a variable with a new instance of `Random`.

```cs
var random = new Random();
```

Finally, we'll respond with a random number within the range provided by the user.

```cs
await ctx.RespondAsync($"Your number is: {random.Next(min, max)}");
```

Run your bot once more with `F5` and give this a try in a text channel.

<discord-messages>
    <discord-message profile="user">
        !random 1 100
    </discord-message>
    <discord-message profile="dcs">
        Your number is: 69
    </discord-message>
</discord-messages>

CommandsNext converted the two arguments from `string` into `int` and passed them to the parameters of our command,
removing the need to manually parse and convert the arguments yourself.

We'll do one more to drive the point home. Head back to our old `GreetCommand` method, remove our
`name` parameter, and replace it with a new parameter of type `DiscordMember` named `member`.

```cs
public async Task GreetCommand(CommandContext ctx, DiscordMember member)
```

Then modify the response to mention the provided member with the `Mention` property on `DiscordMember`.

```cs
public async Task GreetCommand(CommandContext ctx, DiscordMember member)
{
    await ctx.RespondAsync($"Heywo, {member.Mention}! Enjoy the mention!");
}
```

Go ahead and give that a test run.

<discord-messages>
    <discord-message profile="user">
        !greet <discord-mention>Discord User</discord-mention>
    </discord-message>
    <discord-message profile="dcs">
        Heywo, <discord-mention highlight>Discord User</discord-mention>! Enjoy the mention!
    </discord-message>
</discord-messages>

<discord-messages>
    <discord-message profile="user">
        !greet Discord User
    </discord-message>
    <discord-message profile="dcs">
        Heywo, <discord-mention highlight>Discord User</discord-mention>! Enjoy the mention!
    </discord-message>
</discord-messages>

<discord-messages>
    <discord-message profile="user">
        !greet 666339628613107725
    </discord-message>
    <discord-message profile="dcs">
        Heywo, <discord-mention highlight>Discord User</discord-mention>! Enjoy the mention!
    </discord-message>
</discord-messages>

The argument converter for `DiscordMember` is able to parse mentions, usernames, nicknames, and user IDs then look for a matching member within the guild the command was executed from.
Ain't that neat?

## Command Overloads

Command method overloading allows you to create multiple argument configurations for a single command.

```cs
[Command("foo")]
public Task FooCommand(CommandContext ctx, string bar, int baz) { }

[Command("foo")]
public Task FooCommand(CommandContext ctx, DiscordUser bar) { }
```

Executing `!foo green 5` will run the first method, and `!foo @SecondUser` will run the second method.

Additionally, all check attributes are shared between overloads.<br/>

```cs
[Command("foo"), Aliases("bar", "baz")]
[RequireGuild, RequireBotPermissions(Permissions.AttachFiles)]
public Task FooCommand(CommandContext ctx, int bar, int baz, string qux = "agony") { }

[Command("foo")]
public Task FooCommand(CommandContext ctx, DiscordChannel bar, TimeSpan baz) { }
```

The additional attributes and checks applied to the first method will also be applied to the second method.

## Further Reading

Now that you've gotten an understanding of CommandsNext, it'd be a good idea check out the following:

-   [Command Attributes](xref:modules_commandsnext_command_attributes)
-   [Help Formatter](xref:modules_commandsnext_customization_help_formatter)
-   [Dependency Injection](xref:modules_commandsnext_dependency_injection)
