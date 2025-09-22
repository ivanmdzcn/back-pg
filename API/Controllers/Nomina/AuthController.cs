using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize] // requiere token
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        [HttpGet("whoami")]
        public IActionResult WhoAmI()
        {
            var name =
                User?.Identity?.Name ??
                User?.FindFirstValue(ClaimTypes.Name) ??
                User?.FindFirst("usuario")?.Value ??
                "(desconocido)";

            // Nota: Roles pueden venir como ClaimTypes.Role (uno o varios)
            var roles = User.FindAll(ClaimTypes.Role);
            var roleNames = roles is not null ? roles.Select(r => r.Value).ToArray() : Array.Empty<string>();

            // Si también se guarda el código de rol en un claim (p.ej. "rolCodigo"), se peude leer:
            var rolCodigo = User.FindFirst("rolCodigo")?.Value; // opcional

            return Ok(new
            {
                name,
                roles = roleNames,   // p.ej. ["ADMIN"] o ["OPERADOR"]
                rolCodigo            // p.ej. "ADMIN" (si se incluye al emitir el token)
            });
        }
    }
}
