using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using CapaModelo;

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

                var filter = Builders<Configuracion>.Filter.Eq(c => c.Tipo, configuracion.Tipo);
                var existing = _configuracionesCollection.Find(filter).FirstOrDefault();

                if (existing != null)
                {
                    // Actualizar
                    var update = Builders<Configuracion>.Update
                        .Set(c => c.Valores, configuracion.Valores);

                    var result = _configuracionesCollection.UpdateOne(filter, update);
                    return result.ModifiedCount > 0;
                }
                else
                {
                    // Insertar nuevo
                    _configuracionesCollection.InsertOne(configuracion);
                    return true;
                }
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
        public List<ValorConfiguracion> ObtenerValoresConfiguracion(string tipo)
        {
            try
            {
                var configuracion = ObtenerConfiguracionPorTipo(tipo);
                return configuracion?.Valores ?? new List<ValorConfiguracion>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"💥 Error al obtener valores de configuración: {ex.Message}");
                return new List<ValorConfiguracion>();
            }
        }

        /// <summary>
        /// Método específico para obtener sexos (para compatibilidad)
        /// </summary>
        public List<ValorConfiguracion> ObtenerSexos()
        {
            return ObtenerValoresConfiguracion("sexo");
        }

        /// <summary>
        /// Método específico para obtener estados civiles (para compatibilidad)
        /// </summary>
        public List<ValorConfiguracion> ObtenerEstadosCiviles()
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
    }
}