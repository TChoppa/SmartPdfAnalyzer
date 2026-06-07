using PdfRagApi.Services;
using PdfRagApi.VectorStore;

var builder = WebApplication.CreateBuilder(args);

// ── Services ────────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "PDF RAG API", Version = "v1" });
});

// Allow large file uploads
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 50 * 1024 * 1024; // 50 MB
});

// CORS - allow VS Code client (localhost:5500) and any local dev client
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5500",
                "http://127.0.0.1:5500",
                "http://localhost:3000",
                "https://localhost:7102",
                "http://localhost:5104",
                "null") // for file:// HTML files opened directly
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// HttpClient for Ollama calls (shared, long-lived)
builder.Services.AddHttpClient<EmbeddingService>();
builder.Services.AddHttpClient<RagService>();

// Register our services
builder.Services.AddSingleton<InMemoryVectorDb>(); // singleton = survives request lifetime
builder.Services.AddScoped<PdfService>();
builder.Services.AddScoped<EmbeddingService>();
builder.Services.AddScoped<RagService>();

// ── App ─────────────────────────────────────────────────────────────────────
var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(); // ✅ Swagger at /swagger/index.html (matches launchSettings)

// Serve index.html and static files from wwwroot
app.UseDefaultFiles();  // serves index.html by default
app.UseStaticFiles();

app.UseCors("DevCors");
app.UseAuthorization();
app.MapControllers();

app.Run();