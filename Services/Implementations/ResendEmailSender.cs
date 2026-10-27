using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using ChurchPortal.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ChurchPortal.Services.Implementations
{
    public class ResendEmailSender : IEmailSender
    {
        private const string Endpoint = "https://api.resend.com/emails";

        private readonly EmailOptions _options;
        private readonly HttpClient _httpClient;
        private readonly ILogger<ResendEmailSender> _logger;

        public ResendEmailSender(IOptions<EmailOptions> options, HttpClient httpClient, ILogger<ResendEmailSender> logger)
        {
            _options = options.Value;
            _httpClient = httpClient;
            _logger = logger;
        }

        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(_options.ResendApiKey) &&
            !string.IsNullOrWhiteSpace(_options.From);

        public async Task SendAsync(string to, string subject, string body)
        {
            if (!IsConfigured)
            {
                _logger.LogError(
                    "Cannot send '{Subject}' to {To}: Resend is not configured (missing Email:ResendApiKey or Email:From).",
                    subject,
                    to);
                throw new InvalidOperationException(
                    "Email delivery is not configured on the server. Set a Resend API key (RESEND_API_KEY) and try again.");
            }

            var fromAddress = _options.From!;
            var from = string.IsNullOrWhiteSpace(_options.ResendFromName)
                ? fromAddress
                : $"{_options.ResendFromName} <{fromAddress}>";
            var payload = new ResendEmailRequest
            {
                From = from,
                To = [to],
                Subject = subject,
                Html = body
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, Endpoint)
            {
                Content = JsonContent.Create(payload)
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ResendApiKey);

            try
            {
                using var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Resend rejected the email to {To} ({Status}): {Error}", to, (int)response.StatusCode, error);
                    throw new InvalidOperationException(
                        $"The email provider rejected the message ({(int)response.StatusCode}). Check the sender domain and API key.");
                }

                var result = await response.Content.ReadFromJsonAsync<ResendEmailResponse>();
                _logger.LogInformation("Resend accepted email to {To}; id={Id}.", to, result?.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To} via Resend.", to);
                throw;
            }
        }

        private sealed class ResendEmailRequest
        {
            [JsonPropertyName("from")]
            public required string From { get; set; }

            [JsonPropertyName("to")]
            public required List<string> To { get; set; }

            [JsonPropertyName("subject")]
            public required string Subject { get; set; }

            [JsonPropertyName("html")]
            public required string Html { get; set; }
        }

        private sealed class ResendEmailResponse
        {
            [JsonPropertyName("id")]
            public string? Id { get; set; }
        }
    }
}
