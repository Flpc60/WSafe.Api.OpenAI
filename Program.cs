using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ===== Services =====
builder.Services.AddControllers();

// CORS abierto para pruebas (ajusta orígenes luego)
const string CorsPolicy = "WSafeCors";
builder.Services.AddCors(o => o.AddPolicy(CorsPolicy, p =>
    p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()
));

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "WSafe.Api.OpenAI", Version = "v1" });
});

// OpenAI API Key desde variable de entorno
var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
if (string.IsNullOrWhiteSpace(apiKey))
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("⚠ OPENAI_API_KEY no configurada. /api/chatbot/support responderá 503.");
    Console.ResetColor();
}

// HttpClient tipado para OpenAI
builder.Services.AddHttpClient("openai", client =>
{
    client.BaseAddress = new Uri("https://api.openai.com/v1/");
    if (!string.IsNullOrWhiteSpace(apiKey))
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
});

// ===== Pipeline =====
var app = builder.Build();

// Importante: en HTTP no redirijas a HTTPS
// app.UseHttpsRedirection();

app.UseCors(CorsPolicy);

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "WSafe.Api.OpenAI v1");
    // c.RoutePrefix = string.Empty; // Descomenta si quieres Swagger en "/"
});

app.MapControllers();
app.Run();
