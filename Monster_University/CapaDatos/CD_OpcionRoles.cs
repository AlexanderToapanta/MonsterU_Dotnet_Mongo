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
    public class CD_OpcionRoles
    {
        public static CD_OpcionRoles _instancia = null;

        private CD_OpcionRoles()
        {
        }

        public static CD_OpcionRoles Instancia
        {
            get
            {
                if (_instancia == null)
                {
                    _instancia = new CD_OpcionRoles();
                }
                return _instancia;
            }
        }

        public bool AsignarOpcionARol(string XEROL_ID, string XEOPC_ID)
        {
            bool respuesta = false;
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                try
                {
                    // Verificar si ya existe la asignación activa
                    string queryVerificar = @"SELECT COUNT(*) FROM XR_XEROL_XEOPC 
                                               WHERE XEROL_ID = @XEROL_ID 
                                               AND XEOPC_ID = @XEOPC_ID
                                               AND (XROP_FECHA_RETIRO IS NULL OR XROP_FECHA_RETIRO > GETDATE())";

                    string queryInsertar = @"INSERT INTO XR_XEROL_XEOPC 
                                                 (XEROL_ID, XEOPC_ID, XROP_FECHA_ASIG) 
                                                 VALUES 
                                                 (@XEROL_ID, @XEOPC_ID, GETDATE())";

                    oConexion.Open();

                    SqlCommand cmdVerificar = new SqlCommand(queryVerificar, oConexion);
                    cmdVerificar.Parameters.AddWithValue("@XEROL_ID", XEROL_ID);
                    cmdVerificar.Parameters.AddWithValue("@XEOPC_ID", XEOPC_ID);

                    int existeAsignacion = Convert.ToInt32(cmdVerificar.ExecuteScalar());

                    if (existeAsignacion > 0)
                    {
                        respuesta = false;
                        System.Diagnostics.Debug.WriteLine("La opción ya está asignada a este rol.");
                    }
                    else
                    {
                        SqlCommand cmdInsertar = new SqlCommand(queryInsertar, oConexion);
                        cmdInsertar.Parameters.AddWithValue("@XEROL_ID", XEROL_ID);
                        cmdInsertar.Parameters.AddWithValue("@XEOPC_ID", XEOPC_ID);

                        int rowsAffected = cmdInsertar.ExecuteNonQuery();
                        respuesta = rowsAffected > 0;
                    }
                }
                catch (Exception ex)
                {
                    respuesta = false;
                    System.Diagnostics.Debug.WriteLine("Error en AsignarOpcionARol: " + ex.Message);
                }
            }
            return respuesta;
        }

        public bool RetirarOpcionDeRol(string XEROL_ID, string XEOPC_ID)
        {
            bool respuesta = false;
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                try
                {
                    string query = @"UPDATE XR_XEROL_XEOPC 
                                       SET XROP_FECHA_RETIRO = GETDATE()
                                       WHERE XEROL_ID = @XEROL_ID 
                                       AND XEOPC_ID = @XEOPC_ID
                                       AND (XROP_FECHA_RETIRO IS NULL OR XROP_FECHA_RETIRO > GETDATE())";

                    SqlCommand cmd = new SqlCommand(query, oConexion);
                    cmd.Parameters.AddWithValue("@XEROL_ID", XEROL_ID);
                    cmd.Parameters.AddWithValue("@XEOPC_ID", XEOPC_ID);

                    oConexion.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    respuesta = rowsAffected > 0;
                }
                catch (Exception ex)
                {
                    respuesta = false;
                    System.Diagnostics.Debug.WriteLine("Error en RetirarOpcionDeRol: " + ex.Message);
                }
            }
            return respuesta;
        }

        public List<OpcionRol> ObtenerOpcionesConEstadoPorRol(string XEROL_ID)
        {
            List<OpcionRol> rptListaOpcionesRol = new List<OpcionRol>();
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                string query = @"SELECT O.XEOPC_ID, O.XEOPC_NOMBRE,
                                        CASE 
                                            WHEN R.XEROL_ID IS NOT NULL AND 
                                                 (R.XROP_FECHA_RETIRO IS NULL OR R.XROP_FECHA_RETIRO > GETDATE()) 
                                            THEN 1 
                                            ELSE 0 
                                        END AS ASIGNADO,
                                        R.XROP_FECHA_ASIG,
                                        R.XROP_FECHA_RETIRO
                                   FROM XEOPC_OPCION O
                                   LEFT JOIN XR_XEROL_XEOPC R ON O.XEOPC_ID = R.XEOPC_ID 
                                        AND R.XEROL_ID = @XEROL_ID
                                   ORDER BY O.XEOPC_ID";

                SqlCommand cmd = new SqlCommand(query, oConexion);
                cmd.Parameters.AddWithValue("@XEROL_ID", XEROL_ID);

                try
                {
                    oConexion.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        rptListaOpcionesRol.Add(new OpcionRol()
                        {
                            XEOPC_ID = dr["XEOPC_ID"]?.ToString(),
                            XEOPC_NOMBRE = dr["XEOPC_NOMBRE"]?.ToString(),
                            ASIGNADO = Convert.ToInt32(dr["ASIGNADO"]) == 1,
                            XROP_FECHA_ASIG = dr["XROP_FECHA_ASIG"] != DBNull.Value ? Convert.ToDateTime(dr["XROP_FECHA_ASIG"]) : (DateTime?)null,
                            XROP_FECHA_RETIRO = dr["XROP_FECHA_RETIRO"] != DBNull.Value ? Convert.ToDateTime(dr["XROP_FECHA_RETIRO"]) : (DateTime?)null
                        });
                    }
                    dr.Close();
                    return rptListaOpcionesRol;
                }
                catch (Exception ex)
                {
                    rptListaOpcionesRol = null;
                    System.Diagnostics.Debug.WriteLine("Error en ObtenerOpcionesConEstadoPorRol: " + ex.Message);
                    return rptListaOpcionesRol;
                }
            }
        }

        public List<OpcionRol> ObtenerTodasAsignaciones()
        {
            List<OpcionRol> rptListaAsignaciones = new List<OpcionRol>();
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                string query = @"SELECT R.XEROL_ID, RL.XEROL_NOMBRE, 
                                        O.XEOPC_ID, O.XEOPC_NOMBRE,
                                        R.XROP_FECHA_ASIG, R.XROP_FECHA_RETIRO
                                   FROM XR_XEROL_XEOPC R
                                   INNER JOIN XEROL_ROL RL ON R.XEROL_ID = RL.XEROL_ID
                                   INNER JOIN XEOPC_OPCION O ON R.XEOPC_ID = O.XEOPC_ID
                                   WHERE R.XROP_FECHA_RETIRO IS NULL OR R.XROP_FECHA_RETIRO > GETDATE()
                                   ORDER BY RL.XEROL_NOMBRE, O.XEOPC_NOMBRE";

                SqlCommand cmd = new SqlCommand(query, oConexion);

                try
                {
                    oConexion.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        rptListaAsignaciones.Add(new OpcionRol()
                        {
                            XEROL_ID = dr["XEROL_ID"]?.ToString(),
                            XEROL_NOMBRE = dr["XEROL_NOMBRE"]?.ToString(),
                            XEOPC_ID = dr["XEOPC_ID"]?.ToString(),
                            XEOPC_NOMBRE = dr["XEOPC_NOMBRE"]?.ToString(),
                            XROP_FECHA_ASIG = dr["XROP_FECHA_ASIG"] != DBNull.Value ? Convert.ToDateTime(dr["XROP_FECHA_ASIG"]) : (DateTime?)null,
                            XROP_FECHA_RETIRO = dr["XROP_FECHA_RETIRO"] != DBNull.Value ? Convert.ToDateTime(dr["XROP_FECHA_RETIRO"]) : (DateTime?)null
                        });
                    }
                    dr.Close();
                    return rptListaAsignaciones;
                }
                catch (Exception ex)
                {
                    rptListaAsignaciones = null;
                    System.Diagnostics.Debug.WriteLine("Error en ObtenerTodasAsignaciones: " + ex.Message);
                    return rptListaAsignaciones;
                }
            }
        }

        public bool ValidarAsignacionExistente(string XEROL_ID, string XEOPC_ID)
        {
            bool existe = false;
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                try
                {
                    string query = @"SELECT COUNT(*) FROM XR_XEROL_XEOPC 
                                       WHERE XEROL_ID = @XEROL_ID 
                                       AND XEOPC_ID = @XEOPC_ID
                                       AND (XROP_FECHA_RETIRO IS NULL OR XROP_FECHA_RETIRO > GETDATE())";

                    SqlCommand cmd = new SqlCommand(query, oConexion);
                    cmd.Parameters.AddWithValue("@XEROL_ID", XEROL_ID);
                    cmd.Parameters.AddWithValue("@XEOPC_ID", XEOPC_ID);

                    oConexion.Open();
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    existe = count > 0;
                }
                catch (Exception ex)
                {
                    existe = false;
                    System.Diagnostics.Debug.WriteLine("Error en ValidarAsignacionExistente: " + ex.Message);
                }
            }
            return existe;
        }

        public bool ReactivarOpcionARol(string XEROL_ID, string XEOPC_ID)
        {
            bool respuesta = false;
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                try
                {
                    string query = @"UPDATE XR_XEROL_XEOPC 
                                       SET XROP_FECHA_RETIRO = NULL,
                                           XROP_FECHA_ASIG = GETDATE()
                                       WHERE XEROL_ID = @XEROL_ID 
                                       AND XEOPC_ID = @XEOPC_ID";

                    SqlCommand cmd = new SqlCommand(query, oConexion);
                    cmd.Parameters.AddWithValue("@XEROL_ID", XEROL_ID);
                    cmd.Parameters.AddWithValue("@XEOPC_ID", XEOPC_ID);

                    oConexion.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        // Si no existía el registro, insertar uno nuevo
                        respuesta = AsignarOpcionARol(XEROL_ID, XEOPC_ID);
                    }
                    else
                    {
                        respuesta = rowsAffected > 0;
                    }
                }
                catch (Exception ex)
                {
                    respuesta = false;
                    System.Diagnostics.Debug.WriteLine("Error en ReactivarOpcionARol: " + ex.Message);
                }
            }
            return respuesta;
        }
    }
}