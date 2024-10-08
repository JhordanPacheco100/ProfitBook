using Dapper;
using ProfitBook.Models;
using System.Data.SqlClient;

namespace ProfitBook.Servicios
{
    public interface IRepositorioTransacciones
    {
        Task<int> CrearTransaccion(TransaccionesVenta transaccion);
        Task GuardarInversionDia(InversionDia inversion);
        Task<IEnumerable<ResumenVentasVIewModel>> ObtenerResumenVentasPorMes(int mes, int año, int usuarioId);
    }
    public class RepositorioTransacciones: IRepositorioTransacciones
    {
        private readonly string connectionString;
        public RepositorioTransacciones(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        public async Task<int> CrearTransaccion(TransaccionesVenta transaccion)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>(@"
            INSERT INTO TransaccionesVenta (FechaTransaccion, CuentaId, VentaMonto, InversionMonto, UsuarioId) 
            VALUES (@FechaTransaccion, @CuentaId, @VentaMonto, @InversionMonto, @UsuarioId);
            SELECT SCOPE_IDENTITY();", transaccion);
            return id;
        }

        public async Task GuardarInversionDia(InversionDia inversion)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"
            INSERT INTO InversionDia (ProductoId, Precio, TipoCuentaId, Fecha, UsuarioId) 
            VALUES (@ProductoId, @Precio, @TipoCuentaId, @Fecha, @UsuarioId);", inversion);
        }
        public async Task<IEnumerable<ResumenVentasVIewModel>> ObtenerResumenVentasPorMes(int mes, int año, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            string sqlQuery = @"
                SELECT 
                    FechaTransaccion AS Fecha,
                    SUM(VentaMonto) AS Ventas,
                    SUM(InversionMonto) AS Inversion,
                    SUM(VentaMonto - InversionMonto) AS Ganancia
                FROM TransaccionesVenta
                    WHERE MONTH(FechaTransaccion) = @mes 
                    AND YEAR(FechaTransaccion) = @año
                    AND UsuarioId = @usuarioId
                GROUP BY FechaTransaccion";

            return await connection.QueryAsync<ResumenVentasVIewModel>(sqlQuery, new { mes, año, usuarioId });
        }

    }
}
