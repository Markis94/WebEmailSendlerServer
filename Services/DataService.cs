using WebEmailSendler.Enums;
using WebEmailSendler.Managers;
using WebEmailSendler.Models;

namespace WebEmailSendler.Services
{
    public class DataService(FileService fileService, DataManager dataManager)
    {
        private readonly FileService _fileService = fileService;
        private readonly DataManager _dataManager = dataManager;

        public async Task<SendInfo> EmailSendTaskInfo(int emailSendTaskId)
        {
            var result = await _dataManager.EmailSendTaskInfo(emailSendTaskId);
            return result;
        }
        public async Task<List<EmailSendTask>> EmailSendTasks(SendTaskStatusEnum status, DateTime leftDate, DateTime rightDate)
        {
            var result = await _dataManager.EmailSendTaskList(status, leftDate, rightDate);
            foreach (var task in result)
            {
                if (task.SendTaskStatus == SendTaskStatusEnum.started.ToString())
                {
                    task.EmailSendInfo = await _dataManager.EmailSendTaskInfo(task.Id);
                }
                else
                {
                    task.EmailSendInfo = new SendInfo()
                    {
                        BadSendCount = task.BadSendCount,
                        MaxCount = task.MaxCount,
                        SuccessSendCount = task.SuccessSendCount
                    };
                }
            }
            return result;
        }
        public async Task<List<EmailSendTask>> EmailSendTasks()
        {
            var result = await _dataManager.EmailSendTaskList();
            return result;
        }
        public async Task<EmailSendTask?> EmailSendTasksById(int sendTaskId)
        {
            var result = await _dataManager.GetEmailSendTask(sendTaskId);
            return result;
        }
        public async Task<int> CreateEmailDataSendTask(EmailSendTask emailSendTask)
        {
            emailSendTask.SendTaskStatus = SendTaskStatusEnum.created.ToString();
            var resultId = 0;
            if (emailSendTask.CsvData != null && emailSendTask.HtmlMessage != null)
            {
                var resultData = _fileService.ReadEmailDataFromCsv(emailSendTask.CsvData);
                emailSendTask.MaxCount = resultData.Count;
                resultId = await _dataManager.CreateEmailSendTask(emailSendTask);
                emailSendTask.Id = resultId;
                await CreateEmailSendResult(resultData, emailSendTask);
            }
            return resultId;
        }
        private async Task CreateEmailSendResult(List<EmailCsvData> emailSendResultList, EmailSendTask emailSendTask)
        {
            var result = new List<EmailSendData>();
            foreach (var emailData in emailSendResultList)
            {
                try
                {
                    var emailResult = new EmailSendData
                    {
                        Id = 0,
                        EmailSendTaskId = emailSendTask.Id,
                        Email = emailData.Email,
                        IsSuccess = false,
                        ErrorMessage = null,
                        Lschet = emailData.SendParameters?.Lschet,
                        Sum = emailData.SendParameters?.Sum,
                        Text = emailData.SendParameters?.Text,
                        SendDate = DateTime.UtcNow
                    };
                    result.Add(emailResult);
                }
                catch (Exception ex)
                {
                    throw new Exception("Не удалось сформировать список расслыки", ex);
                }
            }
            await _dataManager.CreateEmailSendResult(result);
        }
        public Part<EmailSendData> EmailResultPath(string? inputValue, int sendTaskId, int pageNumber, int pageSize, string? sortField, string? orderBy)
        {
            pageNumber += 1;
            Part<EmailSendData> result = _dataManager.EmailResultPath(inputValue, sendTaskId, pageNumber, pageSize, sortField, orderBy);
            return result;
        }
    }
}
