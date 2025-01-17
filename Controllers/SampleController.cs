using Microsoft.AspNetCore.Mvc;
using WebEmailSendler.Models;
using WebEmailSendler.Services;

namespace WebEmailSendler.Controllers
{
    [Route("api")]
    [ApiController]
    public class SampleController(SampleService sampleService) : ControllerBase
    {
        private readonly SampleService _sampleService = sampleService;

        [HttpPost("createSample")]
        public async Task<Sample> CreateSample(Sample sample)
        {
            return await _sampleService.CreateSample(sample);
        }

        [HttpPost("createCopySample")]
        public async Task<Sample> CreateCopySample(Sample sample)
        {
            return await _sampleService.CreateCopySample(sample);
        }

        [HttpGet("samples")]
        public async Task<IList<Sample>> GetSamples()
        {
            return await _sampleService.SampleList();
        }

        [HttpGet("sampleById")]
        public async Task<Sample> GetSampleById(int sampleId)
        {
            return await _sampleService.SampleById(sampleId);
        }

        [HttpPut("updateSample")]
        public async Task<IActionResult> UpdateSample(Sample sample)
        {
            await _sampleService.UpdateSample(sample);
            return Ok();
        }

        [HttpDelete("deleteSample")]
        public async Task<IActionResult> DeleteSample(int sampleId)
        {
            await _sampleService.DeleteSample(sampleId);
            return Ok();
        }
    }
}
