using SendGrid.Helpers.Mail;
using SendGrid;

namespace ProfitBook.Servicios
{
    public interface IServicioEmail
    {
        Task EnviarEmailCambioPassword(string receptor, string enlace);
    }
    public class ServicioEmail: IServicioEmail
    {
        private readonly IConfiguration configuration;

        public ServicioEmail(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public async Task EnviarEmailCambioPassword(string receptor, string enlace)
        {
            var apiKey = configuration["SendGrid:ApiKey"];
            var cliente = new SendGridClient(apiKey);
            var desde = new EmailAddress("jhordanmisaelpat@hotmail.com", "ProfitBook");
            var asunto = "¿Ha olvidado su contraseña?";

            var contenidoHTML = $@"
        <p>Saludos,</p>
        <p>Este mensaje le llega porque usted ha solicitado un cambio de contraseña. Si esta solicitud no fue hecha por usted, puede ignorar este mensaje.</p>
        <p>Para cambiar su contraseña, haga click en el siguiente enlace:</p>
        <p><a href='{enlace}' target='_blank'>Cambiar Contraseña</a></p>
        <p>Atentamente,<br />Equipo ProfitBook</p>";

            var mensaje = MailHelper.CreateSingleEmail(desde, new EmailAddress(receptor), asunto, contenidoHTML, contenidoHTML);
            var respuesta = await cliente.SendEmailAsync(mensaje);

            if (respuesta.StatusCode != System.Net.HttpStatusCode.OK && respuesta.StatusCode != System.Net.HttpStatusCode.Accepted)
            {
                throw new Exception("Error al enviar el correo: " + respuesta.StatusCode);
            }
        }


    }
}
