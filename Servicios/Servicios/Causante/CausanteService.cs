using AccesoDatos.Causante;
using Dominio.DTO.Causante;
using Servicios.Interfaces.Causante;

namespace Servicios.Servicios.Causante
{
    public class CausanteService : ICausanteService
    {
        private readonly CausanteDao _dao;

        public CausanteService(CausanteDao dao)
        {
            _dao = dao;
        }

        //Insertar
        public bool CrearCausante(CausanteRequestDto dto)
        {
            return _dao.InsertarCausante(dto);
        }

        //Obtener todos
        public List<CausanteResponseDto> ObtenerCausantes()
        {
            return _dao.ListarCausantes();
        }

        //ACtualizar
        public bool ActualizarCausante(int id, CausanteRequestDto dto)
        {
            return _dao.ActualizarCausante(id, dto);
        }

        //Obtener por ID
        public CausanteRequestDto ObtenerCausantePorId(int id)
        {
            return _dao.ObtenerCausantePorId(id);
        }

        //Eliminar
        public bool EliminarCausante(int id)
        {
            return _dao.EliminarCausante(id);
        }

    }
}
