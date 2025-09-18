using AccesoDatos.Nomina;
using Dominio.DTO.Nomina;
using Servicios.Interfaces.Nomina;

namespace Servicios.Servicios.Nomina
{
    public class NominaService : INominaService
    {
        private readonly NominaDao _dao;
        public NominaService(NominaDao dao) { _dao = dao; }

        // ===== Encabezado =====
        public int Crear(NominaCreateDto dto, string usuario) => _dao.Crear(dto, usuario);

        public List<NominaHdrDto> Listar(string usuario, string rol)
        {
            bool esAdmin = string.Equals(rol, "ADMIN", StringComparison.OrdinalIgnoreCase);
            bool esAnalista = string.Equals(rol, "ANALISTA", StringComparison.OrdinalIgnoreCase);

            return _dao.Listar(usuario, esAdmin || esAnalista);
        }

        public NominaDto? Obtener(int nomcod, string usuario, string rol)
        {
            var hdr = _dao.ObtenerHdr(nomcod);
            if (hdr == null) return null;

            bool esAdmin = string.Equals(rol, "ADMIN", StringComparison.OrdinalIgnoreCase);
            bool esAnalista = string.Equals(rol, "ANALISTA", StringComparison.OrdinalIgnoreCase);
            bool esDueno = string.Equals(hdr.Nomudc, usuario, StringComparison.OrdinalIgnoreCase);

            var puedeVer = esAdmin || esAnalista || esDueno;
            if (!puedeVer) return null;

            var permisos = new NominaPermisos
            {
                PuedeVer = true,
                PuedeEditarEncabezado = hdr.Nomstd == "B" && (esAdmin || esDueno),
                PuedeAutorizar = hdr.Nomstd == "B" && esAdmin,
                PuedeCancelar = (hdr.Nomstd == "B" || hdr.Nomstd == "A") && esAdmin,
                PuedeAgregarDetalle = hdr.Nomstd == "B" && (esAdmin || esDueno),
                PuedeEliminarDetalle = hdr.Nomstd == "B" && (esAdmin || esDueno)
            };

            var det = _dao.ListarDetalle(nomcod);

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
                Detalle = det,
                Permisos = permisos
            };
        }

        public bool Autorizar(int nomcod, string usuarioAdmin) => _dao.Autorizar(nomcod, usuarioAdmin);
        public bool Cancelar(int nomcod, string usuarioAdmin) => _dao.Cancelar(nomcod, usuarioAdmin);
        public bool ActualizarFechas(int nomcod, DateTime fdi, DateTime fdf) => _dao.ActualizarFechas(nomcod, fdi, fdf);

        // ===== Detalle =====
        public List<NominaDetalleDto> ListarDetalle(int nomcod) => _dao.ListarDetalle(nomcod);

        // 🔧 Ajustado: usa NominaDetalleDto y pasa campos al DAO
        public bool AgregarDetalle(int nomcod, NominaDetalleDto dto, string usuario, string rol)
        {
            var hdr = _dao.ObtenerHdr(nomcod);
            if (hdr == null) return false;

            bool esAdmin = string.Equals(rol, "ADMIN", StringComparison.OrdinalIgnoreCase);
            bool esDueno = string.Equals(hdr.Nomudc, usuario, StringComparison.OrdinalIgnoreCase);
            if (!(esAdmin || esDueno)) return false;

            return _dao.AgregarDetalle(nomcod, dto.Detcau, dto.Detben, dto.Detmon);
        }

        public bool ActualizarDetalle(int nomcod, int cau, int ben, decimal monto, string usuario, string rol)
        {
            var hdr = _dao.ObtenerHdr(nomcod);
            if (hdr == null) return false;

            bool esAdmin = string.Equals(rol, "ADMIN", StringComparison.OrdinalIgnoreCase);
            bool esDueno = string.Equals(hdr.Nomudc, usuario, StringComparison.OrdinalIgnoreCase);
            if (!(esAdmin || esDueno)) return false;

            return _dao.ActualizarDetalle(nomcod, cau, ben, monto);
        }

        public bool EliminarDetalle(int nomcod, int cau, int ben, string usuario, string rol)
        {
            var hdr = _dao.ObtenerHdr(nomcod);
            if (hdr == null) return false;

            bool esAdmin = string.Equals(rol, "ADMIN", StringComparison.OrdinalIgnoreCase);
            bool esDueno = string.Equals(hdr.Nomudc, usuario, StringComparison.OrdinalIgnoreCase);
            if (!(esAdmin || esDueno)) return false;

            return _dao.EliminarDetalle(nomcod, cau, ben);
        }
    }
}
