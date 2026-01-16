using CapaDatos;
using CapaModelo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Monster_University.Controllers
{
    public class ControladorRolController : Controller
    {
        // GET: ControladorRol/gestionarrol (Lista de roles)
        public ActionResult GestionarRol()
        {
            try
            {
                // Obtener roles desde la capa de datos
                var roles = CD_Rol.Instancia.ObtenerRoles();

                // Obtener opciones del sistema para mostrar en la vista
                var opcionesSistema = CD_Configuracion.Instancia.ObtenerValoresConfiguracion("opciones_sistema");

                // Pasar los roles y opciones al ViewBag
                ViewBag.Roles = roles;
                ViewBag.OpcionesSistema = opcionesSistema;

                // También generar ID para creación
                ViewBag.NewRolId = GenerarIdRolAutomatico();

                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error: {ex.Message}";
                ViewBag.Roles = new List<Rol>();
                ViewBag.OpcionesSistema = new List<Configuracion.ValorConfiguracion>();
                return View();
            }
        }

        // GET: ControladorRol/Crear (Crear nuevo rol)
        public ActionResult Crear()
        {
            // Preparar nuevo rol con ID generado automáticamente
            var nuevoRol = new Rol();
            var nuevoId = GenerarIdRolAutomatico();
            ViewBag.IdGenerado = nuevoId;
            nuevoRol.Codigo = nuevoId;
            nuevoRol.Estado = "ACTIVO"; // Estado por defecto
            nuevoRol.OpcionesPermitidas = new List<string>(); // Array vacío por defecto

            // Obtener opciones del sistema
            var opcionesSistema = CD_Configuracion.Instancia.ObtenerValoresConfiguracion("opciones_sistema");
            ViewBag.OpcionesSistema = opcionesSistema;

            return View(nuevoRol);
        }

        // POST: ControladorRol/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Crear(FormCollection form)
        {
            try
            {
                // Generar ID automáticamente
                var nuevoId = GenerarIdRolAutomatico();

                // Las opciones permitidas inicialmente van vacías (array vacío)
                var nuevoRol = new Rol
                {
                    Codigo = nuevoId, // ID generado automáticamente
                    Nombre = form["Nombre"],
                    Descripcion = form["Descripcion"],
                    OpcionesPermitidas = new List<string>(), // Array vacío inicialmente
                    Estado = "ACTIVO" // Estado por defecto
                };

                // Validaciones
                if (string.IsNullOrEmpty(nuevoRol.Nombre))
                {
                    ViewBag.Error = "El nombre del rol es requerido";
                    ViewBag.IdGenerado = nuevoId;
                    nuevoRol.Codigo = nuevoId;
                    ViewBag.OpcionesSistema = CD_Configuracion.Instancia.ObtenerValoresConfiguracion("opciones_sistema");
                    return View(nuevoRol);
                }

                // Validar que el nombre sea único
                if (!CD_Rol.Instancia.ValidarNombreRolUnico(nuevoRol.Nombre))
                {
                    ViewBag.Error = $"El nombre '{nuevoRol.Nombre}' ya existe";
                    ViewBag.IdGenerado = nuevoId;
                    nuevoRol.Codigo = nuevoId;
                    ViewBag.OpcionesSistema = CD_Configuracion.Instancia.ObtenerValoresConfiguracion("opciones_sistema");
                    return View(nuevoRol);
                }

                // Registrar el rol
                var resultado = CD_Rol.Instancia.RegistrarRol(nuevoRol);

                if (resultado)
                {
                    TempData["SuccessMessage"] = $"Rol '{nuevoRol.Nombre}' creado correctamente con código: {nuevoId}";
                    return RedirectToAction("GestionarRol");
                }
                else
                {
                    ViewBag.Error = "Error al crear el rol";
                    ViewBag.IdGenerado = nuevoId;
                    nuevoRol.Codigo = nuevoId;
                    ViewBag.OpcionesSistema = CD_Configuracion.Instancia.ObtenerValoresConfiguracion("opciones_sistema");
                    return View(nuevoRol);
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error: {ex.Message}";
                var nuevoId = GenerarIdRolAutomatico();
                ViewBag.IdGenerado = nuevoId;
                ViewBag.OpcionesSistema = CD_Configuracion.Instancia.ObtenerValoresConfiguracion("opciones_sistema");

                var rolError = new Rol
                {
                    Codigo = nuevoId,
                    Estado = "ACTIVO"
                };
                return View(rolError);
            }
        }

        // GET: ControladorRol/Editar/{codigo}
        public ActionResult Editar(string codigo)
        {
            try
            {
                var rol = CD_Rol.Instancia.ObtenerRolPorCodigo(codigo);
                if (rol != null)
                {
                    // Asegurar que OpcionesPermitidas no sea null
                    if (rol.OpcionesPermitidas == null)
                    {
                        rol.OpcionesPermitidas = new List<string>();
                    }

                    // Obtener opciones del sistema
                    var opcionesSistema = CD_Configuracion.Instancia.ObtenerValoresConfiguracion("opciones_sistema");
                    ViewBag.OpcionesSistema = opcionesSistema;

                    return View(rol);
                }
                else
                {
                    TempData["ErrorMessage"] = "Rol no encontrado";
                    return RedirectToAction("GestionarRol");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                return RedirectToAction("GestionarRol");
            }
        }

        // POST: ControladorRol/Editar/{codigo}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Editar(FormCollection form)
        {
            try
            {
                var codigoRol = form["Codigo"];
                var rolExistente = CD_Rol.Instancia.ObtenerRolPorCodigo(codigoRol);

                if (rolExistente == null)
                {
                    TempData["ErrorMessage"] = "Rol no encontrado";
                    return RedirectToAction("GestionarRol");
                }

                // Obtener opciones seleccionadas del formulario
                var opcionesSeleccionadas = form["OpcionesPermitidas"]?.Split(',') ?? new string[0];

                // Actualizar solo los campos editables
                rolExistente.Nombre = form["Nombre"];
                rolExistente.Descripcion = form["Descripcion"];
                rolExistente.OpcionesPermitidas = opcionesSeleccionadas
                    .Where(o => !string.IsNullOrEmpty(o))
                    .ToList();
                rolExistente.Estado = form["Estado"] ?? "ACTIVO";

                // Validar que el nombre sea único (excluyendo el rol actual)
                if (!CD_Rol.Instancia.ValidarNombreRolUnico(rolExistente.Nombre, rolExistente.Codigo))
                {
                    ViewBag.Error = $"El nombre '{rolExistente.Nombre}' ya existe";
                    var opcionesSistema = CD_Configuracion.Instancia.ObtenerValoresConfiguracion("opciones_sistema");
                    ViewBag.OpcionesSistema = opcionesSistema;
                    return View(rolExistente);
                }

                // Actualizar el rol
                var resultado = CD_Rol.Instancia.ModificarRol(rolExistente);

                if (resultado)
                {
                    TempData["SuccessMessage"] = "Rol actualizado correctamente";
                }
                else
                {
                    TempData["ErrorMessage"] = "Error al actualizar el rol";
                }

                return RedirectToAction("GestionarRol");
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error: {ex.Message}";
                var codigoRol = form["Codigo"];
                var rol = CD_Rol.Instancia.ObtenerRolPorCodigo(codigoRol);

                if (rol != null)
                {
                    var opcionesSistema = CD_Configuracion.Instancia.ObtenerValoresConfiguracion("opciones_sistema");
                    ViewBag.OpcionesSistema = opcionesSistema;
                }

                return View(rol ?? new Rol());
            }
        }

        // GET: ControladorRol/Eliminar/{codigo}
        public ActionResult Eliminar(string codigo)
        {
            try
            {
                var rol = CD_Rol.Instancia.ObtenerRolPorCodigo(codigo);
                if (rol != null)
                {
                    return View(rol);
                }
                else
                {
                    TempData["ErrorMessage"] = "Rol no encontrado";
                    return RedirectToAction("GestionarRol");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                return RedirectToAction("GestionarRol");
            }
        }

        // POST: ControladorRol/Eliminar/{codigo}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Eliminar")]
        public ActionResult EliminarConfirmado(string codigo)
        {
            try
            {
                var resultado = CD_Rol.Instancia.EliminarRol(codigo);

                if (resultado)
                {
                    TempData["SuccessMessage"] = "Rol eliminado correctamente";
                }
                else
                {
                    TempData["ErrorMessage"] = "No se pudo eliminar el rol. Verifique que no tenga usuarios asignados.";
                }

                return RedirectToAction("GestionarRol");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                return RedirectToAction("GestionarRol");
            }
        }

        // GET: ControladorRol/Detalles/{codigo}
        public ActionResult Detalles(string codigo)
        {
            try
            {
                var rol = CD_Rol.Instancia.ObtenerRolPorCodigo(codigo);
                if (rol != null)
                {
                    // Obtener opciones del sistema para mostrar nombres de las opciones
                    var opcionesSistema = CD_Configuracion.Instancia.ObtenerValoresConfiguracion("opciones_sistema");

                    // Mapear códigos de opciones a nombres
                    var opcionesConNombres = new List<dynamic>();
                    if (rol.OpcionesPermitidas != null && opcionesSistema != null)
                    {
                        foreach (var codigoOpcion in rol.OpcionesPermitidas)
                        {
                            var opcionSistema = opcionesSistema.FirstOrDefault(o => o.Codigo == codigoOpcion);
                            opcionesConNombres.Add(new
                            {
                                Codigo = codigoOpcion,
                                Nombre = opcionSistema?.Nombre ?? codigoOpcion,
                                
                            });
                        }
                    }

                    ViewBag.OpcionesConNombres = opcionesConNombres;
                    ViewBag.OpcionesSistema = opcionesSistema;

                    return View(rol);
                }
                else
                {
                    TempData["ErrorMessage"] = "Rol no encontrado";
                    return RedirectToAction("GestionarRol");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                return RedirectToAction("GestionarRol");
            }
        }

        // POST: ControladorRol/BuscarRol
        [HttpPost]
        public JsonResult BuscarRol(string criterio, string valor)
        {
            try
            {
                var rolesEncontrados = CD_Rol.Instancia.BuscarRol(criterio, valor);
                var datos = rolesEncontrados?.Select(r => new
                {
                    codigo = r.Codigo,
                    nombre = r.Nombre,
                    descripcion = r.Descripcion,
                    estado = r.Estado,
                    opcionesCount = r.OpcionesPermitidas?.Count ?? 0
                }).ToList();

                return Json(new { success = true, roles = datos });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: ControladorRol/ObtenerOpcionesSistema
        [HttpGet]
        public JsonResult ObtenerOpcionesSistema()
        {
            try
            {
                var opciones = CD_Configuracion.Instancia.ObtenerValoresConfiguracion("opciones_sistema");
                return Json(new { success = true, opciones = opciones }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: ControladorRol/ObtenerOpcionesRol/{codigoRol}
        [HttpGet]
        public JsonResult ObtenerOpcionesRol(string codigoRol)
        {
            try
            {
                var opciones = CD_Rol.Instancia.ObtenerOpcionesPorRol(codigoRol);
                return Json(new { success = true, opciones = opciones }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: ControladorRol/RegenerarId (Regenerar ID automático)
        [HttpPost]
        public JsonResult RegenerarId()
        {
            try
            {
                var nuevoId = GenerarIdRolAutomatico();
                return Json(new { success = true, nuevoId = nuevoId });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Método para generar ID de rol automático (ROL001, ROL002, etc.)
        private string GenerarIdRolAutomatico()
        {
            try
            {
                var roles = CD_Rol.Instancia.ObtenerRoles();
                if (roles == null || roles.Count == 0)
                {
                    return "ROL001";
                }

                // Buscar máximo número en IDs ROLXXX
                int maxNumero = 0;
                foreach (var rol in roles)
                {
                    if (rol.Codigo != null &&
                        rol.Codigo.StartsWith("ROL") &&
                        rol.Codigo.Length == 6)
                    {
                        try
                        {
                            string numeroStr = rol.Codigo.Substring(3);
                            if (int.TryParse(numeroStr, out int numero))
                            {
                                if (numero > maxNumero)
                                {
                                    maxNumero = numero;
                                }
                            }
                        }
                        catch { }
                    }
                }

                // Buscar huecos disponibles
                for (int i = 1; i <= 999; i++)
                {
                    string idCandidato = $"ROL{i:000}";
                    bool existe = roles.Any(r => r.Codigo == idCandidato);
                    if (!existe)
                    {
                        return idCandidato;
                    }
                }

                // Si no hay huecos, usar siguiente número
                return $"ROL{maxNumero + 1:000}";
            }
            catch (Exception)
            {
                return "ROL001";
            }
        }

        // POST: ControladorRol/ValidarNombreRol
        [HttpPost]
        public JsonResult ValidarNombreRol(string nombre, string codigoExcluir = null)
        {
            try
            {
                var esUnico = CD_Rol.Instancia.ValidarNombreRolUnico(nombre, codigoExcluir);
                return Json(new { success = true, esUnico = esUnico });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: ControladorRol/ObtenerEstados
        [HttpPost]
        public JsonResult ObtenerEstados()
        {
            try
            {
                var estados = new List<string> { "ACTIVO", "INACTIVO" };
                return Json(new { success = true, estados = estados });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

    }
}