using Discord.Interactions;

namespace MudakAI.Chat.WebService.CQRS.Commands
{
    public class AnnouncementDiscordModule : InteractionModuleBase
    {
        private const string AnnouncementChannelName = "announcements";

        [SlashCommand("announcement", "Зробити оголошення")]
        public async Task SetPersona(string text)
        {
            var guildTextChannels = await Context.Guild.GetTextChannelsAsync();

            var announcementChannel = guildTextChannels.FirstOrDefault(gc => gc.Name == AnnouncementChannelName);
            if (announcementChannel == null)
            {
                await RespondAsync($"Не знайдено каналу оголошень!", ephemeral: true);

                return;
            }

            await announcementChannel.SendMessageAsync(text);

            await RespondAsync("Оголошення успішно опубліковане!", ephemeral: true);
        }
    }
}
