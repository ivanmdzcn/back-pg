using Dominio.DTO.Causante;

namespace Servicios.Interfaces.Causante
{
    public interface ICausanteService
    {
        //Crear
        bool CrearCausante(CausanteRequestDto dto);
        //Actualizar
        bool ActualizarCausante(int id, CausanteRequestDto dto);
        //Obtener todos
        List<CausanteResponseDto> ObtenerCausantes();
        //Obtener solo por ID
        CausanteRequestDto ObtenerCausantePorId(int id);
        //Eliminar
        bool EliminarCausante(int id);
    }
}
