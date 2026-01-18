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
        // GET: ControladorRol/AsignarOpRol
        public ActionResult AsignarOpRol()
        {
            try
            {
                // Obtener roles desde MongoDB
                var roles = CD_Rol.Instancia.ObtenerRoles() ?? new List<Rol>();

                // Crear SelectList usando "codigo" en lugar de "XEROL_ID"
                ViewBag.RolesSelectList = new SelectList(roles, "Codigo", "Nombre");

                // También pasar la lista por si acaso
                ViewBag.Roles = roles;

                // Obtener opciones del sistema desde configuraciones
                var opcionesSistema = CD_Configuracion.Instancia.ObtenerValoresConfiguracion("opciones_sistema");
                ViewBag.OpcionesSistema = opcionesSistema;

                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error: {ex.Message}";
                ViewBag.RolesSelectList = new SelectList(new List<Rol>(), "Codigo", "Nombre");
                ViewBag.Roles = new List<Rol>();
                ViewBag.OpcionesSistema = new List<Configuracion.ValorConfiguracion>();
                return View();
            }
        }

        // POST: ControladorRol/CargarOpcionesPorRol
        [HttpPost]
        public JsonResult CargarOpcionesPorRol(string codigoRol)  // Antes XEROL_ID
        {
            try
            {
                // Obtener el rol desde MongoDB
                var rol = CD_Rol.Instancia.ObtenerRolPorCodigo(codigoRol);
                if (rol == null)
                    return Json(new { success = false, message = "Rol no encontrado" });

                // Obtener todas las opciones del sistema
                var opcionesSistema = CD_Configuracion.Instancia.ObtenerValoresConfiguracion("opciones_sistema");

                // Crear lista de opciones con estado (asignada o no)
                var opcionesConEstado = opcionesSistema.Select(opcion => new
                {
                    // Cambiar XEOPC_ID por Codigo
                    Codigo = opcion.Codigo,
                    Nombre = opcion.Nombre,
                    Asignada = rol.OpcionesPermitidas != null &&
                               rol.OpcionesPermitidas.Contains(opcion.Codigo)
                }).ToList();

                return Json(new
                {
                    success = true,
                    data = opcionesConEstado,
                    rolNombre = rol.Nombre  // Antes XEROL_NOMBRE
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al cargar opciones: " + ex.Message });
            }
        }

        // POST: ControladorRol/AsignarOpcion
        [HttpPost]
        public JsonResult AsignarOpcion(string codigoRol, string codigoOpcion)  // Antes XEROL_ID, XEOPC_ID
        {
            try
            {
                var rol = CD_Rol.Instancia.ObtenerRolPorCodigo(codigoRol);
                if (rol == null)
                    return Json(new { success = false, message = "Rol no encontrado" });

                // Validar que la opción exista en configuraciones
                var opcionesSistema = CD_Configuracion.Instancia.ObtenerValoresConfiguracion("opciones_sistema");
                if (!opcionesSistema.Any(o => o.Codigo == codigoOpcion))
                    return Json(new { success = false, message = "Opción no válida" });

                // Inicializar lista si es null
                if (rol.OpcionesPermitidas == null)
                    rol.OpcionesPermitidas = new List<string>();

                // Agregar opción si no existe
                if (!rol.OpcionesPermitidas.Contains(codigoOpcion))
                {
                    rol.OpcionesPermitidas.Add(codigoOpcion);
                    var resultado = CD_Rol.Instancia.ModificarRol(rol);

                    return Json(new
                    {
                        success = resultado,
                        message = resultado ? "Opción asignada correctamente al rol" : "Error al actualizar el rol"
                    });
                }

                return Json(new
                {
                    success = true,
                    message = "La opción ya está asignada a este rol"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al asignar opción: " + ex.Message });
            }
        }

        // POST: ControladorRol/RetirarOpcion
        [HttpPost]
        public JsonResult RetirarOpcion(string codigoRol, string codigoOpcion)  // Antes XEROL_ID, XEOPC_ID
        {
            try
            {
                var rol = CD_Rol.Instancia.ObtenerRolPorCodigo(codigoRol);
                if (rol == null)
                    return Json(new { success = false, message = "Rol no encontrado" });

                if (rol.OpcionesPermitidas != null && rol.OpcionesPermitidas.Contains(codigoOpcion))
                {
                    rol.OpcionesPermitidas.Remove(codigoOpcion);
                    var resultado = CD_Rol.Instancia.ModificarRol(rol);

                    return Json(new
                    {
                        success = resultado,
                        message = resultado ? "Opción retirada correctamente del rol" : "Error al actualizar el rol"
                    });
                }

                return Json(new
                {
                    success = true,
                    message = "No se pudo retirar la opción o no estaba asignada"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al retirar opción: " + ex.Message });
            }
        }

        // POST: ControladorRol/AsignarMultiplesOpciones
        [HttpPost]
        public JsonResult AsignarMultiplesOpciones(string codigoRol, List<string> opcionesIds)  // Antes XEROL_ID, XEOPC_IDS
        {
            try
            {
                if (opcionesIds == null || !opcionesIds.Any())
                {
                    return Json(new { success = false, message = "No se seleccionaron opciones" });
                }

                var rol = CD_Rol.Instancia.ObtenerRolPorCodigo(codigoRol);
                if (rol == null)
                    return Json(new { success = false, message = "Rol no encontrado" });

                if (rol.OpcionesPermitidas == null)
                    rol.OpcionesPermitidas = new List<string>();

                // Validar opciones en sistema
                var opcionesSistema = CD_Configuracion.Instancia.ObtenerValoresConfiguracion("opciones_sistema");
                var opcionesValidas = opcionesSistema.Select(o => o.Codigo).ToList();

                int asignadosExitosamente = 0;
                List<string> errores = new List<string>();

                foreach (var opcionId in opcionesIds)
                {
                    try
                    {
                        // Validar que la opción exista
                        if (!opcionesValidas.Contains(opcionId))
                        {
                            errores.Add($"Opción {opcionId} no es válida");
                            continue;
                        }

                        // Agregar si no existe
                        if (!rol.OpcionesPermitidas.Contains(opcionId))
                        {
                            rol.OpcionesPermitidas.Add(opcionId);
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

                // Actualizar rol en MongoDB
                if (asignadosExitosamente > 0)
                {
                    var resultado = CD_Rol.Instancia.ModificarRol(rol);
                    if (!resultado)
                    {
                        errores.Add("Error al guardar los cambios en la base de datos");
                    }
                }

                return Json(new
                {
                    success = asignadosExitosamente > 0,
                    message = $"Se asignaron {asignadosExitosamente} de {opcionesIds.Count} opciones",
                    detalles = errores
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al asignar múltiples opciones: " + ex.Message });
            }
        }

        // GET: ControladorRol/ObtenerTodasAsignaciones
        [HttpGet]
        public JsonResult ObtenerTodasAsignaciones()
        {
            try
            {
                var roles = CD_Rol.Instancia.ObtenerRoles();
                var opcionesSistema = CD_Configuracion.Instancia.ObtenerValoresConfiguracion("opciones_sistema");

                var asignaciones = new List<object>();

                foreach (var rol in roles)
                {
                    if (rol.OpcionesPermitidas != null && rol.OpcionesPermitidas.Any())
                    {
                        foreach (var codigoOpcion in rol.OpcionesPermitidas)
                        {
                            var opcion = opcionesSistema.FirstOrDefault(o => o.Codigo == codigoOpcion);
                            asignaciones.Add(new
                            {
                                RolCodigo = rol.Codigo,
                                RolNombre = rol.Nombre,
                                OpcionCodigo = codigoOpcion,
                                OpcionNombre = opcion?.Nombre ?? "Desconocido",
                               
                                FechaAsignacion = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                            });
                        }
                    }
                }

                return Json(new { success = true, data = asignaciones }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al obtener asignaciones: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: ControladorRol/ObtenerOpcionesDisponibles
        [HttpGet]
        public JsonResult ObtenerOpcionesDisponibles()
        {
            try
            {
                // En MongoDB, las opciones están en configuraciones
                var opciones = CD_Configuracion.Instancia.ObtenerValoresConfiguracion("opciones_sistema");

                var opcionesFormateadas = opciones.Select(o => new
                {
                    Codigo = o.Codigo,      // Antes XEOPC_ID
                    Nombre = o.Nombre,      // Antes XEOPC_NOMBRE
                   
                }).ToList();

                return Json(new { success = true, data = opcionesFormateadas }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al obtener opciones: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: ControladorRol/ObtenerRoles
        [HttpGet]
        public JsonResult ObtenerRoles()
        {
            try
            {
                var roles = CD_Rol.Instancia.ObtenerRoles();

                var rolesFormateados = roles.Select(r => new
                {
                    Codigo = r.Codigo,          // Antes XEROL_ID
                    Nombre = r.Nombre,          // Antes XEROL_NOMBRE
                    Descripcion = r.Descripcion, // Antes XEROL_DESCRIPCION
                    Estado = r.Estado,
                    OpcionesCount = r.OpcionesPermitidas?.Count ?? 0
                }).ToList();

                return Json(new { success = true, data = rolesFormateados }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al obtener roles: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: ControladorRol/ReporteAsignaciones
        public ActionResult ReporteAsignaciones()
        {
            try
            {
                var roles = CD_Rol.Instancia.ObtenerRoles();
                var opcionesSistema = CD_Configuracion.Instancia.ObtenerValoresConfiguracion("opciones_sistema");

                var reporte = new List<object>();

                foreach (var rol in roles)
                {
                    var opcionesAsignadas = new List<object>();

                    if (rol.OpcionesPermitidas != null && rol.OpcionesPermitidas.Any())
                    {
                        foreach (var codigoOpcion in rol.OpcionesPermitidas)
                        {
                            var opcion = opcionesSistema.FirstOrDefault(o => o.Codigo == codigoOpcion);
                            opcionesAsignadas.Add(new
                            {
                                Codigo = codigoOpcion,
                                Nombre = opcion?.Nombre ?? "Desconocido",
                               
                            });
                        }
                    }

                    reporte.Add(new
                    {
                        Rol = rol.Nombre,
                        CodigoRol = rol.Codigo,
                        EstadoRol = rol.Estado,
                        TotalOpciones = rol.OpcionesPermitidas?.Count ?? 0,
                        OpcionesAsignadas = opcionesAsignadas
                    });
                }

                ViewBag.Reporte = reporte;
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al generar reporte: " + ex.Message;
                ViewBag.Reporte = new List<object>();
                return View();
            }
        }

        // GET: ControladorRol/DetalleRolOp/{id}
        public ActionResult DetalleRolOp(string id)
        {
            try
            {
                var rol = CD_Rol.Instancia.ObtenerRolPorCodigo(id);
                if (rol == null)
                {
                    return HttpNotFound();
                }

                // Obtener opciones asignadas con detalles
                var opcionesSistema = CD_Configuracion.Instancia.ObtenerValoresConfiguracion("opciones_sistema");
                var opcionesAsignadas = new List<Configuracion.ValorConfiguracion>();

                if (rol.OpcionesPermitidas != null)
                {
                    foreach (var codigoOpcion in rol.OpcionesPermitidas)
                    {
                        var opcion = opcionesSistema.FirstOrDefault(o => o.Codigo == codigoOpcion);
                        if (opcion != null)
                        {
                            opcionesAsignadas.Add(opcion);
                        }
                    }
                }

                ViewBag.OpcionesAsignadas = opcionesAsignadas;
                ViewBag.OpcionesSistema = opcionesSistema;

                return View(rol);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al cargar detalles: " + ex.Message;
                return View();
            }
        }

        // GET: ControladorRol/Index (redirección)
        public ActionResult Index()
        {
            return RedirectToAction("AsignarOpRol");
        }


    }
}