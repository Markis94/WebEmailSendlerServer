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

            emailSendTask.SendTaskStatus = SendTaskStatusEnum.started.ToString();
            _dataManager.UpdateEmailSendTask(emailSendTask);

            return emailSendTask.JobId;
        }

        public string ReCreateSendJob(EmailSendTask emailSendTask)
        {

            emailSendTask.JobId = CreateBackgroundJob(emailSendTask);

            emailSendTask.StartDate = DateTimeOffset.Now.AddSeconds(10);
            emailSendTask.SendTaskStatus = SendTaskStatusEnum.created.ToString();
            _dataManager.UpdateEmailSendTask(emailSendTask);

            return emailSendTask.JobId;
        }

        public async Task CancelSendJob(int emailSendTaskId)
        {
            var task = await _dataManager.GetEmailSendTask(emailSendTaskId) ?? throw new Exception("Не удалось найти задание");

            await TokenHub.CancelAsync(task.Id);
            BackgroundJob.Delete(task.JobId);

            task.SendTaskStatus = SendTaskStatusEnum.cancel.ToString();
            _dataManager.UpdateEmailSendTask(task);
        }

        public string DeleteSendJob(int emailSendTaskId)
        {
            var jobId = BackgroundJob.Enqueue(() => DeleteSendData(emailSendTaskId));
            return jobId ?? "0";
        }

        public string CreateBackgroundJob(EmailSendTask emailSendTask)
        {
            CancellationToken token = TokenHub.AddToken(emailSendTask.Id);
            var jobId = BackgroundJob.Schedule(() => _sendlerService.SendEmailByTask(emailSendTask.Id, token), emailSendTask?.StartDate ?? DateTimeOffset.Now);
            return jobId;
        }
        public void DeleteSendData(int emailSendTaskId)
        {
            _dataManager.DeleteEmailSendTask(emailSendTaskId);
            TokenHub.CancelTokenTasks.Remove(emailSendTaskId);
        }
    }
}
