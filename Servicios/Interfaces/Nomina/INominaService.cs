using Dominio.DTO.Nomina;

namespace Servicios.Interfaces.Nomina
{
    public interface INominaService
    {
        // Encabezado
        int Crear(NominaCreateDto dto, string usuario);
        List<NominaHdrDto> Listar(string usuario, string rol);
        NominaDto? Obtener(int nomcod, string usuario, string rol);
        bool Autorizar(int nomcod, string usuarioAdmin);
        bool Cancelar(int nomcod, string usuarioAdmin);
        bool ActualizarFechas(int nomcod, DateTime fdi, DateTime fdf);

        // Detalle
        List<NominaDetalleDto> ListarDetalle(int nomcod);
        bool AgregarDetalle(int nomcod, NominaDetalleDto dto, string usuario, string rol);
        bool ActualizarDetalle(int nomcod, int cau, int ben, decimal monto, string usuario, string rol);
        bool EliminarDetalle(int nomcod, int cau, int ben, string usuario, string rol);
    }
}
