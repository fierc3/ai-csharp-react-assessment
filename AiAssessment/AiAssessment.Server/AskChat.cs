using Microsoft.Extensions.AI;

namespace AiAssessment.Server
{
    public class AskChat
    {
        public async Task<AskChatMessage> GenerateMessage(List<AskChatMessage> History)
        {
            var apiKey = Environment.GetEnvironmentVariable("INFOMANIAK_API_KEY");
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("API key is missing.");

            var chatClient = new InfomaniakChatClient(apiKey, "gemma3n", "https://api.infomaniak.com\t", "104108");
            var embeddingClient = new InfomaniakEmbeddingClient(apiKey, "bge_multilingual_gemma2", "https://api.infomaniak.com\t", "104108");

            // Convert History to List<Microsoft.Extensions.AI.ChatMessage>
            var chatMessages = History.Select(h =>
                new ChatMessage(
                    h.Sender.Equals("bot", System.StringComparison.OrdinalIgnoreCase) ? ChatRole.Assistant : ChatRole.User,
                    h.Message ?? string.Empty
                )
            ).ToList();

            var llmAnswer = await chatClient.GetResponseAsync(chatMessages);
            var embeddingAnswer = await embeddingClient.GetEmbeddingAsync(History.Last().Message ?? "nothing");
            string embeddingString = string.Join(", ", embeddingAnswer);

            var response =
                new AskChatMessage(
                    "bot",
                    llmAnswer.Text ?? "Couldnt generate answer"
                );

            return response;
        }
    }

    public record AskChatMessage(string Sender, string? Message){}

    public record ChatRequest(List<AskChatMessage> History) { }
}
