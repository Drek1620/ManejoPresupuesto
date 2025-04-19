using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Servicios
{
    public interface IRepositorioTransacciones
    {
        Task Actualizar(Transaccion transaccion, decimal montoAnterior, int cuentaAnterior);
        Task Borrar(int id);
        Task Crear(Transaccion transaccion);
        Task<IEnumerable<Transaccion>> ObtenerPorCuentaId(ObtenerTransaccionesPorCuenta model);
        Task<Transaccion> ObtenerPorId(int id, int usuarioId);
        Task<IEnumerable<ResultadoObtenerPorMes>> ObtenerPorMes(int usuarioId, int año);
        Task<IEnumerable<ResultadoObtenerPorSemana>> ObtenerPorSemana(ParametroObtenerTransaccionesPorUsuario model);
        Task<IEnumerable<Transaccion>> ObtenerPorUsuarioId(ParametroObtenerTransaccionesPorUsuario model);
    }
    public class RepositorioTransacciones : IRepositorioTransacciones
    {
        private readonly string connectionString;
        public RepositorioTransacciones(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(Transaccion transaccion)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>(@"spTransaccionesInsertar",
                new {transaccion.UsuarioId, transaccion.FechaTransaccion, transaccion.Monto, transaccion.CategoriaId,
                transaccion.CuentaId, transaccion.Nota}, commandType: System.Data.CommandType.StoredProcedure);

            transaccion.Id = id;
        }

        public async Task<Transaccion> ObtenerPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<Transaccion>(@"SELECT * FROM Transacciones
                INNER JOIN Categorias cat
                ON cat.Id = Transacciones.CategoriaId
                WHERE Transacciones.Id = @Id AND Transacciones.UsuarioId = @UsuarioId", new {id, usuarioId});
        }

        public async Task Actualizar (Transaccion transaccion, decimal montoAnterior, int cuentaAnteriorId)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("spTransaccionesActualizar",
                new
                {
                    transaccion.Id,
                    transaccion.FechaTransaccion,
                    transaccion.Monto,
                    transaccion.CuentaId,
                    transaccion.CategoriaId,
                    transaccion.Nota,
                    montoAnterior,
                    cuentaAnteriorId
                }, commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<ResultadoObtenerPorSemana>> ObtenerPorSemana(ParametroObtenerTransaccionesPorUsuario model)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<ResultadoObtenerPorSemana>(@"SELECT DATEDIFF(d, @fechaInicio, FechaTransaccion) / 7 + 1 AS Semana,
              SUM(Monto) as Monto , cat.TipoOperacionId
            FROM Transacciones
            INNER JOIN Categorias cat
            On cat.Id = Transacciones.CategoriaId
            WHERE Transacciones.UsuarioId = @usuarioId AND
            FechaTransaccion BETWEEN @fechaInicio AND @fechaFin
            GROUP BY DATEDIFF(d, @fechaInicio, FechaTransaccion)/ 7 + 1, cat.TipoOperacionId", model);
        }

        public async Task< IEnumerable<ResultadoObtenerPorMes>> ObtenerPorMes(int usuarioId, int año)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<ResultadoObtenerPorMes>(@"SELECT MONTH(FechaTransaccion) AS Mes,
            SUM(Monto) AS Monto, cat.TipoOperacionId
            FROM Transacciones
            INNER JOIN Categorias cat
                ON cat.Id = Transacciones.CategoriaId
            WHERE Transacciones.UsuarioId = @usuarioId AND YEAR(FechaTransaccion) = @Año
            GROUP BY MONTH(FechaTransaccion), cat.TipoOperacionId", new { usuarioId, año});
        }
        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("spTransaccionBorrar", new
            {
                id
            },commandType:System.Data.CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<Transaccion>> ObtenerPorCuentaId(ObtenerTransaccionesPorCuenta model)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Transaccion>(@"SELECT t.Id, t.Monto, t.FechaTransaccion, c.Nombre as Categoria, cu.Nombre as Cuenta, c.TipoOperacionId
            FROM Transacciones t
            INNER JOIN Categorias c ON c.Id = t.CategoriaId
            INNER JOIN Cuentas cu ON cu.Id = t.CuentaId
            WHERE t.CuentaId = @CuentaId AND t.UsuarioId = @UsuarioId AND FechaTransaccion BETWEEN @FechaInicio AND @FechaFin", model);
        }

        public async Task<IEnumerable<Transaccion>> ObtenerPorUsuarioId(ParametroObtenerTransaccionesPorUsuario model)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Transaccion>(@"SELECT t.Id, t.Monto, t.FechaTransaccion, c.Nombre as Categoria, cu.Nombre as Cuenta, c.TipoOperacionId, Nota
            FROM Transacciones t
            INNER JOIN Categorias c ON c.Id = t.CategoriaId
            INNER JOIN Cuentas cu ON cu.Id = t.CuentaId
            WHERE t.UsuarioId = @UsuarioId AND FechaTransaccion BETWEEN @FechaInicio AND @FechaFin
            ORDER BY t.FechaTransaccion DESC", model);
        }
    }
}
