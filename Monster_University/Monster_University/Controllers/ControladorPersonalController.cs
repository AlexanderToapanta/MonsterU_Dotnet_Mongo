using CapaDatos;
using CapaModelo;
using Monster_University.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Monster_University.Controllers
{
    public class ControladorPersonalController : Controller
    {
        private readonly string _rutaBaseImagenes;

        public ControladorPersonalController()
        {
            _rutaBaseImagenes = @"C:\Users\Usuario\Documents\MonsterUniversityDotnet\MonsterUDotnet\Monster_University\img";

            if (!Directory.Exists(_rutaBaseImagenes))
            {
                Directory.CreateDirectory(_rutaBaseImagenes);
                System.Diagnostics.Debug.WriteLine($"✅ Directorio creado: {_rutaBaseImagenes}");
            }
        }

        // GET: ControladorPersonal/crearpersonal
        public ActionResult crearpersonal()
        {
            var model = new Personal();
            model.fecha_ingreso = DateTime.Now;
            model.estado = "ACTIVO"; // Estado por defecto
            model.rol = null; // Rol se asigna aparte

            // Cargar listas desplegables
            CargarListasDesplegables();

            // Generar ID
            var nuevoId = GenerarCodigoPersonaAutomatico();
            ViewBag.IdGenerado = nuevoId;
            model.codigo = nuevoId;

            // Inicializar ruta de imágenes
            if (!Directory.Exists(_rutaBaseImagenes))
            {
                Directory.CreateDirectory(_rutaBaseImagenes);
                System.Diagnostics.Debug.WriteLine($"✅ Directorio de imágenes creado: {_rutaBaseImagenes}");
            }

            return View(model);
        }

        // POST: ControladorPersonal/crearpersonal
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CrearPersonal(Personal model, HttpPostedFileBase imagenPersona)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== INICIO CREAR PERSONAL ===");

                // Cargar listas desplegables (por si hay error)
                CargarListasDesplegables();

                // Verificar que el modelo tenga código
                if (string.IsNullOrEmpty(model.codigo))
                {
                    model.codigo = GenerarCodigoPersonaAutomatico();
                    System.Diagnostics.Debug.WriteLine($"🆔 Código generado automáticamente: {model.codigo}");
                }

                // 1. PROCESAR IMAGEN SI SE SUBIÓ
                string nombreArchivoImagen = null;
                if (imagenPersona != null && imagenPersona.ContentLength > 0)
                {
                    System.Diagnostics.Debug.WriteLine("📤 Procesando imagen subida...");

                    // Generar nombre único para la imagen basado en la cédula
                    string cedulaLimpia = new string(model.documento.Where(char.IsDigit).ToArray());
                    string extension = Path.GetExtension(imagenPersona.FileName);
                    nombreArchivoImagen = $"{cedulaLimpia}_{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{extension}";
                    string rutaCompleta = Path.Combine(_rutaBaseImagenes, nombreArchivoImagen);

                    // Guardar imagen
                    imagenPersona.SaveAs(rutaCompleta);
                    System.Diagnostics.Debug.WriteLine($"✅ Imagen guardada: {nombreArchivoImagen}");

                    // Guardar nombre de archivo en una propiedad dinámica (si tu modelo no tiene campo imagen)
                    // model.imagen_perfil = nombreArchivoImagen;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("📷 No se subió imagen, imagen_perfil será null");
                }

                // 2. GENERAR USERNAME Y PASSWORD SI NO EXISTEN
                if (string.IsNullOrEmpty(model.username))
                {
                    model.username = GenerarNombreUsuario(model);
                    System.Diagnostics.Debug.WriteLine($"👤 Username generado: {model.username}");
                }

                if (string.IsNullOrEmpty(model.password_hash))
                {
                    // Generar hash de la cédula como contraseña inicial
                    model.password_hash = GenerarHashPassword(model.documento);
                    System.Diagnostics.Debug.WriteLine($"🔐 Password hash generado");
                }

                // 3. ESTABLECER VALORES POR DEFECTO
                model.rol = null; // Rol se asigna aparte
                model.estado = "ACTIVO";

                // Si tu modelo Personal tiene un campo imagen_perfil, asigna el valor
                // model.imagen_perfil = nombreArchivoImagen; // Esto sería null si no se subió imagen

                // 4. VALIDAR DATOS DE LA PERSONA
                if (!ValidarDatosPersona(model))
                {
                    ViewBag.Error = "Datos inválidos. Revise los campos requeridos.";
                    ViewBag.IdGenerado = model.codigo;
                    return View(model);
                }

                // 5. GUARDAR PERSONA EN BASE DE DATOS
                System.Diagnostics.Debug.WriteLine("💾 Guardando persona en BD...");
                System.Diagnostics.Debug.WriteLine($"📋 Datos a guardar:");
                System.Diagnostics.Debug.WriteLine($"   Código: {model.codigo}");
                System.Diagnostics.Debug.WriteLine($"   Nombre: {model.nombres} {model.apellidos}");
                System.Diagnostics.Debug.WriteLine($"   Email: {model.email}");
                System.Diagnostics.Debug.WriteLine($"   Username: {model.username}");
                System.Diagnostics.Debug.WriteLine($"   Rol: {model.rol}");
                System.Diagnostics.Debug.WriteLine($"   Estado: {model.estado}");

                bool personaCreada = CD_Personal.Instancia.RegistrarPersona(model);

                if (!personaCreada)
                {
                    ViewBag.Error = "Error al guardar la persona en la base de datos.";
                    ViewBag.IdGenerado = model.codigo;
                    return View(model);
                }

                System.Diagnostics.Debug.WriteLine($"✅ Persona creada con código: {model.codigo}");

                // 6. ENVIAR CORREO EN SEGUNDO PLANO
                string nombreUsuarioGenerado = model.username;
                string emailDestino = model.email;
                string cedula = model.documento;

                // Enviar en segundo plano
                Task.Run(() => EnviarCorreoEnSegundoPlano(emailDestino, nombreUsuarioGenerado, cedula));

                TempData["SuccessMessage"] = $"✅ Persona creada con código: {model.codigo}<br/>" +
                                            $"👤 Usuario: {nombreUsuarioGenerado}<br/>" +
                                            $"🔐 Contraseña inicial: {cedula}<br/>" +
                                            $"📧 Se enviará correo a: {emailDestino}";

                return RedirectToAction("CrearPersonal");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"💥 ERROR: {ex.Message}");
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);

                ViewBag.Error = $"Error al crear persona: {ex.Message}";
                CargarListasDesplegables();

                if (string.IsNullOrEmpty(model.codigo))
                    model.codigo = GenerarCodigoPersonaAutomatico();
                ViewBag.IdGenerado = model.codigo;

                return View(model);
            }
        }

        private void CargarListasDesplegables()
        {
            // Obtener sexos desde MongoDB usando CD_Configuracion
            var sexos = CD_Configuracion.Instancia.ObtenerSexos();
            var listaSexos = new List<SelectListItem>();

            if (sexos != null && sexos.Count > 0)
            {
                foreach (var valor in sexos)
                {
                    if (valor.Activo)
                    {
                        listaSexos.Add(new SelectListItem
                        {
                            Value = valor.Codigo,
                            Text = valor.Nombre
                        });
                    }
                }
            }
            else
            {
                listaSexos.Add(new SelectListItem { Value = "M", Text = "Masculino" });
                listaSexos.Add(new SelectListItem { Value = "F", Text = "Femenino" });
            }
            ViewBag.Sexos = listaSexos;

            // Obtener estados civiles desde MongoDB
            var estadosCiviles = CD_Configuracion.Instancia.ObtenerEstadosCiviles();
            var listaEstadosCiviles = new List<SelectListItem>();

            if (estadosCiviles != null && estadosCiviles.Count > 0)
            {
                foreach (var valor in estadosCiviles)
                {
                    if (valor.Activo)
                    {
                        listaEstadosCiviles.Add(new SelectListItem
                        {
                            Value = valor.Codigo,
                            Text = valor.Nombre
                        });
                    }
                }
            }
            else
            {
                listaEstadosCiviles.Add(new SelectListItem { Value = "S", Text = "Soltero/a" });
                listaEstadosCiviles.Add(new SelectListItem { Value = "C", Text = "Casado/a" });
                listaEstadosCiviles.Add(new SelectListItem { Value = "D", Text = "Divorciado/a" });
                listaEstadosCiviles.Add(new SelectListItem { Value = "V", Text = "Viudo/a" });
            }
            ViewBag.EstadosCiviles = listaEstadosCiviles;

            // Cargar tipos de personal
            var tiposPersonal = new List<SelectListItem>
            {
                new SelectListItem { Value = "Administrador del Sistema", Text = "Administrador del Sistema" },
                new SelectListItem { Value = "Docente", Text = "Docente" },
                new SelectListItem { Value = "Estudiante", Text = "Estudiante" },
                new SelectListItem { Value = "Personal Administrativo", Text = "Personal Administrativo" },
                new SelectListItem { Value = "Director", Text = "Director" },
                new SelectListItem { Value = "Coordinador", Text = "Coordinador" }
            };
            ViewBag.TiposPersonal = tiposPersonal;
        }

        private void EnviarCorreoEnSegundoPlano(string email, string nombreUsuario, string cedula)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"📧 Iniciando envío en segundo plano a: {email}");

                if (string.IsNullOrWhiteSpace(email))
                {
                    System.Diagnostics.Debug.WriteLine("❌ Email vacío, no se envía correo");
                    return;
                }

                var emailService = new EmailService();
                bool enviado = emailService.EnviarCredencialesSincrono(email, nombreUsuario, cedula);

                if (enviado)
                {
                    System.Diagnostics.Debug.WriteLine($"✅ Correo enviado exitosamente a: {email}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ No se pudo enviar correo a: {email}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"💥 Error en envío segundo plano: {ex.Message}");
            }
        }

        public ActionResult editarpersonal(string codigo)
        {
            if (string.IsNullOrEmpty(codigo))
            {
                TempData["ErrorMessage"] = "Código de personal requerido.";
                return RedirectToAction("listapersonal");
            }

            var personal = CD_Personal.Instancia.ObtenerPersonaPorCodigo(codigo);
            if (personal == null)
            {
                TempData["ErrorMessage"] = "Personal no encontrado.";
                return RedirectToAction("listapersonal");
            }

            CargarListasDesplegables();
            return View(personal);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult editarpersonal(Personal model, HttpPostedFileBase imagenPersona)
        {
            try
            {
                if (string.IsNullOrEmpty(model.codigo))
                {
                    TempData["ErrorMessage"] = "Código de personal requerido.";
                    return RedirectToAction("listapersonal");
                }

                var personaActual = CD_Personal.Instancia.ObtenerPersonaPorCodigo(model.codigo);
                if (personaActual == null)
                {
                    TempData["ErrorMessage"] = "Personal no encontrado.";
                    return RedirectToAction("listapersonal");
                }

                // Mantener el password_hash si no se está cambiando
                if (string.IsNullOrEmpty(model.password_hash))
                {
                    model.password_hash = personaActual.password_hash;
                }

                // Mantener el rol existente (no cambiar desde aquí)
                model.rol = personaActual.rol;

                // Procesar nueva imagen si se sube
                if (imagenPersona != null && imagenPersona.ContentLength > 0)
                {
                    // Eliminar imagen anterior si existe
                    if (!string.IsNullOrEmpty(personaActual.imagen_perfil))
                    {
                        string rutaImagenAnterior = Path.Combine(_rutaBaseImagenes, personaActual.imagen_perfil);
                        if (System.IO.File.Exists(rutaImagenAnterior))
                        {
                            System.IO.File.Delete(rutaImagenAnterior);
                        }
                    }

                    // Guardar nueva imagen
                    string cedulaLimpia = new string(model.documento.Where(char.IsDigit).ToArray());
                    string extension = Path.GetExtension(imagenPersona.FileName);
                    string nombreArchivo = $"{cedulaLimpia}_{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{extension}";
                    string rutaCompleta = Path.Combine(_rutaBaseImagenes, nombreArchivo);

                    imagenPersona.SaveAs(rutaCompleta);
                    model.imagen_perfil = nombreArchivo;
                }
                else
                {
                    // Mantener la imagen actual
                    model.imagen_perfil = personaActual.imagen_perfil;
                }

                if (!ValidarDatosPersona(model, true))
                {
                    TempData["ErrorMessage"] = "Datos inválidos.";
                    return RedirectToAction("editarpersonal", new { codigo = model.codigo });
                }

                bool resultado = CD_Personal.Instancia.ModificarPersona(model);

                if (resultado)
                {
                    TempData["SuccessMessage"] = "Personal actualizado correctamente.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Error al actualizar el personal.";
                }

                return RedirectToAction("listapersonal");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                return RedirectToAction("listapersonal");
            }
        }

        public ActionResult listapersonal()
        {
            var listaPersonal = CD_Personal.Instancia.ObtenerPersonas();
            if (listaPersonal == null)
            {
                ViewBag.Error = "Error al cargar la lista de personal.";
                return View(new List<Personal>());
            }

            foreach (var personal in listaPersonal)
            {
                if (!string.IsNullOrEmpty(personal.sexo))
                {
                    personal.sexo = CD_Configuracion.Instancia.ObtenerNombrePorCodigo("sexo", personal.sexo);
                }

                if (!string.IsNullOrEmpty(personal.estado_civil))
                {
                    personal.estado_civil = CD_Configuracion.Instancia.ObtenerNombrePorCodigo("estado_civil", personal.estado_civil);
                }
            }

            return View(listaPersonal);
        }

        [HttpPost]
        public JsonResult SubirImagen(HttpPostedFileBase imagenSubida, string cedulaPersona)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== DEBUG: SUBIR IMAGEN ===");

                if (imagenSubida == null || imagenSubida.ContentLength == 0)
                {
                    System.Diagnostics.Debug.WriteLine("❌ ERROR: No hay imagen seleccionada");
                    return Json(new { success = false, message = "No hay imagen seleccionada" });
                }

                string cedula = "temp";
                if (!string.IsNullOrEmpty(cedulaPersona))
                {
                    cedula = new string(cedulaPersona.Where(char.IsDigit).ToArray());
                }
                else
                {
                    cedula = "temp_" + DateTimeOffset.Now.ToUnixTimeMilliseconds();
                }

                string extension = Path.GetExtension(imagenSubida.FileName);
                string[] extensionesPermitidas = { ".jpg", ".jpeg", ".png", ".gif" };

                if (!extensionesPermitidas.Contains(extension.ToLower()))
                {
                    return Json(new { success = false, message = "Solo se permiten imágenes JPG, JPEG, PNG o GIF" });
                }

                if (imagenSubida.ContentLength > 5 * 1024 * 1024)
                {
                    return Json(new { success = false, message = "La imagen no debe superar los 5MB" });
                }

                string nombreArchivoImagen = $"{cedula}_{DateTimeOffset.Now.ToUnixTimeMilliseconds()}{extension}";
                string rutaCompleta = Path.Combine(_rutaBaseImagenes, nombreArchivoImagen);

                Directory.CreateDirectory(_rutaBaseImagenes);
                imagenSubida.SaveAs(rutaCompleta);

                return Json(new
                {
                    success = true,
                    fileName = nombreArchivoImagen,
                    originalName = imagenSubida.FileName
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"💥 ERROR CRÍTICO: {ex.Message}");
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpPost]
        public JsonResult EliminarImagen(string nombreArchivo)
        {
            try
            {
                if (!string.IsNullOrEmpty(nombreArchivo))
                {
                    string rutaCompleta = Path.Combine(_rutaBaseImagenes, nombreArchivo);

                    System.Diagnostics.Debug.WriteLine($"🗑️ Intentando eliminar: {rutaCompleta}");

                    if (System.IO.File.Exists(rutaCompleta))
                    {
                        System.IO.File.Delete(rutaCompleta);
                        System.Diagnostics.Debug.WriteLine($"✅ Imagen eliminada del servidor: {nombreArchivo}");
                    }
                }

                return Json(new { success = true, message = "Imagen eliminada" });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Error al eliminar imagen: {ex.Message}");
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        public string GetUrlImagen(string nombreArchivo)
        {
            if (string.IsNullOrEmpty(nombreArchivo))
            {
                System.Diagnostics.Debug.WriteLine("⚠️ GetUrlImagen: nombreArchivo es null/vacío");
                return null;
            }

            string url = Url.Action("MostrarImagen", "ControladorPersonal", new { nombre = nombreArchivo });

            string rutaFisica = Path.Combine(_rutaBaseImagenes, nombreArchivo);
            bool archivoExiste = System.IO.File.Exists(rutaFisica);
            System.Diagnostics.Debug.WriteLine($"📁 Archivo existe físicamente ({rutaFisica}): {archivoExiste}");

            return archivoExiste ? url : null;
        }

        public ActionResult MostrarImagen(string nombre)
        {
            if (string.IsNullOrEmpty(nombre))
            {
                return HttpNotFound();
            }

            string rutaCompleta = Path.Combine(_rutaBaseImagenes, nombre);

            if (!System.IO.File.Exists(rutaCompleta))
            {
                return HttpNotFound();
            }

            string extension = Path.GetExtension(nombre).ToLower();
            string contentType = "image/jpeg";

            switch (extension)
            {
                case ".png":
                    contentType = "image/png";
                    break;
                case ".gif":
                    contentType = "image/gif";
                    break;
                case ".bmp":
                    contentType = "image/bmp";
                    break;
                case ".jpg":
                case ".jpeg":
                    contentType = "image/jpeg";
                    break;
            }

            return File(rutaCompleta, contentType);
        }

        public ActionResult eliminarpersonal(string codigo)
        {
            if (string.IsNullOrEmpty(codigo))
            {
                TempData["ErrorMessage"] = "Código de personal requerido.";
                return RedirectToAction("listapersonal");
            }

            var personal = CD_Personal.Instancia.ObtenerPersonaPorCodigo(codigo);
            if (personal == null)
            {
                TempData["ErrorMessage"] = "Personal no encontrado.";
                return RedirectToAction("listapersonal");
            }

            return View(personal);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("eliminarpersonal")]
        public ActionResult eliminarpersonalconfirmado(string codigo)
        {
            try
            {
                if (string.IsNullOrEmpty(codigo))
                {
                    TempData["ErrorMessage"] = "Código de personal requerido.";
                    return RedirectToAction("listapersonal");
                }

                var personal = CD_Personal.Instancia.ObtenerPersonaPorCodigo(codigo);
                if (personal == null)
                {
                    TempData["ErrorMessage"] = "Personal no encontrado.";
                    return RedirectToAction("listapersonal");
                }

                // Eliminar imagen si existe
                if (!string.IsNullOrEmpty(personal.imagen_perfil))
                {
                    string rutaImagen = Path.Combine(_rutaBaseImagenes, personal.imagen_perfil);
                    if (System.IO.File.Exists(rutaImagen))
                    {
                        System.IO.File.Delete(rutaImagen);
                    }
                }

                bool resultado = CD_Personal.Instancia.EliminarPersona(personal.id);

                if (resultado)
                {
                    TempData["SuccessMessage"] = "Personal eliminado correctamente.";
                }
                else
                {
                    TempData["ErrorMessage"] = "No se pudo eliminar el personal.";
                }

                return RedirectToAction("listapersonal");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                return RedirectToAction("listapersonal");
            }
        }

        public ActionResult detallespersonal(string codigo)
        {
            if (string.IsNullOrEmpty(codigo))
            {
                TempData["ErrorMessage"] = "Código de personal requerido.";
                return RedirectToAction("listapersonal");
            }

            var personal = CD_Personal.Instancia.ObtenerPersonaPorCodigo(codigo);
            if (personal == null)
            {
                TempData["ErrorMessage"] = "Personal no encontrado.";
                return RedirectToAction("listapersonal");
            }

            if (!string.IsNullOrEmpty(personal.sexo))
            {
                personal.sexo = CD_Configuracion.Instancia.ObtenerNombrePorCodigo("sexo", personal.sexo);
            }

            if (!string.IsNullOrEmpty(personal.estado_civil))
            {
                personal.estado_civil = CD_Configuracion.Instancia.ObtenerNombrePorCodigo("estado_civil", personal.estado_civil);
            }

            return View(personal);
        }

        private string GenerarCodigoPersonaAutomatico()
        {
            try
            {
                var listaPersonal = CD_Personal.Instancia.ObtenerPersonas();
                if (listaPersonal == null || listaPersonal.Count == 0)
                {
                    return "PE001";
                }

                int maxNumero = 0;
                foreach (var persona in listaPersonal)
                {
                    if (persona.codigo != null &&
                        persona.codigo.StartsWith("PE") &&
                        persona.codigo.Length == 5)
                    {
                        try
                        {
                            string numeroStr = persona.codigo.Substring(2);
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

                for (int i = 1; i <= 999; i++)
                {
                    string codigoCandidato = $"PE{i:000}";
                    bool existe = listaPersonal.Any(p => p.codigo == codigoCandidato);
                    if (!existe)
                    {
                        return codigoCandidato;
                    }
                }

                return $"PE{maxNumero + 1:000}";
            }
            catch (Exception)
            {
                return "PE001";
            }
        }

        private string GenerarNombreUsuario(Personal persona)
        {
            if (string.IsNullOrEmpty(persona.nombres) || string.IsNullOrEmpty(persona.apellidos))
            {
                return "user_" + persona.documento;
            }

            string primeraLetraNombre = persona.nombres.Substring(0, 1).ToLower();
            string apellido = persona.apellidos.Split(' ')[0].ToLower();

            apellido = RemoverTildes(apellido);
            apellido = new string(apellido.Where(c => char.IsLetter(c)).ToArray());

            string usernameBase = primeraLetraNombre + apellido;

            int contador = 1;
            string username = usernameBase;
            while (CD_Personal.Instancia.ObtenerPersonaPorUsername(username) != null)
            {
                username = $"{usernameBase}{contador}";
                contador++;
            }

            return username;
        }

        private string GenerarHashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        private string RemoverTildes(string texto)
        {
            if (string.IsNullOrEmpty(texto))
                return texto;

            var caracteres = texto.Normalize(NormalizationForm.FormD)
                .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                .ToArray();

            return new string(caracteres).Normalize(NormalizationForm.FormC);
        }

        private bool ValidarDatosPersona(Personal persona, bool esEdicion = false)
        {
            if (string.IsNullOrEmpty(persona.codigo))
                return false;

            if (string.IsNullOrEmpty(persona.nombres))
                return false;

            if (string.IsNullOrEmpty(persona.apellidos))
                return false;

            if (string.IsNullOrEmpty(persona.documento))
                return false;

            if (string.IsNullOrEmpty(persona.email))
                return false;

            if (string.IsNullOrEmpty(persona.sexo))
                return false;

            if (string.IsNullOrEmpty(persona.peperTipo))
                return false;

            if (!esEdicion && persona.documento.Length < 6)
                return false;

            string idExcluir = esEdicion ? persona.id : null;

            if (!CD_Personal.Instancia.ValidarDocumentoUnico(persona.documento, idExcluir))
                return false;

            if (!CD_Personal.Instancia.ValidarEmailUnico(persona.email, idExcluir))
                return false;

            if (!string.IsNullOrEmpty(persona.username) && !CD_Personal.Instancia.ValidarUsernameUnico(persona.username, idExcluir))
                return false;

            return true;
        }
    }
}