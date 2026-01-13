using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CapaModelo;

namespace CapaDatos
{
    public class CD_EstadoCivil
    {
        public static CD_EstadoCivil _instancia = null;

        private CD_EstadoCivil()
        {
        }

        public static CD_EstadoCivil Instancia
        {
            get
            {
                if (_instancia == null)
                {
                    _instancia = new CD_EstadoCivil();
                }
                return _instancia;
            }
        }

        // Método para obtener todos los estados civiles
        public List<PEESC_ESTCIV> ObtenerEstadosCiviles()
        {
            List<PEESC_ESTCIV> listaEstadosCiviles = new List<PEESC_ESTCIV>();
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                string query = @"SELECT PEESC_ID, PEESC_DESCRI 
                                 FROM PEESC_ESTCIV 
                                 ORDER BY PEESC_ID";

                SqlCommand cmd = new SqlCommand(query, oConexion);

                try
                {
                    oConexion.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        listaEstadosCiviles.Add(new PEESC_ESTCIV()
                        {
                            PEESC_ID = dr["PEESC_ID"]?.ToString(),
                            PEESC_DESCRI = dr["PEESC_DESCRI"]?.ToString()
                        });
                    }
                    dr.Close();
                }
                catch (Exception ex)
                {
                    listaEstadosCiviles = null;
                    System.Diagnostics.Debug.WriteLine("Error en ObtenerEstadosCiviles: " + ex.Message);
                }
            }
            return listaEstadosCiviles;
        }

        // Método para obtener un estado civil específico por ID
        public PEESC_ESTCIV ObtenerEstadoCivilPorId(string id)
        {
            PEESC_ESTCIV estadoCivil = null;
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                string query = @"SELECT PEESC_ID, PEESC_DESCRI 
                                 FROM PEESC_ESTCIV 
                                 WHERE PEESC_ID = @PEESC_ID";

                SqlCommand cmd = new SqlCommand(query, oConexion);
                cmd.Parameters.AddWithValue("@PEESC_ID", id);

                try
                {
                    oConexion.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.Read())
                    {
                        estadoCivil = new PEESC_ESTCIV()
                        {
                            PEESC_ID = dr["PEESC_ID"]?.ToString(),
                            PEESC_DESCRI = dr["PEESC_DESCRI"]?.ToString()
                        };
                    }
                    dr.Close();
                }
                catch (Exception ex)
                {
                    estadoCivil = null;
                    System.Diagnostics.Debug.WriteLine("Error en ObtenerEstadoCivilPorId: " + ex.Message);
                }
            }
            return estadoCivil;
        }

        // Método para registrar un nuevo estado civil
        public bool RegistrarEstadoCivil(PEESC_ESTCIV estadoCivil)
        {
            bool respuesta = false;
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                try
                {
                    string query = @"INSERT INTO PEESC_ESTCIV (PEESC_ID, PEESC_DESCRI) 
                                     VALUES (@PEESC_ID, @PEESC_DESCRI)";

                    SqlCommand cmd = new SqlCommand(query, oConexion);
                    cmd.Parameters.AddWithValue("@PEESC_ID", estadoCivil.PEESC_ID);
                    cmd.Parameters.AddWithValue("@PEESC_DESCRI", estadoCivil.PEESC_DESCRI);

                    oConexion.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    respuesta = rowsAffected > 0;
                }
                catch (Exception ex)
                {
                    respuesta = false;
                    System.Diagnostics.Debug.WriteLine("Error en RegistrarEstadoCivil: " + ex.Message);
                }
            }
            return respuesta;
        }

        // Método para modificar un estado civil
        public bool ModificarEstadoCivil(PEESC_ESTCIV estadoCivil)
        {
            bool respuesta = false;
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                try
                {
                    string query = @"UPDATE PEESC_ESTCIV 
                                     SET PEESC_DESCRI = @PEESC_DESCRI
                                     WHERE PEESC_ID = @PEESC_ID";

                    SqlCommand cmd = new SqlCommand(query, oConexion);
                    cmd.Parameters.AddWithValue("@PEESC_ID", estadoCivil.PEESC_ID);
                    cmd.Parameters.AddWithValue("@PEESC_DESCRI", estadoCivil.PEESC_DESCRI);

                    oConexion.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    respuesta = rowsAffected > 0;
                }
                catch (Exception ex)
                {
                    respuesta = false;
                    System.Diagnostics.Debug.WriteLine("Error en ModificarEstadoCivil: " + ex.Message);
                }
            }
            return respuesta;
        }

        // Método para eliminar un estado civil
        public bool EliminarEstadoCivil(string id)
        {
            bool respuesta = false;
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                try
                {
                    // Primero verificar si hay personas usando este estado civil
                    string queryVerificar = @"SELECT COUNT(*) FROM PEPER_PERSON 
                                              WHERE PEESC_ID = @PEESC_ID";

                    string queryEliminar = @"DELETE FROM PEESC_ESTCIV 
                                              WHERE PEESC_ID = @PEESC_ID";

                    oConexion.Open();

                    SqlCommand cmdVerificar = new SqlCommand(queryVerificar, oConexion);
                    cmdVerificar.Parameters.AddWithValue("@PEESC_ID", id);
                    int personasRelacionadas = Convert.ToInt32(cmdVerificar.ExecuteScalar());

                    if (personasRelacionadas > 0)
                    {
                        respuesta = false;
                        System.Diagnostics.Debug.WriteLine("No se puede eliminar estado civil con personas relacionadas.");
                    }
                    else
                    {
                        SqlCommand cmdEliminar = new SqlCommand(queryEliminar, oConexion);
                        cmdEliminar.Parameters.AddWithValue("@PEESC_ID", id);

                        int rowsAffected = cmdEliminar.ExecuteNonQuery();
                        respuesta = rowsAffected > 0;
                    }
                }
                catch (Exception ex)
                {
                    respuesta = false;
                    System.Diagnostics.Debug.WriteLine("Error en EliminarEstadoCivil: " + ex.Message);
                }
            }
            return respuesta;
        }
    }
}