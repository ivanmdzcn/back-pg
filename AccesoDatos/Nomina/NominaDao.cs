using AccesoDatos.Conexion;
using Dominio.DTO.Nomina;
using Oracle.ManagedDataAccess.Client;

namespace AccesoDatos.Nomina
{
    public class NominaDao
    {
        private readonly ConexionOracle _conexion;
        public NominaDao(ConexionOracle conexion) { _conexion = conexion; }

        // INSERT encabezado, retorna NOMCOD generado por trigger (RETURNING)
        public int Crear(NominaCreateDto dto, string usuarioCreador)
        {
            using var cn = _conexion.ObtenerConexion();
            cn.Open();

            using var cmd = cn.CreateCommand();
            cmd.CommandText = @"
        INSERT INTO NOMINA (NOMTIP, NOMSTD, NOMFDI, NOMFDF, NOMUDC, NOMFDG, NOMHDG)
        VALUES (:tip, 'B', :fdi, :fdf, :udc, SYSDATE, TO_CHAR(SYSDATE,'HH24:MI:SS'))
        RETURNING NOMCOD INTO :out_id";

            cmd.Parameters.Add(new OracleParameter("tip", dto.Nomtip));
            cmd.Parameters.Add(new OracleParameter("fdi", dto.Nomfdi));
            cmd.Parameters.Add(new OracleParameter("fdf", dto.Nomfdf));
            cmd.Parameters.Add(new OracleParameter("udc", usuarioCreador));

            var outId = new OracleParameter("out_id", OracleDbType.Int32)
            { Direction = System.Data.ParameterDirection.Output };
            cmd.Parameters.Add(outId);

            cmd.ExecuteNonQuery();
            return Convert.ToInt32(outId.Value.ToString());
        }

        // GET uno (encabezado)
        public NominaHdrDto? ObtenerHdr(int nomcod)
        {
            using var cn = _conexion.ObtenerConexion();
            cn.Open();

            using var cmd = cn.CreateCommand();
            cmd.CommandText = @"
                SELECT NOMCOD, NOMTIP, NOMSTD, NOMFDI, NOMFDF, NOMUDC, NOMUDA, NOMFAU
                FROM NOMINA
                WHERE NOMCOD = :id";
            cmd.Parameters.Add(new OracleParameter("id", nomcod));

            using var rd = cmd.ExecuteReader();
            if (!rd.Read()) return null;

            return new NominaHdrDto
            {
                Nomcod = Convert.ToInt32(rd["NOMCOD"]),
                Nomtip = rd["NOMTIP"].ToString()!,
                Nomstd = rd["NOMSTD"].ToString()!,
                Nomfdi = Convert.ToDateTime(rd["NOMFDI"]),
                Nomfdf = Convert.ToDateTime(rd["NOMFDF"]),
                Nomudc = rd["NOMUDC"].ToString()!,
                Nomuda = rd["NOMUDA"] as string,
                Nomfau = rd["NOMFAU"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(rd["NOMFAU"])
            };
        }

        // LIST encabezados (admin ve todos; operador solo los suyos)
        public List<NominaHdrDto> Listar(string usuario, bool esAdmin)
        {
            var lista = new List<NominaHdrDto>();

            using var cn = _conexion.ObtenerConexion();
            cn.Open();

            using var cmd = cn.CreateCommand();
            if (esAdmin)
            {
                cmd.CommandText = @"
                    SELECT NOMCOD, NOMTIP, NOMSTD, NOMFDI, NOMFDF, NOMUDC, NOMUDA, NOMFAU
                    FROM NOMINA
                    ORDER BY NOMCOD DESC";
            }
            else
            {
                cmd.CommandText = @"
                    SELECT NOMCOD, NOMTIP, NOMSTD, NOMFDI, NOMFDF, NOMUDC, NOMUDA, NOMFAU
                    FROM NOMINA
                    WHERE NOMUDC = :usr
                    ORDER BY NOMCOD DESC";
                cmd.Parameters.Add(new OracleParameter("usr", usuario));
            }

            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                lista.Add(new NominaHdrDto
                {
                    Nomcod = Convert.ToInt32(rd["NOMCOD"]),
                    Nomtip = rd["NOMTIP"].ToString()!,
                    Nomstd = rd["NOMSTD"].ToString()!,
                    Nomfdi = Convert.ToDateTime(rd["NOMFDI"]),
                    Nomfdf = Convert.ToDateTime(rd["NOMFDF"]),
                    Nomudc = rd["NOMUDC"].ToString()!,
                    Nomuda = rd["NOMUDA"] as string,
                    Nomfau = rd["NOMFAU"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(rd["NOMFAU"])
                });
            }
            return lista;
        }

        // UPDATE estado -> Autorizar (A): solo si estaba en B
        public bool Autorizar(int nomcod, string usuarioAdmin)
        {
            using var cn = _conexion.ObtenerConexion();
            cn.Open();

            using var cmd = cn.CreateCommand();
            cmd.CommandText = @"
                UPDATE NOMINA
                SET NOMSTD = 'A',
                    NOMUDA = :admin,
                    NOMFAU = SYSDATE
                WHERE NOMCOD = :id
                  AND NOMSTD = 'B'";
            cmd.Parameters.Add(new OracleParameter("admin", usuarioAdmin));
            cmd.Parameters.Add(new OracleParameter("id", nomcod));

            return cmd.ExecuteNonQuery() > 0;
        }

        // UPDATE estado -> Cancelar (C): desde B o A
        public bool Cancelar(int nomcod, string usuarioAdmin)
        {
            using var cn = _conexion.ObtenerConexion();
            cn.Open();

            using var cmd = cn.CreateCommand();
            cmd.CommandText = @"
                UPDATE NOMINA
                SET NOMSTD = 'C',
                    NOMUDA = :admin,
                    NOMFAU = SYSDATE
                WHERE NOMCOD = :id
                  AND NOMSTD IN ('B','A')";
            cmd.Parameters.Add(new OracleParameter("admin", usuarioAdmin));
            cmd.Parameters.Add(new OracleParameter("id", nomcod));

            return cmd.ExecuteNonQuery() > 0;
        }

        // (Opcional) UPDATE rango de fechas en borrador
        public bool ActualizarFechas(int nomcod, DateTime fdi, DateTime fdf)
        {
            using var cn = _conexion.ObtenerConexion();
            cn.Open();

            using var cmd = cn.CreateCommand();
            cmd.CommandText = @"
                UPDATE NOMINA
                SET NOMFDI = :fdi, NOMFDF = :fdf
                WHERE NOMCOD = :id
                  AND NOMSTD = 'B'";
            cmd.Parameters.Add(new OracleParameter("fdi", fdi));
            cmd.Parameters.Add(new OracleParameter("fdf", fdf));
            cmd.Parameters.Add(new OracleParameter("id", nomcod));

            return cmd.ExecuteNonQuery() > 0;
        }

        // ===========================
        // === DETALLE DE NÓMINA ====
        // ===========================



    }
}
