using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace TaglineAIWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaglineController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public TaglineController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public class TaglineRequest
        {
            public string? prompt { get; set; }
        }

        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromBody] TaglineRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.prompt))
                return BadRequest(new { result = "Error: Please enter a business description." });

            try
            {
                var client = _httpClientFactory.CreateClient("ollama");

                // Ollama phi model expects `prompt` field
                var payload = new
                {
                    model = "phi",
                    prompt = $"Create a catchy business tagline for: {request.prompt}"
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("api/generate", content);
                response.EnsureSuccessStatusCode();

                var responseText = await response.Content.ReadAsStringAsync();

                // Sometimes Ollama returns JSON like { "output": "..."} - parse it safely
                string result;
                try
                {
                    using var doc = JsonDocument.Parse(responseText);
                    result = doc.RootElement.GetProperty("output").GetString() ?? "No tagline generated.";
                }
                catch
                {
                    result = responseText; // fallback
                }

                return Ok(new { result });
            }
            catch (Exception ex)
            {
                return Ok(new { result = "Error: Cannot connect to Ollama. Make sure phi is running.\n" + ex.Message });
            }
        }
    }
}