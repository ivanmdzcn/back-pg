using AccesoDatos.Conexion;
using Dominio.DTO.Beneficiario;
using Oracle.ManagedDataAccess.Client;

namespace AccesoDatos.Beneficiario
{
    public class BeneficiarioDao
    {
        private readonly ConexionOracle _conexion;
        public BeneficiarioDao(ConexionOracle conexion) { _conexion = conexion; }

        // === Listar TODOS los campos + descripciones de catálogos ===
        public List<BeneficiarioResponseDto> ListarPorCausante(int causanteId)
        {
            var lista = new List<BeneficiarioResponseDto>();

            using var cn = _conexion.ObtenerConexion();
            cn.Open();

            using var cmd = cn.CreateCommand();
            cmd.CommandText = @"
                SELECT b.BENCAU, b.BENCOD, b.BENNO1, b.BENNO2, b.BENAP1, b.BENAP2,
                       b.BENNOC, b.BENRES, b.BENDPI, b.BENPAR, b.BENMON, b.BENSIT,
                       b.BENTRM, b.BENATE,
                       TO_CHAR(b.BENFDG,'DD-MM-YYYY') AS FDG, b.BENHDG, b.BENUDG,
                       p.PARDSC AS PARENTESCO_DESC,
                       s.SITDSC AS SITUACION_DESC,
                       t.TRMDSC AS TERMINACION_DESC
                FROM   BENEFICIARIO b
                LEFT JOIN PARENTESCO p ON p.PARCOD = b.BENPAR
                LEFT JOIN SITUACION  s ON s.SITCOD = b.BENSIT
                LEFT JOIN TERMINACION t ON t.TRMCOD = b.BENTRM
                WHERE  b.BENCAU = :p_causante
                ORDER  BY b.BENCOD";
            cmd.Parameters.Add(new OracleParameter("p_causante", causanteId));

            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                lista.Add(new BeneficiarioResponseDto
                {
                    Bencau = Convert.ToInt32(rd["BENCAU"]),
                    Bencod = Convert.ToInt32(rd["BENCOD"]),
                    Benno1 = rd["BENNO1"]?.ToString() ?? "",
                    Benno2 = rd["BENNO2"]?.ToString() ?? "",
                    Benap1 = rd["BENAP1"]?.ToString() ?? "",
                    Benap2 = rd["BENAP2"]?.ToString() ?? "",
                    Bennoc = rd["BENNOC"]?.ToString() ?? "",
                    Benres = rd["BENRES"]?.ToString() ?? "",
                    Bendpi = rd["BENDPI"]?.ToString() ?? "",
                    Benpar = rd["BENPAR"]?.ToString() ?? "",
                    Benmon = rd["BENMON"] == DBNull.Value ? 0 : Convert.ToDecimal(rd["BENMON"]),
                    Bensit = rd["BENSIT"]?.ToString() ?? "",
                    Bentrm = rd["BENTRM"] == DBNull.Value ? (int?)null : Convert.ToInt32(rd["BENTRM"]),
                    Benate = rd["BENATE"] == DBNull.Value ? (int?)null : Convert.ToInt32(rd["BENATE"]),
                    Fdg = rd["FDG"]?.ToString() ?? "",
                    Hdg = rd["BENHDG"]?.ToString() ?? "",
                    Benudg = rd["BENUDG"]?.ToString() ?? "",
                    ParentescoDesc = rd["PARENTESCO_DESC"]?.ToString(),
                    SituacionDesc = rd["SITUACION_DESC"]?.ToString(),
                    TerminacionDesc = rd["TERMINACION_DESC"]?.ToString()
                });
            }
            return lista;
        }

        // === Obtener UNO por PK compuesta ===
        public BeneficiarioEditDto? ObtenerUno(int bencau, int bencod)
        {
            using var cn = _conexion.ObtenerConexion();
            cn.Open();

            using var cmd = cn.CreateCommand();
            cmd.CommandText = @"
                SELECT BENCAU, BENCOD, BENNO1, BENNO2, BENAP1, BENAP2, BENRES, BENDPI,
                       BENPAR, BENMON, BENSIT, BENTRM, BENATE, BENUDG
                FROM BENEFICIARIO
                WHERE BENCAU = :cau AND BENCOD = :cod";
            cmd.Parameters.Add(new OracleParameter("cau", bencau));
            cmd.Parameters.Add(new OracleParameter("cod", bencod));

            using var rd = cmd.ExecuteReader();
            if (!rd.Read()) return null;

            return new BeneficiarioEditDto
            {
                Bencau = Convert.ToInt32(rd["BENCAU"]),
                Bencod = Convert.ToInt32(rd["BENCOD"]),
                Benno1 = rd["BENNO1"]?.ToString() ?? "",
                Benno2 = rd["BENNO2"] as string,
                Benap1 = rd["BENAP1"]?.ToString() ?? "",
                Benap2 = rd["BENAP2"] as string,
                Benres = rd["BENRES"]?.ToString() ?? "",
                Bendpi = rd["BENDPI"]?.ToString() ?? "",
                Benpar = rd["BENPAR"]?.ToString() ?? "",
                Benmon = rd["BENMON"] == DBNull.Value ? 0 : Convert.ToDecimal(rd["BENMON"]),
                Bensit = rd["BENSIT"] as string,
                Bentrm = rd["BENTRM"] == DBNull.Value ? (int?)null : Convert.ToInt32(rd["BENTRM"]),
                Benate = rd["BENATE"] == DBNull.Value ? (int?)null : Convert.ToInt32(rd["BENATE"]),
                Benudg = rd["BENUDG"] as string
            };
        }

        // === Insertar beneficiario ===
        public bool Insertar(BeneficiarioRequestDto dto)
        {
            using var cn = _conexion.ObtenerConexion();
            cn.Open();

            using var cmd = cn.CreateCommand();

            // nombre completo
            var nombreCompleto = $"{dto.Benno1} {dto.Benno2 ?? ""} {dto.Benap1} {dto.Benap2 ?? ""}"
                                .Replace("  ", " ").Trim();

            cmd.CommandText = @"
                INSERT INTO BENEFICIARIO
                (BENCAU, BENCOD, BENNO1, BENNO2, BENAP1, BENAP2, BENNOC, BENRES, BENDPI,
                 BENPAR, BENMON, BENSIT, BENTRM, BENATE, BENUDG)
                VALUES
                (:bencau, :bencod, :benno1, :benno2, :benap1, :benap2, :bennoc, :benres, :bendpi,
                 :benpar, :benmon, :bensit, :bentrm, :benate, :benudg)";

            cmd.Parameters.Add(new OracleParameter("bencau", dto.Bencau));
            cmd.Parameters.Add(new OracleParameter("bencod", dto.Bencod)); // lo ingresa el usuario
            cmd.Parameters.Add(new OracleParameter("benno1", dto.Benno1));
            cmd.Parameters.Add(new OracleParameter("benno2", dto.Benno2 ?? ""));
            cmd.Parameters.Add(new OracleParameter("benap1", dto.Benap1));
            cmd.Parameters.Add(new OracleParameter("benap2", dto.Benap2 ?? ""));
            cmd.Parameters.Add(new OracleParameter("bennoc", nombreCompleto));
            cmd.Parameters.Add(new OracleParameter("benres", dto.Benres));
            cmd.Parameters.Add(new OracleParameter("bendpi", dto.Bendpi));
            cmd.Parameters.Add(new OracleParameter("benpar", dto.Benpar));
            cmd.Parameters.Add(new OracleParameter("benmon", dto.Benmon));
            cmd.Parameters.Add(new OracleParameter("bensit", dto.Bensit ?? (object)DBNull.Value));
            cmd.Parameters.Add(new OracleParameter("bentrm", dto.Bentrm ?? (object)DBNull.Value));
            cmd.Parameters.Add(new OracleParameter("benate", dto.Benate ?? (object)DBNull.Value));
            cmd.Parameters.Add(new OracleParameter("benudg", dto.Benudg));

            return cmd.ExecuteNonQuery() > 0;
        }

        // === Actualizar beneficiario ===
        public bool Actualizar(BeneficiarioEditDto dto)
        {
            using var cn = _conexion.ObtenerConexion();
            cn.Open();

            using var cmd = cn.CreateCommand();

            var nombreCompleto = $"{dto.Benno1} {dto.Benno2 ?? ""} {dto.Benap1} {dto.Benap2 ?? ""}"
                                .Replace("  ", " ").Trim();

            cmd.CommandText = @"
                UPDATE BENEFICIARIO SET
                    BENNO1 = :no1,
                    BENNO2 = :no2,
                    BENAP1 = :ap1,
                    BENAP2 = :ap2,
                    BENNOC = :noc,
                    BENRES = :res,
                    BENDPI = :dpi,
                    BENPAR = :par,
                    BENMON = :mon,
                    BENSIT = :sit,
                    BENTRM = :trm,
                    BENATE = :ate,
                    BENUDG = :udg
                WHERE BENCAU = :cau AND BENCOD = :cod";

            cmd.Parameters.Add(new OracleParameter("no1", dto.Benno1));
            cmd.Parameters.Add(new OracleParameter("no2", dto.Benno2 ?? ""));
            cmd.Parameters.Add(new OracleParameter("ap1", dto.Benap1));
            cmd.Parameters.Add(new OracleParameter("ap2", dto.Benap2 ?? ""));
            cmd.Parameters.Add(new OracleParameter("noc", nombreCompleto));
            cmd.Parameters.Add(new OracleParameter("res", dto.Benres));
            cmd.Parameters.Add(new OracleParameter("dpi", dto.Bendpi));
            cmd.Parameters.Add(new OracleParameter("par", dto.Benpar));
            cmd.Parameters.Add(new OracleParameter("mon", dto.Benmon));
            cmd.Parameters.Add(new OracleParameter("sit", string.IsNullOrWhiteSpace(dto.Bensit) ? (object)DBNull.Value : dto.Bensit));
            cmd.Parameters.Add(new OracleParameter("trm", dto.Bentrm ?? (object)DBNull.Value));
            cmd.Parameters.Add(new OracleParameter("ate", dto.Benate ?? (object)DBNull.Value));
            cmd.Parameters.Add(new OracleParameter("udg", dto.Benudg ?? ""));
            cmd.Parameters.Add(new OracleParameter("cau", dto.Bencau));
            cmd.Parameters.Add(new OracleParameter("cod", dto.Bencod));

            return cmd.ExecuteNonQuery() > 0;
        }

        // === Eliminar beneficiario ===
        public bool Eliminar(int bencau, int bencod)
        {
            using var cn = _conexion.ObtenerConexion();
            cn.Open();

            using var cmd = cn.CreateCommand();
            cmd.CommandText = @"DELETE FROM BENEFICIARIO WHERE BENCAU = :cau AND BENCOD = :cod";
            cmd.Parameters.Add(new OracleParameter("cau", bencau));
            cmd.Parameters.Add(new OracleParameter("cod", bencod));

            return cmd.ExecuteNonQuery() > 0;
        }
    }
}
