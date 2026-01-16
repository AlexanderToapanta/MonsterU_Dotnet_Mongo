using CapaDatos;
using CapaModelo;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Security;
using System.Linq;
using MongoDB.Bson;
using Newtonsoft.Json;

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

            // Limpiar sesión anterior
            Session.Clear();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string username, string password)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== INICIO PROCESO LOGIN ===");
                System.Diagnostics.Debug.WriteLine($"Usuario: {username}");

                var respuesta = LoginPersonal(username, password);

                if (respuesta.estado)
                {
                    FormsAuthentication.SetAuthCookie(username, false);

                    var personalDetalle = ObtenerPersonalPorUsername(username);
                    if (personalDetalle.estado && personalDetalle.objeto != null)
                    {
                        var personal = personalDetalle.objeto;

                        System.Diagnostics.Debug.WriteLine("=== PERSONAL ENCONTRADO ===");
                        System.Diagnostics.Debug.WriteLine($"Código: {personal.codigo}");
                        System.Diagnostics.Debug.WriteLine($"Nombre completo: {personal.nombres} {personal.apellidos}");
                        System.Diagnostics.Debug.WriteLine($"Username: {personal.username}");
                        System.Diagnostics.Debug.WriteLine($"Email: {personal.email}");
                        System.Diagnostics.Debug.WriteLine($"Estado: {personal.estado}");

                        // Verificar estado del personal
                        if (personal.estado?.ToUpper() != "ACTIVO")
                        {
                            System.Diagnostics.Debug.WriteLine($"❌ Personal no activo. Estado: {personal.estado}");
                            ViewBag.Error = "Su cuenta no está activa. Contacte al administrador.";
                            FormsAuthentication.SignOut();
                            return View();
                        }

                        // Guardar información en sesión
                        Session["Personal"] = personal;
                        Session["PersonalID"] = personal.codigo;
                        Session["PersonalNombre"] = $"{personal.nombres} {personal.apellidos}";
                        Session["PersonalEmail"] = personal.email;
                        Session["PersonalTipo"] = personal.peperTipo;

                        // Manejar el rol como objeto anidado
                        string rolCodigo = "";
                        Rol rolObjeto = null;

                        if (personal.rol != null)
                        {
                            // Intentar deserializar el rol
                            try
                            {
                                // Si rol es un BsonDocument, convertirlo a string primero
                                if (personal.rol is BsonDocument bsonRol)
                                {
                                    var rolJson = bsonRol.ToJson();
                                    rolObjeto = JsonConvert.DeserializeObject<Rol>(rolJson);
                                }
                                // Si ya es un string JSON
                                else if (personal.rol is string rolString)
                                {
                                    rolObjeto = JsonConvert.DeserializeObject<Rol>(rolString);
                                }
                                // Si ya es un objeto Rol
                                else if (personal.rol is Rol)
                                {
                                    rolObjeto = (Rol)personal.rol;
                                }

                                if (rolObjeto != null)
                                {
                                    rolCodigo = rolObjeto.Codigo;
                                    Session["Rol"] = rolObjeto;
                                    Session["RolCodigo"] = rolObjeto.Codigo;
                                    Session["RolNombre"] = rolObjeto.Nombre;

                                    System.Diagnostics.Debug.WriteLine($"✅ Rol obtenido: {rolObjeto.Codigo} - {rolObjeto.Nombre}");

                                    // Obtener opciones del rol
                                    var opcionesRespuesta = ObtenerOpcionesPorRol(rolObjeto.Codigo);
                                    if (opcionesRespuesta.estado && opcionesRespuesta.objeto != null)
                                    {
                                        Session["OpcionesPermitidas"] = opcionesRespuesta.objeto;
                                        System.Diagnostics.Debug.WriteLine($"✅ Opciones cargadas: {opcionesRespuesta.objeto.Count}");
                                    }
                                    else
                                    {
                                        Session["OpcionesPermitidas"] = new List<string>();
                                        System.Diagnostics.Debug.WriteLine("⚠️ No se pudieron obtener opciones del rol");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"❌ Error al procesar rol: {ex.Message}");
                                rolCodigo = "SIN_ROL";
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("⚠️ Personal sin rol asignado");
                            rolCodigo = "SIN_ROL";
                        }

                        Session["RolCodigo"] = rolCodigo;

                        // Obtener datos adicionales de configuración
                        CargarDatosAdicionales();

                        System.Diagnostics.Debug.WriteLine("=== LOGIN EXITOSO ===");
                        System.Diagnostics.Debug.WriteLine($"Redirigiendo a Home/Index...");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("❌ No se pudo obtener detalle del personal");
                        ViewBag.Error = "Error al obtener información del usuario.";
                        return View();
                    }

                    return RedirectToAction("Index", "Home");
                }

                ViewBag.Error = respuesta.mensaje;
                return View();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"💥 ERROR GENERAL EN LOGIN: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                ViewBag.Error = "Error interno del sistema. Intente nuevamente.";
                return View();
            }
        }

        // GET: Login/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            try
            {
                FormsAuthentication.SignOut();
                Session.Abandon();
                Session.Clear();

                System.Diagnostics.Debug.WriteLine("=== LOGOUT EXITOSO ===");
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"💥 ERROR EN LOGOUT: {ex.Message}");
                return RedirectToAction("Login");
            }
        }

        // Métodos auxiliares actualizados
        private Respuesta<int> LoginPersonal(string username, string password)
        {
            Respuesta<int> response = new Respuesta<int>();
            try
            {
                System.Diagnostics.Debug.WriteLine($"🔍 Validando login para: {username}");

                if (string.IsNullOrEmpty(username))
                {
                    response.estado = false;
                    response.mensaje = "El nombre de usuario es requerido";
                    return response;
                }

                if (string.IsNullOrEmpty(password))
                {
                    response.estado = false;
                    response.mensaje = "La contraseña es requerida";
                    return response;
                }

                // Usar el método LoginPersonal en lugar de LoginUsuario
                var resultado = CD_Personal.Instancia.LoginPersonal(username, password);

                response.estado = resultado > 0;
                response.objeto = resultado;
                response.mensaje = resultado > 0 ? "Login exitoso" : "Usuario o contraseña incorrectos";

                System.Diagnostics.Debug.WriteLine($"📋 Resultado login: {resultado} - {response.mensaje}");
            }
            catch (Exception ex)
            {
                response.estado = false;
                response.mensaje = "Error en el sistema: " + ex.Message;
                System.Diagnostics.Debug.WriteLine($"💥 ERROR LoginPersonal: {ex.Message}");
            }
            return response;
        }

        private Respuesta<Personal> ObtenerPersonalPorUsername(string username)
        {
            Respuesta<Personal> response = new Respuesta<Personal>();
            try
            {
                System.Diagnostics.Debug.WriteLine($"🔍 Buscando personal por username: {username}");

                var lista = CD_Personal.Instancia.ObtenerPersonal();
                Personal personal = null;

                if (lista != null)
                {
                    personal = lista.FirstOrDefault(p =>
                        p.username != null &&
                        p.username.Trim().ToUpper() == username.Trim().ToUpper() &&
                        p.estado?.ToUpper() == "ACTIVO"
                    );
                }

                if (personal != null)
                {
                    System.Diagnostics.Debug.WriteLine($"✅ Personal encontrado:");
                    System.Diagnostics.Debug.WriteLine($"   Código: {personal.codigo}");
                    System.Diagnostics.Debug.WriteLine($"   Nombres: {personal.nombres}");
                    System.Diagnostics.Debug.WriteLine($"   Apellidos: {personal.apellidos}");
                    System.Diagnostics.Debug.WriteLine($"   Email: {personal.email}");
                    System.Diagnostics.Debug.WriteLine($"   Tipo: {personal.peperTipo}");
                    System.Diagnostics.Debug.WriteLine($"   Rol: {(personal.rol != null ? "Presente" : "Ausente")}");

                    response.estado = true;
                    response.objeto = personal;
                    response.mensaje = "Personal obtenido correctamente";
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Personal NO encontrado o inactivo");

                    // Listar todos los usuarios para debug
                    if (lista != null)
                    {
                        System.Diagnostics.Debug.WriteLine("📋 Personal en la lista:");
                        foreach (var p in lista)
                        {
                            System.Diagnostics.Debug.WriteLine($"   - {p.username} (Nombre: {p.nombres}, Estado: {p.estado})");
                        }
                    }

                    response.estado = false;
                    response.mensaje = "Personal no encontrado o inactivo";
                }
            }
            catch (Exception ex)
            {
                response.estado = false;
                response.mensaje = "Error: " + ex.Message;
                System.Diagnostics.Debug.WriteLine($"💥 ERROR ObtenerPersonalPorUsername: {ex.Message}");
            }
            return response;
        }

        private Respuesta<List<string>> ObtenerOpcionesPorRol(string rolCodigo)
        {
            Respuesta<List<string>> response = new Respuesta<List<string>>();
            try
            {
                System.Diagnostics.Debug.WriteLine($"🔍 Obteniendo opciones para rol: {rolCodigo}");

                if (string.IsNullOrEmpty(rolCodigo))
                {
                    response.estado = false;
                    response.mensaje = "El código del rol está vacío";
                    response.objeto = new List<string>();
                    return response;
                }

                // Obtener el rol desde MongoDB usando CD_Rol
                var rol = CD_Rol.Instancia.ObtenerRolPorCodigo(rolCodigo);

                if (rol != null)
                {
                    response.estado = true;
                    response.objeto = rol.OpcionesPermitidas ?? new List<string>();
                    response.mensaje = $"Se obtuvieron {response.objeto.Count} opciones";

                    System.Diagnostics.Debug.WriteLine($"✅ Rol encontrado: {rol.Nombre}");
                    System.Diagnostics.Debug.WriteLine($"   Opciones permitidas: {response.objeto.Count}");

                    if (response.objeto.Count > 0)
                    {
                        foreach (var opcion in response.objeto)
                        {
                            System.Diagnostics.Debug.WriteLine($"   - {opcion}");
                        }
                    }
                }
                else
                {
                    response.estado = false;
                    response.mensaje = $"No se encontró el rol con código: {rolCodigo}";
                    response.objeto = new List<string>();

                    System.Diagnostics.Debug.WriteLine($"❌ Rol no encontrado: {rolCodigo}");
                }
            }
            catch (Exception ex)
            {
                response.estado = false;
                response.mensaje = "Error al obtener opciones: " + ex.Message;
                response.objeto = new List<string>();
                System.Diagnostics.Debug.WriteLine($"💥 ERROR ObtenerOpcionesPorRol: {ex.Message}");
            }
            return response;
        }

        private void CargarDatosAdicionales()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🔍 Cargando datos adicionales...");

                // Obtener sexos desde configuración
                var sexos = CD_Configuracion.Instancia.ObtenerSexos();
                if (sexos != null && sexos.Count > 0)
                {
                    Session["Sexos"] = sexos;
                    System.Diagnostics.Debug.WriteLine($"✅ Sexos cargados: {sexos.Count}");
                }

                // Obtener estados civiles desde configuración
                var estadosCiviles = CD_Configuracion.Instancia.ObtenerEstadosCiviles();
                if (estadosCiviles != null && estadosCiviles.Count > 0)
                {
                    Session["EstadosCiviles"] = estadosCiviles;
                    System.Diagnostics.Debug.WriteLine($"✅ Estados civiles cargados: {estadosCiviles.Count}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Error al cargar datos adicionales: {ex.Message}");
            }
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