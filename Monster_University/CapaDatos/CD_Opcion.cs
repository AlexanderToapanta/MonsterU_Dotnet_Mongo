using CapaModelo;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaDatos
{
    public class CD_Opcion
    {
        public static CD_Opcion _instancia = null;

        private CD_Opcion()
        {
        }

        public static CD_Opcion Instancia
        {
            get
            {
                if (_instancia == null)
                {
                    _instancia = new CD_Opcion();
                }
                return _instancia;
            }
        }

        public List<Opcion> ObtenerOpciones()
        {
            List<Opcion> rptListaOpciones = new List<Opcion>();
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                string query = @"SELECT XEOPC_ID, XEOPC_NOMBRE 
                                   FROM XEOPC_OPCION 
                                   ORDER BY XEOPC_ID";

                SqlCommand cmd = new SqlCommand(query, oConexion);

                try
                {
                    oConexion.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        rptListaOpciones.Add(new Opcion()
                        {
                            XEOPC_ID = dr["XEOPC_ID"]?.ToString(),
                            XEOPC_NOMBRE = dr["XEOPC_NOMBRE"]?.ToString()
                        });
                    }
                    dr.Close();
                    return rptListaOpciones;
                }
                catch (Exception ex)
                {
                    rptListaOpciones = null;
                    System.Diagnostics.Debug.WriteLine("Error en ObtenerOpciones: " + ex.Message);
                    return rptListaOpciones;
                }
            }
        }

        public Opcion ObtenerDetalleOpcion(string XEOPC_ID)
        {
            Opcion rptOpcion = new Opcion();
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                string query = @"SELECT XEOPC_ID, XEOPC_NOMBRE
                                   FROM XEOPC_OPCION 
                                   WHERE XEOPC_ID = @XEOPC_ID";

                SqlCommand cmd = new SqlCommand(query, oConexion);
                cmd.Parameters.AddWithValue("@XEOPC_ID", XEOPC_ID);

                try
                {
                    oConexion.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.Read())
                    {
                        rptOpcion = new Opcion()
                        {
                            XEOPC_ID = dr["XEOPC_ID"]?.ToString(),
                            XEOPC_NOMBRE = dr["XEOPC_NOMBRE"]?.ToString()
                        };
                    }
                    else
                    {
                        rptOpcion = null;
                    }
                    dr.Close();
                    return rptOpcion;
                }
                catch (Exception ex)
                {
                    rptOpcion = null;
                    System.Diagnostics.Debug.WriteLine("Error en ObtenerDetalleOpcion: " + ex.Message);
                    return rptOpcion;
                }
            }
        }

        public bool RegistrarOpcion(Opcion oOpcion)
        {
            bool respuesta = false;
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                try
                {
                    string query = @"INSERT INTO XEOPC_OPCION 
                                         (XEOPC_ID, XEOPC_NOMBRE) 
                                         VALUES 
                                         (@XEOPC_ID, @XEOPC_NOMBRE)";

                    SqlCommand cmd = new SqlCommand(query, oConexion);
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.AddWithValue("@XEOPC_ID", oOpcion.XEOPC_ID);
                    cmd.Parameters.AddWithValue("@XEOPC_NOMBRE", oOpcion.XEOPC_NOMBRE);

                    oConexion.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    respuesta = rowsAffected > 0;
                }
                catch (Exception ex)
                {
                    respuesta = false;
                    System.Diagnostics.Debug.WriteLine("Error en RegistrarOpcion: " + ex.Message);
                }
            }
            return respuesta;
        }

        public bool ModificarOpcion(Opcion oOpcion)
        {
            bool respuesta = false;
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                try
                {
                    string query = @"UPDATE XEOPC_OPCION SET 
                                         XEOPC_NOMBRE = @XEOPC_NOMBRE
                                         WHERE XEOPC_ID = @XEOPC_ID";

                    SqlCommand cmd = new SqlCommand(query, oConexion);

                    cmd.Parameters.AddWithValue("@XEOPC_ID", oOpcion.XEOPC_ID);
                    cmd.Parameters.AddWithValue("@XEOPC_NOMBRE", oOpcion.XEOPC_NOMBRE);

                    oConexion.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    respuesta = rowsAffected > 0;
                }
                catch (Exception ex)
                {
                    respuesta = false;
                    System.Diagnostics.Debug.WriteLine("Error en ModificarOpcion: " + ex.Message);
                }
            }
            return respuesta;
        }

        public bool EliminarOpcion(string XEOPC_ID)
        {
            bool respuesta = false;
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                try
                {
                    // Verificar si la opción está asignada a algún rol
                    string queryVerificar = @"SELECT COUNT(*) FROM XR_XEROL_XEOPC 
                                               WHERE XEOPC_ID = @XEOPC_ID";

                    string queryEliminar = @"DELETE FROM XEOPC_OPCION 
                                               WHERE XEOPC_ID = @XEOPC_ID";

                    oConexion.Open();

                    SqlCommand cmdVerificar = new SqlCommand(queryVerificar, oConexion);
                    cmdVerificar.Parameters.AddWithValue("@XEOPC_ID", XEOPC_ID);
                    int rolesAsignados = Convert.ToInt32(cmdVerificar.ExecuteScalar());

                    if (rolesAsignados > 0)
                    {
                        respuesta = false;
                        System.Diagnostics.Debug.WriteLine("No se puede eliminar opción con roles asignados.");
                    }
                    else
                    {
                        SqlCommand cmdEliminar = new SqlCommand(queryEliminar, oConexion);
                        cmdEliminar.Parameters.AddWithValue("@XEOPC_ID", XEOPC_ID);

                        int rowsAffected = cmdEliminar.ExecuteNonQuery();
                        respuesta = rowsAffected > 0;
                    }
                }
                catch (Exception ex)
                {
                    respuesta = false;
                    System.Diagnostics.Debug.WriteLine("Error en EliminarOpcion: " + ex.Message);
                }
            }
            return respuesta;
        }

        public List<Opcion> BuscarOpcion(string criterio, string valor)
        {
            List<Opcion> rptListaOpciones = new List<Opcion>();
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                try
                {
                    string query = @"SELECT XEOPC_ID, XEOPC_NOMBRE
                                       FROM XEOPC_OPCION 
                                       WHERE ";

                    switch (criterio.ToUpper())
                    {
                        case "ID":
                            query += "XEOPC_ID LIKE @VALOR";
                            break;
                        case "NOMBRE":
                            query += "XEOPC_NOMBRE LIKE @VALOR";
                            break;
                        default:
                            query += "XEOPC_NOMBRE LIKE @VALOR";
                            break;
                    }

                    query += " ORDER BY XEOPC_ID";

                    SqlCommand cmd = new SqlCommand(query, oConexion);
                    cmd.Parameters.AddWithValue("@VALOR", "%" + valor + "%");

                    oConexion.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        rptListaOpciones.Add(new Opcion()
                        {
                            XEOPC_ID = dr["XEOPC_ID"]?.ToString(),
                            XEOPC_NOMBRE = dr["XEOPC_NOMBRE"]?.ToString()
                        });
                    }
                    dr.Close();
                }
                catch (Exception ex)
                {
                    rptListaOpciones = null;
                    System.Diagnostics.Debug.WriteLine("Error en BuscarOpcion: " + ex.Message);
                }
            }
            return rptListaOpciones;
        }

        public bool ValidarNombreOpcionUnico(string XEOPC_NOMBRE, string XEOPC_ID_EXCLUIR = null)
        {
            bool esUnico = true;
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                try
                {
                    string query;
                    SqlCommand cmd;

                    if (string.IsNullOrEmpty(XEOPC_ID_EXCLUIR))
                    {
                        query = @"SELECT COUNT(*) FROM XEOPC_OPCION 
                                   WHERE XEOPC_NOMBRE = @XEOPC_NOMBRE";
                        cmd = new SqlCommand(query, oConexion);
                        cmd.Parameters.AddWithValue("@XEOPC_NOMBRE", XEOPC_NOMBRE);
                    }
                    else
                    {
                        query = @"SELECT COUNT(*) FROM XEOPC_OPCION 
                                   WHERE XEOPC_NOMBRE = @XEOPC_NOMBRE 
                                   AND XEOPC_ID != @XEOPC_ID";
                        cmd = new SqlCommand(query, oConexion);
                        cmd.Parameters.AddWithValue("@XEOPC_NOMBRE", XEOPC_NOMBRE);
                        cmd.Parameters.AddWithValue("@XEOPC_ID", XEOPC_ID_EXCLUIR);
                    }

                    oConexion.Open();
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    esUnico = count == 0;
                }
                catch (Exception ex)
                {
                    esUnico = false;
                    System.Diagnostics.Debug.WriteLine("Error en ValidarNombreOpcionUnico: " + ex.Message);
                }
            }
            return esUnico;
        }

        public List<Opcion> ObtenerOpcionesPorRol(string XEROL_ID)
        {
            List<Opcion> rptListaOpciones = new List<Opcion>();
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                string query = @"SELECT O.XEOPC_ID, O.XEOPC_NOMBRE, 
                                        R.XROP_FECHA_ASIG, R.XROP_FECHA_RETIRO
                                   FROM XEOPC_OPCION O
                                   INNER JOIN XR_XEROL_XEOPC R ON O.XEOPC_ID = R.XEOPC_ID
                                   WHERE R.XEROL_ID = @XEROL_ID
                                   AND (R.XROP_FECHA_RETIRO IS NULL OR R.XROP_FECHA_RETIRO > GETDATE())
                                   ORDER BY O.XEOPC_ID";

                SqlCommand cmd = new SqlCommand(query, oConexion);
                cmd.Parameters.AddWithValue("@XEROL_ID", XEROL_ID);

                try
                {
                    oConexion.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        rptListaOpciones.Add(new Opcion()
                        {
                            XEOPC_ID = dr["XEOPC_ID"]?.ToString(),
                            XEOPC_NOMBRE = dr["XEOPC_NOMBRE"]?.ToString(),
                            XROP_FECHA_ASIG = dr["XROP_FECHA_ASIG"] != DBNull.Value ? Convert.ToDateTime(dr["XROP_FECHA_ASIG"]) : (DateTime?)null,
                            XROP_FECHA_RETIRO = dr["XROP_FECHA_RETIRO"] != DBNull.Value ? Convert.ToDateTime(dr["XROP_FECHA_RETIRO"]) : (DateTime?)null
                        });
                    }
                    dr.Close();
                    return rptListaOpciones;
                }
                catch (Exception ex)
                {
                    rptListaOpciones = null;
                    System.Diagnostics.Debug.WriteLine("Error en ObtenerOpcionesPorRol: " + ex.Message);
                    return rptListaOpciones;
                }
            }
        }
    }
}