using AgenciaDeTransporteWeb.Models.Entities;
using AgenciaDeTransporteWeb.Services.Interfaces;
using AgenciaDeTransporteWeb.Data;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AgenciaDeTransporteWeb.Services.Implementations
{
    public class PDFService : IPDFService
    {
        private readonly ApplicationDbContext _context;

        public PDFService(ApplicationDbContext context)
        {
            _context = context;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<byte[]> GenerarTicketReservaAsync(Reserva reserva)
        {
            var reservaCompleta = await _context.Reservas
                .Include(r => r.Usuario)
                .Include(r => r.Viaje)
                    .ThenInclude(v => v.Ruta)
                .Include(r => r.Viaje)
                    .ThenInclude(v => v.Autobus)
                .Include(r => r.Asiento)
                .FirstOrDefaultAsync(r => r.Id == reserva.Id);

            if (reservaCompleta == null)
                throw new ArgumentException("Reserva no encontrada");

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header()
                        .Height(100)
                        .Background(Colors.Blue.Medium)
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span("TICKET DE VIAJE")
                                .FontSize(20)
                                .FontColor(Colors.White)
                                .Bold();
                        });

                    page.Content()
                        .Column(col =>
                        {
                            col.Spacing(20);

                            // Información de la reserva
                            col.Item().Row(row =>
                            {
                                row.RelativeItem().Column(column =>
                                {
                                    column.Item().Text(text =>
                                    {
                                        text.Span("CÓDIGO DE RESERVA").Bold().FontSize(14);
                                    });
                                    column.Item().Text(text =>
                                    {
                                        text.Span(reservaCompleta.CodigoReserva).FontSize(16);
                                    });
                                });

                                row.RelativeItem().Column(column =>
                                {
                                    column.Item().Text(text =>
                                    {
                                        text.Span("FECHA DE EMISIÓN").Bold().FontSize(14);
                                    });
                                    column.Item().Text(text =>
                                    {
                                        text.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
                                    });
                                });
                            });

                            // Información del pasajero
                            col.Item().Background(Colors.Grey.Lighten3).Padding(10).Column(column =>
                            {
                                column.Item().Text(text =>
                                {
                                    text.Span("INFORMACIÓN DEL PASAJERO").Bold().FontSize(14);
                                });
                                column.Item().Text(text =>
                                {
                                    text.Span($"Nombre: {reservaCompleta.Usuario?.NombreCompleto ?? "N/A"}");
                                });
                                column.Item().Text(text =>
                                {
                                    text.Span($"Email: {reservaCompleta.Usuario?.Email ?? "N/A"}");
                                });
                                if (!string.IsNullOrEmpty(reservaCompleta.Usuario?.Documento))
                                {
                                    column.Item().Text(text =>
                                    {
                                        text.Span($"Documento: {reservaCompleta.Usuario.Documento}");
                                    });
                                }
                            });

                            // Información del viaje
                            col.Item().Background(Colors.Blue.Lighten4).Padding(10).Column(column =>
                            {
                                column.Item().Text(text =>
                                {
                                    text.Span("INFORMACIÓN DEL VIAJE").Bold().FontSize(14);
                                });
                                column.Item().Text(text =>
                                {
                                    text.Span($"Ruta: {reservaCompleta.Viaje?.Ruta?.CiudadOrigen} → {reservaCompleta.Viaje?.Ruta?.CiudadDestino}");
                                });
                                column.Item().Text(text =>
                                {
                                    text.Span($"Fecha de Salida: {reservaCompleta.Viaje?.FechaSalida:dd/MM/yyyy}");
                                });
                                column.Item().Text(text =>
                                {
                                    text.Span($"Hora de Salida: {reservaCompleta.Viaje?.FechaSalida:HH:mm}");
                                });
                                column.Item().Text(text =>
                                {
                                    text.Span($"Hora de Llegada: {reservaCompleta.Viaje?.FechaLlegada:HH:mm}");
                                });
                                column.Item().Text(text =>
                                {
                                    text.Span($"Autobús: {reservaCompleta.Viaje?.Autobus?.Placa} - {reservaCompleta.Viaje?.Autobus?.Modelo}");
                                });
                            });

                            // Información del asiento
                            col.Item().Background(Colors.Green.Lighten4).Padding(10).Column(column =>
                            {
                                column.Item().Text(text =>
                                {
                                    text.Span("INFORMACIÓN DEL ASIENTO").Bold().FontSize(14);
                                });
                                column.Item().Text(text =>
                                {
                                    text.Span($"Asiento Nº: {reservaCompleta.Asiento?.NumeroAsiento}");
                                });
                                column.Item().Text(text =>
                                {
                                    text.Span($"Tipo: {reservaCompleta.Asiento?.TipoAsiento}");
                                });
                            });

                            // Información del pago
                            col.Item().Background(Colors.Yellow.Lighten4).Padding(10).Column(column =>
                            {
                                column.Item().Text(text =>
                                {
                                    text.Span("INFORMACIÓN DEL PAGO").Bold().FontSize(14);
                                });
                                column.Item().Text(text =>
                                {
                                    text.Span($"Monto Total: S/. {reservaCompleta.MontoTotal:F2}");
                                });
                                column.Item().Text(text =>
                                {
                                    text.Span($"Estado: {reservaCompleta.EstadoReserva}");
                                });
                                column.Item().Text(text =>
                                {
                                    text.Span($"Fecha de Reserva: {reservaCompleta.FechaReserva:dd/MM/yyyy HH:mm}");
                                });
                            });

                            // Términos y condiciones
                            col.Item().PaddingTop(20).Column(column =>
                            {
                                column.Item().Text(text =>
                                {
                                    text.Span("TÉRMINOS Y CONDICIONES").Bold().FontSize(12);
                                });
                                column.Item().Text(text =>
                                {
                                    text.Span("• Presentar documento de identidad al abordar");
                                });
                                column.Item().Text(text =>
                                {
                                    text.Span("• Llegar 15 minutos antes de la hora de salida");
                                });
                                column.Item().Text(text =>
                                {
                                    text.Span("• Este ticket es personal e intransferible");
                                });
                                column.Item().Text(text =>
                                {
                                    text.Span("• Cancelaciones hasta 2 horas antes de la salida");
                                });
                            });
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span("TransporteMax - Tu viaje perfecto | www.transportemax.com")
                                .FontSize(10);
                        });
                });
            }).GeneratePdf();
        }

        public async Task<byte[]> GenerarReporteVentasAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            var transacciones = await _context.Transacciones
                .Include(t => t.Reserva)
                    .ThenInclude(r => r.Viaje)
                        .ThenInclude(v => v.Ruta)
                .Where(t => t.FechaTransaccion >= fechaInicio &&
                           t.FechaTransaccion <= fechaFin &&
                           t.EstadoTransaccion == "Completada")
                .ToListAsync();

            var totalVentas = transacciones.Sum(t => t.Monto);
            var totalTransacciones = transacciones.Count;

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);

                    page.Header()
                        .Height(80)
                        .Background(Colors.Blue.Medium)
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span("REPORTE DE VENTAS")
                                .FontSize(18)
                                .FontColor(Colors.White)
                                .Bold();
                        });

                    page.Content()
                        .Column(col =>
                        {
                            col.Spacing(15);

                            // Resumen
                            col.Item().Row(row =>
                            {
                                row.RelativeItem().Column(column =>
                                {
                                    column.Item().Text(text =>
                                    {
                                        text.Span($"Período: {fechaInicio:dd/MM/yyyy} - {fechaFin:dd/MM/yyyy}").Bold();
                                    });
                                    column.Item().Text(text =>
                                    {
                                        text.Span($"Total Transacciones: {totalTransacciones}");
                                    });
                                    column.Item().Text(text =>
                                    {
                                        text.Span($"Total Ventas: S/. {totalVentas:F2}").FontSize(16).Bold();
                                    });
                                });
                            });

                            // Tabla de transacciones
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(80);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                    columns.ConstantColumn(80);
                                    columns.ConstantColumn(80);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Grey.Medium).Padding(5).Text(text => text.Span("Fecha").Bold());
                                    header.Cell().Background(Colors.Grey.Medium).Padding(5).Text(text => text.Span("Ruta").Bold());
                                    header.Cell().Background(Colors.Grey.Medium).Padding(5).Text(text => text.Span("Método").Bold());
                                    header.Cell().Background(Colors.Grey.Medium).Padding(5).Text(text => text.Span("Estado").Bold());
                                    header.Cell().Background(Colors.Grey.Medium).Padding(5).Text(text => text.Span("Monto").Bold());
                                });

                                foreach (var transaccion in transacciones)
                                {
                                    table.Cell().BorderBottom(1).Padding(5).Text(text => text.Span(transaccion.FechaTransaccion.ToString("dd/MM")));
                                    table.Cell().BorderBottom(1).Padding(5).Text(text => text.Span($"{transaccion.Reserva.Viaje.Ruta.CiudadOrigen} - {transaccion.Reserva.Viaje.Ruta.CiudadDestino}"));
                                    table.Cell().BorderBottom(1).Padding(5).Text(text => text.Span(transaccion.MetodoPago));
                                    table.Cell().BorderBottom(1).Padding(5).Text(text => text.Span(transaccion.EstadoTransaccion));
                                    table.Cell().BorderBottom(1).Padding(5).Text(text => text.Span($"S/. {transaccion.Monto:F2}"));
                                }
                            });
                        });
                });
            }).GeneratePdf();
        }

        public async Task<byte[]> GenerarReporteRutasAsync()
        {
            var rutas = await _context.Rutas
                .Where(r => r.Activo)
                .ToListAsync();

            var estadisticasRutas = new List<dynamic>();

            foreach (var ruta in rutas)
            {
                var reservas = await _context.Reservas
                    .Include(r => r.Viaje)
                    .Where(r => r.Viaje.RutaId == ruta.Id && r.EstadoReserva != "Cancelada")
                    .CountAsync();

                var ingresos = await _context.Transacciones
                    .Include(t => t.Reserva)
                        .ThenInclude(r => r.Viaje)
                    .Where(t => t.Reserva.Viaje.RutaId == ruta.Id && t.EstadoTransaccion == "Completada")
                    .SumAsync(t => t.Monto);

                estadisticasRutas.Add(new
                {
                    Ruta = ruta,
                    TotalReservas = reservas,
                    TotalIngresos = ingresos
                });
            }

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);

                    page.Header()
                        .Height(80)
                        .Background(Colors.Green.Medium)
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span("REPORTE DE RUTAS")
                                .FontSize(18)
                                .FontColor(Colors.White)
                                .Bold();
                        });

                    page.Content()
                        .Column(col =>
                        {
                            col.Spacing(15);

                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                    columns.ConstantColumn(80);
                                    columns.ConstantColumn(80);
                                    columns.ConstantColumn(100);
                                    columns.ConstantColumn(100);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Grey.Medium).Padding(5).Text(text => text.Span("Origen").Bold());
                                    header.Cell().Background(Colors.Grey.Medium).Padding(5).Text(text => text.Span("Destino").Bold());
                                    header.Cell().Background(Colors.Grey.Medium).Padding(5).Text(text => text.Span("Precio").Bold());
                                    header.Cell().Background(Colors.Grey.Medium).Padding(5).Text(text => text.Span("Duración").Bold());
                                    header.Cell().Background(Colors.Grey.Medium).Padding(5).Text(text => text.Span("Reservas").Bold());
                                    header.Cell().Background(Colors.Grey.Medium).Padding(5).Text(text => text.Span("Ingresos").Bold());
                                });

                                foreach (var estadistica in estadisticasRutas)
                                {
                                    table.Cell().BorderBottom(1).Padding(5).Text(text => text.Span(estadistica.Ruta.CiudadOrigen));
                                    table.Cell().BorderBottom(1).Padding(5).Text(text => text.Span(estadistica.Ruta.CiudadDestino));
                                    table.Cell().BorderBottom(1).Padding(5).Text(text => text.Span($"S/. {estadistica.Ruta.Precio:F2}"));
                                    table.Cell().BorderBottom(1).Padding(5).Text(text => text.Span($"{estadistica.Ruta.DuracionHoras}h"));
                                    table.Cell().BorderBottom(1).Padding(5).Text(text => text.Span(estadistica.TotalReservas.ToString()));
                                    table.Cell().BorderBottom(1).Padding(5).Text(text => text.Span($"S/. {estadistica.TotalIngresos:F2}"));
                                }
                            });
                        });
                });
            }).GeneratePdf();
        }

        public async Task<byte[]> GenerarManifiestoViajeAsync(int viajeId)
        {
            var viaje = await _context.Viajes
                .Include(v => v.Ruta)
                .Include(v => v.Autobus)
                .FirstOrDefaultAsync(v => v.Id == viajeId);

            var reservas = await _context.Reservas
                .Include(r => r.Usuario)
                .Include(r => r.Asiento)
                .Where(r => r.ViajeId == viajeId && r.EstadoReserva != "Cancelada")
                .OrderBy(r => r.Asiento.NumeroAsiento)
                .ToListAsync();

            if (viaje == null)
                throw new ArgumentException("Viaje no encontrado");

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);

                    page.Header()
                        .Height(100)
                        .Background(Colors.Purple.Medium)
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span("MANIFIESTO DE VIAJE")
                                .FontSize(18)
                                .FontColor(Colors.White)
                                .Bold();
                        });

                    page.Content()
                        .Column(col =>
                        {
                            col.Spacing(15);

                            // Información del viaje
                            col.Item().Background(Colors.Grey.Lighten3).Padding(10).Column(column =>
                            {
                                column.Item().Text(text => text.Span($"Ruta: {viaje.Ruta.CiudadOrigen} → {viaje.Ruta.CiudadDestino}").Bold());
                                column.Item().Text(text => text.Span($"Fecha: {viaje.FechaSalida:dd/MM/yyyy}"));
                                column.Item().Text(text => text.Span($"Hora Salida: {viaje.FechaSalida:HH:mm} - Hora Llegada: {viaje.FechaLlegada:HH:mm}"));
                                column.Item().Text(text => text.Span($"Autobús: {viaje.Autobus.Placa} - {viaje.Autobus.Modelo}"));
                                column.Item().Text(text => text.Span($"Total Pasajeros: {reservas.Count}"));
                            });

                            // Lista de pasajeros
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(60);
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                    columns.ConstantColumn(100);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Grey.Medium).Padding(5).Text(text => text.Span("Asiento").Bold());
                                    header.Cell().Background(Colors.Grey.Medium).Padding(5).Text(text => text.Span("Nombre").Bold());
                                    header.Cell().Background(Colors.Grey.Medium).Padding(5).Text(text => text.Span("Documento").Bold());
                                    header.Cell().Background(Colors.Grey.Medium).Padding(5).Text(text => text.Span("Código").Bold());
                                    header.Cell().Background(Colors.Grey.Medium).Padding(5).Text(text => text.Span("Estado").Bold());
                                });

                                foreach (var reserva in reservas)
                                {
                                    table.Cell().BorderBottom(1).Padding(5).Text(text => text.Span(reserva.Asiento.NumeroAsiento.ToString()));
                                    table.Cell().BorderBottom(1).Padding(5).Text(text => text.Span(reserva.Usuario.NombreCompleto));
                                    table.Cell().BorderBottom(1).Padding(5).Text(text => text.Span(reserva.Usuario.Documento ?? "N/A"));
                                    table.Cell().BorderBottom(1).Padding(5).Text(text => text.Span(reserva.CodigoReserva));
                                    table.Cell().BorderBottom(1).Padding(5).Text(text => text.Span(reserva.EstadoReserva));
                                }
                            });
                        });
                });
            }).GeneratePdf();
        }

        public async Task<byte[]> GenerarFacturaAsync(Transaccion transaccion)
        {
            var transaccionCompleta = await _context.Transacciones
                .Include(t => t.Reserva)
                    .ThenInclude(r => r.Usuario)
                .Include(t => t.Reserva)
                    .ThenInclude(r => r.Viaje)
                        .ThenInclude(v => v.Ruta)
                .FirstOrDefaultAsync(t => t.Id == transaccion.Id);

            if (transaccionCompleta == null)
                throw new ArgumentException("Transacción no encontrada");

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);

                    page.Header()
                        .Height(80)
                        .Background(Colors.Red.Medium)
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span("FACTURA")
                                .FontSize(18)
                                .FontColor(Colors.White)
                                .Bold();
                        });

                    page.Content()
                        .Column(col =>
                        {
                            col.Spacing(15);

                            // Información de la empresa
                            col.Item().Row(row =>
                            {
                                row.RelativeItem().Column(column =>
                                {
                                    column.Item().Text(text => text.Span("TransporteMax S.A.C.").Bold().FontSize(16));
                                    column.Item().Text(text => text.Span("RUC: 20123456789"));
                                    column.Item().Text(text => text.Span("Av. Principal 123, Lima"));
                                    column.Item().Text(text => text.Span("Teléfono: +51 999 888 777"));
                                });

                                row.RelativeItem().Column(column =>
                                {
                                    column.Item().AlignRight().Text(text => text.Span($"Factura Nº: F001-{transaccionCompleta.Id:0000}").Bold());
                                    column.Item().AlignRight().Text(text => text.Span($"Fecha: {transaccionCompleta.FechaTransaccion:dd/MM/yyyy}"));
                                    column.Item().AlignRight().Text(text => text.Span($"Referencia: {transaccionCompleta.ReferenciaPago}"));
                                });
                            });

                            // Información del cliente
                            col.Item().Background(Colors.Grey.Lighten3).Padding(10).Column(column =>
                            {
                                column.Item().Text(text => text.Span("DATOS DEL CLIENTE").Bold());
                                column.Item().Text(text => text.Span($"Nombre: {transaccionCompleta.Reserva?.Usuario?.NombreCompleto ?? "N/A"}"));
                                column.Item().Text(text => text.Span($"Email: {transaccionCompleta.Reserva?.Usuario?.Email ?? "N/A"}"));
                                if (!string.IsNullOrEmpty(transaccionCompleta.Reserva?.Usuario?.Documento))
                                {
                                    column.Item().Text(text => text.Span($"Documento: {transaccionCompleta.Reserva.Usuario.Documento}"));
                                }
                            });

                            // Detalle del servicio
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(4);
                                    columns.ConstantColumn(80);
                                    columns.ConstantColumn(80);
                                    columns.ConstantColumn(100);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Grey.Medium).Padding(5).Text(text => text.Span("Descripción").Bold());
                                    header.Cell().Background(Colors.Grey.Medium).Padding(5).Text(text => text.Span("Cantidad").Bold());
                                    header.Cell().Background(Colors.Grey.Medium).Padding(5).Text(text => text.Span("P. Unit.").Bold());
                                    header.Cell().Background(Colors.Grey.Medium).Padding(5).Text(text => text.Span("Total").Bold());
                                });

                                var monto = transaccionCompleta.Monto;
                                table.Cell().BorderBottom(1).Padding(5).Text(text => text.Span($"Pasaje {transaccionCompleta.Reserva?.Viaje?.Ruta?.CiudadOrigen} - {transaccionCompleta.Reserva?.Viaje?.Ruta?.CiudadDestino}"));
                                table.Cell().BorderBottom(1).Padding(5).Text(text => text.Span("1"));
                                table.Cell().BorderBottom(1).Padding(5).Text(text => text.Span($"S/. {monto:F2}"));
                                table.Cell().BorderBottom(1).Padding(5).Text(text => text.Span($"S/. {monto:F2}"));
                            });

                            // Total
                            col.Item().AlignRight().Column(column =>
                            {
                                var monto = transaccionCompleta.Monto;
                                column.Item().Text(text => text.Span($"Subtotal: S/. {monto:F2}").FontSize(14));
                                column.Item().Text(text => text.Span($"IGV (18%): S/. {monto * 0.18m:F2}").FontSize(14));
                                column.Item().Text(text => text.Span($"TOTAL: S/. {monto * 1.18m:F2}").Bold().FontSize(16));
                            });
                        });
                });
            }).GeneratePdf();
        }
    }
}