using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace AccesoDatos.Conexion
{
    //internal class ConexionOracle
    public class ConexionOracle
    {
        private readonly string _connectionString;
        public ConexionOracle(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("OracleConnection");
        }

        public IDbConnection ObtenerConexion()
        {
            return new OracleConnection(_connectionString);
        }
    }
}
