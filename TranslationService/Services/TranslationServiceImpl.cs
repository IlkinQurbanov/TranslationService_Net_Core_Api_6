using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using System.Net.Http;
using System.Text.Json;
using TranslationService.Interfaces;

namespace TranslationService.Services
{
    public class TranslationServiceImpl : ITranslationService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<TranslationServiceImpl> _logger;
        private readonly string _apiKey;
        private readonly IDistributedCache _cache;

        public TranslationServiceImpl(IDistributedCache cache, IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<TranslationServiceImpl> logger)
        {
            _cache = cache;
            _httpClientFactory = httpClientFactory;
            _apiKey = configuration["GoogleTranslate:ApiKey"];
            _logger = logger;
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
                var cacheKey = $"{sourceLang}:{targetLang}:{text}";
                var cachedTranslation = await _cache.GetStringAsync(cacheKey);

                if (cachedTranslation != null)
                {
                    results.Add(cachedTranslation);
                }
                else
                {
                    var translatedText = await CallTranslateApiAsync(text, sourceLang, targetLang);
                    await _cache.SetStringAsync(cacheKey, translatedText, new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
                    });
                    results.Add(translatedText);
                }
            }

            return results;
        }


        private async Task<string> CallTranslateApiAsync(string text, string sourceLang, string targetLang)
        {
            var client = _httpClientFactory.CreateClient();
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

            try
            {
                _logger.LogInformation("Sending translation request for text: {Text}, sourceLang: {SourceLang}, targetLang: {TargetLang}", text, sourceLang, targetLang);
                var response = await client.PostAsync(url, httpContent);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Translation request failed with status code {StatusCode}. Response: {ErrorContent}", response.StatusCode, errorContent);
                    throw new HttpRequestException($"Translation request failed with status code {response.StatusCode}: {errorContent}");
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                using JsonDocument doc = JsonDocument.Parse(jsonResponse);
                string translatedText = doc.RootElement
                    .GetProperty("data")
                    .GetProperty("translations")[0]
                    .GetProperty("translatedText")
                    .GetString();

                _logger.LogInformation("Successfully received translation for text: {Text}", text);
                return translatedText;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while translating text: {Text}", text);
                throw;
            }
        }

    }
}
