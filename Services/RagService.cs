using System.Net.Http.Json;
using System.Text.Json;
using PdfRagApi.Models;
using PdfRagApi.VectorStore;

namespace PdfRagApi.Services;

public class RagService
{
    private readonly InMemoryVectorDb _vectorDb;
    private readonly EmbeddingService _embeddingService;
    private readonly HttpClient _httpClient;
    private readonly string _ollamaBaseUrl;
    private const string ChatModel = "llama3.2"; // faster and good quality

    public RagService(
        InMemoryVectorDb vectorDb,
        EmbeddingService embeddingService,
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _vectorDb = vectorDb;
        _embeddingService = embeddingService;
        _httpClient = httpClient;
        _ollamaBaseUrl = configuration["Ollama:BaseUrl"] ?? "http://localhost:11434";
    }

    public async Task<QueryResponse> QueryAsync(string question, int topK = 5)
    {
        if (_vectorDb.Count == 0)
            return new QueryResponse { Answer = "No documents uploaded yet. Please upload a PDF first." };

        // 1. Embed the question
        var questionEmbedding = await _embeddingService.GetEmbeddingAsync(question);

        // 2. Get more chunks for better coverage
        var relevantChunks = _vectorDb.Search(questionEmbedding, topK);

        if (relevantChunks.Count == 0)
            return new QueryResponse { Answer = "Could not find relevant content for your question." };

        // 3. Build context
        var context = BuildContext(relevantChunks);

        // 4. Generate answer
        var answer = await GenerateAnswerAsync(question, context);

        return new QueryResponse
        {
            Answer = answer,
            Sources = new List<SourceChunk>() // sources hidden from UI
        };
    }

    private string BuildContext(List<DocumentChunk> chunks)
    {
        // Deduplicate similar chunks and join
        var uniqueChunks = chunks
            .GroupBy(c => c.Text[..Math.Min(50, c.Text.Length)])
            .Select(g => g.First())
            .ToList();

        var parts = uniqueChunks.Select((c, i) =>
            $"[Page {c.PageNumber}]\n{c.Text}");

        return string.Join("\n\n---\n\n", parts);
    }

    private async Task<string> GenerateAnswerAsync(string question, string context)
    {
        var systemPrompt = """
            You are an intelligent document assistant. Your job is to answer questions accurately based on the document content provided.

            RULES:
            - Answer ONLY from the provided document context
            - Be specific and extract exact values (numbers, dates, names, codes) when present
            - If the exact answer is in the context, provide it directly — do not say "I don't know" if the data exists
            - If the answer is truly not in the context, say: "This information is not available in the uploaded document."
            - Keep answers concise and clear
            - For numerical data, tables, or codes — extract and present them clearly
            - Do NOT make up any information
            """;

        var userMessage = $"""
            Document content:
            {context}

            Question: {question}

            Answer based strictly on the document content above:
            """;

        var request = new
        {
            model = ChatModel,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userMessage }
            },
            stream = false,
            options = new
            {
                temperature = 0.1,  // low temperature = more factual, less creative
                num_predict = 500   // limit response length for speed
            }
        };

        var response = await _httpClient.PostAsJsonAsync(
            $"{_ollamaBaseUrl}/api/chat", request);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty("message").GetProperty("content").GetString()
               ?? "Error generating response.";
    }
}