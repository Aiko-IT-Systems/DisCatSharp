# Summary
### [Threads](https://github.com/discord/discord-api-docs/pull/2855/files)

- [x] Thread methods
- [x] Gateway Events
- [x] Opcodes / Statuscodes
- [x] Guild Structure
- [x] Channels / Messages
- [x] Permissions

### TODO Lala
- [x] Activity buttons (should also close #692)
- [x] _Guild member mute and deafen (Already in master)_
- [x] Application flags
- [x] Application invites
- [x] Application privacy policy/tos
- [x]  Mentionable slash command option type
- [x]  Webhooks source guild and channel
- [x]  Invite expiration
- [x]  Message application id
- [x]  Webhook url
- [x]  Guild create and modify fields (Won't include `features` in edit/create, because it's set by discord)
- [x]  _Team name (Already in master)_
- [x] Webhook attachment patch and Channel attachment patch
- [x] Modify channel position
- [x] Integration events
- [x] Stage Instance
- [x] Stage Instance Events
- [x] Webhooks on threads
- [x] Api v9 and threads
- [x] Slash command permissions
- [x] Get webhook message/interaction response
- [x] Remove user dms endpoint and Remove private channels

# Details
- Application flags
_Flags will be 0 because of `Returns the bot's OAuth2 [application](#DOCS_TOPICS_OAUTH2/application) object without flags.`_

- [Application TOS & Privacy URL & TeamName Field](https://github.com/DSharpPlus/DSharpPlus/pull/890#issuecomment-845488837)

![image](https://user-images.githubusercontent.com/14029133/119054644-45c3f500-b9c8-11eb-9211-9264076adaa6.png)

- Activity Buttons

- [Application Invites](https://github.com/DSharpPlus/DSharpPlus/pull/890#issuecomment-845454359) 

![image](https://user-images.githubusercontent.com/14029133/119038431-65045780-b9b3-11eb-8de4-1fd9aef8265c.png)
![2021-05-21_08h44_45](https://user-images.githubusercontent.com/14029133/119100710-877c8c00-ba18-11eb-81da-fb239eafb938.png)

- Mentionable slash command option type

![image](https://user-images.githubusercontent.com/14029133/119069086-b9282f80-b9e5-11eb-9439-de46ab1148aa.png)

- [Invite expiration](https://github.com/DSharpPlus/DSharpPlus/pull/890#issuecomment-845488111)

![2021-05-20_23h24_41](https://user-images.githubusercontent.com/14029133/119050850-b1a35f00-b9c2-11eb-9177-a084079395a1.png)

- [Message application id](https://github.com/DSharpPlus/DSharpPlus/pull/890#issuecomment-845734506)
_Used on slash command responses_

![image](https://user-images.githubusercontent.com/14029133/119092511-d1607480-ba0e-11eb-8b79-7496c63bca16.png)

- [Webhooks source guild and channel](https://github.com/DSharpPlus/DSharpPlus/pull/890#issuecomment-845475748)

![image](https://user-images.githubusercontent.com/14029133/119048526-a26ee200-b9bf-11eb-862f-3d1b94318eee.png)

- Webhook url
_Don't know how to test_

- Guild create and modify fields

[Preview](https://github.com/DSharpPlus/DSharpPlus/pull/890#issuecomment-845564078) 
![image](https://user-images.githubusercontent.com/14029133/119063842-49f90e00-b9da-11eb-9b75-b305f7d05c8f.png)
[Self](https://github.com/DSharpPlus/DSharpPlus/pull/890#issuecomment-845565105)
![image](https://user-images.githubusercontent.com/14029133/119064277-4ca83300-b9db-11eb-9f33-49f5a5376852.png)

- Webhook attachment patch and Channel attachment patch

- [Modify Channel Position](https://github.com/DSharpPlus/DSharpPlus/pull/890#issuecomment-845583553)

- [Integration events](https://github.com/DSharpPlus/DSharpPlus/pull/890#issuecomment-845537673)
_Twitch & YouTube.._

![2021-05-22_02h31_03](https://user-images.githubusercontent.com/14029133/119209555-88a8ca00-baa7-11eb-8a8d-3a578d8b5007.png)

- [Stage Instance Events](https://github.com/DSharpPlus/DSharpPlus/pull/890#issuecomment-845458837)

![image](https://user-images.githubusercontent.com/14029133/119058818-5fb50600-b9cf-11eb-90b3-26c568c3d0b5.png)

- [Stage Instance](https://github.com/DSharpPlus/DSharpPlus/pull/890#issuecomment-845451032) - [Example 2 - Advanced](https://github.com/DSharpPlus/DSharpPlus/pull/890#issuecomment-847535179)
Use with `channel.OpenStageAsync("topic");`

![2021-05-20_22h05_58](https://user-images.githubusercontent.com/14029133/119042043-a26ae400-b9b7-11eb-942f-11624c02f39d.png)
![2021-05-20_22h06_02](https://user-images.githubusercontent.com/14029133/119042054-a39c1100-b9b7-11eb-8ab4-8210b9858498.png)

- [Webhooks on threads](https://github.com/DSharpPlus/DSharpPlus/pull/890#issuecomment-845777692)
_Works_

- Slash Command Permissions

- Threads

- Deprecate guild region value



Stage channel control example:

```csharp
        [Command("openstage"), Description("Opens a stage")]
        public async Task OpenStageAsync(CommandContext ctx, [Description("Stage channel")] DiscordChannel channel, [RemainingText, Description("Stage topic")] string topic)
        {
            if (channel.Type != ChannelType.Stage)
                return;

            var stage = await channel.OpenStageAsync(topic);
            await ctx.RespondAsync($"Opened stage {channel.Name} with topic `{topic}`. It has the ID {stage.Id}");
        }

        [Command("modifystage"), Description("Modifies a stage topic")]
        public async Task CloseStageAsync(CommandContext ctx, [Description("Stage channel")] DiscordChannel channel, [RemainingText, Description("New topic")] string topic)
        {
            if (channel.Type != ChannelType.Stage)
                return;

            await channel.ModifyStageAsync(topic);
            await ctx.RespondAsync($"Modified stage {channel.Name} with new topic {topic}.");
        }

        [Command("closestage"), Description("Closes a stage")]
        public async Task CloseStageAsync(CommandContext ctx, [Description("Stage channel")] DiscordChannel channel)
        {
            if (channel.Type != ChannelType.Stage)
                return;

            await channel.CloseStageAsync();
            await ctx.RespondAsync($"Closed stage {channel.Name}.");
        }
```




Application invite example:
```csharp
        [Command("yt"), Description("Generate YouTube Together Invite")]
        public async Task GenerateYouTubeTogetherInviteAsync(CommandContext ctx, DiscordChannel channel)
        {
            DiscordInvite invite = await channel.CreateInviteAsync(0, 0, TargetType.EmbeddedApplication, TargetActivity.YouTubeTogether);

            await ctx.RespondAsync($"https://discord.gg/{invite.Code}");
        }

        [Command("fish"), Description("Generate Fishington Invite")]
        public async Task GenerateFishingtonInviteAsync(CommandContext ctx, DiscordChannel channel)
        {
            DiscordInvite invite = await channel.CreateInviteAsync(0, 0, TargetType.EmbeddedApplication, TargetActivity.Fishington);

            await ctx.RespondAsync($"https://discord.gg/{invite.Code}");
        }
```




Stage Instance Event:
```csharp
            Client.StageInstanceCreated += Client_StageInstanceCreatedAsync;
            Client.StageInstanceDeleted += Client_StageInstanceDeletedAsync;
            Client.StageInstanceUpdated += Client_StageInstanceUpdatedAsync;
```
[..]
```csharp
        private async Task Client_StageInstanceUpdatedAsync(DiscordClient sender, StageInstanceUpdateEventArgs e)
        {
            Console.WriteLine($"[StageInstance] Update: In guild {e.StageInstance.GuildId} inside channel {e.StageInstance.ChannelId} with id {e.StageInstance.Id} and new topic {e.StageInstance.Topic}");
            await Task.Delay(200);
        }

        private async Task Client_StageInstanceDeletedAsync(DiscordClient sender, StageInstanceDeleteEventArgs e)
        {
            Console.WriteLine($"[StageInstance] Delete: In guild {e.StageInstance.GuildId} inside channel {e.StageInstance.ChannelId} with id {e.StageInstance.Id}");
            await Task.Delay(200);
        }

        private async Task Client_StageInstanceCreatedAsync(DiscordClient sender, StageInstanceCreateEventArgs e)
        {
            Console.WriteLine($"[StageInstance] Create: In guild {e.StageInstance.GuildId} inside channel {e.StageInstance.ChannelId} with id {e.StageInstance.Id} and topic {e.StageInstance.Topic}");
            await Task.Delay(200);
        }
```




Webhook Source Guild & Channel
```csharp
        [Command("webhooks"), Description("Gets webhooks")]
        public async Task GetWebhooksAsync(CommandContext ctx)
        {
            var webhooks = await ctx.Guild.GetWebhooksAsync();
            foreach(DiscordWebhook webhook in webhooks)
            {
                if (webhook.SourceGuild != null)
                {
                    await ctx.RespondAsync($"This webhook follows {webhook.SourceGuild.Name}'s channel {webhook.SourceChannel.Name} in {ctx.Guild.GetChannel(webhook.ChannelId).Mention}");
                } else
                {
                    await ctx.RespondAsync($"This webhook is a normal webhook for {ctx.Guild.GetChannel(webhook.ChannelId).Mention}");
                }
            }
        }
```





Invite Expires_At
```csharp
        [Command("invites"), Description("Gets invites")]
        public async Task GetInvitesAsync(CommandContext ctx)
        {
            var invites = await ctx.Guild.GetInvitesAsync();
            foreach (DiscordInvite invite in invites)
            {
                DiscordInvite test = await ctx.Client.GetInviteByCodeAsync(invite.Code, true, true);
                DiscordEmbedBuilder builder = new DiscordEmbedBuilder();
                builder.WithTitle($"Invite {test.Code}")
                    .WithAuthor(test.Inviter.Username, null, test.Inviter.AvatarUrl)
                    .WithTimestamp(test.CreatedAt)
                    .WithDescription($"Max uses: {invite.MaxUses}\n" +
                    $"Expires {test.ExpiresAt}\n" +
                    $"Max age: {test.MaxAge}\n" +
                    $"Channel: {test.Channel.Name}");
                if(test.TargetType == TargetType.EmbeddedApplication)
                {
                    builder.AddField("Activity", test.TargetApplication.Name);
                    builder.AddField("ActivityFlags", test.TargetApplication.Flags.ToString());
                }
                await ctx.RespondAsync(builder.Build());
            }
        }
```





Added Application TOS & Privacy URL & TeamName
```csharp

        [Command("dev"), Description("Dev!")]
        public async Task GetDeveloperInfoAsync(CommandContext ctx)
        {
            try
            {
                var app = Process.GetCurrentProcess();
                var emb = new DiscordEmbedBuilder()
                .WithColor(new DiscordColor(0xC665A))
                .WithAuthor($"{ctx.Client.CurrentUser.Username}#{ctx.Client.CurrentUser.Discriminator}", $"{ctx.Client.CurrentApplication.GenerateBotOAuth(Permissions.Administrator)}", $"{ctx.Client.CurrentUser.AvatarUrl}")
                .WithTitle("Developer Stuff")
                .WithThumbnail($"{ctx.Client.CurrentApplication.CoverImageUrl}")
                .WithDescription(":3")
                .AddField($"D# Version", ctx.Client.VersionString)
                .AddField($"TOS", ctx.Client.CurrentApplication.TermsOfServiceUrl)
                .AddField($"Privacy", ctx.Client.CurrentApplication.PrivacyPolicyUrl)
                .AddField($"Presence status", ctx.Client.Presences.Values.Where(g => g.User == ctx.Client.CurrentUser).First().Status.ToString())
                .WithFooter($"{ctx.Client.CurrentApplication.Name} developed by {ctx.Client.CurrentApplication.TeamName} | {ctx.Client.CurrentApplication.Team.CreationTimestamp.DateTime}", $"{ctx.Client.CurrentApplication.Team.Icon}");
                await ctx.Channel.SendMessageAsync("Dev Info ready :3", emb.Build());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " | " + ex.StackTrace);
            }
            finally
            {
                await ctx.Message.DeleteAsync("Command Hide");
            }
        }
```





```csharp
            Client.GuildIntegrationCreated += Client_GuildIntegrationCreatedAsync;
            Client.GuildIntegrationUpdated += Client_GuildIntegrationUpdatedAsync;
            Client.GuildIntegrationDeleted += Client_GuildIntegrationDeletedAsync;
```
[..]
```csharp
        private async Task Client_GuildIntegrationCreatedAsync(DiscordClient sender, GuildIntegrationCreateEventArgs e)
        {
            Console.WriteLine($"[Guildintegration] Create: In guild {e.GuildId} the integration {e.Integration.Type} was created with account {e.Integration.Account.Name}.");
            await Task.Delay(200);
        }

        private async Task Client_GuildIntegrationUpdatedAsync(DiscordClient sender, GuildIntegrationUpdateEventArgs e)
        {
            Console.WriteLine($"[Guildintegration] Update: In guild {e.GuildId} the integration {e.Integration.Type} was updated with account {e.Integration.Account.Name}.");
            await Task.Delay(200);
        }

        private async Task Client_GuildIntegrationDeletedAsync(DiscordClient sender, GuildIntegrationDeleteEventArgs e)
        {
            Console.WriteLine($"[Guildintegration] Delete: In guild {e.GuildId} the integration {e.IntegrationId} was deleted");
            await Task.Delay(200);
        }
```




Guild Preview Description
```csharp
        [Command("preview"), Description("Preview guild")]
        public async Task GetGuildPreviewAsync(CommandContext ctx, ulong guild)
        {
            DiscordGuildPreview p = await ctx.Client.GetGuildPreviewAsync(guild);
            await ctx.RespondAsync($"Guild **{p.Name}** has description `{p.Description}`.");
        }
```




Get Guild with SystemChannelFlags & Modify JoinNotification
```csharp
        [Command("guild"), Description("Get guild")]
        public async Task GetGuildAsync(CommandContext ctx)
        {
            await ctx.RespondAsync($"Guild **{ctx.Guild.Name}** has description `{ctx.Guild.Description}` and system channel flags `{ctx.Guild.SystemChannelFlags}`. Updating to surpress join notification");
            await ctx.Guild.ModifyAsync(g => g.SystemChannelFlags = (ctx.Guild.SystemChannelFlags | SystemChannelFlags.SuppressJoinNotifications));
            await ctx.RespondAsync($"Guild **{ctx.Guild.Name}** has new system channel flags `{ctx.Guild.SystemChannelFlags}`.");
        }
```




Modify Channel Position with sync permissions on new parent
```csharp
        [Command("movechannel"), Description("Move Channel")]
        public async Task MoveChannelAsync(CommandContext ctx, DiscordChannel channel, ulong parentId)
        {
            await channel.ModifyPositionAsync(1, true, parentId);
        }
```

Remove channel from parent
```csharp
        [Command("removeparent"), Description("Remove Parent")]
        public async Task RemoveParentAsync(CommandContext ctx, DiscordChannel channel)
        {
            await channel.RemoveParentAsync();
        }
```




Message application id

```csharp
        [Command("appid"), Description("Get application id")]
        public async Task GetApplicationIdAsync(CommandContext ctx, ulong msgid)
        {
            DiscordMessage msg = await ctx.Channel.GetMessageAsync(msgid);
            await ctx.RespondAsync($"Application ID: {msg.ApplicationId}");
        }
```





Execute webhook on thread
```csharp
// thread_id defaults to null, so it doesn't need to be set
ExecuteAsync(DiscordWebhookBuilder builder, string thread_id = null)
```




> 
> 
> This isn't handled. Seems new: `[2021-05-22 17:50:36 +02:00] [105 /WebSocketRec] [Warn ] Unknown event: GUILD_JOIN_REQUEST_DELETE payload: "user_id": "69564852<redacted>", "guild_id": "8172324<redacted>"`
> I guess it's part of the upcoming membership screening application

Here this
Ether it's when the user clicks `Leave Server` or the mod selects `Deny`
![2021-05-22_23h07_04](https://user-images.githubusercontent.com/14029133/119240877-08d83980-bb53-11eb-9fbe-fd6dccb5c0e2.png)
![AaDJhYW](https://user-images.githubusercontent.com/14029133/119240878-0a096680-bb53-11eb-8f14-7b044b40d84e.png)
![unknown](https://user-images.githubusercontent.com/14029133/119240880-0a096680-bb53-11eb-9268-d47a0e4b01fb.png)
![2021-05-22_23h07_39](https://user-images.githubusercontent.com/14029133/119240881-0a096680-bb53-11eb-80a6-13f7d5230c17.png)






> 
> 
> This isn't handled. Seems new: `[2021-05-22 17:50:36 +02:00] [105 /WebSocketRec] [Warn ] Unknown event: GUILD_JOIN_REQUEST_DELETE payload: "user_id": "69564852<redacted>", "guild_id": "8172324<redacted>"`
> I guess it's part of the upcoming membership screening application

Is part of the next version of membership screening
https://discord.com/channels/641574644578648068/689591708962652289/845836910991507486
Can be ignored for now

Answer from advaith: 
> iirc it happens when a user leaves a server where they havent completed screening yet
https://discord.com/channels/641574644578648068/689591708962652289/845838160047112202





### Stage Channel Example v2
Stage channel control example with music part:

```csharp
        [Command("openstage"), Description("Opens a stage with the bot")]
        public async Task OpenStageAsync(CommandContext ctx, [Description("Stage Channel")] DiscordChannel channel, [RemainingText, Description("Stage Topic")] string topic)
        {
            if (channel.Type != ChannelType.Stage)
            {
                await ctx.RespondAsync("This is no stage channel!");
            }

            var vnext = ctx.Client.GetVoiceNext();
            if (vnext == null)
            {
                await ctx.RespondAsync("VNext is not enabled or configured.");
                return;
            }

            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc != null)
            {
                vnc.Disconnect();
                await vnext.ConnectAsync(channel);
            }
            else
            {
                await vnext.ConnectAsync(channel);
            }

            await channel.OpenStageAsync(topic);

            var self = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);
            await self.UpdateVoiceStateAsync(channel, false);
        }

        [Command("closestage"), Description("Closes a stage with the bot")]
        public async Task CloseStageAsync(CommandContext ctx, [Description("Stage Channel")] DiscordChannel channel)
        {
            if (channel.Type != ChannelType.Stage)
            {
                await ctx.RespondAsync("This is no stage channel!");
            }

            var stage = await channel.GetStageAsync();
            if (stage == null)
            {
                await ctx.RespondAsync("This stage is already closed!");
            }

            var vnext = ctx.Client.GetVoiceNext();
            if (vnext == null)
            {
                await ctx.RespondAsync("VNext is not enabled or configured.");
                return;
            }

            var vnc = vnext.GetConnection(ctx.Guild);

            if (vnc != null)
            {
                var self = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);
                await self.UpdateVoiceStateAsync(channel, true);
                vnc.Disconnect();
            }

            await channel.CloseStageAsync();
        }

        [Command("youtube"), Description("Play a YouTube Video")]
        public async Task PlayYouTubeAsync(CommandContext ctx, string url, DiscordChannel chn = null)
        {
            try
            {
                string rf = RandomString(30);
                var vnext = ctx.Client.GetVoiceNext();
                if (vnext == null)
                {
                    await ctx.RespondAsync("VNext is not enabled or configured.");
                    return;
                }

                var vstat = ctx.Member?.VoiceState;
                if (vstat?.Channel == null && chn == null)
                {
                    await ctx.RespondAsync("You are not in a voice channel.");
                    return;
                }

                if (chn == null)
                    chn = vstat.Channel;

                var vnc = vnext.GetConnection(ctx.Guild);
                if (vnc != null)
                {
                    // Already Connected
                }
                else
                {
                    vnc = await vnext.ConnectAsync(chn);
                    await ctx.Channel.SendMessageAsync($"Connected to `{chn.Name}`");
                }

                Exception exc = null;

                try
                {
                    if (vnc.IsPlaying)
                    {
                        await vnc.SendSpeakingAsync(false);
                        vnc.Disconnect();
                        vnc = await vnext.ConnectAsync(chn);
                    }
                    YoutubeDL yt = new YoutubeDL
                    {
                        OverwriteFiles = true,
                        OutputFileTemplate = $"yt-{rf}.mp3",
                        YoutubeDLPath = (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "/usr/bin/youtube-dl" : $"youtube-dl.exe")
                    };
                    var res = await yt.RunVideoDataFetch(url, ytc.Token, true, new OptionSet() { AudioFormat = AudioConversionFormat.Mp3 });
                    await yt.RunAudioDownload(url, AudioConversionFormat.Mp3);
                    VideoData video = res.Data;
                    string title = video.Title;
                    string uploader = video.Uploader;

                    DiscordMessage playState = await ctx.RespondAsync($"Playing `{title}` from `{uploader}`!");
                    await vnc.SendSpeakingAsync(true);

                    var psi = new ProcessStartInfo
                    {
                        FileName = (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "/usr/bin/ffmpeg" : $"ffmpeg.exe"),
                        Arguments = $@"-ac 2 -f s16le -ar 48000 pipe:1 -loglevel quiet -i yt-{rf}.mp3",
                        RedirectStandardOutput = true,
                        UseShellExecute = false
                    };
                    var ffmpeg = Process.Start(psi);
                    var ffout = ffmpeg.StandardOutput.BaseStream;

                    var txStream = vnc.GetTransmitSink();
                    await ffout.CopyToAsync(txStream);
                    await txStream.FlushAsync();
                    await vnc.WaitForPlaybackFinishAsync();
                    await vnc.SendSpeakingAsync(false);
                    string playInfo = playState.Content.Replace("Playing", "");
                    await playState.ModifyAsync($"Finished playing" + playInfo);
                }
                catch (Exception ex) { exc = ex; }

                if (exc != null)
                    await ctx.RespondAsync($"An exception occured during playback: `{exc.GetType()}: {exc.Message}`");

                File.Delete($"yt-{rf}.mp3");
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
            }
        }
```

Automatically make the bot speaker when joining a stage channel
```csharp
// Unnecessary code removed
        public Bot() {

            Client.VoiceStateUpdated += Client_VoiceStateUpdatedAsync;

            Voice = Client.UseVoiceNext(new VoiceNextConfiguration
            {
                AudioFormat = AudioFormat.Default,
                EnableIncoming = false
            });
        }

        private async Task Client_VoiceStateUpdatedAsync(DiscordClient sender, VoiceStateUpdateEventArgs e)
        {
            if (e.User == sender.CurrentUser)
            {
                if (e.Before == null)
                {
                    if (e.After.Channel.IsStage)
                    {
                        DiscordMember self = await e.Guild.GetMemberAsync(e.User.Id);
                        await self.UpdateVoiceStateAsync(e.Channel, false);
                    }
                }
            }
        }
``` 
