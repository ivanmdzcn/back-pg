using Dominio.DTO.Nomina;

namespace Servicios.Interfaces.Nomina
{
    public interface INominaService
    {
        int Crear(NominaCreateDto dto, string usuario);
        NominaDto? Obtener(int nomcod, string usuario, string rol);
        List<NominaHdrDto> Listar(string usuario, string rol);
        bool Autorizar(int nomcod, string usuarioAdmin);
        bool Cancelar(int nomcod, string usuarioAdmin);
        bool ActualizarFechas(int nomcod, DateTime fdi, DateTime fdf);
    }
}
