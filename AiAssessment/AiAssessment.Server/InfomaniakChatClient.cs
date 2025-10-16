namespace AiAssessment.Server
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.AI;
    using Newtonsoft.Json;

    public class InfomaniakChatClient : IChatClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _model;
        private readonly string _infomaniakId;


        public InfomaniakChatClient(string apiKey, string model, string baseUrl, string infomaniakId)
        {
            _httpClient = new HttpClient();
            _model = model;
            _infomaniakId = infomaniakId;

            _httpClient.BaseAddress = new Uri(baseUrl);
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);
            _infomaniakId = infomaniakId;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }


        public async Task<ChatResponse> GetResponseAsync(IEnumerable<Microsoft.Extensions.AI.ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
        {
            var request = new InfomaniakChatRequest
            {
                Model = _model,
                Messages = messages.Select(m => new InfomaniakChatMessage
                {
                    Role = m.Role.ToString().ToLowerInvariant(),
                    Content = m.Text
                }).ToList(),
                Stream = false,
                Temperature = options?.Temperature ?? 0.7,
                TopP = options?.TopP ?? 0.9,
                MaxTokens = options?.MaxOutputTokens ?? 5000
            };

            var json = JsonConvert.SerializeObject(
                request,
                Newtonsoft.Json.Formatting.Indented,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"/1/ai/{_infomaniakId}/openai/chat/completions", content, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<InfomaniakChatResponse>(responseBody);

            var choice = result?.Choices?.FirstOrDefault();

            if (choice == null)
            {
                throw new InvalidOperationException("No choices returned by Infomaniak AI.");
            }

            var chatResponse = new ChatResponse(new ChatMessage(ChatRole.Assistant, choice.Message.Content))
            {
                Usage = result?.Usage != null
                    ? new UsageDetails
                    {
                        InputTokenCount = result.Usage.InputTokens,
                        OutputTokenCount = result.Usage.OutputTokens,
                        TotalTokenCount = result.Usage.TotalTokens
                    }
                    : new UsageDetails
                    {
                        InputTokenCount = 0,
                        OutputTokenCount = 0,
                        TotalTokenCount = 99 // Fallback
                    }
            };

            return chatResponse;
        }

        public object GetService(Type serviceType, object serviceKey = null)
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<Microsoft.Extensions.AI.ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }


    public class InfomaniakChatRequest
    {
        [JsonProperty("model")]
        public string Model { get; set; }

        [JsonProperty("messages")]
        public List<InfomaniakChatMessage> Messages { get; set; }

        [JsonProperty("temperature")]
        public double Temperature { get; set; } = 0.7;

        [JsonProperty("top_p")]
        public double TopP { get; set; } = 1.0;

        [JsonProperty("max_tokens")]
        public int MaxTokens { get; set; } = 256;

        [JsonProperty("frequency_penalty")]
        public double? FrequencyPenalty { get; set; }

        [JsonProperty("presence_penalty")]
        public double? PresencePenalty { get; set; }

        [JsonProperty("stop")]
        public List<string> Stop { get; set; }

        [JsonProperty("profile_type")]
        public string ProfileType { get; set; }

        [JsonProperty("seed")]
        public int? Seed { get; set; }

        [JsonProperty("stream")]
        public bool Stream { get; set; } = false;
    }

    public class InfomaniakChatMessage
    {
        [JsonProperty("role")]
        public string Role { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }
    }

    public class InfomaniakChatResponse
    {
        [JsonProperty("choices")]
        public List<InfomaniakChoice> Choices { get; set; }

        [JsonProperty("usage")]
        public InfomaniakUsage Usage { get; set; }
    }

    public class InfomaniakChoice
    {
        [JsonProperty("message")]
        public InfomaniakChatMessage Message { get; set; }

        [JsonProperty("delta")]
        public InfomaniakChatMessage Delta { get; set; }
    }

    public class InfomaniakUsage
    {
        [JsonProperty("input_tokens")]
        public int InputTokens { get; set; }

        [JsonProperty("output_tokens")]
        public int OutputTokens { get; set; }

        [JsonProperty("prompt_tokens")]
        public int PromptTokens { get; set; }

        [JsonProperty("completion_tokens")]
        public int CompletionTokens { get; set; }

        [JsonProperty("total_tokens")]
        public int TotalTokens { get; set; }
    }

}
