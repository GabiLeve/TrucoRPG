using TrucoRPG.Dominio.Repositorios;
using TrucoRPG.Dominio.Servicios;

namespace TrucoRPG.Dominio.UseCases
{
    public class SolicitarResetPasswordUseCase : ISolicitarResetPasswordUseCase
    {
        private readonly IUsuarioRepositorio _usuarioRepo;
        private readonly IEmailService       _emailService;
        private readonly string              _frontendUrl;

        public SolicitarResetPasswordUseCase(
            IUsuarioRepositorio usuarioRepo,
            IEmailService emailService,
            string frontendUrl)
        {
            _usuarioRepo  = usuarioRepo;
            _emailService = emailService;
            _frontendUrl  = frontendUrl;
        }

        public async Task EjecutarAsync(string email)
        {
            var token = await _usuarioRepo.GenerarTokenResetPasswordAsync(email);

            var tokenEncoded = Uri.EscapeDataString(token);
            var emailEncoded = Uri.EscapeDataString(email);
            var link = $"{_frontendUrl}/reset-password?email={emailEncoded}&token={tokenEncoded}";

            var cuerpo = $"""
                <div style="font-family:sans-serif;max-width:480px;margin:auto">
                  <h2 style="color:#c8a030">Truco & Maña — Restablecer contraseña</h2>
                  <p>Recibimos una solicitud para restablecer tu contraseña.</p>
                  <p>
                    <a href="{link}"
                       style="display:inline-block;padding:12px 24px;background:#c8a030;color:#1a1208;
                              text-decoration:none;font-weight:bold;border-radius:4px">
                      Restablecer contraseña
                    </a>
                  </p>
                  <p style="color:#888;font-size:0.85rem">
                    Este link expira en 1 hora. Si no solicitaste el cambio, ignorá este email.
                  </p>
                </div>
                """;

            await _emailService.EnviarAsync(email, "Restablecer contraseña — Truco & Maña", cuerpo);
        }
    }
}
