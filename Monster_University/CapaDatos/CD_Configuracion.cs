using CapaModelo;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CapaDatos
{
    public class CD_Configuracion
    {
        private static CD_Configuracion _instancia = null;
        private IMongoCollection<Configuracion> _configuracionesCollection;

        private CD_Configuracion()
        {
            // Usamos la conexión existente de la clase Conexion
            _configuracionesCollection = Conexion.GetCollection<Configuracion>("configuraciones");

            // Opcional: Crear índices al inicializar
            CrearIndices();
        }

        public static CD_Configuracion Instancia
        {
            get
            {
                if (_instancia == null)
                    _instancia = new CD_Configuracion();
                return _instancia;
            }
        }

        /// <summary>
        /// Crea índices para optimizar las consultas
        /// </summary>
        private void CrearIndices()
        {
            try
            {
                // Índice único para el campo Tipo
                var tipoIndexKeys = Builders<Configuracion>.IndexKeys.Ascending(c => c.Tipo);
                var tipoIndexOptions = new CreateIndexOptions { Unique = true };
                var tipoIndexModel = new CreateIndexModel<Configuracion>(tipoIndexKeys, tipoIndexOptions);

                _configuracionesCollection.Indexes.CreateOne(tipoIndexModel);

                System.Diagnostics.Debug.WriteLine("✅ Índices creados para la colección configuraciones");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Error al crear índices: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtiene una configuración por su tipo
        /// </summary>
        public Configuracion ObtenerConfiguracionPorTipo(string tipo)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"🔍 Buscando configuración de tipo: {tipo}");

                var filter = Builders<Configuracion>.Filter.Eq(c => c.Tipo, tipo);
                var configuracion = _configuracionesCollection.Find(filter).FirstOrDefault();

                if (configuracion != null)
                {
                    System.Diagnostics.Debug.WriteLine($"✅ Configuración encontrada: {tipo}");
                    System.Diagnostics.Debug.WriteLine($"📋 Valores encontrados: {configuracion.Valores?.Count ?? 0}");

                    if (configuracion.Valores != null)
                    {
                        foreach (var valor in configuracion.Valores)
                        {
                            System.Diagnostics.Debug.WriteLine($"   - {valor.Codigo}: {valor.Nombre}");
                        }
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ Configuración no encontrada: {tipo}");
                }

                return configuracion;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"💥 Error al obtener configuración {tipo}: {ex.Message}");
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
                return null;
            }
        }

        /// <summary>
        /// Obtiene todas las configuraciones
        /// </summary>
        public List<Configuracion> ObtenerTodasConfiguraciones()
        {
            try
            {
                return _configuracionesCollection.Find(_ => true).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"💥 Error al obtener todas las configuraciones: {ex.Message}");
                return new List<Configuracion>();
            }
        }

        /// <summary>
        /// Inserta o actualiza una configuración
        /// </summary>
        public bool GuardarConfiguracion(Configuracion configuracion)
        {
            try
            {
                if (configuracion == null)
                    return false;

                // Asegurarse que la lista de valores no sea null
                configuracion.Valores = configuracion.Valores ?? new List<Configuracion.ValorConfiguracion>();

                var filter = Builders<Configuracion>.Filter.Eq(c => c.Tipo, configuracion.Tipo);
                var existing = _configuracionesCollection.Find(filter).FirstOrDefault();

                if (existing != null)
                {
                    // Actualizar documento existente
                    var update = Builders<Configuracion>.Update
                        .Set(c => c.Valores, configuracion.Valores);

                    var result = _configuracionesCollection.UpdateOne(filter, update);

                    System.Diagnostics.Debug.WriteLine($"✅ Configuración actualizada: {configuracion.Tipo}");
                    return result.ModifiedCount > 0 || result.MatchedCount > 0;
                }
                else
                {
                    // Insertar nuevo documento
                    _configuracionesCollection.InsertOne(configuracion);
                    System.Diagnostics.Debug.WriteLine($"✅ Nueva configuración creada: {configuracion.Tipo}");
                    return true;
                }
            }
            catch (MongoWriteException ex) 
            {
                System.Diagnostics.Debug.WriteLine($"💥 Error: Ya existe una configuración con el tipo '{configuracion.Tipo}'");
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"💥 Error al guardar configuración: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Elimina una configuración por tipo
        /// </summary>
        public bool EliminarConfiguracion(string tipo)
        {
            try
            {
                var filter = Builders<Configuracion>.Filter.Eq(c => c.Tipo, tipo);
                var result = _configuracionesCollection.DeleteOne(filter);

                System.Diagnostics.Debug.WriteLine($"✅ Configuración eliminada: {tipo} (afectados: {result.DeletedCount})");
                return result.DeletedCount > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"💥 Error al eliminar configuración: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Obtiene solo los valores de una configuración específica como lista simple
        /// </summary>
        public List<Configuracion.ValorConfiguracion> ObtenerValoresConfiguracion(string tipo)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"🔍 Buscando valores para tipo: {tipo}");

                // Pipeline de agregación para obtener solo los valores
                var pipeline = new[]
                {
            new BsonDocument("$match",
                new BsonDocument("tipo", tipo)),
            new BsonDocument("$project",
                new BsonDocument
                {
                    { "valores", 1 },
                    { "_id", 0 }
                })
        };

                var result = _configuracionesCollection
                    .Aggregate<BsonDocument>(pipeline)
                    .FirstOrDefault();

                if (result != null && result.Contains("valores"))
                {
                    var valores = result["valores"].AsBsonArray;
                    var listaValores = new List<Configuracion.ValorConfiguracion>();

                    foreach (var valor in valores)
                    {
                        listaValores.Add(new Configuracion.ValorConfiguracion
                        {
                            Codigo = valor["codigo"].AsString,
                            Nombre = valor["nombre"].AsString
                        });
                    }

                    System.Diagnostics.Debug.WriteLine($"✅ Valores encontrados para {tipo}: {listaValores.Count}");
                    return listaValores;
                }

                System.Diagnostics.Debug.WriteLine($"⚠️ No se encontraron valores para: {tipo}");
                return new List<Configuracion.ValorConfiguracion>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"💥 Error al obtener valores de configuración: {ex.Message}");
                return new List<Configuracion.ValorConfiguracion>();
            }
        }

        /// <summary>
        /// Método específico para obtener sexos (para compatibilidad)
        /// </summary>
        public List<Configuracion.ValorConfiguracion> ObtenerSexos()
        {
            return ObtenerValoresConfiguracion("sexo");
        }

        /// <summary>
        /// Método específico para obtener estados civiles (para compatibilidad)
        /// </summary>
        public List<Configuracion.ValorConfiguracion> ObtenerEstadosCiviles()
        {
            return ObtenerValoresConfiguracion("estado_civil");
        }

        /// <summary>
        /// Obtiene el nombre de un valor por su código
        /// </summary>
        public string ObtenerNombrePorCodigo(string tipo, string codigo)
        {
            try
            {
                var valores = ObtenerValoresConfiguracion(tipo);
                var valor = valores.FirstOrDefault(v => v.Codigo == codigo);
                return valor?.Nombre ?? codigo;
            }
            catch
            {
                return codigo;
            }
        }

        /// <summary>
        /// Agrega un nuevo valor a una configuración existente
        /// </summary>
        public bool AgregarValorConfiguracion(string tipo, Configuracion.ValorConfiguracion nuevoValor)
        {
            try
            {
                if (string.IsNullOrEmpty(tipo) || nuevoValor == null)
                    return false;

                var filter = Builders<Configuracion>.Filter.Eq(c => c.Tipo, tipo);
                var update = Builders<Configuracion>.Update
                    .Push(c => c.Valores, nuevoValor);

                var result = _configuracionesCollection.UpdateOne(filter, update);

                if (result.ModifiedCount > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"✅ Valor agregado a {tipo}: {nuevoValor.Codigo} - {nuevoValor.Nombre}");
                    return true;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ No se pudo agregar el valor a {tipo}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"💥 Error al agregar valor a configuración: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Inicializa configuraciones básicas si no existen
        /// </summary>
        public void InicializarConfiguracionesBasicas()
        {
            try
            {
                var configuracionesBasicas = new List<Configuracion>
                {
                    new Configuracion
                    {
                        Tipo = "sexo",
                        Valores = new List<Configuracion.ValorConfiguracion>
                        {
                            new Configuracion.ValorConfiguracion { Codigo = "M", Nombre = "Masculino" },
                            new Configuracion.ValorConfiguracion { Codigo = "F", Nombre = "Femenino" }
                        }
                    },
                    new Configuracion
                    {
                        Tipo = "estado_civil",
                        Valores = new List<Configuracion.ValorConfiguracion>
                        {
                            new Configuracion.ValorConfiguracion { Codigo = "S", Nombre = "Soltero/a" },
                            new Configuracion.ValorConfiguracion { Codigo = "C", Nombre = "Casado/a" },
                            new Configuracion.ValorConfiguracion { Codigo = "D", Nombre = "Divorciado/a" },
                            new Configuracion.ValorConfiguracion { Codigo = "V", Nombre = "Viudo/a" }
                        }
                    }
                };

                foreach (var config in configuracionesBasicas)
                {
                    if (ObtenerConfiguracionPorTipo(config.Tipo) == null)
                    {
                        GuardarConfiguracion(config);
                    }
                }

                System.Diagnostics.Debug.WriteLine("✅ Configuraciones básicas inicializadas");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"💥 Error al inicializar configuraciones: {ex.Message}");
            }
        }
    }
}