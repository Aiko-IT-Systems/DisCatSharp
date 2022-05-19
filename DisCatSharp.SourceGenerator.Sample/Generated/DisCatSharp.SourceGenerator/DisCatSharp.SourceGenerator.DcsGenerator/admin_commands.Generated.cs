
/*
	A T T E N T I O N

	Do not modify this file.
	It is auto-generated and will be overriden during build.

	A T T E N T I O N
*/

using DisCatSharp.SourceGenerator.Sample;
using DisCatSharp;
using DisCatSharp.Entities;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
public abstract class admin_commands
{
	[SlashCommand("ac-test","Testing autocomplete provider")]
    private async Task ac_testAsync(InteractionContext context, [Autocomplete(typeof(MyAcProvider)), Option("ac-param", "values are given by ac provider", true)] string acparam)
    {
        await HandleSlashInteraction_ac_test(context, acparam);
    }
    


    public abstract Task HandleSlashInteraction_ac_test(InteractionContext context, string acparam);


[SlashCommand("ban-user","Bans a designated user from server")]
    private async Task ban_userAsync(InteractionContext context, [Option("User", "user to ban")] DiscordUser user, [Option("Reason", "Reasoning for ban")] string reason)
    {
        await HandleSlashInteraction_ban_user(context, user,reason);
    }
    


    public abstract Task HandleSlashInteraction_ban_user(InteractionContext context, DiscordUser user, string reason);

}