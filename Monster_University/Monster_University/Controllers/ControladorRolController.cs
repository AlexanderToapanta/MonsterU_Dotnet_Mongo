using CapaDatos;
using CapaModelo;
using MongoDB.Bson;
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

        // GET: ControladorRol/AsignarRolPersonal (Vista principal para asignar roles a personas)
        public ActionResult AsignarRol()
        {
            try
            {
                Console.WriteLine("=== CARGANDO VISTA ASIGNAR ROL PERSONAL ===");

                // 1. Obtener roles desde MongoDB
                var roles = CD_Rol.Instancia.ObtenerRoles();
                Console.WriteLine($"Roles obtenidos del servicio: {roles?.Count ?? 0}");

                // 2. Preparar lista para el dropdown
                var listaRoles = new List<SelectListItem>();

                if (roles != null && roles.Any())
                {
                    foreach (var rol in roles)
                    {
                        Console.WriteLine($"Agregando rol:  {rol.Codigo} - {rol.Nombre}");

                        listaRoles.Add(new SelectListItem
                        {
                            Value = rol.Codigo,  // ObjectId como string
                            Text = $"{rol.Nombre} ({rol.Codigo})"
                        });
                    }
                }
                else
                {
                    Console.WriteLine("No se encontraron roles en la base de datos");
                }

                // 3. Pasar a ViewBag como SelectList
                ViewBag.Roles = new SelectList(listaRoles, "Value", "Text");
                Console.WriteLine($"Items en ViewBag.Roles: {listaRoles.Count}");

                // 4. También puedes pasar la lista original por si la necesitas
                ViewBag.ListaRolesOriginal = roles ?? new List<Rol>();

                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EXCEPCIÓN en AsignarRolPersonal: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");

                ViewBag.Error = $"Error: {ex.Message}";
                ViewBag.Roles = new SelectList(new List<SelectListItem>());
                return View();
            }
        }

        // GET: ControladorRol/PersonasSinRol (Obtener personas sin rol)
        [HttpGet]
        public JsonResult PersonasSinRol()
        {
            try
            {
                // Obtener todas las personas
                var personas = CD_Personal.Instancia.ObtenerPersonal();

                // Filtrar personas sin rol (rol es null o no tiene codigo)
                var personasSinRol = personas?.Where(p =>
                    p.rol == null ||
                    (p.rol is BsonDocument bsonDoc && !bsonDoc.Contains("codigo")) ||
                    (p.rol is Rol rolObj && string.IsNullOrEmpty(rolObj.Codigo))
                ).ToList() ?? new List<Personal>();

                return Json(new
                {
                    success = true,
                    personas = personasSinRol.Select(p => new
                    {
                        id = p.id,
                        codigo = p.codigo,
                        nombres = p.nombres,
                        apellidos = p.apellidos,
                        email = p.email,
                        estado = p.estado
                    }),
                    total = personasSinRol.Count
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Error: {ex.Message}"
                }, JsonRequestBehavior.AllowGet);
            }
        }
        // GET: ControladorRol/PersonasDisponiblesParaRol (Personas sin rol O con otros roles)
        [HttpGet]
        public JsonResult PersonasDisponiblesParaRol(string rolId)
        {
            try
            {
                if (string.IsNullOrEmpty(rolId))
                {
                    return Json(new
                    {
                        success = false,
                        message = "ID de rol requerido"
                    }, JsonRequestBehavior.AllowGet);
                }

                // Obtener el rol seleccionado para saber su código
                var rolSeleccionado = CD_Rol.Instancia.ObtenerDetalleRol(rolId);
                if (rolSeleccionado == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Rol no encontrado"
                    }, JsonRequestBehavior.AllowGet);
                }

                // Obtener todas las personas
                var todasPersonas = CD_Personal.Instancia.ObtenerPersonal();

                // Filtrar personas que NO tienen este rol específico
                // (pueden no tener rol o tener otro rol diferente)
                var personasDisponibles = todasPersonas?.Where(p =>
                {
                    if (p.rol == null) return true; // Persona sin rol

                    if (p.rol is BsonDocument bsonDoc && bsonDoc.Contains("codigo"))
                    {
                        // Verificar si el rol actual de la persona es diferente al seleccionado
                        return bsonDoc["codigo"].AsString != rolSeleccionado.Codigo;
                    }
                    else if (p.rol is Rol rolObj && !string.IsNullOrEmpty(rolObj.Codigo))
                    {
                        // Verificar si el rol actual de la persona es diferente al seleccionado
                        return rolObj.Codigo != rolSeleccionado.Codigo;
                    }

                    return true; // Si el rol no tiene código, está disponible
                }).ToList() ?? new List<Personal>();

                return Json(new
                {
                    success = true,
                    personas = personasDisponibles.Select(p => new
                    {
                        id = p.id,
                        codigo = p.codigo,
                        nombres = p.nombres,
                        apellidos = p.apellidos,
                        email = p.email,
                        estado = p.estado,
                        rolActual = p.rol == null ? "Sin rol" :
                            p.rol is BsonDocument bd && bd.Contains("nombre") ?
                            bd["nombre"].AsString :
                            p.rol is Rol r ? r.Nombre : "Rol asignado"
                    }),
                    total = personasDisponibles.Count
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Error: {ex.Message}"
                }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: ControladorRol/PersonasConRol/{rolId} (Obtener personas con un rol específico)
        [HttpGet]
        public JsonResult PersonasConRol(string rolId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== PersonasConRol ===");
                System.Diagnostics.Debug.WriteLine($"Rol ID recibido del dropdown: {rolId}");

                if (string.IsNullOrEmpty(rolId))
                {
                    return Json(new { success = false, message = "ID de rol requerido" });
                }

                // 1. Obtener el rol por su ID de MongoDB
                var rol = CD_Rol.Instancia.ObtenerDetalleRol(rolId);
                if (rol == null)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Rol con ID {rolId} NO encontrado");
                    return Json(new { success = false, message = "Rol no encontrado" });
                }

                System.Diagnostics.Debug.WriteLine($"✅ Rol encontrado: {rol.Nombre} (Código: {rol.Codigo})");

                // 2. Obtener todas las personas
                var todasPersonas = CD_Personal.Instancia.ObtenerPersonal();
                System.Diagnostics.Debug.WriteLine($"Total personas en BD: {todasPersonas?.Count ?? 0}");

                // 3. Filtrar personas por CÓDIGO del rol
                var personasConEsteRol = new List<Personal>();

                if (todasPersonas != null)
                {
                    foreach (var persona in todasPersonas)
                    {
                        if (persona.rol == null)
                        {
                            continue;
                        }

                        string codigoRolPersona = ObtenerCodigoRolDePersona(persona);

                        if (!string.IsNullOrEmpty(codigoRolPersona) && codigoRolPersona == rol.Codigo)
                        {
                            personasConEsteRol.Add(persona);
                            System.Diagnostics.Debug.WriteLine($"   ✅ {persona.nombres} {persona.apellidos} tiene rol {codigoRolPersona}");
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine($"Personas con rol {rol.Codigo}: {personasConEsteRol.Count}");

                return Json(new
                {
                    success = true,
                    rol = new
                    {
                        id = rol.Id,
                        codigo = rol.Codigo,
                        nombre = rol.Nombre,
                        descripcion = rol.Descripcion
                    },
                    personas = personasConEsteRol.Select(p => new
                    {
                        id = p.id,
                        codigo = p.codigo,
                        nombres = p.nombres,
                        apellidos = p.apellidos,
                        email = p.email,
                        estado = p.estado
                    }),
                    total = personasConEsteRol.Count
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR: {ex.Message}");
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // Método auxiliar para extraer código del rol
        private string ObtenerCodigoRolDePersona(Personal persona)
        {
            if (persona.rol == null) return null;

            if (persona.rol is BsonDocument bsonDoc)
            {
                if (bsonDoc.Contains("codigo"))
                    return bsonDoc["codigo"].AsString;
                if (bsonDoc.Contains("Codigo"))
                    return bsonDoc["Codigo"].AsString;
            }
            else if (persona.rol is Rol rolObj)
            {
                return rolObj.Codigo;
            }

            return null;
        }

        // POST: ControladorRol/AsignarRolAPersona (Asignar rol a una persona)
        [HttpPost]
        public JsonResult AsignarRolAPersona(string personaId, string rolId)
        {
            try
            {
                if (string.IsNullOrEmpty(personaId) || string.IsNullOrEmpty(rolId))
                {
                    return Json(new
                    {
                        success = false,
                        message = "ID de persona y rol requeridos"
                    });
                }

                // Obtener la persona
                var persona = CD_Personal.Instancia.ObtenerPersonalPorId(personaId);
                if (persona == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Persona no encontrada"
                    });
                }

                // Verificar si ya tiene un rol asignado
                bool tieneRol = false;
                if (persona.rol != null)
                {
                    if (persona.rol is BsonDocument bsonDoc)
                    {
                        tieneRol = bsonDoc.Contains("codigo") && !string.IsNullOrEmpty(bsonDoc["codigo"].AsString);
                    }
                    else if (persona.rol is Rol rolObj)
                    {
                        tieneRol = !string.IsNullOrEmpty(rolObj.Codigo);
                    }
                }

                if (tieneRol)
                {
                    return Json(new
                    {
                        success = false,
                        message = "La persona ya tiene un rol asignado. Debe quitar el rol actual primero."
                    });
                }

                // Obtener el rol
                var rol = CD_Rol.Instancia.ObtenerDetalleRol(rolId);
                if (rol == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Rol no encontrado"
                    });
                }

                // Crear objeto rol para asignar
                var rolParaAsignar = new
                {
                    codigo = rol.Codigo,
                    nombre = rol.Nombre,
                    descripcion = rol.Descripcion,
                    opciones_permitidas = rol.OpcionesPermitidas,
                    estado = rol.Estado
                };

                // Asignar el rol a la persona
                var resultado = CD_Personal.Instancia.ActualizarRolPersona(personaId, rolParaAsignar);

                if (resultado)
                {
                    return Json(new
                    {
                        success = true,
                        message = $"Rol '{rol.Nombre}' asignado correctamente a {persona.nombres} {persona.apellidos}"
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = "Error al asignar el rol"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Error: {ex.Message}"
                });
            }
        }

        // POST: ControladorRol/AsignarRolAVariasPersonas (Asignar rol a múltiples personas)
        [HttpPost]
        public JsonResult AsignarRolAVariasPersonas(string rolId, List<string> personasIds)
        {
            try
            {
                if (string.IsNullOrEmpty(rolId) || personasIds == null || !personasIds.Any())
                {
                    return Json(new
                    {
                        success = false,
                        message = "Datos incompletos"
                    });
                }

                // Obtener el rol
                var rol = CD_Rol.Instancia.ObtenerDetalleRol(rolId);
                if (rol == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Rol no encontrado"
                    });
                }

                // Preparar objeto rol para asignar
                var rolParaAsignar = new
                {
                    codigo = rol.Codigo,
                    nombre = rol.Nombre,
                    descripcion = rol.Descripcion,
                    opciones_permitidas = rol.OpcionesPermitidas,
                    estado = rol.Estado
                };

                int asignadosExitosamente = 0;
                List<string> errores = new List<string>();

                foreach (var personaId in personasIds)
                {
                    try
                    {
                        // Obtener la persona
                        var persona = CD_Personal.Instancia.ObtenerPersonalPorId(personaId);
                        if (persona == null)
                        {
                            errores.Add($"Persona con ID {personaId} no encontrada");
                            continue;
                        }

                        // Verificar si ya tiene rol
                        bool tieneRol = false;
                        if (persona.rol != null)
                        {
                            if (persona.rol is BsonDocument bsonDoc)
                            {
                                tieneRol = bsonDoc.Contains("codigo") && !string.IsNullOrEmpty(bsonDoc["codigo"].AsString);
                            }
                            else if (persona.rol is Rol rolObj)
                            {
                                tieneRol = !string.IsNullOrEmpty(rolObj.Codigo);
                            }
                        }

                        if (tieneRol)
                        {
                            errores.Add($"{persona.nombres} {persona.apellidos} ya tiene un rol asignado");
                            continue;
                        }

                        // Asignar el rol
                        var resultado = CD_Personal.Instancia.ActualizarRolPersona(personaId, rolParaAsignar);
                        if (resultado)
                        {
                            asignadosExitosamente++;
                        }
                        else
                        {
                            errores.Add($"Error al asignar rol a {persona.nombres} {persona.apellidos}");
                        }
                    }
                    catch (Exception ex)
                    {
                        errores.Add($"Error con persona {personaId}: {ex.Message}");
                    }
                }

                return Json(new
                {
                    success = asignadosExitosamente > 0,
                    message = $"Se asignaron roles a {asignadosExitosamente} personas exitosamente",
                    asignados = asignadosExitosamente,
                    errores = errores,
                    totalErrores = errores.Count
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Error general: {ex.Message}"
                });
            }
        }

        // POST: ControladorRol/QuitarRolDePersona (Quitar rol de una persona)
        [HttpPost]
        public JsonResult QuitarRolDePersona(string personaId)
        {
            try
            {
                if (string.IsNullOrEmpty(personaId))
                {
                    return Json(new
                    {
                        success = false,
                        message = "ID de persona requerido"
                    });
                }

                // Obtener la persona
                var persona = CD_Personal.Instancia.ObtenerPersonalPorId(personaId);
                if (persona == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Persona no encontrada"
                    });
                }

                // Verificar si tiene rol
                bool tieneRol = false;
                string nombreRol = "";

                if (persona.rol != null)
                {
                    if (persona.rol is BsonDocument bsonDoc && bsonDoc.Contains("codigo"))
                    {
                        tieneRol = true;
                        nombreRol = bsonDoc.Contains("nombre") ? bsonDoc["nombre"].AsString : "Rol Desconocido";
                    }
                    else if (persona.rol is Rol rolObj && !string.IsNullOrEmpty(rolObj.Codigo))
                    {
                        tieneRol = true;
                        nombreRol = rolObj.Nombre;
                    }
                }

                if (!tieneRol)
                {
                    return Json(new
                    {
                        success = false,
                        message = "La persona no tiene ningún rol asignado"
                    });
                }

                // Quitar el rol (establecer en null)
                var resultado = CD_Personal.Instancia.ActualizarRolPersona(personaId, null);

                if (resultado)
                {
                    return Json(new
                    {
                        success = true,
                        message = $"Rol '{nombreRol}' quitado correctamente de {persona.nombres} {persona.apellidos}"
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = "Error al quitar el rol"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Error: {ex.Message}"
                });
            }
        }

        // POST: ControladorRol/QuitarRolDeVariasPersonas (Quitar rol de múltiples personas)
        [HttpPost]
        public JsonResult QuitarRolDeVariasPersonas(List<string> personasIds)
        {
            try
            {
                if (personasIds == null || !personasIds.Any())
                {
                    return Json(new
                    {
                        success = false,
                        message = "Lista de personas requerida"
                    });
                }

                int quitadosExitosamente = 0;
                List<string> errores = new List<string>();

                foreach (var personaId in personasIds)
                {
                    try
                    {
                        // Obtener la persona
                        var persona = CD_Personal.Instancia.ObtenerPersonalPorId(personaId);
                        if (persona == null)
                        {
                            errores.Add($"Persona con ID {personaId} no encontrada");
                            continue;
                        }

                        // Quitar el rol
                        var resultado = CD_Personal.Instancia.ActualizarRolPersona(personaId, null);
                        if (resultado)
                        {
                            quitadosExitosamente++;
                        }
                        else
                        {
                            errores.Add($"Error al quitar rol de {persona.nombres} {persona.apellidos}");
                        }
                    }
                    catch (Exception ex)
                    {
                        errores.Add($"Error con persona {personaId}: {ex.Message}");
                    }
                }

                return Json(new
                {
                    success = quitadosExitosamente > 0,
                    message = $"Se quitaron roles de {quitadosExitosamente} personas exitosamente",
                    quitados = quitadosExitosamente,
                    errores = errores,
                    totalErrores = errores.Count
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Error general: {ex.Message}"
                });
            }
        }

        // GET: ControladorRol/DetallesPersonaRol/{personaId} (Ver detalles del rol de una persona)
        [HttpGet]
        public JsonResult DetallesPersonaRol(string personaId)
        {
            try
            {
                if (string.IsNullOrEmpty(personaId))
                {
                    return Json(new
                    {
                        success = false,
                        message = "ID de persona requerido"
                    }, JsonRequestBehavior.AllowGet);
                }

                // Obtener la persona
                var persona = CD_Personal.Instancia.ObtenerPersonalPorId(personaId);
                if (persona == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Persona no encontrada"
                    }, JsonRequestBehavior.AllowGet);
                }

                // Extraer información del rol si existe
                object rolInfo = null;
                bool tieneRol = false;

                if (persona.rol != null)
                {
                    if (persona.rol is BsonDocument bsonDoc && bsonDoc.Contains("codigo"))
                    {
                        tieneRol = true;
                        rolInfo = new
                        {
                            codigo = bsonDoc["codigo"].AsString,
                            nombre = bsonDoc.Contains("nombre") ? bsonDoc["nombre"].AsString : "",
                            descripcion = bsonDoc.Contains("descripcion") ? bsonDoc["descripcion"].AsString : "",
                            estado = bsonDoc.Contains("estado") ? bsonDoc["estado"].AsString : "",
                            opciones_permitidas = bsonDoc.Contains("opciones_permitidas") ?
                                bsonDoc["opciones_permitidas"].AsBsonArray.Select(x => x.AsString).ToList() :
                                new List<string>()
                        };
                    }
                    else if (persona.rol is Rol rolObj && !string.IsNullOrEmpty(rolObj.Codigo))
                    {
                        tieneRol = true;
                        rolInfo = new
                        {
                            codigo = rolObj.Codigo,
                            nombre = rolObj.Nombre,
                            descripcion = rolObj.Descripcion,
                            estado = rolObj.Estado,
                            opciones_permitidas = rolObj.OpcionesPermitidas
                        };
                    }
                }

                return Json(new
                {
                    success = true,
                    persona = new
                    {
                        id = persona.id,
                        codigo = persona.codigo,
                        nombres = persona.nombres,
                        apellidos = persona.apellidos,
                        email = persona.email,
                        estado = persona.estado
                    },
                    tieneRol = tieneRol,
                    rol = rolInfo
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Error: {ex.Message}"
                }, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: ControladorRol/VerificarPersonaSinRol/{personaId} (Verificar si una persona puede recibir un rol)
        [HttpPost]
        public JsonResult VerificarPersonaSinRol(string personaId)
        {
            try
            {
                if (string.IsNullOrEmpty(personaId))
                {
                    return Json(new
                    {
                        success = false,
                        message = "ID de persona requerido",
                        puedeAsignar = false
                    });
                }

                // Obtener la persona
                var persona = CD_Personal.Instancia.ObtenerPersonalPorId(personaId);
                if (persona == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Persona no encontrada",
                        puedeAsignar = false
                    });
                }

                // Verificar si tiene rol
                bool tieneRol = false;
                if (persona.rol != null)
                {
                    if (persona.rol is BsonDocument bsonDoc)
                    {
                        tieneRol = bsonDoc.Contains("codigo") && !string.IsNullOrEmpty(bsonDoc["codigo"].AsString);
                    }
                    else if (persona.rol is Rol rolObj)
                    {
                        tieneRol = !string.IsNullOrEmpty(rolObj.Codigo);
                    }
                }

                return Json(new
                {
                    success = true,
                    puedeAsignar = !tieneRol,
                    persona = new
                    {
                        id = persona.id,
                        codigo = persona.codigo,
                        nombres = persona.nombres,
                        apellidos = persona.apellidos
                    },
                    mensaje = tieneRol ?
                        "La persona ya tiene un rol asignado" :
                        "La persona puede recibir un rol"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Error: {ex.Message}",
                    puedeAsignar = false
                });
            }
        }

    }
}