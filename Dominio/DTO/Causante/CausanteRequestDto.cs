namespace Dominio.DTO.Causante
{
    public class CausanteRequestDto
    {
        public string Afi { get; set; }
        public string Nombre1 { get; set; }
        public string Nombre2 { get; set; }
        public string Apellido1 { get; set; }
        public string Apellido2 { get; set; }
        //public string NombreCompleto { get; set; }
        public string Direccion { get; set; }
        public string Dpi { get; set; }
        public string Estado { get; set; } // Código del estado (A, B, etc.)
        public string Usuario { get; set; } // Quien lo está creando
    }
}
