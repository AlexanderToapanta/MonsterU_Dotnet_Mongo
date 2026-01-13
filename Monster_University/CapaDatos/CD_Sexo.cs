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
    public class CD_Sexo
    {
        public static CD_Sexo _instancia = null;

        private CD_Sexo()
        {
        }

        public static CD_Sexo Instancia
        {
            get
            {
                if (_instancia == null)
                {
                    _instancia = new CD_Sexo();
                }
                return _instancia;
            }
        }

        public List<PESEX_SEXO> ObtenerSexos()
        {
            List<PESEX_SEXO> rptListaSexo = new List<PESEX_SEXO>();
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                string query = @"SELECT PESEX_ID, PESEX_DESCRI 
                                 FROM PESEX_SEXO 
                                 ORDER BY PESEX_ID";

                SqlCommand cmd = new SqlCommand(query, oConexion);

                try
                {
                    oConexion.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        rptListaSexo.Add(new PESEX_SEXO()
                        {
                            PESEX_ID = dr["PESEX_ID"]?.ToString(),
                            PESEX_DESCRI = dr["PESEX_DESCRI"]?.ToString()
                        });
                    }
                    dr.Close();
                    return rptListaSexo;
                }
                catch (Exception ex)
                {
                    rptListaSexo = null;
                    System.Diagnostics.Debug.WriteLine("Error en ObtenerSexos: " + ex.Message);
                    return rptListaSexo;
                }
            }
        }

        public PESEX_SEXO ObtenerDetalleSexo(string PESEX_ID)
        {
            PESEX_SEXO rptSexo = new PESEX_SEXO();
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                string query = @"SELECT PESEX_ID, PESEX_DESCRI 
                         FROM PESEX_SEXO 
                         WHERE PESEX_ID = @PESEX_ID";

                SqlCommand cmd = new SqlCommand(query, oConexion);
                cmd.Parameters.AddWithValue("@PESEX_ID", PESEX_ID); 
                try
                {
                    oConexion.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.Read())
                    {
                        rptSexo = new PESEX_SEXO()
                        {
                            PESEX_ID = dr["PESEX_ID"]?.ToString(),
                            PESEX_DESCRI = dr["PESEX_DESCRI"]?.ToString()
                        };
                    }
                    else
                    {
                        rptSexo = null;
                    }
                    dr.Close();
                    return rptSexo;
                }
                catch (Exception ex)
                {
                    rptSexo = null;
                    System.Diagnostics.Debug.WriteLine("Error en ObtenerDetalleSexo: " + ex.Message);
                    return rptSexo;
                }
            }
        }

        public bool RegistrarSexo(PESEX_SEXO oSexo)
        {
            bool respuesta = false;
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                try
                {
                    string query = @"INSERT INTO PESEX_SEXO 
                                    (PESEX_ID, PESEX_DESCRI)
                                     VALUES 
                                    (@PESEX_ID, @PESEX_DESCRI)";

                    SqlCommand cmd = new SqlCommand(query, oConexion);
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.AddWithValue("@PESEX_ID", oSexo.PESEX_ID);
                    cmd.Parameters.AddWithValue("@PESEX_DESCRI", oSexo.PESEX_DESCRI);

                    oConexion.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    respuesta = rowsAffected > 0;
                }
                catch (Exception ex)
                {
                    respuesta = false;
                    System.Diagnostics.Debug.WriteLine("Error en RegistrarSexo: " + ex.Message);
                }
            }
            return respuesta;
        }

        public bool ModificarSexo(PESEX_SEXO oSexo)
        {
            bool respuesta = false;
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                try
                {
                    string query = @"UPDATE PESEX_SEXO SET 
                                    PESEX_DESCRI = @PESEX_DESCRI
                                    WHERE PESEX_ID = @PESEX_ID";

                    SqlCommand cmd = new SqlCommand(query, oConexion);

                    cmd.Parameters.AddWithValue("@PESEX_ID", oSexo.PESEX_ID);
                    cmd.Parameters.AddWithValue("@PESEX_DESCRI", oSexo.PESEX_DESCRI);

                    oConexion.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    respuesta = rowsAffected > 0;
                }
                catch (Exception ex)
                {
                    respuesta = false;
                    System.Diagnostics.Debug.WriteLine("Error en ModificarSexo: " + ex.Message);
                }
            }
            return respuesta;
        }

        public bool EliminarSexo(string PESEX_ID)
        {
            bool respuesta = false;
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                try
                {
                    // Verificar si hay personas relacionadas antes de eliminar
                    string queryVerificar = @"SELECT COUNT(*) FROM PEPER_PERSON 
                                             WHERE PESEX_ID = @PESEX_ID";

                    string queryEliminar = @"DELETE FROM PESEX_SEXO 
                                            WHERE PESEX_ID = @PESEX_ID";

                    oConexion.Open();

                    // Verificar si hay personas relacionadas
                    SqlCommand cmdVerificar = new SqlCommand(queryVerificar, oConexion);
                    cmdVerificar.Parameters.AddWithValue("@PESEX_ID", PESEX_ID);
                    int personasRelacionadas = Convert.ToInt32(cmdVerificar.ExecuteScalar());

                    if (personasRelacionadas > 0)
                    {
                        // No se puede eliminar porque hay personas relacionadas
                        respuesta = false;
                        System.Diagnostics.Debug.WriteLine("No se puede eliminar sexo con personas relacionadas.");
                    }
                    else
                    {
                        // Eliminar el sexo
                        SqlCommand cmdEliminar = new SqlCommand(queryEliminar, oConexion);
                        cmdEliminar.Parameters.AddWithValue("@PESEX_ID", PESEX_ID);

                        int rowsAffected = cmdEliminar.ExecuteNonQuery();
                        respuesta = rowsAffected > 0;
                    }
                }
                catch (Exception ex)
                {
                    respuesta = false;
                    System.Diagnostics.Debug.WriteLine("Error en EliminarSexo: " + ex.Message);
                }
            }
            return respuesta;
        }

        public List<PESEX_SEXO> BuscarSexo(string criterio, string valor)
        {
            List<PESEX_SEXO> rptListaSexo = new List<PESEX_SEXO>();
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                try
                {
                    string query = @"SELECT PESEX_ID, PESEX_DESCRI 
                                     FROM PESEX_SEXO 
                                     WHERE ";

                    switch (criterio.ToUpper())
                    {
                        case "CODIGO":
                            query += "PESEX_ID = @VALOR";
                            break;
                        case "DESCRIPCION":
                            query += "PESEX_DESCRI LIKE @VALOR";
                            break;
                        default:
                            query += "PESEX_DESCRI LIKE @VALOR";
                            break;
                    }

                    query += " ORDER BY PESEX_ID";

                    SqlCommand cmd = new SqlCommand(query, oConexion);

                    if (criterio.ToUpper() == "DESCRIPCION")
                    {
                        cmd.Parameters.AddWithValue("@VALOR", "%" + valor + "%");
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@VALOR", valor);
                    }

                    oConexion.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        rptListaSexo.Add(new PESEX_SEXO()
                        {
                            PESEX_ID = dr["PESEX_ID"]?.ToString(),
                            PESEX_DESCRI = dr["PESEX_DESCRI"]?.ToString()
                        });
                    }
                    dr.Close();
                }
                catch (Exception ex)
                {
                    rptListaSexo = null;
                    System.Diagnostics.Debug.WriteLine("Error en BuscarSexo: " + ex.Message);
                }
            }
            return rptListaSexo;
        }

        public bool ValidarCodigoUnico(string PESEX_ID, string PESEX_ID_EXCLUIR = null)
        {
            bool esUnico = true;
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                try
                {
                    string query;
                    SqlCommand cmd;

                    if (string.IsNullOrEmpty(PESEX_ID_EXCLUIR))
                    {
                        query = @"SELECT COUNT(*) FROM PESEX_SEXO 
                                 WHERE PESEX_ID = @PESEX_ID";
                        cmd = new SqlCommand(query, oConexion);
                        cmd.Parameters.AddWithValue("@PESEX_ID", PESEX_ID);
                    }
                    else
                    {
                        query = @"SELECT COUNT(*) FROM PESEX_SEXO 
                                 WHERE PESEX_ID = @PESEX_ID 
                                 AND PESEX_ID != @PESEX_ID_EXCLUIR";
                        cmd = new SqlCommand(query, oConexion);
                        cmd.Parameters.AddWithValue("@PESEX_ID", PESEX_ID);
                        cmd.Parameters.AddWithValue("@PESEX_ID_EXCLUIR", PESEX_ID_EXCLUIR);
                    }

                    oConexion.Open();
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    esUnico = count == 0;
                }
                catch (Exception ex)
                {
                    esUnico = false;
                    System.Diagnostics.Debug.WriteLine("Error en ValidarCodigoUnico: " + ex.Message);
                }
            }
            return esUnico;
        }

        public bool ValidarDescripcionUnica(string PESEX_DESCRI, string PESEX_ID_EXCLUIR = null)
        {
            bool esUnica = true;
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                try
                {
                    string query;
                    SqlCommand cmd;

                    if (string.IsNullOrEmpty(PESEX_ID_EXCLUIR))
                    {
                        query = @"SELECT COUNT(*) FROM PESEX_SEXO 
                                 WHERE PESEX_DESCRI = @PESEX_DESCRI";
                        cmd = new SqlCommand(query, oConexion);
                        cmd.Parameters.AddWithValue("@PESEX_DESCRI", PESEX_DESCRI);
                    }
                    else
                    {
                        query = @"SELECT COUNT(*) FROM PESEX_SEXO 
                                 WHERE PESEX_DESCRI = @PESEX_DESCRI 
                                 AND PESEX_ID != @PESEX_ID_EXCLUIR";
                        cmd = new SqlCommand(query, oConexion);
                        cmd.Parameters.AddWithValue("@PESEX_DESCRI", PESEX_DESCRI);
                        cmd.Parameters.AddWithValue("@PESEX_ID_EXCLUIR", PESEX_ID_EXCLUIR);
                    }

                    oConexion.Open();
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    esUnica = count == 0;
                }
                catch (Exception ex)
                {
                    esUnica = false;
                    System.Diagnostics.Debug.WriteLine("Error en ValidarDescripcionUnica: " + ex.Message);
                }
            }
            return esUnica;
        }

        public Dictionary<string, string> ObtenerSexosDictionary()
        {
            Dictionary<string, string> diccionario = new Dictionary<string, string>();
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                string query = @"SELECT PESEX_ID, PESEX_DESCRI 
                                 FROM PESEX_SEXO 
                                 ORDER BY PESEX_ID";

                SqlCommand cmd = new SqlCommand(query, oConexion);

                try
                {
                    oConexion.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        string id = dr["PESEX_ID"]?.ToString();
                        string descripcion = dr["PESEX_DESCRI"]?.ToString();
                        if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(descripcion))
                        {
                            diccionario[id] = descripcion;
                        }
                    }
                    dr.Close();
                }
                catch (Exception ex)
                {
                    diccionario = null;
                    System.Diagnostics.Debug.WriteLine("Error en ObtenerSexosDictionary: " + ex.Message);
                }
            }
            return diccionario;
        }

        public List<string> ObtenerCodigosSexo()
        {
            List<string> codigos = new List<string>();
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                string query = @"SELECT PESEX_ID 
                                 FROM PESEX_SEXO 
                                 ORDER BY PESEX_ID";

                SqlCommand cmd = new SqlCommand(query, oConexion);

                try
                {
                    oConexion.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        codigos.Add(dr["PESEX_ID"]?.ToString());
                    }
                    dr.Close();
                }
                catch (Exception ex)
                {
                    codigos = null;
                    System.Diagnostics.Debug.WriteLine("Error en ObtenerCodigosSexo: " + ex.Message);
                }
            }
            return codigos;
        }
    }
}