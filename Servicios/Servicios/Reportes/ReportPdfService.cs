// Servicios/Reportes/ReportPdfService.cs
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Dominio.DTO.Reportes;

namespace Servicios.Servicios.Reportes
{
    public class ReportPdfService
    {
        public byte[] CrearPdf(ReporteNominaHdrDto hdr, NominaDetalleReporteDto det, string? logoBase64 = null)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var bytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    // Header (logo + título)
                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("DEMO PROYECTO DE GRADUACION II")
                                     .SemiBold().FontSize(12);
                            col.Item().Text("Reporte de Nómina").Bold().FontSize(16);
                            col.Item().Text($"Nómina No.: {hdr.Nomcod}  |  Tipo: {hdr.Nomtip}  |  Estado: {EstadoBonito(hdr.Nomstd)}");
                            col.Item().Text($"Del: {hdr.Nomfdi:dd-MM-yyyy}   Al: {hdr.Nomfdf:dd-MM-yyyy}");
                        });

                        if (!string.IsNullOrWhiteSpace(logoBase64))
                        {
                            var img = Convert.FromBase64String(logoBase64);
                            row.ConstantItem(90).Height(60).Image(img);
                        }
                    });

                    // Contenido
                    page.Content().PaddingVertical(10).Column(col =>
                    {
                        col.Item()
                           .PaddingBottom(5)        
                           .Text(txt =>
                           {
                               txt.Span($"Total beneficiarios: {det.TotalBeneficiarios}   |   ")
                                  .Bold().FontSize(11);

                               txt.Span($"Total monto: {det.TotalMonto:n2}")
                                  .Bold().FontSize(11);
                           });

                        // Tabla detalle
                        col.Item().Table(tbl =>
                        {
                            tbl.ColumnsDefinition(cols =>
                            {
                                cols.RelativeColumn(1); // cod nomina
                                cols.RelativeColumn(3); // causante
                                cols.RelativeColumn(3); // beneficiario
                                cols.RelativeColumn(2); // monto
                            });

                            // Encabezado tabla
                            tbl.Header(h =>
                            {
                                h.Cell().Element(CellHead).Text("No.");
                                h.Cell().Element(CellHead).Text("Causante");
                                h.Cell().Element(CellHead).Text("Beneficiario");
                                h.Cell().Element(CellHead).Text("Monto");

                                static IContainer CellHead(IContainer c) => c.DefaultTextStyle(x => x.Bold())
                                    .Padding(4).Background(Colors.Grey.Lighten3).BorderBottom(1).BorderColor(Colors.Grey.Darken2);
                            });

                            foreach (var r in det.Lineas)
                            {
                                tbl.Cell().Element(Cell).Text(r.Detcod.ToString());
                                tbl.Cell().Element(Cell).Text($"{r.Detcau} — {r.CausanteNombre}");
                                tbl.Cell().Element(Cell).Text($"{r.Detben} — {r.BeneficiarioNombre}");
                                tbl.Cell().Element(Cell).AlignRight().Text($"{r.Detmon:n2}");

                                static IContainer Cell(IContainer c) => c.Padding(4).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1);
                            }
                        });

                        // Firmas
                        col.Item().PaddingTop(25).Row(r =>
                        {
                            r.RelativeItem().Column(cc =>
                            {
                                cc.Item().Text("Elaboró").Bold();
                                cc.Item().PaddingTop(35).Text("_______________________________");
                                cc.Item().Text(hdr.Nomudc);
                            });
                            r.RelativeItem().Column(cc =>
                            {
                                cc.Item().Text("Autorizó").Bold();
                                cc.Item().PaddingTop(35).Text("_______________________________");
                                cc.Item().Text(string.IsNullOrWhiteSpace(hdr.Nomuda) ? "(pendiente)" : hdr.Nomuda);
                            });
                        });
                    });

                    // Pie de página
                    page.Footer().AlignRight().Text(txt =>
                    {
                        txt.Span("Generado el ").Italic();
                        txt.Span($"{DateTime.Now:dd-MM-yyyy HH:mm}");
                    });
                });
            }).GeneratePdf();

            return bytes;
        }

        private static string EstadoBonito(string cod) => cod switch
        {
            "A" => "Autorizada",
            "B" => "Borrador",
            "C" => "Cancelada",
            _ => cod
        };
    }
}
