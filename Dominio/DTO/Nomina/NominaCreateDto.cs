namespace Dominio.DTO.Nomina
{
    public class NominaCreateDto
    {
        public string Nomtip { get; set; } = "P";  // tipo de nómina
        public DateTime Nomfdi { get; set; }       // desde
        public DateTime Nomfdf { get; set; }       // hasta
    }
}
