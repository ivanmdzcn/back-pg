namespace Dominio.DTO.Nomina
{
    public class NominaDto
    {
        public int Nomcod { get; set; }
        public string Nomtip { get; set; } = "";
        public string Nomstd { get; set; } = "";
        public DateTime Nomfdi { get; set; }
        public DateTime Nomfdf { get; set; }
        public string Nomudc { get; set; } = "";
        public string? Nomuda { get; set; }
        public DateTime? Nomfau { get; set; }

        public List<NominaDetalleDto> Detalle { get; set; } = new();
        public NominaPermisos Permisos { get; set; } = new();
    }
}
