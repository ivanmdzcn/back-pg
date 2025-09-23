using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Servicios.Interfaces.Reportes;
using System.Security.Claims;

using Servicios.Servicios.Reportes;

namespace API.Controllers.Reportes
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportesController : ControllerBase
    {
        private readonly IReportesService _svc;
        public ReportesController(IReportesService svc) { _svc = svc; }

        private string Usuario() =>
            User?.Identity?.Name ?? User?.FindFirstValue(ClaimTypes.Name) ?? "anonimo";

        private string RolCodigo()
        {
            // Puede haber varios roles; priorizamos ADMIN si existe
            var roles = User?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList() ?? new List<string>();
            if (roles.Count == 0) return "OPERADOR";

            // Toma el primero por defecto, pero si hay ADMIN se usa ese
            var raw = roles.FirstOrDefault() ?? "OPERADOR";
            if (roles.Any(r => string.Equals(r, "ADMIN", StringComparison.OrdinalIgnoreCase)))
                raw = "ADMIN";

            raw = raw.Trim();
            var upper = raw.ToUpperInvariant();

            // Si ya viene codificado, se usa tal cual
            if (upper is "ADMIN" or "ANALISTA" or "OPERADOR") return upper;

            // Mapear descripciones a códigos
            return upper switch
            {
                "ADMINISTRADOR" => "ADMIN",
                "ANALISTA" or "ANALISTA DE NOMINA" or "ANALISTA DE NÓMINA" => "ANALISTA",
                _ => "OPERADOR"
            };
        }

        // GET /api/Reportes/nominas-autorizadas
        [HttpGet("nominas-autorizadas")]
        public IActionResult NominasAutorizadas()
            => Ok(_svc.ListarAutorizadas(Usuario(), RolCodigo()));

        // GET /api/Reportes/nomina/{id}/detalle
        [HttpGet("nomina/{id:int}/detalle")]
        public IActionResult DetalleNomina(int id)
        {
            var dto = _svc.DetallePorNomina(id, Usuario(), RolCodigo());
            if (dto == null) return Forbid();
            return Ok(dto);
        }

        [HttpGet("nomina/{id:int}/pdf")]
        public IActionResult PdfNomina(int id, [FromServices] ReportPdfService pdfSrv, [FromServices] AccesoDatos.Reportes.ReportesDao dao)
        {
            // Misma validación de permisos que el JSON:
            var rol = RolCodigo();
            var usuario = Usuario();
            var verTodas = rol is "ADMIN" or "ANALISTA";

            // Encabezado (si no existe, 404)
            var hdr = dao.ObtenerHdrBasico(id);
            if (hdr is null) return NotFound();

            // Validación de visibilidad (reutilizamos DetallePorNomina que ya valida)
            var dto = _svc.DetallePorNomina(id, usuario, rol);
            if (dto == null) return Forbid();

            // (Opcional) logo base64: se podría leer un PNG del disco y pasarlo
            string? logoBase64 = null;
            // logoBase64 = Convert.ToBase64String(System.IO.File.ReadAllBytes("wwwroot/img/logo.png"));

            var bytes = pdfSrv.CrearPdf(hdr, dto, logoBase64);
            return File(bytes, "application/pdf", $"nomina_{id}.pdf");
        }
    }
}
