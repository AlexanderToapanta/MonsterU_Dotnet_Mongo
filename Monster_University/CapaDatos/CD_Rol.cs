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
                return roles;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error en ObtenerRoles: " + ex.Message);
                return null;
            }
        }

        public Rol ObtenerDetalleRol(string codigo)
        {
            try
            {
                var filter = Builders<Rol>.Filter.Eq(r => r.codigo, codigo);
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
                var filter = Builders<Rol>.Filter.Eq(r => r.nombre, nombre);
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
                if (string.IsNullOrEmpty(oRol.codigo))
                {
                    oRol.codigo = ObjectId.GenerateNewId().ToString();
                }

                if (string.IsNullOrEmpty(oRol.estado))
                {
                    oRol.estado = "ACTIVO";
                }

                if (oRol.opciones_permitidas == null)
                {
                    oRol.opciones_permitidas = new List<string>();
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
                var filter = Builders<Rol>.Filter.Eq(r => r.codigo, oRol.codigo);
                var update = Builders<Rol>.Update
                    .Set(r => r.nombre, oRol.nombre)
                    .Set(r => r.descripcion, oRol.descripcion)
                    .Set(r => r.opciones_permitidas, oRol.opciones_permitidas)
                    .Set(r => r.estado, oRol.estado);

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
                var personasCollection = Conexion.GetCollection<Persona>("personas");
                var filterPersonas = Builders<Persona>.Filter.Eq("rol.codigo", codigo);

                var countUsuarios = personasCollection.CountDocuments(filterPersonas);

                if (countUsuarios > 0)
                {
                    System.Diagnostics.Debug.WriteLine("No se puede eliminar rol con usuarios asignados.");
                    return false;
                }

                // Eliminar el rol
                var filter = Builders<Rol>.Filter.Eq(r => r.codigo, codigo);
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
                        filter = Builders<Rol>.Filter.Regex(r => r.codigo,
                            new BsonRegularExpression(valor, "i"));
                        break;
                    case "NOMBRE":
                        filter = Builders<Rol>.Filter.Regex(r => r.nombre,
                            new BsonRegularExpression(valor, "i"));
                        break;
                    case "DESCRIPCION":
                        filter = Builders<Rol>.Filter.Regex(r => r.descripcion,
                            new BsonRegularExpression(valor, "i"));
                        break;
                    case "ESTADO":
                        filter = Builders<Rol>.Filter.Eq(r => r.estado, valor);
                        break;
                    default:
                        filter = Builders<Rol>.Filter.Regex(r => r.nombre,
                            new BsonRegularExpression(valor, "i"));
                        break;
                }

                var roles = _rolesCollection.Find(filter).ToList();
                return roles;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error en BuscarRol: " + ex.Message);
                return null;
            }
        }

        public bool ValidarNombreRolUnico(string nombre, string codigoExcluir = null)
        {
            try
            {
                FilterDefinition<Rol> filter;

                if (string.IsNullOrEmpty(codigoExcluir))
                {
                    filter = Builders<Rol>.Filter.Eq(r => r.nombre, nombre);
                }
                else
                {
                    filter = Builders<Rol>.Filter.And(
                        Builders<Rol>.Filter.Eq(r => r.nombre, nombre),
                        Builders<Rol>.Filter.Ne(r => r.codigo, codigoExcluir)
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
                var filter = Builders<Rol>.Filter.Eq(r => r.codigo, codigo);
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

        public bool AgregarOpcionARol(string codigoRol, string codigoOpcion)
        {
            try
            {
                var filter = Builders<Rol>.Filter.Eq(r => r.codigo, codigoRol);
                var update = Builders<Rol>.Update.AddToSet(r => r.opciones_permitidas, codigoOpcion);

                var result = _rolesCollection.UpdateOne(filter, update);
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error en AgregarOpcionARol: " + ex.Message);
                return false;
            }
        }

        public bool RemoverOpcionDeRol(string codigoRol, string codigoOpcion)
        {
            try
            {
                var filter = Builders<Rol>.Filter.Eq(r => r.codigo, codigoRol);
                var update = Builders<Rol>.Update.Pull(r => r.opciones_permitidas, codigoOpcion);

                var result = _rolesCollection.UpdateOne(filter, update);
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error en RemoverOpcionDeRol: " + ex.Message);
                return false;
            }
        }

        public bool TieneOpcion(string codigoRol, string codigoOpcion)
        {
            try
            {
                var filter = Builders<Rol>.Filter.And(
                    Builders<Rol>.Filter.Eq(r => r.codigo, codigoRol),
                    Builders<Rol>.Filter.AnyEq(r => r.opciones_permitidas, codigoOpcion)
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

                var filter = Builders<Rol>.Filter.Eq(r => r.codigo, codigoRol);

                // Proyección para obtener solo las opciones
                var rol = _rolesCollection.Find(filter)
                    .Project<BsonDocument>(Builders<Rol>.Projection
                        .Include(r => r.opciones_permitidas))
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
                return estados;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error en ObtenerEstadosRol: " + ex.Message);
                return null;
            }
        }

        public List<Rol> ObtenerRolesPorEstado(string estado)
        {
            try
            {
                var filter = Builders<Rol>.Filter.Eq(r => r.estado, estado);
                var roles = _rolesCollection.Find(filter).ToList();
                return roles;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error en ObtenerRolesPorEstado: " + ex.Message);
                return null;
            }
        }

        public List<Rol> ObtenerRolesConOpcion(string codigoOpcion)
        {
            try
            {
                var filter = Builders<Rol>.Filter.AnyEq(r => r.opciones_permitidas, codigoOpcion);
                var roles = _rolesCollection.Find(filter).ToList();
                return roles;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error en ObtenerRolesConOpcion: " + ex.Message);
                return null;
            }
        }

        public List<Rol> ObtenerRolesActivos()
        {
            try
            {
                var filter = Builders<Rol>.Filter.Eq(r => r.estado, "ACTIVO");
                var roles = _rolesCollection.Find(filter).ToList();
                return roles;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error en ObtenerRolesActivos: " + ex.Message);
                return null;
            }
        }
    }
}