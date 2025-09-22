using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Servicios.Interfaces.Nomina;
using Dominio.DTO.Nomina;
using System.Security.Claims;

namespace API.Controllers.Nomina
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NominaController : ControllerBase
    {
        private readonly INominaService _svc;
        public NominaController(INominaService svc) { _svc = svc; }

        private string GetUsuario() =>
            User?.Identity?.Name ??
            User?.FindFirstValue(ClaimTypes.Name) ??
            "anonimo";

        private string GetRol() =>
            (User?.FindFirstValue(ClaimTypes.Role) ?? "OPERADOR").ToUpperInvariant();

        // ===== Encabezado =====

        [HttpPost]
        public IActionResult Crear([FromBody] NominaCreateDto dto)
        {
            if (dto == null) return BadRequest(new { mensaje = "Body requerido" });
            if (dto.Nomfdf < dto.Nomfdi) return BadRequest(new { mensaje = "Rango de fechas inválido" });

            var id = _svc.Crear(dto, GetUsuario());
            return Ok(new { nomcod = id, mensaje = "Nómina creada en borrador." });
        }

        [HttpGet]
        public ActionResult<List<NominaHdrDto>> Listar()
        {
            return Ok(_svc.Listar(GetUsuario(), GetRol()));
        }

        [HttpGet("{id:int}")]
        public ActionResult<NominaDto> Obtener(int id)
        {
            var dto = _svc.Obtener(id, GetUsuario(), GetRol());
            if (dto == null) return Forbid(); // o NotFound si se quiere ocultar la existencia
            return Ok(dto);
        }

        [HttpPost("{id:int}/autorizar")]
        [Authorize(Roles = "ADMIN")]
        public IActionResult Autorizar(int id)
        {
            var ok = _svc.Autorizar(id, GetUsuario());
            if (!ok) return BadRequest(new { mensaje = "Solo se autoriza una nómina en estado Borrador." });
            return Ok(new { mensaje = "Nómina autorizada." });
        }

        [HttpPost("{id:int}/cancelar")]
        [Authorize(Roles = "ADMIN")]
        public IActionResult Cancelar(int id)
        {
            var ok = _svc.Cancelar(id, GetUsuario());
            if (!ok) return BadRequest(new { mensaje = "No se pudo cancelar (estado inválido)." });
            return Ok(new { mensaje = "Nómina cancelada." });
        }

        [HttpPut("{id:int}/fechas")]
        public IActionResult ActualizarFechas(int id, [FromBody] NominaCreateDto dto)
        {
            if (dto == null) return BadRequest(new { mensaje = "Body requerido" });
            if (dto.Nomfdf < dto.Nomfdi) return BadRequest(new { mensaje = "Rango de fechas inválido" });

            var ok = _svc.ActualizarFechas(id, dto.Nomfdi, dto.Nomfdf);
            if (!ok) return BadRequest(new { mensaje = "Solo se puede editar fechas en Borrador." });
            return Ok(new { mensaje = "Fechas actualizadas." });
        }

        // ===== Detalle =====

        // ✅ Ahora valida permisos reutilizando Obtener(...)
        [HttpGet("{id:int}/detalle")]
        public ActionResult<List<NominaDetalleDto>> ListarDetalle(int id)
        {
            var nomina = _svc.Obtener(id, GetUsuario(), GetRol());
            if (nomina == null || !nomina.Permisos.PuedeVer) return Forbid();
            return Ok(nomina.Detalle);
        }

        [HttpPost("{id:int}/detalle")]
        public IActionResult AgregarDetalle(int id, [FromBody] NominaDetalleDto dto)
        {
            if (dto == null || dto.Detcau <= 0 || dto.Detben <= 0 || dto.Detmon <= 0)
                return BadRequest(new { mensaje = "Datos inválidos" });

            var ok = _svc.AgregarDetalle(id, dto, GetUsuario(), GetRol());
            if (!ok) return BadRequest(new { mensaje = "No se pudo agregar (permiso o estado inválido)." });
            return Ok(new { mensaje = "Detalle agregado." });
        }

        [HttpPut("{id:int}/detalle/{cau:int}/{ben:int}")]
        public IActionResult ActualizarDetalle(int id, int cau, int ben, [FromBody] NominaDetUpdateDto dto)
        {
            if (dto == null || dto.Detmon <= 0)
                return BadRequest(new { mensaje = "Monto inválido" });

            var ok = _svc.ActualizarDetalle(id, cau, ben, dto.Detmon, GetUsuario(), GetRol());
            if (!ok) return BadRequest(new { mensaje = "No se pudo actualizar (permiso o estado inválido)." });
            return Ok(new { mensaje = "Detalle actualizado." });
        }

        [HttpDelete("{id:int}/detalle/{cau:int}/{ben:int}")]
        public IActionResult EliminarDetalle(int id, int cau, int ben)
        {
            var ok = _svc.EliminarDetalle(id, cau, ben, GetUsuario(), GetRol());
            if (!ok) return BadRequest(new { mensaje = "No se pudo eliminar (permiso o estado inválido)." });
            return Ok(new { mensaje = "Detalle eliminado." });
        }
    }
}
