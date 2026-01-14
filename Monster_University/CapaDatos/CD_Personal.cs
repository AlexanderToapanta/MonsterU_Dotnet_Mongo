using CapaModelo;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CapaDatos
{
    public class CD_Persona
    {
        public static CD_Persona _instancia = null;
        private IMongoCollection<Personal> _personasCollection;

        private CD_Persona()
        {
            _personasCollection = Conexion.GetCollection<Personal>("personas");
        }

        public static CD_Persona Instancia
        {
            get
            {
                if (_instancia == null)
                {
                    _instancia = new CD_Persona();
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
    }
}