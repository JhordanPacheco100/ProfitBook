using Dapper;
using ProfitBook.Models;
using System.Data.SqlClient;

namespace ProfitBook.Servicios
{
    public interface IRepositorioProductos
    {
        Task Borrar(int id);
        Task Crear(Productos producto);
        Task Editar(Productos producto);
        Task<bool> Existe(string nombre, int usuarioId);
        Task<Productos> ObtenerPorId(int id, int usuarioId);
        Task<IEnumerable<Productos>> ObtenerProductos(int usuarioId);
    }
    public class RepositorioProductos: IRepositorioProductos
    {
        private readonly string connectionString;
        public RepositorioProductos(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<IEnumerable<Productos>> ObtenerProductos(int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Productos>(@"
                SELECT Id, Nombre,TipoOperacionId FROM Productos WHERE UsuarioId = @UsuarioId",
                        new { usuarioId });
        }
        // Obtener producto por ID y UsuarioId
        public async Task<Productos> ObtenerPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<Productos>(@"
                SELECT Id, Nombre, TipoOperacionId FROM Productos
                WHERE Id = @Id AND UsuarioId = @UsuarioId;",
                    new { id, usuarioId });
        }

        public async Task Crear(Productos producto)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>(@"
                INSERT INTO Productos (Nombre, TipoOperacionId, UsuarioId) 
                VALUES (@Nombre, @TipoOperacionId, @UsuarioId);
                SELECT SCOPE_IDENTITY();",
                new { producto.Nombre, producto.TipoOperacionId, producto.UsuarioId });
            producto.Id = id;
        }
        public async Task<bool> Existe(string nombre, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            var existe = await connection.QueryFirstOrDefaultAsync<int>(@"
                SELECT 1
                FROM Productos
                WHERE Nombre = @Nombre AND UsuarioId = @UsuarioId;",
                new { nombre, usuarioId });

            return existe == 1;
        }
        public async Task Editar(Productos producto)
        {
            using var connession = new SqlConnection(connectionString);
            await connession.ExecuteAsync(@"UPDATE Productos
                    SET Nombre = @Nombre, TipoOperacionId = @TipoOperacionID
                    WHERE Id = @Id", producto);
        }

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("DELETE Productos WHERE Id = @Id", new { id });
        }

    }
}
