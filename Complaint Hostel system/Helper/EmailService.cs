using System.Net;
using System.Net.Mail;

public static class EmailService
{
    public static void SendResetLink(string to, string url)
    {
        var mail = new MailMessage
        {
            From = new MailAddress("mindset22910@gmail.com"),
            Subject = "Password Reset",
            Body = $"Click here to reset your password: {url}",
            IsBodyHtml = false
        };
        mail.To.Add(to);

        using (var smtp = new SmtpClient("smtp.gmail.com", 587))
        {
            smtp.EnableSsl = true;
            smtp.Credentials = new NetworkCredential("mindset22910@gmail.com", "keisnhcennehsxpx");
            smtp.Send(mail);
        }
    }
}