using Dominio.DTO.Usuario;
using AccesoDatos.Conexion;
using Microsoft.Extensions.Configuration;

namespace AccesoDatos.Usuario
{
    public class UsuarioDao
    {
        private readonly ConexionOracle _conexion;

        public UsuarioDao(IConfiguration configuration)
        {
            _conexion = new ConexionOracle(configuration);
        }

        public List<UsuarioDto> ObtenerUsuarios()
        {
            List<UsuarioDto> usuarios = new();
            using (var conn = _conexion.ObtenerConexion())
            {
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT USRCOD, USRNOM, USRCON, USRROL, USRFDG, USRHDG, USRUDG FROM USUARIO";

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    usuarios.Add(new UsuarioDto
                    {
                        Codigo = reader.GetString(0),
                        Nombre = reader.GetString(1),
                        Contrasena = reader.GetString(2),
                        Rol = reader.GetInt32(3),
                        FechaCreacion = reader.GetDateTime(4),
                        HoraCreacion = reader.GetString(5),
                        UsuarioCreador = reader.IsDBNull(6) ? null : reader.GetString(6)
                    });
                }
            }
            return usuarios;
        }
    }
}
