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

                // Pasar los roles al ViewBag
                ViewBag.Roles = roles;

                // También generar ID para creación
                ViewBag.NewRolId = GenerarIdRolAutomatico();

                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error: {ex.Message}";
                ViewBag.Roles = new List<Rol>();
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
            nuevoRol.XEROL_ID = nuevoId;

            return View(nuevoRol);
        }

        // POST: ControladorRol/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Crear(FormCollection form)
        {
            try
            {
                var nuevoRol = new Rol
                {
                    XEROL_ID = form["XEROL_ID"],
                    XEROL_NOMBRE = form["XEROL_NOMBRE"],
                    XEROL_DESCRI = form["XEROL_DESCRI"]
                };

                // Validaciones
                if (string.IsNullOrEmpty(nuevoRol.XEROL_ID))
                {
                    ViewBag.Error = "El ID del rol es requerido";
                    ViewBag.IdGenerado = GenerarIdRolAutomatico();
                    return View(nuevoRol);
                }

                if (string.IsNullOrEmpty(nuevoRol.XEROL_NOMBRE))
                {
                    ViewBag.Error = "El nombre del rol es requerido";
                    ViewBag.IdGenerado = nuevoRol.XEROL_ID;
                    return View(nuevoRol);
                }

                // Validar que el ID sea único
                var rolExistente = CD_Rol.Instancia.ObtenerDetalleRol(nuevoRol.XEROL_ID);
                if (rolExistente != null)
                {
                    ViewBag.Error = $"El ID '{nuevoRol.XEROL_ID}' ya existe";
                    ViewBag.IdGenerado = GenerarIdRolAutomatico();
                    return View(nuevoRol);
                }

                // Validar que el nombre sea único
                if (!CD_Rol.Instancia.ValidarNombreRolUnico(nuevoRol.XEROL_NOMBRE))
                {
                    ViewBag.Error = $"El nombre '{nuevoRol.XEROL_NOMBRE}' ya existe";
                    ViewBag.IdGenerado = nuevoRol.XEROL_ID;
                    return View(nuevoRol);
                }

                // Registrar el rol
                var resultado = CD_Rol.Instancia.RegistrarRol(nuevoRol);

                if (resultado)
                {
                    TempData["SuccessMessage"] = "Rol creado correctamente";
                    return RedirectToAction("GestionarRol");
                }
                else
                {
                    ViewBag.Error = "Error al crear el rol";
                    ViewBag.IdGenerado = nuevoRol.XEROL_ID;
                    return View(nuevoRol);
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error: {ex.Message}";
                ViewBag.IdGenerado = GenerarIdRolAutomatico();
                return View(new Rol());
            }
        }

        // GET: ControladorRol/Editar/{id}
        public ActionResult Editar(string id)
        {
            try
            {
                var rol = CD_Rol.Instancia.ObtenerDetalleRol(id);
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

        // POST: ControladorRol/Editar/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Editar(FormCollection form)
        {
            try
            {
                var rolEditado = new Rol
                {
                    XEROL_ID = form["XEROL_ID"],
                    XEROL_NOMBRE = form["XEROL_NOMBRE"],
                    XEROL_DESCRI = form["XEROL_DESCRI"]
                };

                // Validaciones
                if (string.IsNullOrEmpty(rolEditado.XEROL_NOMBRE))
                {
                    ViewBag.Error = "El nombre del rol es requerido";
                    return View(rolEditado);
                }

                // Validar que el nombre sea único (excluyendo el rol actual)
                if (!CD_Rol.Instancia.ValidarNombreRolUnico(rolEditado.XEROL_NOMBRE, rolEditado.XEROL_ID))
                {
                    ViewBag.Error = $"El nombre '{rolEditado.XEROL_NOMBRE}' ya existe";
                    return View(rolEditado);
                }

                // Actualizar el rol
                var resultado = CD_Rol.Instancia.ModificarRol(rolEditado);

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
                return View(new Rol());
            }
        }

        // GET: ControladorRol/Eliminar/{id}
        public ActionResult Eliminar(string id)
        {
            try
            {
                var rol = CD_Rol.Instancia.ObtenerDetalleRol(id);
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

        // POST: ControladorRol/Eliminar/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Eliminar")]
        public ActionResult EliminarConfirmado(string id)
        {
            try
            {
                var resultado = CD_Rol.Instancia.EliminarRol(id);

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

        // GET: ControladorRol/Detalles/{id}
        public ActionResult Detalles(string id)
        {
            try
            {
                var rol = CD_Rol.Instancia.ObtenerDetalleRol(id);
                if (rol != null)
                {
                    // Obtener usuarios con este rol
                    var usuarios = CD_Usuario.Instancia.ObtenerUsuarios();
                    var usuariosConRol = usuarios?.Where(u => u.XEROL_ID == id).ToList() ?? new List<Usuario>();
                    ViewBag.UsuariosConRol = usuariosConRol;
                    ViewBag.TotalUsuarios = usuariosConRol.Count;

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

        // GET: ControladorRol/AsignarRol (Vista principal de asignación)
        public ActionResult AsignarRol()
        {
            try
            {
                // Cargar todos los roles
                var roles = CD_Rol.Instancia.ObtenerRoles();

                // Crear SelectList para el dropdown
                // Opción 1: Usando SelectList directamente
                ViewBag.RolesSelectList = new SelectList(roles ?? new List<Rol>(),
                    "XEROL_ID", "XEROL_NOMBRE");

                // Opción 2: O también puedes pasar la lista como ViewBag.RolesList
                ViewBag.RolesList = roles ?? new List<Rol>();

                // Inicializar modelo de vista
                var model = new AsignacionRolViewModel
                {
                    RolSeleccionadoId = "",
                    UsuariosSinRol = new List<Usuario>(),
                    UsuariosConRol = new List<Usuario>(),
                    UsuariosSeleccionadosSinRol = new List<string>(),
                    UsuariosSeleccionadosConRol = new List<string>()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error: {ex.Message}";
                return View(new AsignacionRolViewModel());
            }
        }

        // POST: ControladorRol/AsignarRol (Cargar usuarios según rol seleccionado)
        [HttpPost]
        public ActionResult CargarUsuariosRol(string rolId)
        {
            try
            {
                Console.WriteLine($"=== Cargando usuarios para rol ID: {rolId} ===");

                if (string.IsNullOrEmpty(rolId))
                {
                    return Json(new { success = false, message = "ID de rol requerido" });
                }

                // Obtener rol seleccionado
                var rol = CD_Rol.Instancia.ObtenerDetalleRol(rolId);
                if (rol == null)
                {
                    return Json(new { success = false, message = "Rol no encontrado" });
                }

                // Obtener TODOS los usuarios SIN rol
                var todosUsuarios = CD_Usuario.Instancia.ObtenerUsuarios();
                var usuariosSinRol = todosUsuarios?
                    .Where(u => string.IsNullOrEmpty(u.XEROL_ID))
                    .ToList() ?? new List<Usuario>();

                // Obtener usuarios CON este rol específico
                var usuariosConRol = todosUsuarios?
                    .Where(u => u.XEROL_ID == rolId)
                    .ToList() ?? new List<Usuario>();

                Console.WriteLine($"Usuarios sin rol (izquierda): {usuariosSinRol.Count}");
                Console.WriteLine($"Usuarios con este rol (derecha): {usuariosConRol.Count}");

                // Preparar datos para JSON
                var usuariosSinRolData = usuariosSinRol.Select(u => new
                {
                    id = u.XEUSU_ID,
                    nombre = u.XEUSU_NOMBRE,
                    descripcion = $"{u.XEUSU_NOMBRE} ({u.XEUSU_ID})",
                    estado = u.XEUSU_ESTADO
                }).ToList();

                var usuariosConRolData = usuariosConRol.Select(u => new
                {
                    id = u.XEUSU_ID,
                    nombre = u.XEUSU_NOMBRE,
                    descripcion = $"{u.XEUSU_NOMBRE} ({u.XEUSU_ID})",
                    estado = u.XEUSU_ESTADO
                }).ToList();

                return Json(new
                {
                    success = true,
                    rolNombre = rol.XEROL_NOMBRE,
                    usuariosSinRol = usuariosSinRolData,
                    usuariosConRol = usuariosConRolData,
                    totalSinRol = usuariosSinRol.Count,
                    totalConRol = usuariosConRol.Count
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar usuarios: {ex.Message}");
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // POST: ControladorRol/AsignarUsuarios (Mover usuarios de sin rol a con rol)
        [HttpPost]
        public ActionResult AsignarUsuarios(string rolId, List<string> usuariosIds)
        {
            try
            {
                if (string.IsNullOrEmpty(rolId) || usuariosIds == null || !usuariosIds.Any())
                {
                    return Json(new { success = false, message = "Datos incompletos" });
                }

                var rol = CD_Rol.Instancia.ObtenerDetalleRol(rolId);
                if (rol == null)
                {
                    return Json(new { success = false, message = "Rol no encontrado" });
                }

                int asignados = 0;
                foreach (var usuarioId in usuariosIds)
                {
                    // Usar el nuevo método que solo actualiza el rol
                    if (CD_Usuario.Instancia.ActualizarRolUsuario(usuarioId, rolId))
                    {
                        asignados++;
                        Console.WriteLine($"Rol asignado a usuario ID: {usuarioId}");
                    }
                }

                return Json(new
                {
                    success = true,
                    message = $"Se asignaron {asignados} usuarios al rol",
                    asignados = asignados
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al asignar usuarios: {ex.Message}");
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // POST: ControladorRol/QuitarUsuarios (Mover usuarios de con rol a sin rol)
        [HttpPost]
        public ActionResult QuitarUsuarios(string rolId, List<string> usuariosIds)
        {
            try
            {
                if (string.IsNullOrEmpty(rolId) || usuariosIds == null || !usuariosIds.Any())
                {
                    return Json(new { success = false, message = "Datos incompletos" });
                }

                var rol = CD_Rol.Instancia.ObtenerDetalleRol(rolId);
                if (rol == null)
                {
                    return Json(new { success = false, message = "Rol no encontrado" });
                }

                int quitados = 0;
                foreach (var usuarioId in usuariosIds)
                {
                    // Usar el nuevo método que solo actualiza el rol (null para quitar)
                    if (CD_Usuario.Instancia.ActualizarRolUsuario(usuarioId, null))
                    {
                        quitados++;
                        Console.WriteLine($"Rol quitado de usuario ID: {usuarioId}");
                    }
                }

                return Json(new
                {
                    success = true,
                    message = $"Se quitaron {quitados} usuarios del rol",
                    quitados = quitados
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al quitar usuarios: {ex.Message}");
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // POST: ControladorRol/GuardarCambios (Guardar todas las asignaciones)
        [HttpPost]
        public ActionResult GuardarCambios(string rolId, List<string> usuariosConRolIds)
        {
            try
            {
                if (string.IsNullOrEmpty(rolId))
                {
                    return Json(new { success = false, message = "ID de rol requerido" });
                }

                var rol = CD_Rol.Instancia.ObtenerDetalleRol(rolId);
                if (rol == null)
                {
                    return Json(new { success = false, message = "Rol no encontrado" });
                }

                Console.WriteLine($"=== Guardando cambios para rol: {rol.XEROL_NOMBRE} ===");

                // Obtener usuarios que actualmente tienen este rol
                var todosUsuarios = CD_Usuario.Instancia.ObtenerUsuarios();
                var usuariosConRolOriginal = todosUsuarios?
                    .Where(u => u.XEROL_ID == rolId)
                    .ToList() ?? new List<Usuario>();

                Console.WriteLine($"Usuarios con rol originalmente: {usuariosConRolOriginal.Count}");

                // 1. Quitar rol a usuarios que ya no están en la lista
                foreach (var usuarioOriginal in usuariosConRolOriginal)
                {
                    bool sigueEnLista = usuariosConRolIds?.Contains(usuarioOriginal.XEUSU_ID) ?? false;

                    if (!sigueEnLista)
                    {
                        // Usar el nuevo método para quitar el rol
                        if (CD_Usuario.Instancia.ActualizarRolUsuario(usuarioOriginal.XEUSU_ID, null))
                        {
                            Console.WriteLine($"Rol quitado de: {usuarioOriginal.XEUSU_ID}");
                        }
                    }
                }

                // 2. Asignar rol a nuevos usuarios
                if (usuariosConRolIds != null)
                {
                    foreach (var usuarioId in usuariosConRolIds)
                    {
                        bool yaTieneRol = usuariosConRolOriginal.Any(u => u.XEUSU_ID == usuarioId);

                        if (!yaTieneRol)
                        {
                            // Usar el nuevo método para asignar el rol
                            if (CD_Usuario.Instancia.ActualizarRolUsuario(usuarioId, rolId))
                            {
                                Console.WriteLine($"Rol asignado a: {usuarioId}");
                            }
                        }
                    }
                }

                Console.WriteLine("=== Cambios guardados correctamente ===");
                return Json(new { success = true, message = "Asignaciones guardadas correctamente" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al guardar cambios: {ex.Message}");
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }
        // GET: ControladorRol/RegenerarId (Regenerar ID automático)
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

        // Método para generar ID de rol automático
        private string GenerarIdRolAutomatico()
        {
            try
            {
                var roles = CD_Rol.Instancia.ObtenerRoles();
                if (roles == null || roles.Count == 0)
                {
                    return "RO001";
                }

                // Buscar máximo número en IDs ROXXX
                int maxNumero = 0;
                foreach (var rol in roles)
                {
                    if (rol.XEROL_ID != null &&
                        rol.XEROL_ID.StartsWith("RO") &&
                        rol.XEROL_ID.Length == 5)
                    {
                        try
                        {
                            string numeroStr = rol.XEROL_ID.Substring(2);
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
                    string idCandidato = $"RO{i:000}";
                    bool existe = roles.Any(r => r.XEROL_ID == idCandidato);
                    if (!existe)
                    {
                        return idCandidato;
                    }
                }

                // Si no hay huecos, usar siguiente número
                return $"RO{maxNumero + 1:000}";
            }
            catch (Exception)
            {
                return "RO001";
            }
        }

        // Método para obtener estadísticas
        [HttpPost]
        public JsonResult ObtenerEstadisticas(string rolId)
        {
            try
            {
                if (string.IsNullOrEmpty(rolId))
                {
                    return Json(new { success = false, message = "ID de rol requerido" });
                }

                var todosUsuarios = CD_Usuario.Instancia.ObtenerUsuarios();
                var usuariosSinRol = todosUsuarios?.Where(u => string.IsNullOrEmpty(u.XEROL_ID)).Count() ?? 0;
                var usuariosConRol = todosUsuarios?.Where(u => u.XEROL_ID == rolId).Count() ?? 0;

                return Json(new
                {
                    success = true,
                    sinRol = usuariosSinRol,
                    conRol = usuariosConRol,
                    mensaje = $"Usuarios sin rol: {usuariosSinRol} | Usuarios con este rol: {usuariosConRol}"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
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
                    id = r.XEROL_ID,
                    nombre = r.XEROL_NOMBRE,
                    descripcion = r.XEROL_DESCRI
                }).ToList();

                return Json(new { success = true, roles = datos });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }

    // ViewModel para la asignación de roles
    public class AsignacionRolViewModel
    {
        public string RolSeleccionadoId { get; set; }
        public List<Usuario> UsuariosSinRol { get; set; }
        public List<Usuario> UsuariosConRol { get; set; }
        public List<string> UsuariosSeleccionadosSinRol { get; set; }
        public List<string> UsuariosSeleccionadosConRol { get; set; }
        public string RolNombre { get; set; }
        public int TotalUsuariosSinRol { get; set; }
        public int TotalUsuariosConRol { get; set; }
    }
}