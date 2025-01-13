using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace WebEmailSendler.Models
{
    public class SmtpConfiguration
    {
        [Required(ErrorMessage = "Поле Server не может быть пустым")]
        public required string Server { get; set; }

        [Required(ErrorMessage = "Поле Port не может быть пустым")]
        public int Port { get; set; } = 25;

        [Required(ErrorMessage = "Поле Ssl не может быть пустым")]
        public bool EnableSsl { get; set; } = false;

        [Required(ErrorMessage = "Поле Login не может быть пустым")]
        public required string Login { get; set; }

        [Required(ErrorMessage = "Поле Password не может быть пустым")]
        public required string Password { get; set; }

        [Required(ErrorMessage = "Поле HostEmailAddress не может быть пустым")]
        public required string HostEmailAddress { get; set; }

        [Required(ErrorMessage = "Поле DisplayName не может быть пустым")]
        public string? DisplayName { get; set; }
    }

    public class AppConfiguration : SmtpConfiguration
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Поле ThreadCount не может быть пустым")]
        public int ThreadCount { get; set; } = 5;

        [Required(ErrorMessage = "Поле ThreadSleep не может быть пустым")]
        public int ThreadSleep { get; set; } = 0;

        [Required(ErrorMessage = "Поле EmailPackSize не может быть пустым")]
        public int EmailPackSize { get; set; } = 50;

    }
}
