using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Servicios
{
    public interface IRepositorioTiposCuentas
    {
        Task Actualizar(TipoCuenta tipoCuenta);
        Task Borrar(int id);
        Task Crear(TipoCuenta tipoCuenta);
        Task<bool> Existe(string nombre, int usuarioId);
        Task<List<TipoCuenta>> Obtener(int usuarioId);
        Task<TipoCuenta> ObtenerPorId(int id, int usuarioId);
        Task Ordenar(IEnumerable<TipoCuenta> tipoCuentasOrdenados);
    }
    public class RepositorioTiposCuentas : IRepositorioTiposCuentas
    {
        private readonly string connectionString;
        public RepositorioTiposCuentas(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(TipoCuenta tipoCuenta)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>("spInsertar_TiposCuentas",
                new { usuarioId= tipoCuenta.UsuarioId,
                    nombre=tipoCuenta.Nombre},
                commandType:System.Data.CommandType.StoredProcedure);
            tipoCuenta.Id = id; //Manda el id de la consulta al campo id del modelo
        }

        public async Task<bool> Existe(string nombre, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            var existe = await connection.QueryFirstOrDefaultAsync<int>(@"SELECT 1
                                                                    FROM TiposCuentas
                                                                    WHERE Nombre = @Nombre and UsuarioId = @UsuarioId;"
                                                                    , new {nombre,usuarioId});
            return existe == 1;
        }

        public async Task<List<TipoCuenta>> Obtener(int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return (List<TipoCuenta>)await connection.QueryAsync<TipoCuenta>(@"SELECT Id, Nombre,Orden
                                                            FROM TiposCuentas 
                                                            WHERE UsuarioId= @UsuarioId Order by Orden", new {usuarioId});
        }

        public async Task Actualizar(TipoCuenta tipoCuenta)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"UPDATE TiposCuentas
                                           SET Nombre=@Nombre
                                           WHERE Id=@Id", tipoCuenta);
        }

        public async Task<TipoCuenta> ObtenerPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection( connectionString);
            return await connection.QueryFirstOrDefaultAsync<TipoCuenta>(@"SELECT Id, Nombre, Orden
                                                                    FROM TiposCuentas
                                                                    WHERE Id=@Id AND UsuarioId = @UsuarioId",
                                                                    new {id,usuarioId});
        }

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("DELETE TiposCuentas WHERE Id = @Id", new { id });

        }

        public async Task Ordenar(IEnumerable<TipoCuenta> tipoCuentasOrdenados)
        {
            using var connection = new SqlConnection(connectionString);
            var query = "UPDATE TiposCuentas SET Orden = @Orden WHERE Id = @Id";

            await connection.ExecuteAsync(query,tipoCuentasOrdenados);
        }
    }
}
