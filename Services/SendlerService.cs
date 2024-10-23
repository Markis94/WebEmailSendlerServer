using System.Net.Mail;
using System.Net;
using WebEmailSendler.Models;
using WebEmailSendler.Managers;
using Microsoft.Extensions.Options;
using Serilog;
using System.Collections.Concurrent;
using System;

namespace WebEmailSendler.Services
{
    public class SendlerService
    {
        private readonly int TREAD_COUNT = 30;
        private readonly int PACK_SIZE = 500;
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

        public async Task SendEmailByTask(int emailTaskId, CancellationToken cancellationToken)
        {
            var emailList = await _dataManager.GetEmailSendResult(emailTaskId);
            var emailSendTask = await _dataManager.GetEmailSendTask(emailTaskId);
            emailSendTask!.SendTaskStatus = SendTaskStatusEnum.started.ToString();
            _dataManager.UpdateEmailSendTask(emailSendTask);

            int iterations = emailList.Count;
            ConcurrentBag<EmailSendResult> tempSendModified = new ConcurrentBag<EmailSendResult>();
            // Счетчик для отслеживания изменений
            int changeCounter = 0;
            int saveThreshold = PACK_SIZE;
            var options = new ParallelOptions()
            {
                MaxDegreeOfParallelism = TREAD_COUNT,
                CancellationToken = cancellationToken
            };

            try
            {
                await Parallel.ForEachAsync(emailList, options, async (email, cancellationToken) =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var sendResult = await SendEmailAsync(email.Email, (emailSendTask?.Subject ?? "Тема письма"),
                            _fileService.GenerateEmailBody(new SendParameters()
                            {
                                Lschet = email.Lschet ?? "",
                                Sum = email.Sum ?? "",
                                Text = email.Text ?? "",
                            }, emailSendTask?.HtmlMessage ?? ""));
                    if (sendResult.Item1 == true)
                    {
                        email.IsSuccess = true;
                        email.SentDate = DateTime.UtcNow;
                        Log.Information($"Send Complite {email.Lschet} - {email.Email}");
                    }
                    else
                    {
                        email.IsSuccess = false;
                        email.SentDate = DateTime.UtcNow;
                        email.ErrorMessage = sendResult.Item2;
                        Log.Information($"Send Bad {email.Lschet} - {email.Email}");
                    }
                    tempSendModified.Add(email);
                    int currentCount = Interlocked.Increment(ref changeCounter);
                    // Если достигнут порог изменений, сохраняем результаты
                    if (currentCount % saveThreshold == 0)
                    {
                        Log.Information("Save Changes");
                        await _dataManager.UpdateEmailSendResult([.. tempSendModified]);
                    }
                    cancellationToken.ThrowIfCancellationRequested();
                });
            }
            catch (OperationCanceledException e)
            {
                Log.Information($"Cancel: {e.Message}");
                await CancelSend(emailSendTask, [.. tempSendModified]);
                return;
            }
            if (changeCounter % saveThreshold == 0)
            {
                Log.Information("Save Changes");
                await _dataManager.UpdateEmailSendResult([.. tempSendModified]);
            }
            emailSendTask!.SendTaskStatus = SendTaskStatusEnum.complete.ToString();
            emailSendTask.EndDate = DateTime.UtcNow;
            await _dataManager.UpdateEmailSendResult([.. tempSendModified]);
            _dataManager.UpdateEmailSendTask(emailSendTask);
        }

        public async Task CancelSend(EmailSendTask sendTask, List<EmailSendResult> sendResults)
        {
            Log.Error("<--------Cancel--------->");
            sendTask!.SendTaskStatus = SendTaskStatusEnum.complete.ToString();
            sendTask.EndDate = DateTime.UtcNow;
            _dataManager.UpdateEmailSendTask(sendTask);
            await _dataManager.UpdateEmailSendResult(sendResults);
        }

        public async Task<Tuple<bool, string>> SendEmailAsync(string email, string subject, string? body)
        {
            try
            {
                var smtpClient = new SmtpClient(_smtpConfiguration.Server)
                {
                    Port = _smtpConfiguration.Port,
                    Credentials = new NetworkCredential(_smtpConfiguration.Login, _smtpConfiguration.Password),
                    EnableSsl = false,
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
                return Tuple.Create(true, string.Empty);
            }
            catch (Exception ex)
            {
                Log.Error($"Error sending email to {email}: {ex.Message}");
                return Tuple.Create(false, ex.Message);
            }
        }
    }
}
