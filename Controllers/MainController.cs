﻿using Hangfire;
using Microsoft.AspNetCore.Mvc;
using WebEmailSendler.Models;
using WebEmailSendler.Services;

namespace WebEmailSendler.Controllers
{
    [Route("api")]
    [ApiController]
    public class MainController : ControllerBase
    {
        private readonly FileService _fileService;
        private readonly DataService _dataService;
        private readonly SendlerService sendlerService;
        public MainController(FileService fileService, DataService dataService, SendlerService sendlerService)
        {
            _fileService = fileService;
            _dataService = dataService;
            this.sendlerService = sendlerService;
        }

        [HttpGet("getEmailSendTaskInfo")]
        public async Task<EmailSendInfo> GetEmailDataSendTask([FromQuery(Name = "sendTaskId")] int sendTaskId)
        {
            var result = await _dataService.EmailSendTaskInfo(sendTaskId);
            return result;
        }

        [HttpGet("getEmailSendTaskByStatus")]
        public async Task<List<EmailSendTask>> GetEmailDataSendTask([FromQuery(Name = "taskStatus")] SendTaskStatusEnum status)
        {
            var result = await _dataService.EmailSendTasks(status);
            return result;
        }

        [HttpGet("getEmailResultPath")]
        public Part<EmailSendResult> GetEmailResultPath(int sendTaskId, int pageNumber, int pageSize, string? sortField = null, string? orderBy = null, string? inputValue = null)
        {
            var result = _dataService.EmailResultPath(inputValue, sendTaskId, pageNumber, pageSize, sortField, orderBy);
            return result;
        }

        [HttpGet("getEmailSendTaskById")]
        public async Task<EmailSendTask?> GetEmailSendTaskById(int sendTaskId)
        {
            var result = await _dataService.EmailSendTasksById(sendTaskId);
            return result;
        }

        [HttpGet("getEmailSendTask")]
        public async Task<List<EmailSendTask>> GetEmailDataSendTask()
        {
            var result = await _dataService.EmailSendTasks();
            return result;
        }

        [HttpPost("createEmailDataSendTask")]
        public async Task<int> CreateEmailDataSendTask([FromBody] EmailSendTask emailSendTask)
        {
            var id = await _dataService.CreateEmailDataSendTask(emailSendTask);
            return id;
        }

        [HttpPost("createEmailJob")]
        public string CreateEmailJob([FromBody] EmailSendTask emailSendTask)
        {
            var jobId = _dataService.CreateEmailJob(emailSendTask);
            return jobId;
        }

        [HttpPost("reCreateEmailJob")]
        public string ReCreateEmailJob([FromBody] EmailSendTask emailSendTask)
        {
            var jobId = _dataService.ReCreateEmailJob(emailSendTask);
            return jobId;
        }

        [HttpPost("cancelEmailJob")]
        public async Task<IActionResult> CancelEmailJobAsync([FromQuery] int emailSendTaskId)
        {
            await _dataService.CancelEmailSendTask(emailSendTaskId);
            return Ok();
        }

        [HttpPost("deleteTaskAndData")]
        public IActionResult DeleteTaskAndData([FromQuery] int emailSendTaskId)
        {
            var jobId = BackgroundJob.Enqueue(() => _dataService.DeleteTaskAndData(emailSendTaskId));
            return Ok();
        }

        [HttpPost("saveSample")]
        public async Task<IActionResult> SaveSampleAsync(object sample)
        {
            await _dataService.SaveSample(sample);
            return Ok();
        }

        [HttpGet("getSample")]
        public Task<object> GetSample()
        {
            var sample = _dataService.SampleContext();
            return sample;
        }
    }
}
