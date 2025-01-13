using Microsoft.AspNetCore.Mvc;
using WebEmailSendler.Models;
using WebEmailSendler.Services;

namespace WebEmailSendler.Controllers
{
    [Route("api")]
    [ApiController]
    public class ConfigurationController(ConfigurationService configurationService) : ControllerBase
    {
        private readonly ConfigurationService _configurationService = configurationService;

        [HttpGet("configuration")]
        public AppConfiguration GetConfiguration()
        {
            return _configurationService.GetConfiguration();
        }

        [HttpPut("updateConfiguration")]
        public IActionResult UpdateConfiguration(AppConfiguration configuration)
        {
            _configurationService.UpdateConfiguration(configuration);
            return Ok();
        }
    }
}
