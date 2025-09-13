namespace Dominio.DTO.Beneficiario
{
    public class BeneficiarioEditDto
    {
        public int Bencau { get; set; }
        public int Bencod { get; set; }
        public string Benno1 { get; set; } = "";
        public string? Benno2 { get; set; }
        public string Benap1 { get; set; } = "";
        public string? Benap2 { get; set; }
        public string Benres { get; set; } = "";
        public string Bendpi { get; set; } = "";
        public string Benpar { get; set; } = "";
        public decimal Benmon { get; set; }
        public string? Bensit { get; set; }   // 'V','T' o null
        public int? Bentrm { get; set; }
        public int? Benate { get; set; }
        public string? Benudg { get; set; }   // usuario que modifica
    }
}
