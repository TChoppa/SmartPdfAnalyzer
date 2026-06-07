namespace PdfRagApi.Models;

public class QueryRequest
{
    public string Question { get; set; } = string.Empty;
    public int TopK { get; set; } = 3; // How many chunks to retrieve
}

public class QueryResponse
{
    public string Answer { get; set; } = string.Empty;
    public List<SourceChunk> Sources { get; set; } = new();
}

public class SourceChunk
{
    public string FileName { get; set; } = string.Empty;
    public int PageNumber { get; set; }
    public string Excerpt { get; set; } = string.Empty;
}

public class UploadResponse
{
    public string FileName { get; set; } = string.Empty;
    public int TotalChunks { get; set; }
    public string Message { get; set; } = string.Empty;
}