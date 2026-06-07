using PdfRagApi.Models;

namespace PdfRagApi.VectorStore;

/// <summary>
/// Simple in-memory vector store using cosine similarity search.
/// Resets when the application restarts. Good for prototyping.
/// </summary>
public class InMemoryVectorDb
{
    private readonly List<DocumentChunk> _chunks = new();
    private readonly object _lock = new();

    public void AddChunks(IEnumerable<DocumentChunk> chunks)
    {
        lock (_lock)
        {
            _chunks.AddRange(chunks);
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            _chunks.Clear();
        }
    }

    public int Count => _chunks.Count;

    public List<string> GetLoadedFiles()
    {
        lock (_lock)
        {
            return _chunks.Select(c => c.FileName).Distinct().ToList();
        }
    }

    public List<DocumentChunk> GetSampleChunks(int count)
    {
        lock (_lock)
        {
            return _chunks.Take(count).ToList();
        }
    }

    /// <summary>
    /// Returns the top-K most similar chunks using cosine similarity.
    /// </summary>
    public List<DocumentChunk> Search(float[] queryEmbedding, int topK = 3)
    {
        lock (_lock)
        {
            if (_chunks.Count == 0)
                return new List<DocumentChunk>();

            return _chunks
                .Select(chunk => new
                {
                    Chunk = chunk,
                    Score = CosineSimilarity(queryEmbedding, chunk.Embedding)
                })
                .OrderByDescending(x => x.Score)
                .Take(topK)
                .Select(x => x.Chunk)
                .ToList();
        }
    }

    private static float CosineSimilarity(float[] a, float[] b)
    {
        if (a.Length != b.Length) return 0f;

        float dot = 0f, normA = 0f, normB = 0f;
        for (int i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
            normA += a[i] * a[i];
            normB += b[i] * b[i];
        }

        if (normA == 0 || normB == 0) return 0f;
        return dot / (MathF.Sqrt(normA) * MathF.Sqrt(normB));
    }
}