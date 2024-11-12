using Hangfire;
using Newtonsoft.Json;
using WebEmailSendler.Managers;
using WebEmailSendler.Models;

namespace WebEmailSendler.Services
{
    public class DataService
    {
        public static Dictionary<int, CancellationTokenSource> CancelTokenTasks = new Dictionary<int, CancellationTokenSource>();

        private FileService _fileService;
        private DataManager _dataManager;
        private SendlerService _sendlerService;
        public DataService(FileService fileService, DataManager dataManager, SendlerService sendlerService)
        {
            _fileService = fileService;
            _dataManager = dataManager;
            _sendlerService = sendlerService;
        }

        public async Task<EmailSendInfo> EmailSendTaskInfo(int emailSendTaskId)
        {
            var result = await _dataManager.EmailSendTaskInfo(emailSendTaskId);
            return result;
        }

        public async Task<List<EmailSendTask>> EmailSendTasks(SendTaskStatusEnum status)
        {
            var result = await _dataManager.EmailSendTaskList(status);
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

        public string CreateEmailJob(EmailSendTask emailSendTask)
        {
            CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
            CancelTokenTasks.Add(emailSendTask.Id, cancelTokenSource);
            var jobId = BackgroundJob.Enqueue(() => _sendlerService.SendEmailByTask(emailSendTask.Id, cancelTokenSource.Token));
            emailSendTask.JobId = jobId;
            _dataManager.UpdateEmailSendTask(emailSendTask);
            return jobId;
        }

        public async Task CancelEmailSendTask(int emailSendTaskId)
        {
            var token = CancelTokenTasks.GetValueOrDefault(emailSendTaskId);
            if(token !=null)
            {
                await token.CancelAsync();
            }
            var task = await _dataManager.GetEmailSendTask(emailSendTaskId) ?? throw new Exception("Не удалось найти задание");
            BackgroundJob.Delete(task.JobId);
            task.SendTaskStatus = SendTaskStatusEnum.cancel.ToString();
            _dataManager.UpdateEmailSendTask(task);
            CancelTokenTasks.Remove(emailSendTaskId);
        }

        public async Task DeleteTaskAndData(int emailSendTaskId)
        {
            await _dataManager.DeleteEmailSendTask(emailSendTaskId);
            CancelTokenTasks.Remove(emailSendTaskId);
        }

        public string ReCreateEmailJob(EmailSendTask emailSendTask)
        {
            emailSendTask.SendTaskStatus = SendTaskStatusEnum.created.ToString();
            CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
            CancelTokenTasks.Add(emailSendTask.Id, cancelTokenSource);
            var jobId = BackgroundJob.Enqueue(() => _sendlerService.SendEmailByTask(emailSendTask.Id, cancelTokenSource.Token));
            emailSendTask.JobId = jobId;
            _dataManager.UpdateEmailSendTask(emailSendTask);
            return jobId;
        }

        public async Task<int> CreateEmailDataSendTask(EmailSendTask emailSendTask)
        {
            emailSendTask.SendTaskStatus = SendTaskStatusEnum.created.ToString();
            var resultId = await _dataManager.CreateEmailSendTask(emailSendTask);
            if (emailSendTask.CsvData != null && emailSendTask.HtmlMessage != null)
            {
                emailSendTask.Id = resultId;
                var resultData = _fileService.ReadEmailDataFromCsv(emailSendTask.CsvData);
                await CreateEmailSendResult(resultData, emailSendTask);
            }
            return resultId;
        }
        private async Task CreateEmailSendResult(List<EmailData> emailSendResultList, EmailSendTask emailSendTask)
        {
            var result = new List<EmailSendResult>();
            foreach (var emailData in emailSendResultList)
            {
                try
                {
                    var parameters = new Dictionary<string, string>();
                    var emailResult = new EmailSendResult
                    {
                        Id = 0,
                        EmailSendTaskId = emailSendTask.Id,
                        Email = emailData.Email,
                        IsSuccess = false,
                        ErrorMessage = null,
                        Lschet = emailData.SendParameters.Lschet,
                        Sum = emailData.SendParameters.Sum,
                        Text = emailData.SendParameters.Text,
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

        public Part<EmailSendResult> EmailResultPath(string? inputValue, int sendTaskId, int pageNumber, int pageSize, string? sortField, string? orderBy)
        {
            pageNumber += 1;
            Part<EmailSendResult> result = _dataManager.EmailResultPath(inputValue, sendTaskId, pageNumber, pageSize, sortField, orderBy);
            return result;
        }

        public async Task<object> SampleContext()
        {
            var filePath = "Sample/sample.json";
            if (File.Exists(filePath))
            {
                var jsonData = await File.ReadAllTextAsync("Sample/sample.json") ?? "";
                var jsonDocument = JsonConvert.DeserializeObject<object>(jsonData) ?? "";
                return jsonDocument;
            }
            else
            {
                return "";
            }
        }

        public async Task SaveSample(object sample)
        {
            var filePath = "Sample/sample.json";
            var jsonDocument = JsonConvert.SerializeObject(sample);
            File.Delete(filePath);
            await File.WriteAllTextAsync(filePath, jsonDocument);
        }
    }
}
