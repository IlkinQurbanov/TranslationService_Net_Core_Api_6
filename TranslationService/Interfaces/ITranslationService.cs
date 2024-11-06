namespace TranslationService.Interfaces
{
    public interface ITranslationService
    {
        Task<string> GetServiceInfoAsync();
        Task<IEnumerable<string>> TranslateAsync(IEnumerable<string> texts, string sourceLang, string targetLang);
    }

}
