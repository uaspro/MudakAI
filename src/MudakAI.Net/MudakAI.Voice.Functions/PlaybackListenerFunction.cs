using Discord.Audio;
using Discord.WebSocket;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using MudakAI.Connectors.Azure.Blob;
using MudakAI.Connectors.Discord.Services;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace MudakAI.Voice.Functions
{
    public class PlaybackRequest
    {
        public string AudioBlobName { get; set; }
    }

    public class PlaybackListenerFunction
    {
        private const int TemporaryAudioFileBufferSize = 4096;
        private const int PCMStreamBufferSizeMillis = 100;

        private static readonly TimeSpan PlaybackDurationLimit = TimeSpan.FromSeconds(60);

        private readonly DiscordClientService _discordClientService;
        private readonly BlobStorageService _blobStorageService;
        private readonly BlobLocksService _blobLocksService;

        public PlaybackListenerFunction(
            DiscordClientService discordClientService,
            BlobStorageService blobStorageService,
            BlobLocksService blobLocksService)
        {
            _discordClientService = discordClientService;
            _blobStorageService = blobStorageService;
            _blobLocksService = blobLocksService;
        }

        [FunctionName("PlaybackListenerFunction")]
        public async Task Run([ServiceBusTrigger("playback", Connection = "ServiceBus")] PlaybackRequest playbackRequest, ILogger log)
        {
            while (!_discordClientService.IsReady)
            {
                await Task.Delay(100);
            }

            var audioBlobName = playbackRequest.AudioBlobName;
            var audioBlobNameParts = audioBlobName.Split('_');

            var guildId = audioBlobNameParts[0];
            var channelId = audioBlobNameParts[1];
            var messageId = audioBlobNameParts[2];

            try
            {
                log.LogInformation(
                    "Aquiring lock for guild '{guildId}', message '{messageId}' for {playDurationLimit}",
                    guildId,
                    messageId,
                    PlaybackDurationLimit);

                await _blobLocksService.AcquireLock(guildId, messageId, PlaybackDurationLimit);

                var voiceChannel = await _discordClientService.DiscordClient.GetChannelAsync(ulong.Parse(channelId)) as SocketVoiceChannel;
                if (voiceChannel == null)
                {
                    return;
                }

                log.LogInformation(
                    "Trying to play audio '{fileName}' in guild '{guildId}'",
                    audioBlobName,
                    guildId);

                var temporaryAudioFilePath = Path.Combine(Environment.CurrentDirectory, audioBlobName);

                try
                {
                    using var temporaryAudioFileStream = 
                        File.Create(temporaryAudioFilePath, TemporaryAudioFileBufferSize, FileOptions.Asynchronous | FileOptions.DeleteOnClose);

                    temporaryAudioFileStream.Seek(0, SeekOrigin.Begin);

                    var audioBlob = await _blobStorageService.Get(audioBlobName);
                    await audioBlob.CopyToAsync(temporaryAudioFileStream);
                    await temporaryAudioFileStream.FlushAsync();

                    using var audioClient = await voiceChannel.ConnectAsync();

                    using var ffmpeg = Process.Start(new ProcessStartInfo
                    {
                        FileName = "ffmpeg",
                        Arguments = $"-hide_banner -loglevel panic -i \"{temporaryAudioFilePath}\" -ac 2 -f s16le -ar 48000 -af \"volume=0.5\" pipe:1",
                        UseShellExecute = false,
                        RedirectStandardOutput = true
                    });

                    using var output = ffmpeg.StandardOutput.BaseStream;
                    using var discordPCMStream = audioClient.CreatePCMStream(AudioApplication.Mixed, bufferMillis: PCMStreamBufferSizeMillis);

                    try
                    {
                        await output.CopyToAsync(discordPCMStream);
                    }
                    finally
                    {
                        await discordPCMStream.FlushAsync();
                    }

                    await audioClient.StopAsync();

                    await _blobStorageService.Delete(audioBlobName);

                    log.LogInformation(
                        "Finished playing audio '{fileName}' in guild '{guildId}'",
                        audioBlobName,
                        guildId);
                }
                finally
                {
                    File.Delete(temporaryAudioFilePath);
                }
            }
            catch (Exception ex)
            {
                log.LogError(
                    "Error while playing audio '{fileName}' in discord channel '{channelId}'. Failure: {failure}",
                    audioBlobName,
                    channelId,
                    ex);

                throw;
            }
            finally
            {
                await _blobLocksService.ReleaseLock(guildId, messageId);
            }
        }
    }
}
