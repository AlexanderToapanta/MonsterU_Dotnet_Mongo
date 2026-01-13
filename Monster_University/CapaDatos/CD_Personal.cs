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
    public class CD_Personal
    {
        public static CD_Personal _instancia = null;

        private CD_Personal()
        {
        }

        public static CD_Personal Instancia
        {
            get
            {
                if (_instancia == null)
                {
                    _instancia = new CD_Personal();
                }
                return _instancia;
            }
        }

        public List<Personal> ObtenerPersonales()
        {
            List<Personal> rptListaPersonal = new List<Personal>();
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                // Incluyendo PEPEPER_FEC_NA y PEPEPER_IMAG
                string query = @"SELECT PEPER_ID, PEPER_NOMBRE, PEPER_APELLIDO, PEPER_EMAIL, 
                                         PEPER_CEDULA, PEPER_CELULAR, PEPER_TIPO, PEPEPER_FECH_INGR,
                                         PEESC_ID, PESEX_ID, XEUSU_ID,
                                         PEPEPER_FEC_NA, PEPEPER_IMAG 
                                   FROM PEPER_PERSON 
                                   ORDER BY PEPER_ID";

                SqlCommand cmd = new SqlCommand(query, oConexion);

                try
                {
                    oConexion.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        rptListaPersonal.Add(new Personal()
                        {
                            PEPER_ID = dr["PEPER_ID"]?.ToString(),
                            PEPER_NOMBRE = dr["PEPER_NOMBRE"]?.ToString(),
                            PEPER_APELLIDO = dr["PEPER_APELLIDO"]?.ToString(),
                            PEPER_EMAIL = dr["PEPER_EMAIL"]?.ToString(),
                            PEPER_CEDULA = dr["PEPER_CEDULA"]?.ToString(),
                            PEPER_CELULAR = dr["PEPER_CELULAR"]?.ToString(),
                            PEPER_TIPO = dr["PEPER_TIPO"]?.ToString(),
                            PEPEPER_FECH_INGR = dr["PEPEPER_FECH_INGR"] != DBNull.Value ? Convert.ToDateTime(dr["PEPEPER_FECH_INGR"]) : (DateTime?)null,
                            PEESC_ID = dr["PEESC_ID"]?.ToString(),
                            PESEX_ID = dr["PESEX_ID"]?.ToString(),
                            XEUSU_ID = dr["XEUSU_ID"]?.ToString(),

                            // Campos corregidos y mapeados:
                            PEPER_FECHA_NAC = dr["PEPEPER_FEC_NA"] != DBNull.Value ? Convert.ToDateTime(dr["PEPEPER_FEC_NA"]) : (DateTime?)null,
                            PEPER_FOTO = dr["PEPEPER_IMAG"] != DBNull.Value ? dr["PEPEPER_IMAG"].ToString() : null
                        });
                    }
                    dr.Close();
                    return rptListaPersonal;
                }
                catch (Exception ex)
                {
                    rptListaPersonal = null;
                    System.Diagnostics.Debug.WriteLine("Error en ObtenerPersonales: " + ex.Message);
                    return rptListaPersonal;
                }
            }
        }

        public Personal ObtenerDetallePersonal(string PEPER_ID)
        {
            Personal rptPersonal = new Personal();
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                // Incluyendo PEPEPER_FEC_NA y PEPEPER_IMAG
                string query = @"SELECT PEPER_ID, PEPER_NOMBRE, PEPER_APELLIDO, PEPER_EMAIL, 
                                         PEPER_CEDULA, PEPER_CELULAR, PEPER_TIPO, PEPEPER_FECH_INGR,
                                         PEESC_ID, PESEX_ID, XEUSU_ID,
                                         PEPEPER_FEC_NA, PEPEPER_IMAG
                                   FROM PEPER_PERSON 
                                   WHERE PEPER_ID = @PEPER_ID";

                SqlCommand cmd = new SqlCommand(query, oConexion);
                cmd.Parameters.AddWithValue("@PEPER_ID", PEPER_ID);

                try
                {
                    oConexion.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.Read())
                    {
                        rptPersonal = new Personal()
                        {
                            PEPER_ID = dr["PEPER_ID"]?.ToString(),
                            PEPER_NOMBRE = dr["PEPER_NOMBRE"]?.ToString(),
                            PEPER_APELLIDO = dr["PEPER_APELLIDO"]?.ToString(),
                            PEPER_EMAIL = dr["PEPER_EMAIL"]?.ToString(),
                            PEPER_CEDULA = dr["PEPER_CEDULA"]?.ToString(),
                            PEPER_CELULAR = dr["PEPER_CELULAR"]?.ToString(),
                            PEPER_TIPO = dr["PEPER_TIPO"]?.ToString(),
                            PEPEPER_FECH_INGR = dr["PEPEPER_FECH_INGR"] != DBNull.Value ? Convert.ToDateTime(dr["PEPEPER_FECH_INGR"]) : (DateTime?)null,
                            PEESC_ID = dr["PEESC_ID"]?.ToString(),
                            PESEX_ID = dr["PESEX_ID"]?.ToString(),
                            XEUSU_ID = dr["XEUSU_ID"]?.ToString(),

                            // Campos corregidos y mapeados:
                            PEPER_FECHA_NAC = dr["PEPEPER_FEC_NA"] != DBNull.Value ? Convert.ToDateTime(dr["PEPEPER_FEC_NA"]) : (DateTime?)null,
                            PEPER_FOTO = dr["PEPEPER_IMAG"] != DBNull.Value ? dr["PEPEPER_IMAG"].ToString() : null
                        };
                    }
                    else
                    {
                        rptPersonal = null;
                    }
                    dr.Close();
                    return rptPersonal;
                }
                catch (Exception ex)
                {
                    rptPersonal = null;
                    System.Diagnostics.Debug.WriteLine("Error en ObtenerDetallePersonal: " + ex.Message);
                    return rptPersonal;
                }
            }
        }

        public bool RegistrarPersonal(Personal oPersonal)
        {
            bool respuesta = false;
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                try
                {
                    // Incluyendo PEPEPER_FEC_NA y PEPEPER_IMAG en la lista de columnas y valores
                    string query = @"INSERT INTO PEPER_PERSON 
                                         (PEPER_ID, PEPER_NOMBRE, PEPER_APELLIDO, PEPER_EMAIL, 
                                          PEPER_CEDULA, PEPER_CELULAR, PEPER_TIPO, PEPEPER_FECH_INGR,
                                          PEESC_ID, PESEX_ID, XEUSU_ID, 
                                          PEPEPER_FEC_NA, PEPEPER_IMAG) 
                                          VALUES 
                                         (@PEPER_ID, @PEPER_NOMBRE, @PEPER_APELLIDO, @PEPER_EMAIL, 
                                          @PEPER_CEDULA, @PEPER_CELULAR, @PEPER_TIPO, @PEPEPER_FECH_INGR,
                                          @PEESC_ID, @PESEX_ID, @XEUSU_ID,
                                          @PEPEPER_FEC_NA, @PEPEPER_IMAG)";

                    SqlCommand cmd = new SqlCommand(query, oConexion);
                    cmd.CommandType = CommandType.Text;

                    // Parámetros existentes
                    cmd.Parameters.AddWithValue("@PEPER_ID", oPersonal.PEPER_ID);
                    cmd.Parameters.AddWithValue("@PEPER_NOMBRE", oPersonal.PEPER_NOMBRE);
                    cmd.Parameters.AddWithValue("@PEPER_APELLIDO", oPersonal.PEPER_APELLIDO);
                    cmd.Parameters.AddWithValue("@PEPER_EMAIL", oPersonal.PEPER_EMAIL);
                    cmd.Parameters.AddWithValue("@PEPER_CEDULA", oPersonal.PEPER_CEDULA);
                    cmd.Parameters.AddWithValue("@PEPER_CELULAR",
                        string.IsNullOrEmpty(oPersonal.PEPER_CELULAR) ? (object)DBNull.Value : oPersonal.PEPER_CELULAR);
                    cmd.Parameters.AddWithValue("@PEPER_TIPO", oPersonal.PEPER_TIPO);
                    cmd.Parameters.AddWithValue("@PEPEPER_FECH_INGR", oPersonal.PEPEPER_FECH_INGR);
                    cmd.Parameters.AddWithValue("@PESEX_ID", oPersonal.PESEX_ID);
                    cmd.Parameters.AddWithValue("@PEESC_ID",
                        string.IsNullOrEmpty(oPersonal.PEESC_ID) ? (object)DBNull.Value : oPersonal.PEESC_ID);
                    cmd.Parameters.AddWithValue("@XEUSU_ID",
                        string.IsNullOrEmpty(oPersonal.XEUSU_ID) ? (object)DBNull.Value : oPersonal.XEUSU_ID);

                    // Campos corregidos y mapeados (manejo de NULLs):
                    cmd.Parameters.AddWithValue("@PEPEPER_FEC_NA",
                        oPersonal.PEPER_FECHA_NAC.HasValue ? (object)oPersonal.PEPER_FECHA_NAC.Value : (object)DBNull.Value);

                    cmd.Parameters.AddWithValue("@PEPEPER_IMAG",
                        string.IsNullOrEmpty(oPersonal.PEPER_FOTO) ? (object)DBNull.Value : (object)oPersonal.PEPER_FOTO);

                    oConexion.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    respuesta = rowsAffected > 0;
                }
                catch (Exception ex)
                {
                    respuesta = false;
                    System.Diagnostics.Debug.WriteLine("Error en RegistrarPersonal: " + ex.Message);
                }
            }
            return respuesta;
        }

        public bool ModificarPersonal(Personal oPersonal)
        {
            bool respuesta = false;
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                try
                {
                    // Incluyendo PEPEPER_FEC_NA y PEPEPER_IMAG en el UPDATE
                    string query = @"UPDATE PEPER_PERSON SET 
                                         PEPER_NOMBRE = @PEPER_NOMBRE,
                                         PEPER_APELLIDO = @PEPER_APELLIDO,
                                         PEPER_EMAIL = @PEPER_EMAIL,
                                         PEPER_CEDULA = @PEPER_CEDULA,
                                         PEPER_CELULAR = @PEPER_CELULAR,
                                         PEPER_TIPO = @PEPER_TIPO,
                                         PEPEPER_FECH_INGR = @PEPEPER_FECH_INGR,
                                         PEESC_ID = @PEESC_ID,
                                         PESEX_ID = @PESEX_ID,
                                         XEUSU_ID = @XEUSU_ID,
                                         PEPEPER_FEC_NA = @PEPEPER_FEC_NA,
                                         PEPEPER_IMAG = @PEPEPER_IMAG
                                         WHERE PEPER_ID = @PEPER_ID";

                    SqlCommand cmd = new SqlCommand(query, oConexion);

                    // Parámetros existentes
                    cmd.Parameters.AddWithValue("@PEPER_ID", oPersonal.PEPER_ID);
                    cmd.Parameters.AddWithValue("@PEPER_NOMBRE", oPersonal.PEPER_NOMBRE);
                    cmd.Parameters.AddWithValue("@PEPER_APELLIDO", oPersonal.PEPER_APELLIDO);
                    cmd.Parameters.AddWithValue("@PEPER_EMAIL", oPersonal.PEPER_EMAIL);
                    cmd.Parameters.AddWithValue("@PEPER_CEDULA", oPersonal.PEPER_CEDULA);
                    cmd.Parameters.AddWithValue("@PEPER_CELULAR",
                        string.IsNullOrEmpty(oPersonal.PEPER_CELULAR) ? (object)DBNull.Value : oPersonal.PEPER_CELULAR);
                    cmd.Parameters.AddWithValue("@PEPER_TIPO", oPersonal.PEPER_TIPO);
                    cmd.Parameters.AddWithValue("@PEPEPER_FECH_INGR", oPersonal.PEPEPER_FECH_INGR);
                    cmd.Parameters.AddWithValue("@PESEX_ID", oPersonal.PESEX_ID);
                    cmd.Parameters.AddWithValue("@PEESC_ID",
                        string.IsNullOrEmpty(oPersonal.PEESC_ID) ? (object)DBNull.Value : oPersonal.PEESC_ID);
                    cmd.Parameters.AddWithValue("@XEUSU_ID",
                        string.IsNullOrEmpty(oPersonal.XEUSU_ID) ? (object)DBNull.Value : oPersonal.XEUSU_ID);

                    // Campos corregidos y mapeados (manejo de NULLs):
                    cmd.Parameters.AddWithValue("@PEPEPER_FEC_NA",
                        oPersonal.PEPER_FECHA_NAC.HasValue ? (object)oPersonal.PEPER_FECHA_NAC.Value : (object)DBNull.Value);

                    cmd.Parameters.AddWithValue("@PEPEPER_IMAG",
                        string.IsNullOrEmpty(oPersonal.PEPER_FOTO) ? (object)DBNull.Value : (object)oPersonal.PEPER_FOTO);

                    oConexion.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    respuesta = rowsAffected > 0;
                }
                catch (Exception ex)
                {
                    respuesta = false;
                    System.Diagnostics.Debug.WriteLine("Error en ModificarPersonal: " + ex.Message);
                }
            }
            return respuesta;
        }

        public bool EliminarPersonal(string PEPER_ID)
        {
            bool respuesta = false;
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                try
                {
                    string queryVerificar = @"SELECT COUNT(*) FROM XEUSU_USUAR 
                                               WHERE PEPER_ID = @PEPER_ID";

                    string queryEliminar = @"DELETE FROM PEPER_PERSON 
                                               WHERE PEPER_ID = @PEPER_ID";

                    oConexion.Open();

                    SqlCommand cmdVerificar = new SqlCommand(queryVerificar, oConexion);
                    cmdVerificar.Parameters.AddWithValue("@PEPER_ID", PEPER_ID);
                    int usuariosRelacionados = Convert.ToInt32(cmdVerificar.ExecuteScalar());

                    if (usuariosRelacionados > 0)
                    {
                        respuesta = false;
                        System.Diagnostics.Debug.WriteLine("No se puede eliminar personal con usuarios relacionados.");
                    }
                    else
                    {
                        string queryVerificarGrupos = @"SELECT COUNT(*) FROM MEGRP_GRUPO 
                                                         WHERE PEPER_ID = @PEPER_ID";
                        SqlCommand cmdVerificarGrupos = new SqlCommand(queryVerificarGrupos, oConexion);
                        cmdVerificarGrupos.Parameters.AddWithValue("@PEPER_ID", PEPER_ID);
                        int gruposRelacionados = Convert.ToInt32(cmdVerificarGrupos.ExecuteScalar());

                        if (gruposRelacionados > 0)
                        {
                            respuesta = false;
                            System.Diagnostics.Debug.WriteLine("No se puede eliminar personal con grupos asignados.");
                        }
                        else
                        {
                            SqlCommand cmdEliminar = new SqlCommand(queryEliminar, oConexion);
                            cmdEliminar.Parameters.AddWithValue("@PEPER_ID", PEPER_ID);

                            int rowsAffected = cmdEliminar.ExecuteNonQuery();
                            respuesta = rowsAffected > 0;
                        }
                    }
                }
                catch (Exception ex)
                {
                    respuesta = false;
                    System.Diagnostics.Debug.WriteLine("Error en EliminarPersonal: " + ex.Message);
                }
            }
            return respuesta;
        }

        public List<Personal> BuscarPersonal(string criterio, string valor)
        {
            List<Personal> rptListaPersonal = new List<Personal>();
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                try
                {
                    // Incluyendo PEPEPER_FEC_NA y PEPEPER_IMAG
                    string query = @"SELECT PEPER_ID, PEPER_NOMBRE, PEPER_APELLIDO, PEPER_EMAIL, 
                                             PEPER_CEDULA, PEPER_CELULAR, PEPER_TIPO, PEPEPER_FECH_INGR,
                                             PEESC_ID, PESEX_ID, XEUSU_ID,
                                             PEPEPER_FEC_NA, PEPEPER_IMAG
                                       FROM PEPER_PERSON 
                                       WHERE ";

                    switch (criterio.ToUpper())
                    {
                        case "CEDULA":
                            query += "PEPER_CEDULA LIKE @VALOR";
                            break;
                        case "NOMBRE":
                            query += "(PEPER_NOMBRE LIKE @VALOR OR PEPER_APELLIDO LIKE @VALOR)";
                            break;
                        case "EMAIL":
                            query += "PEPER_EMAIL LIKE @VALOR";
                            break;
                        case "TIPO":
                            query += "PEPER_TIPO = @VALOR";
                            break;
                        case "SEXO":
                            query += "PESEX_ID = @VALOR";
                            break;
                        case "ESTADO CIVIL":
                            query += "PEESC_ID = @VALOR";
                            break;
                        default:
                            query += "PEPER_CEDULA LIKE @VALOR";
                            break;
                    }

                    query += " ORDER BY PEPER_ID";

                    SqlCommand cmd = new SqlCommand(query, oConexion);

                    if (criterio.ToUpper() == "NOMBRE" || criterio.ToUpper() == "EMAIL" ||
                        criterio.ToUpper() == "CEDULA")
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
                        rptListaPersonal.Add(new Personal()
                        {
                            PEPER_ID = dr["PEPER_ID"]?.ToString(),
                            PEPER_NOMBRE = dr["PEPER_NOMBRE"]?.ToString(),
                            PEPER_APELLIDO = dr["PEPER_APELLIDO"]?.ToString(),
                            PEPER_EMAIL = dr["PEPER_EMAIL"]?.ToString(),
                            PEPER_CEDULA = dr["PEPER_CEDULA"]?.ToString(),
                            PEPER_CELULAR = dr["PEPER_CELULAR"]?.ToString(),
                            PEPER_TIPO = dr["PEPER_TIPO"]?.ToString(),
                            PEPEPER_FECH_INGR = dr["PEPEPER_FECH_INGR"] != DBNull.Value ? Convert.ToDateTime(dr["PEPEPER_FECH_INGR"]) : (DateTime?)null,
                            PEESC_ID = dr["PEESC_ID"]?.ToString(),
                            PESEX_ID = dr["PESEX_ID"]?.ToString(),
                            XEUSU_ID = dr["XEUSU_ID"]?.ToString(),

                            // Campos corregidos y mapeados:
                            PEPER_FECHA_NAC = dr["PEPEPER_FEC_NA"] != DBNull.Value ? Convert.ToDateTime(dr["PEPEPER_FEC_NA"]) : (DateTime?)null,
                            PEPER_FOTO = dr["PEPEPER_IMAG"] != DBNull.Value ? dr["PEPEPER_IMAG"].ToString() : null
                        });
                    }
                    dr.Close();
                }
                catch (Exception ex)
                {
                    rptListaPersonal = null;
                    System.Diagnostics.Debug.WriteLine("Error en BuscarPersonal: " + ex.Message);
                }
            }
            return rptListaPersonal;
        }

        public bool ValidarCedulaUnica(string PEPER_CEDULA, string PEPER_ID_EXCLUIR = null)
        {
            bool esUnica = true;
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                try
                {
                    string query;
                    SqlCommand cmd;

                    if (string.IsNullOrEmpty(PEPER_ID_EXCLUIR))
                    {
                        query = @"SELECT COUNT(*) FROM PEPER_PERSON 
                                   WHERE PEPER_CEDULA = @PEPER_CEDULA";
                        cmd = new SqlCommand(query, oConexion);
                        cmd.Parameters.AddWithValue("@PEPER_CEDULA", PEPER_CEDULA);
                    }
                    else
                    {
                        query = @"SELECT COUNT(*) FROM PEPER_PERSON 
                                   WHERE PEPER_CEDULA = @PEPER_CEDULA 
                                   AND PEPER_ID != @PEPER_ID";
                        cmd = new SqlCommand(query, oConexion);
                        cmd.Parameters.AddWithValue("@PEPER_CEDULA", PEPER_CEDULA);
                        cmd.Parameters.AddWithValue("@PEPER_ID", PEPER_ID_EXCLUIR);
                    }

                    oConexion.Open();
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    esUnica = count == 0;
                }
                catch (Exception ex)
                {
                    esUnica = false;
                    System.Diagnostics.Debug.WriteLine("Error en ValidarCedulaUnica: " + ex.Message);
                }
            }
            return esUnica;
        }

        public bool ValidarEmailUnico(string PEPER_EMAIL, string PEPER_ID_EXCLUIR = null)
        {
            bool esUnico = true;
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                try
                {
                    string query;
                    SqlCommand cmd;

                    if (string.IsNullOrEmpty(PEPER_ID_EXCLUIR))
                    {
                        query = @"SELECT COUNT(*) FROM PEPER_PERSON 
                                   WHERE PEPER_EMAIL = @PEPER_EMAIL";
                        cmd = new SqlCommand(query, oConexion);
                        cmd.Parameters.AddWithValue("@PEPER_EMAIL", PEPER_EMAIL);
                    }
                    else
                    {
                        query = @"SELECT COUNT(*) FROM PEPER_PERSON 
                                   WHERE PEPER_EMAIL = @PEPER_EMAIL 
                                   AND PEPER_ID != @PEPER_ID";
                        cmd = new SqlCommand(query, oConexion);
                        cmd.Parameters.AddWithValue("@PEPER_EMAIL", PEPER_EMAIL);
                        cmd.Parameters.AddWithValue("@PEPER_ID", PEPER_ID_EXCLUIR);
                    }

                    oConexion.Open();
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    esUnico = count == 0;
                }
                catch (Exception ex)
                {
                    esUnico = false;
                    System.Diagnostics.Debug.WriteLine("Error en ValidarEmailUnico: " + ex.Message);
                }
            }
            return esUnico;
        }

        public List<PESEX_SEXO> ObtenerSexos()
        {
            List<PESEX_SEXO> listaSexos = new List<PESEX_SEXO>();
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
                        listaSexos.Add(new PESEX_SEXO()
                        {
                            PESEX_ID = dr["PESEX_ID"]?.ToString(),
                            PESEX_DESCRI = dr["PESEX_DESCRI"]?.ToString()
                        });
                    }
                    dr.Close();
                }
                catch (Exception ex)
                {
                    listaSexos = null;
                    System.Diagnostics.Debug.WriteLine("Error en ObtenerSexos: " + ex.Message);
                }
            }
            return listaSexos;
        }

        public List<string> ObtenerTiposPersonal()
        {
            List<string> tipos = new List<string>();
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                string query = @"SELECT DISTINCT PEPER_TIPO 
                                   FROM PEPER_PERSON 
                                   ORDER BY PEPER_TIPO";

                SqlCommand cmd = new SqlCommand(query, oConexion);

                try
                {
                    oConexion.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        tipos.Add(dr["PEPER_TIPO"]?.ToString());
                    }
                    dr.Close();
                }
                catch (Exception ex)
                {
                    tipos = null;
                    System.Diagnostics.Debug.WriteLine("Error en ObtenerTiposPersonal: " + ex.Message);
                }
            }
            return tipos;
        }
    
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

        }
}