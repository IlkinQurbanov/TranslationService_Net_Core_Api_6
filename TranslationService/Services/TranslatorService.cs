using Grpc.Core;
using TranslationService.Interfaces;

namespace TranslationService.Services
{
    public class TranslatorService : Translator.TranslatorBase
    {
        private readonly ITranslationService _translationService;

        public TranslatorService(ITranslationService translationService)
        {
            _translationService = translationService;
        }

        public override async Task<ServiceInfoResponse> GetServiceInfo(Empty request, ServerCallContext context)
        {
            var info = await _translationService.GetServiceInfoAsync();
            return new ServiceInfoResponse { Info = info };
        }

        public override async Task<TranslateResponse> Translate(TranslateRequest request, ServerCallContext context)
        {
            var translations = await _translationService.TranslateAsync(request.Texts, request.SourceLang, request.TargetLang);
            return new TranslateResponse { Translations = { translations } };
        }
    }

}
