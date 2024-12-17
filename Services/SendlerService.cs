
using Microsoft.Extensions.Options;
using Serilog;
using System.Net;
using System.Net.Mail;
using WebEmailSendler.Managers;
using WebEmailSendler.Models;

namespace WebEmailSendler.Services
{
    public class SendlerService
    {
        private readonly int TREAD_COUNT;
        private readonly int PACK_SIZE;
        private readonly FileService _fileService;
        private readonly DataManager _dataManager;
        private readonly SmtpConfiguration _smtpConfiguration;

        public SendlerService(FileService fileService, DataManager dataManager, IConfiguration configuration, IOptions<SmtpConfiguration> smtpConfiguration)
        {
            _fileService = fileService;
            _dataManager = dataManager;
            _smtpConfiguration = smtpConfiguration.Value;
            TREAD_COUNT = Convert.ToInt32(configuration.GetSection("TREAD_COUNT").Value);
            PACK_SIZE = Convert.ToInt32(configuration.GetSection("PACK_SIZE").Value);
        }

        public async Task SendEmailByTask(int emailTaskId, CancellationToken token)
        {
            var emailList = await _dataManager.GetEmailSendResult(emailTaskId);
            var emailSendTask = await _dataManager.GetEmailSendTask(emailTaskId);
            emailSendTask!.StartDate = DateTime.UtcNow;
            emailSendTask!.SendTaskStatus = SendTaskStatusEnum.started.ToString();
            _dataManager.UpdateEmailSendTask(emailSendTask);

            var emailPacks = emailList.Chunk(PACK_SIZE);
            var options = new ParallelOptions()
            {
                MaxDegreeOfParallelism = TREAD_COUNT,
                CancellationToken = token
            };
            Log.Information($"Start Send - {DateTime.UtcNow} - {emailSendTask.Name}");
            foreach (var emailPack in emailPacks)
            {
                try
                {
                    token.ThrowIfCancellationRequested();
                    await Parallel.ForEachAsync(emailPack, options, async (email, token) =>
                    {
                        var emailBody = _fileService.GenerateEmailBody(
                            new SendParameters()
                            {
                                Lschet = email.Lschet ?? "",
                                Sum = email.Sum ?? "",
                                Text = email.Text ?? "",
                            },
                            emailSendTask?.HtmlMessage ?? "");

                        var sendResult = await SendEmailAsync(email.Email, emailSendTask?.Subject ?? "Тема письма", emailBody);

                        if (sendResult.Item1 == true)
                        {
                            email.IsSuccess = true;
                            email.ErrorMessage = null;
                            email.SendDate = DateTime.UtcNow;
                        }
                        else
                        {
                            email.IsSuccess = false;
                            email.SendDate = DateTime.UtcNow;
                            email.ErrorMessage = sendResult.Item2;
                        }
                    });

                    Log.Information($"Save Changes - {DateTime.UtcNow}");
                    await _dataManager.UpdateEmailSendResult([.. emailPack]);
#if DEBUG
                    File.AppendAllLinesAsync($"Files\\send_{emailSendTask.Name}", emailPack.Select(x => x.Email));
#endif
                }
                catch (OperationCanceledException e)
                {
#if DEBUG
                    File.AppendAllLinesAsync($"Files\\send_{emailSendTask.Name}", emailPack.Select(x => x.Email));
#endif
                    Log.Information($"Cancel Information: {e.Message}");
                    await SendFinished(emailSendTask, emailList, SendTaskStatusEnum.cancel);
                    return;
                }
                catch (Exception e)
                {
                    Log.Information($"Exception: {e.Message}");
                }
            }

            await SendFinished(emailSendTask, emailList, SendTaskStatusEnum.complete);
            DataService.CancelTokenTasks.Remove(emailTaskId);
            Log.Information($"End Send - {DateTime.UtcNow} - {emailSendTask.Name}");
        }

        private async Task SendFinished(EmailSendTask sendTask, List<EmailSendResult> sendResults, SendTaskStatusEnum status)
        {
            sendTask.SendTaskStatus = SendTaskStatusEnum.complete.ToString();
            sendTask.EndDate = DateTime.UtcNow;
            if (status == SendTaskStatusEnum.cancel)
            {
                Log.Error($"<--------Cancel - {DateTime.UtcNow} - {sendTask.Name}\"--------->");
                sendTask.SendTaskStatus = SendTaskStatusEnum.cancel.ToString();
            }
            _dataManager.UpdateEmailSendTask(sendTask);
            await _dataManager.UpdateEmailSendResult(sendResults);
        }

        private async Task<Tuple<bool, string>> SendEmailAsync(string email, string subject, string? body)
        {
            try
            {
                var smtpClient = new SmtpClient(_smtpConfiguration.Server)
                {
                    Port = _smtpConfiguration.Port,
                    Credentials = new NetworkCredential(_smtpConfiguration.Login, _smtpConfiguration.Password),
                    EnableSsl = false,
                    Timeout = 120000 // Устанавливаем тайм-аут в 120 секунд (120000 миллисекунд)
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_smtpConfiguration.HostEmailAddress, _smtpConfiguration.DisplayName),
                    Subject = subject,
                    Body = body ?? "",
                    IsBodyHtml = true,
                };

                mailMessage.To.Add(email);

                await smtpClient.SendMailAsync(mailMessage);
                Log.Information($"Send Complite {email}");
                return Tuple.Create(true, string.Empty);
            }
            catch (Exception ex)
            {
                Log.Information($"Send Bad {email}");
                Log.Error($"Error sending email to {email}: {ex.Message}");
                return Tuple.Create(false, ex.Message);
            }
        }
    }
}
