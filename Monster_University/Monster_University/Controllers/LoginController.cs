using CapaDatos;
using CapaModelo;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Security;

namespace Monster_University.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login/Login
        public ActionResult Login()
        {
            // Si ya está autenticado, redirigir al dashboard
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string XEUSU_NOMBRE, string XEUSU_CONTRA)
        {
            var respuesta = LoginUsuario(XEUSU_NOMBRE, XEUSU_CONTRA);

            if (respuesta.estado)
            {
                FormsAuthentication.SetAuthCookie(XEUSU_NOMBRE, false);

                var usuarioDetalle = ObtenerUsuarioPorNombre(XEUSU_NOMBRE);
                if (usuarioDetalle.estado && usuarioDetalle.objeto != null)
                {
                    // PARA DEBUG: Verificar EXACTAMENTE qué usuario y rol se obtuvo
                    System.Diagnostics.Debug.WriteLine("=== DEBUG INICIO ===");
                    System.Diagnostics.Debug.WriteLine($"Usuario autenticado: {XEUSU_NOMBRE}");
                    System.Diagnostics.Debug.WriteLine($"Usuario obtenido de BD: {usuarioDetalle.objeto.XEUSU_NOMBRE}");
                    System.Diagnostics.Debug.WriteLine($"ID Usuario: {usuarioDetalle.objeto.XEUSU_ID}");
                    System.Diagnostics.Debug.WriteLine($"ROL del Usuario: {usuarioDetalle.objeto.XEROL_ID}");
                    System.Diagnostics.Debug.WriteLine($"ROL del Usuario (raw): '{usuarioDetalle.objeto.XEROL_ID}'");

                    // Guardar el objeto Usuario completo en sesión
                    Session["Usuario"] = usuarioDetalle.objeto;
                    Session["UsuarioID"] = usuarioDetalle.objeto.XEUSU_ID;
                    Session["UsuarioEstado"] = usuarioDetalle.objeto.XEUSU_ESTADO;

                    // Verificar si el rol no es null o vacío
                    if (!string.IsNullOrEmpty(usuarioDetalle.objeto.XEROL_ID))
                    {
                        string rolId = usuarioDetalle.objeto.XEROL_ID.Trim();
                        System.Diagnostics.Debug.WriteLine($"Obteniendo opciones para rol: '{rolId}'");

                        // PARA DEBUG: Mostrar qué rol estamos consultando
                        System.Diagnostics.Debug.WriteLine($"Longitud del rol: {rolId.Length}");

                        // Primero verificamos si el rol existe
                        bool rolExiste = CD_Rol.Instancia.RolExiste(rolId);
                        System.Diagnostics.Debug.WriteLine($"¿Rol '{rolId}' existe?: {rolExiste}");

                        var opcionesRespuesta = ObtenerOpcionesPorRol(rolId);

                        System.Diagnostics.Debug.WriteLine($"Estado obtención opciones: {opcionesRespuesta.estado}");
                        System.Diagnostics.Debug.WriteLine($"Mensaje: {opcionesRespuesta.mensaje}");

                        if (opcionesRespuesta.estado && opcionesRespuesta.objeto != null)
                        {
                            Session["OpcionesUsuario"] = opcionesRespuesta.objeto;
                            System.Diagnostics.Debug.WriteLine($"Opciones cargadas: {opcionesRespuesta.objeto.Count}");

                            // Imprimir cada opción para debug
                            foreach (var opcion in opcionesRespuesta.objeto)
                            {
                                System.Diagnostics.Debug.WriteLine($"- {opcion.XEOPC_ID}: {opcion.XEOPC_NOMBRE}");
                            }
                        }
                        else
                        {
                            Session["OpcionesUsuario"] = new List<Opcion>();
                            System.Diagnostics.Debug.WriteLine("No se pudieron obtener opciones, lista vacía asignada");
                        }
                    }
                    else
                    {
                        Session["OpcionesUsuario"] = new List<Opcion>();
                        System.Diagnostics.Debug.WriteLine("Usuario sin rol asignado, lista vacía asignada");
                    }

                    System.Diagnostics.Debug.WriteLine("=== DEBUG FIN ===");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("No se pudo obtener detalle del usuario");
                    var tempUsuario = new Usuario
                    {
                        XEUSU_NOMBRE = XEUSU_NOMBRE,
                        XEUSU_ID = "TempID",
                        XEROL_ID = "TempRol"
                    };
                    Session["Usuario"] = tempUsuario;
                    Session["OpcionesUsuario"] = new List<Opcion>();
                }

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = respuesta.mensaje;
            return View();
        }

        // GET: Login/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Abandon();
            Session.Clear();
            return RedirectToAction("Login");
        }


        // Métodos auxiliares
        private Respuesta<int> LoginUsuario(string XEUSU_NOMBRE, string XEUSU_CONTRA)
        {
            Respuesta<int> response = new Respuesta<int>();
            try
            {
                if (string.IsNullOrEmpty(XEUSU_NOMBRE))
                {
                    response.estado = false;
                    response.mensaje = "El nombre de usuario es requerido";
                    return response;
                }

                if (string.IsNullOrEmpty(XEUSU_CONTRA))
                {
                    response.estado = false;
                    response.mensaje = "La contraseña es requerida";
                    return response;
                }

                var resultadoTupla = CD_Usuario.Instancia.LoginUsuario(XEUSU_NOMBRE, XEUSU_CONTRA);
                int resultado = resultadoTupla.Item1;

                response.estado = resultado > 0;
                response.objeto = resultado;
                response.mensaje = resultado > 0 ? "Login exitoso" : "Usuario o contraseña incorrectos";
            }
            catch (Exception ex)
            {
                response.estado = false;
                response.mensaje = "Error: " + ex.Message;
            }
            return response;
        }

        private Respuesta<Usuario> ObtenerUsuarioPorNombre(string XEUSU_NOMBRE)
        {
            Respuesta<Usuario> response = new Respuesta<Usuario>();
            try
            {
                var lista = CD_Usuario.Instancia.ObtenerUsuarios();
                Usuario usuario = lista?.Find(u =>
                    u.XEUSU_NOMBRE != null &&
                    u.XEUSU_NOMBRE.Trim().ToUpper() == XEUSU_NOMBRE.Trim().ToUpper()
                );

                // PARA DEBUG
                System.Diagnostics.Debug.WriteLine($"=== ObtenerUsuarioPorNombre ===");
                System.Diagnostics.Debug.WriteLine($"Buscando usuario: '{XEUSU_NOMBRE}'");
                System.Diagnostics.Debug.WriteLine($"Total usuarios en lista: {lista?.Count ?? 0}");

                if (usuario != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Usuario encontrado: {usuario.XEUSU_NOMBRE}");
                    System.Diagnostics.Debug.WriteLine($"Rol del usuario: '{usuario.XEROL_ID}'");
                    System.Diagnostics.Debug.WriteLine($"ID del usuario: {usuario.XEUSU_ID}");
                    System.Diagnostics.Debug.WriteLine($"Estado del usuario: {usuario.XEUSU_ESTADO}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Usuario NO encontrado");

                    // Listar todos los usuarios para debug
                    if (lista != null)
                    {
                        System.Diagnostics.Debug.WriteLine("Usuarios en la lista:");
                        foreach (var u in lista)
                        {
                            System.Diagnostics.Debug.WriteLine($"- '{u.XEUSU_NOMBRE}' (Rol: '{u.XEROL_ID}', ID: {u.XEUSU_ID})");
                        }
                    }
                }

                response.estado = usuario != null;
                response.objeto = usuario;
                response.mensaje = usuario != null ? "Usuario obtenido correctamente" : "Usuario no encontrado";
            }
            catch (Exception ex)
            {
                response.estado = false;
                response.mensaje = "Error: " + ex.Message;
                System.Diagnostics.Debug.WriteLine($"ERROR ObtenerUsuarioPorNombre: {ex.Message}");
            }
            return response;
        }

        private Respuesta<List<Opcion>> ObtenerOpcionesPorRol(string XEROL_ID)
        {
            Respuesta<List<Opcion>> response = new Respuesta<List<Opcion>>();
            try
            {
                if (string.IsNullOrEmpty(XEROL_ID))
                {
                    response.estado = false;
                    response.mensaje = "El ID del rol está vacío";
                    response.objeto = new List<Opcion>();
                    return response;
                }

                // Primero verificamos si el rol existe
                bool rolExiste = CD_Rol.Instancia.RolExiste(XEROL_ID);
                if (!rolExiste)
                {
                    response.estado = false;
                    response.mensaje = $"El rol '{XEROL_ID}' no existe";
                    response.objeto = new List<Opcion>();
                    return response;
                }

                // LLAMADA REAL a la capa de datos para obtener opciones
                System.Diagnostics.Debug.WriteLine($"=== Llamando a CD_Rol.ObtenerOpcionesPorRol('{XEROL_ID}') ===");
                var lista = CD_Rol.Instancia.ObtenerOpcionesPorRol(XEROL_ID);

                response.estado = lista != null;
                response.objeto = lista ?? new List<Opcion>();
                response.mensaje = lista != null ? $"Se obtuvieron {lista.Count} opciones" : "No se encontraron opciones";
            }
            catch (Exception ex)
            {
                response.estado = false;
                response.mensaje = "Error al obtener opciones: " + ex.Message;
                response.objeto = new List<Opcion>();

                // Para debug: imprime el error
                System.Diagnostics.Debug.WriteLine($"ERROR ObtenerOpcionesPorRol: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
            return response;
        }

        // Clase interna para respuestas
        private class Respuesta<T>
        {
            public bool estado { get; set; }
            public string mensaje { get; set; }
            public T objeto { get; set; }
        }
    }
}