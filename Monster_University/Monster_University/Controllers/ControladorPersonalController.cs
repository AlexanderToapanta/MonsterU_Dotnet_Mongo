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
            model.estado = "ACTIVO";

            // El rol no se asigna aquí, se asigna aparte
            // No establecer el campo rol en el modelo

            CargarListasDesplegables();

            // Generar código automático
            var nuevoCodigo = GenerarCodigoPersonaAutomatico();
            ViewBag.CodigoGenerado = nuevoCodigo;
            ViewBag.IdGenerado = nuevoCodigo;
            model.codigo = nuevoCodigo;

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
                System.Diagnostics.Debug.WriteLine($"Modelo recibido: {model.nombres} {model.apellidos}");
                System.Diagnostics.Debug.WriteLine($"Documento: {model.documento}");

                // Cargar listas desplegables
                CargarListasDesplegables();

                // Verificar que el modelo tenga código
                if (string.IsNullOrEmpty(model.codigo))
                {
                    model.codigo = GenerarCodigoPersonaAutomatico();
                    System.Diagnostics.Debug.WriteLine($"🆔 Código generado automáticamente: {model.codigo}");
                }
                ViewBag.CodigoGenerado = model.codigo;
                ViewBag.IdGenerado = model.codigo;

                // 1. VALIDAR DATOS BÁSICOS
                if (!ValidarDatosPersonaBasicos(model))
                {
                    ViewBag.Error = "Datos incompletos o inválidos";
                    return View(model);
                }

                // 2. VALIDAR UNICIDAD
                if (!ValidarUnicidadDatos(model))
                {
                    ViewBag.Error = "La cédula, email o código ya existen en el sistema";
                    return View(model);
                }

                // 3. PROCESAR IMAGEN SI SE SUBIÓ
                if (imagenPersona != null && imagenPersona.ContentLength > 0)
                {
                    if (!ProcesarImagen(imagenPersona, model))
                    {
                        ViewBag.Error = "Error al procesar la imagen";
                        return View(model);
                    }
                }
                else
                {
                    model.imagen_perfil = null;
                }

                // 4. PREPARAR DATOS PARA GUARDAR
                PrepararDatosParaGuardar(model);

                // 5. VERIFICAR HASH DE CONTRASEÑA
                VerificarYCorregirHash(model);

                // 6. GUARDAR EN BD (sin rol)
                System.Diagnostics.Debug.WriteLine("💾 Guardando persona en BD...");
                System.Diagnostics.Debug.WriteLine($"📋 Datos a guardar:");
                System.Diagnostics.Debug.WriteLine($"   Código: {model.codigo}");
                System.Diagnostics.Debug.WriteLine($"   Nombre: {model.nombres} {model.apellidos}");
                System.Diagnostics.Debug.WriteLine($"   Documento: {model.documento}");
                System.Diagnostics.Debug.WriteLine($"   Email: {model.email}");
                System.Diagnostics.Debug.WriteLine($"   Username: {model.username}");
                System.Diagnostics.Debug.WriteLine($"   Password Hash: {model.password_hash}");
                System.Diagnostics.Debug.WriteLine($"   Tipo: {model.peperTipo}");
                System.Diagnostics.Debug.WriteLine($"   Estado: {model.estado}");
                System.Diagnostics.Debug.WriteLine($"   Rol: {(model.rol == null ? "null" : "tiene valor")}");

                bool guardado = CD_Personal.Instancia.RegistrarPersona(model);

                if (!guardado)
                {
                    ViewBag.Error = "Error al guardar en la base de datos";
                    return View(model);
                }

                System.Diagnostics.Debug.WriteLine($"✅ Persona creada con código: {model.codigo}");

                // 7. ENVIAR CORREO EN SEGUNDO PLANO
                if (!string.IsNullOrEmpty(model.email))
                {
                    Task.Run(() => EnviarCredencialesPorCorreo(model));
                }

                TempData["SuccessMessage"] = $"✅ Persona creada exitosamente.<br/>" +
                                           $"📋 Código: {model.codigo}<br/>" +
                                           $"👤 Usuario: {model.username}<br/>" +
                                           $"🔐 Contraseña inicial: {model.documento}";

                return RedirectToAction("crearpersonal");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"💥 ERROR: {ex.Message}");
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);

                ViewBag.Error = $"Error al crear persona: {ex.Message}";
                CargarListasDesplegables();
                return View(model);
            }
        }

        // Método para preparar datos antes de guardar
        private void PrepararDatosParaGuardar(Personal persona)
        {
            // Asegurar estado activo
            if (string.IsNullOrEmpty(persona.estado))
                persona.estado = "ACTIVO";

            // Asegurar fecha de ingreso
            if (persona.fecha_ingreso == default(DateTime))
                persona.fecha_ingreso = DateTime.Now;

            // Generar username si no existe
            if (string.IsNullOrEmpty(persona.username))
                persona.username = GenerarNombreUsuario(persona);

            // IMPORTANTE: No asignar rol aquí
            // El rol se asigna aparte, no en la creación básica
            // No establecer persona.rol
        }

        // Método para verificar y corregir hash
        private void VerificarYCorregirHash(Personal persona)
        {
            System.Diagnostics.Debug.WriteLine($"🔐 ANTES - Password_hash: {persona.password_hash}");

            // Si el password_hash está vacío o no tiene 64 caracteres, regenerar
            if (string.IsNullOrEmpty(persona.password_hash) || persona.password_hash.Length != 64)
            {
                // La contraseña inicial es la cédula
                string passwordInicial = persona.documento;
                System.Diagnostics.Debug.WriteLine($"🔐 Generando hash para: {passwordInicial}");
                persona.password_hash = GenerarHashSHA256(passwordInicial);
                System.Diagnostics.Debug.WriteLine($"🔐 DESPUÉS - Hash generado: {persona.password_hash}");
                System.Diagnostics.Debug.WriteLine($"🔐 Longitud del hash: {persona.password_hash?.Length} caracteres");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"🔐 Hash ya existe y parece válido (64 caracteres)");
            }
        }

        // MÉTODO DE ENCRIPTACIÓN
        private string GenerarHashSHA256(string texto)
        {
            try
            {
                using (var sha256 = SHA256.Create())
                {
                    var bytes = Encoding.UTF8.GetBytes(texto);
                    var hash = sha256.ComputeHash(bytes);

                    // Formato correcto
                    string hashResultado = BitConverter.ToString(hash).Replace("-", "").ToLower();

                    System.Diagnostics.Debug.WriteLine($"🔐 SHA256 de '{texto}': {hashResultado}");

                    return hashResultado;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"💥 Error en GenerarHashSHA256: {ex.Message}");

                // Fallback
                try
                {
                    var bytes = Encoding.UTF8.GetBytes(texto);
                    var sha256 = SHA256.Create();
                    var hash = sha256.ComputeHash(bytes);
                    return BitConverter.ToString(hash).Replace("-", "").ToLower();
                }
                catch
                {
                    return Convert.ToBase64String(Encoding.UTF8.GetBytes(texto));
                }
            }
        }

        private void CargarListasDesplegables()
        {
            try
            {
                // Obtener sexos desde MongoDB
                var sexos = CD_Configuracion.Instancia.ObtenerSexos();
                var listaSexos = new List<SelectListItem>();

                if (sexos != null && sexos.Count > 0)
                {
                    foreach (var valor in sexos)
                    {
                        // Como no existe la propiedad Activo, mostrar todos los valores
                        listaSexos.Add(new SelectListItem
                        {
                            Value = valor.Codigo,
                            Text = valor.Nombre
                        });
                    }

                    // Ordenar por código alfabéticamente si no hay orden específico
                    listaSexos = listaSexos.OrderBy(x => x.Value).ToList();
                }
                else
                {
                    
                }

               
                ViewBag.Sexos = listaSexos;

                // Obtener estados civiles desde MongoDB
                var estadosCiviles = CD_Configuracion.Instancia.ObtenerEstadosCiviles();
                var listaEstadosCiviles = new List<SelectListItem>();

                if (estadosCiviles != null && estadosCiviles.Count > 0)
                {
                    foreach (var valor in estadosCiviles)
                    {
                        // Como no existe la propiedad Activo, mostrar todos los valores
                        listaEstadosCiviles.Add(new SelectListItem
                        {
                            Value = valor.Codigo,
                            Text = valor.Nombre
                        });
                    }

                    // Ordenar por código alfabéticamente
                    listaEstadosCiviles = listaEstadosCiviles.OrderBy(x => x.Value).ToList();
                }
                else
                {
                    
                }

              
                ViewBag.EstadosCiviles = listaEstadosCiviles;

                // Cargar tipos de personal
                var tiposPersonal = new List<SelectListItem>
        {
            
            new SelectListItem { Value = "Administrador del Sistema", Text = "Administrador del Sistema" },
            new SelectListItem { Value = "Docente", Text = "Docente" },
            new SelectListItem { Value = "Administrador de Matriculas", Text = "Administrador de Matriculas" },
            new SelectListItem { Value = "Secretaría Académica", Text = "Secretaría Académica" }
        };
                ViewBag.TiposPersonal = tiposPersonal;

                System.Diagnostics.Debug.WriteLine("✅ Listas desplegables cargadas correctamente");
                System.Diagnostics.Debug.WriteLine($"   - Sexos: {listaSexos.Count - 1} opciones");
                System.Diagnostics.Debug.WriteLine($"   - Estados Civiles: {listaEstadosCiviles.Count - 1} opciones");
                System.Diagnostics.Debug.WriteLine($"   - Tipos Personal: {tiposPersonal.Count - 1} opciones");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"💥 Error al cargar listas desplegables: {ex.Message}");

                // Cargar valores por defecto en caso de error
              
        {
            
            
        };

                
        {
           
           
        };

                ViewBag.TiposPersonal = new List<SelectListItem>
        {

           
            new SelectListItem { Value = "Administrador del Sistema", Text = "Administrador del Sistema" },
            new SelectListItem { Value = "Docente", Text = "Docente" },
            new SelectListItem { Value = "Administrador de Matriculas", Text = "Administrador de Matriculas" },
            new SelectListItem { Value = "Secretaría Académica", Text = "Secretaría Académica" }
        };
            }
        }

        private bool ValidarDatosPersonaBasicos(Personal persona)
        {
            if (string.IsNullOrEmpty(persona.nombres) || persona.nombres.Trim().Length < 2)
                return false;

            if (string.IsNullOrEmpty(persona.apellidos) || persona.apellidos.Trim().Length < 2)
                return false;

            if (string.IsNullOrEmpty(persona.documento) || persona.documento.Trim().Length != 10)
                return false;

            if (string.IsNullOrEmpty(persona.email) || !EsEmailValido(persona.email))
                return false;

            if (string.IsNullOrEmpty(persona.peperTipo))
                return false;

            if (string.IsNullOrEmpty(persona.sexo))
                return false;

            return true;
        }

        private bool EsEmailValido(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private bool ValidarUnicidadDatos(Personal persona)
        {
            // Validar documento único
            if (!CD_Personal.Instancia.ValidarDocumentoUnico(persona.documento, persona.id))
                return false;

            // Validar email único
            if (!CD_Personal.Instancia.ValidarEmailUnico(persona.email, persona.id))
                return false;

            // Validar código único
            if (!CD_Personal.Instancia.ValidarCodigoUnico(persona.codigo, persona.id))
                return false;

            return true;
        }

        private bool ProcesarImagen(HttpPostedFileBase imagen, Personal persona)
        {
            try
            {
                // Validar tipo de archivo
                string extension = Path.GetExtension(imagen.FileName).ToLower();
                string[] extensionesPermitidas = { ".jpg", ".jpeg", ".png", ".gif" };

                if (!extensionesPermitidas.Contains(extension))
                    return false;

                // Validar tamaño (máximo 5MB)
                if (imagen.ContentLength > 5 * 1024 * 1024)
                    return false;

                // Generar nombre único
                string documentoLimpio = new string(persona.documento.Where(char.IsDigit).ToArray());
                string nombreArchivo = $"{documentoLimpio}_{DateTime.Now.Ticks}{extension}";
                string rutaCompleta = Path.Combine(_rutaBaseImagenes, nombreArchivo);

                // Guardar archivo
                imagen.SaveAs(rutaCompleta);
                persona.imagen_perfil = nombreArchivo;

                return true;
            }
            catch
            {
                return false;
            }
        }

        private void EnviarCredencialesPorCorreo(Personal persona)
        {
            Task.Run(() =>
            {
                try
                {
                    var emailService = new EmailService();
                    bool enviado = emailService.EnviarCredencialesSincrono(
                        persona.email,
                        persona.username,
                        persona.documento
                    );

                    if (enviado)
                    {
                        System.Diagnostics.Debug.WriteLine($"✅ Correo enviado a: {persona.email}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"⚠️ No se pudo enviar correo a: {persona.email}");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"💥 Error enviando correo: {ex.Message}");
                }
            });
        }

        private string GenerarCodigoPersonaAutomatico()
        {
            try
            {
                var personas = CD_Personal.Instancia.ObtenerPersonas();
                if (personas == null || personas.Count == 0)
                    return "PE001";

                int maxNumero = 0;
                foreach (var persona in personas)
                {
                    if (!string.IsNullOrEmpty(persona.codigo) &&
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

                // Buscar huecos disponibles
                for (int i = 1; i <= 999; i++)
                {
                    string codigoCandidato = $"PE{i:000}";
                    bool existe = personas.Any(p => p.codigo == codigoCandidato);
                    if (!existe)
                    {
                        return codigoCandidato;
                    }
                }

                return $"PE{maxNumero + 1:000}";
            }
            catch
            {
                return "PE001";
            }
        }

        private string GenerarNombreUsuario(Personal persona)
        {
            if (string.IsNullOrEmpty(persona.nombres) || string.IsNullOrEmpty(persona.apellidos))
                return "user_" + persona.documento;

            string primeraLetra = persona.nombres.Trim().Substring(0, 1).ToUpper();
            string apellido = persona.apellidos.Trim().Split(' ')[0];

            // Limpiar caracteres especiales
            apellido = RemoverTildes(apellido);
            apellido = new string(apellido.Where(c => char.IsLetter(c)).ToArray());

            string usernameBase = primeraLetra + apellido;

            // Verificar si ya existe
            int contador = 1;
            string username = usernameBase;
            while (CD_Personal.Instancia.ObtenerPersonaPorUsername(username) != null)
            {
                username = $"{usernameBase}{contador}";
                contador++;
                if (contador > 100)
                {
                    return "user_" + persona.documento;
                }
            }

            return username.ToLower();
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

        // ============ MÉTODOS PARA OTRAS VISTAS ============

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

                // Validar unicidad excluyendo al usuario actual
                if (!CD_Personal.Instancia.ValidarDocumentoUnico(model.documento, model.id) ||
                    !CD_Personal.Instancia.ValidarEmailUnico(model.email, model.id))
                {
                    TempData["ErrorMessage"] = "La cédula o email ya están registrados.";
                    return RedirectToAction("editarpersonal", new { codigo = model.codigo });
                }

                // Mantener el password_hash si no se está cambiando
                if (string.IsNullOrEmpty(model.password_hash))
                {
                    model.password_hash = personaActual.password_hash;
                }
                else if (model.password_hash.Length != 64)
                {
                    // Si se proporciona nueva contraseña, generar su hash
                    model.password_hash = GenerarHashSHA256(model.password_hash);
                }

                

                // Procesar nueva imagen
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

                    if (!ProcesarImagen(imagenPersona, model))
                    {
                        TempData["ErrorMessage"] = "Error al procesar la imagen";
                        return RedirectToAction("editarpersonal", new { codigo = model.codigo });
                    }
                }
                else
                {
                    // Mantener la imagen actual
                    model.imagen_perfil = personaActual.imagen_perfil;
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

        // Método de verificación de contraseña para login
        public bool VerificarContrasena(string password, string storedHash)
        {
            string hashIngresado = GenerarHashSHA256(password);
            bool coincide = string.Equals(hashIngresado, storedHash, StringComparison.OrdinalIgnoreCase);

            System.Diagnostics.Debug.WriteLine($"🔐 Verificando contraseña:");
            System.Diagnostics.Debug.WriteLine($"   Hash ingresado: {hashIngresado}");
            System.Diagnostics.Debug.WriteLine($"   Hash almacenado: {storedHash}");
            System.Diagnostics.Debug.WriteLine($"   ¿Coinciden?: {coincide}");

            return coincide;
        }

        // Método para resetear contraseña
        public ActionResult ResetearContrasena(string codigo)
        {
            if (string.IsNullOrEmpty(codigo))
            {
                TempData["ErrorMessage"] = "Código requerido.";
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
        public ActionResult ResetearContrasena(string codigo, string nuevaContrasena)
        {
            try
            {
                var personal = CD_Personal.Instancia.ObtenerPersonaPorCodigo(codigo);
                if (personal == null)
                {
                    TempData["ErrorMessage"] = "Personal no encontrado.";
                    return RedirectToAction("listapersonal");
                }

                // Generar hash de la nueva contraseña
                personal.password_hash = GenerarHashSHA256(nuevaContrasena);

                bool resultado = CD_Personal.Instancia.ModificarPersona(personal);

                if (resultado)
                {
                    TempData["SuccessMessage"] = "Contraseña reseteada correctamente.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Error al resetear la contraseña.";
                }

                return RedirectToAction("listapersonal");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                return RedirectToAction("listapersonal");
            }
        }

        // Método de validación actualizado
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