using Dapper;
using ProfitBook.Models;
using System.Data.SqlClient;

namespace ProfitBook.Servicios
{
    public interface IRepositorioTiposCuentas
    {
        Task Borrar(int id);
        Task Crear(TipoCuenta tipoCuenta);
        Task Editar(TipoCuenta tipoCuenta);
        Task<bool> Existe(string nombre, int usuarioId);
        Task<IEnumerable<TipoCuenta>> Obtener(int usuarioId);
        Task<TipoCuenta> ObtenerPorId(int id, int usuarioId);
    }
    public class RepositorioTiposCuentas: IRepositorioTiposCuentas
    {
        private readonly string connectionString;
        public RepositorioTiposCuentas(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        //modificar crear para usuarios
        public async Task Crear(TipoCuenta tipoCuenta)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>(@"
                        INSERT INTO TipoCuenta (Nombre, UsuarioId) 
                        VALUES (@Nombre, @UsuarioId);
                        SELECT SCOPE_IDENTITY();",
                new { tipoCuenta.Nombre, tipoCuenta.UsuarioId });
            tipoCuenta.Id = id;
        }
        //sin modificacion
        public async Task Editar(TipoCuenta tipoCuenta)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"UPDATE TipoCuenta
                    SET Nombre = @Nombre
                    WHERE Id = @Id", tipoCuenta);
        }

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("DELETE TipoCuenta WHERE Id = @Id", new { id });
        }

        //modifcamos
        public async Task<TipoCuenta> ObtenerPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<TipoCuenta>(@"
                                                                SELECT Id, Nombre
                                                                FROM TipoCuenta
                                                                WHERE Id = @Id AND UsuarioId = @UsuarioId",
                                                                new { id, usuarioId });
        }
        //Obtenemos la cuenta con usuario id
        public async Task<IEnumerable<TipoCuenta>> Obtener(int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<TipoCuenta>(@"SELECT Id, Nombre
                                                            FROM TipoCuenta
                                                            WHERE UsuarioId = @UsuarioId", new { usuarioId });
        }
        //verificamos que exista para que no se repita
        public async Task<bool> Existe(string nombre, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            var existe = await connection.QueryFirstOrDefaultAsync<int>(
                                    @"SELECT 1
                                    FROM TipoCuenta
                                    WHERE Nombre = @Nombre AND UsuarioId = @UsuarioId;",
                                    new { nombre, usuarioId });
            return existe == 1;
        }

    }
}
