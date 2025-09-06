using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace WSafe.Api.OpenAI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatbotController : ControllerBase
    {
        private readonly IHttpClientFactory _httpFactory;
        private static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

        public ChatbotController(IHttpClientFactory httpFactory)
        {
            _httpFactory = httpFactory;
        }

        public class SupportRequest { public string Prompt { get; set; } }

        [HttpPost("support")]
        public async Task<IActionResult> Support([FromBody] SupportRequest req)
        {
            var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            if (string.IsNullOrWhiteSpace(apiKey))
                return StatusCode(503, new { success = false, error = "Service unavailable: OPENAI_API_KEY not set." });

            if (req == null || string.IsNullOrWhiteSpace(req.Prompt))
                return BadRequest(new { success = false, error = "Prompt requerido." });

            var http = _httpFactory.CreateClient("openai");

            // payload para /v1/chat/completions
            var payload = new
            {
                model = "gpt-4o-mini",
                messages = new[]
                {
                    new { role = "system", content = "Eres un asistente útil para WSafe." },
                    new { role = "user", content = req.Prompt }
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(payload, _json), Encoding.UTF8, "application/json");
            var resp = await http.PostAsync("chat/completions", content);
            var json = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
            {
                return StatusCode((int)resp.StatusCode, new { success = false, error = json });
            }

            // Extraer el texto: choices[0].message.content
            try
            {
                using var doc = JsonDocument.Parse(json);
                var text = doc.RootElement
                              .GetProperty("choices")[0]
                              .GetProperty("message")
                              .GetProperty("content")
                              .GetString() ?? "";

                return Ok(new { success = true, response = text });
            }
            catch
            {
                // Por si cambia el formato, devolvemos el json crudo
                return Ok(new { success = true, response = json });
            }
        }
    }
}
