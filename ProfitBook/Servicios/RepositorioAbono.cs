using Dapper;
using ProfitBook.Models;
using System.Data.SqlClient;

namespace ProfitBook.Servicios
{
    public interface IRepositorioAbono
    {
        Task GuardarAbono(Abono abono);
        Task<List<DateTime>> ObtenerFechasDisponibles(int usuarioId);
        Task<List<Abono>> ObtenerInversionesPorFecha(DateTime fecha, int usuarioId);
    }
    public class RepositorioAbono: IRepositorioAbono
    {
        private readonly string connectionString;

        public RepositorioAbono(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<List<DateTime>> ObtenerFechasDisponibles(int usuarioId)
        {
            var query = @"SELECT DISTINCT Fecha FROM Inversion WHERE Estado = 'Adeudado' AND UsuarioId = @UsuarioId";

            using (var connection = new SqlConnection(connectionString))
            {
                // Usando Dapper para ejecutar la consulta
                return (await connection.QueryAsync<DateTime>(query, new { UsuarioId = usuarioId })).ToList();
            }
        }
        
        public async Task<List<Abono>> ObtenerInversionesPorFecha(DateTime fecha, int usuarioId)
        {
            var query = @"
                SELECT i.Id, i.ProductoId, p.Nombre AS ProductoNombre, i.Total, i.Saldo, 
                       ISNULL(SUM(a.MontoAbono), 0) AS MontoAbono, i.Fecha, i.Estado
                FROM Inversion i
                JOIN Productos p ON i.ProductoId = p.Id
                LEFT JOIN Abono a ON i.Id = a.IdInversion
                WHERE i.Fecha = @Fecha AND i.Estado = 'Adeudado' AND i.UsuarioId = @UsuarioId
                GROUP BY i.Id, i.ProductoId, p.Nombre, i.Total, i.Saldo, i.Fecha, i.Estado";

            using (var connection = new SqlConnection(connectionString))
            {
                var inversiones = await connection.QueryAsync<Abono>(query, new { Fecha = fecha, UsuarioId = usuarioId });
                return inversiones.ToList();
            }
        }

        public async Task GuardarAbono(Abono abono)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var query = @"
            INSERT INTO Abono (ProductoId, ProductoNombre, Total, MontoAbono, Saldo, FechaAbono, Estado, IdInversion,UsuarioId)
            VALUES (@ProductoId, @ProductoNombre, @Total, @MontoAbono, @Saldo, @FechaAbono, @Estado, @IdInversion, @UsuarioId)";

                await connection.ExecuteAsync(query, new
                {
                    abono.ProductoId,
                    abono.ProductoNombre,
                    abono.Total,
                    abono.MontoAbono,
                    abono.Saldo,
                    abono.FechaAbono,
                    abono.Estado,
                    IdInversion = abono.Id,
                    abono.UsuarioId,
                });

                if (abono.Estado == "Pagado")
                {
                    var queryUpdateInversion = @"UPDATE Inversion SET Estado = 'Pagado' WHERE Id = @IdInversion";

                    await connection.ExecuteAsync(queryUpdateInversion, new { IdInversion = abono.Id });
                }

                if (abono.Saldo != abono.Total)
                {
                    var queryUpdateSaldo = @"UPDATE Inversion SET Saldo = @Saldo WHERE Id = @IdInversion";

                    await connection.ExecuteAsync(queryUpdateSaldo, new
                    {
                        Saldo = abono.Saldo,
                        IdInversion = abono.Id
                    });
                }

                // Buscar si ya existe una transacción para la fecha y usuario
                var transaccionId = await ObtenerTransaccionIdPorFecha(abono.FechaAbono, abono.UsuarioId);
                if (!transaccionId.HasValue)
                {
                    // Si no hay transacción existente, obtener el tipo de cuenta
                    int? tipoCuentaId = await ObtenerTipoCuentaId(abono.UsuarioId);
                    if (!tipoCuentaId.HasValue)
                    {
                        throw new InvalidOperationException("No se pudo encontrar una cuenta para este usuario.");
                    }

                    // Insertar una nueva transacción
                    await InsertarTransaccion(abono.FechaAbono, tipoCuentaId.Value, abono.UsuarioId);

                    // Obtener el Id de la transacción recién creada
                    transaccionId = await ObtenerTransaccionIdPorFecha(abono.FechaAbono, abono.UsuarioId);
                }

                // Actualizar el InversionMonto en la transacción encontrada o creada
                var queryUpdateTransacciones = @"
                    UPDATE TransaccionesVenta 
                    SET InversionMonto = InversionMonto + @MontoAbono 
                    WHERE Id = @TransaccionId";

                await connection.ExecuteAsync(queryUpdateTransacciones, new
                {
                    MontoAbono = abono.MontoAbono,
                    TransaccionId = transaccionId.Value
                });
            }
        }
        private async Task<int?> ObtenerTransaccionIdPorFecha(DateTime fechaAbono, int usuarioId)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var query = @"
                    SELECT TOP 1 Id 
                    FROM TransaccionesVenta 
                    WHERE FechaTransaccion = @FechaAbono AND UsuarioId = @UsuarioId
                    ORDER BY FechaTransaccion DESC";

                return await connection.QuerySingleOrDefaultAsync<int?>(query, new
                {
                    FechaAbono = fechaAbono,
                    UsuarioId = usuarioId
                });
            }
        }
        private async Task InsertarTransaccion(DateTime fechaTransaccion, int cuentaId, int usuarioId)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var query = @"
            INSERT INTO TransaccionesVenta (FechaTransaccion, CuentaId, VentaMonto, InversionMonto, UsuarioId) 
            VALUES (@FechaTransaccion, @CuentaId, @VentaMonto, @InversionMonto, @UsuarioId)";

                await connection.ExecuteAsync(query, new
                {
                    FechaTransaccion = fechaTransaccion,
                    CuentaId = cuentaId,
                    VentaMonto = 0,
                    InversionMonto = 0,
                    UsuarioId = usuarioId
                });
            }
        }
        private async Task<int?> ObtenerTipoCuentaId(int usuarioId)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var query = @"
                    SELECT TOP 1 Id 
                    FROM TipoCuenta 
                    WHERE UsuarioId = @UsuarioId";

                return await connection.QuerySingleOrDefaultAsync<int?>(query, new
                {
                    UsuarioId = usuarioId
                });
            }
        }


    }
}
