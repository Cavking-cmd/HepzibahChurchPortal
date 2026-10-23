namespace ChurchPortal.Services.Interfaces
{
    public class EmailOptions
    {
        public string? Host { get; set; }
        public int Port { get; set; } = 587;
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? From { get; set; }
        public bool EnableSsl { get; set; } = true;

        // Resend
        public string? ResendApiKey { get; set; }
        public string? ResendFromName { get; set; }
        public string? FrontendBaseUrl { get; set; }
        public bool UseResend { get; set; } = true;
    }

    public interface IEmailSender
    {
        Task SendAsync(string to, string subject, string body);
    }
}
