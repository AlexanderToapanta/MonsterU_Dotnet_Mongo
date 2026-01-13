using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CapaModelo;
using CapaDatos;

namespace Monster_University.Controllers
{
    public class ControladorOpRolesController : Controller
    {
        // GET: ControladorOpRoles/AsignarOpRol
        // GET: ControladorOpRoles/AsignarOpRol
        public ActionResult AsignarOpRol()
        {
            try
            {
                // Obtener roles desde la capa de datos
                var roles = CD_Rol.Instancia.ObtenerRoles() ?? new List<Rol>();

                // Crear SelectList para usar con @Html.DropDownList
                ViewBag.RolesSelectList = new SelectList(roles, "XEROL_ID", "XEROL_NOMBRE");

                // También pasar la lista por si acaso
                ViewBag.Roles = roles;

                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error: {ex.Message}";
                ViewBag.RolesSelectList = new SelectList(new List<Rol>(), "XEROL_ID", "XEROL_NOMBRE");
                ViewBag.Roles = new List<Rol>();
                return View();
            }
        }

        // POST: ControladorOpRoles/CargarOpcionesPorRol
        [HttpPost]
        public JsonResult CargarOpcionesPorRol(string XEROL_ID)
        {
            try
            {
                // Obtener todas las opciones con su estado de asignación para el rol seleccionado
                var opcionesConEstado = CD_OpcionRoles.Instancia.ObtenerOpcionesConEstadoPorRol(XEROL_ID);

                return Json(new
                {
                    success = true,
                    data = opcionesConEstado,
                    rolNombre = CD_Rol.Instancia.ObtenerDetalleRol(XEROL_ID)?.XEROL_NOMBRE ?? "Rol no encontrado"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al cargar opciones: " + ex.Message });
            }
        }

        // POST: ControladorOpRoles/AsignarOpcion
        [HttpPost]
        public JsonResult AsignarOpcion(string XEROL_ID, string XEOPC_ID)
        {
            try
            {
                bool resultado = CD_OpcionRoles.Instancia.AsignarOpcionARol(XEROL_ID, XEOPC_ID);

                if (resultado)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Opción asignada correctamente al rol"
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = "La opción ya está asignada a este rol o ocurrió un error"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al asignar opción: " + ex.Message });
            }
        }

        // POST: ControladorOpRoles/RetirarOpcion
        [HttpPost]
        public JsonResult RetirarOpcion(string XEROL_ID, string XEOPC_ID)
        {
            try
            {
                bool resultado = CD_OpcionRoles.Instancia.RetirarOpcionDeRol(XEROL_ID, XEOPC_ID);

                if (resultado)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Opción retirada correctamente del rol"
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = "No se pudo retirar la opción o no estaba asignada"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al retirar opción: " + ex.Message });
            }
        }

        // POST: ControladorOpRoles/AsignarMultiplesOpciones
        [HttpPost]
        public JsonResult AsignarMultiplesOpciones(string XEROL_ID, List<string> XEOPC_IDS)
        {
            try
            {
                if (XEOPC_IDS == null || !XEOPC_IDS.Any())
                {
                    return Json(new { success = false, message = "No se seleccionaron opciones" });
                }

                int asignadosExitosamente = 0;
                List<string> errores = new List<string>();

                foreach (var opcionId in XEOPC_IDS)
                {
                    try
                    {
                        if (CD_OpcionRoles.Instancia.AsignarOpcionARol(XEROL_ID, opcionId))
                        {
                            asignadosExitosamente++;
                        }
                        else
                        {
                            errores.Add($"Opción {opcionId} ya estaba asignada");
                        }
                    }
                    catch (Exception ex)
                    {
                        errores.Add($"Error con opción {opcionId}: {ex.Message}");
                    }
                }

                return Json(new
                {
                    success = asignadosExitosamente > 0,
                    message = $"Se asignaron {asignadosExitosamente} de {XEOPC_IDS.Count} opciones",
                    detalles = errores
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al asignar múltiples opciones: " + ex.Message });
            }
        }

        // GET: ControladorOpRoles/ObtenerTodasAsignaciones
        public JsonResult ObtenerTodasAsignaciones()
        {
            try
            {
                var asignaciones = CD_OpcionRoles.Instancia.ObtenerTodasAsignaciones();
                return Json(new { success = true, data = asignaciones }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al obtener asignaciones: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: ControladorOpRoles/ObtenerOpcionesDisponibles
        [HttpGet]
        public JsonResult ObtenerOpcionesDisponibles()
        {
            try
            {
                var opciones = CD_Opcion.Instancia.ObtenerOpciones();
                return Json(new { success = true, data = opciones }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al obtener opciones: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: ControladorOpRoles/ObtenerRoles
        [HttpGet]
        public JsonResult ObtenerRoles()
        {
            try
            {
                var roles = CD_Rol.Instancia.ObtenerRoles();
                return Json(new { success = true, data = roles }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al obtener roles: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: ControladorOpRoles/ReporteAsignaciones
        public ActionResult ReporteAsignaciones()
        {
            try
            {
                var asignaciones = CD_OpcionRoles.Instancia.ObtenerTodasAsignaciones();
                return View(asignaciones);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al generar reporte: " + ex.Message;
                return View(new List<OpcionRol>());
            }
        }

        // GET: ControladorOpRoles/DetalleRol/{id}
        public ActionResult DetalleRol(string id)
        {
            try
            {
                var rol = CD_Rol.Instancia.ObtenerDetalleRol(id);
                if (rol == null)
                {
                    return HttpNotFound();
                }

                ViewBag.OpcionesAsignadas = CD_Opcion.Instancia.ObtenerOpcionesPorRol(id);
                return View(rol);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al cargar detalles: " + ex.Message;
                return View();
            }
        }

        // Método de ejemplo para vista Index si quieres mantenerlo
        public ActionResult Index()
        {
            return RedirectToAction("AsignarOpRol");
        }
    }
}