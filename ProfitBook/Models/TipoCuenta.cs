using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ProfitBook.Models
{
    public class TipoCuenta
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string Nombre { get; set; }
        public int UsuarioId { get; set; }
    }
}
