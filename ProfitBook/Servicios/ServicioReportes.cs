using Dapper;
using ProfitBook.Models;
using System.Data.SqlClient;

namespace ProfitBook.Servicios
{
    public interface IServicioReportes
    {
        Task<EstadisticasMensualesViewModel> ObtenerEstadisticasMensuales(int usuarioId, int mesActual, int añoActual);
        DateNavigationViewModel ObtenerNavegacionFechas(int mesActual, int añoActual, string actionName);
        Task<RecordMesViewModel> ObtenerRecordsDeVentas(int usuarioId);
        Task<IEnumerable<ResumenVentasMensualViewModel>> ObtenerResumenVentasPorAnio(int año, int usuarioId);
        Task<IEnumerable<VentasSemanalViewModel>> ObtenerResumenVentasPorSemana(int mes, int año, int usuarioId);
    }
    public class ServicioReportes : IServicioReportes
    {
        private readonly string connectionString;

        public ServicioReportes(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        public DateNavigationViewModel ObtenerNavegacionFechas(int mesActual, int añoActual, string actionName)
        {
            int mesAnterior = mesActual == 1 ? 12 : mesActual - 1;
            int añoAnterior = mesActual == 1 ? añoActual - 1 : añoActual;

            int mesPosterior = mesActual == 12 ? 1 : mesActual + 1;
            int añoPosterior = mesActual == 12 ? añoActual + 1 : añoActual;

            var fechaActual = new DateTime(añoActual, mesActual, 1);

            return new DateNavigationViewModel
            {
                MesActual = mesActual,
                AñoActual = añoActual,
                MesAnterior = mesAnterior,
                AñoAnterior = añoAnterior,
                MesPosterior = mesPosterior,
                AñoPosterior = añoPosterior,
                FechaActual = fechaActual,
                ActionName = actionName
            };
        }

        public async Task<IEnumerable<VentasSemanalViewModel>> ObtenerResumenVentasPorSemana(int mes, int año, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            string sqlQuery = @"
                WITH Semanas AS (
                    SELECT 
                        1 AS Semana, 
                        DATEFROMPARTS(@año, @mes, 1) AS FechaInicioSemana,
                        CASE 
                            WHEN DATEADD(DAY, 6, DATEFROMPARTS(@año, @mes, 1)) > EOMONTH(DATEFROMPARTS(@año, @mes, 1))
                            THEN EOMONTH(DATEFROMPARTS(@año, @mes, 1))
                            ELSE DATEADD(DAY, 6, DATEFROMPARTS(@año, @mes, 1))
                        END AS FechaFinSemana
                    UNION ALL
                    SELECT 
                        Semana + 1,
                        DATEADD(DAY, 7, FechaInicioSemana),
                        CASE 
                            WHEN DATEADD(DAY, 13, FechaInicioSemana) > EOMONTH(DATEFROMPARTS(@año, @mes, 1))
                            THEN EOMONTH(DATEFROMPARTS(@año, @mes, 1))
                            ELSE DATEADD(DAY, 13, FechaInicioSemana)
                        END AS FechaFinSemana
                    FROM Semanas
                    WHERE DATEADD(DAY, 7, FechaInicioSemana) <= EOMONTH(DATEFROMPARTS(@año, @mes, 1))
                )
                SELECT 
                    s.Semana,
                    s.FechaInicioSemana,
                    s.FechaFinSemana,
                    COALESCE(SUM(tv.VentaMonto), 0) AS Ventas,
                    COALESCE(SUM(tv.InversionMonto), 0) AS Inversion,
                    COALESCE(SUM(tv.VentaMonto - tv.InversionMonto), 0) AS Ganancia
                FROM Semanas s
                LEFT JOIN TransaccionesVenta tv 
                    ON tv.FechaTransaccion BETWEEN s.FechaInicioSemana AND s.FechaFinSemana
                    AND tv.UsuarioId = @usuarioId
                    AND MONTH(tv.FechaTransaccion) = @mes
                    AND YEAR(tv.FechaTransaccion) = @año
                GROUP BY s.Semana, s.FechaInicioSemana, s.FechaFinSemana
                ORDER BY s.Semana
                OPTION (MAXRECURSION 0);";

            return await connection.QueryAsync<VentasSemanalViewModel>(sqlQuery, new { mes, año, usuarioId });
        }

        public async Task<IEnumerable<ResumenVentasMensualViewModel>> ObtenerResumenVentasPorAnio(int año, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            string sqlQuery = @"
                WITH Meses AS (
                    SELECT 1 AS Mes, FORMAT(DATEFROMPARTS(@año, 1, 1), 'MMMM', 'es-ES') AS NombreMes
                    UNION ALL
                    SELECT Mes + 1, FORMAT(DATEFROMPARTS(@año, Mes + 1, 1), 'MMMM', 'es-ES')
                    FROM Meses
                    WHERE Mes < 12
                )
                SELECT 
                    m.NombreMes AS Mes,
                    COALESCE(SUM(tv.VentaMonto), 0) AS Ventas,
                    COALESCE(SUM(tv.InversionMonto), 0) AS Inversion,
                    COALESCE(SUM(tv.VentaMonto - tv.InversionMonto), 0) AS Ganancia
                FROM Meses m
                LEFT JOIN TransaccionesVenta tv 
                    ON MONTH(tv.FechaTransaccion) = m.Mes AND YEAR(tv.FechaTransaccion) = @año
                    AND tv.UsuarioId = @usuarioId
                GROUP BY m.NombreMes, m.Mes
                ORDER BY m.Mes
                OPTION (MAXRECURSION 0);";

            return await connection.QueryAsync<ResumenVentasMensualViewModel>(sqlQuery, new { año, usuarioId });
        }

        public async Task<EstadisticasMensualesViewModel> ObtenerEstadisticasMensuales(int usuarioId, int mesActual, int añoActual)
        {
            using var connection = new SqlConnection(connectionString);

            string sqlQuery = @"
                SELECT 
                    COUNT(DISTINCT CAST(FechaTransaccion AS DATE)) AS DiasTrabajados, 
                    COALESCE(SUM(VentaMonto), 0) AS VentasTotales, 
                    COALESCE(SUM(InversionMonto), 0) AS InversionesTotales, 
                    COALESCE(SUM(VentaMonto - InversionMonto), 0) AS GananciasTotales
                FROM TransaccionesVenta
                WHERE UsuarioId = @usuarioId 
                AND MONTH(FechaTransaccion) = @mes 
                AND YEAR(FechaTransaccion) = @año;";

            var estadisticas = await connection.QueryFirstOrDefaultAsync<EstadisticasMensualesViewModel>(
                sqlQuery,
                new { usuarioId, mes = mesActual, año = añoActual }
            );

            return estadisticas;
        }

        public async Task<RecordMesViewModel> ObtenerRecordsDeVentas(int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);

            // Obtener récord mensual
            string sqlQueryMes = @"
                SELECT TOP 1 
                    FORMAT(FechaTransaccion, 'MMMM', 'es-ES') AS Mes,
                    SUM(VentaMonto) AS TotalVentas
                FROM TransaccionesVenta
                WHERE UsuarioId = @usuarioId
                GROUP BY FORMAT(FechaTransaccion, 'MMMM', 'es-ES'), YEAR(FechaTransaccion)
                ORDER BY SUM(VentaMonto) DESC;";

            var recordMes = await connection.QueryFirstOrDefaultAsync<RecordMesViewModel>(sqlQueryMes, new { usuarioId });
            return recordMes;
        }
    }
}
