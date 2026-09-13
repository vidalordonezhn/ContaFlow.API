using System;
using System.Collections.Generic;
using System.Globalization;
using ContaFlow.API.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ContaFlow.API.Features.Recibos
{
    public class ReciboDocument : IDocument
    {
        private readonly Recibo _recibo;
        private readonly ConfiguracionDespacho _config;

        public ReciboDocument(Recibo recibo, ConfiguracionDespacho? config = null)
        {
            _recibo = recibo;
            _config = config ?? new ConfiguracionDespacho();
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;
        public DocumentSettings GetSettings() => DocumentSettings.Default;

        public void Compose(IDocumentContainer container)
        {
            container
                .Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    page.Margin(30);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(9.5f).FontFamily("Helvetica"));

                    page.Header().Element(ComposeHeader);
                    page.Content().Element(ComposeContent);
                    page.Footer().Element(ComposeFooter);
                });
        }

        private void ComposeHeader(IContainer container)
        {
            container.BorderBottom(1).BorderColor("#e2e8f0").PaddingBottom(10).Row(row =>
            {
                // Izquierda: Logo y Datos del Despacho
                row.RelativeItem().Row(leftRow =>
                {
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

                    if (logoBytes != null && logoBytes.Length > 0)
                    {
                        leftRow.ConstantItem(60).PaddingRight(10).AlignMiddle().Image(logoBytes).FitArea();
                    }

                    leftRow.RelativeItem().Column(col =>
                    {
                        col.Item().Text(_config.NombreDespacho.ToUpper())
                            .FontSize(12).Bold().FontColor("#0f172a");

                        if (!string.IsNullOrWhiteSpace(_config.Slogan))
                        {
                            col.Item().Text(_config.Slogan)
                                .FontSize(7.5f).FontColor("#475569");
                        }

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

                        col.Item().Text(_config.Direccion)
                            .FontSize(7).FontColor("#64748b");
                    });
                });

                // Derecha: Tarjeta de Número de Recibo / Factura Fiscal
                var tituloDoc = !string.IsNullOrWhiteSpace(_recibo.Cai) ? "RECIBO POR HONORARIOS" : "RECIBO DE HONORARIOS";
                row.ConstantItem(180).Column(col =>
                {
                    col.Item().Border(1.5f).BorderColor("#cbd5e1").Background("#f8fafc").Padding(6).Column(c =>
                    {
                        c.Item().AlignCenter().Text(tituloDoc).Bold().FontSize(8.5f).FontColor("#0f172a");
                        c.Item().AlignCenter().Text(_recibo.NumeroFiscal ?? _recibo.NumeroRecibo).Bold().FontSize(10.5f).FontColor("#4f46e5");
                        c.Item().AlignCenter().Text($"Fecha: {_recibo.FechaEmision:dd/MM/yyyy}").FontSize(7.5f).FontColor("#64748b");
                    });
                });
            });
        }

        private void ComposeContent(IContainer container)
        {
            container.PaddingTop(10).Column(col =>
            {
                // BLOQUE FISCAL DEL SAR (CAI, Rango Autorizado y Fecha Límite)
                if (!string.IsNullOrWhiteSpace(_recibo.Cai))
                {
                    col.Item().Border(1).BorderColor("#cbd5e1").Background("#f8fafc").Padding(6).Row(caiRow =>
                    {
                        caiRow.RelativeItem(3).Column(caiCol =>
                        {
                            caiCol.Item().Text(t =>
                            {
                                t.Span("CAI (SAR): ").Bold().FontSize(7.5f).FontColor("#1e293b");
                                t.Span(_recibo.Cai).FontSize(7.5f).FontColor("#0f172a");
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
                }

                // Caja Monto
                col.Item().PaddingTop(6).Background("#f1f5f9").Padding(8).Row(r =>
                {
                    r.RelativeItem().Text(t =>
                    {
                        t.Span("POR VALOR DE: ").Bold().FontSize(10).FontColor("#1e293b");
                        t.Span($"L. {_recibo.Monto:N2}").Bold().FontSize(13).FontColor("#059669");
                    });
                });

                // Detalle del Pago
                col.Item().PaddingTop(8).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(120);
                        columns.RelativeColumn();
                    });

                    table.Cell().Element(BlockLabel).Text("Recibí de:").Bold().FontColor("#334155");
                    table.Cell().Element(BlockValue).Text(_recibo.NombreCliente ?? "Cliente General").Bold().FontColor("#0f172a");

                    table.Cell().Element(BlockLabel).Text("RTN Cliente:").Bold().FontColor("#334155");
                    table.Cell().Element(BlockValue).Text(_recibo.RtnCliente ?? "N/A").FontColor("#334155");

                    table.Cell().Element(BlockLabel).Text("La suma de:").Bold().FontColor("#334155");
                    table.Cell().Element(BlockValue).Text(_recibo.MontoEnLetras).Italic().FontColor("#0f172a");

                    table.Cell().Element(BlockLabel).Text("Por concepto de:").Bold().FontColor("#334155");
                    table.Cell().Element(BlockValue).Text(_recibo.Concepto).FontColor("#334155");

                    table.Cell().Element(BlockLabel).Text("Método de Pago:").Bold().FontColor("#334155");
                    table.Cell().Element(BlockValue).Text(_recibo.PagoHonorario?.MetodoPago ?? "Transferencia Bancaria").FontColor("#334155");

                    if (!string.IsNullOrWhiteSpace(_recibo.PagoHonorario?.MesAplicado))
                    {
                        table.Cell().Element(BlockLabel).Text("Periodo Aplicado:").Bold().FontColor("#334155");
                        table.Cell().Element(BlockValue).Text(_recibo.PagoHonorario.MesAplicado).FontColor("#334155");
                    }
                });

                // Sección Cuentas Bancarias para Depósito
                var bancosActivos = GetBancosActivos();
                if (bancosActivos.Count > 0)
                {
                    col.Item().PaddingTop(10).Border(1).BorderColor("#e2e8f0").Background("#fafaf9").Padding(6).Column(bankCol =>
                    {
                        bankCol.Item().Text("CUENTAS BANCARIAS AUTORIZADAS PARA DEPÓSITO / TRANSFERENCIA")
                            .Bold().FontSize(7.5f).FontColor("#475569");

                        bankCol.Item().PaddingTop(3).Row(bankRow =>
                        {
                            foreach (var b in bancosActivos)
                            {
                                bankRow.RelativeItem().PaddingRight(6).Column(bItem =>
                                {
                                    bItem.Item().Text($"• {b.Nombre} ({b.Tipo})").Bold().FontSize(7.5f).FontColor("#1e293b");
                                    bItem.Item().Text($"  Cuenta: {b.Numero}").FontSize(7.5f).FontColor("#0284c7");
                                    if (!string.IsNullOrWhiteSpace(b.Beneficiario))
                                    {
                                        bItem.Item().Text($"  A nombre de: {b.Beneficiario}").FontSize(7f).FontColor("#64748b");
                                    }
                                });
                            }
                        });
                    });
                }

                // Firma y Sello
                col.Item().PaddingTop(30).Row(row =>
                {
                    row.RelativeItem(2);
                    row.RelativeItem(3).Column(c =>
                    {
                        c.Item().LineHorizontal(1).LineColor("#94a3b8");
                        c.Item().AlignCenter().Text("FIRMA AUTORIZADA / SELLO").Bold().FontSize(8f).FontColor("#1e293b");
                        c.Item().AlignCenter().Text(_config.NombreContadorTitular).Bold().FontSize(8f).FontColor("#334155");
                        c.Item().AlignCenter().Text(_config.ColegiacionCAH).FontSize(7.5f).FontColor("#64748b");
                    });
                });
            });
        }

        private List<(string Nombre, string Tipo, string Numero, string Beneficiario)> GetBancosActivos()
        {
            var list = new List<(string Nombre, string Tipo, string Numero, string Beneficiario)>();
            if (_config.Banco1Activo && !string.IsNullOrWhiteSpace(_config.Banco1Numero))
                list.Add((_config.Banco1Nombre, _config.Banco1TipoCuenta, _config.Banco1Numero, _config.Banco1Beneficiario));
            if (_config.Banco2Activo && !string.IsNullOrWhiteSpace(_config.Banco2Numero))
                list.Add((_config.Banco2Nombre, _config.Banco2TipoCuenta, _config.Banco2Numero, _config.Banco2Beneficiario));
            if (_config.Banco3Activo && !string.IsNullOrWhiteSpace(_config.Banco3Numero))
                list.Add((_config.Banco3Nombre, _config.Banco3TipoCuenta, _config.Banco3Numero, _config.Banco3Beneficiario));
            if (_config.Banco4Activo && !string.IsNullOrWhiteSpace(_config.Banco4Numero))
                list.Add((_config.Banco4Nombre, _config.Banco4TipoCuenta, _config.Banco4Numero, _config.Banco4Beneficiario));
            return list;
        }

        private static IContainer BlockLabel(IContainer container) =>
            container.PaddingVertical(3);

        private static IContainer BlockValue(IContainer container) =>
            container.PaddingVertical(3);

        private void ComposeFooter(IContainer container)
        {
            container.Column(col =>
            {
                if (!string.IsNullOrWhiteSpace(_recibo.Cai))
                {
                    col.Item().AlignCenter().Text("ORIGINAL: CLIENTE   •   COPIA: OBLIGADO TRIBUTARIO EMISOR")
                        .Bold().FontSize(7f).FontColor("#475569");
                }
                col.Item().AlignCenter().Text(_config.MensajePieRecibo)
                    .FontSize(7f).FontColor("#94a3b8");
            });
        }
    }
}
