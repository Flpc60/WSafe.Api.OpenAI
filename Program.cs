using ChatGPT.Net;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      builder =>
                      {
                          builder.WithOrigins("https://localhost:44315")
                                 .AllowAnyHeader()
                                 .AllowAnyMethod();
                      });
});

// Forzar lectura del archivo appsettings ignorando env vars (si es necesario)
//builder.Configuration.Sources.Clear();
//builder.Configuration
//    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

var apiKey = builder.Configuration["OpenAI:ApiKey"];

builder.Services.AddSingleton(new ChatGpt(apiKey));

// Agrega servicios MVC y Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "WSafe Chatbot API",
        Version = "v1"
    });
});

var app = builder.Build();

app.UseCors(MyAllowSpecificOrigins);

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
