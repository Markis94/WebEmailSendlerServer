using Microsoft.AspNetCore.SignalR;
using WebEmailSendler.Models;

namespace WebEmailSendler
{
    public class SignalHub : Hub
    {
        public async Task SendChangeEmailSendInfo(int emailSendTaskId, EmailSendInfo emailSendInfo)
        {
            await Clients.All.SendAsync("ChangeEmailSendInfo", emailSendTaskId, emailSendInfo);
        }

        public async Task SendChangeEmailSendStatus(EmailSendTask emailSendTask)
        {
            await Clients.All.SendAsync("ChangeEmailSendStatus", emailSendTask);
        }
    }
}
