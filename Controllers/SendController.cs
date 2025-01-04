using Microsoft.AspNetCore.Mvc;
using WebEmailSendler.Enums;
using WebEmailSendler.Models;
using WebEmailSendler.Services;

namespace WebEmailSendler.Controllers
{
    [Route("api")]
    [ApiController]
    public class SendController(DataService dataService, SendlerService sendlerService) : ControllerBase
    {
        private readonly DataService _dataService = dataService;
        private readonly SendlerService _sendlerService = sendlerService;

        [HttpGet("sendTaskInfo")]
        public async Task<SendInfo> GetEmailDataSendTask([FromQuery(Name = "sendTaskId")] int sendTaskId)
        {
            var result = await _dataService.EmailSendTaskInfo(sendTaskId);
            return result;
        }

        [HttpGet("sendTaskByStatus")]
        public async Task<List<EmailSendTask>> GetEmailDataSendTask([FromQuery(Name = "taskStatus")] SendTaskStatusEnum status,
            DateTime leftDate, 
            DateTime rightDate)
        {
            var result = await _dataService.EmailSendTasks(status, leftDate, rightDate);
            return result;
        }

        [HttpGet("sendTaskById")]
        public async Task<EmailSendTask?> GetEmailSendTaskById(int sendTaskId)
        {
            var result = await _dataService.EmailSendTasksById(sendTaskId);
            return result;
        }

        [HttpGet("sendResultPath")]
        public Part<EmailSendData> GetEmailResultPath(int sendTaskId, int pageNumber, int pageSize, string? sortField = null, string? orderBy = null, string? inputValue = null)
        {
            var result = _dataService.EmailResultPath(inputValue, sendTaskId, pageNumber, pageSize, sortField, orderBy);
            return result;
        }

        [HttpGet("sendTasks")]
        public async Task<List<EmailSendTask>> GetEmailDataSendTask()
        {
            var result = await _dataService.EmailSendTasks();
            return result;
        }

        [HttpPost("createEmailDataSend")]
        public async Task<int> CreateEmailDataSendTask([FromBody] EmailSendTask emailSendTask)
        {
            var id = await _dataService.CreateEmailDataSendTask(emailSendTask);
            return id;
        }

        [HttpPost("sendTestMessage")]
        public async Task<IActionResult> SendTestMessage(TestSend testSend)
        {
            await _sendlerService.SendTestMessage(testSend);
            return Ok();
        }
    }
}
