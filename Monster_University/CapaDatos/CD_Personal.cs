using CapaModelo;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace CapaDatos
{
    public class CD_Personal
    {
        public static CD_Personal _instancia = null;
        private IMongoCollection<Personal> _personasCollection;
        private IMongoCollection<BsonDocument> _coleccionBson;

        private CD_Personal()
        {
            _personasCollection = Conexion.GetCollection<Personal>("personas");
            _coleccionBson = Conexion.GetCollection<BsonDocument>("personas");
        }

        public static CD_Personal Instancia
        {
            get
            {
                if (_instancia == null)
                {
                    _instancia = new CD_Personal();
                }
                return _instancia;
            }
        }

        public List<Personal> ObtenerPersonas()
        {
            try
            {
                var filter = Builders<Personal>.Filter.Empty;
                var personas = _personasCollection.Find(filter).ToList();
                return personas;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error en ObtenerPersonas: " + ex.Message);
                return null;
            }
        }

        public Personal ObtenerDetallePersona(string id)
        {
            try
            {
                var filter = Builders<Personal>.Filter.Eq(p => p.id, id);
                var persona = _personasCollection.Find(filter).FirstOrDefault();
                return persona;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error en ObtenerDetallePersona: " + ex.Message);
                return null;
            }
        }

        public Personal ObtenerPersonaPorCodigo(string codigo)
        {
            try
            {
                var filter = Builders<Personal>.Filter.Eq(p => p.codigo, codigo);
                var persona = _personasCollection.Find(filter).FirstOrDefault();
                return persona;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error en ObtenerPersonaPorCodigo: " + ex.Message);
                return null;
            }
        }

        public Personal ObtenerPersonaPorDocumento(string documento)
        {
            try
            {
                var filter = Builders<Personal>.Filter.Eq(p => p.documento, documento);
                var persona = _personasCollection.Find(filter).FirstOrDefault();
                return persona;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error en ObtenerPersonaPorDocumento: " + ex.Message);
                return null;
            }
        }

        public Personal ObtenerPersonaPorUsername(string username)
        {
            try
            {
                var filter = Builders<Personal>.Filter.Eq(p => p.username, username);
                var persona = _personasCollection.Find(filter).FirstOrDefault();
                return persona;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error en ObtenerPersonaPorUsername: " + ex.Message);
                return null;
            }
        }

        public Personal ObtenerPersonaPorEmail(string email)
        {
            try
            {
                var filter = Builders<Personal>.Filter.Eq(p => p.email, email);
                var persona = _personasCollection.Find(filter).FirstOrDefault();
                return persona;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error en ObtenerPersonaPorEmail: " + ex.Message);
                return null;
            }
        }

        public bool RegistrarPersona(Personal oPersona)
        {
            try
            {
                // Asignar valores por defecto si no existen
                if (string.IsNullOrEmpty(oPersona.id))
                {
                    oPersona.id = MongoDB.Bson.ObjectId.GenerateNewId().ToString();
                }

                if (string.IsNullOrEmpty(oPersona.estado))
                {
                    oPersona.estado = "ACTIVO";
                }

                if (oPersona.fecha_ingreso == default(DateTime))
                {
                    oPersona.fecha_ingreso = DateTime.Now;
                }
               
        oPersona.rol = null;

                _personasCollection.InsertOne(oPersona);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error en RegistrarPersona: " + ex.Message);
                return false;
            }
        }

        public bool ModificarPersona(Personal oPersona)
        {
            try
            {
                var filter = Builders<Personal>.Filter.Eq(p => p.id, oPersona.id);
                var update = Builders<Personal>.Update
                    .Set(p => p.codigo, oPersona.codigo)
                    .Set(p => p.peperTipo, oPersona.peperTipo)
                    .Set(p => p.documento, oPersona.documento)
                    .Set(p => p.nombres, oPersona.nombres)
                    .Set(p => p.apellidos, oPersona.apellidos)
                    .Set(p => p.email, oPersona.email)
                    .Set(p => p.celular, oPersona.celular)
                    .Set(p => p.fecha_nacimiento, oPersona.fecha_nacimiento)
                    .Set(p => p.sexo, oPersona.sexo)
                    .Set(p => p.estado_civil, oPersona.estado_civil)
                    .Set(p => p.username, oPersona.username)
                    .Set(p => p.password_hash, oPersona.password_hash)
                    .Set(p => p.rol, oPersona.rol)
                    .Set(p => p.fecha_ingreso, oPersona.fecha_ingreso)
                    .Set(p => p.estado, oPersona.estado);

                var result = _personasCollection.UpdateOne(filter, update);
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error en ModificarPersona: " + ex.Message);
                return false;
            }
        }

        public bool EliminarPersona(string id)
        {
            try
            {
                var filter = Builders<Personal>.Filter.Eq(p => p.id, id);
                var result = _personasCollection.DeleteOne(filter);
                return result.DeletedCount > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error en EliminarPersona: " + ex.Message);
                return false;
            }
        }

        public List<Personal> BuscarPersona(string criterio, string valor)
        {
            try
            {
                FilterDefinition<Personal> filter;

                switch (criterio.ToUpper())
                {
                    case "CEDULA":
                        filter = Builders<Personal>.Filter.Regex(p => p.documento, new MongoDB.Bson.BsonRegularExpression(valor, "i"));
                        break;
                    case "NOMBRE":
                        filter = Builders<Personal>.Filter.Or(
                            Builders<Personal>.Filter.Regex(p => p.nombres, new MongoDB.Bson.BsonRegularExpression(valor, "i")),
                            Builders<Personal>.Filter.Regex(p => p.apellidos, new MongoDB.Bson.BsonRegularExpression(valor, "i"))
                        );
                        break;
                    case "EMAIL":
                        filter = Builders<Personal>.Filter.Regex(p => p.email, new MongoDB.Bson.BsonRegularExpression(valor, "i"));
                        break;
                    case "TIPO":
                        filter = Builders<Personal>.Filter.Eq(p => p.peperTipo, valor);
                        break;
                    case "SEXO":
                        filter = Builders<Personal>.Filter.Eq(p => p.sexo, valor);
                        break;
                    case "ESTADO CIVIL":
                        filter = Builders<Personal>.Filter.Eq(p => p.estado_civil, valor);
                        break;
                    case "ESTADO":
                        filter = Builders<Personal>.Filter.Eq(p => p.estado, valor);
                        break;
                    case "CODIGO":
                        filter = Builders<Personal>.Filter.Eq(p => p.codigo, valor);
                        break;
                    default:
                        filter = Builders<Personal>.Filter.Regex(p => p.documento, new MongoDB.Bson.BsonRegularExpression(valor, "i"));
                        break;
                }

                var personas = _personasCollection.Find(filter).ToList();
                return personas;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error en BuscarPersona: " + ex.Message);
                return null;
            }
        }

        public bool ValidarDocumentoUnico(string documento, string idExcluir = null)
        {
            try
            {
                FilterDefinition<Personal> filter;

                if (string.IsNullOrEmpty(idExcluir))
                {
                    filter = Builders<Personal>.Filter.Eq(p => p.documento, documento);
                }
                else
                {
                    filter = Builders<Personal>.Filter.And(
                        Builders<Personal>.Filter.Eq(p => p.documento, documento),
                        Builders<Personal>.Filter.Ne(p => p.id, idExcluir)
                    );
                }

                var count = _personasCollection.CountDocuments(filter);
                return count == 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error en ValidarDocumentoUnico: " + ex.Message);
                return false;
            }
        }

        public bool ValidarEmailUnico(string email, string idExcluir = null)
        {
            try
            {
                FilterDefinition<Personal> filter;

                if (string.IsNullOrEmpty(idExcluir))
                {
                    filter = Builders<Personal>.Filter.Eq(p => p.email, email);
                }
                else
                {
                    filter = Builders<Personal>.Filter.And(
                        Builders<Personal>.Filter.Eq(p => p.email, email),
                        Builders<Personal>.Filter.Ne(p => p.id, idExcluir)
                    );
                }

                var count = _personasCollection.CountDocuments(filter);
                return count == 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error en ValidarEmailUnico: " + ex.Message);
                return false;
            }
        }

        public bool ValidarUsernameUnico(string username, string idExcluir = null)
        {
            try
            {
                FilterDefinition<Personal> filter;

                if (string.IsNullOrEmpty(idExcluir))
                {
                    filter = Builders<Personal>.Filter.Eq(p => p.username, username);
                }
                else
                {
                    filter = Builders<Personal>.Filter.And(
                        Builders<Personal>.Filter.Eq(p => p.username, username),
                        Builders<Personal>.Filter.Ne(p => p.id, idExcluir)
                    );
                }

                var count = _personasCollection.CountDocuments(filter);
                return count == 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error en ValidarUsernameUnico: " + ex.Message);
                return false;
            }
        }

        public bool ValidarCodigoUnico(string codigo, string idExcluir = null)
        {
            try
            {
                FilterDefinition<Personal> filter;

                if (string.IsNullOrEmpty(idExcluir))
                {
                    filter = Builders<Personal>.Filter.Eq(p => p.codigo, codigo);
                }
                else
                {
                    filter = Builders<Personal>.Filter.And(
                        Builders<Personal>.Filter.Eq(p => p.codigo, codigo),
                        Builders<Personal>.Filter.Ne(p => p.id, idExcluir)
                    );
                }

                var count = _personasCollection.CountDocuments(filter);
                return count == 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error en ValidarCodigoUnico: " + ex.Message);
                return false;
            }
        }

        public List<string> ObtenerTiposPersona()
        {
            try
            {
                var tipos = _personasCollection.Distinct<string>("peperTipo", FilterDefinition<Personal>.Empty).ToList();
                return tipos;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error en ObtenerTiposPersona: " + ex.Message);
                return null;
            }
        }

        public List<string> ObtenerEstados()
        {
            try
            {
                var estados = _personasCollection.Distinct<string>("estado", FilterDefinition<Personal>.Empty).ToList();
                return estados;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error en ObtenerEstados: " + ex.Message);
                return null;
            }
        }

        public List<string> ObtenerSexos()
        {
            try
            {
                var sexos = _personasCollection.Distinct<string>("sexo", FilterDefinition<Personal>.Empty).ToList();
                return sexos;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error en ObtenerSexos: " + ex.Message);
                return null;
            }
        }

        public List<string> ObtenerEstadosCiviles()
        {
            try
            {
                var estadosCiviles = _personasCollection.Distinct<string>("estado_civil", FilterDefinition<Personal>.Empty).ToList();
                return estadosCiviles;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error en ObtenerEstadosCiviles: " + ex.Message);
                return null;
            }
        }

        public List<Personal> ObtenerPersonasPorEstado(string estado)
        {
            try
            {
                var filter = Builders<Personal>.Filter.Eq(p => p.estado, estado);
                var personas = _personasCollection.Find(filter).ToList();
                return personas;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error en ObtenerPersonasPorEstado: " + ex.Message);
                return null;
            }
        }

        public List<Personal> ObtenerPersonasPorTipo(string tipo)
        {
            try
            {
                var filter = Builders<Personal>.Filter.Eq(p => p.peperTipo, tipo);
                var personas = _personasCollection.Find(filter).ToList();
                return personas;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error en ObtenerPersonasPorTipo: " + ex.Message);
                return null;
            }
        }
        public int LoginPersonal(string username, string password)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"🔍 Verificando login para: {username}");

                // Calcular hash SHA256 de la contraseña
                var passwordHash = CalcularSHA256(password);

                var filter = Builders<Personal>.Filter.And(
                    Builders<Personal>.Filter.Eq(p => p.username, username),
                    Builders<Personal>.Filter.Eq(p => p.password_hash, passwordHash),
                    Builders<Personal>.Filter.Eq(p => p.estado, "ACTIVO")
                );

                var personal = _personasCollection.Find(filter).FirstOrDefault();

                if (personal != null)
                {
                    System.Diagnostics.Debug.WriteLine($"✅ Login exitoso para: {username}");
                    return 1; // Éxito
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Login fallido para: {username}");
                    return 0; // Fallo
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"💥 Error en LoginPersonal: {ex.Message}");
                return -1; // Error
            }
        }
   

        public Personal ObtenerPersonalPorCodigo(string codigo)
        {
            try
            {
                var filter = Builders<Personal>.Filter.Eq(p => p.codigo, codigo);
                return _personasCollection.Find(filter).FirstOrDefault();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"💥 Error al obtener personal por código: {ex.Message}");
                return null;
            }
        }

        private string CalcularSHA256(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(input);
                byte[] hash = sha256.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }
        public Personal ObtenerPersonalPorUsername(string username)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"🔍 Buscando personal por username: {username}");

                var filter = Builders<Personal>.Filter.Eq(p => p.username, username);
                var personal = _personasCollection.Find(filter).FirstOrDefault();

                if (personal != null)
                {
                    // Convertir rol si es necesario
                    personal = ConvertirRolEnPersonal(personal);
                }

                return personal;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"💥 Error al obtener personal por username: {ex.Message}");
                return null;
            }
        }

        private Personal ConvertirRolEnPersonal(Personal personal)
        {
            try
            {
                if (personal.rol == null) return personal;

                // Si el rol es BsonDocument
                if (personal.rol is BsonDocument bsonDoc)
                {
                    var rol = new Rol
                    {
                        Codigo = bsonDoc.GetValue("codigo", "").AsString,
                        Nombre = bsonDoc.GetValue("nombre", "").AsString,
                        Descripcion = bsonDoc.GetValue("descripcion", "").AsString,
                        Estado = bsonDoc.GetValue("estado", "ACTIVO").AsString
                    };

                    // Convertir opciones_permitidas
                    if (bsonDoc.Contains("opciones_permitidas"))
                    {
                        var opcionesArray = bsonDoc["opciones_permitidas"].AsBsonArray;
                        rol.OpcionesPermitidas = opcionesArray.Select(o => o.AsString).ToList();
                    }

                    personal.rol = rol;
                    System.Diagnostics.Debug.WriteLine($"✅ Rol convertido de BsonDocument: {rol.Codigo}");
                }
                // Si el rol es ExpandoObject
                else if (personal.rol.GetType().Name.Contains("ExpandoObject"))
                {
                    try
                    {
                        dynamic expando = personal.rol;
                        var rol = new Rol
                        {
                            Codigo = expando.codigo ?? "",
                            Nombre = expando.nombre ?? "",
                            Descripcion = expando.descripcion ?? "",
                            Estado = expando.estado ?? "ACTIVO"
                        };

                        // Intentar obtener opciones_permitidas
                        try
                        {
                            if (expando.opciones_permitidas is List<object> opcionesList)
                            {
                                rol.OpcionesPermitidas = opcionesList.Select(o => o?.ToString()).ToList();
                            }
                        }
                        catch { }

                        personal.rol = rol;
                        System.Diagnostics.Debug.WriteLine($"✅ Rol convertido de ExpandoObject: {rol.Codigo}");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"⚠️ Error al convertir ExpandoObject: {ex.Message}");
                    }
                }

                return personal;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"💥 Error en ConvertirRolEnPersonal: {ex.Message}");
                return personal;
            }
        }
        public List<Personal> ObtenerPersonal()
        {
            try
            {
                return _personasCollection.Find(_ => true).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener personal: {ex.Message}");
                return new List<Personal>();
            }
        }

        // Método para obtener una persona por ID
        public Personal ObtenerPersonalPorId(string id)
        {
            try
            {
                var filter = Builders<Personal>.Filter.Eq("_id", ObjectId.Parse(id));
                return _personasCollection.Find(filter).FirstOrDefault();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener personal por ID: {ex.Message}");
                return null;
            }
        }

        // Método para actualizar el rol de una persona
        public bool ActualizarRolPersona(string personaId, object rol)
        {
            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("_id", ObjectId.Parse(personaId));
                var update = Builders<BsonDocument>.Update.Set("rol", rol);

                var result = _coleccionBson.UpdateOne(filter, update);
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al actualizar rol de persona: {ex.Message}");
                return false;
            }
        }

        // Método para buscar personas por criterio
        public List<Personal> BuscarPersonal(string criterio, string valor)
        {
            try
            {
                FilterDefinition<Personal> filter;

                switch (criterio.ToLower())
                {
                    case "codigo":
                        filter = Builders<Personal>.Filter.Regex("codigo", new BsonRegularExpression(valor, "i"));
                        break;
                    case "nombres":
                        filter = Builders<Personal>.Filter.Regex("nombres", new BsonRegularExpression(valor, "i"));
                        break;
                    case "apellidos":
                        filter = Builders<Personal>.Filter.Regex("apellidos", new BsonRegularExpression(valor, "i"));
                        break;
                    case "email":
                        filter = Builders<Personal>.Filter.Regex("email", new BsonRegularExpression(valor, "i"));
                        break;
                    case "estado":
                        filter = Builders<Personal>.Filter.Eq("estado", valor);
                        break;
                    default:
                        return new List<Personal>();
                }

                return _personasCollection.Find(filter).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al buscar personal: {ex.Message}");
                return new List<Personal>();
            }
        }

        // Método para obtener estadísticas de roles
        public Dictionary<string, int> ObtenerEstadisticasRoles()
        {
            try
            {
                var estadisticas = new Dictionary<string, int>
                {
                    { "total_personas", 0 },
                    { "con_rol", 0 },
                    { "sin_rol", 0 }
                };

                var personas = ObtenerPersonal();
                estadisticas["total_personas"] = personas.Count;

                foreach (var persona in personas)
                {
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
                        estadisticas["con_rol"]++;
                    }
                    else
                    {
                        estadisticas["sin_rol"]++;
                    }
                }

                return estadisticas;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener estadísticas: {ex.Message}");
                return new Dictionary<string, int>();
            }
        }
    }

}