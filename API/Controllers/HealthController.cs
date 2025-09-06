using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using AccesoDatos.Conexion;

namespace API.Controllers
{
    [ApiController]
    [Route("")]
    public class HealthController : ControllerBase
    {
        private readonly ConexionOracle _conexion;

        public HealthController(ConexionOracle conexion)
        {
            _conexion = conexion;
        }

        /// <summary>
        /// Salud básica de la API (no toca la BD).
        /// </summary>
        [HttpGet("health")]
        public IActionResult Health() => Ok(new { status = "OK" });

        /// <summary>
        /// Verifica conectividad real a Oracle (abre conexión y ejecuta SELECT 1 FROM DUAL).
        /// </summary>
        [HttpGet("db-ping")]
        public async Task<IActionResult> DbPing(CancellationToken ct)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                using var conn = (OracleConnection)_conexion.ObtenerConexion();
                await conn.OpenAsync(ct);

                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT 1 FROM DUAL";
                var result = await cmd.ExecuteScalarAsync(ct);

                sw.Stop();
                return Ok(new
                {
                    db = "OK",
                    elapsedMs = sw.ElapsedMilliseconds,
                    result,
                    serverVersion = conn.ServerVersion,
                    dataSource = conn.DataSource
                });
            }
            catch (OracleException ox)
            {
                sw.Stop();
                return StatusCode(500, new
                {
                    db = "DOWN",
                    elapsedMs = sw.ElapsedMilliseconds,
                    error = ox.Message,
                    code = ox.Number
                });
            }
            catch (System.Exception ex)
            {
                sw.Stop();
                return StatusCode(500, new
                {
                    db = "DOWN",
                    elapsedMs = sw.ElapsedMilliseconds,
                    error = ex.Message
                });
            }
        }
    }
}
