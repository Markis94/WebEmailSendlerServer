namespace WebEmailSendler.Models
{
    public class SmtpConfiguration
    {
        public required string Server { get; set; }
        public int Port { get; set; } = 25;
        public required string Login { get; set; }
        public required string Password { get; set; }
        public required string HostEmailAddress { get; set; }
        public string? DisplayName { get; set; }
    }
}
