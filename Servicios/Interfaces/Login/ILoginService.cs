using Dominio.DTO.Login;


namespace Servicios.Interfaces.Login
{
    public interface ILoginService
    {
        LoginResponseDto Login(LoginRequestDto request);
        
    }
}
