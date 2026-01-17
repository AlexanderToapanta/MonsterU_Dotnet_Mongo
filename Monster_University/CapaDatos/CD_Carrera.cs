using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Bson;
using CapaModelo;

namespace CapaDatos
{
    public class CD_Carrera
    {
        private static CD_Carrera _instancia = null;
        private readonly IMongoCollection<Carrera> _coleccion;

        private CD_Carrera()
        {
            // Usamos la conexión centralizada
            _coleccion = Conexion.GetCollection<Carrera>("carreras");
        }

        public static CD_Carrera Instancia
        {
            get
            {
                if (_instancia == null) _instancia = new CD_Carrera();
                return _instancia;
            }
        }

        public List<Carrera> ObtenerCarreras()
        {
            try
            {
                return _coleccion.Find(_ => true)
                    .SortBy(c => c.codigo)
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener carreras: {ex.Message}");
                return null;
            }
        }

        public Carrera ObtenerCarreraPorId(string id)
        {
            try
            {
                // Buscar por código (que es el identificador lógico)
                return _coleccion.Find(c => c.codigo == id).FirstOrDefault();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener carrera por ID: {ex.Message}");
                return null;
            }
        }

        public bool RegistrarCarrera(Carrera c)
        {
            try
            {
                // Verificar que no exista una carrera con el mismo código
                var existe = _coleccion.Find(carr => carr.codigo == c.codigo).Any();
                if (existe)
                {
                    Console.WriteLine($"Ya existe una carrera con código: {c.codigo}");
                    return false;
                }

                _coleccion.InsertOne(c);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al registrar carrera: {ex.Message}");
                return false;
            }
        }

        public List<Carrera> BuscarCarreras(string nombre, int? creditosMin, int? creditosMax)
        {
            try
            {
                var filtro = Builders<Carrera>.Filter.Empty;

                if (!string.IsNullOrWhiteSpace(nombre))
                {
                    filtro &= Builders<Carrera>.Filter.Regex(c => c.nombre,
                        new MongoDB.Bson.BsonRegularExpression(nombre, "i"));
                }

                if (creditosMin.HasValue)
                {
                    filtro &= Builders<Carrera>.Filter.Gte(c => c.creditosMinimos, creditosMin.Value);
                }

                if (creditosMax.HasValue)
                {
                    filtro &= Builders<Carrera>.Filter.Lte(c => c.creditosMaximos, creditosMax.Value);
                }

                return _coleccion.Find(filtro)
                    .SortBy(c => c.nombre)
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al buscar carreras: {ex.Message}");
                return null;
            }
        }

        public bool ModificarCarrera(Carrera c)
        {
            try
            {
                var filtro = Builders<Carrera>.Filter.Eq(carr => carr.codigo, c.codigo);

                var actualizacion = Builders<Carrera>.Update
                    .Set(carr => carr.nombre, c.nombre)
                    .Set(carr => carr.creditosMaximos, c.creditosMaximos)
                    .Set(carr => carr.creditosMinimos, c.creditosMinimos);

                var resultado = _coleccion.UpdateOne(filtro, actualizacion);

                return resultado.IsAcknowledged && resultado.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al modificar carrera: {ex.Message}");
                return false;
            }
        }

        public bool EliminarCarrera(string id)
        {
            try
            {
                var filtro = Builders<Carrera>.Filter.Eq(c => c.codigo, id);
                var resultado = _coleccion.DeleteOne(filtro);

                return resultado.IsAcknowledged && resultado.DeletedCount > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al eliminar carrera: {ex.Message}");
                return false;
            }
        }

        public bool EliminarMultiple(List<string> ids)
        {
            if (ids == null || ids.Count == 0) return false;

            try
            {
                var filtro = Builders<Carrera>.Filter.In(c => c.codigo, ids);
                var resultado = _coleccion.DeleteMany(filtro);

                return resultado.IsAcknowledged && resultado.DeletedCount > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al eliminar múltiples carreras: {ex.Message}");
                return false;
            }
        }

        public string GenerarNuevoId(bool forzarFormato = true)
        {
            try
            {
                // Obtener todas las carreras ordenadas por código
                var carreras = _coleccion.Find(_ => true)
                    .SortByDescending(c => c.codigo)
                    .ToList();

                int siguienteNumero = 1;

                if (carreras != null && carreras.Count > 0)
                {
                    // Buscar el máximo número en códigos existentes que sigan el formato CARR###
                    foreach (var carrera in carreras)
                    {
                        if (!string.IsNullOrEmpty(carrera.codigo) &&
                            carrera.codigo.Length >= 7 &&
                            carrera.codigo.StartsWith("CARR", StringComparison.OrdinalIgnoreCase))
                        {
                            string parteNumerica = carrera.codigo.Substring(4, 3);
                            if (int.TryParse(parteNumerica, out int num))
                            {
                                siguienteNumero = Math.Max(siguienteNumero, num + 1);
                            }
                        }
                    }
                }

                // Si forzarFormato es true, siempre usar el formato CARR###
                if (forzarFormato || siguienteNumero == 1)
                {
                    return $"CARR{siguienteNumero.ToString().PadLeft(3, '0')}";
                }
                else
                {
                    // Permitir otros formatos si ya existen (solo para compatibilidad)
                    return siguienteNumero.ToString().PadLeft(3, '0');
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al generar nuevo ID: {ex.Message}");
                return "CARR001";
            }
        }
        public bool ValidarFormatoCodigo(string codigo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(codigo))
                    return false;

                // El código debe tener el formato CARR seguido de 3 dígitos
                if (codigo.Length != 7)
                    return false;

                if (!codigo.StartsWith("CARR", StringComparison.OrdinalIgnoreCase))
                    return false;

                string parteNumerica = codigo.Substring(4);
                return int.TryParse(parteNumerica, out _);
            }
            catch
            {
                return false;
            }
        }

        // Método adicional para MongoDB - Buscar por ObjectId interno
        public Carrera ObtenerCarreraPorObjectId(string objectId)
        {
            try
            {
                var filtro = Builders<Carrera>.Filter.Eq("_id", ObjectId.Parse(objectId));
                return _coleccion.Find(filtro).FirstOrDefault();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener carrera por ObjectId: {ex.Message}");
                return null;
            }
        }

        // Método para contar total de carreras
        public long ContarCarreras()
        {
            try
            {
                return _coleccion.CountDocuments(_ => true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al contar carreras: {ex.Message}");
                return 0;
            }
        }

        // Método para verificar existencia de código
        public bool ExisteCodigo(string codigo)
        {
            try
            {
                var filtro = Builders<Carrera>.Filter.Eq(c => c.codigo, codigo);
                return _coleccion.Find(filtro).Any();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al verificar código: {ex.Message}");
                return false;
            }
        }
    }
}