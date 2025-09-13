using Dominio.DTO.Beneficiario;
namespace Servicios.Interfaces.Beneficiario
{
    public interface IBeneficiarioService
    {
        List<BeneficiarioResponseDto> ListarPorCausante(int causanteId);
        BeneficiarioEditDto? ObtenerUno(int bencau, int bencod);
        bool Insertar(BeneficiarioRequestDto dto);
        bool Actualizar(BeneficiarioEditDto dto);
        bool Eliminar(int bencau, int bencod);
    }
}
