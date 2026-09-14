using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using ContaFlow.API.Entities;
using ContaFlow.API.Features.Recibos.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ContaFlow.API.Features.Recibos
{
    public class ReciboDocument : IDocument
    {
        private readonly Recibo _recibo;
        private readonly ConfiguracionDespacho _config;
        private readonly List<ReciboItemDto> _items;

        public ReciboDocument(Recibo recibo, ConfiguracionDespacho? config = null)
        {
            _recibo = recibo;
            _config = config ?? new ConfiguracionDespacho();

            _items = new List<ReciboItemDto>();
            if (!string.IsNullOrWhiteSpace(_recibo.ItemsJson))
            {
                try
                {
                    var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    _items = JsonSerializer.Deserialize<List<ReciboItemDto>>(_recibo.ItemsJson, opts) ?? new();
                }
                catch
                {
                    _items = new();
                }
            }

            if (_items.Count == 0)
            {
                _items.Add(new ReciboItemDto
                {
                    Producto = !string.IsNullOrWhiteSpace(_recibo.Concepto) ? _recibo.Concepto : "Honorarios Profesionales",
                    Descripcion = !string.IsNullOrWhiteSpace(_recibo.Concepto) ? _recibo.Concepto : "Servicios Contables y Asesoría Fiscal",
                    Cantidad = 1,
                    Precio = _recibo.Monto,
                    Total = _recibo.Monto
                });
            }
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;
        public DocumentSettings GetSettings() => DocumentSettings.Default;

        public void Compose(IDocumentContainer container)
        {
            var esConCai = !string.IsNullOrWhiteSpace(_recibo.Cai) || string.Equals(_recibo.TipoComprobante, "ConCAI", StringComparison.OrdinalIgnoreCase);

            container
                .Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    page.Margin(30);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Helvetica"));

                    if (esConCai)
                    {
                        page.Header().Element(ComposeHeaderConCai);
                        page.Content().Element(ComposeContentConCai);
                        page.Footer().Element(ComposeFooterConCai);
                    }
                    else
                    {
                        page.Header().Element(ComposeHeaderInformal);
                        page.Content().Element(ComposeContentInformal);
                        page.Footer().Element(ComposeFooterInformal);
                    }
                });
        }

        // =========================================================================
        // DISEÑO 1: RECIBO / FACTURA INFORMAL (IMAGE 3 - LÍDERES CONTABLES)
        // =========================================================================
        private void ComposeHeaderInformal(IContainer container)
        {
            var nombreDespacho = !string.IsNullOrWhiteSpace(_config.NombreDespacho) 
                ? _config.NombreDespacho.ToUpper() 
                : "LIDERES CONTABLES ORDOÑEZ Y ASOCIADOS";

            var titular = !string.IsNullOrWhiteSpace(_config.NombreContadorTitular)
                ? _config.NombreContadorTitular.ToUpper()
                : "JOSE VIDAL ORDOÑEZ GALO";

            var rtn = !string.IsNullOrWhiteSpace(_config.RtnDespacho) ? _config.RtnDespacho : "06011969003369";
            var direccion = !string.IsNullOrWhiteSpace(_config.Direccion) ? _config.Direccion.ToUpper() : "BARRIO LA LIBERTAD ANTIGUAS OFICINAS EEH, 3 CDR AL OESTE";
            var email = !string.IsNullOrWhiteSpace(_config.Email) ? _config.Email.ToUpper() : "JOSEVIDAL.ORDONEZGALO@GMAIL.COM";
            var tel = !string.IsNullOrWhiteSpace(_config.Telefono) ? _config.Telefono : "9279-5295";

            byte[]? logoBytes = null;
            if (!string.IsNullOrWhiteSpace(_config.LogoBase64))
            {
                try
                {
                    var base64Data = _config.LogoBase64;
                    if (base64Data.Contains(","))
                    {
                        base64Data = base64Data.Substring(base64Data.IndexOf(",") + 1);
                    }
                    logoBytes = Convert.FromBase64String(base64Data);
                }
                catch
                {
                    logoBytes = null;
                }
            }

            container.Column(col =>
            {
                col.Item().Row(row =>
                {
                    if (logoBytes != null && logoBytes.Length > 0)
                    {
                        row.ConstantItem(60).PaddingRight(10).AlignMiddle().Image(logoBytes).FitArea();
                    }
                    else
                    {
                        // Logo circular dorado con águila / ícono representativo
                        row.ConstantItem(48).Height(48).Background("#fef08a").Border(1.5f).BorderColor("#eab308")
                            .AlignCenter().AlignMiddle().Text("🦅").FontSize(22);
                    }

                    row.RelativeItem().PaddingLeft(10).Column(c =>
                    {
                        c.Item().Text(nombreDespacho)
                            .Bold().FontSize(14).FontColor("#0f172a");

                        c.Item().PaddingTop(2).Text($"PROP. {titular} | RTN: {rtn} | {direccion} | EMAIL: {email} | TEL. {tel}")
                            .FontSize(6.5f).FontColor("#475569").SemiBold();
                    });
                });

                // Barra amarilla de información del cliente (Light Yellow #fef9c3)
                col.Item().PaddingTop(12).Border(1).BorderColor("#facc15").Background("#fef9c3").Padding(7).Row(cRow =>
                {
                    cRow.RelativeItem(2).Column(c =>
                    {
                        c.Item().Text("RTN:").Bold().FontSize(7.5f).FontColor("#713f12");
                        c.Item().Text(_recibo.RtnCliente ?? "N/A").Bold().FontSize(9f).FontColor("#0f172a");
                    });

                    cRow.RelativeItem(3).Column(c =>
                    {
                        c.Item().Text("CLIENTE:").Bold().FontSize(7.5f).FontColor("#713f12");
                        c.Item().Text(_recibo.NombreCliente ?? "Cliente General").Bold().FontSize(9f).FontColor("#0f172a");
                    });

                    cRow.RelativeItem(2).Column(c =>
                    {
                        c.Item().Text("# FACTURA / RECIBO:").Bold().FontSize(7.5f).FontColor("#713f12");
                        c.Item().Text(_recibo.NumeroRecibo).Bold().FontSize(9.5f).FontColor("#b45309");
                    });

                    cRow.RelativeItem(2).Column(c =>
                    {
                        c.Item().Text("FECHA EMISIÓN:").Bold().FontSize(7.5f).FontColor("#713f12");
                        c.Item().Text($"{_recibo.FechaEmision:dd/MM/yyyy}").Bold().FontSize(9f).FontColor("#0f172a");
                    });
                });
            });
        }

        private void ComposeContentInformal(IContainer container)
        {
            container.PaddingTop(12).Column(col =>
            {
                // TABLA CON COLUMNAS EXACTAS: PRODUCTO | DESCRIPCIÓN | CANTIDAD | PRECIO | TOTAL
                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3); // PRODUCTO
                        columns.RelativeColumn(4); // DESCRIPCIÓN
                        columns.ConstantColumn(55); // CANTIDAD
                        columns.ConstantColumn(75); // PRECIO
                        columns.ConstantColumn(80); // TOTAL
                    });

                    // Encabezados en Slate / Navy oscuro
                    table.Header(header =>
                    {
                        header.Cell().Background("#0f172a").PaddingVertical(6).PaddingHorizontal(6)
                            .Text("PRODUCTO").Bold().FontSize(8f).FontColor(Colors.White);

                        header.Cell().Background("#0f172a").PaddingVertical(6).PaddingHorizontal(6)
                            .Text("DESCRIPCIÓN").Bold().FontSize(8f).FontColor(Colors.White);

                        header.Cell().Background("#0f172a").PaddingVertical(6).PaddingHorizontal(6)
                            .AlignCenter().Text("CANTIDAD").Bold().FontSize(8f).FontColor(Colors.White);

                        header.Cell().Background("#0f172a").PaddingVertical(6).PaddingHorizontal(6)
                            .AlignRight().Text("PRECIO").Bold().FontSize(8f).FontColor(Colors.White);

                        header.Cell().Background("#0f172a").PaddingVertical(6).PaddingHorizontal(6)
                            .AlignRight().Text("TOTAL").Bold().FontSize(8f).FontColor(Colors.White);
                    });

                    // Filas de ítems
                    for (int i = 0; i < _items.Count; i++)
                    {
                        var it = _items[i];
                        var bg = i % 2 == 0 ? "#ffffff" : "#f8fafc";

                        table.Cell().Background(bg).BorderBottom(0.5f).BorderColor("#e2e8f0").PaddingVertical(6).PaddingHorizontal(6)
                            .Text(it.Producto).Bold().FontSize(8f).FontColor("#1e293b");

                        table.Cell().Background(bg).BorderBottom(0.5f).BorderColor("#e2e8f0").PaddingVertical(6).PaddingHorizontal(6)
                            .Text(it.Descripcion).FontSize(8f).FontColor("#475569");

                        table.Cell().Background(bg).BorderBottom(0.5f).BorderColor("#e2e8f0").PaddingVertical(6).PaddingHorizontal(6)
                            .AlignCenter().Text(it.Cantidad.ToString("G29")).FontSize(8f).FontColor("#1e293b");

                        table.Cell().Background(bg).BorderBottom(0.5f).BorderColor("#e2e8f0").PaddingVertical(6).PaddingHorizontal(6)
                            .AlignRight().Text($"L. {it.Precio:N2}").FontSize(8f).FontColor("#1e293b");

                        table.Cell().Background(bg).BorderBottom(0.5f).BorderColor("#e2e8f0").PaddingVertical(6).PaddingHorizontal(6)
                            .AlignRight().Text($"L. {it.Total:N2}").Bold().FontSize(8.5f).FontColor("#0f172a");
                    }
                });

                // SECCIÓN INFERIOR: MENSAJE TRIBUTARIO (IZQUIERDA) Y TOTALES (DERECHA)
                col.Item().PaddingTop(15).Row(bRow =>
                {
                    // Cuadro de Cita Tributaria Amarilla
                    bRow.RelativeItem(5).Border(1).BorderColor("#facc15").Background("#fef9c3").Padding(10).Column(qCol =>
                    {
                        qCol.Item().Row(qr =>
                        {
                            qr.ConstantItem(18).Text("“").Bold().FontSize(18).FontColor("#ca8a04");
                            qr.RelativeItem().Text(
                                "La tributación no es solo una obligación, es una herramienta clave para el desarrollo de un país y la sostenibilidad de las empresas; conocer y aplicar correctamente las normas fiscales permite tomar decisiones inteligentes, evitar sanciones y contribuir al bienestar colectivo."
                            ).Italic().FontSize(7.2f).FontColor("#713f12").LineHeight(1.25f);
                        });
                    });

                    bRow.ConstantItem(15); // Espaciador

                    // Cuadro de Totales
                    var subtotal = _recibo.Subtotal > 0 ? _recibo.Subtotal : _items.Sum(x => x.Total);
                    var impuesto = _recibo.Impuesto;
                    var total = _recibo.Monto > 0 ? _recibo.Monto : (subtotal + impuesto);

                    bRow.RelativeItem(4).Border(1).BorderColor("#cbd5e1").Background("#f8fafc").Padding(8).Column(tCol =>
                    {
                        tCol.Item().Row(r =>
                        {
                            r.RelativeItem().Text("SUBTOTAL:").Bold().FontSize(8f).FontColor("#475569");
                            r.RelativeItem().AlignRight().Text($"L. {subtotal:N2}").FontSize(8.5f).FontColor("#1e293b");
                        });

                        tCol.Item().PaddingTop(3).Row(r =>
                        {
                            r.RelativeItem().Text("IMPUESTO:").Bold().FontSize(8f).FontColor("#475569");
                            r.RelativeItem().AlignRight().Text($"L. {impuesto:N2}").FontSize(8.5f).FontColor("#1e293b");
                        });

                        tCol.Item().PaddingTop(5).LineHorizontal(1).LineColor("#cbd5e1");

                        tCol.Item().PaddingTop(5).Row(r =>
                        {
                            r.RelativeItem().Text("TOTAL:").Bold().FontSize(10f).FontColor("#0f172a");
                            r.RelativeItem().AlignRight().Text($"L. {total:N2}").Bold().FontSize(12f).FontColor("#059669");
                        });
                    });
                });

                // SECCIÓN DE FIRMA Y SELLO
                col.Item().PaddingTop(35).Row(row =>
                {
                    row.RelativeItem(2);
                    row.RelativeItem(3).Column(c =>
                    {
                        c.Item().LineHorizontal(1).LineColor("#94a3b8");
                        c.Item().PaddingTop(4).AlignCenter().Text("FIRMA Y SELLO").Bold().FontSize(8.5f).FontColor("#0f172a");
                        c.Item().AlignCenter().Text(_config.NombreContadorTitular ?? "José Vidal Ordóñez Galo").FontSize(7.5f).FontColor("#475569");
                        if (!string.IsNullOrWhiteSpace(_config.ColegiacionCAH))
                        {
                            c.Item().AlignCenter().Text(_config.ColegiacionCAH).FontSize(7f).FontColor("#64748b");
                        }
                    });
                });
            });
        }

        private void ComposeFooterInformal(IContainer container)
        {
            container.AlignCenter().Text("¡Gracias por su preferencia y confianza en nuestros servicios profesionales contables!")
                .FontSize(7f).FontColor("#94a3b8").Italic();
        }

        // =========================================================================
        // DISEÑO 2: RECIBO OFICIAL CON CAI (SAR)
        // =========================================================================
        private void ComposeHeaderConCai(IContainer container)
        {
            container.BorderBottom(1).BorderColor("#e2e8f0").PaddingBottom(10).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text((_config.NombreDespacho ?? "DESPACHO CONTABLE Y FISCAL").ToUpper())
                        .FontSize(12).Bold().FontColor("#0f172a");

                    col.Item().Text(t =>
                    {
                        t.Span("RTN: ").Bold().FontSize(7.5f).FontColor("#334155");
                        t.Span($"{_config.RtnDespacho}  •  ").FontSize(7.5f).FontColor("#475569");
                        t.Span("Titular: ").Bold().FontSize(7.5f).FontColor("#334155");
                        t.Span($"{_config.NombreContadorTitular} ({_config.ColegiacionCAH})").FontSize(7.5f).FontColor("#475569");
                    });

                    col.Item().Text(t =>
                    {
                        t.Span("Tel: ").Bold().FontSize(7.5f).FontColor("#334155");
                        t.Span($"{_config.Telefono}  •  ").FontSize(7.5f).FontColor("#475569");
                        t.Span("Email: ").Bold().FontSize(7.5f).FontColor("#334155");
                        t.Span($"{_config.Email}").FontSize(7.5f).FontColor("#475569");
                    });

                    col.Item().Text(_config.Direccion).FontSize(7).FontColor("#64748b");
                });

                row.ConstantItem(180).Column(col =>
                {
                    col.Item().Border(1.5f).BorderColor("#cbd5e1").Background("#f8fafc").Padding(6).Column(c =>
                    {
                        c.Item().AlignCenter().Text("FACTURA / RECIBO FISCAL").Bold().FontSize(8.5f).FontColor("#0f172a");
                        c.Item().AlignCenter().Text(_recibo.NumeroFiscal ?? _recibo.NumeroRecibo).Bold().FontSize(10.5f).FontColor("#4f46e5");
                        c.Item().AlignCenter().Text($"Fecha: {_recibo.FechaEmision:dd/MM/yyyy}").FontSize(7.5f).FontColor("#64748b");
                    });
                });
            });
        }

        private void ComposeContentConCai(IContainer container)
        {
            container.PaddingTop(10).Column(col =>
            {
                // BLOQUE CAI DEL SAR
                col.Item().Border(1).BorderColor("#cbd5e1").Background("#f8fafc").Padding(6).Row(caiRow =>
                {
                    caiRow.RelativeItem(3).Column(caiCol =>
                    {
                        caiCol.Item().Text(t =>
                        {
                            t.Span("CAI (SAR): ").Bold().FontSize(7.5f).FontColor("#1e293b");
                            t.Span(_recibo.Cai ?? "N/A").FontSize(7.5f).FontColor("#0f172a");
                        });
                        if (!string.IsNullOrWhiteSpace(_recibo.RangoAutorizado))
                        {
                            caiCol.Item().Text(t =>
                            {
                                t.Span("Rango Autorizado: ").Bold().FontSize(7f).FontColor("#475569");
                                t.Span(_recibo.RangoAutorizado).FontSize(7f).FontColor("#334155");
                            });
                        }
                    });

                    if (_recibo.FechaLimiteEmision.HasValue)
                    {
                        caiRow.RelativeItem(2).AlignRight().Column(fCol =>
                        {
                            fCol.Item().Text(t =>
                            {
                                t.Span("Fecha Límite de Emisión: ").Bold().FontSize(7f).FontColor("#475569");
                                t.Span($"{_recibo.FechaLimiteEmision.Value:dd/MM/yyyy}").FontSize(7f).Bold().FontColor("#b91c1c");
                            });
                        });
                    }
                });

                // DATOS DEL CLIENTE
                col.Item().PaddingTop(8).Border(1).BorderColor("#e2e8f0").Background("#ffffff").Padding(6).Row(cRow =>
                {
                    cRow.RelativeItem(3).Text(t =>
                    {
                        t.Span("CLIENTE: ").Bold().FontSize(8f).FontColor("#475569");
                        t.Span(_recibo.NombreCliente ?? "Cliente General").Bold().FontSize(8.5f).FontColor("#0f172a");
                    });
                    cRow.RelativeItem(2).Text(t =>
                    {
                        t.Span("RTN: ").Bold().FontSize(8f).FontColor("#475569");
                        t.Span(_recibo.RtnCliente ?? "N/A").Bold().FontSize(8.5f).FontColor("#0f172a");
                    });
                });

                // TABLA DE ITEMS
                col.Item().PaddingTop(8).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3); // PRODUCTO
                        columns.RelativeColumn(4); // DESCRIPCIÓN
                        columns.ConstantColumn(55); // CANTIDAD
                        columns.ConstantColumn(75); // PRECIO
                        columns.ConstantColumn(80); // TOTAL
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background("#1e293b").Padding(5).Text("PRODUCTO").Bold().FontSize(8f).FontColor(Colors.White);
                        header.Cell().Background("#1e293b").Padding(5).Text("DESCRIPCIÓN").Bold().FontSize(8f).FontColor(Colors.White);
                        header.Cell().Background("#1e293b").Padding(5).AlignCenter().Text("CANTIDAD").Bold().FontSize(8f).FontColor(Colors.White);
                        header.Cell().Background("#1e293b").Padding(5).AlignRight().Text("PRECIO").Bold().FontSize(8f).FontColor(Colors.White);
                        header.Cell().Background("#1e293b").Padding(5).AlignRight().Text("TOTAL").Bold().FontSize(8f).FontColor(Colors.White);
                    });

                    for (int i = 0; i < _items.Count; i++)
                    {
                        var it = _items[i];
                        var bg = i % 2 == 0 ? "#ffffff" : "#f8fafc";

                        table.Cell().Background(bg).BorderBottom(0.5f).BorderColor("#e2e8f0").Padding(5).Text(it.Producto).Bold().FontSize(8f);
                        table.Cell().Background(bg).BorderBottom(0.5f).BorderColor("#e2e8f0").Padding(5).Text(it.Descripcion).FontSize(8f);
                        table.Cell().Background(bg).BorderBottom(0.5f).BorderColor("#e2e8f0").Padding(5).AlignCenter().Text(it.Cantidad.ToString("G29")).FontSize(8f);
                        table.Cell().Background(bg).BorderBottom(0.5f).BorderColor("#e2e8f0").Padding(5).AlignRight().Text($"L. {it.Precio:N2}").FontSize(8f);
                        table.Cell().Background(bg).BorderBottom(0.5f).BorderColor("#e2e8f0").Padding(5).AlignRight().Text($"L. {it.Total:N2}").Bold().FontSize(8f);
                    }
                });

                // TOTALES Y MONTO EN LETRAS
                var subtotal = _recibo.Subtotal > 0 ? _recibo.Subtotal : _items.Sum(x => x.Total);
                var impuesto = _recibo.Impuesto;
                var total = _recibo.Monto > 0 ? _recibo.Monto : (subtotal + impuesto);

                col.Item().PaddingTop(8).Row(r =>
                {
                    r.RelativeItem(5).Column(mc =>
                    {
                        mc.Item().Text("SON:").Bold().FontSize(7.5f).FontColor("#475569");
                        mc.Item().Text(_recibo.MontoEnLetras).Italic().FontSize(8f).FontColor("#0f172a");
                    });

                    r.RelativeItem(4).Border(1).BorderColor("#cbd5e1").Background("#f8fafc").Padding(6).Column(tc =>
                    {
                        tc.Item().Row(tr =>
                        {
                            tr.RelativeItem().Text("SUBTOTAL:").Bold().FontSize(8f);
                            tr.RelativeItem().AlignRight().Text($"L. {subtotal:N2}").FontSize(8f);
                        });
                        tc.Item().Row(tr =>
                        {
                            tr.RelativeItem().Text("ISV 15%:").Bold().FontSize(8f);
                            tr.RelativeItem().AlignRight().Text($"L. {impuesto:N2}").FontSize(8f);
                        });
                        tc.Item().LineHorizontal(0.5f).LineColor("#cbd5e1");
                        tc.Item().Row(tr =>
                        {
                            tr.RelativeItem().Text("TOTAL:").Bold().FontSize(9.5f);
                            tr.RelativeItem().AlignRight().Text($"L. {total:N2}").Bold().FontSize(11f).FontColor("#059669");
                        });
                    });
                });

                // FIRMA
                col.Item().PaddingTop(30).Row(row =>
                {
                    row.RelativeItem(2);
                    row.RelativeItem(3).Column(c =>
                    {
                        c.Item().LineHorizontal(1).LineColor("#94a3b8");
                        c.Item().AlignCenter().Text("FIRMA AUTORIZADA").Bold().FontSize(8f).FontColor("#1e293b");
                        c.Item().AlignCenter().Text(_config.NombreContadorTitular ?? "Contador Titular").FontSize(7.5f).FontColor("#334155");
                    });
                });
            });
        }

        private void ComposeFooterConCai(IContainer container)
        {
            container.Column(col =>
            {
                col.Item().AlignCenter().Text("ORIGINAL: CLIENTE   •   COPIA: OBLIGADO TRIBUTARIO EMISOR")
                    .Bold().FontSize(7f).FontColor("#475569");
                col.Item().AlignCenter().Text(_config.MensajePieRecibo ?? "Comprobante Fiscal Autorizado")
                    .FontSize(6.5f).FontColor("#94a3b8");
            });
        }
    }
}
