using Hangfire.Dashboard;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebEmailSendler.Enums;

namespace WebEmailSendler.Models
{
    public class Sample
    {
        [Key]
        public int Id { get; set; }
        public required string Name { get; set; }
        public DateTimeOffset CreateDate { get; set; } = DateTimeOffset.Now;
        public DateTimeOffset ChangeDate { get; set; } = DateTimeOffset.Now;
        public required string JsonString { get; set; } = "";
        public required string HtmlString { get; set; } = "";
    }
    public class EmailSendTask : EmailSendInfo
    {
        [Key]
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Subject { get; set; }
        public required string HtmlMessage { get; set; }
        public DateTimeOffset CreateDate { get; set; }
        public DateTimeOffset? StartDate { get; set; } = null;
        public DateTimeOffset EndDate { get; set; }
        public string SendTaskStatus { get; set; } = SendTaskStatusEnum.created.ToString();
        public string JobId { get; set; } = string.Empty;

        [NotMapped]
        public EmailSendInfo? EmailSendInfo { get; set; } = null;

        [NotMapped]
        public string? CsvData { get; set; }
    }

    [Index(nameof(Email))]
    [Index(nameof(IsSuccess))]
    public class EmailSendResult : SendParameters
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("EmailSendTaskId")]
        public EmailSendTask? User { get; set; }

        [Required(ErrorMessage = "Поле EmailSendTaskId не может быть пустым")]
        public int EmailSendTaskId { get; set; }
        [Required]
        public required string Email { get; set; }
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTimeOffset SendDate { get; set; }
    }
    //-------------------------------------------------

    public class TestSend
    {
        [Required]
        public required List<string> Emails { get; set; }
        public string Subject { get; set; } = "Тестовое письмо " + DateTime.Now;
        [Required]
        public required string HtmlString { get; set; }
    }
    public class EmailSendInfo
    {
        public int MaxCount { get; set; } = 0;
        [NotMapped]
        public int SendCount { get; set; } = 0;
        [NotMapped]
        public int CurrentSendCount { get; set; } = 0;
        public int SuccessSendCount { get; set; } = 0;
        public int BadSendCount { get; set; } = 0;
    }
    public class Part<EmailSendTask>
    {
        public List<EmailSendTask> Items { get; set; }
        public int TotalCount { get; set; }
    }

    public class EmailData
    {
        public required string Email { get; set; }
        public SendParameters? SendParameters { get; set; }
    }

    public class SendParameters
    {
        public string? Lschet { get; set; }
        public string? Sum { get; set; }
        public string? Text { get; set; }
    }

    public class MyAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            //var httpContext = context.GetHttpContext();
            // Allow all authenticated users to see the Dashboard (potentially dangerous).
            return true;
        }
    }
}
