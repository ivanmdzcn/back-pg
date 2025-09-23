using Dominio.DTO.Reportes;

namespace Servicios.Interfaces.Reportes
{
    public interface IReportesService
    {
        NominaDetalleReporteDto? DetallePorNomina(int nomcod, string usuario, string rol);
        List<NominaResumenDto> ListarAutorizadas(string usuario, string rol);
    }
}
