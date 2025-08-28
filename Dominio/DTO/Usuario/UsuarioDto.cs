namespace Dominio.DTO.Usuario
{
    public class UsuarioDto
    {
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string Contrasena { get; set; }
        public int Rol { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string HoraCreacion { get; set; }
        public string UsuarioCreador { get; set; }
    }
}
