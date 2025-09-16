using AccesoDatos.Nomina;
using Dominio.DTO.Nomina;
using Servicios.Interfaces.Nomina;

namespace Servicios.Servicios.Nomina
{
    public class NominaService : INominaService
    {
        private readonly NominaDao _dao;
        public NominaService(NominaDao dao) { _dao = dao; }

        public int Crear(NominaCreateDto dto, string usuario) => _dao.Crear(dto, usuario);

        public List<NominaHdrDto> Listar(string usuario, string rol)
        {
            bool esAdmin = string.Equals(rol, "ADMIN", StringComparison.OrdinalIgnoreCase);
            bool esAnalista = string.Equals(rol, "ANALISTA", StringComparison.OrdinalIgnoreCase);

            // Admin y Analista ven TODO; Operador solo sus propias nóminas
            return _dao.Listar(usuario, esAdmin || esAnalista);
        }

        public NominaDto? Obtener(int nomcod, string usuario, string rol)
        {
            var hdr = _dao.ObtenerHdr(nomcod);
            if (hdr == null) return null;

            bool esAdmin = string.Equals(rol, "ADMIN", StringComparison.OrdinalIgnoreCase);
            bool esAnalista = string.Equals(rol, "ANALISTA", StringComparison.OrdinalIgnoreCase);
            bool esDueno = string.Equals(hdr.Nomudc, usuario, StringComparison.OrdinalIgnoreCase);

            var permisos = new NominaPermisos
            {
                // Analista puede ver todo; Admin también; Operador solo si es dueño
                PuedeVer = esAdmin || esAnalista || esDueno,

                // Editar encabezado solo en borrador y si es Admin o Dueño (Analista NO edita)
                PuedeEditarEncabezado = hdr.Nomstd == "B" && (esAdmin || esDueno),

                // Autorizar / Cancelar: solo Admin (con las reglas de estado)
                PuedeAutorizar = hdr.Nomstd == "B" && esAdmin,
                PuedeCancelar = (hdr.Nomstd == "B" || hdr.Nomstd == "A") && esAdmin
            };

            if (!permisos.PuedeVer) return null;

            return new NominaDto
            {
                Nomcod = hdr.Nomcod,
                Nomtip = hdr.Nomtip,
                Nomstd = hdr.Nomstd,
                Nomfdi = hdr.Nomfdi,
                Nomfdf = hdr.Nomfdf,
                Nomudc = hdr.Nomudc,
                Nomuda = hdr.Nomuda,
                Nomfau = hdr.Nomfau,
                Permisos = permisos
            };
        }

        public bool Autorizar(int nomcod, string usuarioAdmin) => _dao.Autorizar(nomcod, usuarioAdmin);
        public bool Cancelar(int nomcod, string usuarioAdmin) => _dao.Cancelar(nomcod, usuarioAdmin);
        public bool ActualizarFechas(int nomcod, DateTime fdi, DateTime fdf) => _dao.ActualizarFechas(nomcod, fdi, fdf);
    }
}
