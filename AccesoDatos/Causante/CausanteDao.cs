using AccesoDatos.Conexion;
using Dominio.DTO.Causante;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;

namespace AccesoDatos.Causante
{
    public class CausanteDao
    {
        //private readonly Conexion.ConexionOracle _conexion;
        private readonly ConexionOracle _conexion;

        //public CausanteDao()
        //{
        //    _conexion = new Conexion.ConexionOracle(new ConfigurationBuilder().AddJsonFile("appsettings.json").Build());
        //}

        public CausanteDao(ConexionOracle conexion)   // 👈 DI te lo entrega ya configurado
        {
            _conexion = conexion;
        }

        //Inbsertar 
        public bool InsertarCausante(CausanteRequestDto dto)
        {
            using var conn = _conexion.ObtenerConexion();
            conn.Open();
            var cmd = conn.CreateCommand();

            string nombreCompleto = $"{dto.Nombre1} {dto.Nombre2 ?? ""} {dto.Apellido1} {dto.Apellido2 ?? ""}".Trim();

            cmd.CommandText = @"INSERT INTO CAUSANTE (
                    CAUAFI, CAUNO1, CAUNO2, CAUAP1, CAUAP2, CAUNOC, CAUDIR, CAUDPI, CAUSTS, CAUUDG) 
                    VALUES (:afi, :no1, :no2, :ap1, :ap2, :noc, :dir, :dpi, :sts, :usr)";

            cmd.Parameters.Add(new OracleParameter("afi", dto.Afi ?? ""));
            cmd.Parameters.Add(new OracleParameter("no1", dto.Nombre1 ?? ""));
            cmd.Parameters.Add(new OracleParameter("no2", dto.Nombre2 ?? ""));
            cmd.Parameters.Add(new OracleParameter("ap1", dto.Apellido1 ?? ""));
            cmd.Parameters.Add(new OracleParameter("ap2", dto.Apellido2 ?? ""));
            //cmd.Parameters.Add(new OracleParameter("noc", dto.NombreCompleto ?? ""));
            cmd.Parameters.Add(new OracleParameter("noc", nombreCompleto));
            cmd.Parameters.Add(new OracleParameter("dir", dto.Direccion ?? ""));
            cmd.Parameters.Add(new OracleParameter("dpi", dto.Dpi ?? ""));
            cmd.Parameters.Add(new OracleParameter("sts", dto.Estado ?? ""));
            cmd.Parameters.Add(new OracleParameter("usr", dto.Usuario ?? ""));
            return cmd.ExecuteNonQuery() > 0;
        }

        //Obtener todos
        // Antes: ListarCausantes() sin filtro
        // Ahora: filtro opcional por afiliación, nombre completo o código (caso)
        public List<CausanteResponseDto> ListarCausantes(string? q = null)
        {
            var lista = new List<CausanteResponseDto>();
            using var conn = _conexion.ObtenerConexion();
            conn.Open();

            var cmd = conn.CreateCommand();

            //Query base 
            var sql = @"
                SELECT CAUCOD, CAUAFI, CAUNOC, CAUSTS,
                       TO_CHAR(CAUFDG, 'DD-MM-YYYY') AS CAUFDG_FMT
                FROM   CAUSANTE
                WHERE  1=1";


            // WHERE dinámico si viene 'q'
            if (!string.IsNullOrWhiteSpace(q))
            {
                // Si q es numérico => coincidencia EXACTA por CAUCOD
                if (int.TryParse(q.Trim(), out var qNum))
                {
                    sql += " AND CAUCOD = :qNum";
                    var pNum = new OracleParameter("qNum", OracleDbType.Int32) { Value = qNum };
                    cmd.Parameters.Add(pNum);
                }
                else
                {
                    // Si q NO es numérico => búsqueda amplia (nombre contiene, afi/caso por prefijo)
                    sql += @"
           AND (
                UPPER(CAUNOC)    LIKE UPPER(:qLike)
             OR CAUAFI          LIKE :qPrefix
             OR TO_CHAR(CAUCOD) LIKE :qPrefix
           )";
                    cmd.Parameters.Add(new OracleParameter("qLike", $"%{q}%"));
                    cmd.Parameters.Add(new OracleParameter("qPrefix", $"{q}%"));
                }
            }

            sql += " ORDER BY CAUCOD"; // orden consistente

            cmd.CommandText = sql;

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var item = new CausanteResponseDto
                {
                    Codigo = Convert.ToInt32(reader["CAUCOD"]),
                    Afi = reader["CAUAFI"]?.ToString() ?? "",
                    NombreCompleto = reader["CAUNOC"]?.ToString() ?? "",
                    Estado = reader["CAUSTS"]?.ToString() ?? "",
                    FechaRegistro = reader["CAUFDG_FMT"]?.ToString() ?? ""
                };
                lista.Add(item);
            }
            return lista;
        }//Fin obtener

        //Actualizar
        public bool ActualizarCausante(int id, CausanteRequestDto dto)
        {
            using var conn = _conexion.ObtenerConexion();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
        UPDATE CAUSANTE SET
            CAUAFI = :afi,
            CAUNO1 = :no1,
            CAUNO2 = :no2,
            CAUAP1 = :ap1,
            CAUAP2 = :ap2,
            CAUNOC = :noc,
            CAUDIR = :dir,
            CAUDPI = :dpi,
            CAUSTS = :sts,
            CAUUDG = :usr
        WHERE CAUCOD = :id";

            string nombreCompleto = $"{dto.Nombre1} {dto.Nombre2 ?? ""} {dto.Apellido1} {dto.Apellido2 ?? ""}".Trim();

            cmd.Parameters.Add(new OracleParameter("afi", dto.Afi));
            cmd.Parameters.Add(new OracleParameter("no1", dto.Nombre1));
            cmd.Parameters.Add(new OracleParameter("no2", dto.Nombre2 ?? ""));
            cmd.Parameters.Add(new OracleParameter("ap1", dto.Apellido1 ?? ""));
            cmd.Parameters.Add(new OracleParameter("ap2", dto.Apellido2 ?? ""));
            cmd.Parameters.Add(new OracleParameter("noc", nombreCompleto));
            cmd.Parameters.Add(new OracleParameter("dir", dto.Direccion));
            cmd.Parameters.Add(new OracleParameter("dpi", dto.Dpi));
            cmd.Parameters.Add(new OracleParameter("sts", dto.Estado));
            cmd.Parameters.Add(new OracleParameter("usr", dto.Usuario));
            cmd.Parameters.Add(new OracleParameter("id", id));

            int rowsAffected = cmd.ExecuteNonQuery();
            return rowsAffected > 0;
        }//Fin Actualizar

        //Obtener por ID
        public CausanteRequestDto ObtenerCausantePorId(int id)
        {
            using var conn = _conexion.ObtenerConexion();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
        SELECT CAUAFI, CAUNO1, CAUNO2, CAUAP1, CAUAP2, CAUDIR, CAUDPI, CAUSTS, CAUUDG
        FROM CAUSANTE
        WHERE CAUCOD = :id";
            cmd.Parameters.Add(new OracleParameter("id", id));

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new CausanteRequestDto
                {
                    Afi = reader["CAUAFI"].ToString(),
                    Nombre1 = reader["CAUNO1"].ToString(),
                    Nombre2 = reader["CAUNO2"].ToString(),
                    Apellido1 = reader["CAUAP1"].ToString(),
                    Apellido2 = reader["CAUAP2"].ToString(),
                    Direccion = reader["CAUDIR"].ToString(),
                    Dpi = reader["CAUDPI"].ToString(),
                    Estado = reader["CAUSTS"].ToString(),
                    Usuario = reader["CAUUDG"].ToString()
                };
            }
            return null;
        }//Fin obtener por ID

        //Eliminar
        public bool EliminarCausante(int id)
        {
            using var conn = _conexion.ObtenerConexion();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM CAUSANTE WHERE CAUCOD = :id";
            cmd.Parameters.Add(new OracleParameter("id", id));

            int rowsAffected = cmd.ExecuteNonQuery();
            return rowsAffected > 0;
        }//Fin eliminar
    }
}
