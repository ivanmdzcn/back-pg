// Dominio/DTO/Reportes/ReporteNominaHdrDto.cs
namespace Dominio.DTO.Reportes
{
    public class ReporteNominaHdrDto
    {
        public int Nomcod { get; set; }
        public string Nomtip { get; set; } = "";
        public string Nomstd { get; set; } = "";
        public DateTime Nomfdi { get; set; }
        public DateTime Nomfdf { get; set; }
        public string Nomudc { get; set; } = "";   // Elaboró (creó)
        public string? Nomuda { get; set; }        // Autorizó (puede ser null)
    }
}
