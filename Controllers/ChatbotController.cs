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
                var prompt = "";
                var response = await _chatGpt.Ask($"Actúa como un asistente técnico amable y conciso para WSafe. {request.Prompt}");

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
