using Dominio.DTO.Login;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Oracle.ManagedDataAccess.Client;
using Servicios.Interfaces.Login;
using AccesoDatos.Conexion;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Servicios.Servicios.Login
{
    public class LoginService : ILoginService
    {
        private readonly IConfiguration _configuration;
        private readonly ConexionOracle _conexion;

        public LoginService(IConfiguration configuration, ConexionOracle conexion)
        {
            _configuration = configuration;
            _conexion = conexion;
        }

        public LoginResponseDto Login(LoginRequestDto request)
        {
            using var conn = _conexion.ObtenerConexion();
            conn.Open();
            using var command = conn.CreateCommand();

            // Consulta que une usuario con el nombre del rol
            command.CommandText = @"
            SELECT u.USRCOD, u.USRCON, r.CODROL, r.NOMROL
            FROM USUARIO u
            JOIN ROL r ON u.USRROL = r.CODROL
            WHERE u.USRCOD = :usuario";

            command.Parameters.Add(new OracleParameter("usuario", request.Usuario));

            //command.Parameters.Add(new OracleParameter("contrasena", request.Contrasena));

            using var reader = command.ExecuteReader();

            if (reader.Read())
            {
                string usuario = reader.GetString(0);   // USRCOD
                string hashGuardado = reader.GetString(1); //Contraseña hash
                int codRol = reader.GetInt32(2);   // CODROL -> 1, 2, ...
                string rolNombre = reader.GetString(2);      // NOMROL (ej: Administrador)

                // Mapea por ID (estable).
                string rolCodigo = codRol switch
                {
                    1 => "ADMIN",
                    2 => "OPERADOR",
                    // 3 => "ANALISTA",
                    _ => "DESCONOCIDO"
                };
                //// 👇 NUEVO: normaliza nombre -> código
                //var mapaRoles = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                //{
                //    { "Administrador", "ADMIN" },
                //    { "Operador", "OPERADOR" },
                //    { "Analista de Nómina", "ANALISTA" }
                //};
                // fallback por si aparece otro
                //string rolCodigo = mapaRoles.TryGetValue(rolNombre, out var cod)? cod: rolNombre.ToUpperInvariant(); 
                

                // Comparar la contraseña ingresada con el hash usando BCrypt
                bool contrasenaValida = BCrypt.Net.BCrypt.Verify(request.Contrasena, hashGuardado);

                if (!contrasenaValida)
                    return null;

                // Preparar valores del token
                var jwtSettings = _configuration.GetSection("Jwt");
                var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);
                var issuer = jwtSettings["Issuer"];
                var audience = jwtSettings["Audience"];

                var tokenHandler = new JwtSecurityTokenHandler();

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.Name, usuario),                    // name (ASP.NET)
                        new Claim(JwtRegisteredClaimNames.UniqueName, usuario), // unique_name (estándar)
                        new Claim("usuario", usuario),                          // custom corto para el front

                        // 👇 para [Authorize(Roles="ADMIN")] etc.
                        new Claim(ClaimTypes.Role, rolCodigo),

                        // 👇 opcionales, útiles para mostrar en UI
                        new Claim("rolCodigo", rolCodigo),
                         new Claim("rolNombre", rolNombre)

                    }),
                    Expires = DateTime.UtcNow.AddHours(1),
                    Issuer = issuer,
                    Audience = audience,
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature
                    )
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);

                return new LoginResponseDto
                {
                    Usuario = usuario,
                    Token = tokenHandler.WriteToken(token)
                };
            }

            return null; // Usuario no válido
        }
    }
}
