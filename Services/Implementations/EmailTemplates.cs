using System.Net;

namespace ChurchPortal.Services.Implementations
{
    public static class EmailTemplates
    {
        public static string VerifyEmail(string name, string link, int hoursValid)
        {
            var safeName = WebUtility.HtmlEncode(name);
            var safeLink = WebUtility.HtmlEncode(link);
            return Wrap(
                "Verify your email address",
                $"<p>Hello {safeName},</p>" +
                "<p>An account has been created for you on the RCCG Hephzibah Parish portal. " +
                "Please confirm your email address to finish setting it up.</p>" +
                Button(link, "Verify my email") +
                "<p>This link expires in " + hoursValid + " hours. If you did not expect this, you can ignore this email.</p>",
                safeLink);
        }

        public static string ResetPassword(string name, string link, int hoursValid)
        {
            var safeName = WebUtility.HtmlEncode(name);
            var safeLink = WebUtility.HtmlEncode(link);
            return Wrap(
                "Reset your password",
                $"<p>Hello {safeName},</p>" +
                "<p>We received a request to reset the password for your RCCG Hephzibah Parish portal account.</p>" +
                Button(link, "Reset my password") +
                "<p>This link expires in " + hoursValid + " hours. If you did not request a reset, you can ignore this email and your password will stay the same.</p>",
                safeLink);
        }

        public static string Notification(string title, string message, string? link)
        {
            var body = $"<p>{WebUtility.HtmlEncode(message)}</p>";
            if (!string.IsNullOrWhiteSpace(link))
            {
                body += Button(link, "Open the portal");
            }
            return Wrap(title, body, link == null ? null : WebUtility.HtmlEncode(link));
        }

        private static string Button(string link, string label)
        {
            var safeLink = WebUtility.HtmlEncode(link);
            return
                "<p style=\"margin:24px 0\">" +
                $"<a href=\"{safeLink}\" style=\"background:#4f46e5;color:#ffffff;padding:10px 20px;border-radius:6px;text-decoration:none;display:inline-block\">{WebUtility.HtmlEncode(label)}</a>" +
                "</p>" +
                $"<p style=\"font-size:12px;color:#64748b;word-break:break-all\">Or paste this link into your browser:<br />{safeLink}</p>";
        }

        private static string Wrap(string title, string innerHtml, string? footerLink)
        {
            return
                "<!doctype html><html><body style=\"margin:0;padding:0;background:#f1f5f9;font-family:Segoe UI,Roboto,Helvetica,Arial,sans-serif;color:#0f172a\">" +
                "<table role=\"presentation\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" style=\"background:#f1f5f9;padding:24px\">" +
                "<tr><td align=\"center\">" +
                "<table role=\"presentation\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" style=\"max-width:520px;background:#ffffff;border-radius:12px;padding:32px\">" +
                $"<tr><td><p style=\"font-size:13px;color:#4f46e5;font-weight:600;margin:0 0 8px\">RCCG Hephzibah Parish</p>" +
                $"<h1 style=\"font-size:20px;margin:0 0 16px\">{WebUtility.HtmlEncode(title)}</h1></td></tr>" +
                $"<tr><td style=\"font-size:15px;line-height:1.6\">{innerHtml}</td></tr>" +
                "<tr><td style=\"padding-top:24px;font-size:12px;color:#94a3b8\">This message was sent by the RCCG Hephzibah Parish portal.</td></tr>" +
                "</table></td></tr></table></body></html>";
        }
    }
}
