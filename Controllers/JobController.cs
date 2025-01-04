using Microsoft.AspNetCore.Mvc;
using WebEmailSendler.Models;
using WebEmailSendler.Services;

namespace WebEmailSendler.Controllers
{
    [Route("api")]
    [ApiController]
    public class JobController(JobService jobService) : ControllerBase
    {
        private readonly JobService _jobService = jobService;

        [HttpPost("createSendJob")]
        public string CreateSendJob([FromBody] EmailSendTask emailSendTask)
        {
            var jobId = _jobService.CreateSendJob(emailSendTask);
            return jobId;
        }

        [HttpPost("reCreateSendJob")]
        public string ReCreateSendJob([FromBody] EmailSendTask emailSendTask)
        {
            var jobId = _jobService.ReCreateSendJob(emailSendTask);
            return jobId;
        }

        [HttpPost("cancelSendJob")]
        public async Task<IActionResult> CancelSendJob([FromQuery] int emailSendTaskId)
        {
            await _jobService.CancelSendJob(emailSendTaskId);
            return Ok();
        }

        [HttpDelete("deleteSendJob")]
        public IActionResult DeleteSendJob([FromQuery] int emailSendTaskId)
        {
            _jobService.DeleteSendJob(emailSendTaskId);
            return Ok();
        }
    }
}
