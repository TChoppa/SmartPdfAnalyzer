using Microsoft.AspNetCore.Mvc;
using PdfRagApi.Models;
using PdfRagApi.Services;
using PdfRagApi.VectorStore;

namespace PdfRagApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PdfController : ControllerBase
{
    private readonly PdfService _pdfService;
    private readonly EmbeddingService _embeddingService;
    private readonly InMemoryVectorDb _vectorDb;
    private readonly ILogger<PdfController> _logger;

    public PdfController(
        PdfService pdfService,
        EmbeddingService embeddingService,
        InMemoryVectorDb vectorDb,
        ILogger<PdfController> logger)
    {
        _pdfService = pdfService;
        _embeddingService = embeddingService;
        _vectorDb = vectorDb;
        _logger = logger;
    }

    /// <summary>
    /// Upload a PDF file to be indexed for Q&A.
    /// </summary>
    [HttpPost("upload")]
    [RequestSizeLimit(50 * 1024 * 1024)] // 50 MB limit
    public async Task<ActionResult<UploadResponse>> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file provided.");

        if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Only PDF files are supported.");

        try
        {
            _logger.LogInformation("Processing PDF: {FileName}", file.FileName);

            // 1. Extract chunks from PDF
            using var stream = file.OpenReadStream();
            var chunks = _pdfService.ExtractChunks(stream, file.FileName);

            if (chunks.Count == 0)
                return BadRequest("Could not extract any text from the PDF. Is it a scanned image-only PDF?");

            _logger.LogInformation("Extracted {Count} chunks from {FileName}", chunks.Count, file.FileName);

            // 2. Generate embeddings for each chunk
            var texts = chunks.Select(c => c.Text).ToList();
            var embeddings = await _embeddingService.GetEmbeddingsBatchAsync(texts);

            for (int i = 0; i < chunks.Count; i++)
                chunks[i].Embedding = embeddings[i];

            // 3. Store in vector DB
            _vectorDb.AddChunks(chunks);

            _logger.LogInformation("Indexed {Count} chunks successfully", chunks.Count);

            return Ok(new UploadResponse
            {
                FileName = file.FileName,
                TotalChunks = chunks.Count,
                Message = $"Successfully indexed {chunks.Count} chunks from '{file.FileName}'."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing PDF {FileName}", file.FileName);
            return StatusCode(500, $"Error processing PDF: {ex.Message}");
        }
    }

    /// <summary>
    /// List all files currently loaded in the vector store.
    /// </summary>
    [HttpGet("files")]
    public ActionResult<List<string>> GetLoadedFiles()
    {
        return Ok(_vectorDb.GetLoadedFiles());
    }

    /// <summary>
    /// Clear all documents from the vector store.
    /// </summary>
    [HttpDelete("clear")]
    public ActionResult Clear()
    {
        _vectorDb.Clear();
        return Ok("Vector store cleared.");
    }

    /// <summary>
    /// Debug: check chunks stored and whether embeddings were generated.
    /// </summary>
    [HttpGet("debug")]
    public ActionResult Debug()
    {   
        return Ok(new
        {
            TotalChunks = _vectorDb.Count,
            LoadedFiles = _vectorDb.GetLoadedFiles(),
            SampleChunks = _vectorDb.GetSampleChunks(3).Select(c => new
            {
                c.FileName,
                c.PageNumber,
                TextPreview = c.Text[..Math.Min(100, c.Text.Length)],
                EmbeddingLength = c.Embedding.Length,
                HasEmbedding = c.Embedding.Length > 0
            })
        });
    }
}