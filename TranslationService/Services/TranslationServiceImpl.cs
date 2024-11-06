using Microsoft.Extensions.Caching.Memory;
using System.Net.Http;
using System.Text.Json;
using TranslationService.Interfaces;

namespace TranslationService.Services
{
    public class TranslationServiceImpl : ITranslationService
    {
        private readonly IMemoryCache _cache;
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly string _apiKey;

        public TranslationServiceImpl(IMemoryCache cache, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _cache = cache;
            _httpClientFactory = httpClientFactory;
            _apiKey = configuration["GoogleTranslate:ApiKey"];
        }
        public async Task<string> GetServiceInfoAsync()
        {
            return "Google Translate API with in-memory caching.";
        }

        public async Task<IEnumerable<string>> TranslateAsync(IEnumerable<string> texts, string sourceLang, string targetLang)
        {
            var results = new List<string>();

            foreach (var text in texts)
            {
                if (_cache.TryGetValue(text, out string translation))
                {
                    results.Add(translation);
                }
                else
                {
                    // Call translation API and add to cache
                    var translatedText = await CallTranslateApiAsync(text, sourceLang, targetLang);
                    _cache.Set(text, translatedText, TimeSpan.FromHours(1));
                    results.Add(translatedText);
                }
            }

            return results;
        }



private async Task<string> CallTranslateApiAsync(string text, string sourceLang, string targetLang)
    {
        var client = _httpClientFactory.CreateClient();

     //   string apiKey = "YOUR_GOOGLE_TRANSLATE_API_KEY";

        string url = $"https://translation.googleapis.com/language/translate/v2?key={_apiKey}";
        var requestBody = new
        {
            q = text,
            source = sourceLang,
            target = targetLang,
            format = "text"
        };

        var jsonContent = JsonSerializer.Serialize(requestBody);
        var httpContent = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

        var response = await client.PostAsync(url, httpContent);
        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync();
        using JsonDocument doc = JsonDocument.Parse(jsonResponse);
        string translatedText = doc.RootElement
            .GetProperty("data")
            .GetProperty("translations")[0]
            .GetProperty("translatedText")
            .GetString();

        return translatedText;
    }

}
}
