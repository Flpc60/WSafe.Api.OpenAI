using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ====== 1) SERVICES ======

// Controllers
builder.Services.AddControllers();

// CORS (ajusta los orígenes a tu front real)
const string CorsPolicy = "WSafeCors";
builder.Services.AddCors(o => o.AddPolicy(CorsPolicy, p =>
{
    p.WithOrigins(
        "http://localhost:8080",   // si tu MVC 5 corre en HTTP
        "https://localhost:44300"  // típico IIS Express MVC 5 en HTTPS
                                   // "https://tu-dominio"    // prod (cuando lo tengas)
    )
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials(); // si usas auth basada en cookies/session
}));

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "WSafe.Api.OpenAI",
        Version = "v1",
        Description = "Endpoints de soporte/LLM"
    });
});

var app = builder.Build();

// ====== 2) MIDDLEWARE PIPELINE ======

// Swagger (útil en dev y también puedes dejarlo en prod si lo requieres)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "WSafe.Api.OpenAI v1");
});

// CORS antes de auth/routing
app.UseCors(CorsPolicy);

// HTTPS redirection:
// - Para pruebas con curl/HTTP puro, puedes comentar esta línea.
// - En entornos formales, déjala activa.
app.UseHttpsRedirection();

app.UseAuthorization();

// Preflight OPTIONS (algunos proxies requieren respuesta explícita)
app.MapMethods("{*path}", new[] { "OPTIONS" }, (HttpContext ctx) =>
{
    // Devolver encabezados típicos de CORS
    var origin = ctx.Request.Headers["Origin"].ToString();
    if (!string.IsNullOrEmpty(origin))
    {
        ctx.Response.Headers["Access-Control-Allow-Origin"] = origin;
        ctx.Response.Headers["Vary"] = "Origin";
    }
    ctx.Response.Headers["Access-Control-Allow-Credentials"] = "true";
    ctx.Response.Headers["Access-Control-Allow-Headers"] = "Content-Type, Authorization";
    ctx.Response.Headers["Access-Control-Allow-Methods"] = "GET,POST,PUT,PATCH,DELETE,OPTIONS";
    return Results.Ok();
});

app.MapControllers();

app.Run();
