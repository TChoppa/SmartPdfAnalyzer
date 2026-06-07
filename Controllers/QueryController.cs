using Microsoft.AspNetCore.Mvc;
using PdfRagApi.Models;
using PdfRagApi.Services;

namespace PdfRagApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QueryController : ControllerBase
{
    private readonly RagService _ragService;
    private readonly ILogger<QueryController> _logger;

    public QueryController(RagService ragService, ILogger<QueryController> logger)
    {
        _ragService = ragService;
        _logger = logger;
    }

    /// <summary>
    /// Ask a question about the uploaded PDF documents.
    /// </summary>
    [HttpPost("ask")]
    public async Task<ActionResult<QueryResponse>> Ask([FromBody] QueryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
            return BadRequest("Question cannot be empty.");

        try
        {
            _logger.LogInformation("Processing question: {Question}", request.Question);
            var response = await _ragService.QueryAsync(request.Question, request.TopK);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing question");
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }
}