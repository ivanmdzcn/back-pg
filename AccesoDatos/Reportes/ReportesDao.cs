using AccesoDatos.Conexion;
using Dominio.DTO.Reportes;
using Oracle.ManagedDataAccess.Client;

namespace AccesoDatos.Reportes
{
    public class ReportesDao
    {
        private readonly ConexionOracle _conexion;
        public ReportesDao(ConexionOracle conexion) { _conexion = conexion; }

        // DETALLE por nomcod
        public NominaDetalleReporteDto? DetallePorNomina(int nomcod, string usuario, bool verTodas)
        {
            using var cn = _conexion.ObtenerConexion();
            cn.Open();

            // Validación de visibilidad
            using (var val = cn.CreateCommand())
            {
                val.CommandText = verTodas
                    ? "SELECT NOMUDC FROM NOMINA WHERE NOMCOD=:id AND NOMSTD='A'"
                    : "SELECT NOMUDC FROM NOMINA WHERE NOMCOD=:id AND NOMSTD='A' AND NOMUDC=:usr";
                val.Parameters.Add(new OracleParameter("id", nomcod));
                if (!verTodas) val.Parameters.Add(new OracleParameter("usr", usuario));

                using var rd = val.ExecuteReader();
                if (!rd.Read()) return null;
            }

            var dto = new NominaDetalleReporteDto { Nomcod = nomcod };

            using (var cmd = cn.CreateCommand())
            {
                cmd.CommandText = @"
            SELECT 
                d.DETCOD, d.DETCAU, d.DETBEN, d.DETMON,
                /* nombre completo de CAUSANTE (ajusta columnas si tu esquema difiere) */
                NVL(c.CAUNOC, TRIM(
                    NVL(c.CAUNO1,'') || ' ' || NVL(c.CAUNO2,'') || ' ' ||
                    NVL(c.CAUAP1,'') || ' ' || NVL(c.CAUAP2,'')
                )) AS CAU_NOMBRE,
                /* nombre completo de BENEFICIARIO (ya tienes BENNOC) */
                NVL(b.BENNOC, TRIM(
                    NVL(b.BENNO1,'') || ' ' || NVL(b.BENNO2,'') || ' ' ||
                    NVL(b.BENAP1,'') || ' ' || NVL(b.BENAP2,'')
                )) AS BEN_NOMBRE
            FROM NOM_DETALLE d
            JOIN CAUSANTE c
              ON c.CAUCOD = d.DETCAU
            JOIN BENEFICIARIO b
              ON b.BENCAU = d.DETCAU
             AND b.BENCOD = d.DETBEN
            WHERE d.DETCOD = :id
            ORDER BY d.DETCAU, d.DETBEN";
                cmd.Parameters.Add(new OracleParameter("id", nomcod));

                using var rd = cmd.ExecuteReader();
                while (rd.Read())
                {
                    dto.Lineas.Add(new NominaDetalleLineaDto
                    {
                        Detcod = Convert.ToInt32(rd["DETCOD"]),
                        Detcau = Convert.ToInt32(rd["DETCAU"]),
                        Detben = Convert.ToInt32(rd["DETBEN"]),
                        Detmon = Convert.ToDecimal(rd["DETMON"]),
                        CausanteNombre = rd["CAU_NOMBRE"]?.ToString(),
                        BeneficiarioNombre = rd["BEN_NOMBRE"]?.ToString()
                    });
                }
            }

            using (var sum = cn.CreateCommand())
            {
                sum.CommandText = @"
            SELECT NVL(SUM(DETMON),0) AS TOTAL, COUNT(*) AS CNT
            FROM NOM_DETALLE
            WHERE DETCOD = :id";
                sum.Parameters.Add(new OracleParameter("id", nomcod));

                using var rd2 = sum.ExecuteReader();
                if (rd2.Read())
                {
                    dto.TotalMonto = Convert.ToDecimal(rd2["TOTAL"]);
                    dto.TotalBeneficiarios = Convert.ToInt32(rd2["CNT"]);
                }
            }

            return dto;
        }

        // LISTADO de autorizadas con totales
        public List<NominaResumenDto> ListarAutorizadas(string usuario, bool verTodas)
        {
            var list = new List<NominaResumenDto>();
            using var cn = _conexion.ObtenerConexion();
            cn.Open();

            using var cmd = cn.CreateCommand();
            cmd.CommandText = verTodas
                ? @"
                    SELECT n.NOMCOD, n.NOMTIP, n.NOMFDI, n.NOMFDF, n.NOMUDC,
                           NVL(SUM(d.DETMON),0) AS TOTAL_MONTO,
                           COUNT(d.DETBEN) AS TOTAL_BEN
                      FROM NOMINA n
                      LEFT JOIN NOM_DETALLE d ON d.DETCOD = n.NOMCOD
                     WHERE n.NOMSTD = 'A'
                     GROUP BY n.NOMCOD, n.NOMTIP, n.NOMFDI, n.NOMFDF, n.NOMUDC
                     ORDER BY n.NOMCOD DESC"
                : @"
                    SELECT n.NOMCOD, n.NOMTIP, n.NOMFDI, n.NOMFDF, n.NOMUDC,
                           NVL(SUM(d.DETMON),0) AS TOTAL_MONTO,
                           COUNT(d.DETBEN) AS TOTAL_BEN
                      FROM NOMINA n
                      LEFT JOIN NOM_DETALLE d ON d.DETCOD = n.NOMCOD
                     WHERE n.NOMSTD = 'A' AND n.NOMUDC = :usr
                     GROUP BY n.NOMCOD, n.NOMTIP, n.NOMFDI, n.NOMFDF, n.NOMUDC
                     ORDER BY n.NOMCOD DESC";

            if (!verTodas) cmd.Parameters.Add(new OracleParameter("usr", usuario));

            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                list.Add(new NominaResumenDto
                {
                    Nomcod = Convert.ToInt32(rd["NOMCOD"]),
                    Nomtip = rd["NOMTIP"].ToString()!,
                    Nomfdi = Convert.ToDateTime(rd["NOMFDI"]),
                    Nomfdf = Convert.ToDateTime(rd["NOMFDF"]),
                    Nomudc = rd["NOMUDC"].ToString()!,
                    TotalBeneficiarios = Convert.ToInt32(rd["TOTAL_BEN"]),
                    TotalMonto = Convert.ToDecimal(rd["TOTAL_MONTO"])
                });
            }
            return list;
        }


        // PARA EL REPORTE: Obtener datos básicos de la nómina
        public ReporteNominaHdrDto? ObtenerHdrBasico(int nomcod)
        {
            using var cn = _conexion.ObtenerConexion();
            cn.Open();
            using var cmd = cn.CreateCommand();
            cmd.CommandText = @"
        SELECT NOMCOD, NOMTIP, NOMSTD, NOMFDI, NOMFDF, NOMUDC, NOMUDA
          FROM NOMINA
         WHERE NOMCOD = :id";
            cmd.Parameters.Add(new OracleParameter("id", nomcod));

            using var rd = cmd.ExecuteReader();
            if (!rd.Read()) return null;

            return new ReporteNominaHdrDto
            {
                Nomcod = Convert.ToInt32(rd["NOMCOD"]),
                Nomtip = rd["NOMTIP"].ToString()!,
                Nomstd = rd["NOMSTD"].ToString()!,
                Nomfdi = Convert.ToDateTime(rd["NOMFDI"]),
                Nomfdf = Convert.ToDateTime(rd["NOMFDF"]),
                Nomudc = rd["NOMUDC"].ToString()!,
                Nomuda = rd["NOMUDA"] as string
            };
        }
    }
}
