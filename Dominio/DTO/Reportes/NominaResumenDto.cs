namespace Dominio.DTO.Reportes
{
    public class NominaResumenDto
    {
        public int Nomcod { get; set; }
        public string Nomtip { get; set; } = "";
        public DateTime Nomfdi { get; set; }
        public DateTime Nomfdf { get; set; }
        public string Nomudc { get; set; } = "";
        public int TotalBeneficiarios { get; set; }
        public decimal TotalMonto { get; set; }
    }
}
