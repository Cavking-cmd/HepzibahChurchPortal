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

        public const string DefaultFrom = "notifications@updates.rccghephzibahmanitoba.org";
        public const string DefaultFromName = "Hephzibah Church";
        public const string DefaultFrontendBaseUrl = "https://hepzibahreactrepo.vercel.app";
    }

    /// <summary>
    /// Thrown when a message could not be delivered. The message is safe to
    /// show to the signed-in user (it never contains the API key).
    /// </summary>
    public class EmailDeliveryException : Exception
    {
        public EmailDeliveryException(string message, Exception? inner = null) : base(message, inner) { }
    }

    public interface IEmailSender
    {
        /// <summary>
        /// True when the sender has everything it needs to deliver a message.
        /// Callers should surface this to the user instead of pretending an
        /// email was sent.
        /// </summary>
        bool IsConfigured { get; }

        Task SendAsync(string to, string subject, string body);
    }
}
