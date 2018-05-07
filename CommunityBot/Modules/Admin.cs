﻿using Discord.Commands;
using Discord.WebSocket;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using CommunityBot.Configuration;
using CommunityBot.Features.GlobalAccounts;
using CommunityBot.Handlers;
using CommunityBot.Preconditions;

namespace CommunityBot.Modules
{
    public class Admin : ModuleBase<SocketCommandContext>
    {
        private static readonly OverwritePermissions denyOverwrite = new OverwritePermissions(addReactions: PermValue.Deny, sendMessages: PermValue.Deny, attachFiles: PermValue.Deny);

        [Command("purge", RunMode = RunMode.Async)]
        [Remarks("Purges An Amount Of Messages")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task Clear(int amountOfMessagesToDelete)
        {
            // TODO: Port to Discord .NET 2.0
            //await Context.Message.Channel.DeleteMessagesAsync(await Context.Message.Channel.GetMessagesAsync(amountOfMessagesToDelete).Flatten());

            // So I am not sure how you want the purge command to be but I will make one right now but not add it. Please note this is just a **rough** idea and is still rough around the edges I intend something to improve this.

            //var messages = await Context.Channel.GetMessagesAsync(amountOfMessagesToDelete + 1).FlattenAsync(); // Gets the messages, adds one message to the amount of messages to delete to delete the user's command then flattens it async.

            //foreach(IMessage msg in messages) // Gets every message in the collection we just made above.
            //{
            //    await Task.Delay(600); // Delay so it doesn't reach rate limit.
            //    await msg.DeleteAsync(); // Deletes the message
            //}
        }

        [Command("purge")]
        [Remarks("Purges A User's Last 100 Messages")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task Clear(SocketGuildUser user)
        {
            // TODO: Port to Discord .NET 2.0

            //var messages = await Context.Message.Channel.GetMessagesAsync(100).Flatten();

            //var result = messages.Where(x => x.Author.Id == user.Id && x.CreatedAt >= DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(14)));

            //await Context.Message.Channel.DeleteMessagesAsync(result);

            // We are going to do the same thing above except add a if statement

            //var messages = await Context.Channel.GetMessagesAsync(101).FlattenAsync(); 

            //foreach (IMessage msg in messages) 
            //{
            //    // This is the easy part all we have to do is this
            //    if (msg.Author == user)
            //    {
            //        await Task.Delay(600);
            //        await msg.DeleteAsync();
            //    }
            //}

        }

        [Command("kick")]
        [Remarks("Kick A User")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task Kick([NoSelf] SocketGuildUser user)
        {
            await user.KickAsync();
        }

        [Command("mute")]
        [Remarks("Mutes A User")]
        [RequireUserPermission(GuildPermission.MuteMembers)]
        public async Task Mute(SocketGuildUser user)
        {
            await Context.Guild.GetUser(user.Id).ModifyAsync(x => x.Mute = true);

            var muteRole = await GetMuteRole(user.Guild);
            if (!user.Roles.Any(r => r.Id == muteRole.Id))
                await user.AddRoleAsync(muteRole).ConfigureAwait(false);
        }

        [Command("unmute")]
        [Remarks("Unmutes A User")]
        [RequireUserPermission(GuildPermission.MuteMembers)]
        public async Task Unmute([NoSelf] SocketGuildUser user)
        {
            await Context.Guild.GetUser(user.Id).ModifyAsync(x => x.Mute = false).ConfigureAwait(false);

            try { await user.ModifyAsync(x => x.Mute = false).ConfigureAwait(false); } catch { }
            try { await user.RemoveRoleAsync(await GetMuteRole(user.Guild)).ConfigureAwait(false); } catch { }
        }

        [Command("ban")]
        [Remarks("Ban A User")]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task Ban([NoSelf] SocketGuildUser user)
        {
            await Context.Guild.AddBanAsync(user);
        }

        [Command("unban")]
        [Remarks("Unban A User")]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task Unban([Remainder]string user)
        {
            var bans = await Context.Guild.GetBansAsync();

            var theUser = bans.FirstOrDefault(x => x.User.ToString().ToLowerInvariant() == user.ToLowerInvariant());

            await Context.Guild.RemoveBanAsync(theUser.User).ConfigureAwait(false);
        }

        [Command("unban")]
        [Remarks("Unban A User")]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task Unban(ulong id)
        {
            var bans = await Context.Guild.GetBansAsync();

            var theUser = bans.FirstOrDefault(x => x.User.Id == id);

            await Context.Guild.RemoveBanAsync(theUser.User);
        }

        [Command("nickname")]
        [Remarks("Set A User's Nickname")]
        [RequireUserPermission(Discord.GuildPermission.ManageNicknames)]
        public async Task Nickname(SocketGuildUser username, [Remainder]string name)
        {
            await Context.Guild.GetUser(username.Id).ModifyAsync(x => x.Nickname = name);
        }

        [Command("createtext")]
        [Remarks("Make A Text Channel")]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        public async Task Text(string channelname)
        {
            await Context.Guild.CreateTextChannelAsync(channelname);
        }

        [Command("createvoice")]
        [Remarks("Make A Voice Channel")]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        public async Task Voice([Remainder]string channelname)
        {
            await Context.Guild.CreateVoiceChannelAsync(channelname);
        }

        [Command("announce")]
        [Remarks("Make A Announcement")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Announce([Remainder]string announcement)
        {
            var embed = EmbedHandler.CreateEmbed("Announcement By " + Context.Message.Author, announcement, EmbedHandler.EmbedMessageType.Info, true);

            await Context.Channel.SendMessageAsync("", false, embed);
            await Context.Message.DeleteAsync();
        }

        [Command("echo")]
        [Remarks("Make The Bot Say A Message")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Echo([Remainder] string message)
        {
            var embed = EmbedHandler.CreateEmbed("Message by: " + Context.Message.Author.Username, message, EmbedHandler.EmbedMessageType.Info, true);

            await Context.Channel.SendMessageAsync("", false, embed);
            await Context.Message.DeleteAsync();
        }

        [Command("Game"), Alias("ChangeGame", "SetGame")]
        [Remarks("Change what the bot is currently playing.")]
        [RequireOwner]
        public async Task SetGame([Remainder] string gamename)
        {
            await Context.Client.SetGameAsync(gamename);
            await ReplyAsync($"Changed game to `{gamename}`");
        }

        public async Task<IRole> GetMuteRole(IGuild guild)
        {
            const string defaultMuteRoleName = "Muted";

            var muteRoleName = "Muted";

            var muteRole = guild.Roles.FirstOrDefault(r => r.Name == muteRoleName);

            if (muteRole == null)
            {
                try
                {
                    muteRole = await guild.CreateRoleAsync(muteRoleName, GuildPermissions.None).ConfigureAwait(false);
                }
                catch
                {
                    muteRole = guild.Roles.FirstOrDefault(r => r.Name == muteRoleName) ?? await guild.CreateRoleAsync(defaultMuteRoleName, GuildPermissions.None).ConfigureAwait(false);
                }
            }

            foreach (var toOverwrite in (await guild.GetTextChannelsAsync()))
            {
                try
                {
                    if (!toOverwrite.PermissionOverwrites.Any(x => x.TargetId == muteRole.Id && x.TargetType == PermissionTarget.Role))
                    {
                        await toOverwrite.AddPermissionOverwriteAsync(muteRole, denyOverwrite)
                                .ConfigureAwait(false);

                        await Task.Delay(200).ConfigureAwait(false);
                    }
                }
                catch
                {

                }
            }

            return muteRole;
        }

        [Command("setAvatar"), Remarks("Sets the bots Avatar")]
        [RequireOwner]
        public async Task SetAvatar(string link)
        {
            var s = Context.Message.DeleteAsync();

            try
            {
                var webClient = new WebClient();
                byte[] imageBytes = webClient.DownloadData(link);

                var stream = new MemoryStream(imageBytes);

                var image = new Image(stream);
                await Context.Client.CurrentUser.ModifyAsync(k => k.Avatar = image);
            }
            catch (Exception)
            {
                var embed = EmbedHandler.CreateEmbed("Avatar", "Coult not set the avatar!", EmbedHandler.EmbedMessageType.Exception);
                await Context.Channel.SendMessageAsync("", false, embed);
            }
        }
    }
}
