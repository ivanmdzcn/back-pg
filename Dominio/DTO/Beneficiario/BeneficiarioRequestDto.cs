namespace Dominio.DTO.Beneficiario
{
    public class BeneficiarioRequestDto
    {
        public int Bencau { get; set; }     // causanteId (FK obligatorio)
        public int Bencod { get; set; }     // correlativo/num beneficiario (o lo generas en DAO)
        public string Benno1 { get; set; } = "";
        public string? Benno2 { get; set; }
        public string Benap1 { get; set; } = "";
        public string? Benap2 { get; set; }
        public string Benres { get; set; } = "";
        public string Bendpi { get; set; } = "";
        public string Benpar { get; set; } = "";  // '1','2',...
        public decimal Benmon { get; set; }
        public string? Bensit { get; set; }     // 'V','T'
        public int? Bentrm { get; set; }        // 1,2...
        public int? Benate { get; set; }
        public string Benudg { get; set; } = ""; // usuario
    }
}
