using CapaModelo;
using MongoDB.Driver;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CapaDatos
{
    public class CD_Rol
    {
        public static CD_Rol _instancia = null;
        private IMongoCollection<Rol> _rolesCollection;

        private CD_Rol()
        {
            _rolesCollection = Conexion.GetCollection<Rol>("roles");
        }

        public static CD_Rol Instancia
        {
            get
            {
                if (_instancia == null)
                {
                    _instancia = new CD_Rol();
                }
                return _instancia;
            }
        }

        public List<Rol> ObtenerRoles()
        {
            try
            {
                var filter = Builders<Rol>.Filter.Empty;
                var roles = _rolesCollection.Find(filter).ToList();
                return roles ?? new List<Rol>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error en ObtenerRoles: " + ex.Message);
                return new List<Rol>();
            }
        }

        public Rol ObtenerDetalleRol(string codigo)
        {
            try
            {
                var filter = Builders<Rol>.Filter.Eq(r => r.Codigo, codigo);
                var rol = _rolesCollection.Find(filter).FirstOrDefault();
                return rol;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error en ObtenerDetalleRol: " + ex.Message);
                return null;
            }
        }

        public Rol ObtenerRolPorNombre(string nombre)
        {
            try
            {
                var filter = Builders<Rol>.Filter.Eq(r => r.Nombre, nombre);
                var rol = _rolesCollection.Find(filter).FirstOrDefault();
                return rol;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error en ObtenerRolPorNombre: " + ex.Message);
                return null;
            }
        }

        public bool RegistrarRol(Rol oRol)
        {
            try
            {
                // Asignar valores por defecto si no existen
                if (string.IsNullOrEmpty(oRol.Codigo))
                {
                    oRol.Codigo = ObjectId.GenerateNewId().ToString();
                }

                if (string.IsNullOrEmpty(oRol.Estado))
                {
                    oRol.Estado = "ACTIVO";
                }

                if (oRol.OpcionesPermitidas == null)
                {
                    oRol.OpcionesPermitidas = new List<string>();
                }

                _rolesCollection.InsertOne(oRol);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error en RegistrarRol: " + ex.Message);
                return false;
            }
        }

        public bool ModificarRol(Rol oRol)
        {
            try
            {
                var filter = Builders<Rol>.Filter.Eq(r => r.Codigo, oRol.Codigo);
                var update = Builders<Rol>.Update
                    .Set(r => r.Nombre, oRol.Nombre)
                    .Set(r => r.Descripcion, oRol.Descripcion)
                    .Set(r => r.OpcionesPermitidas, oRol.OpcionesPermitidas)
                    .Set(r => r.Estado, oRol.Estado);

                var result = _rolesCollection.UpdateOne(filter, update);
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error en ModificarRol: " + ex.Message);
                return false;
            }
        }

        public bool EliminarRol(string codigo)
        {
            try
            {
                // Verificar si hay usuarios con este rol
                var personasCollection = Conexion.GetCollection<Personal>("personas");
                var filterPersonas = Builders<Personal>.Filter.Eq("rol.codigo", codigo);

                var countUsuarios = personasCollection.CountDocuments(filterPersonas);

                if (countUsuarios > 0)
                {
                    System.Diagnostics.Debug.WriteLine("No se puede eliminar rol con usuarios asignados.");
                    return false;
                }

                // Eliminar el rol
                var filter = Builders<Rol>.Filter.Eq(r => r.Codigo, codigo);
                var result = _rolesCollection.DeleteOne(filter);
                return result.DeletedCount > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error en EliminarRol: " + ex.Message);
                return false;
            }
        }

        public List<Rol> BuscarRol(string criterio, string valor)
        {
            try
            {
                FilterDefinition<Rol> filter;

                switch (criterio.ToUpper())
                {
                    case "ID":
                    case "CODIGO":
                        filter = Builders<Rol>.Filter.Regex(r => r.Codigo,
                            new BsonRegularExpression(valor, "i"));
                        break;
                    case "NOMBRE":
                        filter = Builders<Rol>.Filter.Regex(r => r.Nombre,
                            new BsonRegularExpression(valor, "i"));
                        break;
                    case "DESCRIPCION":
                        filter = Builders<Rol>.Filter.Regex(r => r.Descripcion,
                            new BsonRegularExpression(valor, "i"));
                        break;
                    case "ESTADO":
                        filter = Builders<Rol>.Filter.Eq(r => r.Estado, valor);
                        break;
                    default:
                        filter = Builders<Rol>.Filter.Regex(r => r.Nombre,
                            new BsonRegularExpression(valor, "i"));
                        break;
                }

                var roles = _rolesCollection.Find(filter).ToList();
                return roles ?? new List<Rol>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error en BuscarRol: " + ex.Message);
                return new List<Rol>();
            }
        }

        public bool ValidarNombreRolUnico(string nombre, string codigoExcluir = null)
        {
            try
            {
                FilterDefinition<Rol> filter;

                if (string.IsNullOrEmpty(codigoExcluir))
                {
                    filter = Builders<Rol>.Filter.Eq(r => r.Nombre, nombre);
                }
                else
                {
                    filter = Builders<Rol>.Filter.And(
                        Builders<Rol>.Filter.Eq(r => r.Nombre, nombre),
                        Builders<Rol>.Filter.Ne(r => r.Codigo, codigoExcluir)
                    );
                }

                var count = _rolesCollection.CountDocuments(filter);
                return count == 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error en ValidarNombreRolUnico: " + ex.Message);
                return false;
            }
        }

        public bool RolExiste(string codigo)
        {
            try
            {
                var filter = Builders<Rol>.Filter.Eq(r => r.Codigo, codigo);
                var count = _rolesCollection.CountDocuments(filter);
                var existe = count > 0;

                System.Diagnostics.Debug.WriteLine($"CD_Rol.RolExiste('{codigo}'): {existe} (count: {count})");
                return existe;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR CD_Rol.RolExiste: {ex.Message}");
                return false;
            }
        }


        public bool TieneOpcion(string codigoRol, string codigoOpcion)
        {
            try
            {
                var filter = Builders<Rol>.Filter.And(
                    Builders<Rol>.Filter.Eq(r => r.Codigo, codigoRol),
                    Builders<Rol>.Filter.AnyEq(r => r.OpcionesPermitidas, codigoOpcion)
                );

                var count = _rolesCollection.CountDocuments(filter);
                return count > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error en TieneOpcion: " + ex.Message);
                return false;
            }
        }

        public List<string> ObtenerOpcionesPorRol(string codigoRol)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== CD_Rol.ObtenerOpcionesPorRol ===");
                System.Diagnostics.Debug.WriteLine($"Consultando opciones para rol: '{codigoRol}'");

                var filter = Builders<Rol>.Filter.Eq(r => r.Codigo, codigoRol);

                // Proyección para obtener solo las opciones
                var rol = _rolesCollection.Find(filter)
                    .Project<BsonDocument>(Builders<Rol>.Projection
                        .Include(r => r.OpcionesPermitidas))
                    .FirstOrDefault();

                if (rol != null && rol.Contains("opciones_permitidas"))
                {
                    var opcionesBson = rol["opciones_permitidas"].AsBsonArray;
                    var opciones = new List<string>();

                    foreach (var opcion in opcionesBson)
                    {
                        opciones.Add(opcion.AsString);
                    }

                    System.Diagnostics.Debug.WriteLine($"Total opciones encontradas para rol '{codigoRol}': {opciones.Count}");

                    foreach (var opcion in opciones)
                    {
                        System.Diagnostics.Debug.WriteLine($"  Opción: '{opcion}'");
                    }

                    return opciones;
                }

                System.Diagnostics.Debug.WriteLine($"No se encontraron opciones para rol '{codigoRol}'");
                return new List<string>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR CD_Rol.ObtenerOpcionesPorRol: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                return new List<string>();
            }
        }

        public List<string> ObtenerEstadosRol()
        {
            try
            {
                var estados = _rolesCollection.Distinct<string>("estado", FilterDefinition<Rol>.Empty).ToList();
                return estados ?? new List<string>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error en ObtenerEstadosRol: " + ex.Message);
                return new List<string>();
            }
        }

        public List<Rol> ObtenerRolesPorEstado(string estado)
        {
            try
            {
                var filter = Builders<Rol>.Filter.Eq(r => r.Estado, estado);
                var roles = _rolesCollection.Find(filter).ToList();
                return roles ?? new List<Rol>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error en ObtenerRolesPorEstado: " + ex.Message);
                return new List<Rol>();
            }
        }

        public List<Rol> ObtenerRolesConOpcion(string codigoOpcion)
        {
            try
            {
                var filter = Builders<Rol>.Filter.AnyEq(r => r.OpcionesPermitidas, codigoOpcion);
                var roles = _rolesCollection.Find(filter).ToList();
                return roles ?? new List<Rol>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error en ObtenerRolesConOpcion: " + ex.Message);
                return new List<Rol>();
            }
        }

        public List<Rol> ObtenerRolesActivos()
        {
            try
            {
                var filter = Builders<Rol>.Filter.Eq(r => r.Estado, "ACTIVO");
                var roles = _rolesCollection.Find(filter).ToList();
                return roles ?? new List<Rol>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error en ObtenerRolesActivos: " + ex.Message);
                return new List<Rol>();
            }
        }
        private void CrearIndices()
        {
            try
            {
                // Índice único para código de rol
                var codigoIndex = Builders<Rol>.IndexKeys.Ascending(r => r.Codigo);
                var codigoIndexOptions = new CreateIndexOptions { Unique = true };
                _rolesCollection.Indexes.CreateOne(
                    new CreateIndexModel<Rol>(codigoIndex, codigoIndexOptions));

                // Índice para estado
                var estadoIndex = Builders<Rol>.IndexKeys.Ascending(r => r.Estado);
                _rolesCollection.Indexes.CreateOne(
                    new CreateIndexModel<Rol>(estadoIndex));

                System.Diagnostics.Debug.WriteLine("✅ Índices creados para la colección roles");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Error al crear índices: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtiene un rol por su código
        /// </summary>
        /// <param name="codigo">Código del rol (ej: "ROL001")</param>
        /// <returns>Objeto Rol o null si no se encuentra</returns>
        public Rol ObtenerRolPorCodigo(string codigo)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"🔍 Buscando rol por código: '{codigo}'");

                if (string.IsNullOrEmpty(codigo))
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ Código de rol vacío");
                    return null;
                }

                // Primero verificar si la colección tiene datos
                var totalRoles = _rolesCollection.CountDocuments(FilterDefinition<Rol>.Empty);
                System.Diagnostics.Debug.WriteLine($"📊 Total de roles en colección: {totalRoles}");

                if (totalRoles == 0)
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ La colección de roles está vacía");
                    return null;
                }

                // Buscar por código exacto
                var filter = Builders<Rol>.Filter.Eq(r => r.Codigo, codigo);
                var rol = _rolesCollection.Find(filter).FirstOrDefault();

                if (rol != null)
                {
                    System.Diagnostics.Debug.WriteLine($"✅ Rol encontrado:");
                    System.Diagnostics.Debug.WriteLine($"   Código: {rol.Codigo}");
                    System.Diagnostics.Debug.WriteLine($"   Nombre: {rol.Nombre}");
                    System.Diagnostics.Debug.WriteLine($"   Descripción: {rol.Descripcion}");
                    System.Diagnostics.Debug.WriteLine($"   Estado: {rol.Estado}");
                    System.Diagnostics.Debug.WriteLine($"   Opciones permitidas: {rol.OpcionesPermitidas?.Count ?? 0}");

                    if (rol.OpcionesPermitidas != null && rol.OpcionesPermitidas.Count > 0)
                    {
                        foreach (var opcion in rol.OpcionesPermitidas)
                        {
                            System.Diagnostics.Debug.WriteLine($"     - {opcion}");
                        }
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Rol no encontrado con código: '{codigo}'");

                    // Para debug: listar todos los roles disponibles
                    var todosRoles = _rolesCollection.Find(_ => true).ToList();
                    System.Diagnostics.Debug.WriteLine($"📋 Roles disponibles ({todosRoles.Count}):");
                    foreach (var r in todosRoles)
                    {
                        System.Diagnostics.Debug.WriteLine($"   - '{r.Codigo}': {r.Nombre} (Estado: {r.Estado})");
                    }
                }

                return rol;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"💥 Error al obtener rol por código '{codigo}': {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                return null;
            }
        }

    }
}