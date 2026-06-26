using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using TrucoRPG.Dominio.Servicios;

namespace TrucoRPG.Infraestructura.Servicios
{
    public class EmailService : IEmailService
    {
        private readonly string _host;
        private readonly int    _port;
        private readonly string _user;
        private readonly string _password;
        private readonly string _from;

        public EmailService(IConfiguration config)
        {
            var section = config.GetSection("Email");
            _host     = section["Host"]     ?? throw new InvalidOperationException("Email:Host no configurado.");
            _port     = int.Parse(section["Port"] ?? "587");
            _user     = section["User"]     ?? throw new InvalidOperationException("Email:User no configurado.");
            _password = section["Password"] ?? throw new InvalidOperationException("Email:Password no configurado.");
            _from     = section["From"]     ?? _user;
        }

        public async Task EnviarAsync(string destinatario, string asunto, string cuerpoHtml)
        {
            var mensaje = new MimeMessage();
            mensaje.From.Add(MailboxAddress.Parse(_from));
            mensaje.To.Add(MailboxAddress.Parse(destinatario));
            mensaje.Subject = asunto;
            mensaje.Body = new TextPart("html") { Text = cuerpoHtml };

            using var client = new SmtpClient();
            await client.ConnectAsync(_host, _port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_user, _password);
            await client.SendAsync(mensaje);
            await client.DisconnectAsync(true);
        }
    }
}
