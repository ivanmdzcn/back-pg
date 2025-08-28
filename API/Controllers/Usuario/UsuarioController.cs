using AccesoDatos.Usuario;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Usuario
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioController : Controller
    {

        [HttpGet("usuarios")]
        public IActionResult GetUsuarios([FromServices] UsuarioDao usuarioDao)
        {
            var usuarios = usuarioDao.ObtenerUsuarios();
            return Ok(usuarios);
        }
    }
}
