namespace TranslationService.DTOs
{
    public class TranslateRequestDto
    {
        public List<string> Texts { get; set; }
        public string SourceLang { get; set; }
        public string TargetLang { get; set; }
    }
}
