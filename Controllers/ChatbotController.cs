using ChatGPT.Net;
using Microsoft.AspNetCore.Mvc;
using WSafe.Api.OpenAI.Models;

namespace WSafe.Api.OpenAI.Controllers
{
    // Agente encargado de atender el soporte técnico
    [ApiController]
    [Route("api/[controller]")]
    public class ChatbotController : ControllerBase
    {
        private readonly ChatGpt _chatGpt;

        public ChatbotController(ChatGpt chatGpt)
        {
            _chatGpt = chatGpt;
        }

        [HttpPost("support")]
        public async Task<IActionResult> SupportAsync([FromBody] ChatbotRequest request)
        {
            try
            {
                var prompt = @"Actúas como un profesional senior en Seguridad y Salud en el Trabajo (SST), con especialización en inteligencia artificial y amplia experiencia implementando el SG-SST en todos los sectores económicos, sin importar el tamaño de la empresa. Conoces en profundidad la plataforma wsafeapp.com y su funcionalidad. Responde de manera clara, concisa y profesional a cada consulta técnica relacionada con el SG-SST Y sobre el uso y operación la plataforma WSafeApp. Responde solo consultas relacionadas con elSG-SST y la plataforma WSafeApp";

                var fullPrompt = $"{prompt}\n\nPregunta del usuario: {request.Prompt}";

                var response = await _chatGpt.Ask(fullPrompt);

                return Ok(new ChatbotResponse
                {
                    Response = response,
                    Success = true
                });
            }
            catch (HttpRequestException ex) when (ex.Message.Contains("TooManyRequests"))
            {
                return StatusCode(429, new ChatbotResponse
                {
                    Success = false,
                    ErrorMessage = "Has superado la cuota de uso de la API de OpenAI. Verifica tu plan y método de pago."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ChatbotResponse
                {
                    Success = false,
                    ErrorMessage = $"Error inesperado: {ex.Message}"
                });
            }
        }
    }
}
