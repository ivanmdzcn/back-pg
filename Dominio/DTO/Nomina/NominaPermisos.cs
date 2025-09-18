namespace Dominio.DTO.Nomina
{
    public class NominaPermisos
    {
        public bool PuedeVer { get; set; }
        public bool PuedeEditarEncabezado { get; set; }
        public bool PuedeAgregarDetalle { get; set; }
        public bool PuedeEliminarDetalle { get; set; }
        public bool PuedeAutorizar { get; set; }
        public bool PuedeCancelar { get; set; }
    }
}
