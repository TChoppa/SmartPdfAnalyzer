using System.Net.Http.Json;
using System.Text.Json;

namespace PdfRagApi.Services;

public class EmbeddingService
{
    private readonly HttpClient _httpClient;
    private readonly string _ollamaBaseUrl;
    private const string EmbeddingModel = "nomic-embed-text"; // Free, local via Ollama

    public EmbeddingService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _ollamaBaseUrl = configuration["Ollama:BaseUrl"] ?? "http://localhost:11434";
    }

    /// <summary>
    /// Gets an embedding vector for a single text.
    /// </summary>
    public async Task<float[]> GetEmbeddingAsync(string text)
    {
        var request = new { model = EmbeddingModel, prompt = text };

        var response = await _httpClient.PostAsJsonAsync(
            $"{_ollamaBaseUrl}/api/embeddings", request);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        var embeddingArray = json.GetProperty("embedding").EnumerateArray()
            .Select(e => e.GetSingle())
            .ToArray();

        return embeddingArray;
    }

    /// <summary>
    /// Gets embeddings for multiple texts (batched with small delay to avoid overwhelming Ollama).
    /// </summary>
    public async Task<float[][]> GetEmbeddingsBatchAsync(IEnumerable<string> texts)
    {
        var results = new List<float[]>();

        foreach (var text in texts)
        {
            var embedding = await GetEmbeddingAsync(text);
            results.Add(embedding);
            await Task.Delay(10); // small pause to avoid hammering local Ollama
        }

        return results.ToArray();
    }
}