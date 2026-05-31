using SendGrid;
using SendGrid.Helpers.Mail;

namespace BuddyAssist.API.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        // Välkomstmejl när användare registrerar sig
        public async Task SendWelcomeEmailAsync(string toEmail, string toName)
        {
            if (!IsValidEmail(toEmail)) { Console.WriteLine($"Ogiltig e-post: {toEmail}"); return; }
            var msg = CreateMessage(toEmail, toName, "Välkommen till BuddyAssist!",
                $@"<div style='font-family:sans-serif;max-width:600px;margin:0 auto'>
                    <div style='background:#1D9E75;padding:24px;border-radius:12px 12px 0 0;text-align:center'>
                        <h1 style='color:white;margin:0'>BuddyAssist</h1>
                        <p style='color:#E1F5EE;margin:8px 0 0'>Lokal hjälp & belöningssystem</p>
                    </div>
                    <div style='background:#f9f9f7;padding:28px;border-radius:0 0 12px 12px'>
                        <h2 style='color:#085041'>Välkommen, {toName}!</h2>
                        <p style='color:#444;font-size:15px;line-height:1.6'>Ditt konto är nu skapat!</p>
                        <div style='background:#E1F5EE;border-radius:10px;padding:16px;margin:20px 0'>
                            <p style='margin:0;color:#085041;font-weight:600'>Kom igång direkt:</p>
                            <ul style='color:#085041;margin:8px 0 0;padding-left:20px'>
                                <li>Bläddra bland uppdrag nära dig</li>
                                <li>Ta ett uppdrag och tjäna poäng</li>
                                <li>Samla badges och klättra på topplistan</li>
                            </ul>
                        </div>
                        <p style='color:#888;font-size:13px;text-align:center;font-style:italic'>Sma insatser. Stor skillnad.</p>
                    </div>
                </div>");
            await SendAsync(msg);
        }

        // Bekräftelsemejl när användare tar ett uppdrag
        public async Task SendMissionTakenConfirmationAsync(
            string toEmail, string toName, string missionTitle, int points)
        {
            if (!IsValidEmail(toEmail)) { Console.WriteLine($"Ogiltig e-post: {toEmail}"); return; }
            var msg = CreateMessage(toEmail, toName, $"Du har tagit ett uppdrag - {missionTitle}",
                $@"<div style='font-family:sans-serif;max-width:600px;margin:0 auto'>
                    <div style='background:#1D9E75;padding:24px;border-radius:12px 12px 0 0;text-align:center'>
                        <h1 style='color:white;margin:0'>BuddyAssist</h1>
                    </div>
                    <div style='background:#f9f9f7;padding:28px;border-radius:0 0 12px 12px'>
                        <h2 style='color:#085041'>Bra jobbat, {toName}!</h2>
                        <p style='color:#444;font-size:15px'>Du har tagit uppdraget <strong>{missionTitle}</strong>.</p>
                        <div style='background:#E1F5EE;border-radius:10px;padding:20px;margin:20px 0;text-align:center'>
                            <p style='margin:0;color:#085041;font-size:14px'>Du tjanar</p>
                            <p style='margin:8px 0;color:#1D9E75;font-size:32px;font-weight:700'>+{points} poang</p>
                        </div>
                        <p style='color:#888;font-size:13px;text-align:center;font-style:italic'>Tack for att du hjalper till!</p>
                    </div>
                </div>");
            await SendAsync(msg);
        }

        // Notifiering till uppdragsägaren
        public async Task SendMissionTakenNotificationAsync(
            string toEmail, string toName, string takenByName, string missionTitle)
        {
            if (!IsValidEmail(toEmail)) { Console.WriteLine($"Ogiltig e-post: {toEmail}"); return; }
            var msg = CreateMessage(toEmail, toName, $"Ditt uppdrag har tagits - {missionTitle}",
                $@"<div style='font-family:sans-serif;max-width:600px;margin:0 auto'>
                    <div style='background:#1D9E75;padding:24px;border-radius:12px 12px 0 0;text-align:center'>
                        <h1 style='color:white;margin:0'>BuddyAssist</h1>
                    </div>
                    <div style='background:#f9f9f7;padding:28px;border-radius:0 0 12px 12px'>
                        <h2 style='color:#085041'>Hej {toName}!</h2>
                        <p style='color:#444;font-size:15px'>Ditt uppdrag <strong>{missionTitle}</strong> har tagits av <strong style='color:#1D9E75'>{takenByName}</strong>!</p>
                        <div style='background:#E1F5EE;border-radius:10px;padding:16px;margin:20px 0'>
                            <p style='margin:0;color:#085041'>Uppdraget ar nu under arbete!</p>
                        </div>
                        <p style='color:#888;font-size:13px;text-align:center;font-style:italic'>Tack for att du anvander BuddyAssist!</p>
                    </div>
                </div>");
            await SendAsync(msg);
        }

        // Tack-mejl när uppdrag slutförs med betyg
        public async Task SendMissionCompletedEmailAsync(
            string toEmail, string toName, string missionTitle, int rating, string comment)
        {
            if (!IsValidEmail(toEmail)) { Console.WriteLine($"Ogiltig e-post: {toEmail}"); return; }

            var stars = string.Concat(Enumerable.Repeat("*", rating));
            var commentSection = string.IsNullOrEmpty(comment)
                ? ""
                : $"<p style='margin:12px 0 0;color:#444;font-style:italic'>\"{comment}\"</p>";

            var msg = CreateMessage(toEmail, toName, $"Uppdraget slutfort - {missionTitle}",
                $@"<div style='font-family:sans-serif;max-width:600px;margin:0 auto'>
                    <div style='background:#1D9E75;padding:24px;border-radius:12px 12px 0 0;text-align:center'>
                        <h1 style='color:white;margin:0'>BuddyAssist</h1>
                    </div>
                    <div style='background:#f9f9f7;padding:28px;border-radius:0 0 12px 12px'>
                        <h2 style='color:#085041'>Bra jobbat, {toName}!</h2>
                        <p style='color:#444;font-size:15px'>Uppdraget <strong>{missionTitle}</strong> ar nu slutfort!</p>
                        <div style='background:#E1F5EE;border-radius:10px;padding:20px;margin:20px 0;text-align:center'>
                            <p style='margin:0;color:#085041;font-size:14px'>Ditt betyg</p>
                            <p style='margin:8px 0;font-size:28px'>{rating}/5 stjarnor</p>
                            <p style='margin:0;color:#085041;font-size:18px;font-weight:700'>{rating} / 5</p>
                            {commentSection}
                        </div>
                        <p style='color:#888;font-size:13px;text-align:center;font-style:italic'>Tack for att du hjalper till i lokalsamhallet!</p>
                    </div>
                </div>");
            await SendAsync(msg);
        }

        // Validera e-postadress
        private static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch { return false; }
        }

        // Skapa meddelande
        private SendGridMessage CreateMessage(
            string toEmail, string toName, string subject, string htmlContent)
        {
            var fromEmail = _config["SendGrid:FromEmail"];
            var fromName = _config["SendGrid:FromName"];

            var msg = new SendGridMessage
            {
                From = new EmailAddress(fromEmail, fromName),
                Subject = subject,
                HtmlContent = htmlContent,
            };
            msg.AddTo(new EmailAddress(toEmail, toName));
            return msg;
        }

        // Skicka mejl
        private async Task SendAsync(SendGridMessage msg)
        {
            var apiKey = _config["SendGrid:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                Console.WriteLine("SendGrid: API-nyckel saknas!");
                return;
            }

            Console.WriteLine($"Skickar mejl till: {msg.Personalizations?[0]?.Tos?[0]?.Email}");

            var client = new SendGridClient(apiKey);
            var response = await client.SendEmailAsync(msg);
            var body = await response.Body.ReadAsStringAsync();

            Console.WriteLine($"SendGrid status: {response.StatusCode}");
            Console.WriteLine($"SendGrid svar:   {body}");

            if ((int)response.StatusCode >= 400)
                Console.WriteLine($"SendGrid FEL: {body}");
        }
    }


}