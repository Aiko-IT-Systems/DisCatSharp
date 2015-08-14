---
uid: modules_commandsnext_command_attributes
title: Command Attributes
author: DisCatSharp Team
hasDiscordComponents: true
---

## Built-In Attributes

CommandsNext has a variety of built-in attributes to enhance your commands and provide some access control.
The majority of these attributes can be applied to your command methods and command groups.

-   @DisCatSharp.CommandsNext.Attributes.AliasesAttribute
-   @DisCatSharp.CommandsNext.Attributes.DescriptionAttribute
-   @DisCatSharp.CommandsNext.Attributes.DontInjectAttribute
-   @DisCatSharp.CommandsNext.Attributes.HiddenAttribute
-   @DisCatSharp.CommandsNext.Attributes.ModuleLifespanAttribute
-   @DisCatSharp.CommandsNext.Attributes.PriorityAttribute
-   @DisCatSharp.CommandsNext.Attributes.RemainingTextAttribute
-   @DisCatSharp.CommandsNext.Attributes.RequireBotPermissionsAttribute
-   @DisCatSharp.CommandsNext.Attributes.RequireCommunityAttribute
-   @DisCatSharp.CommandsNext.Attributes.RequireDirectMessageAttribute
-   @DisCatSharp.CommandsNext.Attributes.RequireGuildAttribute
-   @DisCatSharp.CommandsNext.Attributes.RequireGuildOwnerAttribute
-   @DisCatSharp.CommandsNext.Attributes.RequireMemberVerificationGateAttribute
-   @DisCatSharp.CommandsNext.Attributes.RequireNsfwAttribute
-   @DisCatSharp.CommandsNext.Attributes.RequireOwnerAttribute
-   @DisCatSharp.CommandsNext.Attributes.RequireOwnerOrIdAttribute
-   @DisCatSharp.CommandsNext.Attributes.RequirePermissionsAttribute
-   @DisCatSharp.CommandsNext.Attributes.RequirePrefixesAttribute
-   @DisCatSharp.CommandsNext.Attributes.RequireRolesAttribute
-   @DisCatSharp.CommandsNext.Attributes.RequireUserPermissionsAttribute
-   @DisCatSharp.CommandsNext.Attributes.RequireWelcomeScreenAttribute

## Custom Attributes

If the above attributes don't meet your needs, CommandsNext also gives you the option of writing your own!
Simply create a new class which inherits from `CheckBaseAttribute` and implement the required method.

Our example below will only allow a command to be ran during a specified year.

```cs
public class RequireYearAttribute : CheckBaseAttribute
{
    public int AllowedYear { get; private set; }

    public RequireYearAttribute(int year)
    {
        AllowedYear = year;
    }

    public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
    {
        return Task.FromResult(AllowedYear == DateTime.Now.Year);
    }
}
```

You'll also need to apply the `AttributeUsage` attribute to your attribute.
For our example attribute, we'll set it to only be usable once on methods.

```cs
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class RequireYearAttribute : CheckBaseAttribute
{
    // ...
}
```

You can provide feedback to the user using the `CommandsNextExtension#CommandErrored` event.

```cs
private async Task MainAsync()
{
    var discord = new DiscordClient();
	var commands = discord.UseCommandsNext();

	commands.CommandErrored += CmdErroredHandler;
}

private async Task CmdErroredHandler(CommandsNextExtension _, CommandErrorEventArgs e)
{
    var failedChecks = ((ChecksFailedException)e.Exception).FailedChecks;
    foreach (var failedCheck in failedChecks)
    {
        if (failedCheck is RequireYearAttribute)
        {
            var yearAttribute = (RequireYearAttribute)failedCheck;
            await e.Context.RespondAsync($"Only usable during year {yearAttribute.AllowedYear}.");
        }
    }
}
```

Once you've got all of that completed, you'll be able to use it on a command!

```cs
[Command("generic"), RequireYear(2030)]
public async Task GenericCommand(CommandContext ctx, string generic)
{
    await ctx.RespondAsync("Generic response.");
}
```

<discord-messages>
    <discord-message profile="user">
        !generic
    </discord-message>
    <discord-message profile="dcs">
        Only usable during the year 2030!
    </discord-message>
</discord-messages>
