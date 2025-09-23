using AccesoDatos.Reportes;
using Dominio.DTO.Reportes;
using Servicios.Interfaces.Reportes;

namespace Servicios.Servicios.Reportes
{
    public class ReportesService : IReportesService
    {
        private readonly ReportesDao _dao;
        public ReportesService(ReportesDao dao) { _dao = dao; }

        private static bool PuedeVerTodas(string rol)
            => string.Equals(rol, "ADMIN", StringComparison.OrdinalIgnoreCase)
            || string.Equals(rol, "ANALISTA", StringComparison.OrdinalIgnoreCase);

        public NominaDetalleReporteDto? DetallePorNomina(int nomcod, string usuario, string rol)
            => _dao.DetallePorNomina(nomcod, usuario, PuedeVerTodas(rol));

        public List<NominaResumenDto> ListarAutorizadas(string usuario, string rol)
            => _dao.ListarAutorizadas(usuario, PuedeVerTodas(rol));
    }
}
