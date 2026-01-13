using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CapaDatos;
using CapaModelo;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;


namespace Monster_University.Controllers
{
    public class ControladorCarreraController : Controller
    {
        public ActionResult Lista()
        {
            try
            {
                var lista = CD_Carrera.Instancia.ObtenerCarreras();
                ViewBag.Carreras = lista ?? new List<Carrera>();

                ViewBag.NewCarreraId = CD_Carrera.Instancia.GenerarNuevoId();
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                ViewBag.Carreras = new List<Carrera>();
                ViewBag.NewCarreraId = CD_Carrera.Instancia.GenerarNuevoId();
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Crear(FormCollection form)
        {
            try
            {
                var idFromForm = (form["MECARR_ID"] ?? string.Empty).ToString();
                var idToUse = string.IsNullOrWhiteSpace(idFromForm) ? CD_Carrera.Instancia.GenerarNuevoId() : idFromForm;

                var c = new Carrera
                {
                    MECARR_ID = idToUse,
                    MECARR_NOMBRE = (form["MECARR_NOMBRE"] ?? string.Empty).ToString(),
                    MECARR_MAXCRED = int.TryParse(form["MECARR_MAXCRED"], out var max) ? max :0,
                    MECARR_MINCRED = int.TryParse(form["MECARR_MINCRED"], out var min) ? min :0
                };

                bool ok = CD_Carrera.Instancia.RegistrarCarrera(c);
                TempData["SuccessMessage"] = ok ? "Carrera registrada correctamente." : "No se pudo registrar la carrera.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error: " + ex.Message;
            }
            return RedirectToAction("Lista");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Editar(FormCollection form)
        {
            try
            {
                var c = new Carrera
                {
                    MECARR_ID = (form["MECARR_ID"] ?? string.Empty).ToString(),
                    MECARR_NOMBRE = (form["MECARR_NOMBRE"] ?? string.Empty).ToString(),
                    MECARR_MAXCRED = int.TryParse(form["MECARR_MAXCRED"], out var max) ? max :0,
                    MECARR_MINCRED = int.TryParse(form["MECARR_MINCRED"], out var min) ? min :0
                };

                bool ok = CD_Carrera.Instancia.ModificarCarrera(c);
                TempData["SuccessMessage"] = ok ? "Carrera actualizada correctamente." : "No se pudo actualizar la carrera.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error: " + ex.Message;
            }
            return RedirectToAction("Lista");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EliminarMultiple(string ids)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ids))
                {
                    TempData["ErrorMessage"] = "No se enviaron IDs.";
                    return RedirectToAction("Lista");
                }

                var idList = ids.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();
                bool ok = CD_Carrera.Instancia.EliminarMultiple(idList);
                TempData["SuccessMessage"] = ok ? "Registros eliminados correctamente." : "No se pudieron eliminar los registros.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error: " + ex.Message;
            }
            return RedirectToAction("Lista");
        }

        // Acción para mostrar el reporte
        public ActionResult Reporte(string nombre, int? creditosMin, int? creditosMax)
        {
            try
            {
                List<Carrera> carreras;

                // Si no hay filtros, mostrar todas
                if (string.IsNullOrWhiteSpace(nombre) && !creditosMin.HasValue && !creditosMax.HasValue)
                {
                    carreras = CD_Carrera.Instancia.ObtenerCarreras();
                }
                else
                {
                    carreras = CD_Carrera.Instancia.BuscarCarreras(nombre, creditosMin, creditosMax);
                }

                ViewBag.Carreras = carreras ?? new List<Carrera>();
                ViewBag.FiltroNombre = nombre;
                ViewBag.FiltroMin = creditosMin;
                ViewBag.FiltroMax = creditosMax;

                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                ViewBag.Carreras = new List<Carrera>();
                return View();
            }
        }

        // Acción para generar PDF
        public ActionResult GenerarPdf(string nombre, int? creditosMin, int? creditosMax)
        {
            try
            {
                List<Carrera> carreras;

                // Aplicar los mismos filtros que la búsqueda
                if (string.IsNullOrWhiteSpace(nombre) && !creditosMin.HasValue && !creditosMax.HasValue)
                {
                    carreras = CD_Carrera.Instancia.ObtenerCarreras();
                }
                else
                {
                    carreras = CD_Carrera.Instancia.BuscarCarreras(nombre, creditosMin, creditosMax);
                }

                // Generar el PDF
                byte[] pdfBytes = GenerarPdfReporte(carreras, nombre, creditosMin, creditosMax);

                // Retornar el archivo PDF
                string nombreArchivo = $"Reporte_Carreras_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                return File(pdfBytes, "application/pdf", nombreArchivo);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al generar PDF: " + ex.Message;
                return RedirectToAction("Reporte");
            }
        }

        // Método privado para generar el PDF usando iTextSharp
        private byte[] GenerarPdfReporte(List<Carrera> carreras, string nombre, int? creditosMin, int? creditosMax)
        {
            using (var ms = new System.IO.MemoryStream())
            {
                // Crear documento PDF (tamaño carta)
                iTextSharp.text.Document document = new iTextSharp.text.Document(iTextSharp.text.PageSize.LETTER);
                iTextSharp.text.pdf.PdfWriter.GetInstance(document, ms);

                document.Open();

                // Fuentes
                var fontTitulo = iTextSharp.text.FontFactory.GetFont("Arial", 18, iTextSharp.text.Font.BOLD);
                var fontSubtitulo = iTextSharp.text.FontFactory.GetFont("Arial", 12, iTextSharp.text.Font.NORMAL);
                var fontEncabezado = iTextSharp.text.FontFactory.GetFont("Arial", 10, iTextSharp.text.Font.BOLD);
                var fontNormal = iTextSharp.text.FontFactory.GetFont("Arial", 9, iTextSharp.text.Font.NORMAL);

                // Título
                var titulo = new iTextSharp.text.Paragraph("REPORTE DE CARRERAS\n\n", fontTitulo);
                titulo.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
                document.Add(titulo);

                // Información de filtros
                var filtros = new iTextSharp.text.Paragraph();
                filtros.Font = fontSubtitulo;
                filtros.Add($"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}\n");

                if (!string.IsNullOrWhiteSpace(nombre) || creditosMin.HasValue || creditosMax.HasValue)
                {
                    filtros.Add("Filtros aplicados:\n");
                    if (!string.IsNullOrWhiteSpace(nombre))
                        filtros.Add($"  - Nombre: {nombre}\n");
                    if (creditosMin.HasValue)
                        filtros.Add($"  - Créditos mínimos: {creditosMin.Value}\n");
                    if (creditosMax.HasValue)
                        filtros.Add($"  - Créditos máximos: {creditosMax.Value}\n");
                }
                else
                {
                    filtros.Add("Sin filtros (mostrando todas las carreras)\n");
                }

                filtros.Add($"\nTotal de registros: {carreras.Count}\n\n");
                document.Add(filtros);

                // Tabla
                var tabla = new iTextSharp.text.pdf.PdfPTable(4);
                tabla.WidthPercentage = 100;
                tabla.SetWidths(new float[] { 15f, 40f, 22.5f, 22.5f });

                // Encabezados
                var celdaEncabezado1 = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase("ID", fontEncabezado));
                celdaEncabezado1.BackgroundColor = new iTextSharp.text.BaseColor(200, 200, 200);
                celdaEncabezado1.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                celdaEncabezado1.Padding = 5;
                tabla.AddCell(celdaEncabezado1);

                var celdaEncabezado2 = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase("Nombre", fontEncabezado));
                celdaEncabezado2.BackgroundColor = new iTextSharp.text.BaseColor(200, 200, 200);
                celdaEncabezado2.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                celdaEncabezado2.Padding = 5;
                tabla.AddCell(celdaEncabezado2);

                var celdaEncabezado3 = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase("Créditos Mínimos", fontEncabezado));
                celdaEncabezado3.BackgroundColor = new iTextSharp.text.BaseColor(200, 200, 200);
                celdaEncabezado3.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                celdaEncabezado3.Padding = 5;
                tabla.AddCell(celdaEncabezado3);

                var celdaEncabezado4 = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase("Créditos Máximos", fontEncabezado));
                celdaEncabezado4.BackgroundColor = new iTextSharp.text.BaseColor(200, 200, 200);
                celdaEncabezado4.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                celdaEncabezado4.Padding = 5;
                tabla.AddCell(celdaEncabezado4);

                // Datos
                foreach (var carrera in carreras)
                {
                    var celdaId = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase(carrera.MECARR_ID ?? "", fontNormal));
                    celdaId.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                    celdaId.Padding = 5;
                    tabla.AddCell(celdaId);

                    var celdaNombre = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase(carrera.MECARR_NOMBRE ?? "", fontNormal));
                    celdaNombre.Padding = 5;
                    tabla.AddCell(celdaNombre);

                    var celdaMin = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase(carrera.MECARR_MINCRED.ToString(), fontNormal));
                    celdaMin.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                    celdaMin.Padding = 5;
                    tabla.AddCell(celdaMin);

                    var celdaMax = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase(carrera.MECARR_MAXCRED.ToString(), fontNormal));
                    celdaMax.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                    celdaMax.Padding = 5;
                    tabla.AddCell(celdaMax);
                }

                document.Add(tabla);

                // Pie de página
                var pie = new iTextSharp.text.Paragraph("\n\nGenerado por Monsters University", fontSubtitulo);
                pie.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
                document.Add(pie);

                document.Close();

                return ms.ToArray();
            }
        }
    }
}
