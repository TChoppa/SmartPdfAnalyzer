using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;
using PdfRagApi.Models;

namespace PdfRagApi.Services;

public class PdfService
{
    private const int ChunkSize = 300;    // smaller chunks = more precise retrieval
    private const int ChunkOverlap = 80; // overlap keeps context between chunks

    public List<DocumentChunk> ExtractChunks(Stream pdfStream, string fileName)
    {
        var allChunks = new List<DocumentChunk>();

        using var pdf = PdfDocument.Open(pdfStream);

        foreach (var page in pdf.GetPages())
        {
            // Use ContentOrderTextExtractor for better table/layout handling
            var pageText = ContentOrderTextExtractor.GetText(page);

            if (string.IsNullOrWhiteSpace(pageText)) continue;

            pageText = CleanText(pageText);

            var chunks = SplitIntoChunks(pageText, page.Number, fileName);
            allChunks.AddRange(chunks);
        }

        return allChunks;
    }

    private List<DocumentChunk> SplitIntoChunks(string text, int pageNumber, string fileName)
    {
        var chunks = new List<DocumentChunk>();

        // Split by sentences/lines first for more natural chunks
        var sentences = SplitBySentences(text);
        var current = new System.Text.StringBuilder();
        int chunkIndex = 0;

        foreach (var sentence in sentences)
        {
            if (current.Length + sentence.Length > ChunkSize && current.Length > 0)
            {
                var chunkText = current.ToString().Trim();
                if (!string.IsNullOrWhiteSpace(chunkText))
                {
                    chunks.Add(new DocumentChunk
                    {
                        FileName = fileName,
                        PageNumber = pageNumber,
                        Text = chunkText,
                        ChunkIndex = chunkIndex++
                    });
                }

                // Keep last part for overlap
                var words = current.ToString().Split(' ');
                var overlapWords = words.TakeLast(20).ToArray();
                current.Clear();
                current.Append(string.Join(" ", overlapWords));
                current.Append(" ");
            }

            current.Append(sentence);
            current.Append(" ");
        }

        // Add remaining text
        if (current.Length > 0)
        {
            var remaining = current.ToString().Trim();
            if (!string.IsNullOrWhiteSpace(remaining))
            {
                chunks.Add(new DocumentChunk
                {
                    FileName = fileName,
                    PageNumber = pageNumber,
                    Text = remaining,
                    ChunkIndex = chunkIndex
                });
            }
        }

        return chunks;
    }

    private List<string> SplitBySentences(string text)
    {
        // Split on sentence endings and newlines
        var parts = System.Text.RegularExpressions.Regex
            .Split(text, @"(?<=[.!?])\s+|\n+")
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToList();

        return parts;
    }

    private static string CleanText(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;

        // Remove excessive whitespace but keep newlines for structure
        text = System.Text.RegularExpressions.Regex.Replace(text, @"[ \t]+", " ");
        text = System.Text.RegularExpressions.Regex.Replace(text, @"\n{3,}", "\n\n");
        text = text.Trim();

        return text;
    }
}