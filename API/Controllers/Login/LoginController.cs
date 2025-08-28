using Dominio.DTO.Login;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Servicios.Interfaces.Login;

namespace API.Controllers.Login
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : Controller
    {
        private readonly ILoginService _loginService;
        public LoginController(ILoginService loginService)
        {
            _loginService = loginService;
        }
        [HttpPost]
        public IActionResult Login([FromBody] LoginRequestDto request)
        {
            var result = _loginService.Login(request);
            if (result == null)
            {
                return Unauthorized(new { mensaje = "Credenciales inválidas" });
            }

            return Ok(result);
        }//Fin Post

        [Authorize(Roles = "Administrador")]
        [HttpGet("ver-usuarios")]
        public IActionResult VerUsuarios()
        {
            return Ok(new { mensaje = "Este endpoint solo lo puede ver el Administrador" });
        }//Fin Rol
    }
}
