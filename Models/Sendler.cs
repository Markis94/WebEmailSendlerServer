using Hangfire.Dashboard;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebEmailSendler.Models
{
    public class EmailSendInfo
    {
        public int MaxCount { get; set; }
        public int SendCount { get; set; }
        public int BadSendCount { get; set; }
    }

    public class Part<EmailSendTask>
    {
        public List<EmailSendTask> Items { get; set; }
        public int TotalCount { get; set; }
    }

    public class EmailSendTask
    {
        [Key]
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Subject { get; set; }
        public required string HtmlMessage { get; set; }
        public DateTimeOffset CreateDate { get; set; }
        public DateTimeOffset EndDate { get; set; }
        public string SendTaskStatus { get; set; } = SendTaskStatusEnum.created.ToString();
        public string JobId { get; set; } = string.Empty;

        [NotMapped]
        public string? CsvData { get; set; }
    }

    [Index(nameof(Email))]
    [Index(nameof(IsSuccess))]
    public class EmailSendResult: SendParameters
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
        public DateTimeOffset SentDate { get; set; }
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

    public class AllowAllAuthorizationFilter : Hangfire.Dashboard.IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            return true; // Разрешить всем
        }
    }

    public enum SendTaskStatusEnum
    {
        created,
        started,
        complete
    }
}
