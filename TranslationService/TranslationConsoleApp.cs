using TranslationService.Interfaces;

namespace TranslationService
{
    public class TranslationConsoleApp
    {
        private readonly ITranslationService _translationService;

        public TranslationConsoleApp(ITranslationService translationService)
        {
            _translationService = translationService;
        }

        public async Task RunAsync()
        {
            Console.WriteLine("Введите текст для перевода:");
            var text = Console.ReadLine();
            var translations = await _translationService.TranslateAsync(new List<string> { text }, "en", "ru");
            Console.WriteLine($"Перевод: {translations.FirstOrDefault()}");
        }
    }

}
