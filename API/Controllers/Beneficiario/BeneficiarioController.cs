using Microsoft.AspNetCore.Mvc;
using Servicios.Interfaces.Beneficiario;
using Dominio.DTO.Beneficiario;
using Oracle.ManagedDataAccess.Client;   // 👈 para capturar ORA- errores

namespace API.Controllers.Beneficiario
{
    [ApiController]
    [Route("api/[controller]")]
    public class BeneficiarioController : ControllerBase
    {
        private readonly IBeneficiarioService _svc;
        public BeneficiarioController(IBeneficiarioService svc) { _svc = svc; }

        // GET /api/Beneficiario?causanteId=1
        [HttpGet]
        public ActionResult<List<BeneficiarioResponseDto>> GetByCausante([FromQuery] int causanteId)
        {
            if (causanteId <= 0) return BadRequest(new { mensaje = "causanteId requerido" });
            return Ok(_svc.ListarPorCausante(causanteId));
        }

        // GET /api/Beneficiario/{bencau}/{bencod}
        [HttpGet("{bencau:int}/{bencod:int}")]
        public ActionResult<BeneficiarioEditDto> GetOne(int bencau, int bencod)
        {
            var ben = _svc.ObtenerUno(bencau, bencod);
            if (ben == null) return NotFound(new { mensaje = "Beneficiario no encontrado." });
            return Ok(ben);
        }

        // POST /api/Beneficiario
        [HttpPost]
        public IActionResult Crear([FromBody] BeneficiarioRequestDto dto)
        {
            if (dto == null) return BadRequest(new { mensaje = "Body requerido" });
            if (dto.Bencau <= 0) return BadRequest(new { mensaje = "BENCAU (causante) requerido" });

            try
            {
                var ok = _svc.Insertar(dto);
                return ok ? Ok(new { mensaje = "Beneficiario creado correctamente." })
                          : BadRequest(new { mensaje = "No se pudo crear el beneficiario." });
            }
            catch (OracleException ex) when (ex.Number == 1) // ORA-00001 duplicado
            {
                // Puede ser PK (BENCAU,BENCOD) o UNIQUE (BENCAU,BENDPI)
                return Conflict(new { mensaje = "Registro duplicado: código o DPI ya existe para este causante." });
            }
            catch (OracleException ex) when (ex.Number == 2291) // ORA-02291 FK no encontrada
            {
                return BadRequest(new { mensaje = "Valor de catálogo inválido (FK).", detalle = ex.Message });
            }
        }

        // PUT /api/Beneficiario/{bencau}/{bencod}
        [HttpPut("{bencau:int}/{bencod:int}")]
        public IActionResult Actualizar(int bencau, int bencod, [FromBody] BeneficiarioEditDto dto)
        {
            if (dto == null) return BadRequest(new { mensaje = "Body requerido" });
            if (dto.Bencau != bencau || dto.Bencod != bencod)
                return BadRequest(new { mensaje = "Parámetros de ruta y body no coinciden." });

            try
            {
                var ok = _svc.Actualizar(dto);
                if (!ok) return NotFound(new { mensaje = "Beneficiario no encontrado." });
                return Ok(new { mensaje = "Beneficiario actualizado correctamente." });
            }
            catch (OracleException ex) when (ex.Number == 1) // ORA-00001 (UNIQUE)
            {
                return Conflict(new { mensaje = "DPI duplicado para este causante." });
            }
            catch (OracleException ex) when (ex.Number == 2291) // ORA-02291 FK no encontrada
            {
                return BadRequest(new { mensaje = "Valor de catálogo inválido (FK).", detalle = ex.Message });
            }
        }

        // DELETE /api/Beneficiario/{bencau}/{bencod}
        [HttpDelete("{bencau:int}/{bencod:int}")]
        public IActionResult Eliminar(int bencau, int bencod)
        {
            try
            {
                var ok = _svc.Eliminar(bencau, bencod);
                if (!ok) return NotFound(new { mensaje = "Beneficiario no encontrado." });
                return Ok(new { mensaje = "Beneficiario eliminado correctamente." });
            }
            catch (OracleException ex) when (ex.Number == 2292) // ORA-02292: referencia en otra tabla
            {
                return Conflict(new { mensaje = "No se puede eliminar: el registro está referenciado.", detalle = ex.Message });
            }
        }
    }
}
