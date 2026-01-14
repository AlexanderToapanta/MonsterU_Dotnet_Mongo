using CapaDatos;
using CapaModelo;
using Monster_University.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            }
        }

        // ============ MÉTODOS PRINCIPALES ============

        public ActionResult crearpersonal()
        {
            var model = new Personal();
            model.fecha_ingreso = DateTime.Now;
            model.estado = "ACTIVO";
            model.codigo = GenerarCodigoPersonaAutomatico();

            ViewBag.CodigoGenerado = model.codigo;
            CargarListasDesplegables();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CrearPersonal(Personal model, HttpPostedFileBase imagenPersona)
        {
            try
            {
                // Cargar listas desplegables
                CargarListasDesplegables();

                // 1. VALIDAR DATOS BÁSICOS
                if (!ValidarDatosPersonaBasicos(model))
                {
                    ViewBag.Error = "Datos incompletos o inválidos";
                    ViewBag.CodigoGenerado = model.codigo;
                    return View(model);
                }

                // 2. VALIDAR CÉDULA
                if (!ValidarCedulaEcuatoriana(model.documento))
                {
                    ViewBag.Error = "Cédula inválida";
                    ViewBag.CodigoGenerado = model.codigo;
                    return View(model);
                }

                // 3. VALIDAR UNICIDAD
                if (!ValidarUnicidadDatos(model))
                {
                    ViewBag.Error = "La cédula, email o código ya existen en el sistema";
                    ViewBag.CodigoGenerado = model.codigo;
                    return View(model);
                }

                // 4. PROCESAR IMAGEN
                if (imagenPersona != null && imagenPersona.ContentLength > 0)
                {
                    if (!ProcesarImagen(imagenPersona, model))
                    {
                        ViewBag.Error = "Error al procesar la imagen";
                        ViewBag.CodigoGenerado = model.codigo;
                        return View(model);
                    }
                }

                // 5. GENERAR DATOS AUTOMÁTICOS
                PrepararDatosAutomaticos(model);

                // 6. GUARDAR EN BD
                bool guardado = CD_Personal.Instancia.RegistrarPersona(model);
                if (!guardado)
                {
                    ViewBag.Error = "Error al guardar en la base de datos";
                    ViewBag.CodigoGenerado = model.codigo;
                    return View(model);
                }

                // 7. ENVIAR CORREO EN SEGUNDO PLANO
                if (!string.IsNullOrEmpty(model.email))
                {
                    EnviarCredencialesPorCorreo(model);
                }

                TempData["SuccessMessage"] = $"Persona creada exitosamente. Código: {model.codigo}";
                return RedirectToAction("crearpersonal");
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error: {ex.Message}";
                CargarListasDesplegables();
                ViewBag.CodigoGenerado = model.codigo;
                return View(model);
            }
        }

        // ============ MÉTODOS DE VALIDACIÓN ============

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

        private bool ValidarCedulaEcuatoriana(string cedula)
        {
            try
            {
                // Validar longitud
                if (cedula.Length != 10 || !cedula.All(char.IsDigit))
                    return false;

                // Validar provincia (01-24)
                int provincia = int.Parse(cedula.Substring(0, 2));
                if (provincia < 1 || provincia > 24)
                    return false;

                // Validar tercer dígito (0-6)
                int tercerDigito = int.Parse(cedula.Substring(2, 1));
                if (tercerDigito < 0 || tercerDigito > 6)
                    return false;

                // Validar dígito verificador
                int total = 0;
                int[] coeficientes = { 2, 1, 2, 1, 2, 1, 2, 1, 2 };

                for (int i = 0; i < 9; i++)
                {
                    int valor = int.Parse(cedula.Substring(i, 1)) * coeficientes[i];
                    if (valor > 9)
                        valor -= 9;
                    total += valor;
                }

                int residuo = total % 10;
                int digitoVerificador = (residuo == 0) ? 0 : 10 - residuo;
                int verificadorIngresado = int.Parse(cedula.Substring(9, 1));

                return digitoVerificador == verificadorIngresado;
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

            // Validar username único si existe
            if (!string.IsNullOrEmpty(persona.username) &&
                !CD_Personal.Instancia.ValidarUsernameUnico(persona.username, persona.id))
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

        // ============ MÉTODOS DE PROCESAMIENTO ============

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

                // Asignar nombre de archivo al modelo (si tuviera campo para imagen)
                // persona.foto = nombreArchivo;

                return true;
            }
            catch
            {
                return false;
            }
        }

        private void PrepararDatosAutomaticos(Personal persona)
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

            // Generar hash de contraseña si no existe
            if (string.IsNullOrEmpty(persona.password_hash))
                persona.password_hash = GenerarHashSHA256(persona.documento);
        }

        // ============ MÉTODOS AUXILIARES ============

        private void CargarListasDesplegables()
        {
            // Cargar sexos
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

            // Cargar estados civiles
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
                                    maxNumero = numero;
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
                        return codigoCandidato;
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
            string apellido = persona.apellidos.Trim().Replace(" ", "");

            string username = primeraLetra + apellido;
            username = new string(username.Where(c => char.IsLetterOrDigit(c)).ToArray());

            // Verificar si ya existe
            if (!CD_Personal.Instancia.ValidarUsernameUnico(username))
            {
                for (int i = 1; i <= 100; i++)
                {
                    string candidato = username + i;
                    if (CD_Personal.Instancia.ValidarUsernameUnico(candidato))
                        return candidato.ToLower();
                }
                // Si todos están ocupados, usar documento
                return "user_" + persona.documento;
            }

            return username.ToLower();
        }

        private string GenerarHashSHA256(string texto)
        {
            try
            {
                using (var sha256 = System.Security.Cryptography.SHA256.Create())
                {
                    var bytes = Encoding.UTF8.GetBytes(texto);
                    var hash = sha256.ComputeHash(bytes);
                    return BitConverter.ToString(hash).Replace("-", "").ToLower();
                }
            }
            catch
            {
                // Fallback simple
                return Convert.ToBase64String(Encoding.UTF8.GetBytes(texto));
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
                        persona.documento // Contraseña inicial es la cédula
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

        // ============ MÉTODOS PARA OTRAS VISTAS ============

        public ActionResult listapersonal()
        {
            var listaPersonal = CD_Personal.Instancia.ObtenerPersonas();
            if (listaPersonal == null)
                return View(new List<Personal>());

            return View(listaPersonal);
        }

        public ActionResult editarpersonal(string id)
        {
            if (string.IsNullOrEmpty(id))
                return RedirectToAction("listapersonal");

            var personal = CD_Personal.Instancia.ObtenerDetallePersona(id);
            if (personal == null)
                return RedirectToAction("listapersonal");

            CargarListasDesplegables();
            return View(personal);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult editarpersonal(Personal model)
        {
            try
            {
                if (!ValidarDatosPersonaBasicos(model) || !ValidarUnicidadDatos(model))
                {
                    TempData["ErrorMessage"] = "Datos inválidos o duplicados";
                    return RedirectToAction("editarpersonal", new { id = model.id });
                }

                bool resultado = CD_Personal.Instancia.ModificarPersona(model);
                if (resultado)
                    TempData["SuccessMessage"] = "Personal actualizado correctamente";
                else
                    TempData["ErrorMessage"] = "Error al actualizar";

                return RedirectToAction("listapersonal");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                return RedirectToAction("listapersonal");
            }
        }

        public ActionResult detallespersonal(string id)
        {
            if (string.IsNullOrEmpty(id))
                return RedirectToAction("listapersonal");

            var personal = CD_Personal.Instancia.ObtenerDetallePersona(id);
            if (personal == null)
                return RedirectToAction("listapersonal");

            return View(personal);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult eliminarpersonalconfirmado(string id)
        {
            try
            {
                bool resultado = CD_Personal.Instancia.EliminarPersona(id);
                if (resultado)
                    TempData["SuccessMessage"] = "Personal eliminado correctamente";
                else
                    TempData["ErrorMessage"] = "No se pudo eliminar el personal";

                return RedirectToAction("listapersonal");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                return RedirectToAction("listapersonal");
            }
        }
    }
}