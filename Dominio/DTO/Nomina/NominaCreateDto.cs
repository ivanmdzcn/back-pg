namespace Dominio.DTO.Nomina
{
    public class NominaCreateDto
    {
        public string Nomtip { get; set; } = "P";  // por ahora fijo a 'P'
        public DateTime Nomfdi { get; set; }       // desde
        public DateTime Nomfdf { get; set; }       // hasta
        // NOMSTD será 'B' por defecto al crear
    }
}
