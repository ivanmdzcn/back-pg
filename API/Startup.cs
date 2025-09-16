using AccesoDatos.Beneficiario;          // 👈 DAO de beneficiarios
using AccesoDatos.Causante;   // 👈 para registrar CausanteDao
using AccesoDatos.Conexion;
//Nomina
using AccesoDatos.Nomina;
using AccesoDatos.Usuario;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Servicios.Interfaces.Beneficiario; // 👈 interfaz
using Servicios.Interfaces.Causante;
using Servicios.Interfaces.Login;
using Servicios.Interfaces.Nomina;
using Servicios.Servicios.Beneficiario;  // 👈 servicio
using Servicios.Servicios.Causante;
using Servicios.Servicios.Login;
using Servicios.Servicios.Nomina;
using System.Security.Claims;
using System.Text;


namespace API
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // ConfigureServices - Configuración de dependencias y servicios
        public void ConfigureServices(IServiceCollection services)
        {
            var jwtSettings = Configuration.GetSection("Jwt");
            var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

            // Repositorios y servicios
            services.AddScoped<UsuarioDao>();
            services.AddScoped<ILoginService, LoginService>();

            //services.AddSingleton<IConfiguration>(Configuration);
            services.AddTransient<ConexionOracle>();
            services.AddScoped<CausanteDao>();
            services.AddScoped<ICausanteService, CausanteService>();

            // === Beneficiarios ===
            services.AddScoped<BeneficiarioDao>();
            services.AddScoped<IBeneficiarioService, BeneficiarioService>();

            // === Nomina ===
            services.AddScoped<NominaDao>();
            services.AddScoped<INominaService, NominaService>();

            services.AddControllers();

            // Configuración de CORS - Usando variables de entorno
            var allowedOrigins = Configuration.GetSection("CORS:AllowedOrigins").Get<string[]>()
                ?? new[] { "http://localhost:4200" }; // Fallback por defecto

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAngular",
                    policy =>
                    {
                        policy.WithOrigins(allowedOrigins)
                              .AllowAnyHeader()
                              .AllowAnyMethod()
                              .AllowCredentials(); // Para JWT en cookies si lo usas
                    });
            });

            // Swagger con JWT
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = Configuration["ApiInfo:Title"] ?? "API",
                    Version = Configuration["ApiInfo:Version"] ?? "v1"
                });

                c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Description = "Ingrese el token JWT en el campo: **Bearer {token}**"
                });

                c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
                {
                    {
                        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                        {
                            Reference = new Microsoft.OpenApi.Models.OpenApiReference
                            {
                                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            });

            // Configurar JWT
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    // Nuevos 
                    NameClaimType = ClaimTypes.Name,
                    RoleClaimType = ClaimTypes.Role
                };
            });

            services.AddAuthorization();
        }

        // Configure - Pipeline de middleware
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Middleware pipeline
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts(); // Solo en producción
            }

            // CORS DEBE IR PRIMERO (antes de HTTPS redirect)
            app.UseCors("AllowAngular");

            // Solo usar HTTPS redirect en producción
            if (!env.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}