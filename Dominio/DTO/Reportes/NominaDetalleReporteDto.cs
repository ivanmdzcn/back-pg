namespace Dominio.DTO.Reportes
{
    public class NominaDetalleReporteDto
    {
        public int Nomcod { get; set; }
        public int TotalBeneficiarios { get; set; }
        public decimal TotalMonto { get; set; }
        public List<NominaDetalleLineaDto> Lineas { get; set; } = new();
    }
}
