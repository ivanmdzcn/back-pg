namespace Dominio.DTO.Nomina
{
    public class NominaHdrDto
    {
        public int Nomcod { get; set; }
        public string Nomtip { get; set; } = "";   // 'P'
        public string Nomstd { get; set; } = "";   // 'B','A','C'
        public DateTime Nomfdi { get; set; }
        public DateTime Nomfdf { get; set; }
        public string Nomudc { get; set; } = "";   // usuario creador
        public string? Nomuda { get; set; }        // usuario que autorizó
        public DateTime? Nomfau { get; set; }
    }
}
