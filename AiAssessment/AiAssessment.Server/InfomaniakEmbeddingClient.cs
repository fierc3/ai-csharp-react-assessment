namespace AiAssessment.Server
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;

    public class InfomaniakEmbeddingClient : IEmbeddingClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _model;
        private readonly string _infomaniakId;

        public InfomaniakEmbeddingClient(string apiKey, string model, string baseUrl, string infomaniakId)
        {
            _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            _model = model;
            _infomaniakId = infomaniakId;
        }

        public async Task<float[]> GetEmbeddingAsync(string input)
        {
            var requestBody = new
            {
                model = _model,
                input
            };

            var body = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(body, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"/1/ai/{_infomaniakId}/openai/v1/embeddings", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();

                throw new Exception($"Infomaniak embeddings API error: {(int)response.StatusCode} {response.ReasonPhrase} - {errorContent}");
            }

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<OpenAIEmbeddingResponse>(json);

            if (result?.Data == null || result.Data.Count == 0)
            {

                throw new Exception("No embeddings returned.");
            }

            return [.. result.Data[0].Embedding];
        }

        public async Task<List<float[]>> GetEmbeddingsAsync(IEnumerable<string> inputs)
        {
            const int MaxBatchSize = 99;
            var inputList = inputs.ToList();
            var allEmbeddings = new List<float[]>();

            for (int i = 0; i < inputList.Count; i += MaxBatchSize)
            {
                var batch = inputList.Skip(i).Take(MaxBatchSize).ToList();

                var requestBody = new
                {
                    model = _model,
                    input = batch
                };

                var body = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(body, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"/1/ai/{_infomaniakId}/openai/v1/embeddings", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();

                    throw new Exception($"Infomaniak embeddings API error: {(int)response.StatusCode} {response.ReasonPhrase} - {errorContent}");
                }

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<OpenAIEmbeddingResponse>(json);

                if (result?.Data == null || result.Data.Count == 0)
                {
                    throw new Exception("No embeddings returned.");
                }

                allEmbeddings.AddRange(result.Data.Select(d => d.Embedding.ToArray()));
            }

            return allEmbeddings;
        }
    }

    public interface IEmbeddingClient
    {
        public Task<float[]> GetEmbeddingAsync(string input);
        public Task<List<float[]>> GetEmbeddingsAsync(IEnumerable<string> inputs);
    }

    public class OpenAIEmbeddingResponse
    {
        [JsonProperty("data")]
        public List<EmbeddingData> Data { get; set; }
    }

    public class EmbeddingData
    {
        [JsonProperty("embedding")]
        public List<float> Embedding { get; set; }
    }
}
