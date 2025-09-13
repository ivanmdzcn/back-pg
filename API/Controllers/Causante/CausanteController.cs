using Dominio.DTO.Causante;
using Microsoft.AspNetCore.Mvc;
using Servicios.Interfaces.Causante;

namespace API.Controllers.Causante
{
    [ApiController]
    [Route("api/[controller]")]
    public class CausanteController : ControllerBase
    {
        private readonly ICausanteService _service;

        public CausanteController(ICausanteService service)
        {
            _service = service;
        }

        //Insertar
        [HttpPost]
        public IActionResult Crear([FromBody] CausanteRequestDto dto)
        {
            if (_service.CrearCausante(dto))
                return Ok(new { mensaje = "Causante creado correctamente." });

            return BadRequest(new { mensaje = "Error al crear causante." });
        }

        //Obtener Todos
        [HttpGet]
        public IActionResult Listar([FromQuery] string? q)
        {
            var lista = _service.ObtenerCausantes(q);
            return Ok(lista);
        }

        //Actualizar
        [HttpPut("{id}")]
        public IActionResult ActualizarCausante(int id, [FromBody] CausanteRequestDto dto)
        {
            var actualizado = _service.ActualizarCausante(id, dto); // ← CORREGIDO
            if (!actualizado)
                return NotFound(new { mensaje = "Causante no encontrado" });

            return Ok(new { mensaje = "Causante actualizado exitosamente" });
        }

        //Obtener por ID
        [HttpGet("{id}")]
        public IActionResult ObtenerPorId(int id)
        {
            var causante = _service.ObtenerCausantePorId(id);
            if (causante == null)
                return NotFound(new { mensaje = "Causante no encontrado." });

            return Ok(causante);
        }

        //Eliminar
        [HttpDelete("{id}")]
        public IActionResult EliminarCausante(int id)
        {
            if (_service.EliminarCausante(id))
                return Ok(new { mensaje = "Causante eliminado correctamente." });

            return NotFound(new { mensaje = "Causante no encontrado o ya eliminado." });
        }
    }
}
