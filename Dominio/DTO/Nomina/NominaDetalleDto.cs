// Dominio/DTO/Nomina/NominaDetalleDto.cs
public class NominaDetalleDto
{
    public int Detcod { get; set; }
    public int Detcau { get; set; }
    public int Detben { get; set; }
    public decimal Detmon { get; set; }
    // ...otros campos que quieras mostrar del beneficiario
}

// Dominio/DTO/Nomina/NominaDto.cs
public class NominaDto
{
    // Encabezado
    public int Nomcod { get; set; }
    public string Nomtip { get; set; } = "";   // 'P'
    public string Nomstd { get; set; } = "";   // 'B'|'A'|'C'
    public DateTime Nomfdi { get; set; }
    public DateTime Nomfdf { get; set; }
    public string Nomudc { get; set; } = "";   // creador
    public string? Nomuda { get; set; }        // autorizó
    public DateTime? Nomfau { get; set; }

    public List<NominaDetalleDto> Detalle { get; set; } = new();

    // Permisos calculados según usuario/rol/estado
    public NominaPermisos Permisos { get; set; } = new();
}

// Dominio/DTO/Nomina/NominaPermisos.cs
public class NominaPermisos
{
    public bool PuedeVer { get; set; }
    public bool PuedeEditarEncabezado { get; set; }
    public bool PuedeAgregarDetalle { get; set; }
    public bool PuedeEliminarDetalle { get; set; }
    public bool PuedeAutorizar { get; set; }
    public bool PuedeCancelar { get; set; }
}
