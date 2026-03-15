using Discord.Interactions;
using MudakAI.Chat.WebService.Resources;

namespace MudakAI.Chat.WebService.CQRS.Commands
{
    public class AnnouncementDiscordModule : InteractionModuleBase
    {
        private const string AnnouncementChannelName = "announcements";
        private const string CommandName = "announcement";
        private const string CommandDescription = "Зробити оголошення";

        [SlashCommand(CommandName, CommandDescription)]
        public async Task SetPersona(string text)
        {
            var guildTextChannels = await Context.Guild.GetTextChannelsAsync();

            var announcementChannel = guildTextChannels.FirstOrDefault(gc => gc.Name == AnnouncementChannelName);
            if (announcementChannel == null)
            {
                await RespondAsync(Strings.Announcement_ChannelNotFound, ephemeral: true);

                return;
            }

            await announcementChannel.SendMessageAsync(text);

            await RespondAsync(Strings.Announcement_Success, ephemeral: true);
        }
    }
}
