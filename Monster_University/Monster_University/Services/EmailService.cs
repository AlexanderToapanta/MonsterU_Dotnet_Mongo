using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Monster_University.Services
{
    public class EmailService
    {
        // CONFIGURACIÓN SENDGRID (AJUSTA CON TUS CREDENCIALES)
        private const string CORREO_REMITENTE = "alexandertoapantaj05@gmail.com";
        private const string CONTRASENIA_REMITENTE = "";
        private const string SERVIDOR_SMTP = "smtp.sendgrid.net";
        private const int PUERTO_SMTP = 587;

        // VERSIÓN SÍNCRONA (como en Java)
        public bool EnviarCredencialesSincrono(string destinatario, string nombreUsuario, string contrasenia)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"📧 Iniciando envío a: {destinatario}");

                if (string.IsNullOrWhiteSpace(destinatario))
                    return false;

                // Reducir timeout para no bloquear tanto
                using (var smtpClient = new SmtpClient())
                {
                    smtpClient.Host = SERVIDOR_SMTP;
                    smtpClient.Port = PUERTO_SMTP;
                    smtpClient.EnableSsl = true;
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Credentials = new NetworkCredential("apikey", CONTRASENIA_REMITENTE);
                    smtpClient.Timeout = 10000; // 15 segundos máximo

                    using (var mailMessage = new MailMessage())
                    {
                        mailMessage.From = new MailAddress(CORREO_REMITENTE, "Monsters University");
                        mailMessage.To.Add(destinatario.Trim());
                        mailMessage.Subject = "Credenciales de Acceso - Monsters University";
                        mailMessage.IsBodyHtml = true;
                        mailMessage.Body = ConstruirContenidoHTML(nombreUsuario, contrasenia);

                        // Envío síncrono pero con timeout controlado
                        smtpClient.Send(mailMessage);

                        System.Diagnostics.Debug.WriteLine($"✅ Correo enviado a: {destinatario}");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error enviando correo a {destinatario}: {ex.Message}");
                return false;
            }
        }

        // (Opcional) Mantener también la versión asíncrona
        public async Task<bool> EnviarCredencialesAsync(string destinatario, string nombreUsuario, string contrasenia)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== ENVIANDO CORREO CON SENDGRID (ASÍNCRONO) ===");
                System.Diagnostics.Debug.WriteLine($"📧 Para: {destinatario}");

                // Validación
                if (string.IsNullOrWhiteSpace(destinatario))
                {
                    System.Diagnostics.Debug.WriteLine("❌ ERROR: Email destinatario vacío");
                    return false;
                }

                // Configuración SMTP
                using (var smtpClient = new SmtpClient())
                {
                    smtpClient.Host = SERVIDOR_SMTP;
                    smtpClient.Port = PUERTO_SMTP;
                    smtpClient.EnableSsl = true;
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Credentials = new NetworkCredential("apikey", CONTRASENIA_REMITENTE);
                    smtpClient.Timeout = 10000;

                    // Crear mensaje
                    using (var mailMessage = new MailMessage())
                    {
                        mailMessage.From = new MailAddress(CORREO_REMITENTE, "Monsters University");
                        mailMessage.To.Add(destinatario.Trim());
                        mailMessage.Subject = "Credenciales de Acceso - Monsters University";
                        mailMessage.IsBodyHtml = true;
                        mailMessage.Body = ConstruirContenidoHTML(nombreUsuario, contrasenia);

                        // Envío asíncrono
                        await smtpClient.SendMailAsync(mailMessage);

                        System.Diagnostics.Debug.WriteLine("✅ CORREO ENVIADO EXITOSAMENTE");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ ERROR: {ex.Message}");
                return false;
            }
        }

        private string ConstruirContenidoHTML(string nombreUsuario, string contrasenia)
        {
            return $@"<!DOCTYPE html>
<html>
<head><meta charset='UTF-8'></head>
<body style='font-family: Arial;'>
    <div style='max-width: 600px; margin: auto; background: #f8f9fa; padding: 20px;'>
        <h2 style='color: #2a64c5;'>Monsters University</h2>
        <h3>¡Bienvenido/a!</h3>
        
        <div style='background: white; padding: 20px; border-radius: 5px;'>
            <p>Tus credenciales de acceso:</p>
            
            <div style='margin: 15px 0;'>
                <strong>👤 Usuario:</strong> 
                <span style='background: #e9ecef; padding: 5px; border-radius: 3px; font-family: monospace;'>
                    {nombreUsuario}
                </span>
            </div>
            
            <div style='margin: 15px 0;'>
                <strong>🔑 Contraseña:</strong> 
                <span style='background: #e9ecef; padding: 5px; border-radius: 3px; font-family: monospace;'>
                    {contrasenia}
                </span>
            </div>
            
            <p><strong>Accede al sistema:</strong> 
            <a href='http://localhost:8080/Monster_University'>http://localhost:8080/Monster_University</a></p>
            
            <div style='background: #fff3cd; padding: 10px; border-radius: 3px; margin-top: 20px;'>
                ⚠️ <strong>IMPORTANTE:</strong> Cambia tu contraseña en el primer acceso.
            </div>
        </div>
        
        <p style='color: #666; font-size: 12px; margin-top: 20px;'>
            © {DateTime.Now.Year} Monsters University
        </p>
    </div>
</body>
</html>";
        }
    }
}