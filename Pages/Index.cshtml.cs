using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace TaglineAIWeb.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public List<string>? Generated { get; set; }
        public string? Error { get; set; }

        public class InputModel
        {
            public string? Description { get; set; }
            public int Count { get; set; } = 1;
        }

        public async Task<IActionResult> OnPostGenerateAsync()
        {
            if (string.IsNullOrWhiteSpace(Input.Description))
            {
                Error = "Please enter a business description.";
                return Page();
            }

            try
            {
                var client = _httpClientFactory.CreateClient("ollama");
                var taglines = new List<string>();

                for (int i = 0; i < Input.Count; i++)
                {
                    var payload = new
                    {
                        model = "phi",
                        prompt = $"Create a catchy business tagline for: {Input.Description}"
                    };

                    var content = new StringContent(
                        JsonSerializer.Serialize(payload),
                        Encoding.UTF8,
                        "application/json"
                    );

                    var response = await client.PostAsync("api/generate", content);
                    response.EnsureSuccessStatusCode();

                    var responseText = await response.Content.ReadAsStringAsync();

                    // Parse streaming JSON and combine "response" fields
                    var taglineBuilder = new StringBuilder();
                    var lines = responseText.Split("\n", StringSplitOptions.RemoveEmptyEntries);

                    foreach (var line in lines)
                    {
                        try
                        {
                            using var doc = JsonDocument.Parse(line);
                            if (doc.RootElement.TryGetProperty("response", out var resp))
                            {
                                var textPart = resp.GetString();
                                if (!string.IsNullOrWhiteSpace(textPart))
                                    taglineBuilder.Append(textPart);
                            }
                        }
                        catch
                        {
                            // Ignore parse errors
                        }
                    }

                    var finalTagline = taglineBuilder.ToString().Trim();
                    taglines.Add(string.IsNullOrWhiteSpace(finalTagline) ? "No tagline generated." : finalTagline);
                }

                Generated = taglines;
                return Page();
            }
            catch (Exception ex)
            {
                Error = $"Cannot connect to Ollama: {ex.Message}";
                return Page();
            }
        }
    }
}