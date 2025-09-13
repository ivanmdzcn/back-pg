using AccesoDatos.Beneficiario;
using Dominio.DTO.Beneficiario;
using Servicios.Interfaces.Beneficiario;

namespace Servicios.Servicios.Beneficiario
{
    public class BeneficiarioService : IBeneficiarioService
    {
        private readonly BeneficiarioDao _dao;
        public BeneficiarioService(BeneficiarioDao dao) { _dao = dao; }

        public List<BeneficiarioResponseDto> ListarPorCausante(int causanteId) => _dao.ListarPorCausante(causanteId);
        public BeneficiarioEditDto? ObtenerUno(int bencau, int bencod) => _dao.ObtenerUno(bencau, bencod);
        public bool Insertar(BeneficiarioRequestDto dto) => _dao.Insertar(dto);
        public bool Actualizar(BeneficiarioEditDto dto) => _dao.Actualizar(dto);
        public bool Eliminar(int bencau, int bencod) => _dao.Eliminar(bencau, bencod);
    }
}
