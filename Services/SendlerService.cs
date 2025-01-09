
using Microsoft.Extensions.Options;
using Serilog;
using System.Net;
using System.Net.Mail;
using WebEmailSendler.Enums;
using WebEmailSendler.Managers;
using WebEmailSendler.Models;

namespace WebEmailSendler.Services
{
    public class SendlerService
    {
        private readonly int TREAD_COUNT;
        private readonly int PACK_SIZE;
        private readonly SignalHub _hub;
        private readonly FileService _fileService;
        private readonly DataManager _dataManager;
        private readonly SmtpConfiguration _smtpConfiguration;

        public SendlerService(FileService fileService, DataManager dataManager, IConfiguration configuration, IOptions<SmtpConfiguration> smtpConfiguration, SignalHub hub)
        {
            _fileService = fileService;
            _dataManager = dataManager;
            _smtpConfiguration = smtpConfiguration.Value;
            TREAD_COUNT = Convert.ToInt32(configuration.GetSection("TREAD_COUNT").Value);
            PACK_SIZE = Convert.ToInt32(configuration.GetSection("PACK_SIZE").Value);
            _hub = hub;
        }

        public async Task SendTestMessage(TestSend testSend)
        {
            foreach (var email in testSend.Emails) {
                await SendEmailAsync(email, testSend.Subject, testSend.HtmlString);
            }
        }

        public async Task SendEmailByTask(int emailTaskId, CancellationToken token)
        {
            var emailList = await _dataManager.GetEmailSendResult(emailTaskId);
            var emailSendTask = await _dataManager.GetEmailSendTask(emailTaskId);
            Log.Information($"Start Send - {DateTime.UtcNow} - {emailSendTask?.Name ?? "" }");

            emailSendTask!.StartDate = DateTime.UtcNow;
            emailSendTask!.SendTaskStatus = SendTaskStatusEnum.started.ToString();
            _dataManager.UpdateEmailSendTask(emailSendTask);
            //делим рассылку на маленькие части.
            var emailPacks = emailList.Chunk(PACK_SIZE);
            //информация по количеству отправленых писем
            var emailinfo = await _dataManager.EmailSendTaskInfo(emailSendTask.Id);
            emailinfo.SendCount = emailList.Count;
            foreach (var emailPack in emailPacks)
            {
                try
                {
                    token.ThrowIfCancellationRequested();
                    var result = await SendEmailParallel(emailSendTask, emailPack.ToList(), TREAD_COUNT, token);

                    //считаем всякое разное и отправляем в хаб
                    emailinfo.CurrentSendCount += result.emailinfo.SuccessSendCount;
                    emailinfo.SuccessSendCount += result.emailinfo.SuccessSendCount;
                    emailinfo.BadSendCount += result.emailinfo.BadSendCount;

                    await UpdatePackResult(result.emailSendResults);
                    await SendInfoHubMessage(emailSendTask, emailinfo);
                }
                catch (OperationCanceledException e)
                {
                    Log.Information($"Cancel Information: {e.Message}");
                    await SendFinished(emailSendTask, emailList, SendTaskStatusEnum.cancel);
                    return;
                }
                catch (Exception e)
                {
                    Log.Information($"Exception: {e.Message}");
                }
                finally {
#if DEBUG
                    await File.AppendAllLinesAsync($"Files\\send_{emailSendTask.Name}", emailPack.Select(x => x.Email), token);
#endif
                }
            }
            await SendFinished(emailSendTask, emailList, SendTaskStatusEnum.complete);
        }

        private async Task SendInfoHubMessage(EmailSendTask emailSendTask, SendInfo emailSendInfo)
        {
            await _hub.SendChangeEmailSendInfo(emailSendTask.Id, emailSendInfo);
        }

        private async Task UpdatePackResult(List<EmailSendData> emailPack)
        {
            if (emailPack.Count > 2000)
            {
                await _dataManager.BulkUpdateEmailSendResult(emailPack.ToList());
            }
            else
            {
                _dataManager.UpdateEmailSendResult(emailPack.ToList());
            }
            Log.Information($"Update Pack Result - {DateTime.UtcNow}");
        }

        private async Task<(SendInfo emailinfo, List<EmailSendData> emailSendResults)> SendEmailParallel(EmailSendTask emailSendTask, List<EmailSendData> emailSendResults, int TreadCount, CancellationToken token)
        {
            var emailinfo = new SendInfo();
            var options = new ParallelOptions()
            {
                MaxDegreeOfParallelism = TREAD_COUNT,
                CancellationToken = token
            };

            await Parallel.ForEachAsync(emailSendResults, options, async (email, token) =>
            {
                try
                {
                    token.ThrowIfCancellationRequested();
                    var emailBody = _fileService.GenerateEmailBody(
                        new SendParameters()
                        {
                            Lschet = email.Lschet ?? "",
                            Sum = email.Sum ?? "",
                            Text = email.Text ?? "",
                        },
                        emailSendTask?.HtmlMessage ?? "");

                    try
                    {
                        var sendResult = await SendEmailAsync(email.Email, emailSendTask?.Subject ?? "Тема письма", emailBody)
                            .WaitAsync(TimeSpan.FromMinutes(2));
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
                    }
                    catch (TimeoutException ex)
                    {
                        email.IsSuccess = false;
                        email.SendDate = DateTime.UtcNow;
                        email.ErrorMessage = ex.Message;
                        Log.Error($"Error timeout exception send email {email.Email}: {ex.Message}");
                    }
                }
                catch (OperationCanceledException e)
                {
                    Log.Information($"Cancel Information: {e.Message}");
                    return;
                }
                catch (Exception ex)
                {
                    Log.Error($"Error processing parallel send email: {ex.Message}");
                }
            });
            emailinfo.SuccessSendCount = emailSendResults.Where(x=>x.IsSuccess == true).ToList().Count;
            emailinfo.BadSendCount = emailSendResults.Where(x => x.IsSuccess == false).ToList().Count;
            return (emailinfo, emailSendResults);
        }

        private async Task SendFinished(EmailSendTask sendTask, List<EmailSendData> sendResults, SendTaskStatusEnum status)
        {
            sendTask.SendTaskStatus = SendTaskStatusEnum.complete.ToString();
            sendTask.EndDate = DateTime.UtcNow;
            if (status == SendTaskStatusEnum.cancel)
            {
                Log.Error($"<--------Cancel - {DateTime.UtcNow} - {sendTask.Name}\"--------->");
                sendTask.SendTaskStatus = SendTaskStatusEnum.cancel.ToString();
            }
            await _dataManager.BulkUpdateEmailSendResult(sendResults);
            var info = await _dataManager.EmailSendTaskInfo(sendTask.Id);
            sendTask.BadSendCount = info.BadSendCount;
            sendTask.SuccessSendCount = info.SuccessSendCount;
            _dataManager.UpdateEmailSendTask(sendTask);
            TokenHub.CancelTokenTasks.Remove(sendTask.Id);
            await _hub.SendChangeEmailSendStatus(sendTask);
            Log.Information($"End Send - {DateTime.UtcNow} - {emailSendTask.Name}");
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
