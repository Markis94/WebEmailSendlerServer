using Hangfire;
using WebEmailSendler.Enums;
using WebEmailSendler.Managers;
using WebEmailSendler.Models;

namespace WebEmailSendler.Services
{
    public class JobService(SendlerService sendlerService, DataManager dataManager)
    {
        private readonly SendlerService _sendlerService = sendlerService;
        private readonly DataManager _dataManager = dataManager;
        public string CreateSendJob(EmailSendTask emailSendTask)
        {
            emailSendTask.JobId = CreateBackgroundJob(emailSendTask);
            _dataManager.UpdateEmailSendTask(emailSendTask);
            return emailSendTask.JobId;
        }

        public string ReCreateSendJob(EmailSendTask emailSendTask)
        {
            //emailSendTask.StartDate = DateTimeOffset.Now;
            emailSendTask.SendTaskStatus = SendTaskStatusEnum.created.ToString();

            emailSendTask.JobId = CreateBackgroundJob(emailSendTask);
            _dataManager.UpdateEmailSendTask(emailSendTask);

            return emailSendTask.JobId;
        }

        public async Task CancelSendJob(int emailSendTaskId)
        {
            await ConfigurationService.CancelAsync(emailSendTaskId);

            //var task = await _dataManager.GetEmailSendTask(emailSendTaskId) ?? throw new Exception("Не удалось найти задание");
            //BackgroundJob.Delete(task.JobId);

            //task.SendTaskStatus = SendTaskStatusEnum.cancel.ToString();
            //_dataManager.UpdateEmailSendTask(task);
        }

        public string DeleteSendJob(int emailSendTaskId)
        {
            var jobId = BackgroundJob.Enqueue(() => DeleteSendData(emailSendTaskId));
            return jobId ?? "0";
        }

        public string CreateBackgroundJob(EmailSendTask emailSendTask)
        {
            var token = ConfigurationService.AddToken(emailSendTask.Id);
            var jobId = BackgroundJob.Enqueue(() => _sendlerService.SendEmailByTask(emailSendTask.Id, token));
            return jobId;
        }
        public void DeleteSendData(int emailSendTaskId)
        {
            _dataManager.DeleteEmailSendTask(emailSendTaskId);
            ConfigurationService.CancelTokenTasks.Remove(emailSendTaskId);
        }
    }
}
