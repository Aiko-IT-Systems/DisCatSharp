---
uid: commands_command_attributes
title: Command Attributes
---

## Built-In Attributes
CommandsNext has a variety of built-in attributes to enhance your commands and provide some access control.
The majority of these attributes can be applied to your command methods and command groups.

- @DSharpPlusNextGen.CommandsNext.Attributes.AliasesAttribute
- @DSharpPlusNextGen.CommandsNext.Attributes.CooldownAttribute
- @DSharpPlusNextGen.CommandsNext.Attributes.DescriptionAttribute
- @DSharpPlusNextGen.CommandsNext.Attributes.DontInjectAttribute
- @DSharpPlusNextGen.CommandsNext.Attributes.HiddenAttribute
- @DSharpPlusNextGen.CommandsNext.Attributes.ModuleLifespanAttribute
- @DSharpPlusNextGen.CommandsNext.Attributes.PriorityAttribute
- @DSharpPlusNextGen.CommandsNext.Attributes.RemainingTextAttribute
- @DSharpPlusNextGen.CommandsNext.Attributes.RequireBotPermissionsAttribute
- @DSharpPlusNextGen.CommandsNext.Attributes.RequireCommunityAttribute
- @DSharpPlusNextGen.CommandsNext.Attributes.RequireDirectMessageAttribute
- @DSharpPlusNextGen.CommandsNext.Attributes.RequireDiscordCertifiedModeratorAttribute
- @DSharpPlusNextGen.CommandsNext.Attributes.RequireDiscordEmployeeAttribute
- @DSharpPlusNextGen.CommandsNext.Attributes.RequireGuildAttribute
- @DSharpPlusNextGen.CommandsNext.Attributes.RequireGuildOwnerAttribute
- @DSharpPlusNextGen.CommandsNext.Attributes.RequireMemberVerificationGateAttribute
- @DSharpPlusNextGen.CommandsNext.Attributes.RequireNsfwAttribute
- @DSharpPlusNextGen.CommandsNext.Attributes.RequireOwnerAttribute
- @DSharpPlusNextGen.CommandsNext.Attributes.RequireOwnerOrIdAttribute
- @DSharpPlusNextGen.CommandsNext.Attributes.RequirePermissionsAttribute
- @DSharpPlusNextGen.CommandsNext.Attributes.RequirePrefixesAttribute
- @DSharpPlusNextGen.CommandsNext.Attributes.RequireRolesAttribute
- @DSharpPlusNextGen.CommandsNext.Attributes.RequireUserPermissionsAttribute
- @DSharpPlusNextGen.CommandsNext.Attributes.RequireWelcomeScreenAttribute


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

<br/>
You'll also need to apply the `AttributeUsage` attribute to your attribute.<br/>
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

<br/>
Once you've got all of that completed, you'll be able to use it on a command!
```cs
[Command("generic"), RequireYear(2030)]
public async Task GenericCommand(CommandContext ctx, string generic)
{
    await ctx.RespondAsync("Generic response.");
}
```

![Generic Image](/images/commands_command_attributes_01.png)
