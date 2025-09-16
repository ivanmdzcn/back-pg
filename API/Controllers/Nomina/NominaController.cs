using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Servicios.Interfaces.Nomina;
using Dominio.DTO.Nomina;
using System.Security.Claims;

namespace API.Controllers.Nomina
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // requiere JWT
    public class NominaController : ControllerBase
    {
        private readonly INominaService _svc;
        public NominaController(INominaService svc) { _svc = svc; }

        // Helpers para leer usuario/rol del token de forma robusta
        private string GetUsuario() =>
            User?.Identity?.Name ??
            User?.FindFirstValue(ClaimTypes.Name) ??
            User?.FindFirstValue("unique_name") ??
            "anonimo";

        private string GetRolCodigo()
        {
            // Preferimos el claim estándar
            var rol = User?.FindFirstValue(ClaimTypes.Role)
                      ?? User?.FindFirstValue("role")
                      ?? "OPERADOR";
            return rol.ToUpperInvariant(); // ADMIN | OPERADOR | ANALISTA
        }

        // GET /api/Nomina  (lista: admin/analista todas, operador solo propias)
        [HttpGet]
        public ActionResult<List<NominaHdrDto>> Listar()
        {
            var usuario = GetUsuario();
            var rol = GetRolCodigo(); // <- "ADMIN" | "OPERADOR" | "ANALISTA"

            var lista = _svc.Listar(usuario, rol);
            return Ok(lista);
        }

        // POST /api/Nomina  (crea borrador)
        [HttpPost]
        public IActionResult Crear([FromBody] NominaCreateDto dto)
        {
            if (dto == null) return BadRequest(new { mensaje = "Body requerido" });
            if (dto.Nomfdf < dto.Nomfdi) return BadRequest(new { mensaje = "Rango de fechas inválido" });

            var usuario = GetUsuario();
            var id = _svc.Crear(dto, usuario);

            return Ok(new { nomcod = id, mensaje = "Nómina creada en borrador." });
        }

        // GET /api/Nomina/{id}  (encabezado + permisos)
        [HttpGet("{id:int}")]
        public ActionResult<NominaDto> Obtener(int id)
        {
            var usuario = GetUsuario();
            var rol = GetRolCodigo();

            var dto = _svc.Obtener(id, usuario, rol);
            if (dto == null) return Forbid(); // o NotFound si prefieres no revelar existencia

            return Ok(dto);
        }

        // POST /api/Nomina/{id}/autorizar  (solo admin)
        [HttpPost("{id:int}/autorizar")]
        [Authorize(Roles = "ADMIN")]
        public IActionResult Autorizar(int id)
        {
            var usuario = GetUsuario(); // quién autoriza (admin)
            var ok = _svc.Autorizar(id, usuario);
            if (!ok) return BadRequest(new { mensaje = "Solo se autoriza una nómina en estado Borrador." });
            return Ok(new { mensaje = "Nómina autorizada." });
        }

        // POST /api/Nomina/{id}/cancelar  (solo admin)
        [HttpPost("{id:int}/cancelar")]
        [Authorize(Roles = "ADMIN")]
        public IActionResult Cancelar(int id)
        {
            var usuario = GetUsuario(); // quién cancela (admin)
            var ok = _svc.Cancelar(id, usuario);
            if (!ok) return BadRequest(new { mensaje = "No se pudo cancelar (estado inválido)." });
            return Ok(new { mensaje = "Nómina cancelada." });
        }

        // PUT /api/Nomina/{id}/fechas (editar rango SOLO en B)
        [HttpPut("{id:int}/fechas")]
        public IActionResult ActualizarFechas(int id, [FromBody] NominaCreateDto dto)
        {
            if (dto == null) return BadRequest(new { mensaje = "Body requerido" });
            if (dto.Nomfdf < dto.Nomfdi) return BadRequest(new { mensaje = "Rango de fechas inválido" });

            var ok = _svc.ActualizarFechas(id, dto.Nomfdi, dto.Nomfdf);
            if (!ok) return BadRequest(new { mensaje = "Solo se puede editar fechas en Borrador." });
            return Ok(new { mensaje = "Fechas actualizadas." });
        }

        // (opcional) Endpoint de diagnóstico para ver qué llega en el token
        [HttpGet("whoami")]
        public IActionResult WhoAmI()
        {
            return Ok(new
            {
                name = GetUsuario(),
                rol = GetRolCodigo()
            });
        }
    }
}
