using Microsoft.AspNetCore.Mvc;
using TranslationService.DTOs;
using TranslationService.Interfaces;

namespace TranslationService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TranslationController : ControllerBase
    {
        private readonly ITranslationService _translationService;

        public TranslationController(ITranslationService translationService)
        {
            _translationService = translationService;
        }

        [HttpGet("info")]
        public async Task<IActionResult> GetServiceInfo()
        {
            var info = await _translationService.GetServiceInfoAsync();
            return Ok(info);
        }

        [HttpPost("translate")]
        public async Task<IActionResult> Translate([FromBody] TranslateRequestDto request)
        {
            var translations = await _translationService.TranslateAsync(request.Texts, request.SourceLang, request.TargetLang);
            return Ok(translations);
        }
    }

}
