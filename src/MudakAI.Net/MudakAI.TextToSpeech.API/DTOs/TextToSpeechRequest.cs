namespace MudakAI.TextToSpeech.API.DTOs
{
    public record TextToSpeechRequest(string UniqueId, string Text, string Voice);
}
