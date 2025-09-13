namespace Dominio.DTO.Beneficiario
{
    public class BeneficiarioResponseDto
    {
        public int Bencau { get; set; }        // CausanteId (FK)
        public int Bencod { get; set; }        // Id/correlativo del beneficiario
        public string Benno1 { get; set; } = "";
        public string Benno2 { get; set; } = "";
        public string Benap1 { get; set; } = "";
        public string Benap2 { get; set; } = "";
        public string Bennoc { get; set; } = "";  // Nombre completo
        public string Benres { get; set; } = "";  // Residencia
        public string Bendpi { get; set; } = "";
        public string Benpar { get; set; } = "";  // Código parentesco
        public decimal Benmon { get; set; }       // Monto
        public string Bensit { get; set; } = "";  // Código situación
        public int? Bentrm { get; set; }        // Código terminación
        public int? Benate { get; set; }        // Años/tiempo?
        public string Fdg { get; set; } = ""; // Fecha (DD-MM-YYYY)
        public string Hdg { get; set; } = ""; // Hora
        public string Benudg { get; set; } = "";

        // Descripciones (joins a catálogos)
        public string? ParentescoDesc { get; set; }
        public string? SituacionDesc { get; set; }
        public string? TerminacionDesc { get; set; }
    }
}
