namespace Dominio.DTO.Reportes
{
    public class NominaDetalleLineaDto
    {
        public int Detcod { get; set; }
        public int Detcau { get; set; }
        public int Detben { get; set; }
        public decimal Detmon { get; set; }
        // NUEVO
        public string? CausanteNombre { get; set; }
        public string? BeneficiarioNombre { get; set; }
    }
}

