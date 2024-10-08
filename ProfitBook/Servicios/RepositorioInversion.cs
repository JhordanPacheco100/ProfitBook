using Dapper;
using ProfitBook.Models;
using System.Data.SqlClient;

namespace ProfitBook.Servicios
{
    public interface IRepositorioInversion
    {
        Task Insertar(Inversion inversion);
        Task<List<DateTime>> ObtenerFechasDisponibles(int usuarioId);
        Task<List<Inversion>> ObtenerPorFecha(DateTime fecha, int usuarioId);
    }
    public class RepositorioInversion: IRepositorioInversion
    {
        private readonly string connectionString;

        public RepositorioInversion(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        
        public async Task<List<DateTime>> ObtenerFechasDisponibles(int usuarioId)
        {
            // Consulta modificada para obtener las fechas disponibles filtradas por UsuarioId
            var query = "SELECT DISTINCT Fecha FROM Inversion WHERE UsuarioId = @UsuarioId ORDER BY Fecha";

            // Usando Dapper para ejecutar la consulta
            using (var connection = new SqlConnection(connectionString))
            {
                // Pasar el UsuarioId como parámetro en la consulta
                return (await connection.QueryAsync<DateTime>(query, new { UsuarioId = usuarioId })).ToList();
            }
        }
        public async Task<List<Inversion>> ObtenerPorFecha(DateTime fecha, int usuarioId)
        {
            var query = @"
                SELECT i.Id, p.Nombre AS ProductoNombre, i.Cantidad, i.PrecioUnitario, i.Total, i.Fecha, i.Estado, i.Saldo
                FROM Inversion i
                JOIN Productos p ON i.ProductoId = p.Id
                WHERE i.Fecha = @Fecha AND i.UsuarioId = @UsuarioId";

            using (var connection = new SqlConnection(connectionString))
            {
                // Pasar correctamente los parámetros 'Fecha' y 'UsuarioId'
                return (await connection.QueryAsync<Inversion>(query, new { Fecha = fecha, UsuarioId = usuarioId })).ToList();
            }
        }

        public async Task Insertar(Inversion inversion)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var query = "INSERT INTO Inversion (ProductoId, Cantidad, PrecioUnitario, Total, Fecha, Saldo, UsuarioId) " +
                    "        VALUES (@ProductoId, @Cantidad, @PrecioUnitario, @Total, @Fecha, @Saldo, @UsuarioId)";
                await connection.ExecuteAsync(query, inversion);
            }
        }
    }
}
